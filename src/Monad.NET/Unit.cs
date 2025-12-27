using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a unit type - a type with only one value.
/// Used as a replacement for void in functional programming contexts
/// where a return value is required but meaningless.
/// </summary>
/// <remarks>
/// <para>
/// In functional programming, Unit is the type with exactly one value,
/// typically written as (). It's used where traditional imperative
/// programming would use void.
/// </para>
/// <para>
/// Unlike void, Unit can be used as a type parameter, stored in collections,
/// and returned from functions, making it useful for:
/// - Generic code that needs to work with "no meaningful value"
/// - Async operations that complete without a result
/// - Writer monad for side-effect tracking
/// - IO monad for actions without return values
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Use Unit for actions that don't return a value
/// IO&lt;Unit&gt; LogMessage(string msg) =>
///     IO&lt;Unit&gt;.Of(() => { Console.WriteLine(msg); return Unit.Value; });
///
/// // Use Unit.From to wrap void-returning actions
/// var result = Unit.From(() => Console.WriteLine("Hello"));
///
/// // Use in async contexts
/// async Task&lt;Unit&gt; SaveAsync() { await db.SaveAsync(); return Unit.Value; }
/// </code>
/// </example>
[Serializable]
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    /// <summary>
    /// The singleton Unit value.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// The default Unit value (alias for <see cref="Value"/>).
    /// </summary>
    public static readonly Unit Default = default;

    /// <summary>
    /// A completed Task returning Unit. Useful for async methods that don't return a value.
    /// </summary>
    /// <example>
    /// <code>
    /// public Task&lt;Unit&gt; DoSomethingAsync()
    /// {
    ///     // synchronous work
    ///     return Unit.Task;
    /// }
    /// </code>
    /// </example>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    /// <summary>
    /// Executes an action and returns Unit.
    /// Useful for wrapping void-returning methods in a functional context.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>Unit.Value</returns>
    /// <example>
    /// <code>
    /// var result = Unit.From(() => Console.WriteLine("Hello"));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit From(Action action)
    {
        ThrowHelper.ThrowIfNull(action);
        action();
        return Value;
    }

    /// <summary>
    /// Executes an async action and returns Unit.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A task that completes with Unit.Value after the action completes.</returns>
    /// <example>
    /// <code>
    /// var result = await Unit.FromAsync(async () => await SaveToDbAsync());
    /// </code>
    /// </example>
    public static async Task<Unit> FromAsync(Func<Task> action)
    {
        ThrowHelper.ThrowIfNull(action);
        await action().ConfigureAwait(false);
        return Value;
    }

    /// <summary>
    /// Executes an async action with cancellation support and returns Unit.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes with Unit.Value after the action completes.</returns>
    public static async Task<Unit> FromAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        await action(cancellationToken).ConfigureAwait(false);
        return Value;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns the string representation of Unit: "()".
    /// </summary>
    public override string ToString() => "()";

    /// <summary>
    /// Compares this Unit to another. Always returns 0 (equal).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Unit other) => 0;

    /// <summary>
    /// Compares this Unit to another object.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>0 if obj is Unit; otherwise throws.</returns>
    /// <exception cref="ArgumentException">Thrown if obj is not Unit.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is Unit)
            return 0;
        throw new ArgumentException("Object must be of type Unit.", nameof(obj));
    }

    /// <summary>
    /// Determines whether two Unit instances are equal (always true).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Determines whether two Unit instances are not equal (always false).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Unit left, Unit right) => false;

    /// <summary>
    /// Comparison operator. Always returns false since all Units are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Unit left, Unit right) => false;

    /// <summary>
    /// Comparison operator. Always returns true since all Units are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Unit left, Unit right) => true;

    /// <summary>
    /// Comparison operator. Always returns false since all Units are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Unit left, Unit right) => false;

    /// <summary>
    /// Comparison operator. Always returns true since all Units are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Unit left, Unit right) => true;
}

