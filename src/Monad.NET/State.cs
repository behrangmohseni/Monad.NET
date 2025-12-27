using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents the result of running a stateful computation.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="T">The type of the value.</typeparam>
public readonly struct StateResult<TState, T> : IEquatable<StateResult<TState, T>>
{
    /// <summary>
    /// Gets the computed value.
    /// </summary>
    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    /// Gets the resulting state.
    /// </summary>
    public TState State
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    /// Creates a new state result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StateResult(T value, TState state)
    {
        Value = value;
        State = state;
    }

    /// <summary>
    /// Deconstructs the state result into its components.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T value, out TState state)
    {
        value = Value;
        state = State;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(StateResult<TState, T> other)
    {
        return EqualityComparer<T>.Default.Equals(Value, other.Value)
            && EqualityComparer<TState>.Default.Equals(State, other.State);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is StateResult<TState, T> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(Value, State);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"StateResult(Value: {Value}, State: {State})";
    }

    /// <summary>
    /// Determines whether two StateResult instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(StateResult<TState, T> left, StateResult<TState, T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two StateResult instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(StateResult<TState, T> left, StateResult<TState, T> right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Represents a stateful computation that transforms state and produces a value.
/// State monad allows threading state through a computation without mutable state.
/// </summary>
/// <typeparam name="TState">The type of the state being threaded through the computation.</typeparam>
/// <typeparam name="T">The type of the value produced by the computation.</typeparam>
/// <remarks>
/// <para>
/// The State monad encapsulates computations that read and modify state. It provides
/// a functional way to handle stateful computations without using mutable variables.
/// </para>
/// <para>
/// Use State when you need to:
/// - Thread state through a sequence of operations
/// - Avoid mutable state in functional pipelines
/// - Compose stateful computations
/// - Implement interpreters or simulations
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Counter example
/// var increment = State&lt;int, int&gt;.Modify(s => s + 1);
/// var getCount = State&lt;int, int&gt;.Get();
/// 
/// var computation = increment
///     .AndThen(_ => increment)
///     .AndThen(_ => increment)
///     .AndThen(_ => getCount);
/// 
/// var (value, finalState) = computation.Run(0);
/// // value = 3, finalState = 3
/// </code>
/// </example>
public readonly struct State<TState, T>
{
    private readonly Func<TState, StateResult<TState, T>> _run;

    private State(Func<TState, StateResult<TState, T>> run)
    {
        ThrowHelper.ThrowIfNull(run);
        _run = run;
    }

    /// <summary>
    /// Runs the stateful computation with the given initial state.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    /// <returns>A tuple containing the computed value and the final state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StateResult<TState, T> Run(TState initialState)
    {
        return _run(initialState);
    }

    /// <summary>
    /// Evaluates the computation and returns only the value, discarding the final state.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    /// <returns>The computed value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Eval(TState initialState)
    {
        return Run(initialState).Value;
    }

    /// <summary>
    /// Executes the computation and returns only the final state, discarding the value.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    /// <returns>The final state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TState Exec(TState initialState)
    {
        return Run(initialState).State;
    }

    /// <summary>
    /// Creates a State that returns the given value without modifying the state.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A State computation that returns the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, T> Pure(T value)
    {
        return new State<TState, T>(state => new StateResult<TState, T>(value, state));
    }

    /// <summary>
    /// Creates a State that returns the given value without modifying the state.
    /// Alias for <see cref="Pure"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, T> Return(T value) => Pure(value);

    /// <summary>
    /// Gets the current state as the value.
    /// </summary>
    /// <returns>A State computation that returns the current state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, TState> Get()
    {
        return new State<TState, TState>(state => new StateResult<TState, TState>(state, state));
    }

    /// <summary>
    /// Replaces the state with a new value.
    /// </summary>
    /// <param name="newState">The new state.</param>
    /// <returns>A State computation that sets the state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, Unit> Put(TState newState)
    {
        return new State<TState, Unit>(_ => new StateResult<TState, Unit>(Unit.Default, newState));
    }

    /// <summary>
    /// Modifies the state using the given function.
    /// </summary>
    /// <param name="modifier">A function that transforms the state.</param>
    /// <returns>A State computation that modifies the state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, Unit> Modify(Func<TState, TState> modifier)
    {
        ThrowHelper.ThrowIfNull(modifier);

        return new State<TState, Unit>(state => new StateResult<TState, Unit>(Unit.Default, modifier(state)));
    }

    /// <summary>
    /// Gets a value derived from the current state.
    /// </summary>
    /// <param name="selector">A function that extracts a value from the state.</param>
    /// <returns>A State computation that returns the derived value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, U> Gets<U>(Func<TState, U> selector)
    {
        ThrowHelper.ThrowIfNull(selector);

        return new State<TState, U>(state => new StateResult<TState, U>(selector(state), state));
    }

    /// <summary>
    /// Creates a State computation from a function.
    /// </summary>
    /// <param name="run">The function that defines the computation.</param>
    /// <returns>A State computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, T> Of(Func<TState, StateResult<TState, T>> run)
    {
        return new State<TState, T>(run);
    }

    /// <summary>
    /// Creates a State computation from a function that returns a tuple.
    /// </summary>
    /// <param name="run">The function that defines the computation.</param>
    /// <returns>A State computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, T> Of(Func<TState, (T value, TState state)> run)
    {
        ThrowHelper.ThrowIfNull(run);

        return new State<TState, T>(state =>
        {
            var (value, newState) = run(state);
            return new StateResult<TState, T>(value, newState);
        });
    }

    /// <summary>
    /// Transforms the value using the given function.
    /// </summary>
    /// <typeparam name="U">The type of the transformed value.</typeparam>
    /// <param name="mapper">The transformation function.</param>
    /// <returns>A new State computation with the transformed value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, U> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        var run = _run;
        return new State<TState, U>(state =>
        {
            var result = run(state);
            return new StateResult<TState, U>(mapper(result.Value), result.State);
        });
    }

    /// <summary>
    /// Executes an action with the computed value without modifying the state, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the computed value.</param>
    /// <returns>A new State computation that executes the action.</returns>
    /// <example>
    /// <code>
    /// counter.Tap(x => Console.WriteLine($"Current: {x}"))
    ///        .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, T> Tap(Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        var run = _run;
        return new State<TState, T>(state =>
        {
            var result = run(state);
            action(result.Value);
            return result;
        });
    }

    /// <summary>
    /// Executes an action with the current state without modifying the computation, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the current state.</param>
    /// <returns>A new State computation that executes the action.</returns>
    /// <example>
    /// <code>
    /// counter.TapState(s => Console.WriteLine($"State: {s}"))
    ///        .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, T> TapState(Action<TState> action)
    {
        ThrowHelper.ThrowIfNull(action);

        var run = _run;
        return new State<TState, T>(state =>
        {
            var result = run(state);
            action(result.State);
            return result;
        });
    }

    /// <summary>
    /// Chains this computation with another that depends on the result.
    /// </summary>
    /// <typeparam name="U">The type of the new value.</typeparam>
    /// <param name="binder">A function that produces a new State based on the value.</param>
    /// <returns>A new State computation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, U> AndThen<U>(Func<T, State<TState, U>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        var run = _run;
        return new State<TState, U>(state =>
        {
            var result = run(state);
            return binder(result.Value).Run(result.State);
        });
    }

    /// <summary>
    /// Chains this computation with another that depends on the result.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, U> FlatMap<U>(Func<T, State<TState, U>> binder) => AndThen(binder);

    /// <summary>
    /// Chains this computation with another that depends on the result.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, U> Bind<U>(Func<T, State<TState, U>> binder) => AndThen(binder);

    /// <summary>
    /// Applies a function wrapped in a State to this State's value.
    /// </summary>
    /// <typeparam name="U">The type of the result.</typeparam>
    /// <param name="stateFunc">A State containing a function.</param>
    /// <returns>A new State with the function applied.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, U> Apply<U>(State<TState, Func<T, U>> stateFunc)
    {
        var run = _run;
        return new State<TState, U>(state =>
        {
            var funcResult = stateFunc.Run(state);
            var valueResult = run(funcResult.State);
            return new StateResult<TState, U>(funcResult.Value(valueResult.Value), valueResult.State);
        });
    }

    /// <summary>
    /// Combines this State with another, producing a tuple of both values.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other State computation.</param>
    /// <returns>A State that produces a tuple of both values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, (T, U)> Zip<U>(State<TState, U> other)
    {
        return AndThen(a => other.Map(b => (a, b)));
    }

    /// <summary>
    /// Combines this State with another using a combiner function.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="other">The other State computation.</param>
    /// <param name="combiner">A function to combine the two values.</param>
    /// <returns>A State that produces the combined value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, V> ZipWith<U, V>(State<TState, U> other, Func<T, U, V> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        return AndThen(a => other.Map(b => combiner(a, b)));
    }

    /// <summary>
    /// Ignores the value and replaces it with a new one.
    /// </summary>
    /// <typeparam name="U">The type of the new value.</typeparam>
    /// <param name="value">The new value.</param>
    /// <returns>A State that returns the new value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, U> As<U>(U value)
    {
        return Map(_ => value);
    }

    /// <summary>
    /// Replaces the value with Unit.
    /// </summary>
    /// <returns>A State that returns Unit.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State<TState, Unit> Void()
    {
        return As(Unit.Default);
    }
}

