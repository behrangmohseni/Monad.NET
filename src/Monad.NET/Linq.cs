using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// LINQ query syntax support for Option&lt;T&gt;.
/// Enables C# query comprehension syntax with from, let, where, and select.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
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
        return option.Bind(selector);
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
        return option.Bind(t =>
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
[EditorBrowsable(EditorBrowsableState.Never)]
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
        return result.Bind(selector);
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
        return result.Bind(t =>
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

        var value = result.GetValue();
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

        var value = result.GetValue();
        return predicate(value)
            ? result
            : Result<T, TErr>.Err(errorFactory(value));
    }
}

/// <summary>
/// LINQ query syntax support for Either&lt;L, R&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
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
        return either.Bind(selector);
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
        return either.Bind(t =>
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

        var value = either.GetRight();
        return predicate(value)
            ? either
            : Either<TLeft, TRight>.Left(leftIfFalse);
    }
}

/// <summary>
/// LINQ query syntax support for RemoteData&lt;T, E&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RemoteDataLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for RemoteData&lt;T, E&gt;.
    /// Equivalent to Map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<U, TErr> Select<T, TErr, U>(
        this RemoteData<T, TErr> remoteData,
        Func<T, U> selector)
    {
        return remoteData.Map(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for RemoteData&lt;T, E&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<U, TErr> SelectMany<T, TErr, U>(
        this RemoteData<T, TErr> remoteData,
        Func<T, RemoteData<U, TErr>> selector)
    {
        return remoteData.Bind(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector for RemoteData&lt;T, E&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<V, TErr> SelectMany<T, TErr, U, V>(
        this RemoteData<T, TErr> remoteData,
        Func<T, RemoteData<U, TErr>> selector,
        Func<T, U, V> resultSelector)
    {
        return remoteData.Bind(t =>
            selector(t).Map(u =>
                resultSelector(t, u)));
    }
}

/// <summary>
/// LINQ query syntax support for Try&lt;T&gt;.
/// Enables C# query comprehension syntax with from, let, where, and select.
/// </summary>
/// <example>
/// <code>
/// var result = from x in Try&lt;int&gt;.Of(() => Parse("42"))
///              from y in Try&lt;int&gt;.Of(() => Parse("10"))
///              select x + y;
/// </code>
/// </example>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TryLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for Try&lt;T&gt;.
    /// Equivalent to Map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<U> Select<T, U>(this Try<T> @try, Func<T, U> selector)
    {
        return @try.Map(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Try&lt;T&gt;.
    /// This is what makes query comprehension work.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<U> SelectMany<T, U>(
        this Try<T> @try,
        Func<T, Try<U>> selector)
    {
        return @try.Bind(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector.
    /// This allows multiple 'from' clauses in query syntax.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<V> SelectMany<T, U, V>(
        this Try<T> @try,
        Func<T, Try<U>> selector,
        Func<T, U, V> resultSelector)
    {
        return @try.Bind(t =>
            selector(t).Map(u =>
                resultSelector(t, u)));
    }

    /// <summary>
    /// Enables LINQ Where (filtering) for Try&lt;T&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Where<T>(this Try<T> @try, Func<T, bool> predicate)
    {
        return @try.Filter(predicate);
    }
}

/// <summary>
/// LINQ query syntax support for Validation&lt;T, E&gt;.
/// Enables C# query comprehension syntax with from, let, and select.
/// </summary>
/// <example>
/// <code>
/// var result = from name in ValidateName(input.Name)
///              from email in ValidateEmail(input.Email)
///              select new User(name, email);
/// </code>
/// </example>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ValidationLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for Validation&lt;T, E&gt;.
    /// Equivalent to Map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<U, TErr> Select<T, TErr, U>(
        this Validation<T, TErr> validation,
        Func<T, U> selector)
    {
        return validation.Map(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Validation&lt;T, E&gt;.
    /// This is what makes query comprehension work.
    /// Note: This uses AndThen which short-circuits on first error.
    /// For accumulating errors, use Validation.Apply or Validation.Zip instead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<U, TErr> SelectMany<T, TErr, U>(
        this Validation<T, TErr> validation,
        Func<T, Validation<U, TErr>> selector)
    {
        return validation.Bind(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector.
    /// This allows multiple 'from' clauses in query syntax.
    /// Note: This uses AndThen which short-circuits on first error.
    /// For accumulating errors, use Validation.Apply or Validation.Zip instead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<V, TErr> SelectMany<T, TErr, U, V>(
        this Validation<T, TErr> validation,
        Func<T, Validation<U, TErr>> selector,
        Func<T, U, V> resultSelector)
    {
        return validation.Bind(t =>
            selector(t).Map(u =>
                resultSelector(t, u)));
    }
}

/// <summary>
/// LINQ query syntax support for Writer&lt;W, T&gt;.
/// Enables C# query comprehension syntax with from, let, and select.
/// </summary>
/// <example>
/// <code>
/// var result = from x in Writer&lt;string, int&gt;.Tell(10, "Started with 10\n")
///              from y in Writer&lt;string, int&gt;.Tell(x * 2, "Doubled\n")
///              select y + 1;
/// </code>
/// </example>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WriterLinq
{
    /// <summary>
    /// Enables LINQ Select (projection) for Writer&lt;W, T&gt;.
    /// Equivalent to Map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<TLog, U> Select<TLog, T, U>(
        this Writer<TLog, T> writer,
        Func<T, U> selector)
    {
        return writer.Map(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Writer&lt;string, T&gt;.
    /// This is what makes query comprehension work.
    /// Uses string concatenation for log combination.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, U> SelectMany<T, U>(
        this Writer<string, T> writer,
        Func<T, Writer<string, U>> selector)
    {
        return writer.Bind(selector, (a, b) => a + b);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector for Writer&lt;string, T&gt;.
    /// This allows multiple 'from' clauses in query syntax.
    /// Uses string concatenation for log combination.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, V> SelectMany<T, U, V>(
        this Writer<string, T> writer,
        Func<T, Writer<string, U>> selector,
        Func<T, U, V> resultSelector)
    {
        return writer.Bind(t =>
            selector(t).Map(u =>
                resultSelector(t, u)), (a, b) => a + b);
    }

    /// <summary>
    /// Enables LINQ SelectMany (monadic bind) for Writer&lt;List&lt;TLog&gt;, T&gt;.
    /// Uses list concatenation for log combination.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<List<TLog>, U> SelectMany<TLog, T, U>(
        this Writer<List<TLog>, T> writer,
        Func<T, Writer<List<TLog>, U>> selector)
    {
        return writer.Bind(selector, (a, b) => a.Concat(b).ToList());
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector for Writer&lt;List&lt;TLog&gt;, T&gt;.
    /// Uses list concatenation for log combination.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<List<TLog>, V> SelectMany<TLog, T, U, V>(
        this Writer<List<TLog>, T> writer,
        Func<T, Writer<List<TLog>, U>> selector,
        Func<T, U, V> resultSelector)
    {
        return writer.Bind(t =>
            selector(t).Map(u =>
                resultSelector(t, u)), (a, b) => a.Concat(b).ToList());
    }
}
