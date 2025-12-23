using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a deferred computation that may perform side effects.
/// The IO monad captures side effects as values, allowing them to be composed
/// and executed in a controlled manner.
/// </summary>
/// <typeparam name="T">The type of the value produced by the computation.</typeparam>
/// <remarks>
/// <para>
/// The IO monad is fundamental in purely functional programming. It separates
/// the description of a computation from its execution, making side effects explicit.
/// </para>
/// <para>
/// Key benefits:
/// - Side effects are explicit in the type signature
/// - Computations can be composed before execution
/// - Testing is easier as effects can be mocked
/// - Referential transparency is preserved until Run() is called
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Describe a computation that reads from console
/// var readLine = IO&lt;string&gt;.Of(() => Console.ReadLine()!);
/// 
/// // Describe a computation that writes to console
/// IO&lt;Unit&gt; WriteLine(string message) => 
///     IO&lt;Unit&gt;.Of(() => { Console.WriteLine(message); return Unit.Default; });
/// 
/// // Compose them
/// var program = 
///     from _1 in WriteLine("What is your name?")
///     from name in readLine
///     from _2 in WriteLine($"Hello, {name}!")
///     select Unit.Default;
/// 
/// // Execute
/// program.Run();
/// </code>
/// </example>
public readonly struct IO<T>
{
    private readonly Func<T> _effect;

    private IO(Func<T> effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        _effect = effect;
    }

    /// <summary>
    /// Creates an IO action from an effect function.
    /// </summary>
    /// <param name="effect">The function representing the side effect.</param>
    /// <returns>An IO action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Of(Func<T> effect)
    {
        return new IO<T>(effect);
    }

    /// <summary>
    /// Creates an IO action that produces a pure value without any side effects.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>An IO action that returns the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Pure(T value)
    {
        return new IO<T>(() => value);
    }

    /// <summary>
    /// Creates an IO action that produces a pure value without any side effects.
    /// Alias for <see cref="Pure"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Return(T value) => Pure(value);

    /// <summary>
    /// Creates a lazily evaluated IO action.
    /// The computation is deferred until Run() is called.
    /// </summary>
    /// <param name="effect">The function to defer.</param>
    /// <returns>An IO action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Delay(Func<T> effect) => Of(effect);

    /// <summary>
    /// Runs the IO action and returns the result.
    /// This is where the side effects actually happen.
    /// </summary>
    /// <returns>The result of the computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Run()
    {
        return _effect();
    }

    /// <summary>
    /// Runs the IO action asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<T> RunAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(_effect, cancellationToken);
    }

    /// <summary>
    /// Transforms the result of the IO action.
    /// </summary>
    /// <typeparam name="U">The type of the transformed result.</typeparam>
    /// <param name="mapper">The transformation function.</param>
    /// <returns>A new IO action with the transformed result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<U> Map<U>(Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var effect = _effect;
        return IO<U>.Of(() => mapper(effect()));
    }

    /// <summary>
    /// Chains this IO action with another that depends on the result.
    /// </summary>
    /// <typeparam name="U">The type of the new result.</typeparam>
    /// <param name="binder">A function that produces a new IO based on the result.</param>
    /// <returns>A new IO action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<U> AndThen<U>(Func<T, IO<U>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var effect = _effect;
        return IO<U>.Of(() => binder(effect()).Run());
    }

    /// <summary>
    /// Chains this IO action with another that depends on the result.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<U> FlatMap<U>(Func<T, IO<U>> binder) => AndThen(binder);

    /// <summary>
    /// Chains this IO action with another that depends on the result.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<U> Bind<U>(Func<T, IO<U>> binder) => AndThen(binder);

    /// <summary>
    /// Executes a side effect with the result without changing the value.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A new IO action that executes the side effect.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var effect = _effect;
        return IO<T>.Of(() =>
        {
            var result = effect();
            action(result);
            return result;
        });
    }

    /// <summary>
    /// Applies a function wrapped in an IO to this IO's value.
    /// </summary>
    /// <typeparam name="U">The type of the result.</typeparam>
    /// <param name="ioFunc">An IO containing a function.</param>
    /// <returns>A new IO with the function applied.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<U> Apply<U>(IO<Func<T, U>> ioFunc)
    {
        var effect = _effect;
        return IO<U>.Of(() => ioFunc.Run()(effect()));
    }

    /// <summary>
    /// Combines this IO with another, producing a tuple of both results.
    /// </summary>
    /// <typeparam name="U">The type of the other result.</typeparam>
    /// <param name="other">The other IO action.</param>
    /// <returns>An IO that produces a tuple of both results.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<(T, U)> Zip<U>(IO<U> other)
    {
        return AndThen(a => other.Map(b => (a, b)));
    }

    /// <summary>
    /// Combines this IO with another using a combiner function.
    /// </summary>
    /// <typeparam name="U">The type of the other result.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="other">The other IO action.</param>
    /// <param name="combiner">A function to combine the results.</param>
    /// <returns>An IO that produces the combined result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<V> ZipWith<U, V>(IO<U> other, Func<T, U, V> combiner)
    {
        ArgumentNullException.ThrowIfNull(combiner);

        return AndThen(a => other.Map(b => combiner(a, b)));
    }

    /// <summary>
    /// Ignores the result and replaces it with a new value.
    /// </summary>
    /// <typeparam name="U">The type of the new result.</typeparam>
    /// <param name="value">The new value.</param>
    /// <returns>An IO that returns the new value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<U> As<U>(U value)
    {
        return Map(_ => value);
    }

    /// <summary>
    /// Ignores the result and replaces it with Unit.
    /// </summary>
    /// <returns>An IO that returns Unit.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<Unit> Void()
    {
        return As(Unit.Default);
    }

    /// <summary>
    /// Attempts to run the IO action and wraps the result in a Try.
    /// </summary>
    /// <returns>An IO that produces a Try containing the result or exception.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<Try<T>> Attempt()
    {
        var effect = _effect;
        return IO<Try<T>>.Of(() => Try<T>.Of(effect));
    }

    /// <summary>
    /// Converts this IO to an async IO.
    /// </summary>
    /// <returns>An async IO action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<T> ToAsync()
    {
        var effect = _effect;
        return IOAsync<T>.Of(async () => await Task.Run(effect).ConfigureAwait(false));
    }

    /// <summary>
    /// Provides a fallback IO action if this one throws an exception.
    /// </summary>
    /// <param name="fallback">The fallback IO action.</param>
    /// <returns>An IO that uses the fallback on failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<T> OrElse(IO<T> fallback)
    {
        var effect = _effect;
        return IO<T>.Of(() =>
        {
            try
            {
                return effect();
            }
            catch
            {
                return fallback.Run();
            }
        });
    }

    /// <summary>
    /// Provides a fallback value if this IO throws an exception.
    /// </summary>
    /// <param name="fallbackValue">The fallback value.</param>
    /// <returns>An IO that uses the fallback on failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IO<T> OrElse(T fallbackValue)
    {
        var effect = _effect;
        return IO<T>.Of(() =>
        {
            try
            {
                return effect();
            }
            catch
            {
                return fallbackValue;
            }
        });
    }

    /// <summary>
    /// Repeats this IO action the specified number of times, collecting results.
    /// </summary>
    /// <param name="count">The number of times to repeat.</param>
    /// <returns>An IO that produces a list of results.</returns>
    public IO<IReadOnlyList<T>> Replicate(int count)
    {
        if (count < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(count), "Count must be non-negative.");

        var effect = _effect;
        return IO<IReadOnlyList<T>>.Of(() =>
        {
            var results = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                results.Add(effect());
            }
            return results;
        });
    }

    /// <summary>
    /// Retries the IO action the specified number of times on failure.
    /// </summary>
    /// <param name="retries">The number of retry attempts.</param>
    /// <returns>An IO that retries on failure.</returns>
    public IO<T> Retry(int retries)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var effect = _effect;
        return IO<T>.Of(() =>
        {
            Exception? lastException = null;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return effect();
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
    /// Retries the IO action with a delay between attempts.
    /// </summary>
    /// <param name="retries">The number of retry attempts.</param>
    /// <param name="delay">The delay between attempts.</param>
    /// <returns>An async IO that retries with delays.</returns>
    public IOAsync<T> RetryWithDelay(int retries, TimeSpan delay)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            Exception? lastException = null;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return effect();
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
}

/// <summary>
/// Represents an asynchronous IO action that may perform side effects.
/// </summary>
/// <typeparam name="T">The type of the value produced by the computation.</typeparam>
public readonly struct IOAsync<T>
{
    private readonly Func<Task<T>> _effect;

    private IOAsync(Func<Task<T>> effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        _effect = effect;
    }

    /// <summary>
    /// Creates an async IO action from an async effect function.
    /// </summary>
    /// <param name="effect">The async function representing the side effect.</param>
    /// <returns>An async IO action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> Of(Func<Task<T>> effect)
    {
        return new IOAsync<T>(effect);
    }

    /// <summary>
    /// Creates an async IO action that produces a pure value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>An async IO action that returns the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> Pure(T value)
    {
        return new IOAsync<T>(() => Task.FromResult(value));
    }

    /// <summary>
    /// Creates an async IO action from a synchronous IO.
    /// </summary>
    /// <param name="io">The synchronous IO action.</param>
    /// <returns>An async IO action.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> FromIO(IO<T> io)
    {
        return new IOAsync<T>(() => Task.FromResult(io.Run()));
    }

    /// <summary>
    /// Runs the async IO action and returns the result.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<T> RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _effect().ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms the result of the async IO action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<U> Map<U>(Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var effect = _effect;
        return IOAsync<U>.Of(async () => mapper(await effect().ConfigureAwait(false)));
    }

    /// <summary>
    /// Transforms the result of the async IO action with an async mapper.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<U> MapAsync<U>(Func<T, Task<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var effect = _effect;
        return IOAsync<U>.Of(async () =>
        {
            var result = await effect().ConfigureAwait(false);
            return await mapper(result).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Chains this async IO action with another.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<U> AndThen<U>(Func<T, IOAsync<U>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var effect = _effect;
        return IOAsync<U>.Of(async () =>
        {
            var result = await effect().ConfigureAwait(false);
            return await binder(result).RunAsync().ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Chains this async IO action with another.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<U> FlatMap<U>(Func<T, IOAsync<U>> binder) => AndThen(binder);

    /// <summary>
    /// Executes a side effect with the result without changing the value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            var result = await effect().ConfigureAwait(false);
            action(result);
            return result;
        });
    }

    /// <summary>
    /// Executes an async side effect with the result without changing the value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<T> TapAsync(Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            var result = await effect().ConfigureAwait(false);
            await action(result).ConfigureAwait(false);
            return result;
        });
    }

    /// <summary>
    /// Combines this async IO with another, producing a tuple of both results.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<(T, U)> Zip<U>(IOAsync<U> other)
    {
        return AndThen(a => other.Map(b => (a, b)));
    }

    /// <summary>
    /// Ignores the result and replaces it with Unit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<Unit> Void()
    {
        return Map(_ => Unit.Default);
    }

    /// <summary>
    /// Attempts to run the async IO action and wraps the result in a Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<Try<T>> Attempt()
    {
        var effect = _effect;
        return IOAsync<Try<T>>.Of(async () =>
        {
            try
            {
                return Try<T>.Success(await effect().ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return Try<T>.Failure(ex);
            }
        });
    }

    /// <summary>
    /// Provides a fallback async IO action if this one throws an exception.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IOAsync<T> OrElse(IOAsync<T> fallback)
    {
        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            try
            {
                return await effect().ConfigureAwait(false);
            }
            catch
            {
                return await fallback.RunAsync().ConfigureAwait(false);
            }
        });
    }

    /// <summary>
    /// Retries the async IO action a specified number of times on failure.
    /// </summary>
    /// <param name="retries">The number of times to retry (0 means no retries, just one attempt).</param>
    /// <returns>An async IO action that retries on failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when retries is negative.</exception>
    public IOAsync<T> Retry(int retries)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            Exception? lastException = null;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return await effect().ConfigureAwait(false);
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
    /// Retries the async IO action with a delay between attempts.
    /// </summary>
    /// <param name="retries">The number of times to retry (0 means no retries, just one attempt).</param>
    /// <param name="delay">The delay between retry attempts.</param>
    /// <returns>An async IO action that retries with delay on failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when retries is negative.</exception>
    public IOAsync<T> RetryWithDelay(int retries, TimeSpan delay)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            Exception? lastException = null;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return await effect().ConfigureAwait(false);
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
    /// Retries the async IO action with exponential backoff between attempts.
    /// </summary>
    /// <param name="retries">The number of times to retry (0 means no retries, just one attempt).</param>
    /// <param name="initialDelay">The initial delay before the first retry. Subsequent delays double.</param>
    /// <param name="maxDelay">The maximum delay between retries (optional).</param>
    /// <returns>An async IO action that retries with exponential backoff on failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when retries is negative.</exception>
    public IOAsync<T> RetryWithExponentialBackoff(int retries, TimeSpan initialDelay, TimeSpan? maxDelay = null)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(retries), "Retries must be non-negative.");

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            Exception? lastException = null;
            var currentDelay = initialDelay;
            for (var i = 0; i <= retries; i++)
            {
                try
                {
                    return await effect().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < retries)
                    {
                        await Task.Delay(currentDelay).ConfigureAwait(false);
                        currentDelay = TimeSpan.FromTicks(currentDelay.Ticks * 2);
                        if (maxDelay.HasValue && currentDelay > maxDelay.Value)
                        {
                            currentDelay = maxDelay.Value;
                        }
                    }
                }
            }
            throw lastException!;
        });
    }

    /// <summary>
    /// Retries the async IO action while a condition is true.
    /// </summary>
    /// <param name="shouldRetry">A function that determines if a retry should be attempted based on the exception and attempt number.</param>
    /// <param name="maxRetries">The maximum number of retries (optional, defaults to int.MaxValue).</param>
    /// <returns>An async IO action that retries while the condition is met.</returns>
    public IOAsync<T> RetryWhile(Func<Exception, int, bool> shouldRetry, int maxRetries = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(shouldRetry);

        var effect = _effect;
        return IOAsync<T>.Of(async () =>
        {
            var attempt = 0;
            while (true)
            {
                try
                {
                    return await effect().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (attempt >= maxRetries || !shouldRetry(ex, attempt))
                    {
                        throw;
                    }
                    attempt++;
                }
            }
        });
    }
}

