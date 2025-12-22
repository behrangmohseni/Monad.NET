using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents an asynchronous computation that depends on a shared environment.
/// The ReaderAsync monad is the async variant of Reader, used for dependency injection in a functional way
/// when the computations are asynchronous.
/// </summary>
/// <typeparam name="R">The type of the environment/dependencies</typeparam>
/// <typeparam name="A">The type of the result</typeparam>
/// <remarks>
/// <para>
/// ReaderAsync is useful when your computations depend on an environment (like configuration,
/// database connections, or services) and are inherently asynchronous.
/// </para>
/// <para>
/// Key benefits:
/// - Dependencies are explicit in the type signature
/// - Async computations can be composed before execution
/// - Testing is easier as the environment can be mocked
/// - Works seamlessly with async/await patterns
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a computation that reads from a database asynchronously
/// var getUser = ReaderAsync&lt;DbContext, User&gt;.From(async db => 
///     await db.Users.FindAsync(userId));
/// 
/// // Compose async computations
/// var program = 
///     from user in getUser
///     from orders in ReaderAsync&lt;DbContext, List&lt;Order&gt;&gt;.From(async db =>
///         await db.Orders.Where(o => o.UserId == user.Id).ToListAsync())
///     select new UserWithOrders(user, orders);
/// 
/// // Execute with the environment
/// var result = await program.RunAsync(dbContext);
/// </code>
/// </example>
public sealed class ReaderAsync<R, A>
{
    private readonly Func<R, Task<A>> _run;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReaderAsync(Func<R, Task<A>> run)
    {
        ArgumentNullException.ThrowIfNull(run);
        _run = run;
    }

