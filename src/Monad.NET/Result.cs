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
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(ResultDebugView<,>))]
public readonly struct Result<T, TError> : IEquatable<Result<T, TError>>, IComparable<Result<T, TError>>
{
    private readonly T? _value;
    private readonly TError? _error;
    private readonly bool _isOk;
    private readonly bool _isInitialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => !_isInitialized ? "Uninitialized (default struct)" : _isOk ? $"Ok({_value})" : $"Err({_error})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Result(T value, TError error, bool isOk)
    {
        _value = value;
        _error = error;
        _isOk = isOk;
        _isInitialized = true;
    }

    /// <summary>
    /// Returns true if the Result was properly initialized via <see cref="Ok(T)"/> or <see cref="Error(TError)"/>.
    /// A default-constructed struct (e.g., <c>default(Result&lt;T,E&gt;)</c>) returns false.
    /// </summary>
    /// <remarks>
    /// Using a default-constructed Result in any operation will throw <see cref="InvalidOperationException"/>.
    /// Always create Results via <see cref="Ok(T)"/> or <see cref="Error(TError)"/> factory methods.
    /// </remarks>
    public bool IsInitialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isInitialized;
    }

    /// <summary>
    /// Returns true if the result is Ok.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _isOk;
        }
    }

    /// <summary>
    /// Returns true if the result is an error (Error).
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return !_isOk;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDefault()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowResultIsDefault();
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
    /// <exception cref="InvalidOperationException">Thrown if the value is Err or if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        ThrowIfDefault();
        if (!_isOk)
            ThrowHelper.ThrowResultIsErr(_error!);

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok or if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TError GetError()
    {
        ThrowIfDefault();
        if (_isOk)
            ThrowHelper.ThrowResultIsOk(_value!);

        return _error!;
    }

    /// <summary>
    /// Returns the contained Ok value or a default value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        ThrowIfDefault();
        return _isOk ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Ok value, or throws an <see cref="InvalidOperationException"/> if Err.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Err or if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (!_isOk)
            ThrowHelper.ThrowResultIsErr(_error!);

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value, or throws an <see cref="InvalidOperationException"/> if Ok.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Ok or if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (_isOk)
            ThrowHelper.ThrowResultIsOk(_value!);

        return _error!;
    }

    /// <summary>
    /// Returns the contained Err value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if Ok.
    /// </summary>
    /// <param name="message">The exception message if Ok</param>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Ok or if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (_isOk)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_value}");

        return _error!;
    }

    /// <summary>
    /// Tries to get the contained Ok value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the Ok value if successful; otherwise, the default value.</param>
    /// <returns>True if the Result is Ok; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        value = _value;
        return _isOk;
    }

    /// <summary>
    /// Tries to get the contained Err value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="error">When this method returns, contains the Err value if failed; otherwise, the default value.</param>
    /// <returns>True if the Result is Err; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        error = _error;
        return !_isOk;
    }

    /// <summary>
    /// Returns true if the Result is Ok and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Result is Ok and contains the specified value; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        return _isOk && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Result is Err and contains the specified error.
    /// Uses the default equality comparer for type TError.
    /// </summary>
    /// <param name="error">The error to check for.</param>
    /// <returns>True if the Result is Err and contains the specified error; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        return !_isOk && EqualityComparer<TError>.Default.Equals(_error, error);
    }

    /// <summary>
    /// Returns true if the Result is Ok and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Result is Ok and the predicate returns true; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        return _isOk && predicate(_value!);
    }

    /// <summary>
    /// Returns true if the Result is Err and the predicate returns true for the contained error.
    /// </summary>
    /// <param name="predicate">The predicate to test the error against.</param>
    /// <returns>True if the Result is Err and the predicate returns true; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        return !_isOk && predicate(_error!);
    }

    /// <summary>
    /// Maps a Result&lt;T, TError&gt; to Result&lt;U, TError&gt; by applying a function to a contained Ok value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TError> Map<U>(Func<T, U> mapper)
    {
        ThrowIfDefault();
        return _isOk ? Result<U, TError>.Ok(mapper(_value!)) : Result<U, TError>.Error(_error!);
    }

    /// <summary>
    /// Maps a Result&lt;T, TError&gt; to Result&lt;T, F&gt; by applying a function to a contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, F> MapError<F>(Func<TError, F> mapper)
    {
        ThrowIfDefault();
        return _isOk ? Result<T, F>.Ok(_value!) : Result<T, F>.Error(mapper(_error!));
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
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();

        return _isOk
            ? Result<U, F>.Ok(okMapper(_value!))
            : Result<U, F>.Error(errMapper(_error!));
    }

    /// <summary>
    /// Returns the provided default (if Err), or applies a function to the contained value (if Ok).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOr<U>(U defaultValue, Func<T, U> mapper)
    {
        ThrowIfDefault();
        return _isOk ? mapper(_value!) : defaultValue;
    }

    /// <summary>
    /// Maps a Result&lt;T, TError&gt; to U by applying a function to a contained Ok value, or a fallback function to a contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOrElse<U>(Func<TError, U> defaultFunc, Func<T, U> mapper)
    {
        ThrowIfDefault();
        return _isOk ? mapper(_value!) : defaultFunc(_error!);
    }

    /// <summary>
    /// Filters the Ok value based on a predicate, returning an Option.
    /// Returns Some(value) if Ok and predicate returns true; otherwise None.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <returns>Some(value) if Ok and predicate is true; otherwise None.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        return _isOk && predicate(_value!) ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Filters the Ok value based on a predicate, returning Err with the specified error if the predicate fails.
    /// If already Err, returns the original error unchanged.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="error">The error to return if Ok and the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; the original Err if already Err; otherwise Err with the provided error.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
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
        ThrowIfDefault();
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
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (!_isOk)
            return this;
        return predicate(_value!) ? this : Error(errorFactory(_value!));
    }

    /// <summary>
    /// Calls the function if the result is Ok, otherwise returns the Err value.
    /// This is the monadic bind operation for control flow based on Result values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TError> Bind<U>(Func<T, Result<U, TError>> binder)
    {
        ThrowIfDefault();
        return _isOk ? binder(_value!) : Result<U, TError>.Error(_error!);
    }

    /// <summary>
    /// Combines this Result with another into a tuple.
    /// Returns the first error encountered if either Result is Err.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Result to combine with.</param>
    /// <returns>A Result containing a tuple of both values, or the first error.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (!_isOk)
            return Result<(T, U), TError>.Error(_error!);
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
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (!_isOk)
            return Result<V, TError>.Error(_error!);
        if (!other.IsOk)
            return Result<V, TError>.Error(other.GetError());
        return Result<V, TError>.Ok(combiner(_value!, other.GetValue()));
    }

    /// <summary>
    /// Returns resultB if the result is Ok, otherwise returns the Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TError> And<U>(Result<U, TError> resultB)
    {
        ThrowIfDefault();
        return _isOk ? resultB : Result<U, TError>.Error(_error!);
    }

    /// <summary>
    /// Calls the function if the result is Err, otherwise returns the Ok value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, F> OrElse<F>(Func<TError, Result<T, F>> op)
    {
        ThrowIfDefault();
        return _isOk ? Result<T, F>.Ok(_value!) : op(_error!);
    }

    /// <summary>
    /// Returns the result if it contains an Ok value, otherwise returns resultB.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Or(Result<T, TError> resultB)
    {
        ThrowIfDefault();
        return _isOk ? this : resultB;
    }

    /// <summary>
    /// Converts from Result&lt;T, TError&gt; to Option&lt;T&gt;.
    /// Converts self into an Option&lt;T&gt;, consuming self, and discarding the error, if any.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Ok()
    {
        ThrowIfDefault();
        return _isOk ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts from Result&lt;T, TError&gt; to Option&lt;TError&gt;.
    /// Converts self into an Option&lt;TError&gt;, consuming self, and discarding the success value, if any.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TError> Err()
    {
        ThrowIfDefault();
        return _isOk ? Option<TError>.None() : Option<TError>.Some(_error!);
    }

    /// <summary>
    /// Converts this Result to an Option, discarding any error information.
    /// Returns Some(value) if Ok; otherwise None.
    /// </summary>
    /// <returns>Some(value) if Ok; None if Err.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        return _isOk ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Pattern matches on the result and executes the appropriate action.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> okAction, Action<TError> errAction)
    {
        ThrowIfDefault();
        if (_isOk)
            okAction(_value!);
        else
            errAction(_error!);
    }

    /// <summary>
    /// Pattern matches on the result and returns a result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> okFunc, Func<TError, U> errFunc)
    {
        ThrowIfDefault();
        return _isOk ? okFunc(_value!) : errFunc(_error!);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Result<T, TError> other)
    {
        // Two uninitialized Results are considered equal
        if (!_isInitialized && !other._isInitialized)
            return true;

        // An uninitialized Result is not equal to an initialized one
        if (_isInitialized != other._isInitialized)
            return false;

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
        if (!_isInitialized)
            return 0;
        return _isOk ? _value?.GetHashCode() ?? 0 : _error?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Compares this Result to another Result.
    /// Err is considered less than Ok. When both are Ok, the values are compared.
    /// When both are Err, the errors are compared.
    /// </summary>
    /// <param name="other">The other Result to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    /// <exception cref="InvalidOperationException">Thrown if either Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Result<T, TError> other)
    {
        ThrowIfDefault();
        other.ThrowIfDefault();
        if (_isOk && other._isOk)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isOk && !other._isOk)
            return Comparer<TError>.Default.Compare(_error, other._error);
        return _isOk ? 1 : -1;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!_isInitialized)
            return "Uninitialized (default struct)";
        return _isOk ? $"Ok({_value})" : $"Err({_error})";
    }

    /// <summary>
    /// Converts the Result to an enumerable sequence.
    /// Returns a sequence containing the value if Ok, or an empty sequence if Err.
    /// </summary>
    /// <returns>An enumerable containing zero or one element.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        if (_isOk)
            yield return _value!;
    }

    /// <summary>
    /// Converts the Result to an array.
    /// Returns an array containing the value if Ok, or an empty array if Err.
    /// </summary>
    /// <returns>An array containing zero or one element.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        ThrowIfDefault();
        return _isOk ? new[] { _value! } : Array.Empty<T>();
    }

    /// <summary>
    /// Converts the Result to a list.
    /// Returns a list containing the value if Ok, or an empty list if Err.
    /// </summary>
    /// <returns>A list containing zero or one element.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        ThrowIfDefault();
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
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
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
        ThrowIfDefault();
        value = _value;
        isOk = _isOk;
    }

    /// <summary>
    /// Deconstructs the Result into all its components for pattern matching.
    /// </summary>
    /// <param name="value">The success value, or default if Err.</param>
    /// <param name="error">The error value, or default if Ok.</param>
    /// <param name="isOk">True if the Result is Ok.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Result is a default-constructed struct.</exception>
    /// <example>
    /// <code>
    /// var (value, error, isOk) = result;
    /// Console.WriteLine(isOk ? $"Value: {value}" : $"Error: {error}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out TError? error, out bool isOk)
    {
        ThrowIfDefault();
        value = _value;
        error = _error;
        isOk = _isOk;
    }
}

