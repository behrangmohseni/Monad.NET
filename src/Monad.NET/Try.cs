using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a computation that might throw an exception.
/// Simpler alternative to Result when you don't need specific error types.
/// All exceptions are captured and can be recovered from.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="Try{T}"/> to wrap code that throws exceptions, converting them to recoverable values.
/// This is useful for integrating with legacy code or external libraries that use exceptions.
/// </para>
/// <para>
/// For typed errors without exceptions, prefer <see cref="Result{T,TError}"/>.
/// For simple presence/absence, use <see cref="Option{T}"/>.
/// </para>
/// </remarks>
/// <seealso cref="Result{T,TError}"/>
/// <seealso cref="Option{T}"/>
/// <seealso cref="TryExtensions"/>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(TryDebugView<>))]
public readonly struct Try<T> : IEquatable<Try<T>>, IComparable<Try<T>>
{
    private readonly T? _value;
    private readonly Exception? _exception;
    private readonly bool _isSuccess;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isSuccess
        ? $"Success({_value})"
        : $"Failure({_exception?.GetType().Name}: {_exception?.Message})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Try(T value, Exception? exception, bool isSuccess)
    {
        _value = value;
        _exception = exception;
        _isSuccess = isSuccess;
    }

    /// <summary>
    /// Returns true if the computation succeeded.
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSuccess;
    }

    /// <summary>
    /// Returns true if the computation failed with an exception.
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isSuccess;
    }

    /// <summary>
    /// Gets the contained value for pattern matching. Returns the value if Success, default otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    /// <example>
    /// <code>
    /// var message = tryResult switch
    /// {
    ///     { IsOk: true, Value: var v } => $"Success: {v}",
    ///     { IsError: true, Exception: var e } => $"Failed: {e.Message}",
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
    /// Gets the contained exception for pattern matching. Returns the exception if Failure, null otherwise.
    /// Use with pattern matching in switch expressions.
    /// </summary>
    public Exception? Exception
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _exception;
    }

    /// <summary>
    /// Creates a successful (Ok) Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Ok(T value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Ok with null value.");

        return new Try<T>(value, null, true);
    }

    /// <summary>
    /// Creates a failed (Error) Try with an exception.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Error(Exception exception)
    {
        if (exception is null)
            ThrowHelper.ThrowArgumentNull(nameof(exception));

        return new Try<T>(default!, exception, false);
    }

    /// <summary>
    /// Executes a function and captures any exception.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Of(Func<T> func)
    {
        try
        {
            return Ok(func());
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Executes an async function and captures any exception.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Ok with the result, or Error with the exception.</returns>
    public static async Task<Try<T>> OfAsync(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Ok(await func().ConfigureAwait(false));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Executes an async function with cancellation support and captures any exception.
    /// </summary>
    public static async Task<Try<T>> OfAsync(Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Ok(await func(cancellationToken).ConfigureAwait(false));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Returns the value if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if failed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        if (!_isSuccess)
            ThrowHelper.ThrowTryIsFailure(_exception!);

        return _value!;
    }

    /// <summary>
    /// Returns the value if successful, or throws an <see cref="InvalidOperationException"/> if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if failed.</exception>
    /// <example>
    /// <code>
    /// var result = Try&lt;int&gt;.Ok(42);
    /// var value = result.GetOrThrow(); // 42
    /// 
    /// var failure = Try&lt;int&gt;.Error(new Exception("error"));
    /// failure.GetOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow()
    {
        if (!_isSuccess)
            ThrowHelper.ThrowTryIsFailure(_exception!);

        return _value!;
    }

    /// <summary>
    /// Returns the exception if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful or if the Try is default-constructed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Exception GetException()
    {
        if (_isSuccess)
            ThrowHelper.ThrowTryIsSuccess(_value!);
        if (_exception is null)
            ThrowHelper.ThrowInvalidOperation(
                "Cannot get exception from default-constructed Try. " +
                "Use Try<T>.Ok(value), Try<T>.Error(exception), or Try<T>.Of(func) to create a valid instance.");
        return _exception!;
    }

    /// <summary>
    /// Returns the exception if failed, or throws an <see cref="InvalidOperationException"/> if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful or if the Try is default-constructed.</exception>
    /// <example>
    /// <code>
    /// var failure = Try&lt;int&gt;.Error(new Exception("error"));
    /// var ex = failure.GetExceptionOrThrow(); // Exception
    /// 
    /// var success = Try&lt;int&gt;.Ok(42);
    /// success.GetExceptionOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Exception GetExceptionOrThrow()
    {
        if (_isSuccess)
            ThrowHelper.ThrowTryIsSuccess(_value!);
        if (_exception is null)
            ThrowHelper.ThrowInvalidOperation(
                "Cannot get exception from default-constructed Try. " +
                "Use Try<T>.Ok(value), Try<T>.Error(exception), or Try<T>.Of(func) to create a valid instance.");
        return _exception!;
    }

    /// <summary>
    /// Returns the error (exception) if failed.
    /// This is equivalent to <see cref="GetException()"/> and provides
    /// a consistent naming convention across monad types.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Exception GetError()
    {
        return GetException();
    }

    /// <summary>
    /// Returns the error (exception) if failed, or throws an <see cref="InvalidOperationException"/> if successful.
    /// This is equivalent to <see cref="GetExceptionOrThrow()"/> and provides
    /// a consistent naming convention across monad types.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Exception GetErrorOrThrow()
    {
        return GetExceptionOrThrow();
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        return _isSuccess ? _value! : defaultValue;
    }

    /// <summary>
    /// Tries to get the contained value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if successful; otherwise, the default value.</param>
    /// <returns>True if the Try is successful; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (tryResult.TryGet(out var value))
    /// {
    ///     Console.WriteLine($"Success: {value}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(out T? value)
    {
        value = _value;
        return _isSuccess;
    }

    /// <summary>
    /// Tries to get the contained exception using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="exception">When this method returns, contains the exception if failed; otherwise, null.</param>
    /// <returns>True if the Try is a failure; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (tryResult.TryGetException(out var ex))
    /// {
    ///     Console.WriteLine($"Failed: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetException(out Exception? exception)
    {
        exception = _exception;
        return !_isSuccess;
    }

    /// <summary>
    /// Tries to get the error (exception) using the TryGet pattern.
    /// This is equivalent to <see cref="TryGetException(out Exception?)"/> and provides
    /// a consistent naming convention across monad types.
    /// </summary>
    /// <param name="error">When this method returns, contains the exception if failed; otherwise, null.</param>
    /// <returns>True if the Try is a failure; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetError(out Exception? error)
    {
        return TryGetException(out error);
    }

    /// <summary>
    /// Returns true if the Try is Success and contains the specified value.
    /// Uses the default equality comparer for type T.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Try is Success and contains the specified value; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var result = Try&lt;int&gt;.Ok(42);
    /// result.Contains(42); // true
    /// result.Contains(0);  // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        return _isSuccess && EqualityComparer<T>.Default.Equals(_value, value);
    }

    /// <summary>
    /// Returns true if the Try is Success and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Try is Success and the predicate returns true; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var result = Try&lt;int&gt;.Ok(42);
    /// result.Exists(x => x > 40); // true
    /// result.Exists(x => x > 50); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isSuccess && predicate(_value!);
    }

    /// <summary>
    /// Maps the value if successful.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<U> Map<U>(Func<T, U> mapper)
    {
        if (!_isSuccess)
            return Try<U>.Error(_exception!);

        try
        {
            return Try<U>.Ok(mapper(_value!));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Try<U>.Error(ex);
        }
    }

    /// <summary>
    /// Chains another Try computation.
    /// This is the monadic bind operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<U> Bind<U>(Func<T, Try<U>> binder)
    {
        if (!_isSuccess)
            return Try<U>.Error(_exception!);

        try
        {
            return binder(_value!);
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Try<U>.Error(ex);
        }
    }

    /// <summary>
    /// Combines this Try with another into a tuple.
    /// Returns the first failure encountered if either Try failed.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Try to combine with.</param>
    /// <returns>A Try containing a tuple of both values, or the first exception.</returns>
    /// <example>
    /// <code>
    /// var parsed1 = Try&lt;int&gt;.Of(() => int.Parse("42"));
    /// var parsed2 = Try&lt;int&gt;.Of(() => int.Parse("100"));
    /// var combined = parsed1.Zip(parsed2); // Success((42, 100))
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<(T, U)> Zip<U>(Try<U> other)
    {
        if (!_isSuccess)
            return Try<(T, U)>.Error(_exception!);
        if (!other.IsOk)
            return Try<(T, U)>.Error(other.GetException());
        return Try<(T, U)>.Ok((_value!, other.GetValue()));
    }

    /// <summary>
    /// Combines this Try with another using a combiner function.
    /// Returns the first failure encountered if either Try failed.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="other">The other Try to combine with.</param>
    /// <param name="combiner">A function to combine the values.</param>
    /// <returns>A Try containing the combined result, or the first exception.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this Try was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var x = Try&lt;int&gt;.Of(() => int.Parse("10"));
    /// var y = Try&lt;int&gt;.Of(() => int.Parse("20"));
    /// var sum = x.ZipWith(y, (a, b) => a + b); // Success(30)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<V> ZipWith<U, V>(Try<U> other, Func<T, U, V> combiner)
    {
        if (!_isSuccess)
            return Try<V>.Error(_exception!);
        if (!other.IsOk)
            return Try<V>.Error(other.GetException());

        try
        {
            return Try<V>.Ok(combiner(_value!, other.GetValue()));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Try<V>.Error(ex);
        }
    }

    /// <summary>
    /// Filters the value with a predicate. Returns Failure if predicate returns false.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Filter(Func<T, bool> predicate)
    {
        if (!_isSuccess)
            return this;

        try
        {
            return predicate(_value!)
                ? this
                : Error(new InvalidOperationException("Predicate not satisfied"));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Filters the value with a predicate. Returns Failure with custom message if predicate returns false.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Filter(Func<T, bool> predicate, string errorMessage)
    {
        if (!_isSuccess)
            return this;

        try
        {
            return predicate(_value!)
                ? this
                : Error(new InvalidOperationException(errorMessage));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Filters the value with a predicate. Returns Failure with custom exception if predicate returns false.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Filter(Func<T, bool> predicate, Func<Exception> exceptionFactory)
    {
        if (!_isSuccess)
            return this;

        try
        {
            return predicate(_value!)
                ? this
                : Error(exceptionFactory());
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Recovers from failure by providing an alternative value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Recover(Func<Exception, T> recovery)
    {
        if (_isSuccess)
            return this;

        try
        {
            return Ok(recovery(_exception!));
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Recovers from failure by providing an alternative Try.
    /// Also known as RecoverWith for consistency with other monads.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> RecoverWith(Func<Exception, Try<T>> recovery)
    {
        if (_isSuccess)
            return this;

        try
        {
            return recovery(_exception!);
        }
        catch (Exception ex) when (!ThrowHelper.IsFatal(ex))
        {
            return Error(ex);
        }
    }

    /// <summary>
    /// Executes an action on success or failure.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> successAction, Action<Exception> failureAction)
    {
        if (_isSuccess)
            successAction(_value!);
        else
            failureAction(_exception!);
    }

    /// <summary>
    /// Pattern matches and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> successFunc, Func<Exception, U> failureFunc)
    {
        return _isSuccess ? successFunc(_value!) : failureFunc(_exception!);
    }

    /// <summary>
    /// Converts to an Option, discarding the exception if failed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        return _isSuccess ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts to a Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, Exception> ToResult()
    {
        return _isSuccess
            ? Result<T, Exception>.Ok(_value!)
            : Result<T, Exception>.Error(_exception!);
    }

    /// <summary>
    /// Converts to a Result with a mapped error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult<TError>(Func<Exception, TError> errorMapper)
    {
        return _isSuccess
            ? Result<T, TError>.Ok(_value!)
            : Result<T, TError>.Error(errorMapper(_exception!));
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Try<T> other)
    {
        if (_isSuccess != other._isSuccess)
            return false;

        if (_isSuccess)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        // Compare exception types and messages
        return _exception!.GetType() == other._exception!.GetType()
               && _exception.Message == other._exception.Message;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Try<T> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return _isSuccess
            ? HashCode.Combine(_isSuccess, _value)
            : HashCode.Combine(_isSuccess, _exception!.GetType(), _exception.Message);
    }

    /// <summary>
    /// Compares this Try to another Try.
    /// Failure is considered less than Success. When both are Success, the values are compared.
    /// When both are Failure, the exception messages are compared.
    /// </summary>
    /// <param name="other">The other Try to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Try<T> other)
    {
        if (_isSuccess && other._isSuccess)
            return Comparer<T>.Default.Compare(_value, other._value);
        if (!_isSuccess && !other._isSuccess)
            return string.Compare(_exception?.Message, other._exception?.Message, StringComparison.Ordinal);
        return _isSuccess ? 1 : -1;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isSuccess
            ? $"Success({_value})"
            : $"Failure({_exception!.GetType().Name}: {_exception.Message})";
    }

    /// <summary>
    /// Determines whether two Try instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Try<T> left, Try<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Try instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Try<T> left, Try<T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the Try into its components for pattern matching.
    /// </summary>
    /// <param name="value">The success value, or default if Failure.</param>
    /// <param name="isSuccess">True if the computation succeeded.</param>
    /// <example>
    /// <code>
    /// var (value, isSuccess) = tryResult;
    /// if (isSuccess)
    ///     Console.WriteLine($"Got: {value}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out bool isSuccess)
    {
        value = _value;
        isSuccess = _isSuccess;
    }

    /// <summary>
    /// Deconstructs the Try into all its components for pattern matching.
    /// </summary>
    /// <param name="value">The success value, or default if Failure.</param>
    /// <param name="exception">The exception, or null if Success.</param>
    /// <param name="isSuccess">True if the computation succeeded.</param>
    /// <example>
    /// <code>
    /// var (value, exception, isSuccess) = tryResult;
    /// Console.WriteLine(isSuccess ? $"Value: {value}" : $"Error: {exception?.Message}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out Exception? exception, out bool isSuccess)
    {
        value = _value;
        exception = _exception;
        isSuccess = _isSuccess;
    }
}

/// <summary>
/// Debug view proxy for <see cref="Try{T}"/> to provide a better debugging experience.
/// </summary>
internal sealed class TryDebugView<T>
{
    private readonly Try<T> _try;

    public TryDebugView(Try<T> @try)
    {
        _try = @try;
    }

    public bool IsSuccess => _try.IsOk;
    public bool IsFailure => _try.IsError;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Value => _try.IsOk ? _try.GetValue() : null;

    public Exception? Exception => _try.IsError ? _try.GetException() : null;
}
