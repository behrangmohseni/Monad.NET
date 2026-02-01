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
/// For typed errors without exceptions, prefer <see cref="Result{T,TErr}"/>.
/// For simple presence/absence, use <see cref="Option{T}"/>.
/// </para>
/// </remarks>
/// <seealso cref="Result{T,TErr}"/>
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
    private readonly bool _isInitialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isInitialized 
        ? (_isSuccess ? $"Success({_value})" : $"Failure({_exception?.GetType().Name}: {_exception?.Message})") 
        : "Uninitialized";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Try(T value, Exception? exception, bool isSuccess)
    {
        _value = value;
        _exception = exception;
        _isSuccess = isSuccess;
        _isInitialized = true;
    }

    /// <summary>
    /// Indicates whether the Try was properly initialized via factory methods.
    /// A default-constructed Try (e.g., default(Try&lt;T&gt;)) is not initialized.
    /// Always create Try instances via <see cref="Success(T)"/>, <see cref="Failure(Exception)"/>, or <see cref="Of(Func{T})"/> factory methods.
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
            ThrowHelper.ThrowTryIsDefault();
    }

    /// <summary>
    /// Returns true if the computation succeeded.
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _isSuccess;
        }
    }

    /// <summary>
    /// Returns true if the computation failed with an exception.
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return !_isSuccess;
        }
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
    /// Creates a successful Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Success(T value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Success with null value.");

        return new Try<T>(value, null, true);
    }

    /// <summary>
    /// Creates a failed Try with an exception.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Failure(Exception exception)
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
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Executes an async function and captures any exception.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Success with the result, or Failure with the exception.</returns>
    public static async Task<Try<T>> OfAsync(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Success(await func().ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Failure(ex);
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
            return Success(await func(cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Returns the value if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if failed or if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        ThrowIfDefault();
        if (!_isSuccess)
            ThrowHelper.ThrowTryIsFailure(_exception!);

        return _value!;
    }

    /// <summary>
    /// Returns the value if successful, or throws an <see cref="InvalidOperationException"/> if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if failed or if the Try was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var result = Try&lt;int&gt;.Success(42);
    /// var value = result.GetOrThrow(); // 42
    /// 
    /// var failure = Try&lt;int&gt;.Failure(new Exception("error"));
    /// failure.GetOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrThrow()
    {
        ThrowIfDefault();
        if (!_isSuccess)
            ThrowHelper.ThrowTryIsFailure(_exception!);

        return _value!;
    }

    /// <summary>
    /// Returns the exception if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful or if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Exception GetException()
    {
        ThrowIfDefault();
        if (_isSuccess)
            ThrowHelper.ThrowTryIsSuccess(_value!);

        return _exception!;
    }

    /// <summary>
    /// Returns the exception if failed, or throws an <see cref="InvalidOperationException"/> if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful or if the Try was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var failure = Try&lt;int&gt;.Failure(new Exception("error"));
    /// var ex = failure.GetExceptionOrThrow(); // Exception
    /// 
    /// var success = Try&lt;int&gt;.Success(42);
    /// success.GetExceptionOrThrow(); // throws InvalidOperationException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Exception GetExceptionOrThrow()
    {
        ThrowIfDefault();
        if (_isSuccess)
            ThrowHelper.ThrowTryIsSuccess(_value!);

        return _exception!;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        ThrowIfDefault();
        return _isSuccess ? _value! : defaultValue;
    }

    /// <summary>
    /// Tries to get the contained value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if successful; otherwise, the default value.</param>
    /// <returns>True if the Try is successful; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
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
        ThrowIfDefault();
        value = _value;
        return _isSuccess;
    }

    /// <summary>
    /// Tries to get the contained exception using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="exception">When this method returns, contains the exception if failed; otherwise, null.</param>
    /// <returns>True if the Try is a failure; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
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
        ThrowIfDefault();
        exception = _exception;
        return !_isSuccess;
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
    /// var result = Try&lt;int&gt;.Success(42);
    /// result.Contains(42); // true
    /// result.Contains(0);  // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value)
    {
        ThrowIfDefault();
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
    /// var result = Try&lt;int&gt;.Success(42);
    /// result.Exists(x => x > 40); // true
    /// result.Exists(x => x > 50); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowIfDefault();
        return _isSuccess && predicate(_value!);
    }

    /// <summary>
    /// Maps the value if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<U> Map<U>(Func<T, U> mapper)
    {
        ThrowIfDefault();
        if (!_isSuccess)
            return Try<U>.Failure(_exception!);

        try
        {
            return Try<U>.Success(mapper(_value!));
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Chains another Try computation.
    /// This is the monadic bind operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<U> Bind<U>(Func<T, Try<U>> binder)
    {
        ThrowIfDefault();
        if (!_isSuccess)
            return Try<U>.Failure(_exception!);

        try
        {
            return binder(_value!);
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Combines this Try with another into a tuple.
    /// Returns the first failure encountered if either Try failed.
    /// </summary>
    /// <typeparam name="U">The type of the other value.</typeparam>
    /// <param name="other">The other Try to combine with.</param>
    /// <returns>A Try containing a tuple of both values, or the first exception.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this Try was not properly initialized.</exception>
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
        ThrowIfDefault();
        if (!_isSuccess)
            return Try<(T, U)>.Failure(_exception!);
        if (!other.IsOk)
            return Try<(T, U)>.Failure(other.GetException());
        return Try<(T, U)>.Success((_value!, other.GetValue()));
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
        ThrowIfDefault();
        if (!_isSuccess)
            return Try<V>.Failure(_exception!);
        if (!other.IsOk)
            return Try<V>.Failure(other.GetException());

        try
        {
            return Try<V>.Success(combiner(_value!, other.GetValue()));
        }
        catch (Exception ex)
        {
            return Try<V>.Failure(ex);
        }
    }

    /// <summary>
    /// Filters the value with a predicate. Returns Failure if predicate returns false.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Filter(Func<T, bool> predicate)
    {
        ThrowIfDefault();
        if (!_isSuccess)
            return this;

        try
        {
            return predicate(_value!)
                ? this
                : Failure(new InvalidOperationException("Predicate not satisfied"));
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Filters the value with a predicate. Returns Failure with custom message if predicate returns false.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Filter(Func<T, bool> predicate, string errorMessage)
    {
        ThrowIfDefault();
        if (!_isSuccess)
            return this;

        try
        {
            return predicate(_value!)
                ? this
                : Failure(new InvalidOperationException(errorMessage));
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Filters the value with a predicate. Returns Failure with custom exception if predicate returns false.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Filter(Func<T, bool> predicate, Func<Exception> exceptionFactory)
    {
        ThrowIfDefault();
        if (!_isSuccess)
            return this;

        try
        {
            return predicate(_value!)
                ? this
                : Failure(exceptionFactory());
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Recovers from failure by providing an alternative value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> Recover(Func<Exception, T> recovery)
    {
        ThrowIfDefault();
        if (_isSuccess)
            return this;

        try
        {
            return Success(recovery(_exception!));
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Recovers from failure by providing an alternative Try.
    /// Also known as RecoverWith for consistency with other monads.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Try<T> RecoverWith(Func<Exception, Try<T>> recovery)
    {
        ThrowIfDefault();
        if (_isSuccess)
            return this;

        try
        {
            return recovery(_exception!);
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Executes an action on success or failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> successAction, Action<Exception> failureAction)
    {
        ThrowIfDefault();
        if (_isSuccess)
            successAction(_value!);
        else
            failureAction(_exception!);
    }

    /// <summary>
    /// Pattern matches and returns a result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> successFunc, Func<Exception, U> failureFunc)
    {
        ThrowIfDefault();
        return _isSuccess ? successFunc(_value!) : failureFunc(_exception!);
    }

    /// <summary>
    /// Converts to an Option, discarding the exception if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        ThrowIfDefault();
        return _isSuccess ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts to a Result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, Exception> ToResult()
    {
        ThrowIfDefault();
        return _isSuccess
            ? Result<T, Exception>.Ok(_value!)
            : Result<T, Exception>.Err(_exception!);
    }

    /// <summary>
    /// Converts to a Result with a mapped error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> ToResult<TErr>(Func<Exception, TErr> errorMapper)
    {
        ThrowIfDefault();
        return _isSuccess
            ? Result<T, TErr>.Ok(_value!)
            : Result<T, TErr>.Err(errorMapper(_exception!));
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
    /// <exception cref="InvalidOperationException">Thrown if either Try was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Try<T> other)
    {
        ThrowIfDefault();
        other.ThrowIfDefault();
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
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
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
        ThrowIfDefault();
        value = _value;
        isSuccess = _isSuccess;
    }

    /// <summary>
    /// Deconstructs the Try into all its components for pattern matching.
    /// </summary>
    /// <param name="value">The success value, or default if Failure.</param>
    /// <param name="exception">The exception, or null if Success.</param>
    /// <param name="isSuccess">True if the computation succeeded.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Try was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var (value, exception, isSuccess) = tryResult;
    /// Console.WriteLine(isSuccess ? $"Value: {value}" : $"Error: {exception?.Message}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? value, out Exception? exception, out bool isSuccess)
    {
        ThrowIfDefault();
        value = _value;
        exception = _exception;
        isSuccess = _isSuccess;
    }
}

/// <summary>
/// Extension methods for Try&lt;T&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TryExtensions
{
    /// <summary>
    /// Executes an action on success, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Tap<T>(this Try<T> @try, Action<T> action)
    {
        if (@try.IsOk)
        {
            try
            {
                action(@try.GetValue());
            }
            catch (Exception ex)
            {
                return Try<T>.Failure(ex);
            }
        }

        return @try;
    }

    /// <summary>
    /// Executes an action on failure, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> TapFailure<T>(this Try<T> @try, Action<Exception> action)
    {
        if (@try.IsError)
            action(@try.GetException());

        return @try;
    }

    /// <summary>
    /// Flattens a nested Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Flatten<T>(this Try<Try<T>> @try)
    {
        return @try.Bind(static inner => inner);
    }

    /// <summary>
    /// Converts a Result to a Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> ToTry<T>(this Result<T, Exception> result)
    {
        return result.Match(
            okFunc: static value => Try<T>.Success(value),
            errFunc: static ex => Try<T>.Failure(ex)
        );
    }

    /// <summary>
    /// Maps the value with an async function.
    /// </summary>
    /// <param name="try">The try to map.</param>
    /// <param name="mapper">An async function to apply to the value if Success.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Success with the mapped value, or the original Failure.</returns>
    public static async Task<Try<U>> MapAsync<T, U>(
        this Try<T> @try,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Failure(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await mapper(@try.GetValue()).ConfigureAwait(false);
            return Try<U>.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    /// <param name="try">The try to chain.</param>
    /// <param name="binder">An async function that returns a new Try based on the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Success, otherwise the original Failure.</returns>
    public static async Task<Try<U>> BindAsync<T, U>(
        this Try<T> @try,
        Func<T, Task<Try<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Failure(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await binder(@try.GetValue()).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    // ============================================================================
    // CancellationToken Overloads
    // ============================================================================

    /// <summary>
    /// Maps the value with an async function with cancellation support.
    /// </summary>
    public static async Task<Try<U>> MapAsync<T, U>(
        this Try<T> @try,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Failure(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await mapper(@try.GetValue(), cancellationToken).ConfigureAwait(false);
            return Try<U>.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Chains an async operation with cancellation support.
    /// </summary>
    public static async Task<Try<U>> BindAsync<T, U>(
        this Try<T> @try,
        Func<T, CancellationToken, Task<Try<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        if (!@try.IsOk)
            return Try<U>.Failure(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await binder(@try.GetValue(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a Try in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Try<T>> AsValueTask<T>(this Try<T> @try)
        => new(@try);

    /// <summary>
    /// Maps the value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Try<U>> MapAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (tryTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(tryTask.Result.Map(mapper));
        }
        return Core(tryTask, mapper, cancellationToken);

        static async ValueTask<Try<U>> Core(ValueTask<Try<T>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Map(m);
        }
    }

    /// <summary>
    /// Maps the value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Try<U>> MapAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await tryTask.ConfigureAwait(false);
        if (!@try.IsOk)
            return Try<U>.Failure(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await mapper(@try.GetValue(), cancellationToken).ConfigureAwait(false);
            return Try<U>.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Chains a synchronous operation. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Try<U>> BindAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, Try<U>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        if (tryTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(tryTask.Result.Bind(binder));
        }
        return Core(tryTask, binder, cancellationToken);

        static async ValueTask<Try<U>> Core(ValueTask<Try<T>> t, Func<T, Try<U>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Bind(b);
        }
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Try<U>> BindAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, CancellationToken, ValueTask<Try<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var @try = await tryTask.ConfigureAwait(false);
        if (!@try.IsOk)
            return Try<U>.Failure(@try.GetException());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await binder(@try.GetValue(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, U>(
        this ValueTask<Try<T>> tryTask,
        Func<T, U> successFunc,
        Func<Exception, U> failureFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(successFunc);
        ThrowHelper.ThrowIfNull(failureFunc);
        if (tryTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(tryTask.Result.Match(successFunc, failureFunc));
        }
        return Core(tryTask, successFunc, failureFunc, cancellationToken);

        static async ValueTask<U> Core(ValueTask<Try<T>> t, Func<T, U> s, Func<Exception, U> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            return r.Match(s, f);
        }
    }

    #endregion
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
