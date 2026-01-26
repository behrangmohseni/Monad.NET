using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Async extensions for Result&lt;T, E&gt; to work seamlessly with Task-based asynchronous code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ResultAsyncExtensions
{
    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        var value = await mapper(result.GetValue()).ConfigureAwait(false);
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> MapErrorAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<F>> mapper)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        var error = await mapper(result.GetError()).ConfigureAwait(false);
        return Result<T, F>.Err(error);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> MapErrorAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, F> mapper)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<Result<U, TErr>>> binder)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(binder);

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        return await binder(result.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains a synchronous operation on a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Result<U, TErr>> binder)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(binder);

        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(binder);
    }

    /// <summary>
    /// Recovers from an error using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<Result<T, F>>> op)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(op);

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        return await op(result.GetError()).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Ok value or computes a default asynchronously.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrElseAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<T>> op)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(op);

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return result.GetValue();

        return await op(result.GetError()).ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, E&gt;&gt; with async handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<U>> okFunc,
        Func<TErr, Task<U>> errFunc)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return await okFunc(result.GetValue()).ConfigureAwait(false);

        return await errFunc(result.GetError()).ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, E&gt;&gt; with synchronous handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, U> okFunc,
        Func<TErr, U> errFunc)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);

        var result = await resultTask.ConfigureAwait(false);
        return result.Match(okFunc, errFunc);
    }

    /// <summary>
    /// Executes an async action if the result is Ok, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task> action)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(action);

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            await action(result.GetValue()).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is Err, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapErrAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task> action)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(action);

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsErr)
            await action(result.GetError()).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Maps a Result&lt;T, E&gt; to Task&lt;Result&lt;U, E&gt;&gt; by applying an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, Task<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        var value = await mapper(result.GetValue()).ConfigureAwait(false);
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Chains an async operation on a Result&lt;T, E&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, Task<Result<U, TErr>>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        return await binder(result.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Result<T, TErr> result,
        Func<TErr, Task<Result<T, F>>> op)
    {
        ThrowHelper.ThrowIfNull(op);

        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        return await op(result.GetError()).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async action if the result is Ok, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Result<T, TErr> result,
        Func<T, Task> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (result.IsOk)
            await action(result.GetValue()).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Wraps a Result in a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Result<T, TErr>> AsTask<T, TErr>(this Result<T, TErr> result)
    {
        return Task.FromResult(result);
    }

    // ============================================================================
    // CancellationToken Overloads
    // ============================================================================

    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        var value = await mapper(result.GetValue(), cancellationToken).ConfigureAwait(false);
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> MapErrorAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, CancellationToken, Task<F>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        var error = await mapper(result.GetError(), cancellationToken).ConfigureAwait(false);
        return Result<T, F>.Err(error);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Result&lt;T, E&gt;&gt; with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, Task<Result<U, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(result.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error using an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, CancellationToken, Task<Result<T, F>>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        return await op(result.GetError(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Ok value or computes a default asynchronously with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrElseAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, CancellationToken, Task<T>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return result.GetValue();

        cancellationToken.ThrowIfCancellationRequested();
        return await op(result.GetError(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, E&gt;&gt; with async handlers and cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, Task<U>> okFunc,
        Func<TErr, CancellationToken, Task<U>> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsOk)
            return await okFunc(result.GetValue(), cancellationToken).ConfigureAwait(false);

        return await errFunc(result.GetError(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async action if the result is Ok with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetValue(), cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is Err with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapErrAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsErr)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetError(), cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Maps a Result&lt;T, E&gt; to Task&lt;Result&lt;U, E&gt;&gt; by applying an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        var value = await mapper(result.GetValue(), cancellationToken).ConfigureAwait(false);
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Chains an async operation on a Result&lt;T, E&gt; with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, CancellationToken, Task<Result<U, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        return await binder(result.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error using an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Result<T, TErr> result,
        Func<TErr, CancellationToken, Task<Result<T, F>>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        return await op(result.GetError(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async action if the result is Ok with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Result<T, TErr> result,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsOk)
            await action(result.GetValue(), cancellationToken).ConfigureAwait(false);

        return result;
    }
}
