using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

public static partial class TryExtensions
{
    /// <summary>
    /// Maps the value with an async function.
    /// </summary>
    /// <param name="try">The try to map.</param>
    /// <param name="mapper">An async function to apply to the value if Success.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Success with the mapped value, or the original Failure.</returns>
    public static async Task<Try<U>> MapAsync<T, U>(
        this Try<T> @try,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Error(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await mapper(@try.GetValue()).ConfigureAwait(false);
            return Try<U>.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Error(ex);
        }
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    /// <param name="try">The try to chain.</param>
    /// <param name="binder">An async function that returns a new Try based on the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Success, otherwise the original Failure.</returns>
    public static async Task<Try<U>> BindAsync<T, U>(
        this Try<T> @try,
        Func<T, Task<Try<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Error(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await binder(@try.GetValue()).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Error(ex);
        }
    }

    // ============================================================================
    // CancellationToken Overloads
    // ============================================================================

    /// <summary>
    /// Maps the value with an async function with cancellation support.
    /// </summary>
    public static async Task<Try<U>> MapAsync<T, U>(
        this Try<T> @try,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Error(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await mapper(@try.GetValue(), cancellationToken).ConfigureAwait(false);
            return Try<U>.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Error(ex);
        }
    }

    /// <summary>
    /// Chains an async operation with cancellation support.
    /// </summary>
    public static async Task<Try<U>> BindAsync<T, U>(
        this Try<T> @try,
        Func<T, CancellationToken, Task<Try<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Error(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await binder(@try.GetValue(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Error(ex);
        }
    }

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a Try in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Try<T>> AsValueTask<T>(this Try<T> @try)
        => new(@try);

    /// <summary>
    /// Maps the value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Try<U>> MapAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (tryTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(tryTask.Result.Map(mapper));
        }
        return Core(tryTask, mapper, cancellationToken);

        static async ValueTask<Try<U>> Core(ValueTask<Try<T>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Map(m);
        }
    }

    /// <summary>
    /// Maps the value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Try<U>> MapAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await tryTask.ConfigureAwait(false);
        if (!@try.IsOk)
            return Try<U>.Error(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await mapper(@try.GetValue(), cancellationToken).ConfigureAwait(false);
            return Try<U>.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Try<U>.Error(ex);
        }
    }

    /// <summary>
    /// Chains a synchronous operation. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Try<U>> BindAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, Try<U>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        if (tryTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(tryTask.Result.Bind(binder));
        }
        return Core(tryTask, binder, cancellationToken);

        static async ValueTask<Try<U>> Core(ValueTask<Try<T>> t, Func<T, Try<U>> b, CancellationToken ct)
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
    public static async ValueTask<Try<U>> BindAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, CancellationToken, ValueTask<Try<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await tryTask.ConfigureAwait(false);
        if (!@try.IsOk)
            return Try<U>.Error(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await binder(@try.GetValue(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Try<U>.Error(ex);
        }
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, U> successFunc,
        Func<Exception, U> failureFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(successFunc);
        ThrowHelper.ThrowIfNull(failureFunc);
        if (tryTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(tryTask.Result.Match(successFunc, failureFunc));
        }
        return Core(tryTask, successFunc, failureFunc, cancellationToken);

        static async ValueTask<U> Core(ValueTask<Try<T>> t, Func<T, U> s, Func<Exception, U> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Match(s, f);
        }
    }

    #endregion
}
