using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a validation result that can accumulate multiple errors.
/// Unlike Result which short-circuits on the first error, Validation collects all errors.
/// This is an Applicative Functor, perfect for form validation and business rule checking.
/// </summary>
/// <typeparam name="T">The type of the valid value</typeparam>
/// <typeparam name="TErr">The type of the error</typeparam>
public readonly struct Validation<T, TErr> : IEquatable<Validation<T, TErr>>
{
    private readonly T? _value;
    private readonly IReadOnlyList<TErr>? _errors;
    private readonly bool _isValid;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Validation(T value, IReadOnlyList<TErr> errors, bool isValid)
    {
        _value = value;
        _errors = errors;
        _isValid = isValid;
    }

    /// <summary>
    /// Returns true if the validation is valid (no errors).
    /// </summary>
    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isValid;
    }

    /// <summary>
    /// Returns true if the validation is invalid (has errors).
    /// </summary>
    public bool IsInvalid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isValid;
    }

    /// <summary>
    /// Creates a valid validation with the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> Valid(T value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Valid with null value.");

        return new Validation<T, TErr>(value, Array.Empty<TErr>(), true);
    }

    /// <summary>
    /// Creates an invalid validation with a single error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> Invalid(TErr error)
    {
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Cannot create Invalid with null error.");

        return new Validation<T, TErr>(default!, new[] { error }, false);
    }

    /// <summary>
    /// Creates an invalid validation with multiple errors.
    /// </summary>
    public static Validation<T, TErr> Invalid(IEnumerable<TErr> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.ToList();
        if (errorList.Count == 0)
            ThrowHelper.ThrowArgument(nameof(errors), "Must provide at least one error.");

        return new Validation<T, TErr>(default!, errorList, false);
    }

    /// <summary>
    /// Returns the valid value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap()
    {
        if (!_isValid)
            ThrowHelper.ThrowInvalidOperation($"Cannot unwrap Invalid validation. Errors: {string.Join(", ", _errors!)}");

        return _value!;
    }

    /// <summary>
    /// Returns the errors.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyList<TErr> UnwrapErrors()
    {
        if (_isValid)
            ThrowHelper.ThrowInvalidOperation("Cannot unwrap errors on Valid validation.");

        return _errors!;
    }

    /// <summary>
    /// Returns the valid value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T UnwrapOr(T defaultValue)
    {
        return _isValid ? _value! : defaultValue;
    }

    /// <summary>
    /// Tries to get the contained valid value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the valid value if present; otherwise, the default value.</param>
    /// <returns>True if the Validation is valid; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (validation.TryGet(out var value))
    /// {
    ///     Console.WriteLine($"Valid: {value}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(out T? value)
    {
        value = _value;
        return _isValid;
    }

    /// <summary>
    /// Tries to get the contained errors using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="errors">When this method returns, contains the errors if invalid; otherwise, an empty list.</param>
    /// <returns>True if the Validation is invalid; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (validation.TryGetErrors(out var errors))
    /// {
    ///     foreach (var error in errors)
    ///         Console.WriteLine($"Error: {error}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetErrors(out IReadOnlyList<TErr> errors)
    {
        errors = _errors ?? Array.Empty<TErr>();
        return !_isValid;
    }

    /// <summary>
    /// Maps the valid value if it exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TErr> Map<U>(Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return _isValid
            ? Validation<U, TErr>.Valid(mapper(_value!))
            : Validation<U, TErr>.Invalid(_errors!);
    }

    /// <summary>
    /// Maps the errors if they exist.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, F> MapErrors<F>(Func<TErr, F> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return _isValid
            ? Validation<T, F>.Valid(_value!)
            : Validation<T, F>.Invalid(_errors!.Select(mapper).ToList());
    }

    /// <summary>
    /// Combines two validations using applicative functor semantics.
    /// If both are valid, applies the function. If either/both are invalid, accumulates ALL errors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TErr> Apply<TIntermediate, U>(
        Validation<TIntermediate, TErr> other,
        Func<T, TIntermediate, U> combiner)
    {
        ArgumentNullException.ThrowIfNull(combiner);

        if (_isValid && other.IsValid)
            return Validation<U, TErr>.Valid(combiner(_value!, other._value!));

        if (!_isValid && !other.IsValid)
        {
            var allErrors = _errors!.Concat(other._errors!).ToList();
            return Validation<U, TErr>.Invalid(allErrors);
        }

        return _isValid
            ? Validation<U, TErr>.Invalid(other._errors!)
            : Validation<U, TErr>.Invalid(_errors!);
    }

    /// <summary>
    /// Combines this Validation with another into a tuple.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Validation to combine with.</param>
    /// <returns>A Validation containing a tuple of both values, or accumulated errors.</returns>
    /// <example>
    /// <code>
    /// var nameValidation = ValidateName(name);   // Validation&lt;string, Error&gt;
    /// var ageValidation = ValidateAge(age);      // Validation&lt;int, Error&gt;
    /// var combined = nameValidation.Zip(ageValidation); // Validation&lt;(string, int), Error&gt;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<(T, U), TErr> Zip<U>(Validation<U, TErr> other)
    {
        if (_isValid && other.IsValid)
            return Validation<(T, U), TErr>.Valid((_value!, other.Unwrap()));

        if (!_isValid && !other.IsValid)
        {
            var allErrors = _errors!.Concat(other.UnwrapErrors()).ToList();
            return Validation<(T, U), TErr>.Invalid(allErrors);
        }

        return _isValid
            ? Validation<(T, U), TErr>.Invalid(other.UnwrapErrors())
            : Validation<(T, U), TErr>.Invalid(_errors!);
    }

    /// <summary>
    /// Combines this Validation with another using a combiner function.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="other">The other Validation to combine with.</param>
    /// <param name="combiner">A function to combine the values.</param>
    /// <returns>A Validation containing the combined result, or accumulated errors.</returns>
    /// <example>
    /// <code>
    /// var nameValidation = ValidateName(name);
    /// var ageValidation = ValidateAge(age);
    /// var person = nameValidation.ZipWith(ageValidation, (n, a) => new Person(n, a));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<V, TErr> ZipWith<U, V>(Validation<U, TErr> other, Func<T, U, V> combiner)
    {
        ArgumentNullException.ThrowIfNull(combiner);

        if (_isValid && other.IsValid)
            return Validation<V, TErr>.Valid(combiner(_value!, other.Unwrap()));

        if (!_isValid && !other.IsValid)
        {
            var allErrors = _errors!.Concat(other.UnwrapErrors()).ToList();
            return Validation<V, TErr>.Invalid(allErrors);
        }

        return _isValid
            ? Validation<V, TErr>.Invalid(other.UnwrapErrors())
            : Validation<V, TErr>.Invalid(_errors!);
    }

    /// <summary>
    /// Combines this validation with another, accumulating errors from both if invalid.
    /// This is useful for running multiple independent validations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TErr> And(Validation<T, TErr> other)
    {
        if (_isValid && other.IsValid)
            return other; // Return the last valid value

        if (!_isValid && !other.IsValid)
        {
            var allErrors = _errors!.Concat(other._errors!).ToList();
            return Validation<T, TErr>.Invalid(allErrors);
        }

        return _isValid ? other : this;
    }

    /// <summary>
    /// Chains a validation operation. If this is invalid, returns this.
    /// If this is valid, applies the function (which may return invalid).
    /// Note: This does NOT accumulate errors like And() - it short-circuits like Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TErr> AndThen<U>(Func<T, Validation<U, TErr>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return _isValid ? binder(_value!) : Validation<U, TErr>.Invalid(_errors!);
    }

    /// <summary>
    /// Pattern matches on the validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> validAction, Action<IReadOnlyList<TErr>> invalidAction)
    {
        ArgumentNullException.ThrowIfNull(validAction);
        ArgumentNullException.ThrowIfNull(invalidAction);

        if (_isValid)
            validAction(_value!);
        else
            invalidAction(_errors!);
    }

    /// <summary>
    /// Pattern matches on the validation and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> validFunc, Func<IReadOnlyList<TErr>, U> invalidFunc)
    {
        ArgumentNullException.ThrowIfNull(validFunc);
        ArgumentNullException.ThrowIfNull(invalidFunc);

        return _isValid ? validFunc(_value!) : invalidFunc(_errors!);
    }

    /// <summary>
    /// Converts this Validation to a Result.
    /// If invalid with multiple errors, only the first error is used.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> ToResult()
    {
        return _isValid
            ? Result<T, TErr>.Ok(_value!)
            : Result<T, TErr>.Err(_errors![0]);
    }

    /// <summary>
    /// Converts this Validation to a Result with a combined error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> ToResult(Func<IReadOnlyList<TErr>, TErr> combineErrors)
    {
        ArgumentNullException.ThrowIfNull(combineErrors);

        return _isValid
            ? Result<T, TErr>.Ok(_value!)
            : Result<T, TErr>.Err(combineErrors(_errors!));
    }

    /// <summary>
    /// Converts this Validation to an Option.
    /// Discards error information if invalid.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        return _isValid ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Validation<T, TErr> other)
    {
        if (_isValid != other._isValid)
            return false;

        if (_isValid)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        if (_errors!.Count != other._errors!.Count)
            return false;

        return _errors.SequenceEqual(other._errors);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Validation<T, TErr> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        if (_isValid)
            return HashCode.Combine(_isValid, _value);

        var hash = new HashCode();
        hash.Add(_isValid);
        foreach (var error in _errors!)
            hash.Add(error);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isValid
            ? $"Valid({_value})"
            : $"Invalid([{string.Join(", ", _errors!)}])";
    }

    /// <summary>
    /// Determines whether two Validation instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Validation<T, TErr> left, Validation<T, TErr> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Validation instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Validation<T, TErr> left, Validation<T, TErr> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implicit conversion from T to Validation&lt;T, TErr&gt; (Valid).
    /// Allows: Validation&lt;int, string&gt; v = 42;
    /// </summary>
    /// <param name="value">The valid value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Validation<T, TErr>(T value)
    {
        return Valid(value);
    }

    /// <summary>
    /// Deconstructs the Validation into its components for pattern matching.
    /// </summary>
    /// <param name="value">The valid value, or default if Invalid.</param>
    /// <param name="isValid">True if the Validation is valid.</param>
    /// <example>
    /// <code>
    /// var (value, isValid) = validation;
    /// if (isValid)
    ///     Console.WriteLine($"Valid: {value}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out bool isValid)
    {
        value = _value;
        isValid = _isValid;
    }

    /// <summary>
    /// Deconstructs the Validation into all its components for pattern matching.
    /// </summary>
    /// <param name="value">The valid value, or default if Invalid.</param>
    /// <param name="errors">The errors, or empty list if Valid.</param>
    /// <param name="isValid">True if the Validation is valid.</param>
    /// <example>
    /// <code>
    /// var (value, errors, isValid) = validation;
    /// if (!isValid)
    ///     foreach (var error in errors)
    ///         Console.WriteLine($"Error: {error}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out IReadOnlyList<TErr> errors, out bool isValid)
    {
        value = _value;
        errors = _errors ?? Array.Empty<TErr>();
        isValid = _isValid;
    }
}

