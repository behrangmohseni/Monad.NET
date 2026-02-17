using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

public static partial class ResultExtensions
{
    /// <summary>
    /// Wraps an async function that may throw an exception into a Result.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Ok with the result, or Err with the exception.</returns>
    public static async Task<Result<T, Exception>> TryAsync<T>(
        Func<Task<T>> func,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return Result<T, Exception>.Ok(await func().ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Error(ex);
        }
    }

    #region Async Combine

    /// <summary>
    /// Asynchronously combines two Result tasks into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = await Result.CombineAsync(
    ///     GetUserAsync(id),
    ///     GetOrderAsync(orderId)
    /// ); // Result&lt;(User, Order), Error&gt;
    /// </code>
    /// </example>
    public static async Task<Result<(T1, T2), TError>> CombineAsync<T1, T2, TError>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        return Combine(result1, result2);
    }

    /// <summary>
    /// Asynchronously combines two Result tasks using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = await Result.CombineAsync(
    ///     GetUserAsync(id),
    ///     GetOrderAsync(orderId),
    ///     (user, order) => new UserOrder(user, order)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<TResult, TError>> CombineAsync<T1, T2, TError, TResult>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        Func<T1, T2, TResult> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        return Combine(result1, result2, combiner);
    }

    /// <summary>
    /// Asynchronously combines three Result tasks into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<(T1, T2, T3), TError>> CombineAsync<T1, T2, T3, TError>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result3 = await third.ConfigureAwait(false);
        return Combine(result1, result2, result3);
    }

    /// <summary>
    /// Asynchronously combines three Result tasks using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<TResult, TError>> CombineAsync<T1, T2, T3, TError, TResult>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third,
        Func<T1, T2, T3, TResult> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result3 = await third.ConfigureAwait(false);
        return Combine(result1, result2, result3, combiner);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks into a single Result containing a list.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var userIds = new[] { 1, 2, 3 };
    /// var usersResult = await ResultExtensions.CombineAsync(
    ///     userIds.Select(id => GetUserAsync(id))
    /// );
    /// // Result&lt;IReadOnlyList&lt;User&gt;, Error&gt;
    /// </code>
    /// </example>
    public static async Task<Result<IReadOnlyList<T>, TError>> CombineAsync<T, TError>(
        IEnumerable<Task<Result<T, TError>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return Combine(results);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks, ignoring the values.
    /// Useful when you only care about success/failure, not the values.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<Unit, TError>> CombineAllAsync<T, TError>(
        IEnumerable<Task<Result<T, TError>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return CombineAll(results);
    }

    #endregion

    #region Error Aggregation Async (CombineErrorsAsync)

    /// <summary>
    /// Asynchronously combines two Result tasks, accumulating ALL errors from both if either/both fail.
    /// </summary>
    public static async Task<Result<(T1, T2), IReadOnlyList<TError>>> CombineErrorsAsync<T1, T2, TError>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);

        return CombineErrors(result1, result2);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks, accumulating ALL errors from all if any fail.
    /// </summary>
    public static async Task<Result<IReadOnlyList<T>, IReadOnlyList<TError>>> CombineErrorsAsync<T, TError>(
        IEnumerable<Task<Result<T, TError>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return CombineErrors(results);
    }

    #endregion

    #region Async Operations

    /// <summary>
    /// Maps the value with an async function.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="U">The target type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="mapper">An async function to apply to the value if Ok.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Ok with the mapped value, or the original Error.</returns>
    public static async Task<Result<U, TError>> MapAsync<T, U, TError>(
        this Result<T, TError> result,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (!result.IsOk)
            return Result<U, TError>.Error(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        var value = await mapper(result.GetValue()).ConfigureAwait(false);
        return Result<U, TError>.Ok(value);
    }

    /// <summary>
    /// Maps the value with an async function that takes a cancellation token.
    /// </summary>
    public static async Task<Result<U, TError>> MapAsync<T, U, TError>(
        this Result<T, TError> result,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (!result.IsOk)
            return Result<U, TError>.Error(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        var value = await mapper(result.GetValue(), cancellationToken).ConfigureAwait(false);
        return Result<U, TError>.Ok(value);
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="U">The target type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to chain.</param>
    /// <param name="binder">An async function that returns a new Result based on the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Ok, otherwise the original Error.</returns>
    public static async Task<Result<U, TError>> BindAsync<T, U, TError>(
        this Result<T, TError> result,
        Func<T, Task<Result<U, TError>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (!result.IsOk)
            return Result<U, TError>.Error(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(result.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains an async operation that takes a cancellation token.
    /// </summary>
    public static async Task<Result<U, TError>> BindAsync<T, U, TError>(
        this Result<T, TError> result,
        Func<T, CancellationToken, Task<Result<U, TError>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (!result.IsOk)
            return Result<U, TError>.Error(result.GetError());

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(result.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Maps the error with an async function.
    /// </summary>
    public static async Task<Result<T, UError>> MapErrorAsync<T, TError, UError>(
        this Result<T, TError> result,
        Func<TError, Task<UError>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (result.IsOk)
            return Result<T, UError>.Ok(result.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        var error = await mapper(result.GetError()).ConfigureAwait(false);
        return Result<T, UError>.Error(error);
    }

    /// <summary>
    /// Executes an async action if the result is Ok.
    /// </summary>
    public static async Task<Result<T, TError>> TapAsync<T, TError>(
        this Result<T, TError> result,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);

        if (result.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetValue()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is Ok, with cancellation support.
    /// </summary>
    public static async Task<Result<T, TError>> TapAsync<T, TError>(
        this Result<T, TError> result,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);

        if (result.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetValue(), cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is Error.
    /// </summary>
    public static async Task<Result<T, TError>> TapErrorAsync<T, TError>(
        this Result<T, TError> result,
        Func<TError, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);

        if (result.IsError)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(result.GetError()).ConfigureAwait(false);
        }

        return result;
    }

    // ============================================================================
    // Task<Result<T, TError>> Extensions - for chaining async operations
    // ============================================================================

    /// <summary>
    /// Maps the value of a Task&lt;Result&lt;T, TError&gt;&gt; with a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TError>> MapAsync<T, U, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Maps the value of a Task&lt;Result&lt;T, TError&gt;&gt; with an async function.
    /// </summary>
    public static async Task<Result<U, TError>> MapAsync<T, U, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.MapAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains a synchronous binder on a Task&lt;Result&lt;T, TError&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<U, TError>> BindAsync<T, U, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Result<U, TError>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(binder);
    }

    /// <summary>
    /// Chains an async binder on a Task&lt;Result&lt;T, TError&gt;&gt;.
    /// </summary>
    public static async Task<Result<U, TError>> BindAsync<T, U, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Task<Result<U, TError>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(binder, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Taps a Task&lt;Result&lt;T, TError&gt;&gt; with a synchronous action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TError>> TapAsync<T, TError>(
        this Task<Result<T, TError>> resultTask,
        Action<T> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Taps a Task&lt;Result&lt;T, TError&gt;&gt; with an async action.
    /// </summary>
    public static async Task<Result<T, TError>> TapAsync<T, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Taps a Task&lt;Result&lt;T, TError&gt;&gt; error with a synchronous action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TError>> TapErrorAsync<T, TError>(
        this Task<Result<T, TError>> resultTask,
        Action<TError> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    /// <summary>
    /// Taps a Task&lt;Result&lt;T, TError&gt;&gt; error with an async action.
    /// </summary>
    public static async Task<Result<T, TError>> TapErrorAsync<T, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<TError, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the value or a default from a Task&lt;Result&lt;T, TError&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrAsync<T, TError>(
        this Task<Result<T, TError>> resultTask,
        T defaultValue,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.GetValueOr(defaultValue);
    }

    /// <summary>
    /// Gets the value or evaluates a default factory from a Task&lt;Result&lt;T, TError&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrAsync<T, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T> defaultFactory,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(defaultFactory);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.IsOk ? result.GetValue() : defaultFactory();
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, TError&gt;&gt;.
    /// </summary>
    public static async Task<U> MatchAsync<T, TError, U>(
        this Task<Result<T, TError>> resultTask,
        Func<T, U> ok,
        Func<TError, U> error,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(ok);
        ThrowHelper.ThrowIfNull(error);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(ok, error);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Result&lt;T, TError&gt;&gt; with async handlers.
    /// </summary>
    public static async Task<U> MatchAsync<T, TError, U>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Task<U>> ok,
        Func<TError, Task<U>> error,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(ok);
        ThrowHelper.ThrowIfNull(error);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.IsOk
            ? await ok(result.GetValue()).ConfigureAwait(false)
            : await error(result.GetError()).ConfigureAwait(false);
    }

    #endregion
}
