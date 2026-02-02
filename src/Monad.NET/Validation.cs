using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a validation result that can accumulate multiple errors.
/// Unlike Result which short-circuits on the first error, Validation collects all errors.
/// This is an Applicative Functor, perfect for form validation and business rule checking.
/// </summary>
/// <typeparam name="T">The type of the valid value</typeparam>
/// <typeparam name="TError">The type of the error</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="Validation{T,TError}"/> when you need to collect ALL errors, such as form validation.
/// Combine multiple validations with <see cref="Apply"/> or <see cref="Zip"/> to accumulate errors.
/// </para>
/// <para>
/// <strong>DO NOT use LINQ query syntax (from...select) with Validation!</strong>
/// LINQ short-circuits on the first error, defeating error accumulation entirely.
/// The analyzer enforces this as an error (MNT013). Use <see cref="Apply"/> or <see cref="Zip"/> instead.
/// </para>
/// <para>
/// For fail-fast error handling, use <see cref="Result{T,TError}"/> instead.
/// </para>
/// </remarks>
/// <seealso cref="Result{T,TError}"/>
/// <seealso cref="Option{T}"/>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(ValidationDebugView<,>))]
public readonly struct Validation<T, TError> : IEquatable<Validation<T, TError>>, IComparable<Validation<T, TError>>
{
    private readonly T? _value;
    private readonly ImmutableArray<TError> _errors;
    private readonly bool _isValid;
    private readonly bool _isInitialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isInitialized
        ? (_isValid ? $"Valid({_value})" : $"Invalid({_errors.Length} errors)")
        : "Uninitialized";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Validation(T value, ImmutableArray<TError> errors, bool isValid)
    {
        _value = value;
        _errors = errors;
        _isValid = isValid;
        _isInitialized = true;
    }

    /// <summary>
    /// Indicates whether the Validation was properly initialized via factory methods.
    /// A default-constructed Validation (e.g., default(Validation&lt;T,E&gt;)) is not initialized.
    /// Always create Validations via <see cref="Ok(T)"/> or <see cref="Error(TError)"/> factory methods.
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
            ThrowHelper.ThrowValidationIsDefault();
    }

    /// <summary>
    /// Returns true if the validation succeeded (no errors).
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _isValid;
        }
    }

    /// <summary>
    /// Returns true if the validation failed (has errors).
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return !_isValid;
        }
    }

    /// <summary>
    /// Gets the contained value for pattern matching. Returns the value if Valid, default otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    /// <example>
    /// <code>
    /// var message = validation switch
    /// {
    ///     { IsOk: true, Value: var v } => $"Valid: {v}",
    ///     { IsError: true, Errors: var e } => $"Errors: {e.Length}",
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
    /// Gets the contained errors for pattern matching. Returns the errors if Invalid, empty array otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    public ImmutableArray<TError> Errors
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
    }

    /// <summary>
    /// Creates a valid (Ok) validation with the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Ok(T value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Ok with null value.");

        return new Validation<T, TError>(value, ImmutableArray<TError>.Empty, true);
    }

    /// <summary>
    /// Creates an invalid (Error) validation with a single error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Error(TError error)
    {
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Cannot create Error with null error.");

        return new Validation<T, TError>(default!, ImmutableArray.Create(error), false);
    }

    /// <summary>
    /// Creates an invalid (Error) validation with multiple errors.
    /// </summary>
    public static Validation<T, TError> Error(IEnumerable<TError> errors)
    {
        ThrowHelper.ThrowIfNull(errors);

        var errorArray = errors.ToImmutableArray();
        if (errorArray.IsEmpty)
            ThrowHelper.ThrowArgument(nameof(errors), "Must provide at least one error.");

        return new Validation<T, TError>(default!, errorArray, false);
    }

    /// <summary>
    /// Creates an invalid (Error) validation with multiple errors from an ImmutableArray.
    /// This overload avoids allocation when errors are already in an ImmutableArray.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Error(ImmutableArray<TError> errors)
    {
        if (errors.IsDefaultOrEmpty)
            ThrowHelper.ThrowArgument(nameof(errors), "Must provide at least one error.");

        return new Validation<T, TError>(default!, errors, false);
    }

    /// <summary>
    /// Returns the valid value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid or not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        ThrowIfDefault();
        if (!_isValid)
            ThrowHelper.ThrowValidationIsInvalid(_errors!);

        return _value!;
    }

    /// <summary>
    /// Returns the errors as an immutable array for efficient concatenation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid or not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<TError> GetErrors()
    {
        ThrowIfDefault();
        if (_isValid)
            ThrowHelper.ThrowValidationIsValid(_value!);

        return _errors;
    }

    /// <summary>
    /// Returns the valid value or a default value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        ThrowIfDefault();
        return _isValid ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the valid value, or throws an <see cref="InvalidOperationException"/> if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid or not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var valid = Validation&lt;int, string&gt;.Ok(42);
    /// var value = valid.GetOrThrow(); // 42
    /// 
    /// var invalid = Validation&lt;int, string&gt;.Error("error");
    /// invalid.GetOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow()
    {
        ThrowIfDefault();
        if (!_isValid)
            ThrowHelper.ThrowValidationIsInvalid(_errors!);

        return _value!;
    }

    /// <summary>
    /// Returns the errors, or throws an <see cref="InvalidOperationException"/> if valid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid or not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var invalid = Validation&lt;int, string&gt;.Error("error");
    /// var errors = invalid.GetErrorsOrThrow(); // ["error"]
    /// 
    /// var valid = Validation&lt;int, string&gt;.Ok(42);
    /// valid.GetErrorsOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<TError> GetErrorsOrThrow()
    {
        ThrowIfDefault();
        if (_isValid)
            ThrowHelper.ThrowValidationIsValid(_value!);

        return _errors;
    }

    /// <summary>
    /// Tries to get the contained valid value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the valid value if present; otherwise, the default value.</param>
    /// <returns>True if the Validation is valid; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
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
        ThrowIfDefault();
        value = _value;
        return _isValid;
    }

    /// <summary>
    /// Tries to get the contained errors using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="errors">When this method returns, contains the errors if invalid; otherwise, an empty array.</param>
    /// <returns>True if the Validation is invalid; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
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
    public bool TryGetErrors(out ImmutableArray<TError> errors)
    {
        ThrowIfDefault();
        errors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
        return !_isValid;
    }

    /// <summary>
    /// Returns true if the Validation is Valid and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Validation is Valid and contains the specified value; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;int, string&gt;.Ok(42);
    /// validation.Contains(42); // true
    /// validation.Contains(0);  // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        ThrowIfDefault();
        return _isValid && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Validation is Valid and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Validation is Valid and the predicate returns true; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;int, string&gt;.Ok(42);
    /// validation.Exists(x => x > 40); // true
    /// validation.Exists(x => x > 50); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowIfDefault();
        return _isValid && predicate(_value!);
    }

    /// <summary>
    /// Maps the valid value if it exists.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TError> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);
        ThrowIfDefault();

        return _isValid
            ? Validation<U, TError>.Ok(mapper(_value!))
            : Validation<U, TError>.Error(_errors!);
    }

    /// <summary>
    /// Maps the errors if they exist.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, F> MapErrors<F>(Func<TError, F> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);
        ThrowIfDefault();

        return _isValid
            ? Validation<T, F>.Ok(_value!)
            : Validation<T, F>.Error(_errors.Select(mapper).ToImmutableArray());
    }

    /// <summary>
    /// Maps both the valid value and errors.
    /// </summary>
    /// <typeparam name="U">The new valid value type.</typeparam>
    /// <typeparam name="F">The new error type.</typeparam>
    /// <param name="valueMapper">Function to transform the value if valid.</param>
    /// <param name="errorMapper">Function to transform each error if invalid.</param>
    /// <returns>A new Validation with transformed value or errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, F> BiMap<U, F>(Func<T, U> valueMapper, Func<TError, F> errorMapper)
    {
        ThrowHelper.ThrowIfNull(valueMapper);
        ThrowHelper.ThrowIfNull(errorMapper);
        ThrowIfDefault();

        return _isValid
            ? Validation<U, F>.Ok(valueMapper(_value!))
            : Validation<U, F>.Error(_errors.Select(errorMapper).ToImmutableArray());
    }

    /// <summary>
    /// Combines two validations using applicative functor semantics.
    /// If both are valid, applies the function. If either/both are invalid, accumulates ALL errors.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TError> Apply<TIntermediate, U>(
        Validation<TIntermediate, TError> other,
        Func<T, TIntermediate, U> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);
        ThrowIfDefault();

        if (_isValid && other.IsOk)
            return Validation<U, TError>.Ok(combiner(_value!, other._value!));

        if (!_isValid && !other.IsOk)
        {
            // Efficient concatenation using ImmutableArray.AddRange
            var allErrors = _errors.AddRange(other._errors);
            return Validation<U, TError>.Error(allErrors);
        }

        return _isValid
            ? Validation<U, TError>.Error(other._errors)
            : Validation<U, TError>.Error(_errors);
    }

    /// <summary>
    /// Combines this Validation with another into a tuple.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Validation to combine with.</param>
    /// <returns>A Validation containing a tuple of both values, or accumulated errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var nameValidation = ValidateName(name);   // Validation&lt;string, Error&gt;
    /// var ageValidation = ValidateAge(age);      // Validation&lt;int, Error&gt;
    /// var combined = nameValidation.Zip(ageValidation); // Validation&lt;(string, int), Error&gt;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<(T, U), TError> Zip<U>(Validation<U, TError> other)
    {
        ThrowIfDefault();
        if (_isValid && other.IsOk)
            return Validation<(T, U), TError>.Ok((_value!, other.GetValue()));

        if (!_isValid && !other.IsOk)
        {
            var allErrors = _errors.AddRange(other._errors);
            return Validation<(T, U), TError>.Error(allErrors);
        }

        return _isValid
            ? Validation<(T, U), TError>.Error(other._errors)
            : Validation<(T, U), TError>.Error(_errors);
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
    /// <exception cref="InvalidOperationException">Thrown if this Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var nameValidation = ValidateName(name);
    /// var ageValidation = ValidateAge(age);
    /// var person = nameValidation.ZipWith(ageValidation, (n, a) => new Person(n, a));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<V, TError> ZipWith<U, V>(Validation<U, TError> other, Func<T, U, V> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);
        ThrowIfDefault();

        if (_isValid && other.IsOk)
            return Validation<V, TError>.Ok(combiner(_value!, other.GetValue()));

        if (!_isValid && !other.IsOk)
        {
            var allErrors = _errors.AddRange(other._errors);
            return Validation<V, TError>.Error(allErrors);
        }

        return _isValid
            ? Validation<V, TError>.Error(other._errors)
            : Validation<V, TError>.Error(_errors);
    }

    /// <summary>
    /// Combines this validation with another, accumulating errors from both if invalid.
    /// This is useful for running multiple independent validations.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TError> And(Validation<T, TError> other)
    {
        ThrowIfDefault();
        if (_isValid && other.IsOk)
            return other; // Return the last valid value

        if (!_isValid && !other.IsOk)
        {
            var allErrors = _errors.AddRange(other._errors);
            return Validation<T, TError>.Error(allErrors);
        }

        return _isValid ? other : this;
    }

    /// <summary>
    /// Chains a validation operation. If this is invalid, returns this.
    /// If this is valid, applies the function (which may return invalid).
    /// Note: This does NOT accumulate errors like And() - it short-circuits like Result.
    /// This is the monadic bind operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TError> Bind<U>(Func<T, Validation<U, TError>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);
        ThrowIfDefault();

        return _isValid ? binder(_value!) : Validation<U, TError>.Error(_errors!);
    }

    /// <summary>
    /// Validates the contained value against a predicate. If the validation is already invalid,
    /// returns this unchanged. If the predicate returns false, returns an Invalid validation with the specified error.
    /// This is useful for adding additional validation rules to an already valid value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <param name="error">The error to return if the predicate fails.</param>
    /// <returns>This validation if valid and predicate passes; Invalid with error if predicate fails; or this if already invalid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;int, string&gt;.Ok(18)
    ///     .Ensure(x =&gt; x &gt;= 18, "Must be at least 18")
    ///     .Ensure(x =&gt; x &lt;= 120, "Must be at most 120");
    /// // Valid(18)
    /// 
    /// var invalid = Validation&lt;int, string&gt;.Ok(15)
    ///     .Ensure(x =&gt; x &gt;= 18, "Must be at least 18");
    /// // Invalid(["Must be at least 18"])
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TError> Ensure(Func<T, bool> predicate, TError error)
    {
        ThrowHelper.ThrowIfNull(predicate);
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Error cannot be null.");
        ThrowIfDefault();

        if (!_isValid)
            return this;

        return predicate(_value!) ? this : Validation<T, TError>.Error(error);
    }

    /// <summary>
    /// Validates the contained value against a predicate with a lazy error factory.
    /// If the validation is already invalid, returns this unchanged (error factory is not called).
    /// If the predicate returns false, returns an Invalid validation with the error from the factory.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <param name="errorFactory">The factory function to create the error if the predicate fails.</param>
    /// <returns>This validation if valid and predicate passes; Invalid with error if predicate fails; or this if already invalid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var validation = Validation&lt;User, string&gt;.Ok(user)
    ///     .Ensure(u =&gt; u.Email.Contains("@"), () =&gt; $"Invalid email: {user.Email}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TError> Ensure(Func<T, bool> predicate, Func<TError> errorFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(errorFactory);
        ThrowIfDefault();

        if (!_isValid)
            return this;

        return predicate(_value!) ? this : Validation<T, TError>.Error(errorFactory());
    }

    /// <summary>
    /// Pattern matches on the validation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> validAction, Action<ImmutableArray<TError>> invalidAction)
    {
        ThrowHelper.ThrowIfNull(validAction);
        ThrowHelper.ThrowIfNull(invalidAction);
        ThrowIfDefault();

        if (_isValid)
            validAction(_value!);
        else
            invalidAction(_errors);
    }

    /// <summary>
    /// Pattern matches on the validation and returns a result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> validFunc, Func<ImmutableArray<TError>, U> invalidFunc)
    {
        ThrowHelper.ThrowIfNull(validFunc);
        ThrowHelper.ThrowIfNull(invalidFunc);
        ThrowIfDefault();

        return _isValid ? validFunc(_value!) : invalidFunc(_errors);
    }

    /// <summary>
    /// Converts this Validation to a Result.
    /// If invalid with multiple errors, only the first error is used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult()
    {
        ThrowIfDefault();
        return _isValid
            ? Result<T, TError>.Ok(_value!)
            : Result<T, TError>.Error(_errors![0]);
    }

    /// <summary>
    /// Converts this Validation to a Result with a combined error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult(Func<ImmutableArray<TError>, TError> combineErrors)
    {
        ThrowHelper.ThrowIfNull(combineErrors);
        ThrowIfDefault();

        return _isValid
            ? Result<T, TError>.Ok(_value!)
            : Result<T, TError>.Error(combineErrors(_errors));
    }

    /// <summary>
    /// Converts this Validation to an Option.
    /// Discards error information if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        ThrowIfDefault();
        return _isValid ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Validation<T, TError> other)
    {
        if (_isValid != other._isValid)
            return false;

        if (_isValid)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        if (_errors.Length != other._errors.Length)
            return false;

        return _errors.SequenceEqual(other._errors);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Validation<T, TError> other && Equals(other);
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
    /// <exception cref="InvalidOperationException">Thrown if either Validation was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Validation<T, TError> other)
    {
        ThrowIfDefault();
        other.ThrowIfDefault();
        if (_isValid && other._isValid)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isValid && !other._isValid)
        {
            var countCompare = _errors.Length.CompareTo(other._errors.Length);
            if (countCompare != 0)
                return countCompare;
            for (int i = 0; i < _errors.Length; i++)
            {
                var cmp = Comparer<TError>.Default.Compare(_errors[i], other._errors[i]);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        }
        return _isValid ? 1 : -1;
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
    public static bool operator ==(Validation<T, TError> left, Validation<T, TError> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Validation instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Validation<T, TError> left, Validation<T, TError> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the Validation into its components for pattern matching.
    /// </summary>
    /// <param name="value">The valid value, or default if Invalid.</param>
    /// <param name="isValid">True if the Validation is valid.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
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
        ThrowIfDefault();
        value = _value;
        isValid = _isValid;
    }

    /// <summary>
    /// Deconstructs the Validation into all its components for pattern matching.
    /// </summary>
    /// <param name="value">The valid value, or default if Invalid.</param>
    /// <param name="errors">The errors, or empty array if Valid.</param>
    /// <param name="isValid">True if the Validation is valid.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Validation was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var (value, errors, isValid) = validation;
    /// if (!isValid)
    ///     foreach (var error in errors)
    ///         Console.WriteLine($"Error: {error}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out ImmutableArray<TError> errors, out bool isValid)
    {
        ThrowIfDefault();
        value = _value;
        errors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
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
    public static Validation<T, TError> Combine<T, TError>(
        this IEnumerable<Validation<T, TError>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

        var validationList = validations.ToList();
        if (validationList.Count == 0)
            ThrowHelper.ThrowArgument(nameof(validations), "Must provide at least one validation.");

        var errorBuilder = ImmutableArray.CreateBuilder<TError>();
        T? lastValue = default;

        foreach (var validation in validationList)
        {
            if (validation.IsOk)
                lastValue = validation.GetValue();
            else
                errorBuilder.AddRange(validation.GetErrors());
        }

        return errorBuilder.Count == 0
            ? Validation<T, TError>.Ok(lastValue!)
            : Validation<T, TError>.Error(errorBuilder.ToImmutable());
    }

    /// <summary>
    /// Executes an action if the validation is valid, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Tap<T, TError>(
        this Validation<T, TError> validation,
        Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (validation.IsOk)
            action(validation.GetValue());

        return validation;
    }

    /// <summary>
    /// Executes an action if the validation is invalid, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> TapErrors<T, TError>(
        this Validation<T, TError> validation,
        Action<ImmutableArray<TError>> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (validation.IsError)
            action(validation.GetErrors());

        return validation;
    }

    /// <summary>
    /// Converts a Result to a Validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> ToValidation<T, TError>(this Result<T, TError> result)
    {
        return result.Match(
            okFunc: static value => Validation<T, TError>.Ok(value),
            errFunc: static err => Validation<T, TError>.Error(err)
        );
    }

    /// <summary>
    /// Flattens a nested Validation into a single Validation.
    /// If the outer validation is invalid, returns those errors.
    /// If the outer is valid and inner is invalid, returns the inner's errors.
    /// If both are valid, returns the inner's value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="nested">The nested validation to flatten.</param>
    /// <returns>The flattened validation.</returns>
    /// <example>
    /// <code>
    /// var nested = Validation&lt;Validation&lt;int, string&gt;, string&gt;.Ok(
    ///     Validation&lt;int, string&gt;.Ok(42));
    /// var flattened = nested.Flatten(); // Valid(42)
    /// 
    /// var nestedInvalid = Validation&lt;Validation&lt;int, string&gt;, string&gt;.Ok(
    ///     Validation&lt;int, string&gt;.Error("inner error"));
    /// var flattenedInvalid = nestedInvalid.Flatten(); // Invalid(["inner error"])
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Flatten<T, TError>(this Validation<Validation<T, TError>, TError> nested)
    {
        return nested.Bind(static inner => inner);
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
    public static async Task<Validation<U, TError>> ApplyAsync<T, TIntermediate, U, TError>(
        Task<Validation<T, TError>> firstTask,
        Task<Validation<TIntermediate, TError>> secondTask,
        Func<T, TIntermediate, U> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
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
    public static async Task<Validation<(T, U), TError>> ZipAsync<T, U, TError>(
        Task<Validation<T, TError>> firstTask,
        Task<Validation<U, TError>> secondTask,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
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
    public static async Task<Validation<V, TError>> ZipWithAsync<T, U, V, TError>(
        Task<Validation<T, TError>> firstTask,
        Task<Validation<U, TError>> secondTask,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.ZipWith(result2, combiner);
    }

    /// <summary>
    /// Asynchronously zips three Validation tasks into a single Validation containing a tuple.
    /// Accumulates ALL errors from all if any are invalid.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3), TError>> ZipAsync<T1, T2, T3, TError>(
        Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result3 = await third.ConfigureAwait(false);

        // Accumulate all errors using ImmutableArray.Builder
        var errorBuilder = ImmutableArray.CreateBuilder<TError>();
        if (result1.IsError)
            errorBuilder.AddRange(result1.GetErrors());
        if (result2.IsError)
            errorBuilder.AddRange(result2.GetErrors());
        if (result3.IsError)
            errorBuilder.AddRange(result3.GetErrors());

        if (errorBuilder.Count > 0)
            return Validation<(T1, T2, T3), TError>.Error(errorBuilder.ToImmutable());

        return Validation<(T1, T2, T3), TError>.Ok((
            result1.GetValue(),
            result2.GetValue(),
            result3.GetValue()
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
    public static async Task<Validation<ImmutableArray<T>, TError>> CombineAsync<T, TError>(
        this IEnumerable<Task<Validation<T, TError>>> validationTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var validations = await Task.WhenAll(validationTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var errorBuilder = ImmutableArray.CreateBuilder<TError>();
        var valueBuilder = ImmutableArray.CreateBuilder<T>();

        foreach (var validation in validations)
        {
            if (validation.IsOk)
                valueBuilder.Add(validation.GetValue());
            else
                errorBuilder.AddRange(validation.GetErrors());
        }

        return errorBuilder.Count == 0
            ? Validation<ImmutableArray<T>, TError>.Ok(valueBuilder.ToImmutable())
            : Validation<ImmutableArray<T>, TError>.Error(errorBuilder.ToImmutable());
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in a valid value.
    /// </summary>
    public static async Task<Validation<T, TError>> TapAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetValue()).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in errors.
    /// </summary>
    public static async Task<Validation<T, TError>> TapErrorsAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<ImmutableArray<TError>, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsError)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetErrors()).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously maps the valid value using an async function.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Validation<T, TError> validation,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (validation.IsError)
            return Validation<U, TError>.Error(validation.GetErrors());

        var result = await mapper(validation.GetValue()).ConfigureAwait(false);
        return Validation<U, TError>.Ok(result);
    }

    /// <summary>
    /// Asynchronously maps the valid value from a validation task using an async function.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        return await validation.MapAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region CancellationToken Overloads

    /// <summary>
    /// Asynchronously executes an action if the validation task results in a valid value, with cancellation support.
    /// </summary>
    public static async Task<Validation<T, TError>> TapAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetValue(), cancellationToken).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in errors, with cancellation support.
    /// </summary>
    public static async Task<Validation<T, TError>> TapErrorsAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<ImmutableArray<TError>, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsError)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetErrors(), cancellationToken).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously maps the valid value using an async function with cancellation support.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Validation<T, TError> validation,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (validation.IsError)
            return Validation<U, TError>.Error(validation.GetErrors());

        var result = await mapper(validation.GetValue(), cancellationToken).ConfigureAwait(false);
        return Validation<U, TError>.Ok(result);
    }

    /// <summary>
    /// Asynchronously maps the valid value from a validation task using an async function with cancellation support.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        return await validation.MapAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously applies a validation with cancellation support.
    /// </summary>
    public static async Task<Validation<U, TError>> ApplyAsync<T, TIntermediate, U, TError>(
        this Task<Validation<TIntermediate, TError>> first,
        Func<TIntermediate, CancellationToken, Task<Validation<T, TError>>> second,
        Func<TIntermediate, T, U> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var firstValidation = await first.ConfigureAwait(false);

        if (firstValidation.IsError)
            return Validation<U, TError>.Error(firstValidation.GetErrors());

        cancellationToken.ThrowIfCancellationRequested();
        var secondValidation = await second(firstValidation.GetValue(), cancellationToken).ConfigureAwait(false);

        if (secondValidation.IsError)
            return Validation<U, TError>.Error(secondValidation.GetErrors());

        return Validation<U, TError>.Ok(combiner(firstValidation.GetValue(), secondValidation.GetValue()));
    }

    /// <summary>
    /// Asynchronously zips two validations with cancellation support.
    /// </summary>
    public static async Task<Validation<(T, U), TError>> ZipAsync<T, U, TError>(
        this Task<Validation<T, TError>> first,
        Func<CancellationToken, Task<Validation<U, TError>>> second,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        cancellationToken.ThrowIfCancellationRequested();

        var firstValidation = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var secondValidation = await second(cancellationToken).ConfigureAwait(false);

        return firstValidation.Zip(secondValidation);
    }

    /// <summary>
    /// Asynchronously zips two validations with a combiner function and cancellation support.
    /// </summary>
    public static async Task<Validation<V, TError>> ZipWithAsync<T, U, V, TError>(
        this Task<Validation<T, TError>> first,
        Func<CancellationToken, Task<Validation<U, TError>>> second,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var firstValidation = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var secondValidation = await second(cancellationToken).ConfigureAwait(false);

        return firstValidation.ZipWith(secondValidation, combiner);
    }

    #endregion

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a Validation in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Validation<T, TError>> AsValueTask<T, TError>(this Validation<T, TError> validation)
        => new(validation);

    /// <summary>
    /// Maps the valid value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Validation<U, TError>> MapAsync<T, U, TError>(
        this ValueTask<Validation<T, TError>> validationTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (validationTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(validationTask.Result.Map(mapper));
        }
        return Core(validationTask, mapper, cancellationToken);

        static async ValueTask<Validation<U, TError>> Core(ValueTask<Validation<T, TError>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var v = await t.ConfigureAwait(false);
            return v.Map(m);
        }
    }

    /// <summary>
    /// Maps the valid value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Validation<U, TError>> MapAsync<T, U, TError>(
        this ValueTask<Validation<T, TError>> validationTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsError)
            return Validation<U, TError>.Error(validation.GetErrors());

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(validation.GetValue(), cancellationToken).ConfigureAwait(false);
        return Validation<U, TError>.Ok(result);
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, TError, U>(
        this ValueTask<Validation<T, TError>> validationTask,
        Func<T, U> validFunc,
        Func<ImmutableArray<TError>, U> invalidFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validFunc);
        ThrowHelper.ThrowIfNull(invalidFunc);
        if (validationTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(validationTask.Result.Match(validFunc, invalidFunc));
        }
        return Core(validationTask, validFunc, invalidFunc, cancellationToken);

        static async ValueTask<U> Core(ValueTask<Validation<T, TError>> t, Func<T, U> v, Func<ImmutableArray<TError>, U> i, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var validation = await t.ConfigureAwait(false);
            return validation.Match(v, i);
        }
    }

    /// <summary>
    /// Zips two ValueTask validations into a tuple. Accumulates ALL errors.
    /// </summary>
    public static async ValueTask<Validation<(T, U), TError>> ZipAsync<T, U, TError>(
        ValueTask<Validation<T, TError>> firstTask,
        ValueTask<Validation<U, TError>> secondTask,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.Zip(result2);
    }

    /// <summary>
    /// Zips two ValueTask validations using a combiner. Accumulates ALL errors.
    /// </summary>
    public static async ValueTask<Validation<V, TError>> ZipWithAsync<T, U, V, TError>(
        ValueTask<Validation<T, TError>> firstTask,
        ValueTask<Validation<U, TError>> secondTask,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.ZipWith(result2, combiner);
    }

    #endregion
}

/// <summary>
/// Debug view proxy for <see cref="Validation{T, TError}"/> to provide a better debugging experience.
/// </summary>
internal sealed class ValidationDebugView<T, TError>
{
    private readonly Validation<T, TError> _validation;

    public ValidationDebugView(Validation<T, TError> validation)
    {
        _validation = validation;
    }

    public bool IsOk => _validation.IsOk;
    public bool IsError => _validation.IsError;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Value => _validation.IsOk ? _validation.GetValue() : null;

    public ImmutableArray<TError>? Errors => _validation.IsError ? _validation.GetErrorsOrThrow() : null;
}