/// <summary>
/// Extension methods for IO monad.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IOExtensions
{
    /// <summary>
    /// Flattens a nested IO action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Flatten<T>(this IO<IO<T>> nested)
    {
        return nested.AndThen(static inner => inner);
    }

    /// <summary>
    /// Flattens a nested async IO action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> Flatten<T>(this IOAsync<IOAsync<T>> nested)
    {
        return nested.AndThen(static inner => inner);
    }

    /// <summary>
    /// Sequences a collection of IO actions into an IO of a collection.
    /// </summary>
    public static IO<IReadOnlyList<T>> Sequence<T>(this IEnumerable<IO<T>> ios)
    {
        ArgumentNullException.ThrowIfNull(ios);

        return IO<IReadOnlyList<T>>.Of(() =>
        {
            var results = new List<T>();
            foreach (var io in ios)
            {
                results.Add(io.Run());
            }
            return results;
        });
    }

    /// <summary>
    /// Sequences a collection of async IO actions into an async IO of a collection.
    /// </summary>
    public static IOAsync<IReadOnlyList<T>> Sequence<T>(this IEnumerable<IOAsync<T>> ios)
    {
        ArgumentNullException.ThrowIfNull(ios);

        var ioList = ios.ToList();
        return IOAsync<IReadOnlyList<T>>.Of(async () =>
        {
            var results = new List<T>(ioList.Count);
            foreach (var io in ioList)
            {
                results.Add(await io.RunAsync().ConfigureAwait(false));
            }
            return results;
        });
    }

    /// <summary>
    /// Traverses a collection, applying an IO-producing function to each element.
    /// </summary>
    public static IO<IReadOnlyList<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, IO<U>> func)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(func);

        return source.Select(func).Sequence();
    }

    /// <summary>
    /// Traverses a collection, applying an async IO-producing function to each element.
    /// </summary>
    public static IOAsync<IReadOnlyList<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, IOAsync<U>> func)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(func);

        return source.Select(func).Sequence();
    }

    /// <summary>
    /// LINQ Select support for IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<U> Select<T, U>(this IO<T> io, Func<T, U> selector)
    {
        return io.Map(selector);
    }

    /// <summary>
    /// LINQ SelectMany support for IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<U> SelectMany<T, U>(this IO<T> io, Func<T, IO<U>> selector)
    {
        return io.AndThen(selector);
    }

    /// <summary>
    /// LINQ SelectMany support with result selector for IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<V> SelectMany<T, U, V>(
        this IO<T> io,
        Func<T, IO<U>> selector,
        Func<T, U, V> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(resultSelector);

        return io.AndThen(a => selector(a).Map(b => resultSelector(a, b)));
    }

    /// <summary>
    /// LINQ Select support for async IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<U> Select<T, U>(this IOAsync<T> io, Func<T, U> selector)
    {
        return io.Map(selector);
    }

    /// <summary>
    /// LINQ SelectMany support for async IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<U> SelectMany<T, U>(this IOAsync<T> io, Func<T, IOAsync<U>> selector)
    {
        return io.AndThen(selector);
    }

    /// <summary>
    /// LINQ SelectMany support with result selector for async IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<V> SelectMany<T, U, V>(
        this IOAsync<T> io,
        Func<T, IOAsync<U>> selector,
        Func<T, U, V> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(resultSelector);

        return io.AndThen(a => selector(a).Map(b => resultSelector(a, b)));
    }
}