/// <summary>
/// Extension methods for State monad.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StateExtensions
{
    /// <summary>
    /// Flattens a nested State computation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, T> Flatten<TState, T>(this State<TState, State<TState, T>> nested)
    {
        return nested.AndThen(static inner => inner);
    }

    /// <summary>
    /// Sequences a collection of State computations into a State of a collection.
    /// </summary>
    public static State<TState, IReadOnlyList<T>> Sequence<TState, T>(
        this IEnumerable<State<TState, T>> states)
    {
        return State<TState, IReadOnlyList<T>>.Of(initialState =>
        {
            var results = new List<T>();
            var currentState = initialState;

            foreach (var state in states)
            {
                var result = state.Run(currentState);
                results.Add(result.Value);
                currentState = result.State;
            }

            return new StateResult<TState, IReadOnlyList<T>>(results, currentState);
        });
    }

    /// <summary>
    /// Traverses a collection, applying a State-producing function to each element.
    /// </summary>
    public static State<TState, IReadOnlyList<U>> Traverse<TState, T, U>(
        this IEnumerable<T> source,
        Func<T, State<TState, U>> func)
    {
        ThrowHelper.ThrowIfNull(func);

        return source.Select(func).Sequence();
    }

    /// <summary>
    /// Repeats a State computation n times, collecting the results.
    /// </summary>
    public static State<TState, IReadOnlyList<T>> Replicate<TState, T>(
        this State<TState, T> state,
        int count)
    {
        if (count < 0)
            ThrowHelper.ThrowArgumentOutOfRange(nameof(count), "Count must be non-negative.");

        return Enumerable.Repeat(state, count).Sequence();
    }

    /// <summary>
    /// Runs a State computation repeatedly while a condition holds.
    /// </summary>
    public static State<TState, IReadOnlyList<T>> WhileM<TState, T>(
        this State<TState, T> body,
        Func<TState, bool> condition)
    {
        ThrowHelper.ThrowIfNull(condition);

        return State<TState, IReadOnlyList<T>>.Of(initialState =>
        {
            var results = new List<T>();
            var currentState = initialState;

            while (condition(currentState))
            {
                var result = body.Run(currentState);
                results.Add(result.Value);
                currentState = result.State;
            }

            return new StateResult<TState, IReadOnlyList<T>>(results, currentState);
        });
    }

    /// <summary>
    /// LINQ Select support for State monad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, U> Select<TState, T, U>(
        this State<TState, T> state,
        Func<T, U> selector)
    {
        return state.Map(selector);
    }

    /// <summary>
    /// LINQ SelectMany support for State monad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, U> SelectMany<TState, T, U>(
        this State<TState, T> state,
        Func<T, State<TState, U>> selector)
    {
        return state.AndThen(selector);
    }

    /// <summary>
    /// LINQ SelectMany support with result selector for State monad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static State<TState, V> SelectMany<TState, T, U, V>(
        this State<TState, T> state,
        Func<T, State<TState, U>> selector,
        Func<T, U, V> resultSelector)
    {
        ThrowHelper.ThrowIfNull(resultSelector);

        return state.AndThen(a => selector(a).Map(b => resultSelector(a, b)));
    }
}

