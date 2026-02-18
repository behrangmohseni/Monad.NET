using System.Collections.Immutable;
using Xunit;

namespace Monad.NET.Tests;

public class ValuePropertyGuardTests
{
    #region Option<T>.Value

    [Fact]
    public void Option_Value_OnSome_ReturnsValue()
    {
        var option = Option<int>.Some(42);
        Assert.Equal(42, option.Value);
    }

    [Fact]
    public void Option_Value_OnNone_Throws()
    {
        var option = Option<int>.None();
        Assert.Throws<InvalidOperationException>(() => option.Value);
    }

    [Fact]
    public void Option_Value_OnDefault_Throws()
    {
        var option = default(Option<int>);
        Assert.Throws<InvalidOperationException>(() => option.Value);
    }

    [Fact]
    public void Option_Value_PatternMatching_Works()
    {
        var some = Option<string>.Some("hello");
        var none = Option<string>.None();

        var someResult = some switch
        {
            { IsSome: true, Value: var v } => v,
            _ => "default"
        };
        var noneResult = none switch
        {
            { IsSome: true, Value: var v } => v,
            { IsNone: true } => "nothing",
            _ => "default"
        };

        Assert.Equal("hello", someResult);
        Assert.Equal("nothing", noneResult);
    }

    #endregion

    #region Result<T, TError>.Value and ErrorValue

    [Fact]
    public void Result_Value_OnOk_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Result_Value_OnError_Throws()
    {
        var result = Result<int, string>.Error("oops");
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Result_ErrorValue_OnError_ReturnsError()
    {
        var result = Result<int, string>.Error("oops");
        Assert.Equal("oops", result.ErrorValue);
    }

    [Fact]
    public void Result_ErrorValue_OnOk_Throws()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Throws<InvalidOperationException>(() => result.ErrorValue);
    }

    [Fact]
    public void Result_Value_PatternMatching_Works()
    {
        var ok = Result<int, string>.Ok(42);
        var err = Result<int, string>.Error("fail");

        var okResult = ok switch
        {
            { IsOk: true, Value: var v } => $"ok:{v}",
            { IsError: true, ErrorValue: var e } => $"err:{e}",
            _ => "unknown"
        };
        var errResult = err switch
        {
            { IsOk: true, Value: var v } => $"ok:{v}",
            { IsError: true, ErrorValue: var e } => $"err:{e}",
            _ => "unknown"
        };

        Assert.Equal("ok:42", okResult);
        Assert.Equal("err:fail", errResult);
    }

    #endregion

    #region Try<T>.Value and Exception

    [Fact]
    public void Try_Value_OnSuccess_ReturnsValue()
    {
        var t = Try<int>.Ok(42);
        Assert.Equal(42, t.Value);
    }

    [Fact]
    public void Try_Value_OnFailure_Throws()
    {
        var t = Try<int>.Error(new Exception("fail"));
        Assert.Throws<InvalidOperationException>(() => t.Value);
    }

    [Fact]
    public void Try_Exception_OnFailure_ReturnsException()
    {
        var ex = new Exception("fail");
        var t = Try<int>.Error(ex);
        Assert.Same(ex, t.Exception);
    }

    [Fact]
    public void Try_Exception_OnSuccess_Throws()
    {
        var t = Try<int>.Ok(42);
        Assert.Throws<InvalidOperationException>(() => t.Exception);
    }

    [Fact]
    public void Try_Value_PatternMatching_Works()
    {
        var ok = Try<int>.Ok(42);
        var fail = Try<int>.Error(new Exception("boom"));

        var okResult = ok switch
        {
            { IsOk: true, Value: var v } => $"ok:{v}",
            { IsError: true, Exception: var e } => $"err:{e!.Message}",
            _ => "unknown"
        };
        var failResult = fail switch
        {
            { IsOk: true, Value: var v } => $"ok:{v}",
            { IsError: true, Exception: var e } => $"err:{e!.Message}",
            _ => "unknown"
        };

        Assert.Equal("ok:42", okResult);
        Assert.Equal("err:boom", failResult);
    }

    #endregion

    #region Validation<T, TError>.Value and Errors

    [Fact]
    public void Validation_Value_OnValid_ReturnsValue()
    {
        var v = Validation<int, string>.Ok(42);
        Assert.Equal(42, v.Value);
    }

    [Fact]
    public void Validation_Value_OnInvalid_Throws()
    {
        var v = Validation<int, string>.Error("err");
        Assert.Throws<InvalidOperationException>(() => v.Value);
    }

    [Fact]
    public void Validation_Errors_OnInvalid_ReturnsErrors()
    {
        var v = Validation<int, string>.Error("err");
        Assert.Equal(new[] { "err" }, v.Errors);
    }

    [Fact]
    public void Validation_Errors_OnValid_Throws()
    {
        var v = Validation<int, string>.Ok(42);
        Assert.Throws<InvalidOperationException>(() => v.Errors);
    }

    [Fact]
    public void Validation_Value_PatternMatching_Works()
    {
        var valid = Validation<int, string>.Ok(42);
        var invalid = Validation<int, string>.Error("bad");

        var validResult = valid switch
        {
            { IsOk: true, Value: var val } => $"ok:{val}",
            { IsError: true, Errors: var e } => $"err:{e.Length}",
            _ => "unknown"
        };
        var invalidResult = invalid switch
        {
            { IsOk: true, Value: var val } => $"ok:{val}",
            { IsError: true, Errors: var e } => $"err:{e.Length}",
            _ => "unknown"
        };

        Assert.Equal("ok:42", validResult);
        Assert.Equal("err:1", invalidResult);
    }

    #endregion
}
