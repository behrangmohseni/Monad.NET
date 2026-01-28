using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Async LINQ extensions that enable query syntax for monadic async operations.
/// <example>
/// <code>
/// // Using LINQ query syntax with async Option
/// var result = await (
///     from user in GetUserAsync(id)
///     from orders in GetOrdersAsync(user.Id)
///     select new { user, orders }
/// );
/// 
/// // Using LINQ query syntax with async Result
/// var result = await (
///     from config in LoadConfigAsync()
///     from connection in ConnectAsync(config)
///     from data in FetchDataAsync(connection)
///     select ProcessData(data)
/// );
/// </code>
/// </example>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AsyncLinqExtensions
{
    #region Option Async LINQ

    /// <summary>
    /// Enables LINQ query syntax for async Option operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<TResult>> SelectMany<T, TCollection, TResult>(
        this Task<Option<T>> source,
        Func<T, Task<Option<TCollection>>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await source.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<TResult>.None();

        var value = option.GetValue();
        cancellationToken.ThrowIfCancellationRequested();

        var collection = await collectionSelector(value).ConfigureAwait(false);
        if (!collection.IsSome)
            return Option<TResult>.None();

        return Option<TResult>.Some(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ query syntax with sync collection selector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<TResult>> SelectMany<T, TCollection, TResult>(
        this Task<Option<T>> source,
        Func<T, Option<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await source.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<TResult>.None();

        var value = option.GetValue();
        var collection = collectionSelector(value);
        if (!collection.IsSome)
            return Option<TResult>.None();

        return Option<TResult>.Some(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ Select for async Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<TResult>> Select<T, TResult>(
        this Task<Option<T>> source,
        Func<T, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await source.ConfigureAwait(false);
        return option.Map(selector);
    }

    /// <summary>
    /// Enables LINQ Where for async Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> Where<T>(
        this Task<Option<T>> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await source.ConfigureAwait(false);
        return option.Filter(predicate);
    }

    #endregion

    #region Result Async LINQ

    /// <summary>
    /// Enables LINQ query syntax for async Result operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<TResult, TErr>> SelectMany<T, TErr, TCollection, TResult>(
        this Task<Result<T, TErr>> source,
        Func<T, Task<Result<TCollection, TErr>>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await source.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<TResult, TErr>.Err(result.GetError());

        var value = result.GetValue();
        cancellationToken.ThrowIfCancellationRequested();

        var collection = await collectionSelector(value).ConfigureAwait(false);
        if (!collection.IsOk)
            return Result<TResult, TErr>.Err(collection.GetError());

        return Result<TResult, TErr>.Ok(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ query syntax with sync collection selector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<TResult, TErr>> SelectMany<T, TErr, TCollection, TResult>(
        this Task<Result<T, TErr>> source,
        Func<T, Result<TCollection, TErr>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await source.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<TResult, TErr>.Err(result.GetError());

        var value = result.GetValue();
        var collection = collectionSelector(value);
        if (!collection.IsOk)
            return Result<TResult, TErr>.Err(collection.GetError());

        return Result<TResult, TErr>.Ok(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ Select for async Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<TResult, TErr>> Select<T, TErr, TResult>(
        this Task<Result<T, TErr>> source,
        Func<T, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await source.ConfigureAwait(false);
        return result.Map(selector);
    }

    #endregion

    #region Try Async LINQ

    /// <summary>
    /// Enables LINQ query syntax for async Try operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Try<TResult>> SelectMany<T, TCollection, TResult>(
        this Task<Try<T>> source,
        Func<T, Task<Try<TCollection>>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await source.ConfigureAwait(false);
        if (!@try.IsOk)
            return Try<TResult>.Failure(@try.GetException());

        var value = @try.GetValue();
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var collection = await collectionSelector(value).ConfigureAwait(false);
            if (!collection.IsOk)
                return Try<TResult>.Failure(collection.GetException());

            return Try<TResult>.Success(resultSelector(value, collection.GetValue()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Try<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Enables LINQ query syntax with sync collection selector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Try<TResult>> SelectMany<T, TCollection, TResult>(
        this Task<Try<T>> source,
        Func<T, Try<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await source.ConfigureAwait(false);
        if (!@try.IsOk)
            return Try<TResult>.Failure(@try.GetException());

        var value = @try.GetValue();

        try
        {
            var collection = collectionSelector(value);
            if (!collection.IsOk)
                return Try<TResult>.Failure(collection.GetException());

            return Try<TResult>.Success(resultSelector(value, collection.GetValue()));
        }
        catch (Exception ex)
        {
            return Try<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Enables LINQ Select for async Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Try<TResult>> Select<T, TResult>(
        this Task<Try<T>> source,
        Func<T, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await source.ConfigureAwait(false);
        return @try.Map(selector);
    }

    #endregion

    #region Validation Async LINQ

    /// <summary>
    /// Enables LINQ query syntax for async Validation operations.
    /// Note: Validation accumulates errors, so this behaves like applicative rather than monadic bind.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Validation<TResult, TErr>> SelectMany<T, TErr, TCollection, TResult>(
        this Task<Validation<T, TErr>> source,
        Func<T, Task<Validation<TCollection, TErr>>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await source.ConfigureAwait(false);
        if (validation.IsError)
            return Validation<TResult, TErr>.Invalid(validation.GetErrors());

        var value = validation.GetValue();
        cancellationToken.ThrowIfCancellationRequested();

        var collection = await collectionSelector(value).ConfigureAwait(false);
        if (collection.IsError)
            return Validation<TResult, TErr>.Invalid(collection.GetErrors());

        return Validation<TResult, TErr>.Valid(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ query syntax with sync collection selector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Validation<TResult, TErr>> SelectMany<T, TErr, TCollection, TResult>(
        this Task<Validation<T, TErr>> source,
        Func<T, Validation<TCollection, TErr>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await source.ConfigureAwait(false);
        if (validation.IsError)
            return Validation<TResult, TErr>.Invalid(validation.GetErrors());

        var value = validation.GetValue();
        var collection = collectionSelector(value);
        if (collection.IsError)
            return Validation<TResult, TErr>.Invalid(collection.GetErrors());

        return Validation<TResult, TErr>.Valid(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ Select for async Validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Validation<TResult, TErr>> Select<T, TErr, TResult>(
        this Task<Validation<T, TErr>> source,
        Func<T, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await source.ConfigureAwait(false);
        return validation.Map(selector);
    }

    #endregion

    #region ValueTask LINQ Overloads

    /// <summary>
    /// Enables LINQ query syntax for ValueTask Option operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Option<TResult>> SelectMany<T, TCollection, TResult>(
        this ValueTask<Option<T>> source,
        Func<T, ValueTask<Option<TCollection>>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await source.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<TResult>.None();

        var value = option.GetValue();
        cancellationToken.ThrowIfCancellationRequested();

        var collection = await collectionSelector(value).ConfigureAwait(false);
        if (!collection.IsSome)
            return Option<TResult>.None();

        return Option<TResult>.Some(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ query syntax for ValueTask Result operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Result<TResult, TErr>> SelectMany<T, TErr, TCollection, TResult>(
        this ValueTask<Result<T, TErr>> source,
        Func<T, ValueTask<Result<TCollection, TErr>>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await source.ConfigureAwait(false);
        if (!result.IsOk)
            return Result<TResult, TErr>.Err(result.GetError());

        var value = result.GetValue();
        cancellationToken.ThrowIfCancellationRequested();

        var collection = await collectionSelector(value).ConfigureAwait(false);
        if (!collection.IsOk)
            return Result<TResult, TErr>.Err(collection.GetError());

        return Result<TResult, TErr>.Ok(resultSelector(value, collection.GetValue()));
    }

    /// <summary>
    /// Enables LINQ Select for ValueTask Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<TResult>> Select<T, TResult>(
        this ValueTask<Option<T>> source,
        Func<T, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(selector);

        if (source.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(source.Result.Map(selector));
        }

        return Core(source, selector, cancellationToken);

        static async ValueTask<Option<TResult>> Core(ValueTask<Option<T>> s, Func<T, TResult> sel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var option = await s.ConfigureAwait(false);
            return option.Map(sel);
        }
    }

    /// <summary>
    /// Enables LINQ Select for ValueTask Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<TResult, TErr>> Select<T, TErr, TResult>(
        this ValueTask<Result<T, TErr>> source,
        Func<T, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(selector);

        if (source.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(source.Result.Map(selector));
        }

        return Core(source, selector, cancellationToken);

        static async ValueTask<Result<TResult, TErr>> Core(ValueTask<Result<T, TErr>> s, Func<T, TResult> sel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await s.ConfigureAwait(false);
            return result.Map(sel);
        }
    }

    /// <summary>
    /// Enables LINQ Where for ValueTask Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<T>> Where<T>(
        this ValueTask<Option<T>> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);

        if (source.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(source.Result.Filter(predicate));
        }

        return Core(source, predicate, cancellationToken);

        static async ValueTask<Option<T>> Core(ValueTask<Option<T>> s, Func<T, bool> p, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var option = await s.ConfigureAwait(false);
            return option.Filter(p);
        }
    }

    #endregion
}