/// <summary>
/// Static helper class for creating IO actions.
/// </summary>
public static class IO
{
    /// <summary>
    /// Creates an IO action from an effect function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Of<T>(Func<T> effect) => IO<T>.Of(effect);

    /// <summary>
    /// Creates an IO action that returns a pure value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<T> Pure<T>(T value) => IO<T>.Pure(value);

    /// <summary>
    /// Creates an IO action that executes an action and returns Unit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<Unit> Execute(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return IO<Unit>.Of(() =>
        {
            action();
            return Unit.Default;
        });
    }

    /// <summary>
    /// Creates an IO action that writes to the console.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<Unit> WriteLine(string message)
    {
        return IO<Unit>.Of(() =>
        {
            Console.WriteLine(message);
            return Unit.Default;
        });
    }

    /// <summary>
    /// Creates an IO action that reads a line from the console.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<string?> ReadLine()
    {
        return IO<string?>.Of(Console.ReadLine);
    }

    /// <summary>
    /// Creates an IO action that reads the current time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<DateTime> Now()
    {
        return IO<DateTime>.Of(() => DateTime.Now);
    }

    /// <summary>
    /// Creates an IO action that reads the current UTC time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<DateTime> UtcNow()
    {
        return IO<DateTime>.Of(() => DateTime.UtcNow);
    }

    /// <summary>
    /// Creates an IO action that generates a new GUID.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<Guid> NewGuid()
    {
        return IO<Guid>.Of(Guid.NewGuid);
    }

    /// <summary>
    /// Creates an IO action that generates a random integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<int> Random()
    {
        return IO<int>.Of(() => System.Random.Shared.Next());
    }

    /// <summary>
    /// Creates an IO action that generates a random integer in the specified range.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<int> Random(int minValue, int maxValue)
    {
        return IO<int>.Of(() => System.Random.Shared.Next(minValue, maxValue));
    }

    /// <summary>
    /// Creates an IO action that reads an environment variable.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IO<Option<string>> GetEnvironmentVariable(string variable)
    {
        return IO<Option<string>>.Of(() =>
            Environment.GetEnvironmentVariable(variable).ToOption());
    }

    /// <summary>
    /// Runs multiple IO actions in parallel.
    /// </summary>
    public static IO<(T1, T2)> Parallel<T1, T2>(IO<T1> io1, IO<T2> io2)
    {
        return IO<(T1, T2)>.Of(() =>
        {
            var task1 = Task.Run(() => io1.Run());
            var task2 = Task.Run(() => io2.Run());
            Task.WaitAll(task1, task2);
            return (task1.Result, task2.Result);
        });
    }

    /// <summary>
    /// Runs multiple IO actions in parallel.
    /// </summary>
    public static IO<(T1, T2, T3)> Parallel<T1, T2, T3>(IO<T1> io1, IO<T2> io2, IO<T3> io3)
    {
        return IO<(T1, T2, T3)>.Of(() =>
        {
            var task1 = Task.Run(() => io1.Run());
            var task2 = Task.Run(() => io2.Run());
            var task3 = Task.Run(() => io3.Run());
            Task.WaitAll(task1, task2, task3);
            return (task1.Result, task2.Result, task3.Result);
        });
    }

    /// <summary>
    /// Runs multiple IO actions in parallel.
    /// </summary>
    public static IO<IReadOnlyList<T>> Parallel<T>(IEnumerable<IO<T>> ios)
    {
        ArgumentNullException.ThrowIfNull(ios);

        var ioList = ios.ToList();
        return IO<IReadOnlyList<T>>.Of(() =>
        {
            var tasks = ioList.Select(io => Task.Run(() => io.Run())).ToArray();
            Task.WaitAll(tasks);
            return tasks.Select(t => t.Result).ToList();
        });
    }

    /// <summary>
    /// Races multiple IO actions, returning the result of the first to complete.
    /// </summary>
    public static IO<T> Race<T>(IO<T> io1, IO<T> io2)
    {
        return IO<T>.Of(() =>
        {
            var task1 = Task.Run(() => io1.Run());
            var task2 = Task.Run(() => io2.Run());
            var completed = Task.WhenAny(task1, task2).Result;
            return completed.Result;
        });
    }
}

