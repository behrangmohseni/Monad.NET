using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for Option parsing extension methods.
/// </summary>
public class OptionParsingTests
{
    #region ParseInt Tests

    [Fact]
    public void ParseInt_ValidString_ReturnsSome()
    {
        var result = "42".ParseInt();
        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void ParseInt_InvalidString_ReturnsNone()
    {
        var result = "not a number".ParseInt();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseInt_Null_ReturnsNone()
    {
        string? value = null;
        var result = value.ParseInt();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseInt_Empty_ReturnsNone()
    {
        var result = "".ParseInt();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseLong Tests

    [Fact]
    public void ParseLong_ValidString_ReturnsSome()
    {
        var result = "9223372036854775807".ParseLong();
        Assert.True(result.IsSome);
        Assert.Equal(long.MaxValue, result.Unwrap());
    }

    [Fact]
    public void ParseLong_InvalidString_ReturnsNone()
    {
        var result = "not a number".ParseLong();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseDouble Tests

    [Fact]
    public void ParseDouble_ValidString_ReturnsSome()
    {
        var result = "3.14159".ParseDouble();
        Assert.True(result.IsSome);
        Assert.True(Math.Abs(result.Unwrap() - 3.14159) < 0.0001);
    }

    [Fact]
    public void ParseDouble_InvalidString_ReturnsNone()
    {
        var result = "not a number".ParseDouble();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseDecimal Tests

    [Fact]
    public void ParseDecimal_ValidString_ReturnsSome()
    {
        var result = "123.45".ParseDecimal();
        Assert.True(result.IsSome);
        Assert.Equal(123.45m, result.Unwrap());
    }

    [Fact]
    public void ParseDecimal_InvalidString_ReturnsNone()
    {
        var result = "not a number".ParseDecimal();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseBool Tests

    [Fact]
    public void ParseBool_True_ReturnsSome()
    {
        var result = "true".ParseBool();
        Assert.True(result.IsSome);
        Assert.True(result.Unwrap());
    }

    [Fact]
    public void ParseBool_False_ReturnsSome()
    {
        var result = "false".ParseBool();
        Assert.True(result.IsSome);
        Assert.False(result.Unwrap());
    }

    [Fact]
    public void ParseBool_InvalidString_ReturnsNone()
    {
        var result = "maybe".ParseBool();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseGuid Tests

    [Fact]
    public void ParseGuid_ValidString_ReturnsSome()
    {
        var guid = Guid.NewGuid();
        var result = guid.ToString().ParseGuid();
        Assert.True(result.IsSome);
        Assert.Equal(guid, result.Unwrap());
    }

    [Fact]
    public void ParseGuid_InvalidString_ReturnsNone()
    {
        var result = "not-a-guid".ParseGuid();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseDateTime Tests

    [Fact]
    public void ParseDateTime_ValidString_ReturnsSome()
    {
        var result = "2024-01-15".ParseDateTime();
        Assert.True(result.IsSome);
        Assert.Equal(new DateTime(2024, 1, 15), result.Unwrap().Date);
    }

    [Fact]
    public void ParseDateTime_InvalidString_ReturnsNone()
    {
        var result = "not a date".ParseDateTime();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseDateTimeOffset Tests

    [Fact]
    public void ParseDateTimeOffset_ValidString_ReturnsSome()
    {
        var result = "2024-01-15T12:00:00Z".ParseDateTimeOffset();
        Assert.True(result.IsSome);
    }

    [Fact]
    public void ParseDateTimeOffset_InvalidString_ReturnsNone()
    {
        var result = "not a date".ParseDateTimeOffset();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseTimeSpan Tests

    [Fact]
    public void ParseTimeSpan_ValidString_ReturnsSome()
    {
        var result = "01:30:00".ParseTimeSpan();
        Assert.True(result.IsSome);
        Assert.Equal(TimeSpan.FromMinutes(90), result.Unwrap());
    }

    [Fact]
    public void ParseTimeSpan_InvalidString_ReturnsNone()
    {
        var result = "not a timespan".ParseTimeSpan();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ParseEnum Tests

    private enum TestColor { Red, Green, Blue }

    [Fact]
    public void ParseEnum_ValidString_ReturnsSome()
    {
        var result = "Green".ParseEnum<TestColor>();
        Assert.True(result.IsSome);
        Assert.Equal(TestColor.Green, result.Unwrap());
    }

    [Fact]
    public void ParseEnum_CaseInsensitive_ReturnsSome()
    {
        var result = "green".ParseEnum<TestColor>();
        Assert.True(result.IsSome);
        Assert.Equal(TestColor.Green, result.Unwrap());
    }

    [Fact]
    public void ParseEnum_CaseSensitive_ReturnsNone()
    {
        var result = "green".ParseEnum<TestColor>(ignoreCase: false);
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ParseEnum_InvalidString_ReturnsNone()
    {
        var result = "Yellow".ParseEnum<TestColor>();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ToOptionNotEmpty Tests

    [Fact]
    public void ToOptionNotEmpty_NonEmpty_ReturnsSome()
    {
        var result = "hello".ToOptionNotEmpty();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.Unwrap());
    }

    [Fact]
    public void ToOptionNotEmpty_Empty_ReturnsNone()
    {
        var result = "".ToOptionNotEmpty();
        Assert.True(result.IsNone);
    }

    [Fact]
    public void ToOptionNotEmpty_Null_ReturnsNone()
    {
        string? value = null;
        var result = value.ToOptionNotEmpty();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ToOptionNotWhiteSpace Tests

    [Fact]
    public void ToOptionNotWhiteSpace_NonEmpty_ReturnsSome()
    {
        var result = "hello".ToOptionNotWhiteSpace();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.Unwrap());
    }

    [Fact]
    public void ToOptionNotWhiteSpace_WhiteSpace_ReturnsNone()
    {
        var result = "   ".ToOptionNotWhiteSpace();
        Assert.True(result.IsNone);
    }

    #endregion

    #region ToOptionTrimmed Tests

    [Fact]
    public void ToOptionTrimmed_NonEmpty_ReturnsTrimmed()
    {
        var result = "  hello  ".ToOptionTrimmed();
        Assert.True(result.IsSome);
        Assert.Equal("hello", result.Unwrap());
    }

    [Fact]
    public void ToOptionTrimmed_WhiteSpaceOnly_ReturnsNone()
    {
        var result = "   ".ToOptionTrimmed();
        Assert.True(result.IsNone);
    }

    #endregion
}