/// <summary>
/// Extension methods for Result&lt;T, TError&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ResultExtensions
{
    /// <summary>
    /// Flattens a nested Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Flatten<T, TError>(this Result<Result<T, TError>, TError> result)
    {
        return result.Bind(static inner => inner);
    }

    /// <summary>
    /// Transposes a Result of an Option into an Option of a Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Result<T, TError>> Transpose<T, TError>(this Result<Option<T>, TError> result)
    {
        return result.Match(
            okFunc: static option => option.Match(
                someFunc: static value => Option<Result<T, TError>>.Some(Result<T, TError>.Ok(value)),
                noneFunc: static () => Option<Result<T, TError>>.None()
            ),
            errFunc: static err => Option<Result<T, TError>>.Some(Result<T, TError>.Error(err))
        );
    }

    /// <summary>
    /// Executes an action if the result is Ok, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Tap<T, TError>(this Result<T, TError> result, Action<T> action)
    {
        if (result.IsOk)
            action(result.GetValue());

        return result;
    }

    /// <summary>
    /// Executes an action if the result is Error, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> TapError<T, TError>(this Result<T, TError> result, Action<TError> action)
    {
        if (result.IsError)
            action(result.GetError());

        return result;
    }

    /// <summary>
    /// Returns the contained Ok value if successful, otherwise throws the specified exception.
    /// This is an alternative to Expect that allows throwing specific exception types.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The source Result.</param>
    /// <param name="exception">The exception to throw if Error.</param>
    /// <returns>The contained Ok value if successful.</returns>
    /// <exception cref="Exception">Throws the specified exception if Error.</exception>
    /// <example>
    /// <code>
    /// var ok = Result&lt;User, string&gt;.Ok(user);
    /// var value = ok.ThrowIfError(new UserNotFoundException()); // returns user
    /// 
    /// var err = Result&lt;User, string&gt;.Error("not found");
    /// err.ThrowIfError(new UserNotFoundException()); // throws UserNotFoundException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfError<T, TError>(this Result<T, TError> result, Exception exception)
    {
        ThrowHelper.ThrowIfNull(exception);

        if (result.IsError)
            throw exception;

        return result.GetValue();
    }

    /// <summary>
    /// Returns the contained Ok value if successful, otherwise throws an exception created by the factory.
    /// The factory receives the error value and is only called if the Result is Error.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The source Result.</param>
    /// <param name="exceptionFactory">The factory function to create the exception from the error.</param>
    /// <returns>The contained Ok value if successful.</returns>
    /// <exception cref="Exception">Throws the exception from the factory if Error.</exception>
    /// <example>
    /// <code>
    /// var result = GetUser(id).ThrowIfError(err => new UserNotFoundException($"User not found: {err}"));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfError<T, TError>(this Result<T, TError> result, Func<TError, Exception> exceptionFactory)
    {
        ThrowHelper.ThrowIfNull(exceptionFactory);

        if (result.IsError)
            throw exceptionFactory(result.GetError());

        return result.GetValue();
    }

    /// <summary>
    /// Wraps a function that may throw an exception into a Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, Exception> Try<T>(Func<T> func)
    {
        try
        {
            return Result<T, Exception>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Error(ex);
        }
    }

    /// <summary>
    /// Wraps an async function that may throw an exception into a Result.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Ok with the result, or Err with the exception.</returns>
    public static async Task<Result<T, Exception>> TryAsync<T>(
        Func<Task<T>> func,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return Result<T, Exception>.Ok(await func().ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Error(ex);
        }
    }

    /// <summary>
    /// Combines two Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = Result.Combine(
    ///     GetUser(id),
    ///     GetOrder(orderId)
    /// ); // Result&lt;(User, Order), Error&gt;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2), TError> Combine<T1, T2, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second)
    {
        if (first.IsError)
            return Result<(T1, T2), TError>.Error(first.GetError());
        if (second.IsError)
            return Result<(T1, T2), TError>.Error(second.GetError());
        return Result<(T1, T2), TError>.Ok((first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines two Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = Result.Combine(
    ///     GetUser(id),
    ///     GetOrder(orderId),
    ///     (user, order) => new UserOrder(user, order)
    /// );
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Combine<T1, T2, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Func<T1, T2, TResult> combiner)
    {
        if (first.IsError)
            return Result<TResult, TError>.Error(first.GetError());
        if (second.IsError)
            return Result<TResult, TError>.Error(second.GetError());
        return Result<TResult, TError>.Ok(combiner(first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines three Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3), TError> Combine<T1, T2, T3, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third)
    {
        if (first.IsError)
            return Result<(T1, T2, T3), TError>.Error(first.GetError());
        if (second.IsError)
            return Result<(T1, T2, T3), TError>.Error(second.GetError());
        if (third.IsError)
            return Result<(T1, T2, T3), TError>.Error(third.GetError());
        return Result<(T1, T2, T3), TError>.Ok((first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines three Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Combine<T1, T2, T3, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        if (first.IsError)
            return Result<TResult, TError>.Error(first.GetError());
        if (second.IsError)
            return Result<TResult, TError>.Error(second.GetError());
        if (third.IsError)
            return Result<TResult, TError>.Error(third.GetError());
        return Result<TResult, TError>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines four Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3, T4), TError> Combine<T1, T2, T3, T4, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Result<T4, TError> fourth)
    {
        if (first.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(first.GetError());
        if (second.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(second.GetError());
        if (third.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(third.GetError());
        if (fourth.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(fourth.GetError());
        return Result<(T1, T2, T3, T4), TError>.Ok((first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines four Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Combine<T1, T2, T3, T4, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Result<T4, TError> fourth,
        Func<T1, T2, T3, T4, TResult> combiner)
    {
        if (first.IsError)
            return Result<TResult, TError>.Error(first.GetError());
        if (second.IsError)
            return Result<TResult, TError>.Error(second.GetError());
        if (third.IsError)
            return Result<TResult, TError>.Error(third.GetError());
        if (fourth.IsError)
            return Result<TResult, TError>.Error(fourth.GetError());
        return Result<TResult, TError>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines a collection of Results into a single Result containing a list.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var userIds = new[] { 1, 2, 3 };
    /// var usersResult = Result.Combine(userIds.Select(GetUser));
    /// // Result&lt;IReadOnlyList&lt;User&gt;, Error&gt;
    /// </code>
    /// </example>
    public static Result<IReadOnlyList<T>, TError> Combine<T, TError>(IEnumerable<Result<T, TError>> results)
    {
        var list = new List<T>();
        foreach (var result in results)
        {
            if (result.IsError)
                return Result<IReadOnlyList<T>, TError>.Error(result.GetError());
            list.Add(result.GetValue());
        }
        return Result<IReadOnlyList<T>, TError>.Ok(list);
    }

    /// <summary>
    /// Combines a collection of Results into a single Result, ignoring the values.
    /// Useful when you only care about success/failure, not the values.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var validations = new[] { ValidateA(), ValidateB(), ValidateC() };
    /// var allValid = Result.CombineAll(validations);
    /// // Result&lt;Unit, Error&gt;
    /// </code>
    /// </example>
    public static Result<Unit, TError> CombineAll<T, TError>(IEnumerable<Result<T, TError>> results)
    {
        foreach (var result in results)
        {
            if (result.IsError)
                return Result<Unit, TError>.Error(result.GetError());
        }
        return Result<Unit, TError>.Ok(Unit.Value);
    }

    #region Async Combine

    /// <summary>
    /// Asynchronously combines two Result tasks into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = await Result.CombineAsync(
    ///     GetUserAsync(id),
    ///     GetOrderAsync(orderId)
    /// ); // Result&lt;(User, Order), Error&gt;
    /// </code>
    /// </example>
    public static async Task<Result<(T1, T2), TError>> CombineAsync<T1, T2, TError>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        return Combine(result1, result2);
    }

    /// <summary>
    /// Asynchronously combines two Result tasks using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = await Result.CombineAsync(
    ///     GetUserAsync(id),
    ///     GetOrderAsync(orderId),
    ///     (user, order) => new UserOrder(user, order)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<TResult, TError>> CombineAsync<T1, T2, TError, TResult>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        Func<T1, T2, TResult> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        return Combine(result1, result2, combiner);
    }

    /// <summary>
    /// Asynchronously combines three Result tasks into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<(T1, T2, T3), TError>> CombineAsync<T1, T2, T3, TError>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third,
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
        return Combine(result1, result2, result3);
    }

    /// <summary>
    /// Asynchronously combines three Result tasks using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<TResult, TError>> CombineAsync<T1, T2, T3, TError, TResult>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third,
        Func<T1, T2, T3, TResult> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result3 = await third.ConfigureAwait(false);
        return Combine(result1, result2, result3, combiner);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks into a single Result containing a list.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var userIds = new[] { 1, 2, 3 };
    /// var usersResult = await ResultExtensions.CombineAsync(
    ///     userIds.Select(id => GetUserAsync(id))
    /// );
    /// // Result&lt;IReadOnlyList&lt;User&gt;, Error&gt;
    /// </code>
    /// </example>
    public static async Task<Result<IReadOnlyList<T>, TError>> CombineAsync<T, TError>(
        IEnumerable<Task<Result<T, TError>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return Combine(results);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks, ignoring the values.
    /// Useful when you only care about success/failure, not the values.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<Unit, TError>> CombineAllAsync<T, TError>(
        IEnumerable<Task<Result<T, TError>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return CombineAll(results);
    }

    #endregion

    #region Error Aggregation (CombineErrors)

    /// <summary>
    /// Combines two Results, accumulating ALL errors from both if either/both fail.
    /// Unlike <see cref="Combine{T1,T2,TError}"/> which returns the first error,
    /// this method collects all errors like Validation does.
    /// </summary>
    /// <example>
    /// <code>
    /// var name = ValidateName(input);  // Result&lt;string, Error&gt;
    /// var age = ValidateAge(input);    // Result&lt;int, Error&gt;
    /// var combined = ResultExtensions.CombineErrors(name, age);
    /// // Result&lt;(string, int), IReadOnlyList&lt;Error&gt;&gt; - contains ALL errors if any failed
    /// </code>
    /// </example>
    public static Result<(T1, T2), IReadOnlyList<TError>> CombineErrors<T1, T2, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second)
    {
        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());

        if (errors.Count > 0)
            return Result<(T1, T2), IReadOnlyList<TError>>.Error(errors);

        return Result<(T1, T2), IReadOnlyList<TError>>.Ok((first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines two Results with a combiner function, accumulating ALL errors from both if either/both fail.
    /// </summary>
    public static Result<TResult, IReadOnlyList<TError>> CombineErrors<T1, T2, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Func<T1, T2, TResult> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());

        if (errors.Count > 0)
            return Result<TResult, IReadOnlyList<TError>>.Error(errors);

        return Result<TResult, IReadOnlyList<TError>>.Ok(combiner(first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines three Results, accumulating ALL errors from all if any fail.
    /// </summary>
    public static Result<(T1, T2, T3), IReadOnlyList<TError>> CombineErrors<T1, T2, T3, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third)
    {
        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());
        if (third.IsError)
            errors.Add(third.GetError());

        if (errors.Count > 0)
            return Result<(T1, T2, T3), IReadOnlyList<TError>>.Error(errors);

        return Result<(T1, T2, T3), IReadOnlyList<TError>>.Ok((first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines three Results with a combiner function, accumulating ALL errors from all if any fail.
    /// </summary>
    public static Result<TResult, IReadOnlyList<TError>> CombineErrors<T1, T2, T3, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());
        if (third.IsError)
            errors.Add(third.GetError());

        if (errors.Count > 0)
            return Result<TResult, IReadOnlyList<TError>>.Error(errors);

        return Result<TResult, IReadOnlyList<TError>>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines four Results, accumulating ALL errors from all if any fail.
    /// </summary>
    public static Result<(T1, T2, T3, T4), IReadOnlyList<TError>> CombineErrors<T1, T2, T3, T4, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Result<T4, TError> fourth)
    {
        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());
        if (third.IsError)
            errors.Add(third.GetError());
        if (fourth.IsError)
            errors.Add(fourth.GetError());

        if (errors.Count > 0)
            return Result<(T1, T2, T3, T4), IReadOnlyList<TError>>.Error(errors);

        return Result<(T1, T2, T3, T4), IReadOnlyList<TError>>.Ok((
            first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines a collection of Results, accumulating ALL errors from all if any fail.
    /// </summary>
    /// <example>
    /// <code>
    /// var validations = items.Select(ValidateItem);
    /// var combined = ResultExtensions.CombineErrors(validations);
    /// // Result&lt;IReadOnlyList&lt;T&gt;, IReadOnlyList&lt;Error&gt;&gt;
    /// </code>
    /// </example>
    public static Result<IReadOnlyList<T>, IReadOnlyList<TError>> CombineErrors<T, TError>(
        IEnumerable<Result<T, TError>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        var values = new List<T>();
        var errors = new List<TError>();

        foreach (var result in results)
        {
            if (result.IsOk)
                values.Add(result.GetValue());
            else
                errors.Add(result.GetError());
        }

        if (errors.Count > 0)
            return Result<IReadOnlyList<T>, IReadOnlyList<TError>>.Error(errors);

        return Result<IReadOnlyList<T>, IReadOnlyList<TError>>.Ok(values);
    }

    /// <summary>
    /// Asynchronously combines two Result tasks, accumulating ALL errors from both if either/both fail.
    /// </summary>
    public static async Task<Result<(T1, T2), IReadOnlyList<TError>>> CombineErrorsAsync<T1, T2, TError>(
        Task<Result<T1, TError>> first,
        Task<Result<T2, TError>> second,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);

        return CombineErrors(result1, result2);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks, accumulating ALL errors from all if any fail.
    /// </summary>
    public static async Task<Result<IReadOnlyList<T>, IReadOnlyList<TError>>> CombineErrorsAsync<T, TError>(
        IEnumerable<Task<Result<T, TError>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(resultTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return CombineErrors(results);
    }

    #endregion
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
