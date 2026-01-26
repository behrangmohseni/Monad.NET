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

    #region Parse Conversions

    [Fact]
    public void ParseInt_WithValidInt_ReturnsSome()
    {
        var result = "42".ParseInt();
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ParseInt_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseInt();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseInt_WithNull_ReturnsNone()
    {
        string? nullString = null;
        var result = nullString.ParseInt();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseInt_WithNegativeNumber_ReturnsSome()
    {
        var result = "-42".ParseInt();
        Assert.True(result.IsSome);
        Assert.Equal(-42, result.GetValue());
    }

    [Fact]
    public void ParseLong_WithValidLong_ReturnsSome()
    {
        var result = "9223372036854775807".ParseLong();
        Assert.True(result.IsSome);
        Assert.Equal(long.MaxValue, result.GetValue());
    }

    [Fact]
    public void ParseLong_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseLong();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseDouble_WithValidDouble_ReturnsSome()
    {
        var result = "3.14".ParseDouble();
        Assert.True(result.IsSome);
        Assert.Equal(3.14, result.GetValue(), 2);
    }

    [Fact]
    public void ParseDouble_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseDouble();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseDecimal_WithValidDecimal_ReturnsSome()
    {
        var result = "123.45".ParseDecimal();
        Assert.True(result.IsSome);
        Assert.Equal(123.45m, result.GetValue());
    }

    [Fact]
    public void ParseDecimal_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseDecimal();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseBool_WithTrue_ReturnsSomeTrue()
    {
        var result = "true".ParseBool();
        Assert.True(result.IsSome);
        Assert.True(result.GetValue());
    }

    [Fact]
    public void ParseBool_WithFalse_ReturnsSomeFalse()
    {
        var result = "false".ParseBool();
        Assert.True(result.IsSome);
        Assert.False(result.GetValue());
    }

    [Fact]
    public void ParseBool_WithInvalidString_ReturnsNone()
    {
        var result = "yes".ParseBool();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseGuid_WithValidGuid_ReturnsSome()
    {
        var guidString = "550e8400-e29b-41d4-a716-446655440000";
        var result = guidString.ParseGuid();
        Assert.True(result.IsSome);
        Assert.Equal(Guid.Parse(guidString), result.GetValue());
    }

    [Fact]
    public void ParseGuid_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseGuid();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseDateTime_WithValidDate_ReturnsSome()
    {
        var result = "2024-01-15".ParseDateTime();
        Assert.True(result.IsSome);
        Assert.Equal(new DateTime(2024, 1, 15), result.GetValue().Date);
    }

    [Fact]
    public void ParseDateTime_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseDateTime();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseDateTimeOffset_WithValidDate_ReturnsSome()
    {
        var result = "2024-01-15T10:30:00+00:00".ParseDateTimeOffset();
        Assert.True(result.IsSome);
    }

    [Fact]
    public void ParseDateTimeOffset_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseDateTimeOffset();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseTimeSpan_WithValidTimeSpan_ReturnsSome()
    {
        var result = "01:30:00".ParseTimeSpan();
        Assert.True(result.IsSome);
        Assert.Equal(TimeSpan.FromHours(1.5), result.GetValue());
    }

    [Fact]
    public void ParseTimeSpan_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseTimeSpan();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseEnum_WithValidEnumValue_ReturnsSome()
    {
        var result = "Monday".ParseEnum<DayOfWeek>();
        Assert.True(result.IsSome);
        Assert.Equal(DayOfWeek.Monday, result.GetValue());
    }

    [Fact]
    public void ParseEnum_WithCaseInsensitive_ReturnsSome()
    {
        var result = "monday".ParseEnum<DayOfWeek>();
        Assert.True(result.IsSome);
        Assert.Equal(DayOfWeek.Monday, result.GetValue());
    }

    [Fact]
    public void ParseEnum_WithCaseSensitive_ReturnsNone()
    {
        var result = "monday".ParseEnum<DayOfWeek>(ignoreCase: false);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseEnum_WithInvalidString_ReturnsNone()
    {
        var result = "invalid".ParseEnum<DayOfWeek>();
        Assert.True(result.IsNone);
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
    public void ChainedParsing_Works()
    {
        var result = "42"
            .ToOptionNotWhiteSpace()
            .Bind(s => s.ParseInt());

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ChainedParsing_WithInvalidInput_ReturnsNone()
    {
        var result = "   "
            .ToOptionNotWhiteSpace()
            .Bind(s => s.ParseInt());

        Assert.True(result.IsNone);
    }

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

