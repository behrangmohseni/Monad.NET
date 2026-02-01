using Monad.NET;

namespace Monad.NET.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_CreatesResultWithValue()
    {
        var result = Result<int, string>.Ok(42);

        Assert.True(result.IsOk);
        Assert.False(result.IsError);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Err_CreatesResultWithError()
    {
        var result = Result<int, string>.Err("error");

        Assert.False(result.IsOk);
        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void Ok_WithNullValue_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string, int>.Ok(null!));
    }

    [Fact]
    public void Err_WithNullValue_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<int, string>.Err(null!));
    }

    [Fact]
    public void Unwrap_OnErr_ThrowsException()
    {
        var result = Result<int, string>.Err("error");

        Assert.Throws<InvalidOperationException>(() => result.GetValue());
    }

    [Fact]
    public void UnwrapErr_OnOk_ThrowsException()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => result.GetError());
    }

    [Fact]
    public void Expect_OnErr_Throws()
    {
        var result = Result<int, string>.Err("error");

        Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
    }

    [Fact]
    public void ExpectErr_OnOk_Throws()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => result.GetErrorOrThrow());
    }

    [Fact]
    public void UnwrapOr_OnOk_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Equal(42, result.GetValueOr(0));
    }

    [Fact]
    public void UnwrapOr_OnErr_ReturnsDefault()
    {
        var result = Result<int, string>.Err("error");

        Assert.Equal(0, result.GetValueOr(0));
    }

    [Fact]
    public void UnwrapOrElse_OnOk_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Equal(42, result.Match(ok => ok, _ => 0));
    }

    [Fact]
    public void UnwrapOrElse_OnErr_ExecutesFunction()
    {
        var result = Result<int, string>.Err("error");

        Assert.Equal(100, result.Match(ok => ok, _ => 100));
    }

    [Fact]
    public void Map_OnOk_TransformsValue()
    {
        var result = Result<int, string>.Ok(42);
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void Map_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsError);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public void MapErr_OnOk_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var mapped = result.MapError(err => err.Length);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public void MapErr_OnErr_TransformsError()
    {
        var result = Result<int, string>.Err("error");
        var mapped = result.MapError(err => err.Length);

        Assert.True(mapped.IsError);
        Assert.Equal(5, mapped.GetError());
    }

    [Fact]
    public void AndThen_OnOk_ExecutesFunction()
    {
        var result = Result<int, string>.Ok(42);
        var chained = result.Bind(x => Result<string, string>.Ok(x.ToString()));

        Assert.True(chained.IsOk);
        Assert.Equal("42", chained.GetValue());
    }

    [Fact]
    public void AndThen_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var chained = result.Bind(x => Result<string, string>.Ok(x.ToString()));

        Assert.True(chained.IsError);
        Assert.Equal("error", chained.GetError());
    }

    [Fact]
    public void Zip_BothOk_ReturnsTuple()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<string, string>.Ok("hello");

        var combined = result1.Zip(result2);

        Assert.True(combined.IsOk);
        Assert.Equal((42, "hello"), combined.GetValue());
    }

    [Fact]
    public void Zip_FirstErr_ReturnsFirstError()
    {
        var result1 = Result<int, string>.Err("first error");
        var result2 = Result<string, string>.Ok("hello");

        var combined = result1.Zip(result2);

        Assert.True(combined.IsError);
        Assert.Equal("first error", combined.GetError());
    }

    [Fact]
    public void Zip_SecondErr_ReturnsSecondError()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<string, string>.Err("second error");

        var combined = result1.Zip(result2);

        Assert.True(combined.IsError);
        Assert.Equal("second error", combined.GetError());
    }

    [Fact]
    public void ZipWith_BothOk_ReturnsCombinedValue()
    {
        var result1 = Result<int, string>.Ok(10);
        var result2 = Result<int, string>.Ok(20);

        var combined = result1.ZipWith(result2, (a, b) => a + b);

        Assert.True(combined.IsOk);
        Assert.Equal(30, combined.GetValue());
    }

    [Fact]
    public void ZipWith_FirstErr_ReturnsFirstError()
    {
        var result1 = Result<int, string>.Err("first error");
        var result2 = Result<int, string>.Ok(20);

        var combined = result1.ZipWith(result2, (a, b) => a + b);

        Assert.True(combined.IsError);
        Assert.Equal("first error", combined.GetError());
    }

    [Fact]
    public void ZipWith_SecondErr_ReturnsSecondError()
    {
        var result1 = Result<int, string>.Ok(10);
        var result2 = Result<int, string>.Err("second error");

        var combined = result1.ZipWith(result2, (a, b) => a + b);

        Assert.True(combined.IsError);
        Assert.Equal("second error", combined.GetError());
    }

    [Fact]
    public void Or_BothOk_ReturnsFirst()
    {
        var result1 = Result<int, string>.Ok(1);
        var result2 = Result<int, string>.Ok(2);
        var combined = result1.Or(result2);

        Assert.Equal(1, combined.GetValue());
    }

    [Fact]
    public void Or_FirstErr_ReturnsSecond()
    {
        var result1 = Result<int, string>.Err("error1");
        var result2 = Result<int, string>.Ok(2);
        var combined = result1.Or(result2);

        Assert.Equal(2, combined.GetValue());
    }

    [Fact]
    public void OrElse_OnOk_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var recovered = result.OrElse(err => Result<int, int>.Ok(0));

        Assert.True(recovered.IsOk);
        Assert.Equal(42, recovered.GetValue());
    }

    [Fact]
    public void OrElse_OnErr_ExecutesFunction()
    {
        var result = Result<int, string>.Err("error");
        var recovered = result.OrElse(err => Result<int, int>.Ok(100));

        Assert.True(recovered.IsOk);
        Assert.Equal(100, recovered.GetValue());
    }

    [Fact]
    public void Ok_Method_OnOk_ReturnsSome()
    {
        var result = Result<int, string>.Ok(42);
        var option = result.Ok();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void Ok_Method_OnErr_ReturnsNone()
    {
        var result = Result<int, string>.Err("error");
        var option = result.Ok();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void Err_Method_OnErr_ReturnsSome()
    {
        var result = Result<int, string>.Err("error");
        var option = result.Err();

        Assert.True(option.IsSome);
        Assert.Equal("error", option.GetValue());
    }

    [Fact]
    public void Err_Method_OnOk_ReturnsNone()
    {
        var result = Result<int, string>.Ok(42);
        var option = result.Err();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void Match_OnOk_ExecutesOkAction()
    {
        var result = Result<int, string>.Ok(42);
        var value = 0;

        result.Match(
            okAction: x => value = x,
            errAction: err => value = -1
        );

        Assert.Equal(42, value);
    }

    [Fact]
    public void Match_OnErr_ExecutesErrAction()
    {
        var result = Result<int, string>.Err("error");
        var errorMessage = string.Empty;

        result.Match(
            okAction: x => errorMessage = "ok",
            errAction: err => errorMessage = err
        );

        Assert.Equal("error", errorMessage);
    }

    [Fact]
    public void Match_WithReturn_OnOk_ReturnsOkValue()
    {
        var result = Result<int, string>.Ok(42);
        var output = result.Match(
            okFunc: x => x.ToString(),
            errFunc: err => err
        );

        Assert.Equal("42", output);
    }

    [Fact]
    public void Match_WithReturn_OnErr_ReturnsErrValue()
    {
        var result = Result<int, string>.Err("error");
        var output = result.Match(
            okFunc: x => x.ToString(),
            errFunc: err => err
        );

        Assert.Equal("error", output);
    }

    [Fact]
    public void Flatten_NestedOk_ReturnsInnerOk()
    {
        var nested = Result<Result<int, string>, string>.Ok(Result<int, string>.Ok(42));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsOk);
        Assert.Equal(42, flattened.GetValue());
    }

    [Fact]
    public void Flatten_NestedErr_ReturnsErr()
    {
        var nested = Result<Result<int, string>, string>.Ok(Result<int, string>.Err("inner error"));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsError);
        Assert.Equal("inner error", flattened.GetError());
    }

    [Fact]
    public void Tap_OnOk_ExecutesAction()
    {
        var result = Result<int, string>.Ok(42);
        var executed = false;

        var returned = result.Tap(x => executed = true);

        Assert.True(executed);
        Assert.Equal(result, returned);
    }

    [Fact]
    public void Tap_OnErr_DoesNotExecuteAction()
    {
        var result = Result<int, string>.Err("error");
        var executed = false;

        var returned = result.Tap(x => executed = true);

        Assert.False(executed);
        Assert.Equal(result, returned);
    }

    [Fact]
    public void TapErr_OnErr_ExecutesAction()
    {
        var result = Result<int, string>.Err("error");
        var executed = false;

        var returned = result.TapErr(err => executed = true);

        Assert.True(executed);
        Assert.Equal(result, returned);
    }

    [Fact]
    public void TapErr_OnOk_DoesNotExecuteAction()
    {
        var result = Result<int, string>.Ok(42);
        var executed = false;

        var returned = result.TapErr(err => executed = true);

        Assert.False(executed);
        Assert.Equal(result, returned);
    }

    [Fact]
    public void Try_WithSuccessfulFunc_ReturnsOk()
    {
        var result = ResultExtensions.Try(() => 42);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Try_WithFailingFunc_ReturnsErr()
    {
        var result = ResultExtensions.Try<int>(() => throw new InvalidOperationException("test error"));

        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetError());
        Assert.Contains("test error", result.GetError().Message);
    }

    [Fact]
    public async Task TryAsync_WithSuccessfulFunc_ReturnsOk()
    {
        var result = await ResultExtensions.TryAsync(() => Task.FromResult(42));

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task TryAsync_WithFailingFunc_ReturnsErr()
    {
        var result = await ResultExtensions.TryAsync<int>(() =>
            Task.FromException<int>(new InvalidOperationException("test error")));

        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetError());
    }

    [Fact]
    public void Equality_TwoOksWithSameValue_AreEqual()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(42);

        Assert.Equal(result1, result2);
        Assert.True(result1 == result2);
    }

    [Fact]
    public void Equality_TwoErrsWithSameError_AreEqual()
    {
        var result1 = Result<int, string>.Err("error");
        var result2 = Result<int, string>.Err("error");

        Assert.Equal(result1, result2);
        Assert.True(result1 == result2);
    }

    [Fact]
    public void Equality_OkAndErr_AreNotEqual()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Err("error");

        Assert.NotEqual(result1, result2);
        Assert.True(result1 != result2);
    }

    [Fact]
    public void ToString_OnOk_ReturnsFormattedString()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Equal("Ok(42)", result.ToString());
    }

    [Fact]
    public void ToString_OnErr_ReturnsFormattedString()
    {
        var result = Result<int, string>.Err("error");

        Assert.Equal("Err(error)", result.ToString());
    }
}

