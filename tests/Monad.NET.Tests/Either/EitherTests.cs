using Monad.NET;

namespace Monad.NET.Tests;

public class EitherTests
{
    [Fact]
    public void Left_CreatesEitherWithLeftValue()
    {
        var either = Either<string, int>.Left("error");

        Assert.True(either.IsLeft);
        Assert.False(either.IsRight);
        Assert.Equal("error", either.GetLeft());
    }

    [Fact]
    public void Right_CreatesEitherWithRightValue()
    {
        var either = Either<string, int>.Right(42);

        Assert.True(either.IsRight);
        Assert.False(either.IsLeft);
        Assert.Equal(42, either.GetRight());
    }

    [Fact]
    public void Left_WithNullValue_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Either<string, int>.Left(null!));
    }

    [Fact]
    public void Right_WithNullValue_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Either<int, string>.Right(null!));
    }

    [Fact]
    public void UnwrapRight_OnLeft_ThrowsException()
    {
        var either = Either<string, int>.Left("error");

        Assert.Throws<InvalidOperationException>(() => either.GetRight());
    }

    [Fact]
    public void UnwrapLeft_OnRight_ThrowsException()
    {
        var either = Either<string, int>.Right(42);

        Assert.Throws<InvalidOperationException>(() => either.GetLeft());
    }

    [Fact]
    public void RightOr_OnRight_ReturnsValue()
    {
        var either = Either<string, int>.Right(42);

        Assert.Equal(42, either.RightOr(0));
    }

    [Fact]
    public void RightOr_OnLeft_ReturnsDefault()
    {
        var either = Either<string, int>.Left("error");

        Assert.Equal(0, either.RightOr(0));
    }

    [Fact]
    public void LeftOr_OnLeft_ReturnsValue()
    {
        var either = Either<string, int>.Left("error");

        Assert.Equal("error", either.LeftOr("default"));
    }

    [Fact]
    public void LeftOr_OnRight_ReturnsDefault()
    {
        var either = Either<string, int>.Right(42);

        Assert.Equal("default", either.LeftOr("default"));
    }

    [Fact]
    public void TryGetRight_OnRight_ReturnsTrueAndValue()
    {
        var either = Either<string, int>.Right(42);

        var result = either.TryGetRight(out var value);

        Assert.True(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetRight_OnLeft_ReturnsFalse()
    {
        var either = Either<string, int>.Left("error");

        var result = either.TryGetRight(out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetLeft_OnLeft_ReturnsTrueAndValue()
    {
        var either = Either<string, int>.Left("error");

        var result = either.TryGetLeft(out var value);

        Assert.True(result);
        Assert.Equal("error", value);
    }

    [Fact]
    public void TryGetLeft_OnRight_ReturnsFalse()
    {
        var either = Either<string, int>.Right(42);

        var result = either.TryGetLeft(out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void MapRight_OnRight_TransformsValue()
    {
        var either = Either<string, int>.Right(42);
        var mapped = either.MapRight(x => x * 2);

        Assert.True(mapped.IsRight);
        Assert.Equal(84, mapped.GetRight());
    }

    [Fact]
    public void MapRight_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("error");
        var mapped = either.MapRight(x => x * 2);

        Assert.True(mapped.IsLeft);
        Assert.Equal("error", mapped.GetLeft());
    }

    [Fact]
    public void MapLeft_OnLeft_TransformsValue()
    {
        var either = Either<string, int>.Left("error");
        var mapped = either.MapLeft(x => x.Length);

        Assert.True(mapped.IsLeft);
        Assert.Equal(5, mapped.GetLeft());
    }

    [Fact]
    public void MapLeft_OnRight_ReturnsRight()
    {
        var either = Either<string, int>.Right(42);
        var mapped = either.MapLeft(x => x.Length);

        Assert.True(mapped.IsRight);
        Assert.Equal(42, mapped.GetRight());
    }

    [Fact]
    public void BiMap_OnRight_TransformsRightValue()
    {
        var either = Either<string, int>.Right(42);
        var mapped = either.BiMap(
            leftMapper: x => x.Length,
            rightMapper: x => x * 2
        );

        Assert.True(mapped.IsRight);
        Assert.Equal(84, mapped.GetRight());
    }

    [Fact]
    public void BiMap_OnLeft_TransformsLeftValue()
    {
        var either = Either<string, int>.Left("error");
        var mapped = either.BiMap(
            leftMapper: x => x.Length,
            rightMapper: x => x * 2
        );

        Assert.True(mapped.IsLeft);
        Assert.Equal(5, mapped.GetLeft());
    }

    [Fact]
    public void AndThen_OnRight_ExecutesFunction()
    {
        var either = Either<string, int>.Right(42);
        var result = either.Bind(x => Either<string, string>.Right(x.ToString()));

        Assert.True(result.IsRight);
        Assert.Equal("42", result.GetRight());
    }

    [Fact]
    public void AndThen_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("error");
        var result = either.Bind(x => Either<string, string>.Right(x.ToString()));

        Assert.True(result.IsLeft);
        Assert.Equal("error", result.GetLeft());
    }

    [Fact]
    public void OrElse_OnLeft_ExecutesFunction()
    {
        var either = Either<string, int>.Left("error");
        var result = either.OrElse(x => Either<int, int>.Right(100));

        Assert.True(result.IsRight);
        Assert.Equal(100, result.GetRight());
    }

    [Fact]
    public void OrElse_OnRight_ReturnsRight()
    {
        var either = Either<string, int>.Right(42);
        var result = either.OrElse(x => Either<int, int>.Right(100));

        Assert.True(result.IsRight);
        Assert.Equal(42, result.GetRight());
    }

    [Fact]
    public void Swap_OnRight_ReturnsLeft()
    {
        var either = Either<string, int>.Right(42);
        var swapped = either.Swap();

        Assert.True(swapped.IsLeft);
        Assert.Equal(42, swapped.GetLeft());
    }

    [Fact]
    public void Swap_OnLeft_ReturnsRight()
    {
        var either = Either<string, int>.Left("error");
        var swapped = either.Swap();

        Assert.True(swapped.IsRight);
        Assert.Equal("error", swapped.GetRight());
    }

    [Fact]
    public void Match_OnRight_ExecutesRightAction()
    {
        var either = Either<string, int>.Right(42);
        var value = 0;

        either.Match(
            leftAction: x => value = -1,
            rightAction: x => value = x
        );

        Assert.Equal(42, value);
    }

    [Fact]
    public void Match_OnLeft_ExecutesLeftAction()
    {
        var either = Either<string, int>.Left("error");
        var message = string.Empty;

        either.Match(
            leftAction: x => message = x,
            rightAction: x => message = "right"
        );

        Assert.Equal("error", message);
    }

    [Fact]
    public void Match_WithReturn_OnRight_ReturnsRightValue()
    {
        var either = Either<string, int>.Right(42);
        var result = either.Match(
            leftFunc: x => x,
            rightFunc: x => x.ToString()
        );

        Assert.Equal("42", result);
    }

    [Fact]
    public void Match_WithReturn_OnLeft_ReturnsLeftValue()
    {
        var either = Either<string, int>.Left("error");
        var result = either.Match(
            leftFunc: x => x,
            rightFunc: x => x.ToString()
        );

        Assert.Equal("error", result);
    }

    [Fact]
    public void RightOption_OnRight_ReturnsSome()
    {
        var either = Either<string, int>.Right(42);
        var option = either.RightOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void RightOption_OnLeft_ReturnsNone()
    {
        var either = Either<string, int>.Left("error");
        var option = either.RightOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void LeftOption_OnLeft_ReturnsSome()
    {
        var either = Either<string, int>.Left("error");
        var option = either.LeftOption();

        Assert.True(option.IsSome);
        Assert.Equal("error", option.GetValue());
    }

    [Fact]
    public void LeftOption_OnRight_ReturnsNone()
    {
        var either = Either<string, int>.Right(42);
        var option = either.LeftOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void ToResult_OnRight_ReturnsOk()
    {
        var either = Either<string, int>.Right(42);
        var result = either.ToResult();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_OnLeft_ReturnsErr()
    {
        var either = Either<string, int>.Left("error");
        var result = either.ToResult();

        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void Flatten_NestedRight_ReturnsInnerRight()
    {
        var nested = Either<string, Either<string, int>>.Right(Either<string, int>.Right(42));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsRight);
        Assert.Equal(42, flattened.GetRight());
    }

    [Fact]
    public void Flatten_NestedLeft_ReturnsLeft()
    {
        var nested = Either<string, Either<string, int>>.Right(Either<string, int>.Left("inner error"));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsLeft);
        Assert.Equal("inner error", flattened.GetLeft());
    }

    [Fact]
    public void TapRight_OnRight_ExecutesAction()
    {
        var either = Either<string, int>.Right(42);
        var executed = false;

        var returned = either.TapRight(x => executed = true);

        Assert.True(executed);
        Assert.Equal(either, returned);
    }

    [Fact]
    public void TapRight_OnLeft_DoesNotExecuteAction()
    {
        var either = Either<string, int>.Left("error");
        var executed = false;

        var returned = either.TapRight(x => executed = true);

        Assert.False(executed);
        Assert.Equal(either, returned);
    }

    [Fact]
    public void TapLeft_OnLeft_ExecutesAction()
    {
        var either = Either<string, int>.Left("error");
        var executed = false;

        var returned = either.TapLeft(x => executed = true);

        Assert.True(executed);
        Assert.Equal(either, returned);
    }

    [Fact]
    public void TapLeft_OnRight_DoesNotExecuteAction()
    {
        var either = Either<string, int>.Right(42);
        var executed = false;

        var returned = either.TapLeft(x => executed = true);

        Assert.False(executed);
        Assert.Equal(either, returned);
    }

    [Fact]
    public void ToEither_FromOk_ReturnsRight()
    {
        var result = Result<int, string>.Ok(42);
        var either = result.ToEither();

        Assert.True(either.IsRight);
        Assert.Equal(42, either.GetRight());
    }

    [Fact]
    public void ToEither_FromErr_ReturnsLeft()
    {
        var result = Result<int, string>.Err("error");
        var either = result.ToEither();

        Assert.True(either.IsLeft);
        Assert.Equal("error", either.GetLeft());
    }

    [Fact]
    public void Equality_TwoRightsWithSameValue_AreEqual()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Right(42);

        Assert.Equal(either1, either2);
        Assert.True(either1 == either2);
    }

    [Fact]
    public void Equality_TwoLeftsWithSameValue_AreEqual()
    {
        var either1 = Either<string, int>.Left("error");
        var either2 = Either<string, int>.Left("error");

        Assert.Equal(either1, either2);
        Assert.True(either1 == either2);
    }

    [Fact]
    public void Equality_RightAndLeft_AreNotEqual()
    {
        var either1 = Either<string, int>.Right(42);
        var either2 = Either<string, int>.Left("error");

        Assert.NotEqual(either1, either2);
        Assert.True(either1 != either2);
    }

    [Fact]
    public void ToString_OnRight_ReturnsFormattedString()
    {
        var either = Either<string, int>.Right(42);

        Assert.Equal("Right(42)", either.ToString());
    }

    [Fact]
    public void ToString_OnLeft_ReturnsFormattedString()
    {
        var either = Either<string, int>.Left("error");

        Assert.Equal("Left(error)", either.ToString());
    }
}

