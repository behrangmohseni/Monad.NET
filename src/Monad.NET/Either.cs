using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents a value of one of two possible types (a disjoint union).
/// An Either is either Left or Right.
/// By convention, Left is used for failure and Right is used for success.
/// </summary>
/// <typeparam name="TLeft">The type of the Left value</typeparam>
/// <typeparam name="TRight">The type of the Right value</typeparam>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(EitherDebugView<,>))]
public readonly struct Either<TLeft, TRight> : IEquatable<Either<TLeft, TRight>>, IComparable<Either<TLeft, TRight>>, IComparable
{
    private readonly TLeft? _left;
    private readonly TRight? _right;
    private readonly bool _isRight;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isRight ? $"Right({_right})" : $"Left({_left})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Either(TLeft left, TRight right, bool isRight)
    {
        _left = left;
        _right = right;
        _isRight = isRight;
    }

    /// <summary>
    /// Returns true if the Either is a Right value.
    /// </summary>
    public bool IsRight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isRight;
    }

    /// <summary>
    /// Returns true if the Either is a Left value.
    /// </summary>
    public bool IsLeft
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isRight;
    }

    /// <summary>
    /// Creates a Left value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> Left(TLeft value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Left with null value.");

        return new Either<TLeft, TRight>(value, default!, false);
    }

    /// <summary>
    /// Creates a Right value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> Right(TRight value)
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNull(nameof(value), "Cannot create Right with null value.");

        return new Either<TLeft, TRight>(default!, value, true);
    }

    /// <summary>
    /// Returns the contained Right value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Left</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRight GetRight()
    {
        if (!_isRight)
            ThrowHelper.ThrowEitherIsLeft(_left!);

        return _right!;
    }

    /// <summary>
    /// Returns the contained Left value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Right</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TLeft GetLeft()
    {
        if (_isRight)
            ThrowHelper.ThrowEitherIsRight(_right!);

        return _left!;
    }

    /// <summary>
    /// Returns the contained Right value, or throws an <see cref="InvalidOperationException"/> if Left.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Left</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRight GetRightOrThrow()
    {
        if (!_isRight)
            ThrowHelper.ThrowEitherIsLeft(_left!);

        return _right!;
    }

    /// <summary>
    /// Returns the contained Right value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if Left.
    /// </summary>
    /// <param name="message">The error message if the value is Left</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is Left</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRight GetRightOrThrow(string message)
    {
        if (!_isRight)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_left}");

        return _right!;
    }

    /// <summary>
    /// Returns the contained Left value, or throws an <see cref="InvalidOperationException"/> if Right.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the value is Right</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TLeft GetLeftOrThrow()
    {
        if (_isRight)
            ThrowHelper.ThrowEitherIsRight(_right!);

        return _left!;
    }

    /// <summary>
    /// Returns the contained Left value, or throws an <see cref="InvalidOperationException"/> 
    /// with the specified message if Right.
    /// </summary>
    /// <param name="message">The error message if the value is Right</param>
    /// <exception cref="InvalidOperationException">Thrown if the value is Right</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TLeft GetLeftOrThrow(string message)
    {
        if (_isRight)
            ThrowHelper.ThrowInvalidOperation($"{message}: {_right}");

        return _left!;
    }

    /// <summary>
    /// Returns the Right value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRight RightOr(TRight defaultValue)
    {
        return _isRight ? _right! : defaultValue;
    }

    /// <summary>
    /// Returns the Left value or a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TLeft LeftOr(TLeft defaultValue)
    {
        return !_isRight ? _left! : defaultValue;
    }

    /// <summary>
    /// Tries to get the contained Right value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the Right value if present; otherwise, the default value.</param>
    /// <returns>True if the Either is Right; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (either.TryGetRight(out var value))
    /// {
    ///     Console.WriteLine($"Right: {value}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetRight(out TRight? value)
    {
        value = _right;
        return _isRight;
    }

    /// <summary>
    /// Tries to get the contained Left value using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="value">When this method returns, contains the Left value if present; otherwise, the default value.</param>
    /// <returns>True if the Either is Left; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (either.TryGetLeft(out var value))
    /// {
    ///     Console.WriteLine($"Left: {value}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetLeft(out TLeft? value)
    {
        value = _left;
        return !_isRight;
    }

    /// <summary>
    /// Returns true if the Either is Right and contains the specified value.
    /// Uses the default equality comparer for type TRight.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Either is Right and contains the specified value; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var either = Either&lt;string, int&gt;.Right(42);
    /// either.ContainsRight(42); // true
    /// either.ContainsRight(0);  // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsRight(TRight value)
    {
        return _isRight && EqualityComparer<TRight>.Default.Equals(_right, value);
    }

    /// <summary>
    /// Returns true if the Either is Left and contains the specified value.
    /// Uses the default equality comparer for type TLeft.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the Either is Left and contains the specified value; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var either = Either&lt;string, int&gt;.Left("error");
    /// either.ContainsLeft("error"); // true
    /// either.ContainsLeft("other"); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsLeft(TLeft value)
    {
        return !_isRight && EqualityComparer<TLeft>.Default.Equals(_left, value);
    }

    /// <summary>
    /// Returns true if the Either is Right and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Either is Right and the predicate returns true; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var either = Either&lt;string, int&gt;.Right(42);
    /// either.ExistsRight(x => x > 40); // true
    /// either.ExistsRight(x => x > 50); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistsRight(Func<TRight, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isRight && predicate(_right!);
    }

    /// <summary>
    /// Returns true if the Either is Left and the predicate returns true for the contained value.
    /// </summary>
    /// <param name="predicate">The predicate to test the value against.</param>
    /// <returns>True if the Either is Left and the predicate returns true; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var either = Either&lt;string, int&gt;.Left("error");
    /// either.ExistsLeft(e => e.Contains("err")); // true
    /// either.ExistsLeft(e => e.Contains("xyz")); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistsLeft(Func<TLeft, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return !_isRight && predicate(_left!);
    }

    /// <summary>
    /// Maps the Right value if it exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<TLeft, U> MapRight<U>(Func<TRight, U> mapper)
    {
        return _isRight
            ? Either<TLeft, U>.Right(mapper(_right!))
            : Either<TLeft, U>.Left(_left!);
    }

    /// <summary>
    /// Maps the Left value if it exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<U, TRight> MapLeft<U>(Func<TLeft, U> mapper)
    {
        return _isRight
            ? Either<U, TRight>.Right(_right!)
            : Either<U, TRight>.Left(mapper(_left!));
    }

    /// <summary>
    /// Maps both Left and Right values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<L, R> BiMap<L, R>(Func<TLeft, L> leftMapper, Func<TRight, R> rightMapper)
    {
        return _isRight
            ? Either<L, R>.Right(rightMapper(_right!))
            : Either<L, R>.Left(leftMapper(_left!));
    }

    /// <summary>
    /// Filters the Right value based on a predicate. Returns Left if the predicate returns false.
    /// </summary>
    /// <param name="predicate">The predicate to test the Right value against.</param>
    /// <param name="leftValue">The Left value to return if the predicate returns false.</param>
    /// <returns>The original Either if Right and predicate is true; otherwise Left.</returns>
    /// <example>
    /// <code>
    /// var either = Either&lt;string, int&gt;.Right(42);
    /// either.FilterRight(x => x > 40, "Value too small"); // Right(42)
    /// either.FilterRight(x => x > 50, "Value too small"); // Left("Value too small")
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<TLeft, TRight> FilterRight(Func<TRight, bool> predicate, TLeft leftValue)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return _isRight && predicate(_right!) ? this : Left(leftValue);
    }

    /// <summary>
    /// Filters the Right value based on a predicate. Returns Left if the predicate returns false.
    /// </summary>
    /// <param name="predicate">The predicate to test the Right value against.</param>
    /// <param name="leftFactory">A function that creates the Left value if the predicate returns false.</param>
    /// <returns>The original Either if Right and predicate is true; otherwise Left.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<TLeft, TRight> FilterRight(Func<TRight, bool> predicate, Func<TLeft> leftFactory)
    {
        ThrowHelper.ThrowIfNull(predicate);
        ThrowHelper.ThrowIfNull(leftFactory);
        return _isRight && predicate(_right!) ? this : Left(leftFactory());
    }

    /// <summary>
    /// Filters the Left value based on a predicate. Returns Right if the predicate returns false.
    /// </summary>
    /// <param name="predicate">The predicate to test the Left value against.</param>
    /// <param name="rightValue">The Right value to return if the predicate returns false.</param>
    /// <returns>The original Either if Left and predicate is true; otherwise Right.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<TLeft, TRight> FilterLeft(Func<TLeft, bool> predicate, TRight rightValue)
    {
        ThrowHelper.ThrowIfNull(predicate);
        return !_isRight && predicate(_left!) ? this : Right(rightValue);
    }

    /// <summary>
    /// Binds the Right value if it exists.
    /// This is the monadic bind operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<TLeft, U> Bind<U>(Func<TRight, Either<TLeft, U>> binder)
    {
        return _isRight ? binder(_right!) : Either<TLeft, U>.Left(_left!);
    }

    /// <summary>
    /// Binds the Left value if it exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<U, TRight> OrElse<U>(Func<TLeft, Either<U, TRight>> binder)
    {
        return _isRight ? Either<U, TRight>.Right(_right!) : binder(_left!);
    }

    /// <summary>
    /// Swaps Left and Right.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either<TRight, TLeft> Swap()
    {
        return _isRight
            ? Either<TRight, TLeft>.Left(_right!)
            : Either<TRight, TLeft>.Right(_left!);
    }

    /// <summary>
    /// Pattern matches on the Either and executes the appropriate action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<TLeft> leftAction, Action<TRight> rightAction)
    {
        if (_isRight)
            rightAction(_right!);
        else
            leftAction(_left!);
    }

    /// <summary>
    /// Pattern matches on the Either and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<TLeft, U> leftFunc, Func<TRight, U> rightFunc)
    {
        return _isRight ? rightFunc(_right!) : leftFunc(_left!);
    }

    /// <summary>
    /// Converts the Either to an Option of the Right value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TRight> RightOption()
    {
        return _isRight ? Option<TRight>.Some(_right!) : Option<TRight>.None();
    }

    /// <summary>
    /// Converts the Either to an Option of the Left value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<TLeft> LeftOption()
    {
        return !_isRight ? Option<TLeft>.Some(_left!) : Option<TLeft>.None();
    }

    /// <summary>
    /// Converts the Either to a Result, treating Left as Err and Right as Ok.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TRight, TLeft> ToResult()
    {
        return _isRight
            ? Result<TRight, TLeft>.Ok(_right!)
            : Result<TRight, TLeft>.Err(_left!);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Either<TLeft, TRight> other)
    {
        if (_isRight != other._isRight)
            return false;

        if (_isRight)
            return EqualityComparer<TRight>.Default.Equals(_right, other._right);

        return EqualityComparer<TLeft>.Default.Equals(_left, other._left);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Either<TLeft, TRight> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return _isRight ? _right?.GetHashCode() ?? 0 : _left?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Compares this Either to another Either.
    /// Left is considered less than Right. When both are Right, the values are compared.
    /// When both are Left, the values are compared.
    /// </summary>
    /// <param name="other">The other Either to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Either<TLeft, TRight> other)
    {
        if (_isRight && other._isRight)
            return Comparer<TRight>.Default.Compare(_right, other._right);
        if (!_isRight && !other._isRight)
            return Comparer<TLeft>.Default.Compare(_left, other._left);
        return _isRight ? 1 : -1;
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is Either<TLeft, TRight> other)
            return CompareTo(other);
        ThrowHelper.ThrowArgument(nameof(obj), $"Object must be of type Either<{typeof(TLeft).Name}, {typeof(TRight).Name}>");
        return 0; // unreachable
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _isRight ? $"Right({_right})" : $"Left({_left})";
    }

    /// <summary>
    /// Determines whether two Either instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Either instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Either<TLeft, TRight> left, Either<TLeft, TRight> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implicit conversion from TRight to Either&lt;TLeft, TRight&gt; (Right).
    /// Allows: Either&lt;string, int&gt; either = 42;
    /// </summary>
    /// <param name="value">The right value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Either<TLeft, TRight>(TRight value)
    {
        return Right(value);
    }

    /// <summary>
    /// Deconstructs the Either into its components for pattern matching.
    /// </summary>
    /// <param name="left">The Left value, or default if Right.</param>
    /// <param name="right">The Right value, or default if Left.</param>
    /// <param name="isRight">True if the Either is Right.</param>
    /// <example>
    /// <code>
    /// var (left, right, isRight) = either;
    /// Console.WriteLine(isRight ? $"Right: {right}" : $"Left: {left}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out TLeft? left, out TRight? right, out bool isRight)
    {
        left = _left;
        right = _right;
        isRight = _isRight;
    }
}

