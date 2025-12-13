namespace Monad.NET;

/// <summary>
/// Async extensions for Result&lt;T, E&gt; to work seamlessly with Task-based asynchronous code.
/// </summary>
public static class ResultAsyncExtensions
{
    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function.
    /// </summary>
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<U>> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var result = await resultTask;
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.UnwrapErr());

        var value = await mapper(result.Unwrap());
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Maps the Ok value inside a Task&lt;Result&lt;T, E&gt;&gt; using a synchronous function.
    /// </summary>
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using an async function.
    /// </summary>
    public static async Task<Result<T, F>> MapErrAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<F>> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var result = await resultTask;
        if (result.IsOk)
            return Result<T, F>.Ok(result.Unwrap());

        var error = await mapper(result.UnwrapErr());
        return Result<T, F>.Err(error);
    }

    /// <summary>
    /// Maps the Err value inside a Task&lt;Result&lt;T, E&gt;&gt; using a synchronous function.
    /// </summary>
    public static async Task<Result<T, F>> MapErrAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, F> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var result = await resultTask;
        return result.MapErr(mapper);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    public static async Task<Result<U, TErr>> AndThenAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<Result<U, TErr>>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        var result = await resultTask;
        if (!result.IsOk)
            return Result<U, TErr>.Err(result.UnwrapErr());

        return await binder(result.Unwrap());
    }

    /// <summary>
    /// Chains a synchronous operation on a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    public static async Task<Result<U, TErr>> AndThenAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Result<U, TErr>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        var result = await resultTask;
        return result.AndThen(binder);
    }

    /// <summary>
    /// Recovers from an error using an async function.
    /// </summary>
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<Result<T, F>>> op)
    {
        if (op is null)
            throw new ArgumentNullException(nameof(op));

        var result = await resultTask;
        if (result.IsOk)
            return Result<T, F>.Ok(result.Unwrap());

        return await op(result.UnwrapErr());
    }

    /// <summary>
    /// Returns the Ok value or computes a default asynchronously.
    /// </summary>
    public static async Task<T> UnwrapOrElseAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task<T>> op)
    {
        if (op is null)
            throw new ArgumentNullException(nameof(op));

        var result = await resultTask;
        if (result.IsOk)
            return result.Unwrap();

        return await op(result.UnwrapErr());
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, E&gt;&gt; with async handlers.
    /// </summary>
    public static async Task<U> MatchAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task<U>> okFunc,
        Func<TErr, Task<U>> errFunc)
    {
        if (okFunc is null)
            throw new ArgumentNullException(nameof(okFunc));
        if (errFunc is null)
            throw new ArgumentNullException(nameof(errFunc));

        var result = await resultTask;
        if (result.IsOk)
            return await okFunc(result.Unwrap());

        return await errFunc(result.UnwrapErr());
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, E&gt;&gt; with synchronous handlers.
    /// </summary>
    public static async Task<U> MatchAsync<T, TErr, U>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, U> okFunc,
        Func<TErr, U> errFunc)
    {
        if (okFunc is null)
            throw new ArgumentNullException(nameof(okFunc));
        if (errFunc is null)
            throw new ArgumentNullException(nameof(errFunc));

        var result = await resultTask;
        return result.Match(okFunc, errFunc);
    }

    /// <summary>
    /// Executes an async action if the result is Ok, allowing method chaining.
    /// </summary>
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var result = await resultTask;
        if (result.IsOk)
            await action(result.Unwrap());

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is Err, allowing method chaining.
    /// </summary>
    public static async Task<Result<T, TErr>> TapErrAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<TErr, Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var result = await resultTask;
        if (result.IsErr)
            await action(result.UnwrapErr());

        return result;
    }

    /// <summary>
    /// Maps a Result&lt;T, E&gt; to Task&lt;Result&lt;U, E&gt;&gt; by applying an async function.
    /// </summary>
    public static async Task<Result<U, TErr>> MapAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, Task<U>> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        if (!result.IsOk)
            return Result<U, TErr>.Err(result.UnwrapErr());

        var value = await mapper(result.Unwrap());
        return Result<U, TErr>.Ok(value);
    }

    /// <summary>
    /// Chains an async operation on a Result&lt;T, E&gt;.
    /// </summary>
    public static async Task<Result<U, TErr>> AndThenAsync<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, Task<Result<U, TErr>>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        if (!result.IsOk)
            return Result<U, TErr>.Err(result.UnwrapErr());

        return await binder(result.Unwrap());
    }

    /// <summary>
    /// Recovers from an error using an async function.
    /// </summary>
    public static async Task<Result<T, F>> OrElseAsync<T, TErr, F>(
        this Result<T, TErr> result,
        Func<TErr, Task<Result<T, F>>> op)
    {
        if (op is null)
            throw new ArgumentNullException(nameof(op));

        if (result.IsOk)
            return Result<T, F>.Ok(result.Unwrap());

        return await op(result.UnwrapErr());
    }

    /// <summary>
    /// Executes an async action if the result is Ok, allowing method chaining.
    /// </summary>
    public static async Task<Result<T, TErr>> TapAsync<T, TErr>(
        this Result<T, TErr> result,
        Func<T, Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (result.IsOk)
            await action(result.Unwrap());

        return result;
    }

    /// <summary>
    /// Wraps a Result in a Task&lt;Result&lt;T, E&gt;&gt;.
    /// </summary>
    public static Task<Result<T, TErr>> AsTask<T, TErr>(this Result<T, TErr> result)
    {
        return Task.FromResult(result);
    }
}

