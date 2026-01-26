using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Option<T> to improve code coverage.
/// </summary>
public class OptionExtendedTests
{
    #region Contains and Exists Tests

    [Fact]
    public void Contains_OnSome_WithMatchingValue_ReturnsTrue()
    {
        var option = Option<int>.Some(42);
        Assert.True(option.Contains(42));
    }

    [Fact]
    public void Contains_OnSome_WithNonMatchingValue_ReturnsFalse()
    {
        var option = Option<int>.Some(42);
        Assert.False(option.Contains(99));
    }

    [Fact]
    public void Contains_OnNone_ReturnsFalse()
    {
        var option = Option<int>.None();
        Assert.False(option.Contains(42));
    }

    [Fact]
    public void Exists_OnSome_WithMatchingPredicate_ReturnsTrue()
    {
        var option = Option<int>.Some(42);
        Assert.True(option.Exists(x => x > 40));
    }

    [Fact]
    public void Exists_OnSome_WithNonMatchingPredicate_ReturnsFalse()
    {
        var option = Option<int>.Some(42);
        Assert.False(option.Exists(x => x > 50));
    }

    [Fact]
    public void Exists_OnNone_ReturnsFalse()
    {
        var option = Option<int>.None();
        Assert.False(option.Exists(x => x > 0));
    }

    #endregion

    #region OrElse and Or Tests

    [Fact]
    public void OrElse_OnSome_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);
        var result = option.OrElse(() => Option<int>.Some(99));

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void OrElse_OnNone_ReturnsAlternative()
    {
        var option = Option<int>.None();
        var result = option.OrElse(() => Option<int>.Some(99));

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void Or_OnSome_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);
        var result = option.Or(Option<int>.Some(99));

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Or_OnNone_ReturnsAlternative()
    {
        var option = Option<int>.None();
        var result = option.Or(Option<int>.Some(99));

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    #endregion

    #region Xor Tests

    [Fact]
    public void Xor_BothSome_ReturnsNone()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(99);
        var result = option1.Xor(option2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Xor_FirstSome_ReturnsFirst()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.None();
        var result = option1.Xor(option2);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Xor_SecondSome_ReturnsSecond()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.Some(99);
        var result = option1.Xor(option2);

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void Xor_BothNone_ReturnsNone()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.None();
        var result = option1.Xor(option2);

        Assert.True(result.IsNone);
    }

    #endregion

    #region Flatten Tests

    [Fact]
    public void Flatten_NestedSome_ReturnsInner()
    {
        var nested = Option<Option<int>>.Some(Option<int>.Some(42));
        var result = nested.Flatten();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Flatten_OuterNone_ReturnsNone()
    {
        var nested = Option<Option<int>>.None();
        var result = nested.Flatten();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Flatten_InnerNone_ReturnsNone()
    {
        var nested = Option<Option<int>>.Some(Option<int>.None());
        var result = nested.Flatten();

        Assert.True(result.IsNone);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SomeWithSameValue_ReturnsTrue()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(42);

        Assert.True(option1.Equals(option2));
        Assert.True(option1 == option2);
    }

    [Fact]
    public void Equals_SomeWithDifferentValue_ReturnsFalse()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(99);

        Assert.False(option1.Equals(option2));
        Assert.True(option1 != option2);
    }

    [Fact]
    public void Equals_SomeWithNone_ReturnsFalse()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.None();

        Assert.False(option1.Equals(option2));
    }

    [Fact]
    public void Equals_BothNone_ReturnsTrue()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.None();

        Assert.True(option1.Equals(option2));
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        var option = Option<int>.Some(42);

        Assert.False(option.Equals(null));
        Assert.False(option.Equals("not an option"));
        Assert.True(option.Equals((object)Option<int>.Some(42)));
    }

    [Fact]
    public void GetHashCode_SameForEqualOptions()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(42);

        Assert.Equal(option1.GetHashCode(), option2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_BothNone_AreSame()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.None();

        Assert.Equal(option1.GetHashCode(), option2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnSome_ContainsValue()
    {
        var option = Option<int>.Some(42);
        var str = option.ToString();

        Assert.Contains("42", str);
        Assert.Contains("Some", str);
    }

    [Fact]
    public void ToString_OnNone_ContainsNone()
    {
        var option = Option<int>.None();
        var str = option.ToString();

        Assert.Contains("None", str);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSome()
    {
        Option<int> option = 42;

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_OnSome_ExecutesAction()
    {
        var option = Option<int>.Some(42);
        var executed = false;

        var result = option.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsSome);
    }

    [Fact]
    public void Tap_OnNone_DoesNotExecuteAction()
    {
        var option = Option<int>.None();
        var executed = false;

        var result = option.Tap(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void TapNone_OnNone_ExecutesAction()
    {
        var option = Option<int>.None();
        var executed = false;

        var result = option.TapNone(() => executed = true);

        Assert.True(executed);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void TapNone_OnSome_DoesNotExecuteAction()
    {
        var option = Option<int>.Some(42);
        var executed = false;

        var result = option.TapNone(() => executed = true);

        Assert.False(executed);
        Assert.True(result.IsSome);
    }

    #endregion

    #region Zip Tests

    [Fact]
    public void Zip_BothSome_ReturnsTuple()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<string>.Some("hello");

        var result = option1.Zip(option2);

        Assert.True(result.IsSome);
        var (a, b) = result.GetValue();
        Assert.Equal(42, a);
        Assert.Equal("hello", b);
    }

    [Fact]
    public void ZipWith_BothSome_AppliesCombiner()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(8);

        var result = option1.ZipWith(option2, (a, b) => a + b);

        Assert.True(result.IsSome);
        Assert.Equal(50, result.GetValue());
    }

    #endregion

    #region OkOr Tests

    [Fact]
    public void OkOrElse_OnSome_ReturnsOk()
    {
        var option = Option<int>.Some(42);
        var factoryExecuted = false;
        var result = option.OkOrElse(() =>
        {
            factoryExecuted = true;
            return "error";
        });

        Assert.False(factoryExecuted);
        Assert.True(result.IsOk);
    }

    [Fact]
    public void OkOrElse_OnNone_ReturnsErr()
    {
        var option = Option<int>.None();
        var result = option.OkOrElse(() => "error");

        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    #endregion

    #region Expect Tests

    [Fact]
    public void Expect_OnSome_ReturnsValue()
    {
        var option = Option<int>.Some(42);
        var value = option.GetOrThrow("Should have value");

        Assert.Equal(42, value);
    }

    [Fact]
    public void Expect_OnNone_ThrowsWithMessage()
    {
        var option = Option<int>.None();

        var ex = Assert.Throws<InvalidOperationException>(() => option.GetOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
    }

    #endregion
}
