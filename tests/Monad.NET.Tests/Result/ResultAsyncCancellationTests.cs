using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for ResultAsync CancellationToken overloads to improve code coverage.
/// </summary>
public class ResultAsyncCancellationTests
{
    #region MapAsync with CancellationToken Tests

    [Fact]
    public async Task MapAsync_TaskResult_WithCancellationToken_Ok_TransformsValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(21));
        var mapped = await resultTask.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_TaskResult_WithCancellationToken_Err_ReturnsErr()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var mapped = await resultTask.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsErr);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public async Task MapAsync_TaskResult_WithCancellation_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await resultTask.MapAsync(async (x, ct) =>
            {
                await Task.Delay(100, ct);
                return x * 2;
            }, cts.Token));
    }

    [Fact]
    public async Task MapAsync_Result_WithCancellationToken_Ok_TransformsValue()
    {
        var result = Result<int, string>.Ok(21);
        var mapped = await result.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_Result_WithCancellationToken_Err_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var mapped = await result.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsErr);
        Assert.Equal("error", mapped.GetError());
    }

    #endregion

    #region MapErrorAsync with CancellationToken Tests

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithCancellationToken_Ok_PreservesValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var mapped = await resultTask.MapErrorAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return e.Length;
        }, CancellationToken.None);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithCancellationToken_Err_TransformsError()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var mapped = await resultTask.MapErrorAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return e.Length;
        }, CancellationToken.None);

        Assert.True(mapped.IsErr);
        Assert.Equal(5, mapped.GetError());
    }

    #endregion

    #region BindAsync with CancellationToken Tests

    [Fact]
    public async Task BindAsync_TaskResult_WithCancellationToken_Ok_Chains()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(21));
        var chained = await resultTask.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsOk);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_TaskResult_WithCancellationToken_Err_ReturnsErr()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var chained = await resultTask.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsErr);
    }

    [Fact]
    public async Task BindAsync_Result_WithCancellationToken_Ok_Chains()
    {
        var result = Result<int, string>.Ok(21);
        var chained = await result.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsOk);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_Result_WithCancellationToken_Err_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var chained = await result.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsErr);
    }

    #endregion

    #region OrElseAsync with CancellationToken Tests

    [Fact]
    public async Task OrElseAsync_TaskResult_WithCancellationToken_Ok_ReturnsOriginal()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var fallback = await resultTask.OrElseAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(99);
        }, CancellationToken.None);

        Assert.True(fallback.IsOk);
        Assert.Equal(42, fallback.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_TaskResult_WithCancellationToken_Err_ExecutesFallback()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var fallback = await resultTask.OrElseAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(99);
        }, CancellationToken.None);

        Assert.True(fallback.IsOk);
        Assert.Equal(99, fallback.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_Result_WithCancellationToken_Ok_ReturnsOriginal()
    {
        var result = Result<int, string>.Ok(42);
        var fallback = await result.OrElseAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(99);
        }, CancellationToken.None);

        Assert.True(fallback.IsOk);
        Assert.Equal(42, fallback.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_Result_WithCancellationToken_Err_ExecutesFallback()
    {
        var result = Result<int, string>.Err("error");
        var fallback = await result.OrElseAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return Result<int, string>.Ok(99);
        }, CancellationToken.None);

        Assert.True(fallback.IsOk);
        Assert.Equal(99, fallback.GetValue());
    }

    #endregion

    #region GetValueOrElseAsync with CancellationToken Tests

    [Fact]
    public async Task GetValueOrElseAsync_TaskResult_WithCancellationToken_Ok_ReturnsValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var value = await resultTask.GetValueOrElseAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return 0;
        }, CancellationToken.None);

        Assert.Equal(42, value);
    }

    [Fact]
    public async Task GetValueOrElseAsync_TaskResult_WithCancellationToken_Err_ExecutesFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var value = await resultTask.GetValueOrElseAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            return 99;
        }, CancellationToken.None);

        Assert.Equal(99, value);
    }

    #endregion

    #region MatchAsync with CancellationToken Tests

    [Fact]
    public async Task MatchAsync_TaskResult_WithCancellationToken_Ok_ExecutesOkFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var matched = await resultTask.MatchAsync(
            async (x, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Value: {x}";
            },
            async (e, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Error: {e}";
            },
            CancellationToken.None);

        Assert.Equal("Value: 42", matched);
    }

    [Fact]
    public async Task MatchAsync_TaskResult_WithCancellationToken_Err_ExecutesErrFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var matched = await resultTask.MatchAsync(
            async (x, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Value: {x}";
            },
            async (e, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Error: {e}";
            },
            CancellationToken.None);

        Assert.Equal("Error: error", matched);
    }

    #endregion

    #region TapAsync with CancellationToken Tests

    [Fact]
    public async Task TapAsync_TaskResult_WithCancellationToken_Ok_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var capturedValue = 0;

        var tapped = await resultTask.TapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            capturedValue = x;
        }, CancellationToken.None);

        Assert.Equal(42, capturedValue);
        Assert.True(tapped.IsOk);
    }

    [Fact]
    public async Task TapAsync_TaskResult_WithCancellationToken_Err_DoesNotExecuteAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var executed = false;

        var tapped = await resultTask.TapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            executed = true;
        }, CancellationToken.None);

        Assert.False(executed);
        Assert.True(tapped.IsErr);
    }

    [Fact]
    public async Task TapAsync_Result_WithCancellationToken_Ok_ExecutesAction()
    {
        var result = Result<int, string>.Ok(42);
        var capturedValue = 0;

        var tapped = await result.TapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            capturedValue = x;
        }, CancellationToken.None);

        Assert.Equal(42, capturedValue);
        Assert.True(tapped.IsOk);
    }

    [Fact]
    public async Task TapAsync_Result_WithCancellationToken_Err_DoesNotExecuteAction()
    {
        var result = Result<int, string>.Err("error");
        var executed = false;

        var tapped = await result.TapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            executed = true;
        }, CancellationToken.None);

        Assert.False(executed);
        Assert.True(tapped.IsErr);
    }

    #endregion

    #region TapErrAsync with CancellationToken Tests

    [Fact]
    public async Task TapErrAsync_TaskResult_WithCancellationToken_Err_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var capturedError = "";

        var tapped = await resultTask.TapErrAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            capturedError = e;
        }, CancellationToken.None);

        Assert.Equal("error", capturedError);
        Assert.True(tapped.IsErr);
    }

    [Fact]
    public async Task TapErrAsync_TaskResult_WithCancellationToken_Ok_DoesNotExecuteAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var executed = false;

        var tapped = await resultTask.TapErrAsync(async (e, ct) =>
        {
            await Task.Delay(1, ct);
            executed = true;
        }, CancellationToken.None);

        Assert.False(executed);
        Assert.True(tapped.IsOk);
    }

    #endregion

    #region Synchronous Mapper Overloads

    [Fact]
    public async Task MapAsync_TaskResult_WithSyncMapper_Ok_TransformsValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(21));
        var mapped = await resultTask.MapAsync(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_TaskResult_WithSyncMapper_Err_ReturnsErr()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var mapped = await resultTask.MapAsync(x => x * 2);

        Assert.True(mapped.IsErr);
    }

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithSyncMapper_Ok_PreservesValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var mapped = await resultTask.MapErrorAsync(e => e.Length);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapErrorAsync_TaskResult_WithSyncMapper_Err_TransformsError()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var mapped = await resultTask.MapErrorAsync(e => e.Length);

        Assert.True(mapped.IsErr);
        Assert.Equal(5, mapped.GetError());
    }

    [Fact]
    public async Task BindAsync_TaskResult_WithSyncBinder_Ok_Chains()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(21));
        var chained = await resultTask.BindAsync(x => Result<int, string>.Ok(x * 2));

        Assert.True(chained.IsOk);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_TaskResult_WithSyncBinder_Err_ReturnsErr()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var chained = await resultTask.BindAsync(x => Result<int, string>.Ok(x * 2));

        Assert.True(chained.IsErr);
    }

    [Fact]
    public async Task MatchAsync_TaskResult_WithSyncHandlers_Ok_ExecutesOkFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var matched = await resultTask.MatchAsync(
            x => $"Value: {x}",
            e => $"Error: {e}");

        Assert.Equal("Value: 42", matched);
    }

    [Fact]
    public async Task MatchAsync_TaskResult_WithSyncHandlers_Err_ExecutesErrFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var matched = await resultTask.MatchAsync(
            x => $"Value: {x}",
            e => $"Error: {e}");

        Assert.Equal("Error: error", matched);
    }

    #endregion
}
