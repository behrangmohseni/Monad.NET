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

            result.Add(option.GetValue());
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

            result.Add(option.GetValue());
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
                return Result<IReadOnlyList<T>, TErr>.Err(result.GetError());

            list.Add(result.GetValue());
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
                return Result<IReadOnlyList<U>, TErr>.Err(result.GetError());

            list.Add(result.GetValue());
        }

        return Result<IReadOnlyList<U>, TErr>.Ok(list);
    }

    #endregion

    #region Validation Async Collection Extensions

    /// <summary>
    /// Async version of Sequence for Validations.
    /// Returns Valid with all values if all validations are Valid, otherwise Invalid with ALL accumulated errors.
    /// </summary>
    /// <typeparam name="T">The type of valid values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="validationTasks">The sequence of tasks that produce validations.</param>
    /// <returns>A task containing Valid with all values, or Invalid with all accumulated errors.</returns>
    public static async Task<Validation<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        this IEnumerable<Task<Validation<T, TErr>>> validationTasks)
    {
        ThrowHelper.ThrowIfNull(validationTasks);

        var values = CollectionHelper.CreateListWithCapacity<Task<Validation<T, TErr>>, T>(validationTasks);
        var errors = new List<TErr>();

        foreach (var task in validationTasks)
        {
            var validation = await task.ConfigureAwait(false);
            if (validation.IsValid)
            {
                values.Add(validation.GetValue());
            }
            else
            {
                errors.AddRange(validation.GetErrors());
            }
        }

        return errors.Count > 0
            ? Validation<IReadOnlyList<T>, TErr>.Invalid(errors)
            : Validation<IReadOnlyList<T>, TErr>.Valid(values);
    }

    /// <summary>
    /// Async version of Traverse for Validations.
    /// Returns Valid of all values if all mappings succeed, otherwise Invalid with ALL accumulated errors.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of valid values in the resulting validations.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to a Validation.</param>
    /// <returns>A task containing Valid with all mapped values, or Invalid with all accumulated errors.</returns>
    public static async Task<Validation<IReadOnlyList<U>, TErr>> TraverseAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Validation<U, TErr>>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        var values = CollectionHelper.CreateListWithCapacity<T, U>(source);
        var errors = new List<TErr>();

        foreach (var item in source)
        {
            var validation = await selector(item).ConfigureAwait(false);
            if (validation.IsValid)
            {
                values.Add(validation.GetValue());
            }
            else
            {
                errors.AddRange(validation.GetErrors());
            }
        }

        return errors.Count > 0
            ? Validation<IReadOnlyList<U>, TErr>.Invalid(errors)
            : Validation<IReadOnlyList<U>, TErr>.Valid(values);
    }

    #endregion

    #region Try Async Collection Extensions

    /// <summary>
    /// Async version of Sequence for Tries.
    /// Returns Success with all values if all tries are Success, otherwise returns the first Failure.
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <param name="tryTasks">The sequence of tasks that produce tries.</param>
    /// <returns>A task containing Success with all values, or the first Failure encountered.</returns>
    public static async Task<Try<IReadOnlyList<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Try<T>>> tryTasks)
    {
        ThrowHelper.ThrowIfNull(tryTasks);

        var values = CollectionHelper.CreateListWithCapacity<Task<Try<T>>, T>(tryTasks);

        foreach (var task in tryTasks)
        {
            var result = await task.ConfigureAwait(false);
            if (result.IsFailure)
                return Try<IReadOnlyList<T>>.Failure(result.GetException());

            values.Add(result.GetValue());
        }

        return Try<IReadOnlyList<T>>.Success(values);
    }

    /// <summary>
    /// Async version of Traverse for Tries.
    /// Returns Success of all values if all mappings succeed, otherwise returns the first failure.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting tries.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to a Try.</param>
    /// <returns>A task containing Success with all mapped values, or the first Failure encountered.</returns>
    public static async Task<Try<IReadOnlyList<U>>> TraverseAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Try<U>>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        var values = CollectionHelper.CreateListWithCapacity<T, U>(source);

        foreach (var item in source)
        {
            var result = await selector(item).ConfigureAwait(false);
            if (result.IsFailure)
                return Try<IReadOnlyList<U>>.Failure(result.GetException());

            values.Add(result.GetValue());
        }

        return Try<IReadOnlyList<U>>.Success(values);
    }

    #endregion

    #region RemoteData Async Collection Extensions

    /// <summary>
    /// Async version of Sequence for RemoteData.
    /// Returns Success with all values if all are Success, otherwise returns the first non-Success state.
    /// Priority order: Failure > Loading > NotAsked > Success
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="remoteDataTasks">The sequence of tasks that produce RemoteData.</param>
    /// <returns>A task containing Success with all values, or the first non-Success state.</returns>
    public static async Task<RemoteData<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        this IEnumerable<Task<RemoteData<T, TErr>>> remoteDataTasks)
    {
        ThrowHelper.ThrowIfNull(remoteDataTasks);

        var values = CollectionHelper.CreateListWithCapacity<Task<RemoteData<T, TErr>>, T>(remoteDataTasks);
        RemoteData<IReadOnlyList<T>, TErr>? firstNonSuccess = null;

        foreach (var task in remoteDataTasks)
        {
            var item = await task.ConfigureAwait(false);

            if (item.IsSuccess)
            {
                values.Add(item.GetValue());
            }
            else if (firstNonSuccess is null)
            {
                if (item.IsFailure)
                    firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Failure(item.GetError());
                else if (item.IsLoading)
                    firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Loading();
                else if (item.IsNotAsked)
                    firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.NotAsked();
            }
            else if (item.IsFailure && !firstNonSuccess.Value.IsFailure)
            {
                firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Failure(item.GetError());
            }
            else if (item.IsLoading && firstNonSuccess.Value.IsNotAsked)
            {
                firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Loading();
            }
        }

        return firstNonSuccess ?? RemoteData<IReadOnlyList<T>, TErr>.Success(values);
    }

    /// <summary>
    /// Async version of Traverse for RemoteData.
    /// Returns Success of all values if all mappings succeed, otherwise returns the first non-Success state.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting RemoteData.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">An async function that maps each element to a RemoteData.</param>
    /// <returns>A task containing Success with all mapped values, or the first non-Success state.</returns>
    public static async Task<RemoteData<IReadOnlyList<U>, TErr>> TraverseAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<RemoteData<U, TErr>>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        var values = CollectionHelper.CreateListWithCapacity<T, U>(source);
        RemoteData<IReadOnlyList<U>, TErr>? firstNonSuccess = null;

        foreach (var item in source)
        {
            var result = await selector(item).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                values.Add(result.GetValue());
            }
            else if (firstNonSuccess is null)
            {
                if (result.IsFailure)
                    firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Failure(result.GetError());
                else if (result.IsLoading)
                    firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Loading();
                else if (result.IsNotAsked)
                    firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.NotAsked();
            }
            else if (result.IsFailure && !firstNonSuccess.Value.IsFailure)
            {
                firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Failure(result.GetError());
            }
            else if (result.IsLoading && firstNonSuccess.Value.IsNotAsked)
            {
                firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Loading();
            }
        }

        return firstNonSuccess ?? RemoteData<IReadOnlyList<U>, TErr>.Success(values);
    }

    #endregion
}

