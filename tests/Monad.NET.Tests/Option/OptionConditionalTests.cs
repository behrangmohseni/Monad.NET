using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for Option conditional factory methods and extension methods.
/// </summary>
public class OptionConditionalTests
{
    #region When Tests

    [Fact]
    public void When_ConditionTrue_ReturnsSome()
    {
        var result = OptionExtensions.When(true, () => 42);
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void When_ConditionFalse_ReturnsNone()
    {
        var result = OptionExtensions.When(false, () => 42);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void When_WithValue_ConditionTrue_ReturnsSome()
    {
        var result = OptionExtensions.When(true, 42);
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void When_WithValue_ConditionFalse_ReturnsNone()
    {
        var result = OptionExtensions.When(false, 42);
        Assert.True(result.IsNone);
    }

    #endregion

    #region Unless Tests

    [Fact]
    public void Unless_ConditionFalse_ReturnsSome()
    {
        var result = OptionExtensions.Unless(false, () => 42);
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Unless_ConditionTrue_ReturnsNone()
    {
        var result = OptionExtensions.Unless(true, () => 42);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void Unless_WithValue_ConditionFalse_ReturnsSome()
    {
        var result = OptionExtensions.Unless(false, 42);
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Unless_WithValue_ConditionTrue_ReturnsNone()
    {
        var result = OptionExtensions.Unless(true, 42);
        Assert.True(result.IsNone);
    }

    #endregion

    #region DefaultIfNone Tests

    [Fact]
    public void DefaultIfNone_Some_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);
        var result = option.DefaultIfNone(99);
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void DefaultIfNone_None_ReturnsDefault()
    {
        var option = Option<int>.None();
        var result = option.DefaultIfNone(99);
        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void DefaultIfNone_WithFactory_Some_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);
        var factoryCalled = false;
        var result = option.DefaultIfNone(() =>
        {
            factoryCalled = true;
            return 99;
        });

        Assert.False(factoryCalled);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void DefaultIfNone_WithFactory_None_CallsFactory()
    {
        var option = Option<int>.None();
        var result = option.DefaultIfNone(() => 99);
        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    #endregion

    #region ThrowIfNone Tests

    [Fact]
    public void ThrowIfNone_Some_ReturnsValue()
    {
        var option = Option<int>.Some(42);
        var result = option.ThrowIfNone(new InvalidOperationException("test"));
        Assert.Equal(42, result);
    }

    [Fact]
    public void ThrowIfNone_None_Throws()
    {
        var option = Option<int>.None();
        Assert.Throws<InvalidOperationException>(() =>
            option.ThrowIfNone(new InvalidOperationException("test")));
    }

    [Fact]
    public void ThrowIfNone_WithFactory_Some_ReturnsValue()
    {
        var option = Option<int>.Some(42);
        var result = option.ThrowIfNone(() => new InvalidOperationException("test"));
        Assert.Equal(42, result);
    }

    [Fact]
    public void ThrowIfNone_WithFactory_None_Throws()
    {
        var option = Option<int>.None();
        Assert.Throws<InvalidOperationException>(() =>
            option.ThrowIfNone(() => new InvalidOperationException("test")));
    }

    #endregion

    #region GetOption Dictionary Tests

    [Fact]
    public void GetOption_KeyExists_ReturnsSome()
    {
        var dict = new Dictionary<string, int> { ["key"] = 42 };
        IReadOnlyDictionary<string, int> readOnlyDict = dict;
        var result = readOnlyDict.GetOption("key");

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void GetOption_KeyMissing_ReturnsNone()
    {
        var dict = new Dictionary<string, int> { ["key"] = 42 };
        IReadOnlyDictionary<string, int> readOnlyDict = dict;
        var result = readOnlyDict.GetOption("missing");

        Assert.True(result.IsNone);
    }

    #endregion

    #region FirstOption Tests

    [Fact]
    public void FirstOption_NonEmpty_ReturnsSome()
    {
        var list = new[] { 1, 2, 3 };
        var result = list.FirstOption();

        Assert.True(result.IsSome);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void FirstOption_Empty_ReturnsNone()
    {
        var list = Array.Empty<int>();
        var result = list.FirstOption();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void FirstOption_WithPredicate_Found_ReturnsSome()
    {
        var list = new[] { 1, 2, 3, 4, 5 };
        var result = list.FirstOption(x => x > 3);

        Assert.True(result.IsSome);
        Assert.Equal(4, result.GetValue());
    }

    [Fact]
    public void FirstOption_WithPredicate_NotFound_ReturnsNone()
    {
        var list = new[] { 1, 2, 3 };
        var result = list.FirstOption(x => x > 10);

        Assert.True(result.IsNone);
    }

    #endregion

    #region LastOption Tests

    [Fact]
    public void LastOption_NonEmpty_ReturnsSome()
    {
        var list = new[] { 1, 2, 3 };
        var result = list.LastOption();

        Assert.True(result.IsSome);
        Assert.Equal(3, result.GetValue());
    }

    [Fact]
    public void LastOption_Empty_ReturnsNone()
    {
        var list = Array.Empty<int>();
        var result = list.LastOption();

        Assert.True(result.IsNone);
    }

    #endregion

    #region SingleOption Tests

    [Fact]
    public void SingleOption_SingleElement_ReturnsSome()
    {
        var list = new[] { 42 };
        var result = list.SingleOption();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void SingleOption_Empty_ReturnsNone()
    {
        var list = Array.Empty<int>();
        var result = list.SingleOption();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void SingleOption_MultipleElements_ReturnsNone()
    {
        var list = new[] { 1, 2, 3 };
        var result = list.SingleOption();

        Assert.True(result.IsNone);
    }

    #endregion

    #region ElementAtOption Tests

    [Fact]
    public void ElementAtOption_ValidIndex_ReturnsSome()
    {
        var list = new[] { 10, 20, 30 };
        var result = list.ElementAtOption(1);

        Assert.True(result.IsSome);
        Assert.Equal(20, result.GetValue());
    }

    [Fact]
    public void ElementAtOption_NegativeIndex_ReturnsNone()
    {
        var list = new[] { 10, 20, 30 };
        var result = list.ElementAtOption(-1);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void ElementAtOption_OutOfRange_ReturnsNone()
    {
        var list = new[] { 10, 20, 30 };
        var result = list.ElementAtOption(5);

        Assert.True(result.IsNone);
    }

    #endregion

    #region OfType Tests

    [Fact]
    public void OfType_MatchingType_ReturnsSome()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfType<string>();

        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void OfType_NonMatchingType_ReturnsNone()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfType<int>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfType_None_ReturnsNone()
    {
        var option = Option<object>.None();
        var result = option.OfType<string>();

        Assert.True(result.IsNone);
    }

    #endregion

    #region Flatten Tests

    [Fact]
    public void Flatten_SomeSome_ReturnsSome()
    {
        var nested = Option<Option<int>>.Some(Option<int>.Some(42));
        var result = nested.Flatten();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Flatten_SomeNone_ReturnsNone()
    {
        var nested = Option<Option<int>>.Some(Option<int>.None());
        var result = nested.Flatten();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Flatten_None_ReturnsNone()
    {
        var nested = Option<Option<int>>.None();
        var result = nested.Flatten();

        Assert.True(result.IsNone);
    }

    #endregion

    #region Transpose Tests

    [Fact]
    public void Transpose_SomeOk_ReturnsSome()
    {
        var option = Option<Result<int, string>>.Some(Result<int, string>.Ok(42));
        var result = option.Transpose();

        Assert.True(result.IsOk);
        Assert.True(result.GetValue().IsSome);
    }

    [Fact]
    public void Transpose_SomeErr_ReturnsErr()
    {
        var option = Option<Result<int, string>>.Some(Result<int, string>.Error("error"));
        var result = option.Transpose();

        Assert.True(result.IsError);
    }

    [Fact]
    public void Transpose_None_ReturnsOkNone()
    {
        var option = Option<Result<int, string>>.None();
        var result = option.Transpose();

        Assert.True(result.IsOk);
        Assert.True(result.GetValue().IsNone);
    }

    #endregion

    #region ToOption Nullable Tests

    [Fact]
    public void ToOption_NullableStruct_HasValue_ReturnsSome()
    {
        int? nullable = 42;
        var result = nullable.ToOption();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToOption_NullableStruct_NoValue_ReturnsNone()
    {
        int? nullable = null;
        var result = nullable.ToOption();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOption_ReferenceType_NotNull_ReturnsSome()
    {
        string? value = "hello";
        var result = value.ToOption();

        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void ToOption_ReferenceType_Null_ReturnsNone()
    {
        string? value = null;
        var result = value.ToOption();

        Assert.True(result.IsNone);
    }

    #endregion
}

