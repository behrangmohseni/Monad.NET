using System.Runtime.CompilerServices;

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
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Ok with null value.");

        return new Result<T, TErr>(value, default!, true);
    }

    /// <summary>
    /// Creates an Err result containing the specified error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Err(TErr error)
    {
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Cannot create Err with null error.");

        return new Result<T, TErr>(default!, error, false);
    }

    /// <summary>
    /// Returns the contained Ok value.
    /// </summary>
    /// <param name="message">The panic message if Err</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is Err</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Expect(string message)
    {
        if (!_isOk)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_error}");

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <param name="message">The panic message if Ok</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TErr ExpectErr(string message)
    {
        if (_isOk)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_value}");

        return _error!;
    }

    /// <summary>
    /// Returns the contained Ok value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Err</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap()
    {
        if (!_isOk)
            ThrowHelper.ThrowInvalidOperation($"Called Unwrap on an Err value: {_error}");

        return _value!;
    }

    /// <summary>
    /// Returns the contained Err value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Ok</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TErr UnwrapErr()
    {
        if (_isOk)
            ThrowHelper.ThrowInvalidOperation($"Called UnwrapErr on an Ok value: {_value}");

        return _error!;
    }

    /// <summary>
    /// Returns the contained Ok value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T UnwrapOr(T defaultValue)
    {
        return _isOk ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained Ok value or computes it from the error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T UnwrapOrElse(Func<TErr, T> op)
    {
        return _isOk ? _value! : op(_error!);
    }

    /// <summary>
    /// Returns the contained Ok value or a default value of type T.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? UnwrapOrDefault()
    {
        return _isOk ? _value : default;
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
    public Result<T, F> MapErr<F>(Func<TErr, F> mapper)
    {
        return _isOk ? Result<T, F>.Ok(_value!) : Result<T, F>.Err(mapper(_error!));
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
    /// Calls the function if the result is Ok, otherwise returns the Err value.
    /// This function can be used for control flow based on Result values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<U, TErr> AndThen<U>(Func<T, Result<U, TErr>> binder)
    {
        return _isOk ? binder(_value!) : Result<U, TErr>.Err(_error!);
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

    /// <inheritdoc />
    public override string ToString()
    {
        return _isOk ? $"Ok({_value})" : $"Err({_error})";
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
}

/// <summary>
/// Extension methods for Result&lt;T, TErr&gt;.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Flattens a nested Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Flatten<T, TErr>(this Result<Result<T, TErr>, TErr> result)
    {
        return result.AndThen(static inner => inner);
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
            action(result.Unwrap());

        return result;
    }

    /// <summary>
    /// Executes an action if the result is Err, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> TapErr<T, TErr>(this Result<T, TErr> result, Action<TErr> action)
    {
        if (result.IsErr)
            action(result.UnwrapErr());

        return result;
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
}
