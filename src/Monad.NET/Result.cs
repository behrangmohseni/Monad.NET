using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Result is a type that represents either success (Ok) or failure (Err).
/// This is inspired by Rust's Result&lt;T, E&gt; type.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
/// <typeparam name="TErr">The type of the error value</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="Result{T,TErr}"/> for operations that can fail with a specific error type.
/// This provides type-safe error handling without exceptions.
/// </para>
/// <para>
/// For simple presence/absence without error info, use <see cref="Option{T}"/>.
/// For validation with multiple accumulated errors, use <see cref="Validation{T,TErr}"/>.
/// For wrapping exception-throwing code, use <see cref="Try{T}"/>.
/// </para>
/// </remarks>
/// <seealso cref="Option{T}"/>
/// <seealso cref="Validation{T,TErr}"/>
/// <seealso cref="Try{T}"/>
/// <seealso cref="Either{TLeft,TRight}"/>
/// <seealso cref="ResultExtensions"/>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(ResultDebugView<,>))]
public readonly struct Result<T, TErr> : IEquatable<Result<T, TErr>>, IComparable<Result<T, TErr>>, IComparable
{
    private readonly T? _value;
    private readonly TErr? _error;
    private readonly bool _isOk;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isOk ? $"Ok({_value})" : $"Err({_error})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Result(T value, TErr error, bool isOk)
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
    /// Returns true if the result is Err.
    /// </summary>
    public bool IsErr
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isOk;
    }

    /// <summary>
    /// Creates an Ok result containing the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Ok(T value)
    {
        if (value is null)
            ThrowHelper.ThrowCannotCreateOkWithNull();

        return new Result<T, TErr>(value, default!, true);
    }

    /// <summary>
    /// Creates an Err result containing the specified error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Err(TErr error)
    {
        if (error is null)
            ThrowHelper.ThrowCannotCreateErrWithNull();

        return new Result<T, TErr>(default!, error, false);
    }

    /// <summary>
    /// Returns the contained Ok value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Err</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue()
    {
        if (!_isOk)
            ThrowHelper.ThrowResultIsErr(_error!);

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TErr GetError()
    {
        if (_isOk)
            ThrowHelper.ThrowResultIsOk(_value!);

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
    /// Returns the contained Ok value or computes it from the error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrElse(Func<TErr, T> op)
    {
        return _isOk ? _value! : op(_error!);
    }

    /// <summary>
    /// Returns the contained Ok value or a default value of type T.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetValueOrDefault()
    {
        return _isOk ? _value : default;
    }

    /// <summary>
    /// Returns the contained Ok value, or throws an <see cref="InvalidOperationException"/> if Err.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Err</exception>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// var value = result.GetOrThrow(); // 42
    /// 
    /// var error = Result&lt;int, string&gt;.Err("failed");
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
    /// Returns the contained Ok value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if Err.
    /// </summary>
    /// <param name="message">The exception message if Err</param>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Err</exception>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// var value = result.GetOrThrow("Expected success"); // 42
    /// 
    /// var error = Result&lt;int, string&gt;.Err("failed");
    /// error.GetOrThrow("Operation must succeed"); // throws with "Operation must succeed: failed"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow(string message)
    {
        if (!_isOk)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_error}");

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value, or throws an <see cref="InvalidOperationException"/> if Ok.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Ok</exception>
    /// <example>
    /// <code>
    /// var error = Result&lt;int, string&gt;.Err("failed");
    /// var err = error.GetErrorOrThrow(); // "failed"
    /// 
    /// var success = Result&lt;int, string&gt;.Ok(42);
    /// success.GetErrorOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TErr GetErrorOrThrow()
    {
        if (_isOk)
            ThrowHelper.ThrowResultIsOk(_value!);

        return _error!;
    }

    /// <summary>
    /// Returns the contained Err value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if Ok.
    /// </summary>
    /// <param name="message">The exception message if Ok</param>
    /// <exception cref="InvalidOperationException">Thrown if the Result is Ok</exception>
    /// <example>
    /// <code>
    /// var error = Result&lt;int, string&gt;.Err("failed");
    /// var err = error.GetErrorOrThrow("Expected failure"); // "failed"
    /// 
    /// var success = Result&lt;int, string&gt;.Ok(42);
    /// success.GetErrorOrThrow("Should have failed"); // throws with "Should have failed: 42"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TErr GetErrorOrThrow(string message)
    {
        if (_isOk)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_value}");

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
    public bool TryGetError(out TErr? error)
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
    /// Result&lt;int, string&gt;.Err("error").Contains(42); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        return _isOk && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Result is Err and contains the specified error.
    /// Uses the default equality comparer for type TErr.
    /// </summary>
    /// <param name="error">The error to check for.</param>
    /// <returns>True if the Result is Err and contains the specified error; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Err("not found");
    /// result.ContainsError("not found"); // true
    /// result.ContainsError("other");     // false
    /// Result&lt;int, string&gt;.Ok(42).ContainsError("not found"); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsError(TErr error)
    {
        return !_isOk && EqualityComparer<TErr>.Default.Equals(_error, error);
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
    /// Result&lt;int, string&gt;.Err("error").Exists(x => x > 0); // false
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
    /// var result = Result&lt;int, string&gt;.Err("not found");
    /// result.ExistsError(e => e.Contains("not")); // true
    /// result.ExistsError(e => e.Contains("xyz")); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistsError(Func<TErr, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return !_isOk && predicate(_error!);
    }

    /// <summary>
    /// Maps a Result&lt;T, TErr&gt; to Result&lt;U, TErr&gt; by applying a function to a contained Ok value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TErr> Map<U>(Func<T, U> mapper)
    {
        return _isOk ? Result<U, TErr>.Ok(mapper(_value!)) : Result<U, TErr>.Err(_error!);
    }

    /// <summary>
    /// Maps a Result&lt;T, TErr&gt; to Result&lt;T, F&gt; by applying a function to a contained Err value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, F> MapError<F>(Func<TErr, F> mapper)
    {
        return _isOk ? Result<T, F>.Ok(_value!) : Result<T, F>.Err(mapper(_error!));
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
    /// var error = Result&lt;int, string&gt;.Err("not found");
    /// var mappedError = error.BiMap(
    ///     x => x.ToString(),
    ///     e => new Error(e)
    /// ); // Result&lt;string, Error&gt;.Err(Error("not found"))
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, F> BiMap<U, F>(Func<T, U> okMapper, Func<TErr, F> errMapper)
    {
        ThrowHelper.ThrowIfNull(okMapper);
        ThrowHelper.ThrowIfNull(errMapper);

        return _isOk
            ? Result<U, F>.Ok(okMapper(_value!))
            : Result<U, F>.Err(errMapper(_error!));
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
    /// Maps a Result&lt;T, TErr&gt; to U by applying a function to a contained Ok value, or a fallback function to a contained Err value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U MapOrElse<U>(Func<TErr, U> defaultFunc, Func<T, U> mapper)
    {
        return _isOk ? mapper(_value!) : defaultFunc(_error!);
    }

    /// <summary>
    /// Filters the Ok value based on a predicate. Returns Err if the predicate returns false.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="error">The error to return if the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; otherwise Err.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// result.Filter(x => x > 40, "Value too small"); // Ok(42)
    /// result.Filter(x => x > 50, "Value too small"); // Err("Value too small")
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> Filter(Func<T, bool> predicate, TErr error)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isOk && predicate(_value!) ? this : Err(error);
    }

    /// <summary>
    /// Filters the Ok value based on a predicate. Returns Err if the predicate returns false.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="errorFactory">A function that creates the error if the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; otherwise Err.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// result.Filter(x => x > 40, () => "Value too small"); // Ok(42)
    /// result.Filter(x => x > 50, () => $"Value {x} is too small"); // Err("Value 42 is too small")
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> Filter(Func<T, bool> predicate, Func<TErr> errorFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(errorFactory);
        return _isOk && predicate(_value!) ? this : Err(errorFactory());
    }

    /// <summary>
    /// Filters the Ok value based on a predicate. Returns Err if the predicate returns false.
    /// The error factory receives the original value.
    /// </summary>
    /// <param name="predicate">The predicate to test the Ok value against.</param>
    /// <param name="errorFactory">A function that creates the error from the value if the predicate returns false.</param>
    /// <returns>The original Result if Ok and predicate is true; otherwise Err.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;int, string&gt;.Ok(42);
    /// result.Filter(x => x > 50, x => $"Value {x} is too small"); // Err("Value 42 is too small")
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> Filter(Func<T, bool> predicate, Func<T, TErr> errorFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(errorFactory);
        return _isOk && predicate(_value!) ? this : _isOk ? Err(errorFactory(_value!)) : this;
    }

    /// <summary>
    /// Calls the function if the result is Ok, otherwise returns the Err value.
    /// This is the monadic bind operation for control flow based on Result values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TErr> Bind<U>(Func<T, Result<U, TErr>> binder)
    {
        return _isOk ? binder(_value!) : Result<U, TErr>.Err(_error!);
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
    public Result<(T, U), TErr> Zip<U>(Result<U, TErr> other)
    {
        if (!_isOk)
            return Result<(T, U), TErr>.Err(_error!);
        if (!other.IsOk)
            return Result<(T, U), TErr>.Err(other.GetError());
        return Result<(T, U), TErr>.Ok((_value!, other.GetValue()));
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
    public Result<V, TErr> ZipWith<U, V>(Result<U, TErr> other, Func<T, U, V> combiner)
    {
        if (!_isOk)
            return Result<V, TErr>.Err(_error!);
        if (!other.IsOk)
            return Result<V, TErr>.Err(other.GetError());
        return Result<V, TErr>.Ok(combiner(_value!, other.GetValue()));
    }

    /// <summary>
    /// Returns resultB if the result is Ok, otherwise returns the Err value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TErr> And<U>(Result<U, TErr> resultB)
    {
        return _isOk ? resultB : Result<U, TErr>.Err(_error!);
    }

    /// <summary>
    /// Calls the function if the result is Err, otherwise returns the Ok value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, F> OrElse<F>(Func<TErr, Result<T, F>> op)
    {
        return _isOk ? Result<T, F>.Ok(_value!) : op(_error!);
    }

    /// <summary>
    /// Returns the result if it contains an Ok value, otherwise returns resultB.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> Or(Result<T, TErr> resultB)
    {
        return _isOk ? this : resultB;
    }

    /// <summary>
    /// Converts from Result&lt;T, TErr&gt; to Option&lt;T&gt;.
    /// Converts self into an Option&lt;T&gt;, consuming self, and discarding the error, if any.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Ok()
    {
        return _isOk ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts from Result&lt;T, TErr&gt; to Option&lt;TErr&gt;.
    /// Converts self into an Option&lt;TErr&gt;, consuming self, and discarding the success value, if any.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TErr> Err()
    {
        return _isOk ? Option<TErr>.None() : Option<TErr>.Some(_error!);
    }

    /// <summary>
    /// Pattern matches on the result and executes the appropriate action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> okAction, Action<TErr> errAction)
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
    public U Match<U>(Func<T, U> okFunc, Func<TErr, U> errFunc)
    {
        return _isOk ? okFunc(_value!) : errFunc(_error!);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Result<T, TErr> other)
    {
        if (_isOk != other._isOk)
            return false;

        if (_isOk)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        return EqualityComparer<TErr>.Default.Equals(_error, other._error);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Result<T, TErr> other && Equals(other);
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
    public int CompareTo(Result<T, TErr> other)
    {
        if (_isOk && other._isOk)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isOk && !other._isOk)
            return Comparer<TErr>.Default.Compare(_error, other._error);
        return _isOk ? 1 : -1;
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is Result<T, TErr> other)
            return CompareTo(other);
        ThrowHelper.ThrowArgument(nameof(obj), $"Object must be of type Result<{typeof(T).Name}, {typeof(TErr).Name}>");
        return 0; // unreachable
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
    /// var results = new[] { Result&lt;int, string&gt;.Ok(1), Result&lt;int, string&gt;.Err("error"), Result&lt;int, string&gt;.Ok(3) };
    /// var values = results.SelectMany(r => r.AsEnumerable()); // [1, 3]
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        return _isOk ? new List<T> { _value! } : new List<T>();
    }

    /// <summary>
    /// Determines whether two Result instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Result<T, TErr> left, Result<T, TErr> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Result instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Result<T, TErr> left, Result<T, TErr> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implicit conversion from T to Result&lt;T, TErr&gt; (Ok).
    /// Allows: Result&lt;int, string&gt; result = 42;
    /// </summary>
    /// <param name="value">The success value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TErr>(T value)
    {
        return Ok(value);
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
    public void Deconstruct(out T? value, out TErr? error, out bool isOk)
    {
        value = _value;
        error = _error;
        isOk = _isOk;
    }
}

/// <summary>
/// Extension methods for Result&lt;T, TErr&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ResultExtensions
{
    /// <summary>
    /// Flattens a nested Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Flatten<T, TErr>(this Result<Result<T, TErr>, TErr> result)
    {
        return result.Bind(static inner => inner);
    }

    /// <summary>
    /// Transposes a Result of an Option into an Option of a Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Result<T, TErr>> Transpose<T, TErr>(this Result<Option<T>, TErr> result)
    {
        return result.Match(
            okFunc: static option => option.Match(
                someFunc: static value => Option<Result<T, TErr>>.Some(Result<T, TErr>.Ok(value)),
                noneFunc: static () => Option<Result<T, TErr>>.None()
            ),
            errFunc: static err => Option<Result<T, TErr>>.Some(Result<T, TErr>.Err(err))
        );
    }

    /// <summary>
    /// Executes an action if the result is Ok, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Tap<T, TErr>(this Result<T, TErr> result, Action<T> action)
    {
        if (result.IsOk)
            action(result.GetValue());

        return result;
    }

    /// <summary>
    /// Executes an action if the result is Err, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> TapErr<T, TErr>(this Result<T, TErr> result, Action<TErr> action)
    {
        if (result.IsErr)
            action(result.GetError());

        return result;
    }

    /// <summary>
    /// Returns the contained Ok value if successful, otherwise throws the specified exception.
    /// This is an alternative to Expect that allows throwing specific exception types.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="result">The source Result.</param>
    /// <param name="exception">The exception to throw if Err.</param>
    /// <returns>The contained Ok value if successful.</returns>
    /// <exception cref="Exception">Throws the specified exception if Err.</exception>
    /// <example>
    /// <code>
    /// var ok = Result&lt;User, string&gt;.Ok(user);
    /// var value = ok.ThrowIfErr(new UserNotFoundException()); // returns user
    /// 
    /// var err = Result&lt;User, string&gt;.Err("not found");
    /// err.ThrowIfErr(new UserNotFoundException()); // throws UserNotFoundException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfErr<T, TErr>(this Result<T, TErr> result, Exception exception)
    {
        ThrowHelper.ThrowIfNull(exception);

        if (result.IsErr)
            throw exception;

        return result.GetValue();
    }

    /// <summary>
    /// Returns the contained Ok value if successful, otherwise throws an exception created by the factory.
    /// The factory receives the error value and is only called if the Result is Err.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="result">The source Result.</param>
    /// <param name="exceptionFactory">The factory function to create the exception from the error.</param>
    /// <returns>The contained Ok value if successful.</returns>
    /// <exception cref="Exception">Throws the exception from the factory if Err.</exception>
    /// <example>
    /// <code>
    /// var result = GetUser(id).ThrowIfErr(err => new UserNotFoundException($"User not found: {err}"));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfErr<T, TErr>(this Result<T, TErr> result, Func<TErr, Exception> exceptionFactory)
    {
        ThrowHelper.ThrowIfNull(exceptionFactory);

        if (result.IsErr)
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
            return Result<T, Exception>.Err(ex);
        }
    }

    /// <summary>
    /// Wraps an async function that may throw an exception into a Result.
    /// </summary>
    public static async Task<Result<T, Exception>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return Result<T, Exception>.Ok(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Err(ex);
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
    public static Result<(T1, T2), TErr> Combine<T1, T2, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second)
    {
        if (first.IsErr)
            return Result<(T1, T2), TErr>.Err(first.GetError());
        if (second.IsErr)
            return Result<(T1, T2), TErr>.Err(second.GetError());
        return Result<(T1, T2), TErr>.Ok((first.GetValue(), second.GetValue()));
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
    public static Result<TResult, TErr> Combine<T1, T2, TErr, TResult>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Func<T1, T2, TResult> combiner)
    {
        if (first.IsErr)
            return Result<TResult, TErr>.Err(first.GetError());
        if (second.IsErr)
            return Result<TResult, TErr>.Err(second.GetError());
        return Result<TResult, TErr>.Ok(combiner(first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines three Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3), TErr> Combine<T1, T2, T3, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third)
    {
        if (first.IsErr)
            return Result<(T1, T2, T3), TErr>.Err(first.GetError());
        if (second.IsErr)
            return Result<(T1, T2, T3), TErr>.Err(second.GetError());
        if (third.IsErr)
            return Result<(T1, T2, T3), TErr>.Err(third.GetError());
        return Result<(T1, T2, T3), TErr>.Ok((first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines three Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TErr> Combine<T1, T2, T3, TErr, TResult>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        if (first.IsErr)
            return Result<TResult, TErr>.Err(first.GetError());
        if (second.IsErr)
            return Result<TResult, TErr>.Err(second.GetError());
        if (third.IsErr)
            return Result<TResult, TErr>.Err(third.GetError());
        return Result<TResult, TErr>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines four Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3, T4), TErr> Combine<T1, T2, T3, T4, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third,
        Result<T4, TErr> fourth)
    {
        if (first.IsErr)
            return Result<(T1, T2, T3, T4), TErr>.Err(first.GetError());
        if (second.IsErr)
            return Result<(T1, T2, T3, T4), TErr>.Err(second.GetError());
        if (third.IsErr)
            return Result<(T1, T2, T3, T4), TErr>.Err(third.GetError());
        if (fourth.IsErr)
            return Result<(T1, T2, T3, T4), TErr>.Err(fourth.GetError());
        return Result<(T1, T2, T3, T4), TErr>.Ok((first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines four Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TErr> Combine<T1, T2, T3, T4, TErr, TResult>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third,
        Result<T4, TErr> fourth,
        Func<T1, T2, T3, T4, TResult> combiner)
    {
        if (first.IsErr)
            return Result<TResult, TErr>.Err(first.GetError());
        if (second.IsErr)
            return Result<TResult, TErr>.Err(second.GetError());
        if (third.IsErr)
            return Result<TResult, TErr>.Err(third.GetError());
        if (fourth.IsErr)
            return Result<TResult, TErr>.Err(fourth.GetError());
        return Result<TResult, TErr>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
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
    public static Result<IReadOnlyList<T>, TErr> Combine<T, TErr>(IEnumerable<Result<T, TErr>> results)
    {
        var list = new List<T>();
        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.GetError());
            list.Add(result.GetValue());
        }
        return Result<IReadOnlyList<T>, TErr>.Ok(list);
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
    public static Result<Unit, TErr> CombineAll<T, TErr>(IEnumerable<Result<T, TErr>> results)
    {
        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<Unit, TErr>.Err(result.GetError());
        }
        return Result<Unit, TErr>.Ok(Unit.Value);
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
    public static async Task<Result<(T1, T2), TErr>> CombineAsync<T1, T2, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);

        var (result1, result2) = (await first.ConfigureAwait(false), await second.ConfigureAwait(false));
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
    public static async Task<Result<TResult, TErr>> CombineAsync<T1, T2, TErr, TResult>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Func<T1, T2, TResult> combiner)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);

        var (result1, result2) = (await first.ConfigureAwait(false), await second.ConfigureAwait(false));
        return Combine(result1, result2, combiner);
    }

    /// <summary>
    /// Asynchronously combines three Result tasks into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<(T1, T2, T3), TErr>> CombineAsync<T1, T2, T3, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Task<Result<T3, TErr>> third)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);

        var result1 = await first.ConfigureAwait(false);
        var result2 = await second.ConfigureAwait(false);
        var result3 = await third.ConfigureAwait(false);
        return Combine(result1, result2, result3);
    }

    /// <summary>
    /// Asynchronously combines three Result tasks using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<TResult, TErr>> CombineAsync<T1, T2, T3, TErr, TResult>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Task<Result<T3, TErr>> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);
        ThrowHelper.ThrowIfNull(combiner);

        var result1 = await first.ConfigureAwait(false);
        var result2 = await second.ConfigureAwait(false);
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
    public static async Task<Result<IReadOnlyList<T>, TErr>> CombineAsync<T, TErr>(
        IEnumerable<Task<Result<T, TErr>>> resultTasks)
    {
        ThrowHelper.ThrowIfNull(resultTasks);

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        return Combine(results);
    }

    /// <summary>
    /// Asynchronously combines a collection of Result tasks, ignoring the values.
    /// Useful when you only care about success/failure, not the values.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    public static async Task<Result<Unit, TErr>> CombineAllAsync<T, TErr>(
        IEnumerable<Task<Result<T, TErr>>> resultTasks)
    {
        ThrowHelper.ThrowIfNull(resultTasks);

        var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
        return CombineAll(results);
    }

    #endregion
}

/// <summary>
/// Debug view proxy for <see cref="Result{T, TErr}"/> to provide a better debugging experience.
/// </summary>
internal sealed class ResultDebugView<T, TErr>
{
    private readonly Result<T, TErr> _result;

    public ResultDebugView(Result<T, TErr> result)
    {
        _result = result;
    }

    public bool IsOk => _result.IsOk;
    public bool IsErr => _result.IsErr;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Value => _result.IsOk ? _result.GetValue() : null;

    public object? Error => _result.IsErr ? _result.GetError() : null;
}
