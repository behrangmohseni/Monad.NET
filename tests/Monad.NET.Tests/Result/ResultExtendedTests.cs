using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Result<T, E> to improve code coverage.
/// </summary>
public class ResultExtendedTests
{
    #region Contains and Exists Tests

    [Fact]
    public void Contains_OnOk_WithMatchingValue_ReturnsTrue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.True(result.Contains(42));
    }

    [Fact]
    public void Contains_OnOk_WithNonMatchingValue_ReturnsFalse()
    {
        var result = Result<int, string>.Ok(42);
        Assert.False(result.Contains(99));
    }

    [Fact]
    public void Contains_OnErr_ReturnsFalse()
    {
        var result = Result<int, string>.Error("error");
        Assert.False(result.Contains(42));
    }

    [Fact]
    public void Exists_OnOk_WithMatchingPredicate_ReturnsTrue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.True(result.Exists(x => x > 40));
    }

    [Fact]
    public void Exists_OnOk_WithNonMatchingPredicate_ReturnsFalse()
    {
        var result = Result<int, string>.Ok(42);
        Assert.False(result.Exists(x => x > 50));
    }

    [Fact]
    public void Exists_OnErr_ReturnsFalse()
    {
        var result = Result<int, string>.Error("error");
        Assert.False(result.Exists(x => x > 0));
    }

    #endregion

    #region OrElse and Or Tests

    [Fact]
    public void OrElse_OnOk_ReturnsOriginal()
    {
        var result = Result<int, string>.Ok(42);
        var final = result.OrElse(_ => Result<int, string>.Ok(99));

        Assert.True(final.IsOk);
        Assert.Equal(42, final.GetValue());
    }

    [Fact]
    public void OrElse_OnErr_ReturnsAlternative()
    {
        var result = Result<int, string>.Error("error");
        var final = result.OrElse(err => Result<int, string>.Ok(99));

        Assert.True(final.IsOk);
        Assert.Equal(99, final.GetValue());
    }

    [Fact]
    public void Or_OnOk_ReturnsOriginal()
    {
        var result = Result<int, string>.Ok(42);
        var final = result.Or(Result<int, string>.Ok(99));

        Assert.True(final.IsOk);
        Assert.Equal(42, final.GetValue());
    }

    [Fact]
    public void Or_OnErr_ReturnsAlternative()
    {
        var result = Result<int, string>.Error("error");
        var final = result.Or(Result<int, string>.Ok(99));

        Assert.True(final.IsOk);
        Assert.Equal(99, final.GetValue());
    }

    #endregion

    #region Flatten Tests

    [Fact]
    public void Flatten_NestedOk_ReturnsInner()
    {
        var nested = Result<Result<int, string>, string>.Ok(Result<int, string>.Ok(42));
        var result = nested.Flatten();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Flatten_OuterErr_ReturnsErr()
    {
        var nested = Result<Result<int, string>, string>.Error("outer error");
        var result = nested.Flatten();

        Assert.True(result.IsError);
        Assert.Equal("outer error", result.GetError());
    }

    [Fact]
    public void Flatten_InnerErr_ReturnsErr()
    {
        var nested = Result<Result<int, string>, string>.Ok(Result<int, string>.Error("inner error"));
        var result = nested.Flatten();

        Assert.True(result.IsError);
        Assert.Equal("inner error", result.GetError());
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_OkWithSameValue_ReturnsTrue()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(42);

        Assert.True(result1.Equals(result2));
        Assert.True(result1 == result2);
    }

    [Fact]
    public void Equals_OkWithDifferentValue_ReturnsFalse()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(99);

        Assert.False(result1.Equals(result2));
        Assert.True(result1 != result2);
    }

    [Fact]
    public void Equals_ErrWithSameValue_ReturnsTrue()
    {
        var result1 = Result<int, string>.Error("error");
        var result2 = Result<int, string>.Error("error");

        Assert.True(result1.Equals(result2));
    }

    [Fact]
    public void Equals_ErrWithDifferentValue_ReturnsFalse()
    {
        var result1 = Result<int, string>.Error("error1");
        var result2 = Result<int, string>.Error("error2");

        Assert.False(result1.Equals(result2));
    }

    [Fact]
    public void Equals_OkWithErr_ReturnsFalse()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Error("error");

        Assert.False(result1.Equals(result2));
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        var result = Result<int, string>.Ok(42);

        Assert.False(result.Equals(null));
        Assert.False(result.Equals("not a result"));
        Assert.True(result.Equals((object)Result<int, string>.Ok(42)));
    }

    [Fact]
    public void GetHashCode_SameForEqualResults()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(42);

        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnOk_ContainsValue()
    {
        var result = Result<int, string>.Ok(42);
        var str = result.ToString();

        Assert.Contains("42", str);
        Assert.Contains("Ok", str);
    }

    [Fact]
    public void ToString_OnErr_ContainsError()
    {
        var result = Result<int, string>.Error("error message");
        var str = result.ToString();

        Assert.Contains("error message", str);
        Assert.Contains("Err", str);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_OnOk_ExecutesAction()
    {
        var result = Result<int, string>.Ok(42);
        var executed = false;

        var tapped = result.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(tapped.IsOk);
    }

    [Fact]
    public void Tap_OnErr_DoesNotExecuteAction()
    {
        var result = Result<int, string>.Error("error");
        var executed = false;

        var tapped = result.Tap(x => executed = true);

        Assert.False(executed);
        Assert.True(tapped.IsError);
    }

    [Fact]
    public void TapErr_OnErr_ExecutesAction()
    {
        var result = Result<int, string>.Error("error");
        var executed = false;

        var tapped = result.TapError(x => executed = true);

        Assert.True(executed);
        Assert.True(tapped.IsError);
    }

    [Fact]
    public void TapErr_OnOk_DoesNotExecuteAction()
    {
        var result = Result<int, string>.Ok(42);
        var executed = false;

        var tapped = result.TapError(x => executed = true);

        Assert.False(executed);
        Assert.True(tapped.IsOk);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_OnOk_ExecutesOkFunc()
    {
        var result = Result<int, string>.Ok(42);
        var matched = result.Match(x => $"Value: {x}", e => $"Error: {e}");

        Assert.Equal("Value: 42", matched);
    }

    [Fact]
    public void Match_OnErr_ExecutesErrFunc()
    {
        var result = Result<int, string>.Error("error");
        var matched = result.Match(x => $"Value: {x}", e => $"Error: {e}");

        Assert.Equal("Error: error", matched);
    }

    #endregion

    #region MapErr Tests

    [Fact]
    public void MapErr_OnErr_TransformsError()
    {
        var result = Result<int, string>.Error("error");
        var mapped = result.MapError(e => e.ToUpper());

        Assert.True(mapped.IsError);
        Assert.Equal("ERROR", mapped.GetError());
    }

    [Fact]
    public void MapErr_OnOk_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var mapped = result.MapError(e => e.ToUpper());

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    #endregion

    #region Expect Tests

    [Fact]
    public void Expect_OnOk_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);
        var value = result.GetOrThrow();

        Assert.Equal(42, value);
    }

    [Fact]
    public void Expect_OnErr_Throws()
    {
        var result = Result<int, string>.Error("error");

        Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
    }

    [Fact]
    public void ExpectErr_OnErr_ReturnsError()
    {
        var result = Result<int, string>.Error("error");
        var error = result.GetErrorOrThrow();

        Assert.Equal("error", error);
    }

    [Fact]
    public void ExpectErr_OnOk_Throws()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => result.GetErrorOrThrow());
    }

    #endregion
}