/// <summary>
/// Extension methods for Validation&lt;T, E&gt;.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Combines multiple validations into one, accumulating all errors.
    /// Returns Valid only if ALL validations are valid.
    /// </summary>
    public static Validation<T, TErr> Combine<T, TErr>(
        this IEnumerable<Validation<T, TErr>> validations)
    {
        ArgumentNullException.ThrowIfNull(validations);

        var validationList = validations.ToList();
        if (validationList.Count == 0)
            ThrowHelper.ThrowArgument(nameof(validations), "Must provide at least one validation.");

        var allErrors = new List<TErr>();
        T? lastValue = default;

        foreach (var validation in validationList)
        {
            if (validation.IsValid)
                lastValue = validation.Unwrap();
            else
                allErrors.AddRange(validation.UnwrapErrors());
        }

        return allErrors.Count == 0
            ? Validation<T, TErr>.Valid(lastValue!)
            : Validation<T, TErr>.Invalid(allErrors);
    }

    /// <summary>
    /// Executes an action if the validation is valid, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> Tap<T, TErr>(
        this Validation<T, TErr> validation,
        Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (validation.IsValid)
            action(validation.Unwrap());

        return validation;
    }

    /// <summary>
    /// Executes an action if the validation is invalid, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> TapErrors<T, TErr>(
        this Validation<T, TErr> validation,
        Action<IReadOnlyList<TErr>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (validation.IsInvalid)
            action(validation.UnwrapErrors());

        return validation;
    }

    /// <summary>
    /// Executes an action if the validation is invalid, allowing method chaining.
    /// Alias for <see cref="TapErrors{T, TErr}"/> with a more concise name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> TapInvalid<T, TErr>(
        this Validation<T, TErr> validation,
        Action<IReadOnlyList<TErr>> action) => validation.TapErrors(action);

    /// <summary>
    /// Converts a Result to a Validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> ToValidation<T, TErr>(this Result<T, TErr> result)
    {
        return result.Match(
            okFunc: static value => Validation<T, TErr>.Valid(value),
            errFunc: static err => Validation<T, TErr>.Invalid(err)
        );
    }
}
