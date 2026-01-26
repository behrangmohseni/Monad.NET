using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for Either to improve code coverage.
/// </summary>
public class EitherCoverageTests
{
    #region GetRightOrThrow with Message Tests

    [Fact]
    public void GetRightOrThrow_WithMessage_Right_ReturnsValue()
    {
        var either = Either<string, int>.Right(42);
        var value = either.GetRightOrThrow("Should not fail");
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetRightOrThrow_WithMessage_Left_ThrowsWithMessage()
    {
        var either = Either<string, int>.Left("error");
        var ex = Assert.Throws<InvalidOperationException>(() => either.GetRightOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("error", ex.Message);
    }

    #endregion

    #region GetLeftOrThrow with Message Tests

    [Fact]
    public void GetLeftOrThrow_WithMessage_Left_ReturnsValue()
    {
        var either = Either<string, int>.Left("error");
        var value = either.GetLeftOrThrow("Should not fail");
        Assert.Equal("error", value);
    }

    [Fact]
    public void GetLeftOrThrow_WithMessage_Right_ThrowsWithMessage()
    {
        var either = Either<string, int>.Right(42);
        var ex = Assert.Throws<InvalidOperationException>(() => either.GetLeftOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    #endregion

    #region IComparable Tests

    [Fact]
    public void CompareTo_Object_Null_ReturnsPositive()
    {
        var either = Either<string, int>.Right(42);
        Assert.Equal(1, ((IComparable)either).CompareTo(null));
    }

    [Fact]
    public void CompareTo_Object_InvalidType_ThrowsArgumentException()
    {
        var either = Either<string, int>.Right(42);
        Assert.Throws<ArgumentException>(() => ((IComparable)either).CompareTo("not an Either"));
    }

    [Fact]
    public void CompareTo_Object_ValidEither_ComparesCorrectly()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Right(50);
        Assert.True(((IComparable)either1).CompareTo(either2) < 0);
    }

    #endregion

    #region FilterRight with Factory Tests

    [Fact]
    public void FilterRight_WithFactory_Right_PredicateTrue_ReturnsOriginal()
    {
        var either = Either<string, int>.Right(42);
        var filtered = either.FilterRight(x => x > 40, () => "too small");
        Assert.True(filtered.IsRight);
        Assert.Equal(42, filtered.GetRight());
    }

    [Fact]
    public void FilterRight_WithFactory_Right_PredicateFalse_ReturnsLeft()
    {
        var either = Either<string, int>.Right(42);
        var filtered = either.FilterRight(x => x > 50, () => "too small");
        Assert.True(filtered.IsLeft);
        Assert.Equal("too small", filtered.GetLeft());
    }

    [Fact]
    public void FilterRight_WithFactory_Left_ReturnsLeft()
    {
        var either = Either<string, int>.Left("original error");
        var filtered = either.FilterRight(x => x > 50, () => "too small");
        Assert.True(filtered.IsLeft);
        // FilterRight on Left returns left from factory, not original
        Assert.Equal("too small", filtered.GetLeft());
    }

    #endregion

    #region FilterLeft Tests

    [Fact]
    public void FilterLeft_Left_PredicateTrue_ReturnsOriginal()
    {
        var either = Either<string, int>.Left("error");
        var filtered = either.FilterLeft(e => e.Contains("err"), 99);
        Assert.True(filtered.IsLeft);
        Assert.Equal("error", filtered.GetLeft());
    }

    [Fact]
    public void FilterLeft_Left_PredicateFalse_ReturnsRight()
    {
        var either = Either<string, int>.Left("error");
        var filtered = either.FilterLeft(e => e.Contains("xyz"), 99);
        Assert.True(filtered.IsRight);
        Assert.Equal(99, filtered.GetRight());
    }

    [Fact]
    public void FilterLeft_Right_ReturnsRight()
    {
        var either = Either<string, int>.Right(42);
        var filtered = either.FilterLeft(e => e.Contains("err"), 99);
        Assert.True(filtered.IsRight);
        // FilterLeft on Right returns the provided rightValue, not original
        Assert.Equal(99, filtered.GetRight());
    }

    #endregion
}
