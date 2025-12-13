namespace Monad.NET;

/// <summary>
/// Represents a value of one of two possible types (a disjoint union).
/// An Either is either Left or Right.
/// By convention, Left is used for failure and Right is used for success.
/// </summary>
/// <typeparam name="TLeft">The type of the Left value</typeparam>
/// <typeparam name="TRight">The type of the Right value</typeparam>
public readonly struct Either<TLeft, TRight> : IEquatable<Either<TLeft, TRight>>
{
    private readonly TLeft? _left;
    private readonly TRight? _right;
    private readonly bool _isRight;

    private Either(TLeft left, TRight right, bool isRight)
    {
        _left = left;
        _right = right;
        _isRight = isRight;
    }

    /// <summary>
    /// Returns true if the Either is a Right value.
    /// </summary>
    public bool IsRight => _isRight;

    /// <summary>
    /// Returns true if the Either is a Left value.
    /// </summary>
    public bool IsLeft => !_isRight;

    /// <summary>
    /// Creates a Left value.
    /// </summary>
    public static Either<TLeft, TRight> Left(TLeft value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create Left with null value.");
        
        return new Either<TLeft, TRight>(value, default!, false);
    }

    /// <summary>
    /// Creates a Right value.
    /// </summary>
    public static Either<TLeft, TRight> Right(TRight value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create Right with null value.");
        
        return new Either<TLeft, TRight>(default!, value, true);
    }

    /// <summary>
    /// Returns the contained Right value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Left</exception>
    public TRight UnwrapRight()
    {
        if (!_isRight)
            throw new InvalidOperationException($"Called UnwrapRight on a Left value: {_left}");
        
        return _right!;
    }

    /// <summary>
    /// Returns the contained Left value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Right</exception>
    public TLeft UnwrapLeft()
    {
        if (_isRight)
            throw new InvalidOperationException($"Called UnwrapLeft on a Right value: {_right}");
        
        return _left!;
    }

    /// <summary>
    /// Returns the Right value or a default value.
    /// </summary>
    public TRight RightOr(TRight defaultValue)
    {
        return _isRight ? _right! : defaultValue;
    }

    /// <summary>
    /// Returns the Left value or a default value.
    /// </summary>
    public TLeft LeftOr(TLeft defaultValue)
    {
        return !_isRight ? _left! : defaultValue;
    }

    /// <summary>
    /// Maps the Right value if it exists.
    /// </summary>
    public Either<TLeft, U> MapRight<U>(Func<TRight, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isRight 
            ? Either<TLeft, U>.Right(mapper(_right!)) 
            : Either<TLeft, U>.Left(_left!);
    }

    /// <summary>
    /// Maps the Left value if it exists.
    /// </summary>
    public Either<U, TRight> MapLeft<U>(Func<TLeft, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
        return _isRight 
            ? Either<U, TRight>.Right(_right!) 
            : Either<U, TRight>.Left(mapper(_left!));
    }

    /// <summary>
    /// Maps both Left and Right values.
    /// </summary>
    public Either<L, R> BiMap<L, R>(Func<TLeft, L> leftMapper, Func<TRight, R> rightMapper)
    {
        if (leftMapper is null)
            throw new ArgumentNullException(nameof(leftMapper));
        if (rightMapper is null)
            throw new ArgumentNullException(nameof(rightMapper));
        
        return _isRight 
            ? Either<L, R>.Right(rightMapper(_right!)) 
            : Either<L, R>.Left(leftMapper(_left!));
    }

    /// <summary>
    /// Binds the Right value if it exists.
    /// </summary>
    public Either<TLeft, U> AndThen<U>(Func<TRight, Either<TLeft, U>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
        return _isRight ? binder(_right!) : Either<TLeft, U>.Left(_left!);
    }

    /// <summary>
    /// Binds the Left value if it exists.
    /// </summary>
    public Either<U, TRight> OrElse<U>(Func<TLeft, Either<U, TRight>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
        return _isRight ? Either<U, TRight>.Right(_right!) : binder(_left!);
    }

    /// <summary>
    /// Swaps Left and Right.
    /// </summary>
    public Either<TRight, TLeft> Swap()
    {
        return _isRight 
            ? Either<TRight, TLeft>.Left(_right!) 
            : Either<TRight, TLeft>.Right(_left!);
    }

    /// <summary>
    /// Pattern matches on the Either and executes the appropriate action.
    /// </summary>
    public void Match(Action<TLeft> leftAction, Action<TRight> rightAction)
    {
        if (leftAction is null)
            throw new ArgumentNullException(nameof(leftAction));
        if (rightAction is null)
            throw new ArgumentNullException(nameof(rightAction));
        
        if (_isRight)
            rightAction(_right!);
        else
            leftAction(_left!);
    }

    /// <summary>
    /// Pattern matches on the Either and returns a result.
    /// </summary>
    public U Match<U>(Func<TLeft, U> leftFunc, Func<TRight, U> rightFunc)
    {
        if (leftFunc is null)
            throw new ArgumentNullException(nameof(leftFunc));
        if (rightFunc is null)
            throw new ArgumentNullException(nameof(rightFunc));
        
        return _isRight ? rightFunc(_right!) : leftFunc(_left!);
    }

    /// <summary>
    /// Converts the Either to an Option of the Right value.
    /// </summary>
    public Option<TRight> RightOption()
    {
        return _isRight ? Option<TRight>.Some(_right!) : Option<TRight>.None();
    }

    /// <summary>
    /// Converts the Either to an Option of the Left value.
    /// </summary>
    public Option<TLeft> LeftOption()
    {
        return !_isRight ? Option<TLeft>.Some(_left!) : Option<TLeft>.None();
    }

    /// <summary>
    /// Converts the Either to a Result, treating Left as Err and Right as Ok.
    /// </summary>
    public Result<TRight, TLeft> ToResult()
    {
        return _isRight 
            ? Result<TRight, TLeft>.Ok(_right!) 
            : Result<TRight, TLeft>.Err(_left!);
    }

    /// <inheritdoc />
    public bool Equals(Either<TLeft, TRight> other)
    {
        if (_isRight != other._isRight)
            return false;
        
        if (_isRight)
            return EqualityComparer<TRight>.Default.Equals(_right, other._right);
        
        return EqualityComparer<TLeft>.Default.Equals(_left, other._left);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Either<TLeft, TRight> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _isRight ? _right?.GetHashCode() ?? 0 : _left?.GetHashCode() ?? 0;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isRight ? $"Right({_right})" : $"Left({_left})";
    }

    /// <summary>
    /// Determines whether two Either instances are equal.
    /// </summary>
    public static bool operator ==(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Either instances are not equal.
    /// </summary>
    public static bool operator !=(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Extension methods for Either&lt;TLeft, TRight&gt;.
/// </summary>
public static class EitherExtensions
{
    /// <summary>
    /// Flattens a nested Either (Right side).
    /// </summary>
    public static Either<TLeft, T> Flatten<TLeft, T>(this Either<TLeft, Either<TLeft, T>> either)
    {
        return either.AndThen(inner => inner);
    }

    /// <summary>
    /// Executes an action if the Either is Right, allowing method chaining.
    /// </summary>
    public static Either<TLeft, TRight> TapRight<TLeft, TRight>(
        this Either<TLeft, TRight> either, 
        Action<TRight> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (either.IsRight)
            action(either.UnwrapRight());
        
        return either;
    }

    /// <summary>
    /// Executes an action if the Either is Left, allowing method chaining.
    /// </summary>
    public static Either<TLeft, TRight> TapLeft<TLeft, TRight>(
        this Either<TLeft, TRight> either, 
        Action<TLeft> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (either.IsLeft)
            action(either.UnwrapLeft());
        
        return either;
    }

    /// <summary>
    /// Converts a Result to an Either.
    /// </summary>
    public static Either<TErr, T> ToEither<T, TErr>(this Result<T, TErr> result)
    {
        return result.Match(
            okFunc: value => Either<TErr, T>.Right(value),
            errFunc: err => Either<TErr, T>.Left(err)
        );
    }
}

