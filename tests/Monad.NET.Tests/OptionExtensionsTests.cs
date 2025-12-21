using Xunit;

namespace Monad.NET.Tests;

public class OptionExtensionsTests
{
    #region When/Unless Guards

    [Fact]
    public void When_WithTrueConditionAndFactory_ReturnsSome()
    {
        var result = OptionExtensions.When(true, () => 42);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void When_WithFalseConditionAndFactory_ReturnsNone()
    {
        var result = OptionExtensions.When(false, () => 42);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void When_WithFalseCondition_DoesNotInvokeFactory()
    {
        var factoryInvoked = false;
        var result = OptionExtensions.When(false, () =>
        {
            factoryInvoked = true;
            return 42;
        });

        Assert.True(result.IsNone);
        Assert.False(factoryInvoked);
    }

    [Fact]
    public void When_WithTrueConditionAndValue_ReturnsSome()
    {
        var result = OptionExtensions.When(true, "hello");

        Assert.True(result.IsSome);
        Assert.Equal("hello", result.Unwrap());
    }

    [Fact]
    public void When_WithFalseConditionAndValue_ReturnsNone()
    {
        var result = OptionExtensions.When(false, "hello");

        Assert.True(result.IsNone);
    }

    [Fact]
    public void When_WithNullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => OptionExtensions.When<int>(true, (Func<int>)null!));
    }

    [Fact]
    public void Unless_WithFalseConditionAndFactory_ReturnsSome()
    {
        var result = OptionExtensions.Unless(false, () => 42);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Unless_WithTrueConditionAndFactory_ReturnsNone()
    {
        var result = OptionExtensions.Unless(true, () => 42);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Unless_WithTrueCondition_DoesNotInvokeFactory()
    {
        var factoryInvoked = false;
        var result = OptionExtensions.Unless(true, () =>
        {
            factoryInvoked = true;
            return 42;
        });

        Assert.True(result.IsNone);
        Assert.False(factoryInvoked);
    }

    [Fact]
    public void Unless_WithFalseConditionAndValue_ReturnsSome()
    {
        var result = OptionExtensions.Unless(false, "hello");

        Assert.True(result.IsSome);
        Assert.Equal("hello", result.Unwrap());
    }

    [Fact]
    public void Unless_WithTrueConditionAndValue_ReturnsNone()
    {
        var result = OptionExtensions.Unless(true, "hello");

        Assert.True(result.IsNone);
    }

    [Fact]
    public void When_RealWorldExample_ConditionalDiscount()
    {
        var orderTotal = 150m;
        var discount = OptionExtensions.When(orderTotal > 100, () => 0.1m);

        Assert.True(discount.IsSome);
        Assert.Equal(0.1m, discount.Unwrap());

        var noDiscount = OptionExtensions.When(orderTotal < 100, () => 0.1m);
        Assert.True(noDiscount.IsNone);
    }

    [Fact]
    public void Unless_RealWorldExample_FallbackValue()
    {
        var cacheHasValue = false;
        var fallback = OptionExtensions.Unless(cacheHasValue, () => "loaded from db");

        Assert.True(fallback.IsSome);
        Assert.Equal("loaded from db", fallback.Unwrap());

        cacheHasValue = true;
        var noFallback = OptionExtensions.Unless(cacheHasValue, () => "loaded from db");
        Assert.True(noFallback.IsNone);
    }

    #endregion

    #region ToOption Extension

    [Fact]
    public void ToOption_FromNullable_SomeToSome()
    {
        int? nullable = 42;
        var option = nullable.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.Unwrap());
    }

    [Fact]
    public void ToOption_FromNullable_NullToNone()
    {
        int? nullable = null;
        var option = nullable.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void ToOption_FromReference_ValueToSome()
    {
        string? value = "hello";
        var option = value.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal("hello", option.Unwrap());
    }

    [Fact]
    public void ToOption_FromReference_NullToNone()
    {
        string? value = null;
        var option = value.ToOption();

        Assert.True(option.IsNone);
    }

    #endregion

    #region Flatten

    [Fact]
    public void Flatten_UnwrapsNestedOption()
    {
        var nested = Option<Option<int>>.Some(Option<int>.Some(42));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsSome);
        Assert.Equal(42, flattened.Unwrap());
    }

    [Fact]
    public void Flatten_OuterNone_ReturnsNone()
    {
        var nested = Option<Option<int>>.None();
        var flattened = nested.Flatten();

        Assert.True(flattened.IsNone);
    }

    [Fact]
    public void Flatten_InnerNone_ReturnsNone()
    {
        var nested = Option<Option<int>>.Some(Option<int>.None());
        var flattened = nested.Flatten();

        Assert.True(flattened.IsNone);
    }

    #endregion

    #region Option Core Coverage

    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        var option = Option<int>.Some(42);

        Assert.True(option.IsSome);
        Assert.False(option.IsNone);
        Assert.Equal(42, option.Unwrap());
    }

    [Fact]
    public void None_CreatesEmptyOption()
    {
        var option = Option<int>.None();

        Assert.False(option.IsSome);
        Assert.True(option.IsNone);
    }

