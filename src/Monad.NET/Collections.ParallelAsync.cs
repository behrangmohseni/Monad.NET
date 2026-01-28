using System.Runtime.CompilerServices;
using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
    #region Parallel Async Collection Extensions

    /// <summary>
    /// Parallel async version of Sequence for Options.
    /// Executes all tasks in parallel and returns Some if all options are Some, otherwise None.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="optionTasks">The sequence of tasks that produce options.</param>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. -1 for unlimited.</param>
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
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
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTasks);
        ParallelHelper.ValidateMaxDegreeOfParallelism(maxDegreeOfParallelism, nameof(maxDegreeOfParallelism));
        cancellationToken.ThrowIfCancellationRequested();

        // Avoid ToList() allocation if source is already a list
        var taskList = CollectionHelper.MaterializeToList(optionTasks);
        if (taskList.Count == 0)
            return Option<IReadOnlyList<T>>.Some(Array.Empty<T>());

        var results = await ParallelHelper.AwaitParallelAsync(taskList, maxDegreeOfParallelism, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return CollectOptionResults(results);
    }

    /// <summary>
    /// Collects Option results into a single Option containing all values.
    /// </summary>
    private static Option<IReadOnlyList<T>> CollectOptionResults<T>(Option<T>[] results)
    {
        var values = new List<T>(results.Length);
        foreach (var option in results)
        {
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();
            values.Add(option.GetValue());
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
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
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
    public static async Task<Option<IReadOnlyList<U>>> TraverseParallelAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> selector,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var sourceList = ParallelHelper.ValidateAndPrepareSource(source, selector, maxDegreeOfParallelism, cancellationToken);
        if (sourceList is null)
            return Option<IReadOnlyList<U>>.Some(Array.Empty<U>());

        var results = await ParallelHelper.RunParallelAsync(sourceList, selector, maxDegreeOfParallelism, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return CollectOptionResults(results);
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
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
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
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        ParallelHelper.ValidateMaxDegreeOfParallelism(maxDegreeOfParallelism, nameof(maxDegreeOfParallelism));
        cancellationToken.ThrowIfCancellationRequested();

        // Avoid ToList() allocation if source is already a list
        var taskList = CollectionHelper.MaterializeToList(resultTasks);
        if (taskList.Count == 0)
            return Result<IReadOnlyList<T>, TErr>.Ok(Array.Empty<T>());

        var results = await ParallelHelper.AwaitParallelAsync(taskList, maxDegreeOfParallelism, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return CollectResultResults(results);
    }

    /// <summary>
    /// Collects Result results into a single Result containing all values.
    /// </summary>
    private static Result<IReadOnlyList<T>, TErr> CollectResultResults<T, TErr>(Result<T, TErr>[] results)
    {
        var values = new List<T>(results.Length);
        foreach (var result in results)
        {
            if (result.IsError)
                return Result<IReadOnlyList<T>, TErr>.Err(result.GetError());
            values.Add(result.GetValue());
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
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
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
    public static async Task<Result<IReadOnlyList<U>, TErr>> TraverseParallelAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, TErr>>> selector,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var sourceList = ParallelHelper.ValidateAndPrepareSource(source, selector, maxDegreeOfParallelism, cancellationToken);
        if (sourceList is null)
            return Result<IReadOnlyList<U>, TErr>.Ok(Array.Empty<U>());

        var results = await ParallelHelper.RunParallelAsync(sourceList, selector, maxDegreeOfParallelism, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return CollectResultResults(results);
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
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
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
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var sourceList = ParallelHelper.ValidateAndPrepareSource(source, selector, maxDegreeOfParallelism, cancellationToken);
        if (sourceList is null)
            return Array.Empty<U>();

        var results = await ParallelHelper.RunParallelAsync(sourceList, selector, maxDegreeOfParallelism, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return results.Where(o => o.IsSome).Select(o => o.GetValue()).ToList();
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
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
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
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var sourceList = ParallelHelper.ValidateAndPrepareSource(source, selector, maxDegreeOfParallelism, cancellationToken);
        if (sourceList is null)
            return (Array.Empty<U>(), Array.Empty<TErr>());

        var results = await ParallelHelper.RunParallelAsync(sourceList, selector, maxDegreeOfParallelism, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        // Pre-allocate with reasonable capacity
        var count = results.Length;
        var oks = new List<U>(count);
        var errors = new List<TErr>((count / 4) + 1); // Expect fewer errors

        foreach (var result in results)
        {
            if (result.IsOk)
                oks.Add(result.GetValue());
            else
                errors.Add(result.GetError());
        }

        return (oks, errors);
    }

    #endregion
}

