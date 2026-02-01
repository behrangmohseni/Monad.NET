using Monad.NET;

namespace Monad.NET.Tests;

public class OptionTests
{
    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        var option = Option<int>.Some(42);

        Assert.True(option.IsSome);
        Assert.False(option.IsNone);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void None_CreatesEmptyOption()
    {
        var option = Option<int>.None();

        Assert.False(option.IsSome);
        Assert.True(option.IsNone);
    }

    [Fact]
    public void Some_WithNullValue_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void Unwrap_OnNone_ThrowsException()
    {
        var option = Option<int>.None();

        Assert.Throws<InvalidOperationException>(() => option.GetValue());
    }

    [Fact]
    public void Expect_OnNone_ThrowsWithMessage()
    {
        var option = Option<int>.None();

        Assert.Throws<InvalidOperationException>(() => option.GetOrThrow());
    }

    [Fact]
    public void UnwrapOr_OnSome_ReturnsValue()
    {
        var option = Option<int>.Some(42);

        Assert.Equal(42, option.GetValueOr(0));
    }

    [Fact]
    public void UnwrapOr_OnNone_ReturnsDefault()
    {
        var option = Option<int>.None();

        Assert.Equal(0, option.GetValueOr(0));
    }

    [Fact]
    public void UnwrapOrElse_OnSome_ReturnsValue()
    {
        var option = Option<int>.Some(42);

        Assert.Equal(42, option.Match(x => x, () => 0));
    }

    [Fact]
    public void UnwrapOrElse_OnNone_ExecutesFunction()
    {
        var option = Option<int>.None();

        Assert.Equal(100, option.Match(x => x, () => 100));
    }

    [Fact]
    public void Map_OnSome_TransformsValue()
    {
        var option = Option<int>.Some(42);
        var mapped = option.Map(x => x * 2);

        Assert.True(mapped.IsSome);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void Map_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var mapped = option.Map(x => x * 2);

        Assert.True(mapped.IsNone);
    }

    [Fact]
    public void Filter_WithMatchingPredicate_ReturnsSome()
    {
        var option = Option<int>.Some(42);
        var filtered = option.Filter(x => x > 40);

        Assert.True(filtered.IsSome);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void Filter_WithNonMatchingPredicate_ReturnsNone()
    {
        var option = Option<int>.Some(42);
        var filtered = option.Filter(x => x < 40);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public void AndThen_OnSome_ExecutesFunction()
    {
        var option = Option<int>.Some(42);
        var result = option.Bind(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsSome);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public void AndThen_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = option.Bind(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Zip_BothSome_ReturnsTuple()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<string>.Some("hello");

        var result = option1.Zip(option2);

        Assert.True(result.IsSome);
        Assert.Equal((42, "hello"), result.GetValue());
    }

    [Fact]
    public void Zip_FirstNone_ReturnsNone()
    {
        var option1 = Option<int>.None();
        var option2 = Option<string>.Some("hello");

        var result = option1.Zip(option2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Zip_SecondNone_ReturnsNone()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<string>.None();

        var result = option1.Zip(option2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void ZipWith_BothSome_ReturnsCombinedValue()
    {
        var option1 = Option<int>.Some(10);
        var option2 = Option<int>.Some(20);

        var result = option1.ZipWith(option2, (a, b) => a + b);

        Assert.True(result.IsSome);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void ZipWith_FirstNone_ReturnsNone()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.Some(20);

        var result = option1.ZipWith(option2, (a, b) => a + b);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void ZipWith_SecondNone_ReturnsNone()
    {
        var option1 = Option<int>.Some(10);
        var option2 = Option<int>.None();

        var result = option1.ZipWith(option2, (a, b) => a + b);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Or_BothSome_ReturnsFirst()
    {
        var option1 = Option<int>.Some(1);
        var option2 = Option<int>.Some(2);
        var result = option1.Or(option2);

        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void Or_FirstNone_ReturnsSecond()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.Some(2);
        var result = option1.Or(option2);

        Assert.Equal(2, result.GetValue());
    }

    [Fact]
    public void Xor_OneSome_ReturnsSome()
    {
        var option1 = Option<int>.Some(1);
        var option2 = Option<int>.None();
        var result = option1.Xor(option2);

        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void Xor_BothSome_ReturnsNone()
    {
        var option1 = Option<int>.Some(1);
        var option2 = Option<int>.Some(2);
        var result = option1.Xor(option2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Match_OnSome_ExecutesSomeAction()
    {
        var option = Option<int>.Some(42);
        var executed = false;

        option.Match(
            someAction: x => executed = true,
            noneAction: () => executed = false
        );

        Assert.True(executed);
    }

    [Fact]
    public void Match_OnNone_ExecutesNoneAction()
    {
        var option = Option<int>.None();
        var executed = false;

        option.Match(
            someAction: x => executed = true,
            noneAction: () => executed = false
        );

        Assert.False(executed);
    }

    [Fact]
    public void Match_WithReturn_OnSome_ReturnsSomeValue()
    {
        var option = Option<int>.Some(42);
        var result = option.Match(
            someFunc: x => x.ToString(),
            noneFunc: () => "none"
        );

        Assert.Equal("42", result);
    }

    [Fact]
    public void OkOr_OnSome_ReturnsOk()
    {
        var option = Option<int>.Some(42);
        var result = option.OkOr("error");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void OkOr_OnNone_ReturnsErr()
    {
        var option = Option<int>.None();
        var result = option.OkOr("error");

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void ToOption_NullableWithValue_ReturnsSome()
    {
        int? value = 42;
        var option = value.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void ToOption_NullableWithNull_ReturnsNone()
    {
        int? value = null;
        var option = value.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void ToOption_ReferenceTypeWithValue_ReturnsSome()
    {
        string? value = "hello";
        var option = value.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal("hello", option.GetValue());
    }

    [Fact]
    public void ToOption_ReferenceTypeWithNull_ReturnsNone()
    {
        string? value = null;
        var option = value.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void Flatten_NestedSome_ReturnsInnerSome()
    {
        var nested = Option<Option<int>>.Some(Option<int>.Some(42));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsSome);
        Assert.Equal(42, flattened.GetValue());
    }

    [Fact]
    public void Flatten_NestedNone_ReturnsNone()
    {
        var nested = Option<Option<int>>.Some(Option<int>.None());
        var flattened = nested.Flatten();

        Assert.True(flattened.IsNone);
    }

    [Fact]
    public void Equality_TwoSomesWithSameValue_AreEqual()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(42);

        Assert.Equal(option1, option2);
        Assert.True(option1 == option2);
        Assert.False(option1 != option2);
    }

    [Fact]
    public void Equality_TwoNones_AreEqual()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.None();

        Assert.Equal(option1, option2);
        Assert.True(option1 == option2);
    }

    [Fact]
    public void Equality_SomeAndNone_AreNotEqual()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.None();

        Assert.NotEqual(option1, option2);
        Assert.True(option1 != option2);
    }

    [Fact]
    public void ToString_OnSome_ReturnsFormattedString()
    {
        var option = Option<int>.Some(42);

        Assert.Equal("Some(42)", option.ToString());
    }

    [Fact]
    public void ToString_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();

        Assert.Equal("None", option.ToString());
    }
}

