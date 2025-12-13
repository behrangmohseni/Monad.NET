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

    private Option(T value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
    }

    /// <summary>
    /// Returns true if the option is a Some value.
    /// </summary>
    public bool IsSome => _isSome;

    /// <summary>
    /// Returns true if the option is a None value.
    /// </summary>
    public bool IsNone => !_isSome;

    /// <summary>
    /// Creates a Some value containing the specified value.
    /// </summary>
    public static Option<T> Some(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create Some with null value. Use None instead.");
        
        return new Option<T>(value, true);
    }

    /// <summary>
    /// Creates a None value.
    /// </summary>
    public static Option<T> None() => new(default!, false);

    /// <summary>
    /// Returns the contained Some value.
    /// </summary>
    /// <param name="message">The panic message if None</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is None</exception>
    public T Expect(string message)
    {
        if (!_isSome)
            throw new InvalidOperationException(message);
        
        return _value!;
    }

    /// <summary>
    /// Returns the contained Some value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is None</exception>
    public T Unwrap()
    {
        return Expect("Called Unwrap on a None value");
    }

    /// <summary>
    /// Returns the contained Some value or a default value.
    /// </summary>
    public T UnwrapOr(T defaultValue)
    {
        return _isSome ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Some value or computes it from a function.
    /// </summary>
    public T UnwrapOrElse(Func<T> defaultFunc)
    {
        if (defaultFunc is null)
            throw new ArgumentNullException(nameof(defaultFunc));
        
        return _isSome ? _value! : defaultFunc();
    }

    /// <summary>
    /// Returns the contained Some value or a default value of type T.
    /// </summary>
    public T? UnwrapOrDefault()
    {
        return _isSome ? _value : default;
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; to Option&lt;U&gt; by applying a function to a contained value.
    /// </summary>
    public Option<U> Map<U>(Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isSome ? Option<U>.Some(mapper(_value!)) : Option<U>.None();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise calls predicate with the wrapped value and returns:
    /// - Some(t) if predicate returns true (where t is the wrapped value)
    /// - None if predicate returns false
    /// </summary>
    public Option<T> Filter(Func<T, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        return _isSome && predicate(_value!) ? this : None();
    }

    /// <summary>
    /// Returns the provided default result (if none), or applies a function to the contained value (if any).
    /// </summary>
    public U MapOr<U>(U defaultValue, Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isSome ? mapper(_value!) : defaultValue;
    }

    /// <summary>
    /// Computes a default function result (if none), or applies a different function to the contained value (if any).
    /// </summary>
    public U MapOrElse<U>(Func<U> defaultFunc, Func<T, U> mapper)
    {
        if (defaultFunc is null)
            throw new ArgumentNullException(nameof(defaultFunc));
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isSome ? mapper(_value!) : defaultFunc();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise calls the function with the wrapped value and returns the result.
    /// Some languages call this operation flatmap.
    /// </summary>
    public Option<U> AndThen<U>(Func<T, Option<U>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
        return _isSome ? binder(_value!) : Option<U>.None();
    }

    /// <summary>
    /// Returns None if the option is None, otherwise returns optionB.
    /// </summary>
    public Option<U> And<U>(Option<U> optionB)
    {
        return _isSome ? optionB : Option<U>.None();
    }

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns optionB.
    /// </summary>
    public Option<T> Or(Option<T> optionB)
    {
        return _isSome ? this : optionB;
    }

    /// <summary>
    /// Returns the option if it contains a value, otherwise calls the function and returns the result.
    /// </summary>
    public Option<T> OrElse(Func<Option<T>> optionFunc)
    {
        if (optionFunc is null)
            throw new ArgumentNullException(nameof(optionFunc));
        
        return _isSome ? this : optionFunc();
    }

    /// <summary>
    /// Returns Some if exactly one of the two options is Some, otherwise returns None.
    /// </summary>
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
    public void Match(Action<T> someAction, Action noneAction)
    {
        if (someAction is null)
            throw new ArgumentNullException(nameof(someAction));
        if (noneAction is null)
            throw new ArgumentNullException(nameof(noneAction));
        
        if (_isSome)
            someAction(_value!);
        else
            noneAction();
    }

    /// <summary>
    /// Pattern matches on the option and returns a result.
    /// </summary>
    public U Match<U>(Func<T, U> someFunc, Func<U> noneFunc)
    {
        if (someFunc is null)
            throw new ArgumentNullException(nameof(someFunc));
        if (noneFunc is null)
            throw new ArgumentNullException(nameof(noneFunc));
        
        return _isSome ? someFunc(_value!) : noneFunc();
    }

    /// <summary>
    /// Converts this Option to a Result, mapping Some(v) to Ok(v) and None to Err(err).
    /// </summary>
    public Result<T, TErr> OkOr<TErr>(TErr err)
    {
        return _isSome ? Result<T, TErr>.Ok(_value!) : Result<T, TErr>.Err(err);
    }

    /// <summary>
    /// Converts this Option to a Result, mapping Some(v) to Ok(v) and None to Err computed from the function.
    /// </summary>
    public Result<T, TErr> OkOrElse<TErr>(Func<TErr> errFunc)
    {
        if (errFunc is null)
            throw new ArgumentNullException(nameof(errFunc));
        
        return _isSome ? Result<T, TErr>.Ok(_value!) : Result<T, TErr>.Err(errFunc());
    }

    /// <inheritdoc />
    public bool Equals(Option<T> other)
    {
        if (_isSome != other._isSome)
            return false;
        
        if (!_isSome)
            return true;
        
        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Option<T> other && Equals(other);
    }

    /// <inheritdoc />
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
    public static bool operator ==(Option<T> left, Option<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Option instances are not equal.
    /// </summary>
    public static bool operator !=(Option<T> left, Option<T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implicit conversion from T to Option&lt;T&gt;.
    /// </summary>
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
    public static Option<T> ToOption<T>(this T? value) where T : struct
    {
        return value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None();
    }

    /// <summary>
    /// Converts a reference type to an Option.
    /// </summary>
    public static Option<T> ToOption<T>(this T? value) where T : class
    {
        return value is not null ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <summary>
    /// Flattens a nested Option.
    /// </summary>
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
    {
        return option.AndThen(inner => inner);
    }

    /// <summary>
    /// Transposes an Option of a Result into a Result of an Option.
    /// </summary>
    public static Result<Option<T>, TErr> Transpose<T, TErr>(this Option<Result<T, TErr>> option)
    {
        return option.Match(
            someFunc: result => result.Match(
                okFunc: value => Result<Option<T>, TErr>.Ok(Option<T>.Some(value)),
                errFunc: err => Result<Option<T>, TErr>.Err(err)
            ),
            noneFunc: () => Result<Option<T>, TErr>.Ok(Option<T>.None())
        );
    }
}

