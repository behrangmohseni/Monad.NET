using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents an optional value. Every Option is either Some and contains a value, or None, and does not.
/// This is inspired by Rust's Option&lt;T&gt; type.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value;
    private readonly bool _isSome;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option(T value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
    }

    /// <summary>
    /// Returns true if the option is a Some value.
    /// </summary>
    public bool IsSome
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSome;
    }

    /// <summary>
    /// Returns true if the option is a None value.
    /// </summary>
    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isSome;
    }

    /// <summary>
    /// Creates a Some value containing the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some(T value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Some with null value. Use None instead.");

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
    /// <param name="message">The panic message if None</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is None</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Expect(string message)
    {
        if (!_isSome)
            ThrowHelper.ThrowInvalidOperation(message);

        return _value!;
    }

    /// <summary>
    /// Returns the contained Some value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is None</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap()
    {
        if (!_isSome)
            ThrowHelper.ThrowInvalidOperation("Cannot unwrap None value.");

        return _value!;
    }

    /// <summary>
    /// Returns the contained Some value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T UnwrapOr(T defaultValue)
    {
        return _isSome ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Some value or computes it from a function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T UnwrapOrElse(Func<T> defaultFunc)
    {
        return _isSome ? _value! : defaultFunc();
    }

    /// <summary>
    /// Returns the contained Some value or a default value of type T.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? UnwrapOrDefault()
    {
        return _isSome ? _value : default;
    }

    /// <summary>
    /// Tries to get the contained value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if Some; otherwise, the default value.</param>
    /// <returns>True if the Option contains a value; otherwise, false.</returns>
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
        value = _value;
        return _isSome;
    }

    /// <summary>
    /// Returns true if the Option is Some and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Option is Some and contains the specified value; otherwise, false.</returns>
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
        return _isSome && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Option is Some and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Option is Some and the predicate returns true; otherwise, false.</returns>
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
        ArgumentNullException.ThrowIfNull(predicate);
        return _isSome && predicate(_value!);
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; to Option&lt;U&gt; by applying a function to a contained value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> Map<U>(Func<T, U> mapper)
    {
        return _isSome ? Option<U>.Some(mapper(_value!)) : Option<U>.None();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise calls predicate with the wrapped value and returns:
    /// - Some(t) if predicate returns true (where t is the wrapped value)
    /// - None if predicate returns false
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Filter(Func<T, bool> predicate)
    {
        return _isSome && predicate(_value!) ? this : None();
    }

    /// <summary>
    /// Returns the provided default result (if none), or applies a function to the contained value (if any).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOr<U>(U defaultValue, Func<T, U> mapper)
    {
        return _isSome ? mapper(_value!) : defaultValue;
    }

    /// <summary>
    /// Computes a default function result (if none), or applies a different function to the contained value (if any).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOrElse<U>(Func<U> defaultFunc, Func<T, U> mapper)
    {
        return _isSome ? mapper(_value!) : defaultFunc();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise calls the function with the wrapped value and returns the result.
    /// Some languages call this operation flatmap.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> AndThen<U>(Func<T, Option<U>> binder)
    {
        return _isSome ? binder(_value!) : Option<U>.None();
    }

    /// <summary>
    /// Combines this Option with another into a tuple.
    /// Returns None if either Option is None.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Option to combine with.</param>
    /// <returns>An Option containing a tuple of both values, or None.</returns>
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
        return _isSome && other.IsSome
            ? Option<(T, U)>.Some((_value!, other.Unwrap()))
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
        return _isSome && other.IsSome
            ? Option<V>.Some(combiner(_value!, other.Unwrap()))
            : Option<V>.None();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise returns optionB.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> And<U>(Option<U> optionB)
    {
        return _isSome ? optionB : Option<U>.None();
    }

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns optionB.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Or(Option<T> optionB)
    {
        return _isSome ? this : optionB;
    }

    /// <summary>
    /// Returns the option if it contains a value, otherwise calls the function and returns the result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> OrElse(Func<Option<T>> optionFunc)
    {
        return _isSome ? this : optionFunc();
    }

    /// <summary>
    /// Returns Some if exactly one of the two options is Some, otherwise returns None.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Xor(Option<T> optionB)
    {
        if (_isSome && !optionB._isSome)
            return this;
        if (!_isSome && optionB._isSome)
            return optionB;

        return None();
    }

    /// <summary>
    /// Executes the provided action if the option contains a value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> someAction, Action noneAction)
    {
        if (_isSome)
            someAction(_value!);
        else
            noneAction();
    }

    /// <summary>
    /// Pattern matches on the option and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> someFunc, Func<U> noneFunc)
    {
        return _isSome ? someFunc(_value!) : noneFunc();
    }

    /// <summary>
    /// Executes an action if the Option is Some, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the contained value.</param>
    /// <returns>The original Option unchanged.</returns>
    /// <example>
    /// <code>
    /// option.Tap(x => Console.WriteLine($"Value: {x}"))
    ///       .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Tap(Action<T> action)
    {
        if (_isSome)
            action(_value!);
        return this;
    }

    /// <summary>
    /// Executes an action if the Option is None, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute when None.</param>
    /// <returns>The original Option unchanged.</returns>
    /// <example>
    /// <code>
    /// option.TapNone(() => Console.WriteLine("No value found"))
    ///       .UnwrapOr(defaultValue);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> TapNone(Action action)
    {
        if (!_isSome)
            action();
        return this;
    }

    /// <summary>
    /// Converts this Option to a Result, mapping Some(v) to Ok(v) and None to Err(err).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> OkOr<TErr>(TErr err)
    {
        return _isSome ? Result<T, TErr>.Ok(_value!) : Result<T, TErr>.Err(err);
    }

    /// <summary>
    /// Converts this Option to a Result, mapping Some(v) to Ok(v) and None to Err computed from the function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> OkOrElse<TErr>(Func<TErr> errFunc)
    {
        return _isSome ? Result<T, TErr>.Ok(_value!) : Result<T, TErr>.Err(errFunc());
    }

    /// <summary>
    /// Converts the Option to an enumerable sequence.
    /// Returns a sequence containing the value if Some, or an empty sequence if None.
    /// </summary>
    /// <returns>An enumerable containing zero or one element.</returns>
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
        if (_isSome)
            yield return _value!;
    }

    /// <summary>
    /// Converts the Option to an array.
    /// Returns an array containing the value if Some, or an empty array if None.
    /// </summary>
    /// <returns>An array containing zero or one element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        return _isSome ? new[] { _value! } : Array.Empty<T>();
    }

    /// <summary>
    /// Converts the Option to a list.
    /// Returns a list containing the value if Some, or an empty list if None.
    /// </summary>
    /// <returns>A list containing zero or one element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        return _isSome ? new List<T> { _value! } : new List<T>();
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
        value = _value;
        isSome = _isSome;
    }

    /// <summary>
    /// Implicit conversion from T to Option&lt;T&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Option<T>(T value)
    {
        return value is null ? None() : Some(value);
    }
}

/// <summary>
/// Extension methods for Option&lt;T&gt;.
/// </summary>
public static class OptionExtensions
{
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
        return option.AndThen(static inner => inner);
    }

    /// <summary>
    /// Transposes an Option of a Result into a Result of an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Option<T>, TErr> Transpose<T, TErr>(this Option<Result<T, TErr>> option)
    {
        return option.Match(
            someFunc: static result => result.Match(
                okFunc: static value => Result<Option<T>, TErr>.Ok(Option<T>.Some(value)),
                errFunc: static err => Result<Option<T>, TErr>.Err(err)
            ),
            noneFunc: static () => Result<Option<T>, TErr>.Ok(Option<T>.None())
        );
    }
}

/// <summary>
/// Helper class for throwing exceptions without inlining the throw site.
/// This keeps hot paths small and improves JIT optimization.
/// </summary>
internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperation(string message)
    {
        throw new InvalidOperationException(message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentNull(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentNull(string paramName, string message)
    {
        throw new ArgumentNullException(paramName, message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentOutOfRange(string paramName, string message)
    {
        throw new ArgumentOutOfRangeException(paramName, message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgument(string paramName, string message)
    {
        throw new ArgumentException(message, paramName);
    }
}
