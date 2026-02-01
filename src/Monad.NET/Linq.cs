// ============================================================================
// ADVANCED USAGE: LINQ Query Syntax Support
// ============================================================================
// These extension methods enable LINQ query syntax (from...select) for monadic
// types. While functional, the RECOMMENDED approach is to use the core methods
// directly: Map(), Bind(), Filter(), Match().
//
// LINQ syntax may hide important behavior:
// - Validation: LINQ short-circuits on first error (use Apply/Zip instead)
// - Option/Result: LINQ is verbose compared to fluent chains
//
// For most use cases, prefer:
//   option.Map(x => x * 2).Bind(Transform)
// Over:
//   from x in option
//   let doubled = x * 2
//   from result in Transform(doubled)
//   select result
// ============================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// <strong>Advanced Usage:</strong> LINQ query syntax support for Option&lt;T&gt;.
/// Enables C# query comprehension syntax with from, let, where, and select.
/// <para>
/// <strong>Recommended:</strong> Use <see cref="Option{T}.Map{U}"/> and <see cref="Option{T}.Bind{U}"/>
/// directly for clearer, more explicit code.
/// </para>
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
        return result.FilterOrElse(predicate, errorIfFalse);
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
        return result.FilterOrElse(predicate, errorFactory);
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
/// LINQ query syntax support for Validation&lt;T, TErr&gt;.
/// Enables C# query comprehension syntax with from, let, and select.
/// </summary>
/// <remarks>
/// <para>
/// <strong>IMPORTANT:</strong> This implementation attempts to accumulate errors by evaluating
/// subsequent validations even when earlier ones fail. However, this only works reliably when
/// validations are <em>independent</em> (don't use values from previous validations).
/// </para>
/// <para>
/// For <em>guaranteed</em> error accumulation, use <c>Apply()</c> or <c>Zip()</c> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Works well - validations are independent:
/// var result = from name in ValidateName(input.Name)
///              from email in ValidateEmail(input.Email)  // Errors accumulated!
///              select new User(name, email);
/// 
/// // For dependent validations or guaranteed accumulation, use Apply/Zip:
/// ValidateName(input.Name)
///     .Zip(ValidateEmail(input.Email))
///     .Map(t => new User(t.Item1, t.Item2));
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
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>WARNING: LINQ query syntax does NOT accumulate errors.</b>
    /// This method uses short-circuit evaluation (like <see cref="Validation{T,TErr}.Bind"/>)
    /// and stops at the first error, just like Result.
    /// </para>
    /// <para>
    /// <b>For error accumulation, use <see cref="Validation{T,TErr}.Apply"/> or 
    /// <see cref="Validation{T,TErr}.Zip"/> instead.</b>
    /// </para>
    /// <para>
    /// Example of error accumulation with Apply:
    /// <code>
    /// var result = ValidateName(name)
    ///     .Apply(ValidateEmail(email), (n, e) => (n, e))
    ///     .Apply(ValidateAge(age), (partial, a) => new User(partial.n, partial.e, a));
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<U, TErr> SelectMany<T, TErr, U>(
        this Validation<T, TErr> validation,
        Func<T, Validation<U, TErr>> selector)
    {
        // Short-circuit: stop at first error (safe, consistent with Bind)
        return validation.Bind(selector);
    }

    /// <summary>
    /// Enables LINQ SelectMany with result selector for multiple 'from' clauses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>WARNING: LINQ query syntax does NOT accumulate errors.</b>
    /// This method uses short-circuit evaluation and stops at the first error.
    /// </para>
    /// <para>
    /// <b>For error accumulation, use <see cref="Validation{T,TErr}.Apply"/> or 
    /// <see cref="Validation{T,TErr}.Zip"/> instead.</b>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<V, TErr> SelectMany<T, TErr, U, V>(
        this Validation<T, TErr> validation,
        Func<T, Validation<U, TErr>> collectionSelector,
        Func<T, U, V> resultSelector)
    {
        ThrowHelper.ThrowIfNull(collectionSelector);
        ThrowHelper.ThrowIfNull(resultSelector);

        // Short-circuit: stop at first error (safe, consistent with Result)
        return validation.Bind(t =>
            collectionSelector(t).Map(u =>
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
