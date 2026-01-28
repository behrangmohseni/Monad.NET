using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Async extensions for Result&lt;T, E&gt; to work seamlessly with Task-based asynchronous code.
/// All async methods support CancellationToken for proper cancellation handling.
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
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        var value = await mapper(result.GetValue()).ConfigureAwait(false);
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> MapErrorAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<F>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        var error = await mapper(result.GetError()).ConfigureAwait(false);
        return Result<T, F>.Err(error);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> MapErrorAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, F> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<Result<U, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(result.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains a synchronous operation on a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> BindAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Result<U, TErr>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(binder);
    }

    /// <summary>
    /// Recovers from an error using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<Result<T, F>>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return Result<T, F>.Ok(result.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        return await op(result.GetError()).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Ok value or computes a default asynchronously.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrElseAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<T>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return result.GetValue();

        cancellationToken.ThrowIfCancellationRequested();
        return await op(result.GetError()).ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, E&gt;&gt; with async handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<U>> okFunc,
        Func<TErr, Task<U>> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

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
        Func<TErr, U> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        return result.Match(okFunc, errFunc);
    }

    /// <summary>
    /// Executes an async action if the result is Ok, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetValue()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is Err, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> TapErrAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsErr)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetError()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Maps a Result&lt;T, E&gt; to Task&lt;Result&lt;U, E&gt;&gt; by applying an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

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
        Func<T, Task<Result<U, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

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
        Func<TErr, Task<Result<T, F>>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

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
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

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

    #region CancellationToken with Func Overloads

    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsOk)
            await action(result.GetValue(), cancellationToken).ConfigureAwait(false);

        return result;
    }

    #endregion

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a Result in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<T, TErr>> AsValueTask<T, TErr>(this Result<T, TErr> result)
        => new(result);

    /// <summary>
    /// Maps the Ok value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<U, TErr>> MapAsync<T, TErr, U>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(resultTask.Result.Map(mapper));
        }
        return Core(resultTask, mapper, cancellationToken);

        static async ValueTask<Result<U, TErr>> Core(ValueTask<Result<T, TErr>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Map(m);
        }
    }

    /// <summary>
    /// Maps the Ok value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Result<U, TErr>> MapAsync<T, TErr, U>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
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
    /// Maps the Err value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<T, F>> MapErrorAsync<T, TErr, F>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<TErr, F> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(resultTask.Result.MapError(mapper));
        }
        return Core(resultTask, mapper, cancellationToken);

        static async ValueTask<Result<T, F>> Core(ValueTask<Result<T, TErr>> t, Func<TErr, F> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.MapError(m);
        }
    }

    /// <summary>
    /// Chains a synchronous operation. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<U, TErr>> BindAsync<T, TErr, U>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, Result<U, TErr>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(resultTask.Result.Bind(binder));
        }
        return Core(resultTask, binder, cancellationToken);

        static async ValueTask<Result<U, TErr>> Core(ValueTask<Result<T, TErr>> t, Func<T, Result<U, TErr>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Bind(b);
        }
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Result<U, TErr>> BindAsync<T, TErr, U>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, ValueTask<Result<U, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(result.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, TErr, U>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, U> okFunc,
        Func<TErr, U> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(resultTask.Result.Match(okFunc, errFunc));
        }
        return Core(resultTask, okFunc, errFunc, cancellationToken);

        static async ValueTask<U> Core(ValueTask<Result<T, TErr>> t, Func<T, U> ok, Func<TErr, U> err, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Match(ok, err);
        }
    }

    /// <summary>
    /// Pattern matches with async handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<U> MatchAsync<T, TErr, U>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, ValueTask<U>> okFunc,
        Func<TErr, CancellationToken, ValueTask<U>> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(okFunc);
        ThrowHelper.ThrowIfNull(errFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return result.IsOk
            ? await okFunc(result.GetValue(), cancellationToken).ConfigureAwait(false)
            : await errFunc(result.GetError(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Ok value or computes a default. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> GetValueOrElseAsync<T, TErr>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<TErr, T> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(op);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(resultTask.Result.GetValueOrElse(op));
        }
        return Core(resultTask, op, cancellationToken);

        static async ValueTask<T> Core(ValueTask<Result<T, TErr>> t, Func<TErr, T> o, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.GetValueOrElse(o);
        }
    }

    /// <summary>
    /// Returns the Ok value or computes a default asynchronously.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> GetValueOrElseAsync<T, TErr>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<TErr, CancellationToken, ValueTask<T>> op,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(op);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask.ConfigureAwait(false);
        if (result.IsOk)
            return result.GetValue();

        cancellationToken.ThrowIfCancellationRequested();
        return await op(result.GetError(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action if Ok. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<T, TErr>> TapAsync<T, TErr>(
        this ValueTask<Result<T, TErr>> resultTask,
        Action<T> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var r = resultTask.Result;
            if (r.IsOk)
                action(r.GetValue());
            return new(r);
        }
        return Core(resultTask, action, cancellationToken);

        static async ValueTask<Result<T, TErr>> Core(ValueTask<Result<T, TErr>> t, Action<T> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsOk)
                a(r.GetValue());
            return r;
        }
    }

    /// <summary>
    /// Executes an async action if Ok.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Result<T, TErr>> TapAsync<T, TErr>(
        this ValueTask<Result<T, TErr>> resultTask,
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
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
    /// Executes an action if Err. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<T, TErr>> TapErrAsync<T, TErr>(
        this ValueTask<Result<T, TErr>> resultTask,
        Action<TErr> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        if (resultTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var r = resultTask.Result;
            if (r.IsErr)
                action(r.GetError());
            return new(r);
        }
        return Core(resultTask, action, cancellationToken);

        static async ValueTask<Result<T, TErr>> Core(ValueTask<Result<T, TErr>> t, Action<TErr> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsErr)
                a(r.GetError());
            return r;
        }
    }

    #endregion
}
