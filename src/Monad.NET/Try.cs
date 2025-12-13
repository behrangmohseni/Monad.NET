namespace Monad.NET;

/// <summary>
/// Represents a computation that might throw an exception.
/// Simpler alternative to Result when you don't need specific error types.
/// All exceptions are captured and can be recovered from.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public readonly struct Try<T> : IEquatable<Try<T>>
{
    private readonly T? _value;
    private readonly Exception? _exception;
    private readonly bool _isSuccess;

    private Try(T value, Exception exception, bool isSuccess)
    {
        _value = value;
        _exception = exception;
        _isSuccess = isSuccess;
    }

    /// <summary>
    /// Returns true if the computation succeeded.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Returns true if the computation failed with an exception.
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Creates a successful Try.
    /// </summary>
    public static Try<T> Success(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create Success with null value.");
        
        return new Try<T>(value, null!, true);
    }

    /// <summary>
    /// Creates a failed Try with an exception.
    /// </summary>
    public static Try<T> Failure(Exception exception)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));
        
        return new Try<T>(default!, exception, false);
    }

    /// <summary>
    /// Executes a function and captures any exception.
    /// </summary>
    public static Try<T> Of(Func<T> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));

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
    public static async Task<Try<T>> OfAsync(Func<Task<T>> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));

        try
        {
            return Success(await func());
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Returns the value if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if failed</exception>
    public T Get()
    {
        if (!_isSuccess)
            throw new InvalidOperationException($"Cannot get value from failed Try: {_exception!.Message}", _exception);
        
        return _value!;
    }

    /// <summary>
    /// Returns the exception if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if successful</exception>
    public Exception GetException()
    {
        if (_isSuccess)
            throw new InvalidOperationException("Cannot get exception from successful Try");
        
        return _exception!;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value.
    /// </summary>
    public T GetOrElse(T defaultValue)
    {
        return _isSuccess ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the value if successful, otherwise computes a default.
    /// </summary>
    public T GetOrElse(Func<T> defaultFunc)
    {
        if (defaultFunc is null)
            throw new ArgumentNullException(nameof(defaultFunc));
        
        return _isSuccess ? _value! : defaultFunc();
    }

    /// <summary>
    /// Returns the value if successful, otherwise computes from the exception.
    /// </summary>
    public T GetOrElse(Func<Exception, T> recovery)
    {
        if (recovery is null)
            throw new ArgumentNullException(nameof(recovery));
        
        return _isSuccess ? _value! : recovery(_exception!);
    }

    /// <summary>
    /// Maps the value if successful.
    /// </summary>
    public Try<U> Map<U>(Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
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
    /// </summary>
    public Try<U> FlatMap<U>(Func<T, Try<U>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
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
    /// Filters the value with a predicate. Returns Failure if predicate returns false.
    /// </summary>
    public Try<T> Filter(Func<T, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

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
    public Try<T> Filter(Func<T, bool> predicate, string errorMessage)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        if (errorMessage is null)
            throw new ArgumentNullException(nameof(errorMessage));

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
    public Try<T> Filter(Func<T, bool> predicate, Func<Exception> exceptionFactory)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        if (exceptionFactory is null)
            throw new ArgumentNullException(nameof(exceptionFactory));

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
    public Try<T> Recover(Func<Exception, T> recovery)
    {
        if (recovery is null)
            throw new ArgumentNullException(nameof(recovery));

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
    /// Recovers from failure by providing an alternative Try (flattening the result).
    /// </summary>
    public Try<T> Recover(Func<Exception, Try<T>> recovery)
    {
        if (recovery is null)
            throw new ArgumentNullException(nameof(recovery));

        if (_isSuccess)
            return this;

        try
        {
            var result = recovery(_exception!);
            return result;
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Recovers from failure by providing an alternative Try.
    /// </summary>
    public Try<T> RecoverWith(Func<Exception, Try<T>> recovery)
    {
        if (recovery is null)
            throw new ArgumentNullException(nameof(recovery));

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
    public void Match(Action<T> successAction, Action<Exception> failureAction)
    {
        if (successAction is null)
            throw new ArgumentNullException(nameof(successAction));
        if (failureAction is null)
            throw new ArgumentNullException(nameof(failureAction));

        if (_isSuccess)
            successAction(_value!);
        else
            failureAction(_exception!);
    }

    /// <summary>
    /// Pattern matches and returns a result.
    /// </summary>
    public U Match<U>(Func<T, U> successFunc, Func<Exception, U> failureFunc)
    {
        if (successFunc is null)
            throw new ArgumentNullException(nameof(successFunc));
        if (failureFunc is null)
            throw new ArgumentNullException(nameof(failureFunc));

        return _isSuccess ? successFunc(_value!) : failureFunc(_exception!);
    }

    /// <summary>
    /// Converts to an Option, discarding the exception if failed.
    /// </summary>
    public Option<T> ToOption()
    {
        return _isSuccess ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts to a Result.
    /// </summary>
    public Result<T, Exception> ToResult()
    {
        return _isSuccess 
            ? Result<T, Exception>.Ok(_value!) 
            : Result<T, Exception>.Err(_exception!);
    }

    /// <summary>
    /// Converts to a Result with a mapped error.
    /// </summary>
    public Result<T, TErr> ToResult<TErr>(Func<Exception, TErr> errorMapper)
    {
        if (errorMapper is null)
            throw new ArgumentNullException(nameof(errorMapper));

        return _isSuccess 
            ? Result<T, TErr>.Ok(_value!) 
            : Result<T, TErr>.Err(errorMapper(_exception!));
    }

    /// <inheritdoc />
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
    public override bool Equals(object? obj)
    {
        return obj is Try<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _isSuccess 
            ? HashCode.Combine(_isSuccess, _value) 
            : HashCode.Combine(_isSuccess, _exception!.GetType(), _exception.Message);
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
    public static bool operator ==(Try<T> left, Try<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Try instances are not equal.
    /// </summary>
    public static bool operator !=(Try<T> left, Try<T> right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Extension methods for Try&lt;T&gt;.
/// </summary>
public static class TryExtensions
{
    /// <summary>
    /// Executes an action on success, allowing method chaining.
    /// </summary>
    public static Try<T> Tap<T>(this Try<T> @try, Action<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (@try.IsSuccess)
        {
            try
            {
                action(@try.Get());
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
    public static Try<T> TapFailure<T>(this Try<T> @try, Action<Exception> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (@try.IsFailure)
            action(@try.GetException());

        return @try;
    }

    /// <summary>
    /// Flattens a nested Try.
    /// </summary>
    public static Try<T> Flatten<T>(this Try<Try<T>> @try)
    {
        return @try.FlatMap(inner => inner);
    }

    /// <summary>
    /// Converts a Result to a Try.
    /// </summary>
    public static Try<T> ToTry<T>(this Result<T, Exception> result)
    {
        return result.Match(
            okFunc: value => Try<T>.Success(value),
            errFunc: ex => Try<T>.Failure(ex)
        );
    }

    /// <summary>
    /// Maps the value with an async function.
    /// </summary>
    public static async Task<Try<U>> MapAsync<T, U>(this Try<T> @try, Func<T, Task<U>> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        if (!@try.IsSuccess)
            return Try<U>.Failure(@try.GetException());

        try
        {
            var result = await mapper(@try.Get());
            return Try<U>.Success(result);
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    public static async Task<Try<U>> FlatMapAsync<T, U>(this Try<T> @try, Func<T, Task<Try<U>>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        if (!@try.IsSuccess)
            return Try<U>.Failure(@try.GetException());

        try
        {
            return await binder(@try.Get());
        }
        catch (Exception ex)
        {
            return Try<U>.Failure(ex);
        }
    }
}

