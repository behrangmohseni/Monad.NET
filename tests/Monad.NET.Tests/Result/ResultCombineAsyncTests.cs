using System.Diagnostics;
using Xunit;

namespace Monad.NET.Tests;

public class ResultCombineAsyncTests
{
    private static async Task<Result<T, string>> DelayedOk<T>(T value, int delayMs)
    {
        await Task.Delay(delayMs);
        return Result<T, string>.Ok(value);
    }

    private static async Task<Result<T, string>> DelayedError<T>(string error, int delayMs)
    {
        await Task.Delay(delayMs);
        return Result<T, string>.Error(error);
    }

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

    #region Parallelization verification

    [Fact]
    public async Task CombineAsync_Two_RunsInParallel()
    {
        const int delayMs = 1000;
        const int sequentialMs = delayMs * 2;

        var sw = Stopwatch.StartNew();
        var combined = await ResultExtensions.CombineAsync(
            DelayedOk(1, delayMs),
            DelayedOk(2, delayMs));
        sw.Stop();

        Assert.True(combined.IsOk);
        Assert.True(sw.ElapsedMilliseconds < sequentialMs,
            $"Expected parallel execution under {sequentialMs}ms, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CombineAsync_Three_RunsInParallel()
    {
        const int delayMs = 1000;
        const int sequentialMs = delayMs * 3;

        var sw = Stopwatch.StartNew();
        var combined = await ResultExtensions.CombineAsync(
            DelayedOk(1, delayMs),
            DelayedOk("two", delayMs),
            DelayedOk(3.0, delayMs));
        sw.Stop();

        Assert.True(combined.IsOk);
        Assert.True(sw.ElapsedMilliseconds < sequentialMs,
            $"Expected parallel execution under {sequentialMs}ms, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CombineAsync_Two_WithCombiner_RunsInParallel()
    {
        const int delayMs = 1000;
        const int sequentialMs = delayMs * 2;

        var sw = Stopwatch.StartNew();
        var combined = await ResultExtensions.CombineAsync(
            DelayedOk(10, delayMs),
            DelayedOk(20, delayMs),
            (a, b) => a + b);
        sw.Stop();

        Assert.True(combined.IsOk);
        Assert.Equal(30, combined.GetValue());
        Assert.True(sw.ElapsedMilliseconds < sequentialMs,
            $"Expected parallel execution under {sequentialMs}ms, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CombineAsync_Three_WithCombiner_RunsInParallel()
    {
        const int delayMs = 1000;
        const int sequentialMs = delayMs * 3;

        var sw = Stopwatch.StartNew();
        var combined = await ResultExtensions.CombineAsync(
            DelayedOk(1, delayMs),
            DelayedOk(2, delayMs),
            DelayedOk(3, delayMs),
            (a, b, c) => a + b + c);
        sw.Stop();

        Assert.True(combined.IsOk);
        Assert.Equal(6, combined.GetValue());
        Assert.True(sw.ElapsedMilliseconds < sequentialMs,
            $"Expected parallel execution under {sequentialMs}ms, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CombineErrorsAsync_Two_RunsInParallel()
    {
        const int delayMs = 1000;
        const int sequentialMs = delayMs * 2;

        var sw = Stopwatch.StartNew();
        var combined = await ResultExtensions.CombineErrorsAsync(
            DelayedOk(1, delayMs),
            DelayedOk(2, delayMs));
        sw.Stop();

        Assert.True(combined.IsOk);
        Assert.True(sw.ElapsedMilliseconds < sequentialMs,
            $"Expected parallel execution under {sequentialMs}ms, but took {sw.ElapsedMilliseconds}ms");
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