    /// <summary>
    /// Creates a ReaderAsync from an async function.
    /// </summary>
    /// <param name="func">The async function that takes the environment and returns a Task.</param>
    /// <returns>A new ReaderAsync instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> From(Func<R, Task<A>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return new ReaderAsync<R, A>(func);
    }

    /// <summary>
    /// Creates a ReaderAsync from a synchronous Reader.
    /// </summary>
    /// <param name="reader">The synchronous Reader to convert.</param>
    /// <returns>A new ReaderAsync instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> FromReader(Reader<R, A> reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return new ReaderAsync<R, A>(env => Task.FromResult(reader.Run(env)));
    }

    /// <summary>
    /// Creates a ReaderAsync that returns a constant value, ignoring the environment.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A new ReaderAsync that returns the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> Pure(A value)
    {
        return new ReaderAsync<R, A>(_ => Task.FromResult(value));
    }

    /// <summary>
    /// Creates a ReaderAsync that returns the environment itself.
    /// </summary>
    /// <returns>A new ReaderAsync that returns the environment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, R> Ask()
    {
        return new ReaderAsync<R, R>(static env => Task.FromResult(env));
    }

    /// <summary>
    /// Creates a ReaderAsync that extracts a value from the environment synchronously.
    /// </summary>
    /// <param name="selector">The function to extract a value from the environment.</param>
    /// <returns>A new ReaderAsync that returns the extracted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> Asks(Func<R, A> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new ReaderAsync<R, A>(env => Task.FromResult(selector(env)));
    }

    /// <summary>
    /// Creates a ReaderAsync that extracts a value from the environment asynchronously.
    /// </summary>
    /// <param name="selector">The async function to extract a value from the environment.</param>
    /// <returns>A new ReaderAsync that returns the extracted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> AsksAsync(Func<R, Task<A>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new ReaderAsync<R, A>(selector);
    }

    /// <summary>
    /// Runs the ReaderAsync with the provided environment.
    /// </summary>
    /// <param name="environment">The environment to run the computation with.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A Task containing the result of the computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<A> RunAsync(R environment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(environment);
        cancellationToken.ThrowIfCancellationRequested();
        return await _run(environment).ConfigureAwait(false);
    }

    /// <summary>
    /// Maps the result value using a synchronous transformation.
    /// </summary>
    /// <typeparam name="B">The type of the transformed result.</typeparam>
    /// <param name="mapper">The transformation function.</param>
    /// <returns>A new ReaderAsync with the transformed result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, B> Map<B>(Func<A, B> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var run = _run;
        return ReaderAsync<R, B>.From(async env =>
        {
            var result = await run(env).ConfigureAwait(false);
            return mapper(result);
        });
    }

    /// <summary>
    /// Maps the result value using an asynchronous transformation.
    /// </summary>
    /// <typeparam name="B">The type of the transformed result.</typeparam>
    /// <param name="mapper">The async transformation function.</param>
    /// <returns>A new ReaderAsync with the transformed result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, B> MapAsync<B>(Func<A, Task<B>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var run = _run;
        return ReaderAsync<R, B>.From(async env =>
        {
            var result = await run(env).ConfigureAwait(false);
            return await mapper(result).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Executes an action with the computed value without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the computed value.</param>
    /// <returns>A new ReaderAsync that executes the action.</returns>
    /// <example>
    /// <code>
    /// readerAsync.Tap(x => Console.WriteLine($"Value: {x}"))
    ///            .Map(x => x.ToUpper());
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> Tap(Action<A> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            var result = await run(env).ConfigureAwait(false);
            action(result);
            return result;
        });
    }

    /// <summary>
    /// Executes an async action with the computed value without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The async action to execute with the computed value.</param>
    /// <returns>A new ReaderAsync that executes the action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> TapAsync(Func<A, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            var result = await run(env).ConfigureAwait(false);
            await action(result).ConfigureAwait(false);
            return result;
        });
    }

    /// <summary>
    /// Executes an action with the environment without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the environment.</param>
    /// <returns>A new ReaderAsync that executes the action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> TapEnv(Action<R> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            action(env);
            return await run(env).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Executes an async action with the environment without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The async action to execute with the environment.</param>
    /// <returns>A new ReaderAsync that executes the action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> TapEnvAsync(Func<R, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            await action(env).ConfigureAwait(false);
            return await run(env).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Chains ReaderAsync computations (monadic bind).
    /// </summary>
    /// <typeparam name="B">The type of the new result.</typeparam>
    /// <param name="binder">A function that takes the result and returns a new ReaderAsync.</param>
    /// <returns>A new ReaderAsync representing the chained computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, B> FlatMap<B>(Func<A, ReaderAsync<R, B>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var run = _run;
        return ReaderAsync<R, B>.From(async env =>
        {
            var a = await run(env).ConfigureAwait(false);
            return await binder(a).RunAsync(env).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Chains ReaderAsync computations with an async binder.
    /// </summary>
    /// <typeparam name="B">The type of the new result.</typeparam>
    /// <param name="binder">An async function that takes the result and returns a new ReaderAsync.</param>
    /// <returns>A new ReaderAsync representing the chained computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, B> FlatMapAsync<B>(Func<A, Task<ReaderAsync<R, B>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var run = _run;
        return ReaderAsync<R, B>.From(async env =>
        {
            var a = await run(env).ConfigureAwait(false);
            var nextReader = await binder(a).ConfigureAwait(false);
            return await nextReader.RunAsync(env).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Chains ReaderAsync computations.
    /// Alias for <see cref="FlatMap{B}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, B> AndThen<B>(Func<A, ReaderAsync<R, B>> binder) => FlatMap(binder);

    /// <summary>
    /// Chains ReaderAsync computations.
    /// Alias for <see cref="FlatMap{B}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, B> Bind<B>(Func<A, ReaderAsync<R, B>> binder) => FlatMap(binder);

    /// <summary>
    /// Transforms the environment before running the computation.
    /// This allows using a ReaderAsync with a different environment type.
    /// </summary>
    /// <typeparam name="R2">The new environment type.</typeparam>
    /// <param name="transform">The function to transform the new environment to the expected type.</param>
    /// <returns>A new ReaderAsync that works with the new environment type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R2, A> WithEnvironment<R2>(Func<R2, R> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        var run = _run;
        return ReaderAsync<R2, A>.From(env2 => run(transform(env2)));
    }

    /// <summary>
    /// Transforms the environment before running the computation, asynchronously.
    /// </summary>
    /// <typeparam name="R2">The new environment type.</typeparam>
    /// <param name="transform">The async function to transform the new environment to the expected type.</param>
    /// <returns>A new ReaderAsync that works with the new environment type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R2, A> WithEnvironmentAsync<R2>(Func<R2, Task<R>> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        var run = _run;
        return ReaderAsync<R2, A>.From(async env2 =>
        {
            var env = await transform(env2).ConfigureAwait(false);
            return await run(env).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Combines two ReaderAsync computations that depend on the same environment.
    /// </summary>
    /// <typeparam name="B">The type of the other result.</typeparam>
    /// <typeparam name="C">The type of the combined result.</typeparam>
    /// <param name="other">The other ReaderAsync to combine with.</param>
    /// <param name="combiner">The function to combine the results.</param>
    /// <returns>A new ReaderAsync that produces the combined result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, C> Zip<B, C>(ReaderAsync<R, B> other, Func<A, B, C> combiner)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(combiner);

        var run = _run;
        return ReaderAsync<R, C>.From(async env =>
        {
            var taskA = run(env);
            var taskB = other.RunAsync(env);
            await Task.WhenAll(taskA, taskB).ConfigureAwait(false);
            return combiner(taskA.Result, taskB.Result);
        });
    }

    /// <summary>
    /// Combines two ReaderAsync computations into a tuple.
    /// </summary>
    /// <typeparam name="B">The type of the other result.</typeparam>
    /// <param name="other">The other ReaderAsync to combine with.</param>
    /// <returns>A new ReaderAsync that produces a tuple of both results.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, (A, B)> Zip<B>(ReaderAsync<R, B> other)
    {
        return Zip(other, static (a, b) => (a, b));
    }

    /// <summary>
    /// Attempts to run the ReaderAsync and wraps the result in a Try.
    /// </summary>
    /// <returns>A ReaderAsync that produces a Try containing the result or exception.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, Try<A>> Attempt()
    {
        var run = _run;
        return ReaderAsync<R, Try<A>>.From(async env =>
        {
            try
            {
                var result = await run(env).ConfigureAwait(false);
                return Try<A>.Success(result);
            }
            catch (Exception ex)
            {
                return Try<A>.Failure(ex);
            }
        });
    }

    /// <summary>
    /// Provides a fallback ReaderAsync if this one throws an exception.
    /// </summary>
    /// <param name="fallback">The fallback ReaderAsync to use on failure.</param>
    /// <returns>A new ReaderAsync that uses the fallback on failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> OrElse(ReaderAsync<R, A> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            try
            {
                return await run(env).ConfigureAwait(false);
            }
            catch
            {
                return await fallback.RunAsync(env).ConfigureAwait(false);
            }
        });
    }

    /// <summary>
    /// Provides a fallback value if this ReaderAsync throws an exception.
    /// </summary>
    /// <param name="fallbackValue">The fallback value to use on failure.</param>
    /// <returns>A new ReaderAsync that uses the fallback value on failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, A> OrElse(A fallbackValue)
    {
        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            try
            {
                return await run(env).ConfigureAwait(false);
            }
            catch
            {
                return fallbackValue;
            }
        });
    }

    /// <summary>
    /// Retries the ReaderAsync computation a specified number of times on failure.
    /// </summary>
    /// <param name="retries">The number of times to retry (0 means no retries, just one attempt).</param>
    /// <returns>A ReaderAsync that retries on failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when retries is negative.</exception>
    public ReaderAsync<R, A> Retry(int retries)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            Exception? lastException = null;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return await run(env).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }
            throw lastException!;
        });
    }

    /// <summary>
    /// Retries the ReaderAsync computation with a delay between attempts.
    /// </summary>
    /// <param name="retries">The number of times to retry (0 means no retries, just one attempt).</param>
    /// <param name="delay">The delay between retry attempts.</param>
    /// <returns>A ReaderAsync that retries with delay on failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when retries is negative.</exception>
    public ReaderAsync<R, A> RetryWithDelay(int retries, TimeSpan delay)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var run = _run;
        return ReaderAsync<R, A>.From(async env =>
        {
            Exception? lastException = null;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return await run(env).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < retries)
                    {
                        await Task.Delay(delay).ConfigureAwait(false);
                    }
                }
            }
            throw lastException!;
        });
    }

    /// <summary>
    /// Ignores the result and replaces it with Unit.
    /// </summary>
    /// <returns>A ReaderAsync that returns Unit.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReaderAsync<R, Unit> Void()
    {
        return Map(_ => Unit.Default);
    }
}

