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

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isValid
        ? $"Valid({_value})"
        : $"Invalid({(_errors.IsDefault ? 0 : _errors.Length)} errors)";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Validation(T value, ImmutableArray<TError> errors, bool isValid)
    {
        _value = value;
        _errors = errors;
        _isValid = isValid;
    }

    /// <summary>
    /// Returns true if the validation succeeded (no errors).
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isValid;
    }

    /// <summary>
    /// Returns true if the validation failed (has errors).
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isValid;
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
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        if (!_isValid)
            ThrowHelper.ThrowValidationIsInvalid(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);

        return _value!;
    }

    /// <summary>
    /// Returns the errors as an immutable array for efficient concatenation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<TError> GetErrors()
    {
        if (_isValid)
            ThrowHelper.ThrowValidationIsValid(_value!);

        return _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
    }

    /// <summary>
    /// Returns the valid value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        return _isValid ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the valid value, or throws an <see cref="InvalidOperationException"/> if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is invalid.</exception>
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
        if (!_isValid)
            ThrowHelper.ThrowValidationIsInvalid(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);

        return _value!;
    }

    /// <summary>
    /// Returns the errors, or throws an <see cref="InvalidOperationException"/> if valid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation is valid.</exception>
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
        if (_isValid)
            ThrowHelper.ThrowValidationIsValid(_value!);

        return _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
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
    /// <param name="errors">When this method returns, contains the errors if invalid; otherwise, an empty array.</param>
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
    public bool TryGetErrors(out ImmutableArray<TError> errors)
    {
        errors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
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
    /// var validation = Validation&lt;int, string&gt;.Ok(42);
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
    /// var validation = Validation&lt;int, string&gt;.Ok(42);
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
    public Validation<U, TError> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        return _isValid
            ? Validation<U, TError>.Ok(mapper(_value!))
            : Validation<U, TError>.Error(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
    }

    /// <summary>
    /// Maps the errors if they exist.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, F> MapErrors<F>(Func<TError, F> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        return _isValid
            ? Validation<T, F>.Ok(_value!)
            : Validation<T, F>.Error((_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors).Select(mapper).ToImmutableArray());
    }

    /// <summary>
    /// Maps each individual error using the specified mapper function.
    /// This is equivalent to <see cref="MapErrors{F}(Func{TError, F})"/> and provides
    /// a consistent naming convention with Result&lt;T, TError&gt;.MapError().
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, F> MapError<F>(Func<TError, F> mapper)
    {
        return MapErrors(mapper);
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
    public Validation<U, F> BiMap<U, F>(Func<T, U> valueMapper, Func<TError, F> errorMapper)
    {
        ThrowHelper.ThrowIfNull(valueMapper);
        ThrowHelper.ThrowIfNull(errorMapper);

        return _isValid
            ? Validation<U, F>.Ok(valueMapper(_value!))
            : Validation<U, F>.Error((_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors).Select(errorMapper).ToImmutableArray());
    }

    /// <summary>
    /// Combines two validations using applicative functor semantics.
    /// If both are valid, applies the function. If either/both are invalid, accumulates ALL errors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TError> Apply<TIntermediate, U>(
        Validation<TIntermediate, TError> other,
        Func<T, TIntermediate, U> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        if (_isValid && other.IsOk)
            return Validation<U, TError>.Ok(combiner(_value!, other._value!));

        if (!_isValid && !other.IsOk)
        {
            // Efficient concatenation using ImmutableArray.AddRange
            var myErrors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
            var otherErrors = other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors;
            var allErrors = myErrors.AddRange(otherErrors);
            return Validation<U, TError>.Error(allErrors);
        }

        return _isValid
            ? Validation<U, TError>.Error(other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors)
            : Validation<U, TError>.Error(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
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
    public Validation<(T, U), TError> Zip<U>(Validation<U, TError> other)
    {
        if (_isValid && other.IsOk)
            return Validation<(T, U), TError>.Ok((_value!, other.GetValue()));

        if (!_isValid && !other.IsOk)
        {
            var myErrors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
            var otherErrors = other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors;
            var allErrors = myErrors.AddRange(otherErrors);
            return Validation<(T, U), TError>.Error(allErrors);
        }

        return _isValid
            ? Validation<(T, U), TError>.Error(other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors)
            : Validation<(T, U), TError>.Error(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
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
    public Validation<V, TError> ZipWith<U, V>(Validation<U, TError> other, Func<T, U, V> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        if (_isValid && other.IsOk)
            return Validation<V, TError>.Ok(combiner(_value!, other.GetValue()));

        if (!_isValid && !other.IsOk)
        {
            var myErrors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
            var otherErrors = other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors;
            var allErrors = myErrors.AddRange(otherErrors);
            return Validation<V, TError>.Error(allErrors);
        }

        return _isValid
            ? Validation<V, TError>.Error(other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors)
            : Validation<V, TError>.Error(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
    }

    /// <summary>
    /// Combines this validation with another, accumulating errors from both if invalid.
    /// This is useful for running multiple independent validations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TError> And(Validation<T, TError> other)
    {
        if (_isValid && other.IsOk)
            return other; // Return the last valid value

        if (!_isValid && !other.IsOk)
        {
            var myErrors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
            var otherErrors = other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors;
            var allErrors = myErrors.AddRange(otherErrors);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<U, TError> Bind<U>(Func<T, Validation<U, TError>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);
        return _isValid ? binder(_value!) : Validation<U, TError>.Error(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
    }

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

        if (!_isValid)
            return this;

        return predicate(_value!) ? this : Validation<T, TError>.Error(errorFactory());
    }

    /// <summary>
    /// Validates the value against a predicate, returning Invalid with the specified error if the predicate fails.
    /// This is equivalent to <see cref="Ensure(Func{T, bool}, TError)"/> and provides
    /// a consistent naming convention with Result&lt;T, TError&gt;.FilterOrElse().
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <param name="error">The error to return if the predicate fails.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TError> FilterOrElse(Func<T, bool> predicate, TError error)
    {
        return Ensure(predicate, error);
    }

    /// <summary>
    /// Validates the value against a predicate, returning Invalid with a lazy error if the predicate fails.
    /// This is equivalent to <see cref="Ensure(Func{T, bool}, Func{TError})"/> and provides
    /// a consistent naming convention with Result&lt;T, TError&gt;.FilterOrElse().
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <param name="errorFactory">The factory function to create the error if the predicate fails.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Validation<T, TError> FilterOrElse(Func<T, bool> predicate, Func<TError> errorFactory)
    {
        return Ensure(predicate, errorFactory);
    }

    /// <summary>
    /// Pattern matches on the validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> validAction, Action<ImmutableArray<TError>> invalidAction)
    {
        ThrowHelper.ThrowIfNull(validAction);
        ThrowHelper.ThrowIfNull(invalidAction);

        if (_isValid)
            validAction(_value!);
        else
            invalidAction(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
    }

    /// <summary>
    /// Pattern matches on the validation and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> validFunc, Func<ImmutableArray<TError>, U> invalidFunc)
    {
        ThrowHelper.ThrowIfNull(validFunc);
        ThrowHelper.ThrowIfNull(invalidFunc);

        return _isValid ? validFunc(_value!) : invalidFunc(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors);
    }

    /// <summary>
    /// Converts this Validation to a Result.
    /// If invalid with multiple errors, only the first error is used.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult()
    {
        if (_isValid)
            return Result<T, TError>.Ok(_value!);

        var errs = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
        if (errs.Length == 0)
            ThrowHelper.ThrowValidationIsInvalid(errs);
        return Result<T, TError>.Error(errs[0]);
    }

    /// <summary>
    /// Converts this Validation to a Result with a combined error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult(Func<ImmutableArray<TError>, TError> combineErrors)
    {
        ThrowHelper.ThrowIfNull(combineErrors);

        return _isValid
            ? Result<T, TError>.Ok(_value!)
            : Result<T, TError>.Error(combineErrors(_errors.IsDefault ? ImmutableArray<TError>.Empty : _errors));
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
    public bool Equals(Validation<T, TError> other)
    {
        if (_isValid != other._isValid)
            return false;

        if (_isValid)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        var myErrors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
        var otherErrors = other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors;
        if (myErrors.Length != otherErrors.Length)
            return false;

        return myErrors.SequenceEqual(otherErrors);
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
        foreach (var error in _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors)
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
    public int CompareTo(Validation<T, TError> other)
    {
        if (_isValid && other._isValid)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isValid && !other._isValid)
        {
            var myErrors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
            var otherErrors = other._errors.IsDefault ? ImmutableArray<TError>.Empty : other._errors;
            var countCompare = myErrors.Length.CompareTo(otherErrors.Length);
            if (countCompare != 0)
                return countCompare;
            for (int i = 0; i < myErrors.Length; i++)
            {
                var cmp = Comparer<TError>.Default.Compare(myErrors[i], otherErrors[i]);
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
            : $"Invalid([{string.Join(", ", _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors)}])";
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
    /// <param name="errors">The errors, or empty array if Valid.</param>
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
    public void Deconstruct(out T? value, out ImmutableArray<TError> errors, out bool isValid)
    {
        value = _value;
        errors = _errors.IsDefault ? ImmutableArray<TError>.Empty : _errors;
        isValid = _isValid;
    }
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
