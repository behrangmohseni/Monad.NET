using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a validation result that can accumulate multiple errors.
/// Unlike Result which short-circuits on the first error, Validation collects all errors.
/// This is an Applicative Functor, perfect for form validation and business rule checking.
/// </summary>
/// <typeparam name="T">The type of the valid value</typeparam>
/// <typeparam name="TErr">The type of the error</typeparam>
[Serializable]
public readonly struct Validation<T, TErr> : IEquatable<Validation<T, TErr>>, IComparable<Validation<T, TErr>>, IComparable
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
        ThrowHelper.ThrowIfNull(errors);

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
    /// Returns the valid value with a custom error message if invalid.
    /// Similar to Rust's expect() method.
    /// </summary>
    /// <param name="message">The error message if invalid</param>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid</exception>
    /// <example>
    /// <code>
    /// var valid = Validation&lt;int, string&gt;.Valid(42);
    /// var value = valid.Expect("Expected a value"); // 42
    /// 
    /// var invalid = Validation&lt;int, string&gt;.Invalid("error");
    /// invalid.Expect("Must be valid"); // throws with "Must be valid: error"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Expect(string message)
    {
        if (!_isValid)
            ThrowHelper.ThrowInvalidOperation($"{message}: {string.Join(", ", _errors!)}");

        return _value!;
    }

    /// <summary>
    /// Returns the errors with a custom error message if valid.
    /// Similar to Rust's expect_err() method.
    /// </summary>
    /// <param name="message">The error message if valid</param>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid</exception>
    /// <example>
    /// <code>
    /// var invalid = Validation&lt;int, string&gt;.Invalid("error");
    /// var errors = invalid.ExpectErrors("Expected errors"); // ["error"]
    /// 
    /// var valid = Validation&lt;int, string&gt;.Valid(42);
    /// valid.ExpectErrors("Should be invalid"); // throws with "Should be invalid: 42"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyList<TErr> ExpectErrors(string message)
    {
        if (_isValid)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_value}");

        return _errors!;
    }

    /// <summary>
    /// Returns the valid value, or throws an <see cref="InvalidOperationException"/> if invalid.
    /// This is an alias for <see cref="Unwrap"/> with more explicit C# naming.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid</exception>
    /// <example>
    /// <code>
    /// var valid = Validation&lt;int, string&gt;.Valid(42);
    /// var value = valid.GetOrThrow(); // 42
    /// 
    /// var invalid = Validation&lt;int, string&gt;.Invalid("error");
    /// invalid.GetOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow()
    {
        if (!_isValid)
            ThrowHelper.ThrowInvalidOperation($"Validation is Invalid. Cannot get value. Errors: {string.Join(", ", _errors!)}");

        return _value!;
    }

    /// <summary>
    /// Returns the valid value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if invalid.
    /// This is an alias for <see cref="Expect"/> with more explicit C# naming.
    /// </summary>
    /// <param name="message">The exception message if invalid</param>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid</exception>
    /// <example>
    /// <code>
    /// var valid = Validation&lt;int, string&gt;.Valid(42);
    /// var value = valid.GetOrThrow("Expected success"); // 42
    /// 
    /// var invalid = Validation&lt;int, string&gt;.Invalid("error");
    /// invalid.GetOrThrow("Must be valid"); // throws with "Must be valid: error"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow(string message)
    {
        if (!_isValid)
            ThrowHelper.ThrowInvalidOperation($"{message}: {string.Join(", ", _errors!)}");

        return _value!;
    }

    /// <summary>
    /// Returns the errors, or throws an <see cref="InvalidOperationException"/> if valid.
    /// This is an alias for <see cref="UnwrapErrors"/> with more explicit C# naming.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid</exception>
    /// <example>
    /// <code>
    /// var invalid = Validation&lt;int, string&gt;.Invalid("error");
    /// var errors = invalid.GetErrorsOrThrow(); // ["error"]
    /// 
    /// var valid = Validation&lt;int, string&gt;.Valid(42);
    /// valid.GetErrorsOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyList<TErr> GetErrorsOrThrow()
    {
        if (_isValid)
            ThrowHelper.ThrowInvalidOperation($"Validation is Valid. Cannot get errors. Value: {_value}");

        return _errors!;
    }

    /// <summary>
    /// Returns the errors, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if valid.
    /// This is an alias for <see cref="ExpectErrors"/> with more explicit C# naming.
    /// </summary>
    /// <param name="message">The exception message if valid</param>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid</exception>
    /// <example>
    /// <code>
    /// var invalid = Validation&lt;int, string&gt;.Invalid("error");
    /// var errors = invalid.GetErrorsOrThrow("Expected errors"); // ["error"]
    /// 
    /// var valid = Validation&lt;int, string&gt;.Valid(42);
    /// valid.GetErrorsOrThrow("Should be invalid"); // throws with "Should be invalid: 42"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyList<TErr> GetErrorsOrThrow(string message)
    {
        if (_isValid)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_value}");

        return _errors!;
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
    /// Returns true if the Validation is Valid and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Validation is Valid and contains the specified value; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;int, string&gt;.Valid(42);
    /// validation.Contains(42); // true
    /// validation.Contains(0);  // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        return _isValid && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Validation is Valid and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Validation is Valid and the predicate returns true; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;int, string&gt;.Valid(42);
    /// validation.Exists(x => x > 40); // true
    /// validation.Exists(x => x > 50); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isValid && predicate(_value!);
    }

    /// <summary>
    /// Maps the valid value if it exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TErr> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

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
        ThrowHelper.ThrowIfNull(mapper);

        return _isValid
            ? Validation<T, F>.Valid(_value!)
            : Validation<T, F>.Invalid(_errors!.Select(mapper).ToList());
    }

    /// <summary>
    /// Maps both the valid value and errors.
    /// </summary>
    /// <typeparam name="U">The new valid value type.</typeparam>
    /// <typeparam name="F">The new error type.</typeparam>
    /// <param name="valueMapper">Function to transform the value if valid.</param>
    /// <param name="errorMapper">Function to transform each error if invalid.</param>
    /// <returns>A new Validation with transformed value or errors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, F> BiMap<U, F>(Func<T, U> valueMapper, Func<TErr, F> errorMapper)
    {
        ThrowHelper.ThrowIfNull(valueMapper);
        ThrowHelper.ThrowIfNull(errorMapper);

        return _isValid
            ? Validation<U, F>.Valid(valueMapper(_value!))
            : Validation<U, F>.Invalid(_errors!.Select(errorMapper).ToList());
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
        ThrowHelper.ThrowIfNull(combiner);

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
        ThrowHelper.ThrowIfNull(combiner);

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
        ThrowHelper.ThrowIfNull(binder);

        return _isValid ? binder(_value!) : Validation<U, TErr>.Invalid(_errors!);
    }

    /// <summary>
    /// Chains validation operations.
    /// Note: Unlike AndThen, this does NOT accumulate errors - it short-circuits on first error.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TErr> FlatMap<U>(Func<T, Validation<U, TErr>> binder) => AndThen(binder);

    /// <summary>
    /// Chains validation operations.
    /// Note: Unlike AndThen, this does NOT accumulate errors - it short-circuits on first error.
    /// Alias for <see cref="AndThen{U}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TErr> Bind<U>(Func<T, Validation<U, TErr>> binder) => AndThen(binder);

    /// <summary>
    /// Validates the contained value against a predicate. If the validation is already invalid,
    /// returns this unchanged. If the predicate returns false, returns an Invalid validation with the specified error.
    /// This is useful for adding additional validation rules to an already valid value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <param name="error">The error to return if the predicate fails.</param>
    /// <returns>This validation if valid and predicate passes; Invalid with error if predicate fails; or this if already invalid.</returns>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;int, string&gt;.Valid(18)
    ///     .Ensure(x =&gt; x &gt;= 18, "Must be at least 18")
    ///     .Ensure(x =&gt; x &lt;= 120, "Must be at most 120");
    /// // Valid(18)
    /// 
    /// var invalid = Validation&lt;int, string&gt;.Valid(15)
    ///     .Ensure(x =&gt; x &gt;= 18, "Must be at least 18");
    /// // Invalid(["Must be at least 18"])
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TErr> Ensure(Func<T, bool> predicate, TErr error)
    {
        ThrowHelper.ThrowIfNull(predicate);
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Error cannot be null.");

        if (!_isValid)
            return this;

        return predicate(_value!) ? this : Validation<T, TErr>.Invalid(error);
    }

    /// <summary>
    /// Validates the contained value against a predicate with a lazy error factory.
    /// If the validation is already invalid, returns this unchanged (error factory is not called).
    /// If the predicate returns false, returns an Invalid validation with the error from the factory.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <param name="errorFactory">The factory function to create the error if the predicate fails.</param>
    /// <returns>This validation if valid and predicate passes; Invalid with error if predicate fails; or this if already invalid.</returns>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;User, string&gt;.Valid(user)
    ///     .Ensure(u =&gt; u.Email.Contains("@"), () =&gt; $"Invalid email: {user.Email}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TErr> Ensure(Func<T, bool> predicate, Func<TErr> errorFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(errorFactory);

        if (!_isValid)
            return this;

        return predicate(_value!) ? this : Validation<T, TErr>.Invalid(errorFactory());
    }

    /// <summary>
    /// Pattern matches on the validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> validAction, Action<IReadOnlyList<TErr>> invalidAction)
    {
        ThrowHelper.ThrowIfNull(validAction);
        ThrowHelper.ThrowIfNull(invalidAction);

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
        ThrowHelper.ThrowIfNull(validFunc);
        ThrowHelper.ThrowIfNull(invalidFunc);

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
        ThrowHelper.ThrowIfNull(combineErrors);

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

    /// <summary>
    /// Compares this Validation to another Validation.
    /// Invalid is considered less than Valid. When both are Valid, the values are compared.
    /// When both are Invalid, the error counts are compared first, then errors lexicographically.
    /// </summary>
    /// <param name="other">The other Validation to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Validation<T, TErr> other)
    {
        if (_isValid && other._isValid)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isValid && !other._isValid)
        {
            var countCompare = _errors!.Count.CompareTo(other._errors!.Count);
            if (countCompare != 0)
                return countCompare;
            for (int i = 0; i < _errors.Count; i++)
            {
                var cmp = Comparer<TErr>.Default.Compare(_errors[i], other._errors[i]);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        }
        return _isValid ? 1 : -1;
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is Validation<T, TErr> other)
            return CompareTo(other);
        ThrowHelper.ThrowArgument(nameof(obj), $"Object must be of type Validation<{typeof(T).Name}, {typeof(TErr).Name}>");
        return 0; // unreachable
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
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ValidationExtensions
{
    /// <summary>
    /// Combines multiple validations into one, accumulating all errors.
    /// Returns Valid only if ALL validations are valid.
    /// </summary>
    public static Validation<T, TErr> Combine<T, TErr>(
        this IEnumerable<Validation<T, TErr>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

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
        ThrowHelper.ThrowIfNull(action);

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
        ThrowHelper.ThrowIfNull(action);

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

    /// <summary>
    /// Flattens a nested Validation into a single Validation.
    /// If the outer validation is invalid, returns those errors.
    /// If the outer is valid and inner is invalid, returns the inner's errors.
    /// If both are valid, returns the inner's value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TErr">The type of the error.</typeparam>
    /// <param name="nested">The nested validation to flatten.</param>
    /// <returns>The flattened validation.</returns>
    /// <example>
    /// <code>
    /// var nested = Validation&lt;Validation&lt;int, string&gt;, string&gt;.Valid(
    ///     Validation&lt;int, string&gt;.Valid(42));
    /// var flattened = nested.Flatten(); // Valid(42)
    /// 
    /// var nestedInvalid = Validation&lt;Validation&lt;int, string&gt;, string&gt;.Valid(
    ///     Validation&lt;int, string&gt;.Invalid("inner error"));
    /// var flattenedInvalid = nestedInvalid.Flatten(); // Invalid(["inner error"])
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TErr> Flatten<T, TErr>(this Validation<Validation<T, TErr>, TErr> nested)
    {
        return nested.AndThen(static inner => inner);
    }

    #region Async Operations

    /// <summary>
    /// Asynchronously combines two Validation tasks using applicative functor semantics.
    /// If both are valid, applies the combiner function. If either/both are invalid, accumulates ALL errors.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = await ValidationExtensions.ApplyAsync(
    ///     ValidateUserAsync(user),
    ///     ValidateAddressAsync(address),
    ///     (u, a) => new ValidatedUserWithAddress(u, a)
    /// );
    /// </code>
    /// </example>
    public static async Task<Validation<U, TErr>> ApplyAsync<T, TIntermediate, U, TErr>(
        Task<Validation<T, TErr>> firstTask,
        Task<Validation<TIntermediate, TErr>> secondTask,
        Func<T, TIntermediate, U> combiner)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.Apply(result2, combiner);
    }

    /// <summary>
    /// Asynchronously zips two Validation tasks into a single Validation containing a tuple.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = await ValidationExtensions.ZipAsync(
    ///     ValidateNameAsync(name),
    ///     ValidateAgeAsync(age)
    /// ); // Task&lt;Validation&lt;(string, int), Error&gt;&gt;
    /// </code>
    /// </example>
    public static async Task<Validation<(T, U), TErr>> ZipAsync<T, U, TErr>(
        Task<Validation<T, TErr>> firstTask,
        Task<Validation<U, TErr>> secondTask)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.Zip(result2);
    }

    /// <summary>
    /// Asynchronously zips two Validation tasks using a combiner function.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <example>
    /// <code>
    /// var person = await ValidationExtensions.ZipWithAsync(
    ///     ValidateNameAsync(name),
    ///     ValidateAgeAsync(age),
    ///     (n, a) => new Person(n, a)
    /// );
    /// </code>
    /// </example>
    public static async Task<Validation<V, TErr>> ZipWithAsync<T, U, V, TErr>(
        Task<Validation<T, TErr>> firstTask,
        Task<Validation<U, TErr>> secondTask,
        Func<T, U, V> combiner)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.ZipWith(result2, combiner);
    }

    /// <summary>
    /// Asynchronously zips three Validation tasks into a single Validation containing a tuple.
    /// Accumulates ALL errors from all if any are invalid.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3), TErr>> ZipAsync<T1, T2, T3, TErr>(
        Task<Validation<T1, TErr>> first,
        Task<Validation<T2, TErr>> second,
        Task<Validation<T3, TErr>> third)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);

        var result1 = await first.ConfigureAwait(false);
        var result2 = await second.ConfigureAwait(false);
        var result3 = await third.ConfigureAwait(false);

        // Accumulate all errors
        var allErrors = new List<TErr>();
        if (result1.IsInvalid)
            allErrors.AddRange(result1.UnwrapErrors());
        if (result2.IsInvalid)
            allErrors.AddRange(result2.UnwrapErrors());
        if (result3.IsInvalid)
            allErrors.AddRange(result3.UnwrapErrors());

        if (allErrors.Count > 0)
            return Validation<(T1, T2, T3), TErr>.Invalid(allErrors);

        return Validation<(T1, T2, T3), TErr>.Valid((
            result1.Unwrap(),
            result2.Unwrap(),
            result3.Unwrap()
        ));
    }

    /// <summary>
    /// Asynchronously combines a collection of Validation tasks into a single Validation.
    /// Accumulates ALL errors from all if any are invalid.
    /// </summary>
    /// <example>
    /// <code>
    /// var items = new[] { item1, item2, item3 };
    /// var allValidated = await items
    ///     .Select(item => ValidateItemAsync(item))
    ///     .CombineAsync();
    /// </code>
    /// </example>
    public static async Task<Validation<IReadOnlyList<T>, TErr>> CombineAsync<T, TErr>(
        this IEnumerable<Task<Validation<T, TErr>>> validationTasks)
    {
        ThrowHelper.ThrowIfNull(validationTasks);

        var validations = await Task.WhenAll(validationTasks).ConfigureAwait(false);

        var allErrors = new List<TErr>();
        var values = new List<T>();

        foreach (var validation in validations)
        {
            if (validation.IsValid)
                values.Add(validation.Unwrap());
            else
                allErrors.AddRange(validation.UnwrapErrors());
        }

        return allErrors.Count == 0
            ? Validation<IReadOnlyList<T>, TErr>.Valid(values)
            : Validation<IReadOnlyList<T>, TErr>.Invalid(allErrors);
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in a valid value.
    /// </summary>
    public static async Task<Validation<T, TErr>> TapAsync<T, TErr>(
        this Task<Validation<T, TErr>> validationTask,
        Func<T, Task> action)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsValid)
            await action(validation.Unwrap()).ConfigureAwait(false);

        return validation;
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in errors.
    /// </summary>
    public static async Task<Validation<T, TErr>> TapErrorsAsync<T, TErr>(
        this Task<Validation<T, TErr>> validationTask,
        Func<IReadOnlyList<TErr>, Task> action)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsInvalid)
            await action(validation.UnwrapErrors()).ConfigureAwait(false);

        return validation;
    }

    /// <summary>
    /// Asynchronously maps the valid value using an async function.
    /// </summary>
    public static async Task<Validation<U, TErr>> MapAsync<T, U, TErr>(
        this Validation<T, TErr> validation,
        Func<T, Task<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (validation.IsInvalid)
            return Validation<U, TErr>.Invalid(validation.UnwrapErrors());

        var result = await mapper(validation.Unwrap()).ConfigureAwait(false);
        return Validation<U, TErr>.Valid(result);
    }

    /// <summary>
    /// Asynchronously maps the valid value from a validation task using an async function.
    /// </summary>
    public static async Task<Validation<U, TErr>> MapAsync<T, U, TErr>(
        this Task<Validation<T, TErr>> validationTask,
        Func<T, Task<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(mapper);

        var validation = await validationTask.ConfigureAwait(false);
        return await validation.MapAsync(mapper).ConfigureAwait(false);
    }

    #endregion
}
