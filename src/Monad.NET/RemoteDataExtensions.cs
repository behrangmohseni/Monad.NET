using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Extension methods for RemoteData&lt;T, E&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RemoteDataExtensions
{
    /// <summary>
    /// Executes an action if the data is in Success state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> Tap<T, TError>(
        this RemoteData<T, TError> remoteData,
        Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsOk)
            action(remoteData.GetValue());

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in Error state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> TapError<T, TError>(
        this RemoteData<T, TError> remoteData,
        Action<TError> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsError)
            action(remoteData.GetError());

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in NotAsked state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> TapNotAsked<T, TError>(
        this RemoteData<T, TError> remoteData,
        Action action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsNotAsked)
            action();

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in Loading state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> TapLoading<T, TError>(
        this RemoteData<T, TError> remoteData,
        Action action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsLoading)
            action();

        return remoteData;
    }

    /// <summary>
    /// Converts a Result to RemoteData in Success or Failure state.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> ToRemoteData<T, TError>(this Result<T, TError> result)
    {
        return result.Match(
            okFunc: static data => RemoteData<T, TError>.Ok(data),
            errFunc: static err => RemoteData<T, TError>.Error(err)
        );
    }

    /// <summary>
    /// Wraps an async operation in RemoteData, starting with Loading and ending with Success/Failure.
    /// </summary>
    /// <param name="taskFunc">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Success with the result, or Failure with the exception.</returns>
    public static async Task<RemoteData<T, Exception>> FromTaskAsync<T>(
        Func<Task<T>> taskFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(taskFunc);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = await taskFunc().ConfigureAwait(false);
            return RemoteData<T, Exception>.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return RemoteData<T, Exception>.Error(ex);
        }
    }

    /// <summary>
    /// Maps RemoteData with an async function.
    /// </summary>
    /// <param name="remoteData">The remote data to map.</param>
    /// <param name="mapper">An async function to apply to the value if Success.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Success with the mapped value, or the original state preserved.</returns>
    public static async Task<RemoteData<U, TError>> MapAsync<T, TError, U>(
        this RemoteData<T, TError> remoteData,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (!remoteData.IsOk)
            return remoteData.Map(static _ => default(U)!); // Preserves state

        var result = await mapper(remoteData.GetValue()).ConfigureAwait(false);
        return RemoteData<U, TError>.Ok(result);
    }

    /// <summary>
    /// Returns true if the data is loaded (either Success or Failure).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLoaded<T, TError>(this RemoteData<T, TError> remoteData)
    {
        return remoteData.IsOk || remoteData.IsError;
    }

    /// <summary>
    /// Returns true if the data is not loaded (either NotAsked or Loading).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotLoaded<T, TError>(this RemoteData<T, TError> remoteData)
    {
        return remoteData.IsNotAsked || remoteData.IsLoading;
    }

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a RemoteData in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<RemoteData<T, TError>> AsValueTask<T, TError>(this RemoteData<T, TError> remoteData)
        => new(remoteData);

    /// <summary>
    /// Maps the value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<RemoteData<U, TError>> MapAsync<T, TError, U>(
        this ValueTask<RemoteData<T, TError>> remoteDataTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (remoteDataTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(remoteDataTask.Result.Map(mapper));
        }
        return Core(remoteDataTask, mapper, cancellationToken);

        static async ValueTask<RemoteData<U, TError>> Core(ValueTask<RemoteData<T, TError>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var rd = await t.ConfigureAwait(false);
            return rd.Map(m);
        }
    }

    /// <summary>
    /// Maps the value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<RemoteData<U, TError>> MapAsync<T, TError, U>(
        this ValueTask<RemoteData<T, TError>> remoteDataTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var remoteData = await remoteDataTask.ConfigureAwait(false);
        if (!remoteData.IsOk)
            return remoteData.Map(static _ => default(U)!);

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(remoteData.GetValue(), cancellationToken).ConfigureAwait(false);
        return RemoteData<U, TError>.Ok(result);
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, TError, U>(
        this ValueTask<RemoteData<T, TError>> remoteDataTask,
        Func<U> notAskedFunc,
        Func<U> loadingFunc,
        Func<T, U> successFunc,
        Func<TError, U> failureFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(notAskedFunc);
        ThrowHelper.ThrowIfNull(loadingFunc);
        ThrowHelper.ThrowIfNull(successFunc);
        ThrowHelper.ThrowIfNull(failureFunc);

        if (remoteDataTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(remoteDataTask.Result.Match(notAskedFunc, loadingFunc, successFunc, failureFunc));
        }
        return Core(remoteDataTask, notAskedFunc, loadingFunc, successFunc, failureFunc, cancellationToken);

        static async ValueTask<U> Core(
            ValueTask<RemoteData<T, TError>> t,
            Func<U> na, Func<U> l, Func<T, U> s, Func<TError, U> f,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var rd = await t.ConfigureAwait(false);
            return rd.Match(na, l, s, f);
        }
    }

    #endregion
}
