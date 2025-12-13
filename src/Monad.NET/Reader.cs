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

    private Reader(Func<R, A> run)
    {
        _run = run ?? throw new ArgumentNullException(nameof(run));
    }

    /// <summary>
    /// Creates a Reader from a function.
    /// </summary>
    public static Reader<R, A> From(Func<R, A> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));
        
        return new Reader<R, A>(func);
    }

    /// <summary>
    /// Creates a Reader that returns a constant value, ignoring the environment.
    /// </summary>
    public static Reader<R, A> Pure(A value)
    {
        return new Reader<R, A>(_ => value);
    }

    /// <summary>
    /// Creates a Reader that returns the environment itself.
    /// </summary>
    public static Reader<R, R> Ask()
    {
        return new Reader<R, R>(env => env);
    }

    /// <summary>
    /// Creates a Reader that extracts a value from the environment.
    /// </summary>
    public static Reader<R, A> Asks(Func<R, A> selector)
    {
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));
        
        return new Reader<R, A>(selector);
    }

    /// <summary>
    /// Runs the Reader with the provided environment.
    /// </summary>
    public A Run(R environment)
    {
        if (environment is null)
            throw new ArgumentNullException(nameof(environment));
        
        return _run(environment);
    }

    /// <summary>
    /// Maps the result value.
    /// </summary>
    public Reader<R, B> Map<B>(Func<A, B> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return new Reader<R, B>(env => mapper(_run(env)));
    }

    /// <summary>
    /// Chains Reader computations.
    /// This is the monadic bind operation.
    /// </summary>
    public Reader<R, B> FlatMap<B>(Func<A, Reader<R, B>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
        return new Reader<R, B>(env =>
        {
            var a = _run(env);
            return binder(a).Run(env);
        });
    }

    /// <summary>
    /// Transforms the environment before running the computation.
    /// This allows using a Reader with a different environment type.
    /// </summary>
    public Reader<R2, A> WithEnvironment<R2>(Func<R2, R> transform)
    {
        if (transform is null)
            throw new ArgumentNullException(nameof(transform));
        
        return new Reader<R2, A>(env2 => _run(transform(env2)));
    }

    /// <summary>
    /// Combines two Readers that depend on the same environment.
    /// </summary>
    public Reader<R, C> Zip<B, C>(Reader<R, B> other, Func<A, B, C> combiner)
    {
        if (combiner is null)
            throw new ArgumentNullException(nameof(combiner));
        
        return new Reader<R, C>(env =>
        {
            var a = _run(env);
            var b = other.Run(env);
            return combiner(a, b);
        });
    }
}

/// <summary>
/// Extension methods and helpers for Reader&lt;R, A&gt;.
/// </summary>
public static class ReaderExtensions
{
    /// <summary>
    /// LINQ Select support for Reader.
    /// </summary>
    public static Reader<R, B> Select<R, A, B>(this Reader<R, A> reader, Func<A, B> selector)
    {
        return reader.Map(selector);
    }

    /// <summary>
    /// LINQ SelectMany support for Reader.
    /// </summary>
    public static Reader<R, B> SelectMany<R, A, B>(
        this Reader<R, A> reader,
        Func<A, Reader<R, B>> selector)
    {
        return reader.FlatMap(selector);
    }

    /// <summary>
    /// LINQ SelectMany support with result selector.
    /// </summary>
    public static Reader<R, C> SelectMany<R, A, B, C>(
        this Reader<R, A> reader,
        Func<A, Reader<R, B>> selector,
        Func<A, B, C> resultSelector)
    {
        if (resultSelector is null)
            throw new ArgumentNullException(nameof(resultSelector));

        return reader.FlatMap(a =>
            selector(a).Map(b =>
                resultSelector(a, b)));
    }

    /// <summary>
    /// Sequences a collection of Readers into a Reader of a collection.
    /// </summary>
    public static Reader<R, IEnumerable<A>> Sequence<R, A>(this IEnumerable<Reader<R, A>> readers)
    {
        if (readers is null)
            throw new ArgumentNullException(nameof(readers));

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
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

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
    public static Reader<R, A> From<R, A>(Func<R, A> func)
    {
        return Reader<R, A>.From(func);
    }

    /// <summary>
    /// Creates a Reader that returns a constant value.
    /// </summary>
    public static Reader<R, A> Pure<R, A>(A value)
    {
        return Reader<R, A>.Pure(value);
    }

    /// <summary>
    /// Creates a Reader that returns the environment itself.
    /// </summary>
    public static Reader<R, R> Ask<R>()
    {
        return Reader<R, R>.Ask();
    }

    /// <summary>
    /// Creates a Reader that extracts a value from the environment.
    /// </summary>
    public static Reader<R, A> Asks<R, A>(Func<R, A> selector)
    {
        return Reader<R, A>.Asks(selector);
    }
}