    [Fact]
    public void Unwrap_ThrowsOnNone()
    {
        var option = Option<int>.None();

        Assert.Throws<InvalidOperationException>(() => option.Unwrap());
    }

    [Fact]
    public void Expect_ThrowsWithMessageOnNone()
    {
        var option = Option<int>.None();

        var ex = Assert.Throws<InvalidOperationException>(() => option.Expect("custom message"));
        Assert.Contains("custom message", ex.Message);
    }

    [Fact]
    public void UnwrapOr_ReturnsValueOnSome()
    {
        var option = Option<int>.Some(42);
        Assert.Equal(42, option.UnwrapOr(0));
    }

    [Fact]
    public void UnwrapOr_ReturnsDefaultOnNone()
    {
        var option = Option<int>.None();
        Assert.Equal(0, option.UnwrapOr(0));
    }

    [Fact]
    public void UnwrapOrElse_ComputesDefaultOnNone()
    {
        var option = Option<int>.None();
        Assert.Equal(100, option.UnwrapOrElse(() => 100));
    }

    [Fact]
    public void Map_TransformsSome()
    {
        var option = Option<int>.Some(10);
        var mapped = option.Map(x => x * 2);

        Assert.True(mapped.IsSome);
        Assert.Equal(20, mapped.Unwrap());
    }

    [Fact]
    public void Map_PreservesNone()
    {
        var option = Option<int>.None();
        var mapped = option.Map(x => x * 2);

        Assert.True(mapped.IsNone);
    }

    [Fact]
    public void Filter_KeepsMatchingValue()
    {
        var option = Option<int>.Some(10);
        var filtered = option.Filter(x => x > 5);

        Assert.True(filtered.IsSome);
    }

    [Fact]
    public void Filter_RejectsNonMatchingValue()
    {
        var option = Option<int>.Some(3);
        var filtered = option.Filter(x => x > 5);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public void AndThen_ChainsSome()
    {
        var option = Option<int>.Some(10);
        var chained = option.AndThen(x => Option<int>.Some(x * 2));

        Assert.True(chained.IsSome);
        Assert.Equal(20, chained.Unwrap());
    }

    [Fact]
    public void AndThen_PropagatesNone()
    {
        var option = Option<int>.None();
        var chained = option.AndThen(x => Option<int>.Some(x * 2));

        Assert.True(chained.IsNone);
    }

    [Fact]
    public void Or_ReturnsSomeWhenSome()
    {
        var option = Option<int>.Some(42);
        var result = option.Or(Option<int>.Some(0));

        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Or_ReturnsAlternativeWhenNone()
    {
        var option = Option<int>.None();
        var result = option.Or(Option<int>.Some(100));

        Assert.Equal(100, result.Unwrap());
    }

    [Fact]
    public void Xor_ReturnsWhenExactlyOneSome()
    {
        var some = Option<int>.Some(42);
        var none = Option<int>.None();

        Assert.True(some.Xor(none).IsSome);
        Assert.True(none.Xor(some).IsSome);
        Assert.True(some.Xor(some).IsNone);
        Assert.True(none.Xor(none).IsNone);
    }

    [Fact]
    public void Match_ExecutesSomeFuncOnSome()
    {
        var option = Option<int>.Some(42);
        var result = option.Match(v => $"Value: {v}", () => "None");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_ExecutesNoneFuncOnNone()
    {
        var option = Option<int>.None();
        var result = option.Match(v => $"Value: {v}", () => "None");

        Assert.Equal("None", result);
    }

    [Fact]
    public void OkOr_ConvertsToOkWhenSome()
    {
        var option = Option<int>.Some(42);
        var result = option.OkOr("error");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void OkOr_ConvertsToErrWhenNone()
    {
        var option = Option<int>.None();
        var result = option.OkOr("error");

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualSomes()
    {
        var opt1 = Option<int>.Some(42);
        var opt2 = Option<int>.Some(42);

        Assert.True(opt1.Equals(opt2));
        Assert.True(opt1 == opt2);
    }

    [Fact]
    public void Equals_ReturnsTrueForNones()
    {
        var opt1 = Option<int>.None();
        var opt2 = Option<int>.None();

        Assert.True(opt1.Equals(opt2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var opt1 = Option<int>.Some(42);
        var opt2 = Option<int>.Some(43);

        Assert.False(opt1.Equals(opt2));
        Assert.True(opt1 != opt2);
    }

    [Fact]
    public void GetHashCode_SameForEqualOptions()
    {
        var opt1 = Option<int>.Some(42);
        var opt2 = Option<int>.Some(42);

        Assert.Equal(opt1.GetHashCode(), opt2.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsSome()
    {
        var option = Option<int>.Some(42);
        Assert.Contains("Some", option.ToString());
        Assert.Contains("42", option.ToString());
    }

    [Fact]
    public void ToString_FormatsNone()
    {
        var option = Option<int>.None();
        Assert.Contains("None", option.ToString());
    }

    #endregion
}
