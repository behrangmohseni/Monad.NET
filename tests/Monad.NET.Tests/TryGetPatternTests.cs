using Xunit;

namespace Monad.NET.Tests;

public class TryGetPatternTests
{
    #region Option.TryGet Tests

    [Fact]
    public void Option_TryGet_Some_ReturnsTrueAndValue()
    {
        var option = Option<int>.Some(42);

        var success = option.TryGet(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Option_TryGet_None_ReturnsFalseAndDefault()
    {
        var option = Option<int>.None();

        var success = option.TryGet(out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void Option_TryGet_Some_WorksWithIfStatement()
    {
        var option = Option<string>.Some("hello");

        if (option.TryGet(out var value))
        {
            Assert.Equal("hello", value);
        }
        else
        {
            Assert.Fail("Expected TryGet to return true");
        }
    }

    [Fact]
    public void Option_TryGet_None_WorksWithIfStatement()
    {
        var option = Option<string>.None();
        var wasNone = false;

        if (!option.TryGet(out _))
        {
            wasNone = true;
        }

        Assert.True(wasNone);
    }

    [Fact]
    public void Option_TryGet_ReferenceType_Some_ReturnsValue()
    {
        var option = Option<string>.Some("test");

        Assert.True(option.TryGet(out var value));
        Assert.Equal("test", value);
    }

    [Fact]
    public void Option_TryGet_ReferenceType_None_ReturnsNull()
    {
        var option = Option<string>.None();

        Assert.False(option.TryGet(out var value));
        Assert.Null(value);
    }

    #endregion

    #region Result.TryGet Tests

    [Fact]
    public void Result_TryGet_Ok_ReturnsTrueAndValue()
    {
        var result = Result<int, string>.Ok(42);

        var success = result.TryGet(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Result_TryGet_Err_ReturnsFalseAndDefault()
    {
        var result = Result<int, string>.Err("error");

        var success = result.TryGet(out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void Result_TryGetError_Err_ReturnsTrueAndError()
    {
        var result = Result<int, string>.Err("error message");

        var hasError = result.TryGetError(out var error);

        Assert.True(hasError);
        Assert.Equal("error message", error);
    }

    [Fact]
    public void Result_TryGetError_Ok_ReturnsFalseAndDefault()
    {
        var result = Result<int, string>.Ok(42);

        var hasError = result.TryGetError(out var error);

        Assert.False(hasError);
        Assert.Null(error);
    }

    [Fact]
    public void Result_TryGet_WorksWithIfStatement()
    {
        var result = Result<string, int>.Ok("success");

        if (result.TryGet(out var value))
        {
            Assert.Equal("success", value);
        }
        else
        {
            Assert.Fail("Expected TryGet to return true");
        }
    }

    [Fact]
    public void Result_TryGetError_WorksWithIfStatement()
    {
        var result = Result<string, int>.Err(404);

        if (result.TryGetError(out var error))
        {
            Assert.Equal(404, error);
        }
        else
        {
            Assert.Fail("Expected TryGetError to return true");
        }
    }

    #endregion

    #region Try.TryGet Tests

    [Fact]
    public void Try_TryGet_Success_ReturnsTrueAndValue()
    {
        var tryResult = Try<int>.Success(42);

        var success = tryResult.TryGet(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Try_TryGet_Failure_ReturnsFalseAndDefault()
    {
        var tryResult = Try<int>.Failure(new Exception("error"));

        var success = tryResult.TryGet(out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void Try_TryGetException_Failure_ReturnsTrueAndException()
    {
        var exception = new InvalidOperationException("test error");
        var tryResult = Try<int>.Failure(exception);

        var hasException = tryResult.TryGetException(out var ex);

        Assert.True(hasException);
        Assert.Same(exception, ex);
    }

    [Fact]
    public void Try_TryGetException_Success_ReturnsFalseAndNull()
    {
        var tryResult = Try<int>.Success(42);

        var hasException = tryResult.TryGetException(out var ex);

        Assert.False(hasException);
        Assert.Null(ex);
    }

    [Fact]
    public void Try_TryGet_Of_Success_WorksWithIfStatement()
    {
        var tryResult = Try<int>.Of(() => int.Parse("42"));

        if (tryResult.TryGet(out var value))
        {
            Assert.Equal(42, value);
        }
        else
        {
            Assert.Fail("Expected TryGet to return true");
        }
    }

    [Fact]
    public void Try_TryGetException_Of_Failure_WorksWithIfStatement()
    {
        var tryResult = Try<int>.Of(() => int.Parse("not a number"));

        if (tryResult.TryGetException(out var ex))
        {
            Assert.IsType<FormatException>(ex);
        }
        else
        {
            Assert.Fail("Expected TryGetException to return true");
        }
    }

    #endregion

    #region Real-World Usage Patterns

    [Fact]
    public void TryGet_DictionaryStylePattern()
    {
        // Simulating dictionary-like usage
        Option<string> GetConfig(string key) =>
            key == "api_url" 
                ? Option<string>.Some("https://api.example.com") 
                : Option<string>.None();

        // Familiar dictionary TryGetValue pattern
        if (GetConfig("api_url").TryGet(out var url))
        {
            Assert.Equal("https://api.example.com", url);
        }

        // Missing key
        Assert.False(GetConfig("missing_key").TryGet(out _));
    }

    [Fact]
    public void TryGet_ParseStylePattern()
    {
        // Simulating parse-like usage
        Result<int, string> ParseInt(string input)
        {
            if (int.TryParse(input, out var result))
                return Result<int, string>.Ok(result);
            return Result<int, string>.Err($"Cannot parse '{input}' as int");
        }

        // Familiar TryParse pattern
        if (ParseInt("123").TryGet(out var value))
        {
            Assert.Equal(123, value);
        }

        // Failed parse
        var parseResult = ParseInt("abc");
        if (parseResult.TryGetError(out var error))
        {
            Assert.Contains("Cannot parse", error);
        }
    }

    #endregion
}

