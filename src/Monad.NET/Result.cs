using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Result is a type that represents either success (Ok) or failure (Error).
/// This is inspired by Rust's Result&lt;T, E&gt; type.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
/// <typeparam name="TError">The type of the error value</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="Result{T,TError}"/> for operations that can fail with a specific error type.
/// This provides type-safe error handling without exceptions.
/// </para>
/// <para>
/// For simple presence/absence without error info, use <see cref="Option{T}"/>.
/// For validation with multiple accumulated errors, use <see cref="Validation{T,TError}"/>.
/// For wrapping exception-throwing code, use <see cref="Try{T}"/>.
/// </para>
/// </remarks>
/// <seealso cref="Option{T}"/>
/// <seealso cref="Validation{T,TError}"/>
/// <seealso cref="Try{T}"/>
/// <seealso cref="ResultExtensions"/>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(ResultDebugView<,>))]
public readonly struct Result<T, TError> : IEquatable<Result<T, TError>>, IComparable<Result<T, TError>>
{
    private readonly T? _value;
    private readonly TError? _error;
    private readonly bool _isOk;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isOk ? $"Ok({_value})" : $"Err({_error})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Result(T value, TError error, bool isOk)
    {
        _value = value;
        _error = error;
        _isOk = isOk;
    }

    /// <summary>
    /// Returns true if the result is Ok.
    /// </summary>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isOk;
    }

    /// <summary>
    /// Returns true if the result is an error (Error).
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isOk;
    }

    /// <summary>
    /// Gets the contained value for pattern matching. Returns the value if Ok, default otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    /// <example>
    /// <code>
    /// var message = result switch
    /// {
    ///     { IsOk: true, Value: var v } => $"Success: {v}",
    ///     { IsError: true, ErrorValue: var e } => $"Error: {e}",
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
    /// Gets the contained error for pattern matching. Returns the error if Error, default otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    public TError? ErrorValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _error;
    }

    /// <summary>
    /// Creates an Ok result containing the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Ok(T value)
    {
        if (value is null)
            ThrowHelper.ThrowCannotCreateOkWithNull();

        return new Result<T, TError>(value, default!, true);
    }

    /// <summary>
    /// Creates an Error result containing the specified error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Error(TError error)
    {
        if (error is null)
            ThrowHelper.ThrowCannotCreateErrWithNull();

        return new Result<T, TError>(default!, error, false);
    }

    /// <summary>
    /// Returns the contained Ok value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Err.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        if (!_isOk)
            ThrowHelper.ThrowResultIsErr(_error!);

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TError GetError()
    {
        if (_isOk)
            ThrowHelper.ThrowResultIsOk(_value!);
        if (_error is null)
            ThrowHelper.ThrowInvalidOperation(
                "Cannot get error from default-constructed Result. " +
                "Use Result<T,E>.Ok(value) or Result<T,E>.Error(error) to create a valid instance.");
        return _error!;
    }

    /// <summary>
    /// Returns the contained Ok value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        return _isOk ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Ok value, or throws an <see cref="InvalidOperationException"/> if Err.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Err.</exception>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// var value = result.GetOrThrow(); // 42
    /// 
    /// var error = Result&lt;int, string&gt;.Error("failed");
    /// error.GetOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow()
    {
        if (!_isOk)
            ThrowHelper.ThrowResultIsErr(_error!);

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value, or throws an <see cref="InvalidOperationException"/> if Ok.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Ok.</exception>
    /// <example>
    /// <code>
    /// var error = Result&lt;int, string&gt;.Error("failed");
    /// var err = error.GetErrorOrThrow(); // "failed"
    /// 
    /// var success = Result&lt;int, string&gt;.Ok(42);
    /// success.GetErrorOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TError GetErrorOrThrow()
    {
        if (_isOk)
            ThrowHelper.ThrowResultIsOk(_value!);
        if (_error is null)
            ThrowHelper.ThrowInvalidOperation(
                "Cannot get error from default-constructed Result. " +
                "Use Result<T,E>.Ok(value) or Result<T,E>.Error(error) to create a valid instance.");
        return _error!;
    }

    /// <summary>
    /// Returns the contained Err value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if Ok.
    /// </summary>
    /// <param name="message">The exception message if Ok</param>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Ok.</exception>
    /// <example>
    /// <code>
    /// var error = Result&lt;int, string&gt;.Error("failed");
    /// var err = error.GetErrorOrThrow("Expected failure"); // "failed"
    /// 
    /// var success = Result&lt;int, string&gt;.Ok(42);
    /// success.GetErrorOrThrow("Should have failed"); // throws with "Should have failed: 42"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TError GetErrorOrThrow(string message)
    {
        if (_isOk)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_value}");
        if (_error is null)
            ThrowHelper.ThrowInvalidOperation(
                "Cannot get error from default-constructed Result. " +
                "Use Result<T,E>.Ok(value) or Result<T,E>.Error(error) to create a valid instance.");
        return _error!;
    }

    /// <summary>
    /// Tries to get the contained Ok value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the Ok value if successful; otherwise, the default value.</param>
    /// <returns>True if the Result is Ok; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (result.TryGet(out var value))
    /// {
    ///     Console.WriteLine($"Success: {value}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(out T? value)
    {
        value = _value;
        return _isOk;
    }

    /// <summary>
    /// Tries to get the contained Err value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="error">When this method returns, contains the Err value if failed; otherwise, the default value.</param>
    /// <returns>True if the Result is Err; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (result.TryGetError(out var error))
    /// {
    ///     Console.WriteLine($"Error: {error}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetError(out TError? error)
    {
        error = _error;
        return !_isOk;
    }

    /// <summary>
    /// Returns true if the Result is Ok and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Result is Ok and contains the specified value; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// result.Contains(42); // true
    /// result.Contains(0);  // false
    /// Result&lt;int, string&gt;.Error("error").Contains(42); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        return _isOk && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Result is Err and contains the specified error.
    /// Uses the default equality comparer for type TError.
    /// </summary>
    /// <param name="error">The error to check for.</param>
    /// <returns>True if the Result is Err and contains the specified error; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Error("not found");
    /// result.ContainsError("not found"); // true
    /// result.ContainsError("other");     // false
    /// Result&lt;int, string&gt;.Ok(42).ContainsError("not found"); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsError(TError error)
    {
        return !_isOk && EqualityComparer<TError>.Default.Equals(_error, error);
    }

    /// <summary>
    /// Returns true if the Result is Ok and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Result is Ok and the predicate returns true; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// result.Exists(x => x > 40); // true
    /// result.Exists(x => x > 50); // false
    /// Result&lt;int, string&gt;.Error("error").Exists(x => x > 0); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isOk && predicate(_value!);
    }

    /// <summary>
    /// Returns true if the Result is Err and the predicate returns true for the contained error.
    /// </summary>
    /// <param name="predicate">The predicate to test the error against.</param>
    /// <returns>True if the Result is Err and the predicate returns true; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Error("not found");
    /// result.ExistsError(e => e.Contains("not")); // true
    /// result.ExistsError(e => e.Contains("xyz")); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistsError(Func<TError, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return !_isOk && predicate(_error!);
    }

    /// <summary>
    /// Maps a Result&lt;T, TError&gt; to Result&lt;U, TError&gt; by applying a function to a contained Ok value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TError> Map<U>(Func<T, U> mapper)
    {
        if (_isOk)
            return Result<U, TError>.Ok(mapper(_value!));
        return _error is null ? default : Result<U, TError>.Error(_error);
    }

    /// <summary>
    /// Maps a Result&lt;T, TError&gt; to Result&lt;T, F&gt; by applying a function to a contained Err value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, F> MapError<F>(Func<TError, F> mapper)
    {
        if (_isOk)
            return Result<T, F>.Ok(_value!);
        if (_error is null)
            return default;
        return Result<T, F>.Error(mapper(_error));
    }

    /// <summary>
    /// Maps both the Ok and Err values using the provided functions.
    /// This is useful when you need to transform both the success and error types simultaneously.
    /// </summary>
    /// <typeparam name="U">The new success type.</typeparam>
    /// <typeparam name="F">The new error type.</typeparam>
    /// <param name="okMapper">The function to apply to the Ok value.</param>
    /// <param name="errMapper">The function to apply to the Err value.</param>
    /// <returns>A new Result with both types transformed.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// var mapped = result.BiMap(
    ///     x => x.ToString(),
    ///     e => new Error(e)
    /// ); // Result&lt;string, Error&gt;.Ok("42")
    /// 
    /// var error = Result&lt;int, string&gt;.Error("not found");
    /// var mappedError = error.BiMap(
    ///     x => x.ToString(),
    ///     e => new Error(e)
    /// ); // Result&lt;string, Error&gt;.Error(Error("not found"))
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, F> BiMap<U, F>(Func<T, U> okMapper, Func<TError, F> errMapper)
    {
        ThrowHelper.ThrowIfNull(okMapper);
        ThrowHelper.ThrowIfNull(errMapper);

        if (_isOk)
            return Result<U, F>.Ok(okMapper(_value!));
        if (_error is null)
            return default;
        return Result<U, F>.Error(errMapper(_error));
    }

    /// <summary>
    /// Returns the provided default (if Err), or applies a function to the contained value (if Ok).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOr<U>(U defaultValue, Func<T, U> mapper)
    {
        return _isOk ? mapper(_value!) : defaultValue;
    }

    /// <summary>
    /// Maps a Result&lt;T, TError&gt; to U by applying a function to a contained Ok value, or a fallback function to a contained Err value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOrElse<U>(Func<TError, U> defaultFunc, Func<T, U> mapper)
    {
        return _isOk ? mapper(_value!) : defaultFunc(_error!);
    }

    /// <summary>
    /// Filters the Ok value based on a predicate, returning an Option.
    /// Returns Some(value) if Ok and predicate returns true; otherwise None.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <returns>Some(value) if Ok and predicate is true; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// result.Filter(x => x > 40); // Some(42)
    /// result.Filter(x => x > 50); // None
    /// 
    /// var err = Result&lt;int, string&gt;.Error("error");
    /// err.Filter(x => true); // None
    /// </code>
    /// </example>
    /// <remarks>
    /// This method discards the error information when converting to Option.
    /// Use <see cref="FilterOrElse(Func{T, bool}, TError)"/> if you need to preserve
    /// the Result type with a custom error for failed predicates.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Filter(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isOk && predicate(_value!) ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Filters the Ok value based on a predicate, returning Err with the specified error if the predicate fails.
    /// If already Err, returns the original error unchanged.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="error">The error to return if Ok and the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; the original Err if already Err; otherwise Err with the provided error.</returns>
    /// <example>
    /// <code>
    /// var ok = Result&lt;int, string&gt;.Ok(42);
    /// ok.FilterOrElse(x => x > 40, "Value too small"); // Ok(42)
    /// ok.FilterOrElse(x => x > 50, "Value too small"); // Error("Value too small")
    /// 
    /// var err = Result&lt;int, string&gt;.Error("original error");
    /// err.FilterOrElse(x => x > 0, "Value too small"); // Error("original error") - preserved
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> FilterOrElse(Func<T, bool> predicate, TError error)
    {
        ThrowHelper.ThrowIfNull(predicate);
        if (!_isOk)
            return this;
        return predicate(_value!) ? this : Error(error);
    }

    /// <summary>
    /// Filters the Ok value based on a predicate, returning Err with an error from the factory if the predicate fails.
    /// If already Err, returns the original error unchanged (factory is not called).
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="errorFactory">A function that creates the error if Ok and the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; the original Err if already Err; otherwise Err with the factory-created error.</returns>
    /// <example>
    /// <code>
    /// var ok = Result&lt;int, string&gt;.Ok(42);
    /// ok.FilterOrElse(x => x > 40, () => "Value too small"); // Ok(42)
    /// ok.FilterOrElse(x => x > 50, () => "Value too small"); // Error("Value too small")
    /// 
    /// var err = Result&lt;int, string&gt;.Error("original error");
    /// err.FilterOrElse(x => x > 0, () => "Value too small"); // Error("original error") - preserved
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> FilterOrElse(Func<T, bool> predicate, Func<TError> errorFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(errorFactory);
        if (!_isOk)
            return this;
        return predicate(_value!) ? this : Error(errorFactory());
    }

    /// <summary>
    /// Filters the Ok value based on a predicate, returning Err with an error from the factory if the predicate fails.
    /// The error factory receives the original value. If already Err, returns the original error unchanged.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="errorFactory">A function that creates the error from the value if Ok and the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; the original Err if already Err; otherwise Err with the factory-created error.</returns>
    /// <example>
    /// <code>
    /// var ok = Result&lt;int, string&gt;.Ok(42);
    /// ok.FilterOrElse(x => x > 50, x => $"Value {x} is too small"); // Error("Value 42 is too small")
    /// 
    /// var err = Result&lt;int, string&gt;.Error("original error");
    /// err.FilterOrElse(x => x > 0, x => $"Value {x} too small"); // Error("original error") - preserved
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> FilterOrElse(Func<T, bool> predicate, Func<T, TError> errorFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(errorFactory);
        if (!_isOk)
            return this;
        return predicate(_value!) ? this : Error(errorFactory(_value!));
    }

    /// <summary>
    /// Validates the Ok value against a predicate, returning Err with the specified error if the predicate fails.
    /// This is equivalent to <see cref="FilterOrElse(Func{T, bool}, TError)"/> and provides
    /// a consistent naming convention with Validation&lt;T, TError&gt;.Ensure().
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="error">The error to return if Ok and the predicate returns false.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Ensure(Func<T, bool> predicate, TError error)
    {
        return FilterOrElse(predicate, error);
    }

    /// <summary>
    /// Validates the Ok value against a predicate, returning Err with an error from the factory if the predicate fails.
    /// This is equivalent to <see cref="FilterOrElse(Func{T, bool}, Func{TError})"/> and provides
    /// a consistent naming convention with Validation&lt;T, TError&gt;.Ensure().
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="errorFactory">A function that creates the error if the predicate fails.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Ensure(Func<T, bool> predicate, Func<TError> errorFactory)
    {
        return FilterOrElse(predicate, errorFactory);
    }

    /// <summary>
    /// Validates the Ok value against a predicate, returning Err with an error from the factory if the predicate fails.
    /// The error factory receives the original value.
    /// This is equivalent to <see cref="FilterOrElse(Func{T, bool}, Func{T, TError})"/> and provides
    /// a consistent naming convention with Validation&lt;T, TError&gt;.Ensure().
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="errorFactory">A function that creates the error from the value if the predicate fails.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Ensure(Func<T, bool> predicate, Func<T, TError> errorFactory)
    {
        return FilterOrElse(predicate, errorFactory);
    }

    /// <summary>
    /// Calls the function if the result is Ok, otherwise returns the Err value.
    /// This is the monadic bind operation for control flow based on Result values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TError> Bind<U>(Func<T, Result<U, TError>> binder)
    {
        if (_isOk)
            return binder(_value!);
        return _error is null ? default : Result<U, TError>.Error(_error);
    }

    /// <summary>
    /// Combines this Result with another into a tuple.
    /// Returns the first error encountered if either Result is Err.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Result to combine with.</param>
    /// <returns>A Result containing a tuple of both values, or the first error.</returns>
    /// <example>
    /// <code>
    /// var user = GetUser(id);     // Result&lt;User, Error&gt;
    /// var order = GetOrder(oid);  // Result&lt;Order, Error&gt;
    /// var combined = user.Zip(order); // Result&lt;(User, Order), Error&gt;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<(T, U), TError> Zip<U>(Result<U, TError> other)
    {
        if (!_isOk)
            return _error is null ? default : Result<(T, U), TError>.Error(_error);
        if (!other.IsOk)
            return Result<(T, U), TError>.Error(other.GetError());
        return Result<(T, U), TError>.Ok((_value!, other.GetValue()));
    }

    /// <summary>
    /// Combines this Result with another using a combiner function.
    /// Returns the first error encountered if either Result is Err.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="other">The other Result to combine with.</param>
    /// <param name="combiner">A function to combine the values.</param>
    /// <returns>A Result containing the combined result, or the first error.</returns>
    /// <example>
    /// <code>
    /// var user = GetUser(id);
    /// var order = GetOrder(oid);
    /// var dto = user.ZipWith(order, (u, o) => new UserOrderDto(u, o));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<V, TError> ZipWith<U, V>(Result<U, TError> other, Func<T, U, V> combiner)
    {
        if (!_isOk)
            return _error is null ? default : Result<V, TError>.Error(_error);
        if (!other.IsOk)
            return Result<V, TError>.Error(other.GetError());
        return Result<V, TError>.Ok(combiner(_value!, other.GetValue()));
    }

    /// <summary>
    /// Returns resultB if the result is Ok, otherwise returns the Err value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TError> And<U>(Result<U, TError> resultB)
    {
        if (_isOk)
            return resultB;
        return _error is null ? default : Result<U, TError>.Error(_error);
    }

    /// <summary>
    /// Calls the function if the result is Err, otherwise returns the Ok value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, F> OrElse<F>(Func<TError, Result<T, F>> op)
    {
        return _isOk ? Result<T, F>.Ok(_value!) : op(_error!);
    }

    /// <summary>
    /// Returns the result if it contains an Ok value, otherwise returns resultB.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Or(Result<T, TError> resultB)
    {
        return _isOk ? this : resultB;
    }

    /// <summary>
    /// Converts from Result&lt;T, TError&gt; to Option&lt;T&gt;.
    /// Converts self into an Option&lt;T&gt;, consuming self, and discarding the error, if any.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Ok()
    {
        return _isOk ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts from Result&lt;T, TError&gt; to Option&lt;TError&gt;.
    /// Converts self into an Option&lt;TError&gt;, consuming self, and discarding the success value, if any.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TError> Err()
    {
        return _isOk ? Option<TError>.None() : (_error is null ? Option<TError>.None() : Option<TError>.Some(_error));
    }

    /// <summary>
    /// Converts the error to an Option.
    /// Returns Some(error) if Error, None if Ok.
    /// This is equivalent to <see cref="Err()"/> and provides
    /// a consistent naming convention with other conversion methods.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TError> ToErrorOption()
    {
        return Err();
    }

    /// <summary>
    /// Converts this Result to an Option, discarding any error information.
    /// Returns Some(value) if Ok; otherwise None.
    /// </summary>
    /// <returns>Some(value) if Ok; None if Err.</returns>
    /// <example>
    /// <code>
    /// Result&lt;int, string&gt;.Ok(42).ToOption();     // Some(42)
    /// Result&lt;int, string&gt;.Error("error").ToOption(); // None
    /// </code>
    /// </example>
    /// <remarks>
    /// This is equivalent to calling <see cref="Ok()"/> but provides a consistent
    /// API with other monadic types like Try, Validation, and RemoteData.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        return _isOk ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Pattern matches on the result and executes the appropriate action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> okAction, Action<TError> errAction)
    {
        if (_isOk)
            okAction(_value!);
        else
            errAction(_error!);
    }

    /// <summary>
    /// Pattern matches on the result and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> okFunc, Func<TError, U> errFunc)
    {
        return _isOk ? okFunc(_value!) : errFunc(_error!);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Result<T, TError> other)
    {
        if (_isOk != other._isOk)
            return false;

        if (_isOk)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        return EqualityComparer<TError>.Default.Equals(_error, other._error);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Result<T, TError> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return _isOk ? _value?.GetHashCode() ?? 0 : _error?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Compares this Result to another Result.
    /// Err is considered less than Ok. When both are Ok, the values are compared.
    /// When both are Err, the errors are compared.
    /// </summary>
    /// <param name="other">The other Result to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Result<T, TError> other)
    {
        if (_isOk && other._isOk)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isOk && !other._isOk)
            return Comparer<TError>.Default.Compare(_error, other._error);
        return _isOk ? 1 : -1;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isOk ? $"Ok({_value})" : $"Err({_error})";
    }

    /// <summary>
    /// Converts the Result to an enumerable sequence.
    /// Returns a sequence containing the value if Ok, or an empty sequence if Err.
    /// </summary>
    /// <returns>An enumerable containing zero or one element.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// foreach (var value in result.AsEnumerable())
    ///     Console.WriteLine(value); // Prints: 42
    ///
    /// // Useful for flattening collections of Results
    /// var results = new[] { Result&lt;int, string&gt;.Ok(1), Result&lt;int, string&gt;.Error("error"), Result&lt;int, string&gt;.Ok(3) };
    /// var values = results.SelectMany(r => r.AsEnumerable()); // [1, 3]
    /// </code>
    /// </example>
    public IEnumerable<T> AsEnumerable()
    {
        if (_isOk)
            yield return _value!;
    }

    /// <summary>
    /// Converts the Result to an array.
    /// Returns an array containing the value if Ok, or an empty array if Err.
    /// </summary>
    /// <returns>An array containing zero or one element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        return _isOk ? new[] { _value! } : Array.Empty<T>();
    }

    /// <summary>
    /// Converts the Result to a list.
    /// Returns a list containing the value if Ok, or an empty list if Err.
    /// </summary>
    /// <returns>A list containing zero or one element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        return _isOk ? [_value!] : [];
    }

    /// <summary>
    /// Determines whether two Result instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Result<T, TError> left, Result<T, TError> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Result instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Result<T, TError> left, Result<T, TError> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the Result into its components for pattern matching.
    /// </summary>
    /// <param name="value">The success value, or default if Err.</param>
    /// <param name="isOk">True if the Result is Ok.</param>
    /// <example>
    /// <code>
    /// var (value, isOk) = result;
    /// if (isOk)
    ///     Console.WriteLine($"Success: {value}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out bool isOk)
    {
        value = _value;
        isOk = _isOk;
    }

    /// <summary>
    /// Deconstructs the Result into all its components for pattern matching.
    /// </summary>
    /// <param name="value">The success value, or default if Err.</param>
    /// <param name="error">The error value, or default if Ok.</param>
    /// <param name="isOk">True if the Result is Ok.</param>
    /// <example>
    /// <code>
    /// var (value, error, isOk) = result;
    /// Console.WriteLine(isOk ? $"Value: {value}" : $"Error: {error}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out TError? error, out bool isOk)
    {
        value = _value;
        error = _error;
        isOk = _isOk;
    }
}

/// <summary>
/// Debug view proxy for <see cref="Result{T, TError}"/> to provide a better debugging experience.
/// </summary>
internal sealed class ResultDebugView<T, TError>
{
    private readonly Result<T, TError> _result;

    public ResultDebugView(Result<T, TError> result)
    {
        _result = result;
    }

    public bool IsOk => _result.IsOk;
    public bool IsErr => _result.IsError;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Value => _result.IsOk ? _result.GetValue() : null;

    public object? Error => _result.IsError ? _result.GetError() : null;
}
