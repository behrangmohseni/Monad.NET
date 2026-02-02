using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents an optional value. Every Option is either Some and contains a value, or None, and does not.
/// This is inspired by Rust's Option&lt;T&gt; type.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="Option{T}"/> when a value may or may not exist, as an alternative to null.
/// This forces explicit handling of the absent case at compile time.
/// </para>
/// <para>
/// For error handling with typed errors, consider <see cref="Result{T,TError}"/> instead.
/// For validation with multiple errors, use <see cref="Validation{T,TError}"/>.
/// For exception-throwing code, wrap with <see cref="Try{T}"/>.
/// </para>
/// </remarks>
/// <seealso cref="Result{T,TError}"/>
/// <seealso cref="Validation{T,TError}"/>
/// <seealso cref="Try{T}"/>
/// <seealso cref="OptionExtensions"/>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(OptionDebugView<>))]
public readonly struct Option<T> : IEquatable<Option<T>>, IComparable<Option<T>>
{
    private readonly T? _value;
    private readonly bool _isSome;
    private readonly bool _isInitialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isInitialized
        ? (_isSome ? $"Some({_value})" : "None")
        : "Uninitialized";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option(T value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
        _isInitialized = true;
    }

    /// <summary>
    /// Indicates whether the Option was properly initialized via factory methods.
    /// A default-constructed Option (e.g., default(Option&lt;T&gt;)) is not initialized.
    /// Always create Options via <see cref="Some(T)"/> or <see cref="None()"/> factory methods.
    /// </summary>
    public bool IsInitialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isInitialized;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDefault()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowOptionIsDefault();
    }

    /// <summary>
    /// Returns true if the option is a Some value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    public bool IsSome
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _isSome;
        }
    }

    /// <summary>
    /// Returns true if the option is a None value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return !_isSome;
        }
    }

    /// <summary>
    /// Gets the contained value for pattern matching. Returns the value if Some, default otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    /// <example>
    /// <code>
    /// var message = option switch
    /// {
    ///     { IsSome: true, Value: var v } => $"Got: {v}",
    ///     { IsNone: true } => "Nothing",
    ///     _ => "Unknown"
    /// };
    /// </code>
    /// </example>
    public T? Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    /// <summary>
    /// Creates a Some value containing the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some(T value)
    {
        if (value is null)
            ThrowHelper.ThrowCannotCreateSomeWithNull();

        return new Option<T>(value, true);
    }

    /// <summary>
    /// Creates a None value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> None() => new(default!, false);

    /// <summary>
    /// Returns the contained Some value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is None or if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        ThrowIfDefault();
        if (!_isSome)
            ThrowHelper.ThrowOptionIsNone();

        return _value!;
    }

    /// <summary>
    /// Returns the contained Some value or a default value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        ThrowIfDefault();
        return _isSome ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Some value, or throws an <see cref="InvalidOperationException"/> if None.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is None or if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var option = Option&lt;int&gt;.Some(42);
    /// var value = option.GetOrThrow(); // 42
    /// 
    /// var none = Option&lt;int&gt;.None();
    /// none.GetOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow()
    {
        ThrowIfDefault();
        if (!_isSome)
            ThrowHelper.ThrowOptionIsNone();

        return _value!;
    }

    /// <summary>
    /// Tries to get the contained value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if Some; otherwise, the default value.</param>
    /// <returns>True if the Option contains a value; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// if (option.TryGet(out var value))
    /// {
    ///     Console.WriteLine($"Got: {value}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(out T? value)
    {
        ThrowIfDefault();
        value = _value;
        return _isSome;
    }

    /// <summary>
    /// Returns true if the Option is Some and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Option is Some and contains the specified value; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var option = Option&lt;int&gt;.Some(42);
    /// option.Contains(42); // true
    /// option.Contains(0);  // false
    /// Option&lt;int&gt;.None().Contains(42); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        ThrowIfDefault();
        return _isSome && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Option is Some and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Option is Some and the predicate returns true; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var option = Option&lt;int&gt;.Some(42);
    /// option.Exists(x => x > 40); // true
    /// option.Exists(x => x > 50); // false
    /// Option&lt;int&gt;.None().Exists(x => x > 0); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowIfDefault();
        return _isSome && predicate(_value!);
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; to Option&lt;U&gt; by applying a function to a contained value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> Map<U>(Func<T, U> mapper)
    {
        ThrowIfDefault();
        return _isSome ? Option<U>.Some(mapper(_value!)) : Option<U>.None();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise calls predicate with the wrapped value and returns:
    /// - Some(t) if predicate returns true (where t is the wrapped value)
    /// - None if predicate returns false
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Filter(Func<T, bool> predicate)
    {
        ThrowIfDefault();
        return _isSome && predicate(_value!) ? this : None();
    }

    /// <summary>
    /// Returns the provided default result (if none), or applies a function to the contained value (if any).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOr<U>(U defaultValue, Func<T, U> mapper)
    {
        ThrowIfDefault();
        return _isSome ? mapper(_value!) : defaultValue;
    }

    /// <summary>
    /// Computes a default function result (if none), or applies a different function to the contained value (if any).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOrElse<U>(Func<U> defaultFunc, Func<T, U> mapper)
    {
        ThrowIfDefault();
        return _isSome ? mapper(_value!) : defaultFunc();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise calls the function with the wrapped value and returns the result.
    /// This is the monadic bind operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> Bind<U>(Func<T, Option<U>> binder)
    {
        ThrowIfDefault();
        return _isSome ? binder(_value!) : Option<U>.None();
    }

    /// <summary>
    /// Combines this Option with another into a tuple.
    /// Returns None if either Option is None.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Option to combine with.</param>
    /// <returns>An Option containing a tuple of both values, or None.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var name = Option&lt;string&gt;.Some("Alice");
    /// var age = Option&lt;int&gt;.Some(30);
    /// var combined = name.Zip(age); // Some(("Alice", 30))
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<(T, U)> Zip<U>(Option<U> other)
    {
        ThrowIfDefault();
        return _isSome && other.IsSome
            ? Option<(T, U)>.Some((_value!, other.GetValue()))
            : Option<(T, U)>.None();
    }

    /// <summary>
    /// Combines this Option with another using a combiner function.
    /// Returns None if either Option is None.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="other">The other Option to combine with.</param>
    /// <param name="combiner">A function to combine the values.</param>
    /// <returns>An Option containing the combined result, or None.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var firstName = Option&lt;string&gt;.Some("Alice");
    /// var lastName = Option&lt;string&gt;.Some("Smith");
    /// var fullName = firstName.ZipWith(lastName, (f, l) => $"{f} {l}"); // Some("Alice Smith")
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<V> ZipWith<U, V>(Option<U> other, Func<T, U, V> combiner)
    {
        ThrowIfDefault();
        return _isSome && other.IsSome
            ? Option<V>.Some(combiner(_value!, other.GetValue()))
            : Option<V>.None();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise returns optionB.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> And<U>(Option<U> optionB)
    {
        ThrowIfDefault();
        return _isSome ? optionB : Option<U>.None();
    }

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns optionB.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Or(Option<T> optionB)
    {
        ThrowIfDefault();
        return _isSome ? this : optionB;
    }

    /// <summary>
    /// Returns the option if it contains a value, otherwise calls the function and returns the result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> OrElse(Func<Option<T>> optionFunc)
    {
        ThrowIfDefault();
        return _isSome ? this : optionFunc();
    }

    /// <summary>
    /// Returns Some if exactly one of the two options is Some, otherwise returns None.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Xor(Option<T> optionB)
    {
        ThrowIfDefault();
        if (_isSome && !optionB._isSome)
            return this;
        if (!_isSome && optionB._isSome)
            return optionB;

        return None();
    }

    /// <summary>
    /// Executes the provided action if the option contains a value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> someAction, Action noneAction)
    {
        ThrowIfDefault();
        if (_isSome)
            someAction(_value!);
        else
            noneAction();
    }

    /// <summary>
    /// Pattern matches on the option and returns a result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> someFunc, Func<U> noneFunc)
    {
        ThrowIfDefault();
        return _isSome ? someFunc(_value!) : noneFunc();
    }

    /// <summary>
    /// Executes an action if the Option is Some, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the contained value.</param>
    /// <returns>The original Option unchanged.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// option.Tap(x => Console.WriteLine($"Value: {x}"))
    ///       .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Tap(Action<T> action)
    {
        ThrowIfDefault();
        if (_isSome)
            action(_value!);
        return this;
    }

    /// <summary>
    /// Executes an action if the Option is None, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute when None.</param>
    /// <returns>The original Option unchanged.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// option.TapNone(() => Console.WriteLine("No value found"))
    ///       .GetValueOr(defaultValue);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> TapNone(Action action)
    {
        ThrowIfDefault();
        if (!_isSome)
            action();
        return this;
    }

    /// <summary>
    /// Converts this Option to a Result, mapping Some(v) to Ok(v) and None to Err(err).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> OkOr<TError>(TError err)
    {
        ThrowIfDefault();
        return _isSome ? Result<T, TError>.Ok(_value!) : Result<T, TError>.Error(err);
    }

    /// <summary>
    /// Converts this Option to a Result, mapping Some(v) to Ok(v) and None to Err computed from the function.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> OkOrElse<TError>(Func<TError> errFunc)
    {
        ThrowIfDefault();
        return _isSome ? Result<T, TError>.Ok(_value!) : Result<T, TError>.Error(errFunc());
    }

    /// <summary>
    /// Converts the Option to an enumerable sequence.
    /// Returns a sequence containing the value if Some, or an empty sequence if None.
    /// </summary>
    /// <returns>An enumerable containing zero or one element.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var option = Option&lt;int&gt;.Some(42);
    /// foreach (var value in option.AsEnumerable())
    ///     Console.WriteLine(value); // Prints: 42
    ///
    /// // Useful for flattening collections of Options
    /// var options = new[] { Option&lt;int&gt;.Some(1), Option&lt;int&gt;.None(), Option&lt;int&gt;.Some(3) };
    /// var values = options.SelectMany(o => o.AsEnumerable()); // [1, 3]
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> AsEnumerable()
    {
        ThrowIfDefault();
        if (_isSome)
            yield return _value!;
    }

    /// <summary>
    /// Converts the Option to an array.
    /// Returns an array containing the value if Some, or an empty array if None.
    /// </summary>
    /// <returns>An array containing zero or one element.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        ThrowIfDefault();
        return _isSome ? new[] { _value! } : Array.Empty<T>();
    }

    /// <summary>
    /// Converts the Option to a list.
    /// Returns a list containing the value if Some, or an empty list if None.
    /// </summary>
    /// <returns>A list containing zero or one element.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        ThrowIfDefault();
        return _isSome ? [_value!] : [];
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Option<T> other)
    {
        if (_isSome != other._isSome)
            return false;

        if (!_isSome)
            return true;

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Option<T> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return _isSome ? _value?.GetHashCode() ?? 0 : 0;
    }

    /// <summary>
    /// Compares this Option to another Option.
    /// None is considered less than Some. When both are Some, the contained values are compared.
    /// </summary>
    /// <param name="other">The other Option to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    /// <exception cref="InvalidOperationException">Thrown if either Option was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Option<T> other)
    {
        ThrowIfDefault();
        other.ThrowIfDefault();
        if (!_isSome && !other._isSome)
            return 0;
        if (!_isSome)
            return -1;
        if (!other._isSome)
            return 1;
        return Comparer<T>.Default.Compare(_value, other._value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isSome ? $"Some({_value})" : "None";
    }

    /// <summary>
    /// Determines whether two Option instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Option<T> left, Option<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Option instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Option<T> left, Option<T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the Option into its components for pattern matching.
    /// </summary>
    /// <param name="value">The contained value, or default if None.</param>
    /// <param name="isSome">True if the Option contains a value.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Option was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var (value, isSome) = option;
    /// if (isSome)
    ///     Console.WriteLine($"Got: {value}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out bool isSome)
    {
        ThrowIfDefault();
        value = _value;
        isSome = _isSome;
    }
}

/// <summary>
/// Extension methods for Option&lt;T&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class OptionExtensions
{
    #region When/Unless Guards

    /// <summary>
    /// Creates an Option based on a condition. Returns Some containing the factory result if the condition is true,
    /// otherwise returns None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="factory">The factory function to create the value if condition is true.</param>
    /// <returns>Some containing the factory result if condition is true; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var result = OptionExtensions.When(user.IsAdmin, () => new AdminPanel());
    /// // Some(AdminPanel) if user is admin, None otherwise
    /// 
    /// var discount = OptionExtensions.When(order.Total > 100, () => 0.1m);
    /// // Some(0.1m) if order total > 100, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> When<T>(bool condition, Func<T> factory)
    {
        ThrowHelper.ThrowIfNull(factory);
        return condition ? Option<T>.Some(factory()) : Option<T>.None();
    }

    /// <summary>
    /// Creates an Option based on a condition. Returns Some containing the value if the condition is true,
    /// otherwise returns None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The value to wrap if condition is true.</param>
    /// <returns>Some containing the value if condition is true; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var result = OptionExtensions.When(isEnabled, defaultConfig);
    /// // Some(defaultConfig) if isEnabled, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> When<T>(bool condition, T value)
    {
        return condition ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <summary>
    /// Creates an Option based on a negated condition. Returns Some containing the factory result if the condition is false,
    /// otherwise returns None. This is the opposite of <see cref="When{T}(bool, Func{T})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate (negated).</param>
    /// <param name="factory">The factory function to create the value if condition is false.</param>
    /// <returns>Some containing the factory result if condition is false; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var warning = OptionExtensions.Unless(user.HasVerifiedEmail, () => "Please verify your email");
    /// // Some("Please verify...") if email NOT verified, None otherwise
    /// 
    /// var fallback = OptionExtensions.Unless(cache.HasValue, () => LoadFromDatabase());
    /// // Some(dbValue) if cache is empty, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Unless<T>(bool condition, Func<T> factory)
    {
        return When(!condition, factory);
    }

    /// <summary>
    /// Creates an Option based on a negated condition. Returns Some containing the value if the condition is false,
    /// otherwise returns None. This is the opposite of <see cref="When{T}(bool, T)"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate (negated).</param>
    /// <param name="value">The value to wrap if condition is false.</param>
    /// <returns>Some containing the value if condition is false; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var defaultValue = OptionExtensions.Unless(hasCustomValue, standardDefault);
    /// // Some(standardDefault) if NOT hasCustomValue, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Unless<T>(bool condition, T value)
    {
        return When(!condition, value);
    }

    #endregion

    #region DefaultIfNone

    /// <summary>
    /// Returns the Option if it contains a value, otherwise returns an Option containing the default value.
    /// Unlike UnwrapOr which extracts the value, this returns an Option containing the default.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="defaultValue">The default value to use if None.</param>
    /// <returns>The original Option if Some; otherwise Some containing the default value.</returns>
    /// <example>
    /// <code>
    /// var some = Option&lt;int&gt;.Some(42);
    /// var result = some.DefaultIfNone(0); // Some(42)
    /// 
    /// var none = Option&lt;int&gt;.None();
    /// var result2 = none.DefaultIfNone(0); // Some(0)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> DefaultIfNone<T>(this Option<T> option, T defaultValue)
    {
        return option.IsSome ? option : Option<T>.Some(defaultValue);
    }

    /// <summary>
    /// Returns the Option if it contains a value, otherwise returns an Option containing the result of the factory function.
    /// The factory is only called if the Option is None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="defaultFactory">The factory function to create the default value if None.</param>
    /// <returns>The original Option if Some; otherwise Some containing the factory result.</returns>
    /// <example>
    /// <code>
    /// var some = Option&lt;Config&gt;.Some(existingConfig);
    /// var result = some.DefaultIfNone(() => new Config()); // Some(existingConfig)
    /// 
    /// var none = Option&lt;Config&gt;.None();
    /// var result2 = none.DefaultIfNone(() => new Config()); // Some(new Config())
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> DefaultIfNone<T>(this Option<T> option, Func<T> defaultFactory)
    {
        ThrowHelper.ThrowIfNull(defaultFactory);
        return option.IsSome ? option : Option<T>.Some(defaultFactory());
    }

    #endregion

    #region ThrowIfNone

    /// <summary>
    /// Returns the contained value if Some, otherwise throws the specified exception.
    /// This is an alternative to Expect that allows throwing specific exception types.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="exception">The exception to throw if None.</param>
    /// <returns>The contained value if Some.</returns>
    /// <exception cref="Exception">Throws the specified exception if None.</exception>
    /// <example>
    /// <code>
    /// var some = Option&lt;User&gt;.Some(user);
    /// var value = some.ThrowIfNone(new UserNotFoundException()); // returns user
    /// 
    /// var none = Option&lt;User&gt;.None();
    /// none.ThrowIfNone(new UserNotFoundException()); // throws UserNotFoundException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNone<T>(this Option<T> option, Exception exception)
    {
        ThrowHelper.ThrowIfNull(exception);

        if (option.IsNone)
            throw exception;

        return option.GetValue();
    }

    /// <summary>
    /// Returns the contained value if Some, otherwise throws an exception created by the factory.
    /// The factory is only called if the Option is None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="exceptionFactory">The factory function to create the exception if None.</param>
    /// <returns>The contained value if Some.</returns>
    /// <exception cref="Exception">Throws the exception from the factory if None.</exception>
    /// <example>
    /// <code>
    /// var result = FindUser(id).ThrowIfNone(() => new UserNotFoundException(id));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNone<T>(this Option<T> option, Func<Exception> exceptionFactory)
    {
        ThrowHelper.ThrowIfNull(exceptionFactory);

        if (option.IsNone)
            throw exceptionFactory();

        return option.GetValue();
    }

    #endregion

    /// <summary>
    /// Converts a nullable value to an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T>(this T? value) where T : struct
    {
        return value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None();
    }

    /// <summary>
    /// Converts a reference type to an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T>(this T? value) where T : class
    {
        return value is not null ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <summary>
    /// Flattens a nested Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
    {
        return option.Bind(static inner => inner);
    }

    /// <summary>
    /// Transposes an Option of a Result into a Result of an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Option<T>, TError> Transpose<T, TError>(this Option<Result<T, TError>> option)
    {
        return option.Match(
            someFunc: static result => result.Match(
                okFunc: static value => Result<Option<T>, TError>.Ok(Option<T>.Some(value)),
                errFunc: static err => Result<Option<T>, TError>.Error(err)
            ),
            noneFunc: static () => Result<Option<T>, TError>.Ok(Option<T>.None())
        );
    }

    /// <summary>
    /// Attempts to cast the contained value to the specified type.
    /// Returns Some if the Option is Some and the value is of type TTarget; otherwise None.
    /// This is the Option equivalent of LINQ's OfType for single values.
    /// </summary>
    /// <typeparam name="TSource">The source type of the Option.</typeparam>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <returns>Some containing the cast value if successful; otherwise None.</returns>
    /// <example>
    /// <code>
    /// Option&lt;object&gt; objOption = Option&lt;object&gt;.Some("hello");
    /// Option&lt;string&gt; strOption = objOption.OfType&lt;object, string&gt;(); // Some("hello")
    /// Option&lt;int&gt; intOption = objOption.OfType&lt;object, int&gt;();       // None
    /// 
    /// // With base/derived types
    /// Option&lt;Animal&gt; animal = Option&lt;Animal&gt;.Some(new Dog());
    /// Option&lt;Dog&gt; dog = animal.OfType&lt;Animal, Dog&gt;();               // Some(Dog)
    /// Option&lt;Cat&gt; cat = animal.OfType&lt;Animal, Cat&gt;();               // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTarget> OfType<TSource, TTarget>(this Option<TSource> option)
        where TTarget : class
    {
        return option.Bind(value => value is TTarget target
            ? Option<TTarget>.Some(target)
            : Option<TTarget>.None());
    }

    /// <summary>
    /// Attempts to cast the contained value to the specified value type.
    /// Returns Some if the Option is Some and the value is of type TTarget; otherwise None.
    /// </summary>
    /// <typeparam name="TSource">The source type of the Option.</typeparam>
    /// <typeparam name="TTarget">The target value type to cast to.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <returns>Some containing the cast value if successful; otherwise None.</returns>
    /// <example>
    /// <code>
    /// Option&lt;object&gt; objOption = Option&lt;object&gt;.Some(42);
    /// Option&lt;int&gt; intOption = objOption.OfTypeValue&lt;object, int&gt;(); // Some(42)
    /// Option&lt;string&gt; strOption = objOption.OfType&lt;object, string&gt;(); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTarget> OfTypeValue<TSource, TTarget>(this Option<TSource> option)
        where TTarget : struct
    {
        return option.Bind(value => value is TTarget target
            ? Option<TTarget>.Some(target)
            : Option<TTarget>.None());
    }

    /// <summary>
    /// Attempts to cast the contained value to the specified type using a type parameter.
    /// Returns Some if the Option is Some and the value can be cast to TTarget; otherwise None.
    /// Works with both reference types and value types.
    /// </summary>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <param name="option">The source Option containing an object.</param>
    /// <returns>Some containing the cast value if successful; otherwise None.</returns>
    /// <example>
    /// <code>
    /// Option&lt;object&gt; objOption = Option&lt;object&gt;.Some("hello");
    /// Option&lt;string&gt; strOption = objOption.OfType&lt;string&gt;(); // Some("hello")
    /// 
    /// Option&lt;object&gt; numOption = Option&lt;object&gt;.Some(42);
    /// Option&lt;int&gt; intOption = numOption.OfType&lt;int&gt;();        // Some(42)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTarget> OfType<TTarget>(this Option<object> option)
    {
        return option.Bind(value => value is TTarget target
            ? Option<TTarget>.Some(target)
            : Option<TTarget>.None());
    }

    #region String Conversions

    /// <summary>
    /// Converts a string to an Option, returning None if the string is null or empty.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>Some containing the string if not null or empty; otherwise None.</returns>
    /// <example>
    /// <code>
    /// "hello".ToOptionNotEmpty();  // Some("hello")
    /// "".ToOptionNotEmpty();       // None
    /// ((string?)null).ToOptionNotEmpty(); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> ToOptionNotEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value)
            ? Option<string>.None()
            : Option<string>.Some(value);
    }

    /// <summary>
    /// Converts a string to an Option, returning None if the string is null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>Some containing the string if not null, empty, or whitespace; otherwise None.</returns>
    /// <example>
    /// <code>
    /// "hello".ToOptionNotWhiteSpace();  // Some("hello")
    /// "   ".ToOptionNotWhiteSpace();    // None
    /// "".ToOptionNotWhiteSpace();       // None
    /// ((string?)null).ToOptionNotWhiteSpace(); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> ToOptionNotWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Option<string>.None()
            : Option<string>.Some(value);
    }

    /// <summary>
    /// Converts a string to an Option with the string trimmed, returning None if the result is empty.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>Some containing the trimmed string if not empty after trimming; otherwise None.</returns>
    /// <example>
    /// <code>
    /// "  hello  ".ToOptionTrimmed();  // Some("hello")
    /// "   ".ToOptionTrimmed();        // None
    /// "".ToOptionTrimmed();           // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> ToOptionTrimmed(this string? value)
    {
        if (value is null)
            return Option<string>.None();

        var trimmed = value.Trim();
        return trimmed.Length == 0
            ? Option<string>.None()
            : Option<string>.Some(trimmed);
    }

    #endregion

    #region Dictionary/Collection Lookups

    /// <summary>
    /// Attempts to get a value from a dictionary by key.
    /// Returns Some if the key exists; otherwise None.
    /// Works with Dictionary, ImmutableDictionary, and other dictionary types.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some containing the value if the key exists; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, int&gt; { ["a"] = 1, ["b"] = 2 };
    /// dict.GetOption("a"); // Some(1)
    /// dict.GetOption("c"); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TValue> GetOption<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key)
    {
        ThrowHelper.ThrowIfNull(dictionary);

        return dictionary.TryGetValue(key, out var value)
            ? Option<TValue>.Some(value!)
            : Option<TValue>.None();
    }

    /// <summary>
    /// Returns the first element of a sequence, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the first element from.</param>
    /// <returns>Some containing the first element if the sequence is not empty; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.FirstOption();      // Some(1)
    /// Array.Empty&lt;int&gt;().FirstOption();    // None
    /// </code>
    /// </example>
    public static Option<T> FirstOption<T>(this IEnumerable<T> source)
    {
        ThrowHelper.ThrowIfNull(source);

        if (source is IList<T> list)
        {
            return list.Count > 0
                ? Option<T>.Some(list[0])
                : Option<T>.None();
        }

        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext()
            ? Option<T>.Some(enumerator.Current)
            : Option<T>.None();
    }

    /// <summary>
    /// Returns the first element of a sequence that matches the predicate, or None if no match is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to search.</param>
    /// <param name="predicate">The condition to match.</param>
    /// <returns>Some containing the first matching element; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.FirstOption(x => x > 1);  // Some(2)
    /// new[] { 1, 2, 3 }.FirstOption(x => x > 10); // None
    /// </code>
    /// </example>
    public static Option<T> FirstOption<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);

        foreach (var item in source)
        {
            if (predicate(item))
                return Option<T>.Some(item);
        }

        return Option<T>.None();
    }

    /// <summary>
    /// Returns the last element of a sequence, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the last element from.</param>
    /// <returns>Some containing the last element if the sequence is not empty; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.LastOption();      // Some(3)
    /// Array.Empty&lt;int&gt;().LastOption();    // None
    /// </code>
    /// </example>
    public static Option<T> LastOption<T>(this IEnumerable<T> source)
    {
        ThrowHelper.ThrowIfNull(source);

        if (source is IList<T> list)
        {
            return list.Count > 0
                ? Option<T>.Some(list[list.Count - 1])
                : Option<T>.None();
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return Option<T>.None();

        var last = enumerator.Current;
        while (enumerator.MoveNext())
        {
            last = enumerator.Current;
        }

        return Option<T>.Some(last);
    }

    /// <summary>
    /// Returns the single element of a sequence, or None if the sequence is empty or has more than one element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the single element from.</param>
    /// <returns>Some containing the element if exactly one exists; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 42 }.SingleOption();         // Some(42)
    /// new[] { 1, 2 }.SingleOption();       // None (more than one)
    /// Array.Empty&lt;int&gt;().SingleOption();  // None (empty)
    /// </code>
    /// </example>
    public static Option<T> SingleOption<T>(this IEnumerable<T> source)
    {
        ThrowHelper.ThrowIfNull(source);

        if (source is IList<T> list)
        {
            return list.Count == 1
                ? Option<T>.Some(list[0])
                : Option<T>.None();
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return Option<T>.None();

        var single = enumerator.Current;
        if (enumerator.MoveNext())
            return Option<T>.None(); // More than one element

        return Option<T>.Some(single);
    }

    /// <summary>
    /// Returns the element at the specified index, or None if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to index into.</param>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>Some containing the element at the index; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.ElementAtOption(1);  // Some(2)
    /// new[] { 1, 2, 3 }.ElementAtOption(10); // None
    /// new[] { 1, 2, 3 }.ElementAtOption(-1); // None
    /// </code>
    /// </example>
    public static Option<T> ElementAtOption<T>(this IEnumerable<T> source, int index)
    {
        ThrowHelper.ThrowIfNull(source);

        if (index < 0)
            return Option<T>.None();

        if (source is IList<T> list)
        {
            return index < list.Count
                ? Option<T>.Some(list[index])
                : Option<T>.None();
        }

        var currentIndex = 0;
        foreach (var item in source)
        {
            if (currentIndex == index)
                return Option<T>.Some(item);
            currentIndex++;
        }

        return Option<T>.None();
    }

    #endregion
}

/// <summary>
/// Debug view proxy for <see cref="Option{T}"/> to provide a better debugging experience.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
internal sealed class OptionDebugView<T>
{
    private readonly Option<T> _option;

    public OptionDebugView(Option<T> option)
    {
        _option = option;
    }

    public bool IsSome => _option.IsSome;
    public bool IsNone => _option.IsNone;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Value => _option.IsSome ? _option.GetValue() : null;
}