/// <summary>
/// Static helper class for creating async IO actions.
/// </summary>
public static class IOAsync
{
    /// <summary>
    /// Creates an async IO action from an async effect function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> Of<T>(Func<Task<T>> effect) => IOAsync<T>.Of(effect);

    /// <summary>
    /// Creates an async IO action that returns a pure value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> Pure<T>(T value) => IOAsync<T>.Pure(value);

    /// <summary>
    /// Creates an async IO action from a synchronous IO.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<T> FromIO<T>(IO<T> io) => IOAsync<T>.FromIO(io);

    /// <summary>
    /// Creates an async IO action that executes an async action and returns Unit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<Unit> Execute(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return IOAsync<Unit>.Of(async () =>
        {
            await action().ConfigureAwait(false);
            return Unit.Default;
        });
    }

    /// <summary>
    /// Delays execution for the specified duration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOAsync<Unit> Delay(TimeSpan delay)
    {
        return IOAsync<Unit>.Of(async () =>
        {
            await Task.Delay(delay).ConfigureAwait(false);
            return Unit.Default;
        });
    }

    /// <summary>
    /// Runs multiple async IO actions in parallel.
    /// </summary>
    public static IOAsync<(T1, T2)> Parallel<T1, T2>(IOAsync<T1> io1, IOAsync<T2> io2)
    {
        return IOAsync<(T1, T2)>.Of(async () =>
        {
            var task1 = io1.RunAsync();
            var task2 = io2.RunAsync();
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
            return (task1.Result, task2.Result);
        });
    }

    /// <summary>
    /// Runs multiple async IO actions in parallel.
    /// </summary>
    public static IOAsync<IReadOnlyList<T>> Parallel<T>(IEnumerable<IOAsync<T>> ios)
    {
        ArgumentNullException.ThrowIfNull(ios);

        var ioList = ios.ToList();
        return IOAsync<IReadOnlyList<T>>.Of(async () =>
        {
            var tasks = ioList.Select(io => io.RunAsync()).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(t => t.Result).ToList();
        });
    }

    /// <summary>
    /// Races multiple async IO actions, returning the result of the first to complete.
    /// </summary>
    public static IOAsync<T> Race<T>(IOAsync<T> io1, IOAsync<T> io2)
    {
        return IOAsync<T>.Of(async () =>
        {
            var task1 = io1.RunAsync();
            var task2 = io2.RunAsync();
            var completed = await Task.WhenAny(task1, task2).ConfigureAwait(false);
            return await completed.ConfigureAwait(false);
        });
    }
}