/// <summary>
/// Extension methods and helpers for ReaderAsync&lt;R, A&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ReaderAsyncExtensions
{
    /// <summary>
    /// LINQ Select support for ReaderAsync.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, B> Select<R, A, B>(this ReaderAsync<R, A> reader, Func<A, B> selector)
    {
        return reader.Map(selector);
    }

    /// <summary>
    /// LINQ SelectMany support for ReaderAsync.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, B> SelectMany<R, A, B>(
        this ReaderAsync<R, A> reader,
        Func<A, ReaderAsync<R, B>> selector)
    {
        return reader.FlatMap(selector);
    }

    /// <summary>
    /// LINQ SelectMany support with result selector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, C> SelectMany<R, A, B, C>(
        this ReaderAsync<R, A> reader,
        Func<A, ReaderAsync<R, B>> selector,
        Func<A, B, C> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(resultSelector);

        return reader.FlatMap(a =>
            selector(a).Map(b =>
                resultSelector(a, b)));
    }

    /// <summary>
    /// Sequences a collection of ReaderAsync computations into a ReaderAsync of a collection.
    /// </summary>
    /// <typeparam name="R">The environment type.</typeparam>
    /// <typeparam name="A">The result type.</typeparam>
    /// <param name="readers">The collection of ReaderAsync computations.</param>
    /// <returns>A ReaderAsync that produces a list of results.</returns>
    public static ReaderAsync<R, IReadOnlyList<A>> Sequence<R, A>(this IEnumerable<ReaderAsync<R, A>> readers)
    {
        ArgumentNullException.ThrowIfNull(readers);

        var readerList = readers.ToList();
        return ReaderAsync<R, IReadOnlyList<A>>.From(async env =>
        {
            var results = new List<A>(readerList.Count);
            foreach (var reader in readerList)
            {
                results.Add(await reader.RunAsync(env).ConfigureAwait(false));
            }
            return results;
        });
    }

    /// <summary>
    /// Sequences a collection of ReaderAsync computations in parallel.
    /// </summary>
    /// <typeparam name="R">The environment type.</typeparam>
    /// <typeparam name="A">The result type.</typeparam>
    /// <param name="readers">The collection of ReaderAsync computations.</param>
    /// <returns>A ReaderAsync that produces a list of results computed in parallel.</returns>
    public static ReaderAsync<R, IReadOnlyList<A>> SequenceParallel<R, A>(this IEnumerable<ReaderAsync<R, A>> readers)
    {
        ArgumentNullException.ThrowIfNull(readers);

        var readerList = readers.ToList();
        return ReaderAsync<R, IReadOnlyList<A>>.From(async env =>
        {
            var tasks = readerList.Select(r => r.RunAsync(env)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(t => t.Result).ToList();
        });
    }

    /// <summary>
    /// Maps each element and sequences the results.
    /// </summary>
    /// <typeparam name="R">The environment type.</typeparam>
    /// <typeparam name="A">The source element type.</typeparam>
    /// <typeparam name="B">The result element type.</typeparam>
    /// <param name="items">The collection of items to traverse.</param>
    /// <param name="selector">The function to apply to each item.</param>
    /// <returns>A ReaderAsync that produces a list of results.</returns>
    public static ReaderAsync<R, IReadOnlyList<B>> Traverse<R, A, B>(
        this IEnumerable<A> items,
        Func<A, ReaderAsync<R, B>> selector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);

        return items.Select(selector).Sequence();
    }

    /// <summary>
    /// Maps each element and sequences the results in parallel.
    /// </summary>
    /// <typeparam name="R">The environment type.</typeparam>
    /// <typeparam name="A">The source element type.</typeparam>
    /// <typeparam name="B">The result element type.</typeparam>
    /// <param name="items">The collection of items to traverse.</param>
    /// <param name="selector">The function to apply to each item.</param>
    /// <returns>A ReaderAsync that produces a list of results computed in parallel.</returns>
    public static ReaderAsync<R, IReadOnlyList<B>> TraverseParallel<R, A, B>(
        this IEnumerable<A> items,
        Func<A, ReaderAsync<R, B>> selector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);

        return items.Select(selector).SequenceParallel();
    }

    /// <summary>
    /// Flattens a nested ReaderAsync.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> Flatten<R, A>(this ReaderAsync<R, ReaderAsync<R, A>> nested)
    {
        return nested.FlatMap(static inner => inner);
    }
}

