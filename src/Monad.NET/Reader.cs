using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a computation that depends on a shared environment.
/// The Reader monad is used for dependency injection in a functional way.
/// Instead of passing dependencies through every function call, they're injected at the end.
/// </summary>
/// <typeparam name="R">The type of the environment/dependencies</typeparam>
/// <typeparam name="A">The type of the result</typeparam>
public sealed class Reader<R, A>
{
    private readonly Func<R, A> _run;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Reader(Func<R, A> run)
    {
        ThrowHelper.ThrowIfNull(run);
        _run = run;
    }

    /// <summary>
    /// Creates a Reader from a function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, A> From(Func<R, A> func)
    {
        ThrowHelper.ThrowIfNull(func);

        return new Reader<R, A>(func);
    }

    /// <summary>
    /// Creates a Reader that returns a constant value, ignoring the environment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, A> Return(A value)
    {
        return new Reader<R, A>(_ => value);
    }

    /// <summary>
    /// Creates a Reader that returns the environment itself.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, R> Ask()
    {
        return new Reader<R, R>(static env => env);
    }

    /// <summary>
    /// Creates a Reader that extracts a value from the environment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, A> Asks(Func<R, A> selector)
    {
        ThrowHelper.ThrowIfNull(selector);

        return new Reader<R, A>(selector);
    }

    /// <summary>
    /// Runs the Reader with the provided environment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public A Run(R environment)
    {
        ThrowHelper.ThrowIfNull(environment);

        return _run(environment);
    }

    /// <summary>
    /// Maps the result value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader<R, B> Map<B>(Func<A, B> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        var run = _run;
        return new Reader<R, B>(env => mapper(run(env)));
    }

    /// <summary>
    /// Executes an action with the computed value without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the computed value.</param>
    /// <returns>A new Reader that executes the action.</returns>
    /// <example>
    /// <code>
    /// reader.Tap(x => Console.WriteLine($"Value: {x}"))
    ///       .Map(x => x.ToUpper());
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader<R, A> Tap(Action<A> action)
    {
        ThrowHelper.ThrowIfNull(action);

        var run = _run;
        return new Reader<R, A>(env =>
        {
            var result = run(env);
            action(result);
            return result;
        });
    }

    /// <summary>
    /// Executes an action with the environment without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the environment.</param>
    /// <returns>A new Reader that executes the action.</returns>
    /// <example>
    /// <code>
    /// reader.TapEnv(env => Console.WriteLine($"Environment: {env}"))
    ///       .Map(x => x.ToUpper());
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader<R, A> TapEnv(Action<R> action)
    {
        ThrowHelper.ThrowIfNull(action);

        var run = _run;
        return new Reader<R, A>(env =>
        {
            action(env);
            return run(env);
        });
    }

    /// <summary>
    /// Chains Reader computations.
    /// This is the monadic bind operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader<R, B> Bind<B>(Func<A, Reader<R, B>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        var run = _run;
        return new Reader<R, B>(env =>
        {
            var a = run(env);
            return binder(a).Run(env);
        });
    }

    /// <summary>
    /// Transforms the environment before running the computation.
    /// This allows using a Reader with a different environment type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader<R2, A> WithEnvironment<R2>(Func<R2, R> transform)
    {
        ThrowHelper.ThrowIfNull(transform);

        var run = _run;
        return Reader<R2, A>.From(env2 => run(transform(env2)));
    }

    /// <summary>
    /// Combines two Readers that depend on the same environment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Reader<R, C> Zip<B, C>(Reader<R, B> other, Func<A, B, C> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        var run = _run;
        return Reader<R, C>.From(env =>
        {
            var a = run(env);
            var b = other.Run(env);
            return combiner(a, b);
        });
    }

    /// <summary>
    /// Converts this synchronous Reader to an async ReaderAsync.
    /// </summary>
    /// <returns>A ReaderAsync that wraps this Reader's computation.</returns>
    /// <example>
    /// <code>
    /// var reader = Reader&lt;Config, int&gt;.Asks(c => c.Timeout);
    /// var asyncReader = reader.ToAsync();
    /// var result = await asyncReader.RunAsync(config);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> ToAsync()
    {
        return ReaderAsync<R, A>.FromReader(this);
    }
}

/// <summary>
/// Extension methods and helpers for Reader&lt;R, A&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ReaderExtensions
{
    /// <summary>
    /// LINQ Select support for Reader.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, B> Select<R, A, B>(this Reader<R, A> reader, Func<A, B> selector)
    {
        return reader.Map(selector);
    }

    /// <summary>
    /// LINQ SelectMany support for Reader.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, B> SelectMany<R, A, B>(
        this Reader<R, A> reader,
        Func<A, Reader<R, B>> selector)
    {
        return reader.Bind(selector);
    }

    /// <summary>
    /// LINQ SelectMany support with result selector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, C> SelectMany<R, A, B, C>(
        this Reader<R, A> reader,
        Func<A, Reader<R, B>> selector,
        Func<A, B, C> resultSelector)
    {
        ThrowHelper.ThrowIfNull(resultSelector);

        return reader.Bind(a =>
            selector(a).Map(b =>
                resultSelector(a, b)));
    }

    /// <summary>
    /// Sequences a collection of Readers into a Reader of a collection.
    /// </summary>
    public static Reader<R, IEnumerable<A>> Sequence<R, A>(this IEnumerable<Reader<R, A>> readers)
    {
        ThrowHelper.ThrowIfNull(readers);

        return Reader<R, IEnumerable<A>>.From(env =>
            readers.Select(reader => reader.Run(env)).ToList()
        );
    }

    /// <summary>
    /// Maps each element and sequences the results.
    /// </summary>
    public static Reader<R, IEnumerable<B>> Traverse<R, A, B>(
        this IEnumerable<A> items,
        Func<A, Reader<R, B>> selector)
    {
        ThrowHelper.ThrowIfNull(items);
        ThrowHelper.ThrowIfNull(selector);

        return Reader<R, IEnumerable<B>>.From(env =>
            items.Select(item => selector(item).Run(env)).ToList()
        );
    }
}

/// <summary>
/// Static helper class for creating Readers without specifying generic types.
/// </summary>
public static class Reader
{
    /// <summary>
    /// Creates a Reader from a function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, A> From<R, A>(Func<R, A> func)
    {
        return Reader<R, A>.From(func);
    }

    /// <summary>
    /// Creates a Reader that returns a constant value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, A> Return<R, A>(A value)
    {
        return Reader<R, A>.Return(value);
    }

    /// <summary>
    /// Creates a Reader that returns the environment itself.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, R> Ask<R>()
    {
        return Reader<R, R>.Ask();
    }

    /// <summary>
    /// Creates a Reader that extracts a value from the environment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Reader<R, A> Asks<R, A>(Func<R, A> selector)
    {
        return Reader<R, A>.Asks(selector);
    }
}
