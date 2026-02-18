using Xunit;

namespace Monad.NET.Tests;

public class ResultCombineAsyncTests
{
    #region CombineAsync 2-arity

    [Fact]
    public async Task CombineAsync_TwoOk_ReturnsTuple()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)));

        Assert.True(combined.IsOk);
        Assert.Equal((1, 2), combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_TwoOk_WithCombiner_ReturnsResult()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Ok(10)),
            Task.FromResult(Result<int, string>.Ok(20)),
            (a, b) => a + b);

        Assert.True(combined.IsOk);
        Assert.Equal(30, combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_FirstErr_ReturnsError()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Error("e1")),
            Task.FromResult(Result<int, string>.Ok(2)));

        Assert.True(combined.IsError);
        Assert.Equal("e1", combined.GetError());
    }

    [Fact]
    public async Task CombineAsync_SecondErr_ReturnsError()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Error("e2")));

        Assert.True(combined.IsError);
        Assert.Equal("e2", combined.GetError());
    }

    [Fact]
    public async Task CombineAsync_BothErr_ReturnsFirstError()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Error("e1")),
            Task.FromResult(Result<int, string>.Error("e2")));

        Assert.True(combined.IsError);
        Assert.Equal("e1", combined.GetError());
    }

    #endregion

    #region CombineAsync 3-arity

    [Fact]
    public async Task CombineAsync_ThreeOk_ReturnsTuple()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<string, string>.Ok("two")),
            Task.FromResult(Result<double, string>.Ok(3.0)));

        Assert.True(combined.IsOk);
        Assert.Equal((1, "two", 3.0), combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_ThreeOk_WithCombiner_ReturnsResult()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3)),
            (a, b, c) => a + b + c);

        Assert.True(combined.IsOk);
        Assert.Equal(6, combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_ThirdErr_ReturnsError()
    {
        var combined = await ResultExtensions.CombineAsync(
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Error("e3")));

        Assert.True(combined.IsError);
        Assert.Equal("e3", combined.GetError());
    }

    #endregion

    #region CombineErrorsAsync 2-arity

    [Fact]
    public async Task CombineErrorsAsync_TwoOk_ReturnsTuple()
    {
        var combined = await ResultExtensions.CombineErrorsAsync(
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)));

        Assert.True(combined.IsOk);
        Assert.Equal((1, 2), combined.GetValue());
    }

    [Fact]
    public async Task CombineErrorsAsync_BothErr_AccumulatesErrors()
    {
        var combined = await ResultExtensions.CombineErrorsAsync(
            Task.FromResult(Result<int, string>.Error("e1")),
            Task.FromResult(Result<int, string>.Error("e2")));

        Assert.True(combined.IsError);
        Assert.Equal(new[] { "e1", "e2" }, combined.GetError());
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task CombineAsync_Two_RespectsCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            ResultExtensions.CombineAsync(
                Task.FromResult(Result<int, string>.Ok(1)),
                Task.FromResult(Result<int, string>.Ok(2)),
                cts.Token));
    }

    [Fact]
    public async Task CombineAsync_Three_RespectsCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            ResultExtensions.CombineAsync(
                Task.FromResult(Result<int, string>.Ok(1)),
                Task.FromResult(Result<int, string>.Ok(2)),
                Task.FromResult(Result<int, string>.Ok(3)),
                cts.Token));
    }

    #endregion
}
