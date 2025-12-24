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
}