/// <summary>
/// Static helper class for creating ReaderAsync computations without specifying generic types.
/// </summary>
public static class ReaderAsync
{
    /// <summary>
    /// Creates a ReaderAsync from an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> From<R, A>(Func<R, Task<A>> func)
    {
        return ReaderAsync<R, A>.From(func);
    }

    /// <summary>
    /// Creates a ReaderAsync from a synchronous Reader.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> FromReader<R, A>(Reader<R, A> reader)
    {
        return ReaderAsync<R, A>.FromReader(reader);
    }

    /// <summary>
    /// Creates a ReaderAsync that returns a constant value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> Pure<R, A>(A value)
    {
        return ReaderAsync<R, A>.Pure(value);
    }

    /// <summary>
    /// Creates a ReaderAsync that returns the environment itself.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, R> Ask<R>()
    {
        return ReaderAsync<R, R>.Ask();
    }

    /// <summary>
    /// Creates a ReaderAsync that extracts a value from the environment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> Asks<R, A>(Func<R, A> selector)
    {
        return ReaderAsync<R, A>.Asks(selector);
    }

    /// <summary>
    /// Creates a ReaderAsync that extracts a value from the environment asynchronously.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReaderAsync<R, A> AsksAsync<R, A>(Func<R, Task<A>> selector)
    {
        return ReaderAsync<R, A>.AsksAsync(selector);
    }

