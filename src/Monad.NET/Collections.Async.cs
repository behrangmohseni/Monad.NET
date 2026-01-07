using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
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
        ThrowHelper.ThrowIfNull(optionTasks);

        // Pre-allocate with capacity if we can determine the count
        var result = CollectionHelper.CreateListWithCapacity<Task<Option<T>>, T>(optionTasks);

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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        // Pre-allocate with capacity if we can determine the count
        var result = CollectionHelper.CreateListWithCapacity<T, U>(source);

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
        ThrowHelper.ThrowIfNull(resultTasks);

        // Pre-allocate with capacity if we can determine the count
        var list = CollectionHelper.CreateListWithCapacity<Task<Result<T, TErr>>, T>(resultTasks);

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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        // Pre-allocate with capacity if we can determine the count
        var list = CollectionHelper.CreateListWithCapacity<T, U>(source);

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
}