/// <summary>
/// Extension methods for Either&lt;TLeft, TRight&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class EitherExtensions
{
    /// <summary>
    /// Flattens a nested Either (Right side).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, T> Flatten<TLeft, T>(this Either<TLeft, Either<TLeft, T>> either)
    {
        return either.Bind(static inner => inner);
    }

    /// <summary>
    /// Executes an action if the Either is Right, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> TapRight<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Action<TRight> action)
    {
        if (either.IsRight)
            action(either.GetRight());

        return either;
    }

    /// <summary>
    /// Executes an action if the Either is Left, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> TapLeft<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Action<TLeft> action)
    {
        if (either.IsLeft)
            action(either.GetLeft());

        return either;
    }

    /// <summary>
    /// Converts a Result to an Either.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TErr, T> ToEither<T, TErr>(this Result<T, TErr> result)
    {
        return result.Match(
            okFunc: static value => Either<TErr, T>.Right(value),
            errFunc: static err => Either<TErr, T>.Left(err)
        );
    }
}

/// <summary>
/// Debug view proxy for <see cref="Either{TLeft, TRight}"/> to provide a better debugging experience.
/// </summary>
internal sealed class EitherDebugView<TLeft, TRight>
{
    private readonly Either<TLeft, TRight> _either;

    public EitherDebugView(Either<TLeft, TRight> either)
    {
        _either = either;
    }

    public bool IsRight => _either.IsRight;
    public bool IsLeft => _either.IsLeft;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Value => _either.IsRight ? _either.GetRight() : null;

    public object? Left => _either.IsLeft ? _either.GetLeft() : null;
}
