using Xunit;

namespace Monad.NET.Tests;

public class StringNullableConversionTests
{
    #region String Conversions

    [Fact]
    public void ToOptionNotEmpty_WithNonEmptyString_ReturnsSome()
    {
        var result = "hello".ToOptionNotEmpty();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void ToOptionNotEmpty_WithEmptyString_ReturnsNone()
    {
        var result = "".ToOptionNotEmpty();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionNotEmpty_WithNull_ReturnsNone()
    {
        string? nullString = null;
        var result = nullString.ToOptionNotEmpty();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionNotEmpty_WithWhitespace_ReturnsSome()
    {
        // Whitespace is not empty
        var result = "   ".ToOptionNotEmpty();
        Assert.True(result.IsSome);
        Assert.Equal("   ", result.GetValue());
    }

    [Fact]
    public void ToOptionNotWhiteSpace_WithNonWhitespaceString_ReturnsSome()
    {
        var result = "hello".ToOptionNotWhiteSpace();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void ToOptionNotWhiteSpace_WithWhitespace_ReturnsNone()
    {
        var result = "   ".ToOptionNotWhiteSpace();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionNotWhiteSpace_WithEmptyString_ReturnsNone()
    {
        var result = "".ToOptionNotWhiteSpace();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionNotWhiteSpace_WithNull_ReturnsNone()
    {
        string? nullString = null;
        var result = nullString.ToOptionNotWhiteSpace();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionTrimmed_WithPaddedString_ReturnsTrimmed()
    {
        var result = "  hello  ".ToOptionTrimmed();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void ToOptionTrimmed_WithWhitespaceOnly_ReturnsNone()
    {
        var result = "   ".ToOptionTrimmed();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionTrimmed_WithEmptyString_ReturnsNone()
    {
        var result = "".ToOptionTrimmed();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionTrimmed_WithNull_ReturnsNone()
    {
        string? nullString = null;
        var result = nullString.ToOptionTrimmed();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionTrimmed_WithNoWhitespace_ReturnsUnchanged()
    {
        var result = "hello".ToOptionTrimmed();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    #endregion

    #region Dictionary Lookups

    [Fact]
    public void GetOption_WithExistingKey_ReturnsSome()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var result = dict.GetOption("a");
        Assert.True(result.IsSome);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void GetOption_WithMissingKey_ReturnsNone()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var result = dict.GetOption("c");
        Assert.True(result.IsNone);
    }

    [Fact]
    public void GetOption_WithIntKeys_ReturnsSome()
    {
        var dict = new Dictionary<int, string> { [1] = "one", [2] = "two" };
        var result = dict.GetOption(1);
        Assert.True(result.IsSome);
        Assert.Equal("one", result.GetValue());
    }

    [Fact]
    public void GetOption_WithIntKeys_MissingKey_ReturnsNone()
    {
        var dict = new Dictionary<int, string> { [1] = "one", [2] = "two" };
        var result = dict.GetOption(3);
        Assert.True(result.IsNone);
    }

    #endregion

    #region Collection Lookups

    [Fact]
    public void FirstOption_WithNonEmptyCollection_ReturnsSome()
    {
        var result = new[] { 1, 2, 3 }.FirstOption();
        Assert.True(result.IsSome);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void FirstOption_WithEmptyCollection_ReturnsNone()
    {
        var result = Array.Empty<int>().FirstOption();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void FirstOption_WithPredicate_MatchFound_ReturnsSome()
    {
        var result = new[] { 1, 2, 3 }.FirstOption(x => x > 1);
        Assert.True(result.IsSome);
        Assert.Equal(2, result.GetValue());
    }

    [Fact]
    public void FirstOption_WithPredicate_NoMatch_ReturnsNone()
    {
        var result = new[] { 1, 2, 3 }.FirstOption(x => x > 10);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void FirstOption_WithList_ReturnsSome()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = list.FirstOption();
        Assert.True(result.IsSome);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void LastOption_WithNonEmptyCollection_ReturnsSome()
    {
        var result = new[] { 1, 2, 3 }.LastOption();
        Assert.True(result.IsSome);
        Assert.Equal(3, result.GetValue());
    }

    [Fact]
    public void LastOption_WithEmptyCollection_ReturnsNone()
    {
        var result = Array.Empty<int>().LastOption();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void LastOption_WithSingleElement_ReturnsSome()
    {
        var result = new[] { 42 }.LastOption();
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void LastOption_WithList_ReturnsSome()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = list.LastOption();
        Assert.True(result.IsSome);
        Assert.Equal(3, result.GetValue());
    }

    [Fact]
    public void SingleOption_WithSingleElement_ReturnsSome()
    {
        var result = new[] { 42 }.SingleOption();
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void SingleOption_WithMultipleElements_ReturnsNone()
    {
        var result = new[] { 1, 2 }.SingleOption();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void SingleOption_WithEmptyCollection_ReturnsNone()
    {
        var result = Array.Empty<int>().SingleOption();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void SingleOption_WithList_ReturnsSome()
    {
        var list = new List<int> { 42 };
        var result = list.SingleOption();
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ElementAtOption_WithValidIndex_ReturnsSome()
    {
        var result = new[] { 1, 2, 3 }.ElementAtOption(1);
        Assert.True(result.IsSome);
        Assert.Equal(2, result.GetValue());
    }

    [Fact]
    public void ElementAtOption_WithOutOfRangeIndex_ReturnsNone()
    {
        var result = new[] { 1, 2, 3 }.ElementAtOption(10);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ElementAtOption_WithNegativeIndex_ReturnsNone()
    {
        var result = new[] { 1, 2, 3 }.ElementAtOption(-1);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ElementAtOption_WithList_ReturnsSome()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = list.ElementAtOption(1);
        Assert.True(result.IsSome);
        Assert.Equal(2, result.GetValue());
    }

    [Fact]
    public void ElementAtOption_WithEnumerable_ReturnsSome()
    {
        var result = Enumerable.Range(1, 5).ElementAtOption(2);
        Assert.True(result.IsSome);
        Assert.Equal(3, result.GetValue());
    }

    #endregion

    #region Chaining Examples

    [Fact]
    public void DictionaryLookup_ChainedWithMap_Works()
    {
        var dict = new Dictionary<string, string> { ["name"] = "  john  " };

        var result = dict.GetOption("name")
            .Bind(s => s.ToOptionTrimmed())
            .Map(s => s.ToUpper());

        Assert.True(result.IsSome);
        Assert.Equal("JOHN", result.GetValue());
    }

    #endregion
}

