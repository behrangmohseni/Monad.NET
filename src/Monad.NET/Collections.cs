using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Collection extensions for working with sequences of Option&lt;T&gt;, Result&lt;T, E&gt;, and Either&lt;L, R&gt;.
/// </summary>
public static class MonadCollectionExtensions
{
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
        ArgumentNullException.ThrowIfNull(options);

        var result = new List<T>();

        foreach (var option in options)
        {
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            result.Add(option.Unwrap());
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
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var result = new List<U>();

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsNone)
                return Option<IReadOnlyList<U>>.None();

            result.Add(option.Unwrap());
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
        ArgumentNullException.ThrowIfNull(options);

        foreach (var option in options)
        {
            if (option.IsSome)
                yield return option.Unwrap();
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
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsSome)
                yield return option.Unwrap();
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
        ArgumentNullException.ThrowIfNull(options);

        foreach (var option in options)
        {
            if (option.IsSome)
                return option;
        }

        return Option<T>.None();
    }

    #endregion

    #region Result Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Result&lt;T, E&gt;&gt; to Result&lt;IReadOnlyList&lt;T&gt;, E&gt;.
    /// Returns Ok with all values if all results are Ok, otherwise returns the first Err.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to transpose.</param>
    /// <returns>Ok containing all values if all results are Ok; otherwise the first Err encountered.</returns>
    public static Result<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var list = new List<T>();

        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Maps each element to a Result and sequences the results.
    /// Returns Ok of all values if all mappings succeed, otherwise returns the first error.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">A function that maps each element to a Result.</param>
    /// <returns>Ok containing all mapped values if all mappings return Ok; otherwise the first Err encountered.</returns>
    public static Result<IReadOnlyList<U>, TErr> Traverse<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Result<U, TErr>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var list = new List<U>();

        foreach (var item in source)
        {
            var result = selector(item);
            if (result.IsErr)
                return Result<IReadOnlyList<U>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<U>, TErr>.Ok(list);
    }

    /// <summary>
    /// Collects all Ok values from a sequence of Results.
    /// Discards all Err values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Ok values.</returns>
    public static IEnumerable<T> CollectOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsOk)
                yield return result.Unwrap();
        }
    }

    /// <summary>
    /// Collects all Err values from a sequence of Results.
    /// Discards all Ok values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Err values.</returns>
    public static IEnumerable<TErr> CollectErr<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsErr)
                yield return result.UnwrapErr();
        }
    }

    /// <summary>
    /// Partitions a sequence of Results into Ok and Err values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to partition.</param>
    /// <returns>A tuple containing a list of Ok values and a list of Err values.</returns>
    public static (IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errors) Partition<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var oks = new List<T>();
        var errors = new List<TErr>();

        foreach (var result in results)
        {
            if (result.IsOk)
                oks.Add(result.Unwrap());
            else
                errors.Add(result.UnwrapErr());
        }

        return (oks, errors);
    }

    /// <summary>
    /// Returns the first Ok value in the sequence, or the last Err if all are Err.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to search.</param>
    /// <returns>The first Ok result found, or the last Err result if no Ok is found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sequence is empty.</exception>
    public static Result<T, TErr> FirstOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        Result<T, TErr>? lastErr = null;

        foreach (var result in results)
        {
            if (result.IsOk)
                return result;

            lastErr = result;
        }

        if (lastErr is null)
            ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");

        return lastErr.Value;
    }

    /// <summary>
    /// Returns the first Ok value in the sequence, or the last Err if all are Err.
    /// Returns an Err with <paramref name="defaultError"/> if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to search.</param>
    /// <param name="defaultError">The error to return if the sequence is empty.</param>
    /// <returns>The first Ok result found, the last Err result, or an Err with the default error if empty.</returns>
    public static Result<T, TErr> FirstOkOrDefault<T, TErr>(
        this IEnumerable<Result<T, TErr>> results,
        TErr defaultError)
    {
        ArgumentNullException.ThrowIfNull(results);

        Result<T, TErr>? lastErr = null;

        foreach (var result in results)
        {
            if (result.IsOk)
                return result;

            lastErr = result;
        }

        return lastErr ?? Result<T, TErr>.Err(defaultError);
    }

    #endregion

    #region Either Collections

    /// <summary>
    /// Collects all Right values from a sequence of Eithers.
    /// </summary>
    /// <typeparam name="TLeft">The type of Left values in the eithers.</typeparam>
    /// <typeparam name="TRight">The type of Right values in the eithers.</typeparam>
    /// <param name="eithers">The sequence of eithers to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Right values.</returns>
    public static IEnumerable<TRight> CollectRights<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ArgumentNullException.ThrowIfNull(eithers);

        foreach (var either in eithers)
        {
            if (either.IsRight)
                yield return either.UnwrapRight();
        }
    }

    /// <summary>
    /// Collects all Left values from a sequence of Eithers.
    /// </summary>
    /// <typeparam name="TLeft">The type of Left values in the eithers.</typeparam>
    /// <typeparam name="TRight">The type of Right values in the eithers.</typeparam>
    /// <param name="eithers">The sequence of eithers to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Left values.</returns>
    public static IEnumerable<TLeft> CollectLefts<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ArgumentNullException.ThrowIfNull(eithers);

        foreach (var either in eithers)
        {
            if (either.IsLeft)
                yield return either.UnwrapLeft();
        }
    }

    /// <summary>
    /// Partitions a sequence of Eithers into Left and Right values.
    /// </summary>
    /// <typeparam name="TLeft">The type of Left values in the eithers.</typeparam>
    /// <typeparam name="TRight">The type of Right values in the eithers.</typeparam>
    /// <param name="eithers">The sequence of eithers to partition.</param>
    /// <returns>A tuple containing a list of Left values and a list of Right values.</returns>
    public static (IReadOnlyList<TLeft> Lefts, IReadOnlyList<TRight> Rights) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ArgumentNullException.ThrowIfNull(eithers);

        var lefts = new List<TLeft>();
        var rights = new List<TRight>();

        foreach (var either in eithers)
        {
            if (either.IsLeft)
                lefts.Add(either.UnwrapLeft());
            else
                rights.Add(either.UnwrapRight());
        }

        return (lefts, rights);
    }

    #endregion

    #region Async Collection Extensions

    /// <summary>
    /// Async version of Sequence for Options.
    /// Returns Some if all options are Some, otherwise None.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="optionTasks">The sequence of tasks that produce options.</param>
    /// <returns>A task containing Some with all values if all options are Some; otherwise None.</returns>
    public static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks)
    {
        ArgumentNullException.ThrowIfNull(optionTasks);

        var result = new List<T>();

        foreach (var task in optionTasks)
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<T>>.Some(result);
    }

    /// <summary>
    /// Async version of Traverse for Options.
    /// Returns Some of all values if all mappings succeed, otherwise None.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to an Option.</param>
    /// <returns>A task containing Some with all mapped values if all mappings return Some; otherwise None.</returns>
    public static async Task<Option<IReadOnlyList<U>>> TraverseAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var result = new List<U>();

        foreach (var item in source)
        {
            var option = await selector(item).ConfigureAwait(false);
            if (option.IsNone)
                return Option<IReadOnlyList<U>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<U>>.Some(result);
    }

    /// <summary>
    /// Async version of Sequence for Results.
    /// Returns Ok with all values if all results are Ok, otherwise returns the first Err.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="resultTasks">The sequence of tasks that produce results.</param>
    /// <returns>A task containing Ok with all values if all results are Ok; otherwise the first Err encountered.</returns>
    public static async Task<Result<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        this IEnumerable<Task<Result<T, TErr>>> resultTasks)
    {
        ArgumentNullException.ThrowIfNull(resultTasks);

        var list = new List<T>();

        foreach (var task in resultTasks)
        {
            var result = await task.ConfigureAwait(false);
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Async version of Traverse for Results.
    /// Returns Ok of all values if all mappings succeed, otherwise returns the first error.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to a Result.</param>
    /// <returns>A task containing Ok with all mapped values if all mappings return Ok; otherwise the first Err encountered.</returns>
    public static async Task<Result<IReadOnlyList<U>, TErr>> TraverseAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, TErr>>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var list = new List<U>();

        foreach (var item in source)
        {
            var result = await selector(item).ConfigureAwait(false);
            if (result.IsErr)
                return Result<IReadOnlyList<U>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<U>, TErr>.Ok(list);
    }

    #endregion

    #region Parallel Async Collection Extensions

    /// <summary>
    /// Parallel async version of Sequence for Options.
    /// Executes all tasks in parallel and returns Some if all options are Some, otherwise None.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="optionTasks">The sequence of tasks that produce options.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <returns>A task containing Some with all values if all options are Some; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var tasks = userIds.Select(id => GetUserAsync(id));
    /// var result = await tasks.SequenceParallelAsync(maxDegreeOfParallelism: 4);
    /// // Some([users]) if all found, None if any not found
    /// </code>
    /// </example>
    public static async Task<Option<IReadOnlyList<T>>> SequenceParallelAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(optionTasks);

        var taskList = optionTasks.ToList();
        if (taskList.Count == 0)
            return Option<IReadOnlyList<T>>.Some(Array.Empty<T>());

        Option<T>[] results;

        if (maxDegreeOfParallelism == -1 || maxDegreeOfParallelism >= taskList.Count)
        {
            // Run all in parallel
            results = await Task.WhenAll(taskList).ConfigureAwait(false);
        }
        else
        {
            // Throttled parallel execution
            results = new Option<T>[taskList.Count];
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var indexedTasks = taskList.Select(async (task, index) =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    results[index] = await task.ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(indexedTasks).ConfigureAwait(false);
        }

        var values = new List<T>(results.Length);
        foreach (var option in results)
        {
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();
            values.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<T>>.Some(values);
    }

    /// <summary>
    /// Parallel async version of Traverse for Options.
    /// Maps each element in parallel and returns Some of all values if all mappings succeed, otherwise None.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to an Option.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <returns>A task containing Some with all mapped values if all mappings return Some; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var result = await userIds.TraverseParallelAsync(
    ///     id => FindUserAsync(id),
    ///     maxDegreeOfParallelism: 4
    /// );
    /// // Some([users]) if all found, None if any not found
    /// </code>
    /// </example>
    public static Task<Option<IReadOnlyList<U>>> TraverseParallelAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> selector,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Select(selector).SequenceParallelAsync(maxDegreeOfParallelism);
    }

    /// <summary>
    /// Parallel async version of Sequence for Results.
    /// Executes all tasks in parallel and returns Ok with all values if all results are Ok,
    /// otherwise returns the first Err encountered.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="resultTasks">The sequence of tasks that produce results.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <returns>A task containing Ok with all values if all results are Ok; otherwise the first Err encountered.</returns>
    /// <example>
    /// <code>
    /// var tasks = orderIds.Select(id => ProcessOrderAsync(id));
    /// var result = await tasks.SequenceParallelAsync(maxDegreeOfParallelism: 8);
    /// // Ok([results]) if all succeed, Err(firstError) if any fail
    /// </code>
    /// </example>
    public static async Task<Result<IReadOnlyList<T>, TErr>> SequenceParallelAsync<T, TErr>(
        this IEnumerable<Task<Result<T, TErr>>> resultTasks,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(resultTasks);

        var taskList = resultTasks.ToList();
        if (taskList.Count == 0)
            return Result<IReadOnlyList<T>, TErr>.Ok(Array.Empty<T>());

        Result<T, TErr>[] results;

        if (maxDegreeOfParallelism == -1 || maxDegreeOfParallelism >= taskList.Count)
        {
            // Run all in parallel
            results = await Task.WhenAll(taskList).ConfigureAwait(false);
        }
        else
        {
            // Throttled parallel execution
            results = new Result<T, TErr>[taskList.Count];
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var indexedTasks = taskList.Select(async (task, index) =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    results[index] = await task.ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(indexedTasks).ConfigureAwait(false);
        }

        var values = new List<T>(results.Length);
        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.UnwrapErr());
            values.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<T>, TErr>.Ok(values);
    }

    /// <summary>
    /// Parallel async version of Traverse for Results.
    /// Maps each element in parallel and returns Ok of all values if all mappings succeed,
    /// otherwise returns the first error.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to a Result.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <returns>A task containing Ok with all mapped values if all mappings return Ok; otherwise the first Err encountered.</returns>
    /// <example>
    /// <code>
    /// var result = await orderIds.TraverseParallelAsync(
    ///     id => ProcessOrderAsync(id),
    ///     maxDegreeOfParallelism: 8
    /// );
    /// // Ok([results]) if all succeed, Err(firstError) if any fail
    /// </code>
    /// </example>
    public static Task<Result<IReadOnlyList<U>, TErr>> TraverseParallelAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, TErr>>> selector,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Select(selector).SequenceParallelAsync(maxDegreeOfParallelism);
    }

    /// <summary>
    /// Parallel version of Choose for Options.
    /// Maps each element in parallel using an async selector and collects only the Some values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source sequence to process.</param>
    /// <param name="selector">An async function that maps each element to an Option.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <returns>A task containing a list of unwrapped Some values.</returns>
    /// <example>
    /// <code>
    /// var validUsers = await userIds.ChooseParallelAsync(
    ///     id => TryGetUserAsync(id),
    ///     maxDegreeOfParallelism: 4
    /// );
    /// // Returns only the users that were found
    /// </code>
    /// </example>
    public static async Task<IReadOnlyList<U>> ChooseParallelAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> selector,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var sourceList = source.ToList();
        if (sourceList.Count == 0)
            return Array.Empty<U>();

        Option<U>[] results;

        if (maxDegreeOfParallelism == -1 || maxDegreeOfParallelism >= sourceList.Count)
        {
            // Run all in parallel
            results = await Task.WhenAll(sourceList.Select(selector)).ConfigureAwait(false);
        }
        else
        {
            // Throttled parallel execution
            results = new Option<U>[sourceList.Count];
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var indexedTasks = sourceList.Select(async (item, index) =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    results[index] = await selector(item).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(indexedTasks).ConfigureAwait(false);
        }

        return results.Where(o => o.IsSome).Select(o => o.Unwrap()).ToList();
    }

    /// <summary>
    /// Parallel version of Partition for Results.
    /// Processes each element in parallel and partitions into Ok and Err values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="source">The source sequence to process.</param>
    /// <param name="selector">An async function that maps each element to a Result.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <returns>A task containing a tuple of Ok values and Err values.</returns>
    /// <example>
    /// <code>
    /// var (successes, failures) = await orders.PartitionParallelAsync(
    ///     order => ProcessOrderAsync(order),
    ///     maxDegreeOfParallelism: 8
    /// );
    /// </code>
    /// </example>
    public static async Task<(IReadOnlyList<U> Oks, IReadOnlyList<TErr> Errors)> PartitionParallelAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, TErr>>> selector,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var sourceList = source.ToList();
        if (sourceList.Count == 0)
            return (Array.Empty<U>(), Array.Empty<TErr>());

        Result<U, TErr>[] results;

        if (maxDegreeOfParallelism == -1 || maxDegreeOfParallelism >= sourceList.Count)
        {
            // Run all in parallel
            results = await Task.WhenAll(sourceList.Select(selector)).ConfigureAwait(false);
        }
        else
        {
            // Throttled parallel execution
            results = new Result<U, TErr>[sourceList.Count];
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var indexedTasks = sourceList.Select(async (item, index) =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    results[index] = await selector(item).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(indexedTasks).ConfigureAwait(false);
        }

        var oks = new List<U>();
        var errors = new List<TErr>();

        foreach (var result in results)
        {
            if (result.IsOk)
                oks.Add(result.Unwrap());
            else
                errors.Add(result.UnwrapErr());
        }

        return (oks, errors);
    }

    #endregion

    #region General Enumerable Extensions

    /// <summary>
    /// Executes an action for each element in the sequence and returns the original sequence.
    /// Useful for side effects in a functional pipeline without breaking the chain.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <returns>The original sequence, allowing method chaining.</returns>
    /// <remarks>
    /// Unlike ForEach, this method returns the sequence for continued chaining.
    /// The action is executed lazily when the sequence is enumerated.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = items
    ///     .Where(x => x.IsValid)
    ///     .Do(x => Console.WriteLine($"Processing: {x}"))
    ///     .Select(x => x.Transform())
    ///     .ToList();
    /// </code>
    /// </example>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Executes an action for each element in the sequence with the element's index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element and its index.</param>
    /// <returns>The original sequence, allowing method chaining.</returns>
    /// <example>
    /// <code>
    /// var results = items
    ///     .Do((x, i) => Console.WriteLine($"[{i}] {x}"))
    ///     .ToList();
    /// </code>
    /// </example>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            action(item, index++);
            yield return item;
        }
    }

    /// <summary>
    /// Executes an action for each element in the sequence.
    /// This is an eager operation that immediately iterates the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <remarks>
    /// Unlike Do, this method does not return the sequence.
    /// It immediately executes the action for all elements.
    /// </remarks>
    /// <example>
    /// <code>
    /// items.ForEach(x => Console.WriteLine(x));
    /// </code>
    /// </example>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an action for each element in the sequence with the element's index.
    /// This is an eager operation that immediately iterates the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element and its index.</param>
    /// <example>
    /// <code>
    /// items.ForEach((x, i) => Console.WriteLine($"[{i}] {x}"));
    /// </code>
    /// </example>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Executes an async action for each element in the sequence sequentially.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The async action to execute for each element.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// await items.ForEachAsync(async x => await ProcessAsync(x));
    /// </code>
    /// </example>
    public static async Task ForEachAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes an async action for each element in the sequence with the element's index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The async action to execute for each element and its index.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// await items.ForEachAsync(async (x, i) => await ProcessAsync(x, i));
    /// </code>
    /// </example>
    public static async Task ForEachAsync<T>(
        this IEnumerable<T> source,
        Func<T, int, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item, index++).ConfigureAwait(false);
        }
    }

    #endregion
}
