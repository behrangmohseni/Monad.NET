using System.ComponentModel;
using System.Runtime.CompilerServices;
using Monad.NET.Internal;

namespace Monad.NET;

/// <summary>
/// Collection extensions for working with sequences of Option&lt;T&gt;, Result&lt;T, E&gt;, and other monadic types.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class MonadCollectionExtensions
{
    /// <summary>
    /// Private helper class for parallel collection operations.
    /// </summary>
    internal static class ParallelHelper
    {
        /// <summary>
        /// Validates the maxDegreeOfParallelism parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateMaxDegreeOfParallelism(int maxDegreeOfParallelism, string paramName)
        {
            if (maxDegreeOfParallelism < -1 || maxDegreeOfParallelism == 0)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    maxDegreeOfParallelism,
                    "maxDegreeOfParallelism must be -1 (unlimited) or a positive integer.");
        }

        /// <summary>
        /// Executes tasks in parallel with optional throttling.
        /// Optimized to avoid intermediate allocations where possible.
        /// </summary>
        public static async Task<TResult[]> RunParallelAsync<TSource, TResult>(
            IList<TSource> source,
            Func<TSource, Task<TResult>> selector,
            int maxDegreeOfParallelism,
            CancellationToken cancellationToken)
        {
            var count = source.Count;

            if (maxDegreeOfParallelism == -1 || maxDegreeOfParallelism >= count)
            {
                // Run all in parallel - pre-allocate task array to avoid LINQ allocation
                var tasks = new Task<TResult>[count];
                for (var i = 0; i < count; i++)
                {
                    tasks[i] = selector(source[i]);
                }
                return await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            // Throttled parallel execution
            var results = new TResult[count];
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            // Pre-allocate task array instead of using LINQ Select
            var indexedTasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                var index = i; // Capture for closure
                var item = source[i];
                indexedTasks[i] = ExecuteWithSemaphoreAsync(semaphore, item, index, selector, results, cancellationToken);
            }

            await Task.WhenAll(indexedTasks).ConfigureAwait(false);
            return results;
        }

        /// <summary>
        /// Helper to execute a single item with semaphore throttling.
        /// </summary>
        private static async Task ExecuteWithSemaphoreAsync<TSource, TResult>(
            SemaphoreSlim semaphore,
            TSource item,
            int index,
            Func<TSource, Task<TResult>> selector,
            TResult[] results,
            CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                results[index] = await selector(item).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Awaits pre-started tasks in parallel with optional throttling.
        /// Optimized to avoid intermediate LINQ allocations.
        /// </summary>
        public static async Task<TResult[]> AwaitParallelAsync<TResult>(
            IList<Task<TResult>> tasks,
            int maxDegreeOfParallelism,
            CancellationToken cancellationToken)
        {
            var count = tasks.Count;

            if (maxDegreeOfParallelism == -1 || maxDegreeOfParallelism >= count)
            {
                // Run all in parallel - copy to array for Task.WhenAll if needed
                if (tasks is Task<TResult>[] taskArray)
                {
                    return await Task.WhenAll(taskArray).ConfigureAwait(false);
                }

                // Pre-allocate task array to avoid LINQ allocation
                var tasksArray = new Task<TResult>[count];
                for (var i = 0; i < count; i++)
                {
                    tasksArray[i] = tasks[i];
                }
                return await Task.WhenAll(tasksArray).ConfigureAwait(false);
            }

            // Throttled parallel execution
            var results = new TResult[count];
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            // Pre-allocate task array instead of using LINQ Select
            var indexedTasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                var index = i;
                var task = tasks[i];
                indexedTasks[i] = AwaitWithSemaphoreAsync(semaphore, task, index, results, cancellationToken);
            }

            await Task.WhenAll(indexedTasks).ConfigureAwait(false);
            return results;
        }

        /// <summary>
        /// Helper to await a task with semaphore throttling.
        /// </summary>
        private static async Task AwaitWithSemaphoreAsync<TResult>(
            SemaphoreSlim semaphore,
            Task<TResult> task,
            int index,
            TResult[] results,
            CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                results[index] = await task.ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Validates traverse parameters and prepares the source list.
        /// Returns null if the source is empty, otherwise returns the prepared list.
        /// Avoids unnecessary allocations when the source is already a list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TSource>? ValidateAndPrepareSource<TSource, TSelector>(
            IEnumerable<TSource> source,
            TSelector selector,
            int maxDegreeOfParallelism,
            CancellationToken cancellationToken)
            where TSelector : Delegate
        {
            ThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNull(selector);
            ValidateMaxDegreeOfParallelism(maxDegreeOfParallelism, nameof(maxDegreeOfParallelism));
            cancellationToken.ThrowIfCancellationRequested();

            // Avoid ToList() allocation if source is already a list or array
            var sourceList = CollectionHelper.MaterializeToList(source);
            return sourceList.Count == 0 ? null : sourceList;
        }
    }

    #region Option Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Option&lt;T&gt;&gt; to Option&lt;IReadOnlyList&lt;T&gt;&gt;.
    /// Returns Some if all options are Some, otherwise None.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The sequence of options to transpose.</param>
    /// <returns>Some containing all values if all options are Some; otherwise None.</returns>
    public static Option<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        ThrowHelper.ThrowIfNull(options);

        // Pre-allocate with capacity if we can determine the count
        var result = CollectionHelper.CreateListWithCapacity<Option<T>, T>(options);

        foreach (var option in options)
        {
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            result.Add(option.GetValue());
        }

        return Option<IReadOnlyList<T>>.Some(result);
    }

    /// <summary>
    /// Maps each element to an Option and sequences the results.
    /// Returns Some of all values if all mappings succeed, otherwise None.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">A function that maps each element to an Option.</param>
    /// <returns>Some containing all mapped values if all mappings return Some; otherwise None.</returns>
    public static Option<IReadOnlyList<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, Option<U>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        // Pre-allocate with capacity if we can determine the count
        var result = CollectionHelper.CreateListWithCapacity<T, U>(source);

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsNone)
                return Option<IReadOnlyList<U>>.None();

            result.Add(option.GetValue());
        }

        return Option<IReadOnlyList<U>>.Some(result);
    }

    /// <summary>
    /// Filters and unwraps Some values from a sequence of Options.
    /// Similar to Rust's filter_map.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The sequence of options to filter.</param>
    /// <returns>An enumerable containing only the unwrapped Some values.</returns>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> options)
    {
        ThrowHelper.ThrowIfNull(options);

        foreach (var option in options)
        {
            if (option.IsSome)
                yield return option.GetValue();
        }
    }

    /// <summary>
    /// Maps and filters in one operation, keeping only Some results.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source sequence to process.</param>
    /// <param name="selector">A function that maps each element to an Option.</param>
    /// <returns>An enumerable containing only the unwrapped Some values from the mapping.</returns>
    public static IEnumerable<U> Choose<T, U>(
        this IEnumerable<T> source,
        Func<T, Option<U>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsSome)
                yield return option.GetValue();
        }
    }

    /// <summary>
    /// Returns the first Some value in the sequence, or None if all are None.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The sequence of options to search.</param>
    /// <returns>The first Some option found, or None if all options are None.</returns>
    public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> options)
    {
        ThrowHelper.ThrowIfNull(options);

        foreach (var option in options)
        {
            if (option.IsSome)
                return option;
        }

        return Option<T>.None();
    }

    #endregion
}
