using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Either<L, R> to improve code coverage.
/// </summary>
public class EitherExtendedTests
{
    #region Contains Tests

    [Fact]
    public void ContainsRight_OnRight_WithMatchingValue_ReturnsTrue()
    {
        var either = Either<string, int>.Right(42);
        Assert.True(either.ContainsRight(42));
    }

    [Fact]
    public void ContainsRight_OnRight_WithNonMatchingValue_ReturnsFalse()
    {
        var either = Either<string, int>.Right(42);
        Assert.False(either.ContainsRight(99));
    }

    [Fact]
    public void ContainsRight_OnLeft_ReturnsFalse()
    {
        var either = Either<string, int>.Left("error");
        Assert.False(either.ContainsRight(42));
    }

    [Fact]
    public void ContainsLeft_OnLeft_WithMatchingValue_ReturnsTrue()
    {
        var either = Either<string, int>.Left("error");
        Assert.True(either.ContainsLeft("error"));
    }

    [Fact]
    public void ContainsLeft_OnLeft_WithNonMatchingValue_ReturnsFalse()
    {
        var either = Either<string, int>.Left("error");
        Assert.False(either.ContainsLeft("different"));
    }

    [Fact]
    public void ContainsLeft_OnRight_ReturnsFalse()
    {
        var either = Either<string, int>.Right(42);
        Assert.False(either.ContainsLeft("error"));
    }

    #endregion

    #region BiMap Tests

    [Fact]
    public void BiMap_OnRight_AppliesRightMapper()
    {
        var either = Either<string, int>.Right(42);
        var result = either.BiMap(l => l.ToUpper(), r => r * 2);

        Assert.True(result.IsRight);
        Assert.Equal(84, result.UnwrapRight());
    }

    [Fact]
    public void BiMap_OnLeft_AppliesLeftMapper()
    {
        var either = Either<string, int>.Left("error");
        var result = either.BiMap(l => l.ToUpper(), r => r * 2);

        Assert.True(result.IsLeft);
        Assert.Equal("ERROR", result.UnwrapLeft());
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_RightWithSameValue_ReturnsTrue()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Right(42);

        Assert.True(either1.Equals(either2));
        Assert.True(either1 == either2);
    }

    [Fact]
    public void Equals_RightWithDifferentValue_ReturnsFalse()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Right(99);

        Assert.False(either1.Equals(either2));
        Assert.True(either1 != either2);
    }

    [Fact]
    public void Equals_LeftWithSameValue_ReturnsTrue()
    {
        var either1 = Either<string, int>.Left("error");
        var either2 = Either<string, int>.Left("error");

        Assert.True(either1.Equals(either2));
    }

    [Fact]
    public void Equals_LeftWithDifferentValue_ReturnsFalse()
    {
        var either1 = Either<string, int>.Left("error1");
        var either2 = Either<string, int>.Left("error2");

        Assert.False(either1.Equals(either2));
    }

    [Fact]
    public void Equals_RightWithLeft_ReturnsFalse()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Left("error");

        Assert.False(either1.Equals(either2));
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        var either = Either<string, int>.Right(42);

        Assert.False(either.Equals(null));
        Assert.False(either.Equals("not an either"));
        Assert.True(either.Equals((object)Either<string, int>.Right(42)));
    }

    [Fact]
    public void GetHashCode_SameForEqualEithers()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Right(42);

        Assert.Equal(either1.GetHashCode(), either2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnRight_ContainsValue()
    {
        var either = Either<string, int>.Right(42);
        var str = either.ToString();

        Assert.Contains("42", str);
        Assert.Contains("Right", str);
    }

    [Fact]
    public void ToString_OnLeft_ContainsValue()
    {
        var either = Either<string, int>.Left("error message");
        var str = either.ToString();

        Assert.Contains("error message", str);
        Assert.Contains("Left", str);
    }

    #endregion

    #region TapRight/TapLeft Tests

    [Fact]
    public void TapRight_OnRight_ExecutesAction()
    {
        var either = Either<string, int>.Right(42);
        var executed = false;

        var result = either.TapRight(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsRight);
    }

    [Fact]
    public void TapRight_OnLeft_DoesNotExecuteAction()
    {
        var either = Either<string, int>.Left("error");
        var executed = false;

        var result = either.TapRight(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsLeft);
    }

    [Fact]
    public void TapLeft_OnLeft_ExecutesAction()
    {
        var either = Either<string, int>.Left("error");
        var executed = false;

        var result = either.TapLeft(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsLeft);
    }

    [Fact]
    public void TapLeft_OnRight_DoesNotExecuteAction()
    {
        var either = Either<string, int>.Right(42);
        var executed = false;

        var result = either.TapLeft(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsRight);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void MapRight_OnRight_Transforms()
    {
        var either = Either<string, int>.Right(42);
        var result = either.MapRight(x => x * 2);

        Assert.True(result.IsRight);
        Assert.Equal(84, result.UnwrapRight());
    }

    [Fact]
    public void MapRight_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("error");
        var result = either.MapRight(x => x * 2);

        Assert.True(result.IsLeft);
        Assert.Equal("error", result.UnwrapLeft());
    }

    [Fact]
    public void MapLeft_OnLeft_Transforms()
    {
        var either = Either<string, int>.Left("error");
        var result = either.MapLeft(x => x.ToUpper());

        Assert.True(result.IsLeft);
        Assert.Equal("ERROR", result.UnwrapLeft());
    }

    [Fact]
    public void MapLeft_OnRight_ReturnsRight()
    {
        var either = Either<string, int>.Right(42);
        var result = either.MapLeft(x => x.ToUpper());

        Assert.True(result.IsRight);
        Assert.Equal(42, result.UnwrapRight());
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_OnRight_ExecutesRightFunc()
    {
        var either = Either<string, int>.Right(42);
        var result = either.Match(l => $"Left: {l}", r => $"Right: {r}");

        Assert.Equal("Right: 42", result);
    }

    [Fact]
    public void Match_OnLeft_ExecutesLeftFunc()
    {
        var either = Either<string, int>.Left("error");
        var result = either.Match(l => $"Left: {l}", r => $"Right: {r}");

        Assert.Equal("Left: error", result);
    }

    #endregion
}
