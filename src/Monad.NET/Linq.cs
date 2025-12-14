using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// LINQ query syntax support for Option&lt;T&gt;.
/// Enables C# query comprehension syntax with from, let, where, and select.
/// </summary>
public static class OptionLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for Option&lt;T&gt;.
    /// Equivalent to Map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<U> Select<T, U>(this Option<T> option, Func<T, U> selector)
    {
        return option.Map(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Option&lt;T&gt;.
    /// This is what makes query comprehension work.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<U> SelectMany<T, U>(
        this Option<T> option,
        Func<T, Option<U>> selector)
    {
        return option.AndThen(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector.
    /// This allows multiple 'from' clauses in query syntax.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<V> SelectMany<T, U, V>(
        this Option<T> option,
        Func<T, Option<U>> selector,
        Func<T, U, V> resultSelector)
    {
        return option.AndThen(t =>
            selector(t).Map(u =>
                resultSelector(t, u)));
    }

    /// <summary>
    /// Enables LINQ Where (filtering) for Option&lt;T&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate)
    {
        return option.Filter(predicate);
    }
}

/// <summary>
/// LINQ query syntax support for Result&lt;T, E&gt;.
/// Enables C# query comprehension syntax with from, let, where, and select.
/// </summary>
public static class ResultLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for Result&lt;T, E&gt;.
    /// Equivalent to Map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<U, TErr> Select<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, U> selector)
    {
        return result.Map(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Result&lt;T, E&gt;.
    /// This is what makes query comprehension work.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<U, TErr> SelectMany<T, TErr, U>(
        this Result<T, TErr> result,
        Func<T, Result<U, TErr>> selector)
    {
        return result.AndThen(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector.
    /// This allows multiple 'from' clauses in query syntax.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<V, TErr> SelectMany<T, TErr, U, V>(
        this Result<T, TErr> result,
        Func<T, Result<U, TErr>> selector,
        Func<T, U, V> resultSelector)
    {
        return result.AndThen(t =>
            selector(t).Map(u =>
                resultSelector(t, u)));
    }

    /// <summary>
    /// Enables LINQ Where (filtering) for Result&lt;T, E&gt;.
    /// Converts Ok to Err if the predicate is not satisfied.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Where<T, TErr>(
        this Result<T, TErr> result,
        Func<T, bool> predicate,
        TErr errorIfFalse)
    {
        if (!result.IsOk)
            return result;

        var value = result.Unwrap();
        return predicate(value)
            ? result
            : Result<T, TErr>.Err(errorIfFalse);
    }

    /// <summary>
    /// Enables LINQ Where (filtering) for Result&lt;T, E&gt; with error factory.
    /// Converts Ok to Err if the predicate is not satisfied.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Where<T, TErr>(
        this Result<T, TErr> result,
        Func<T, bool> predicate,
        Func<T, TErr> errorFactory)
    {
        if (!result.IsOk)
            return result;

        var value = result.Unwrap();
        return predicate(value)
            ? result
            : Result<T, TErr>.Err(errorFactory(value));
    }
}

/// <summary>
/// LINQ query syntax support for Either&lt;L, R&gt;.
/// </summary>
public static class EitherLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for Either&lt;L, R&gt; on the Right side.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, U> Select<TLeft, TRight, U>(
        this Either<TLeft, TRight> either,
        Func<TRight, U> selector)
    {
        return either.MapRight(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Either&lt;L, R&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, U> SelectMany<TLeft, TRight, U>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, U>> selector)
    {
        return either.AndThen(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector for Either&lt;L, R&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, V> SelectMany<TLeft, TRight, U, V>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, U>> selector,
        Func<TRight, U, V> resultSelector)
    {
        return either.AndThen(t =>
            selector(t).MapRight(u =>
                resultSelector(t, u)));
    }

    /// <summary>
    /// Enables LINQ Where (filtering) for Either&lt;L, R&gt;.
    /// Converts Right to Left if the predicate is not satisfied.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> Where<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Func<TRight, bool> predicate,
        TLeft leftIfFalse)
    {
        if (!either.IsRight)
            return either;

        var value = either.UnwrapRight();
        return predicate(value)
            ? either
            : Either<TLeft, TRight>.Left(leftIfFalse);
    }
}
