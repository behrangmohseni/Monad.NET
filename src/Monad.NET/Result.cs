namespace Monad.NET;

/// <summary>
/// Result is a type that represents either success (Ok) or failure (Err).
/// This is inspired by Rust's Result&lt;T, E&gt; type.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
/// <typeparam name="TErr">The type of the error value</typeparam>
public readonly struct Result<T, TErr> : IEquatable<Result<T, TErr>>
{
    private readonly T? _value;
    private readonly TErr? _error;
    private readonly bool _isOk;

    private Result(T value, TErr error, bool isOk)
    {
        _value = value;
        _error = error;
        _isOk = isOk;
    }

    /// <summary>
    /// Returns true if the result is Ok.
    /// </summary>
    public bool IsOk => _isOk;

    /// <summary>
    /// Returns true if the result is Err.
    /// </summary>
    public bool IsErr => !_isOk;

    /// <summary>
    /// Creates an Ok result containing the specified value.
    /// </summary>
    public static Result<T, TErr> Ok(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create Ok with null value.");
        
        return new Result<T, TErr>(value, default!, true);
    }

    /// <summary>
    /// Creates an Err result containing the specified error.
    /// </summary>
    public static Result<T, TErr> Err(TErr error)
    {
        if (error is null)
            throw new ArgumentNullException(nameof(error), "Cannot create Err with null error.");
        
        return new Result<T, TErr>(default!, error, false);
    }

    /// <summary>
    /// Returns the contained Ok value.
    /// </summary>
    /// <param name="message">The panic message if Err</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is Err</exception>
    public T Expect(string message)
    {
        if (!_isOk)
            throw new InvalidOperationException($"{message}: {_error}");
        
        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <param name="message">The panic message if Ok</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok</exception>
    public TErr ExpectErr(string message)
    {
        if (_isOk)
            throw new InvalidOperationException($"{message}: {_value}");
        
        return _error!;
    }

    /// <summary>
    /// Returns the contained Ok value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Err</exception>
    public T Unwrap()
    {
        return Expect("Called Unwrap on an Err value");
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok</exception>
    public TErr UnwrapErr()
    {
        return ExpectErr("Called UnwrapErr on an Ok value");
    }

    /// <summary>
    /// Returns the contained Ok value or a default value.
    /// </summary>
    public T UnwrapOr(T defaultValue)
    {
        return _isOk ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Ok value or computes it from the error.
    /// </summary>
    public T UnwrapOrElse(Func<TErr, T> op)
    {
        if (op is null)
            throw new ArgumentNullException(nameof(op));
        
        return _isOk ? _value! : op(_error!);
    }

    /// <summary>
    /// Returns the contained Ok value or a default value of type T.
    /// </summary>
    public T? UnwrapOrDefault()
    {
        return _isOk ? _value : default;
    }

    /// <summary>
    /// Maps a Result&lt;T, TErr&gt; to Result&lt;U, TErr&gt; by applying a function to a contained Ok value.
    /// </summary>
    public Result<U, TErr> Map<U>(Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isOk ? Result<U, TErr>.Ok(mapper(_value!)) : Result<U, TErr>.Err(_error!);
    }

    /// <summary>
    /// Maps a Result&lt;T, TErr&gt; to Result&lt;T, F&gt; by applying a function to a contained Err value.
    /// </summary>
    public Result<T, F> MapErr<F>(Func<TErr, F> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isOk ? Result<T, F>.Ok(_value!) : Result<T, F>.Err(mapper(_error!));
    }

    /// <summary>
    /// Returns the provided default (if Err), or applies a function to the contained value (if Ok).
    /// </summary>
    public U MapOr<U>(U defaultValue, Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isOk ? mapper(_value!) : defaultValue;
    }

    /// <summary>
    /// Maps a Result&lt;T, TErr&gt; to U by applying a function to a contained Ok value, or a fallback function to a contained Err value.
    /// </summary>
    public U MapOrElse<U>(Func<TErr, U> defaultFunc, Func<T, U> mapper)
    {
        if (defaultFunc is null)
            throw new ArgumentNullException(nameof(defaultFunc));
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isOk ? mapper(_value!) : defaultFunc(_error!);
    }

    /// <summary>
    /// Calls the function if the result is Ok, otherwise returns the Err value.
    /// This function can be used for control flow based on Result values.
    /// </summary>
    public Result<U, TErr> AndThen<U>(Func<T, Result<U, TErr>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
        return _isOk ? binder(_value!) : Result<U, TErr>.Err(_error!);
    }

    /// <summary>
    /// Returns resultB if the result is Ok, otherwise returns the Err value.
    /// </summary>
    public Result<U, TErr> And<U>(Result<U, TErr> resultB)
    {
        return _isOk ? resultB : Result<U, TErr>.Err(_error!);
    }

    /// <summary>
    /// Calls the function if the result is Err, otherwise returns the Ok value.
    /// </summary>
    public Result<T, F> OrElse<F>(Func<TErr, Result<T, F>> op)
    {
        if (op is null)
            throw new ArgumentNullException(nameof(op));
        
        return _isOk ? Result<T, F>.Ok(_value!) : op(_error!);
    }

    /// <summary>
    /// Returns the result if it contains an Ok value, otherwise returns resultB.
    /// </summary>
    public Result<T, TErr> Or(Result<T, TErr> resultB)
    {
        return _isOk ? this : resultB;
    }

    /// <summary>
    /// Converts from Result&lt;T, TErr&gt; to Option&lt;T&gt;.
    /// Converts self into an Option&lt;T&gt;, consuming self, and discarding the error, if any.
    /// </summary>
    public Option<T> Ok()
    {
        return _isOk ? Option<T>.Some(_value!) : Option<T>.None();
    }

    /// <summary>
    /// Converts from Result&lt;T, TErr&gt; to Option&lt;TErr&gt;.
    /// Converts self into an Option&lt;TErr&gt;, consuming self, and discarding the success value, if any.
    /// </summary>
    public Option<TErr> Err()
    {
        return _isOk ? Option<TErr>.None() : Option<TErr>.Some(_error!);
    }

    /// <summary>
    /// Pattern matches on the result and executes the appropriate action.
    /// </summary>
    public void Match(Action<T> okAction, Action<TErr> errAction)
    {
        if (okAction is null)
            throw new ArgumentNullException(nameof(okAction));
        if (errAction is null)
            throw new ArgumentNullException(nameof(errAction));
        
        if (_isOk)
            okAction(_value!);
        else
            errAction(_error!);
    }

    /// <summary>
    /// Pattern matches on the result and returns a result.
    /// </summary>
    public U Match<U>(Func<T, U> okFunc, Func<TErr, U> errFunc)
    {
        if (okFunc is null)
            throw new ArgumentNullException(nameof(okFunc));
        if (errFunc is null)
            throw new ArgumentNullException(nameof(errFunc));
        
        return _isOk ? okFunc(_value!) : errFunc(_error!);
    }

    /// <inheritdoc />
    public bool Equals(Result<T, TErr> other)
    {
        if (_isOk != other._isOk)
            return false;
        
        if (_isOk)
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        
        return EqualityComparer<TErr>.Default.Equals(_error, other._error);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Result<T, TErr> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _isOk ? _value?.GetHashCode() ?? 0 : _error?.GetHashCode() ?? 0;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isOk ? $"Ok({_value})" : $"Err({_error})";
    }

    /// <summary>
    /// Determines whether two Result instances are equal.
    /// </summary>
    public static bool operator ==(Result<T, TErr> left, Result<T, TErr> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Result instances are not equal.
    /// </summary>
    public static bool operator !=(Result<T, TErr> left, Result<T, TErr> right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Extension methods for Result&lt;T, TErr&gt;.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Flattens a nested Result.
    /// </summary>
    public static Result<T, TErr> Flatten<T, TErr>(this Result<Result<T, TErr>, TErr> result)
    {
        return result.AndThen(inner => inner);
    }

    /// <summary>
    /// Transposes a Result of an Option into an Option of a Result.
    /// </summary>
    public static Option<Result<T, TErr>> Transpose<T, TErr>(this Result<Option<T>, TErr> result)
    {
        return result.Match(
            okFunc: option => option.Match(
                someFunc: value => Option<Result<T, TErr>>.Some(Result<T, TErr>.Ok(value)),
                noneFunc: () => Option<Result<T, TErr>>.None()
            ),
            errFunc: err => Option<Result<T, TErr>>.Some(Result<T, TErr>.Err(err))
        );
    }

    /// <summary>
    /// Executes an action if the result is Ok, allowing method chaining.
    /// </summary>
    public static Result<T, TErr> Tap<T, TErr>(this Result<T, TErr> result, Action<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (result.IsOk)
            action(result.Unwrap());
        
        return result;
    }

    /// <summary>
    /// Executes an action if the result is Err, allowing method chaining.
    /// </summary>
    public static Result<T, TErr> TapErr<T, TErr>(this Result<T, TErr> result, Action<TErr> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (result.IsErr)
            action(result.UnwrapErr());
        
        return result;
    }

    /// <summary>
    /// Wraps a function that may throw an exception into a Result.
    /// </summary>
    public static Result<T, Exception> Try<T>(Func<T> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));
        
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
        if (func is null)
            throw new ArgumentNullException(nameof(func));
        
        try
        {
            return Result<T, Exception>.Ok(await func());
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Err(ex);
        }
    }
}