    /// <summary>
    /// Runs multiple ReaderAsync computations in parallel.
    /// </summary>
    public static ReaderAsync<R, (A, B)> Parallel<R, A, B>(
        ReaderAsync<R, A> first,
        ReaderAsync<R, B> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        return ReaderAsync<R, (A, B)>.From(async env =>
        {
            var task1 = first.RunAsync(env);
            var task2 = second.RunAsync(env);
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
            return (task1.Result, task2.Result);
        });
    }

    /// <summary>
    /// Runs multiple ReaderAsync computations in parallel.
    /// </summary>
    public static ReaderAsync<R, (A, B, C)> Parallel<R, A, B, C>(
        ReaderAsync<R, A> first,
        ReaderAsync<R, B> second,
        ReaderAsync<R, C> third)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(third);

        return ReaderAsync<R, (A, B, C)>.From(async env =>
        {
            var task1 = first.RunAsync(env);
            var task2 = second.RunAsync(env);
            var task3 = third.RunAsync(env);
            await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result);
        });
    }

    /// <summary>
    /// Runs a collection of ReaderAsync computations in parallel.
    /// </summary>
    public static ReaderAsync<R, IReadOnlyList<A>> Parallel<R, A>(IEnumerable<ReaderAsync<R, A>> readers)
    {
        return readers.SequenceParallel();
    }
}

