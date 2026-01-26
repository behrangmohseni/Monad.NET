using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for ResultAsyncExtensions to improve code coverage.
/// </summary>
public class ResultAsyncExtendedTests
{
    #region MapAsync Tests

    [Fact]
    public async Task MapAsync_Task_Ok_TransformsValue()
    {
        var result = Task.FromResult(Result<int, string>.Ok(21));
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_Task_Err_ReturnsErr()
    {
        var result = Task.FromResult(Result<int, string>.Err("error"));
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsErr);
        Assert.Equal("error", mapped.GetError());
    }

    #endregion

    #region MapErrAsync Tests

    [Fact]
    public async Task MapErrAsync_Task_Ok_PreservesValue()
    {
        var result = Task.FromResult(Result<int, string>.Ok(42));
        var mapped = await result.MapErrorAsync(async e =>
        {
            await Task.Delay(1);
            return e.ToUpper();
        });

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapErrAsync_Task_Err_TransformsError()
    {
        var result = Task.FromResult(Result<int, string>.Err("error"));
        var mapped = await result.MapErrorAsync(async e =>
        {
            await Task.Delay(1);
            return e.ToUpper();
        });

        Assert.True(mapped.IsErr);
        Assert.Equal("ERROR", mapped.GetError());
    }

    #endregion

    #region BindAsync Tests

    [Fact]
    public async Task BindAsync_Task_Ok_Chains()
    {
        var result = Task.FromResult(Result<int, string>.Ok(21));
        var chained = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(x * 2);
        });

        Assert.True(chained.IsOk);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_Task_Err_ReturnsErr()
    {
        var result = Task.FromResult(Result<int, string>.Err("error"));
        var chained = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(x * 2);
        });

        Assert.True(chained.IsErr);
    }

    [Fact]
    public async Task BindAsync_OkToErr_ReturnsErr()
    {
        var result = Task.FromResult(Result<int, string>.Ok(42));
        var chained = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int, string>.Err("chained error");
        });

        Assert.True(chained.IsErr);
        Assert.Equal("chained error", chained.GetError());
    }

    #endregion

    #region OrElseAsync Tests

    [Fact]
    public async Task OrElseAsync_Task_Ok_ReturnsOriginal()
    {
        var result = Task.FromResult(Result<int, string>.Ok(42));
        var fallback = await result.OrElseAsync(async e =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(99);
        });

        Assert.True(fallback.IsOk);
        Assert.Equal(42, fallback.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_Task_Err_ExecutesFallback()
    {
        var result = Task.FromResult(Result<int, string>.Err("error"));
        var fallback = await result.OrElseAsync(async e =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(99);
        });

        Assert.True(fallback.IsOk);
        Assert.Equal(99, fallback.GetValue());
    }

    #endregion

    #region MatchAsync Tests

    [Fact]
    public async Task MatchAsync_Task_Ok_ExecutesOkFunc()
    {
        var result = Task.FromResult(Result<int, string>.Ok(42));
        var matched = await result.MatchAsync(
            async x =>
            {
                await Task.Delay(1);
                return $"Value: {x}";
            },
            async e =>
            {
                await Task.Delay(1);
                return $"Error: {e}";
            });

        Assert.Equal("Value: 42", matched);
    }

    [Fact]
    public async Task MatchAsync_Task_Err_ExecutesErrFunc()
    {
        var result = Task.FromResult(Result<int, string>.Err("error"));
        var matched = await result.MatchAsync(
            async x =>
            {
                await Task.Delay(1);
                return $"Value: {x}";
            },
            async e =>
            {
                await Task.Delay(1);
                return $"Error: {e}";
            });

        Assert.Equal("Error: error", matched);
    }

    #endregion

    #region TapAsync Tests

    [Fact]
    public async Task TapAsync_Task_Ok_ExecutesAction()
    {
        var result = Task.FromResult(Result<int, string>.Ok(42));
        var capturedValue = 0;

        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });

        Assert.Equal(42, capturedValue);
        Assert.True(tapped.IsOk);
    }

    [Fact]
    public async Task TapAsync_Task_Err_DoesNotExecuteAction()
    {
        var result = Task.FromResult(Result<int, string>.Err("error"));
        var executed = false;

        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(tapped.IsErr);
    }

    #endregion

    #region CombineAsync Tests

    [Fact]
    public async Task CombineAsync_BothOk_CombinesValues()
    {
        var result1 = Task.FromResult(Result<int, string>.Ok(10));
        var result2 = Task.FromResult(Result<int, string>.Ok(32));

        var combined = await ResultExtensions.CombineAsync(result1, result2, (a, b) => a + b);

        Assert.True(combined.IsOk);
        Assert.Equal(42, combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_FirstErr_ReturnsErr()
    {
        var result1 = Task.FromResult(Result<int, string>.Err("first error"));
        var result2 = Task.FromResult(Result<int, string>.Ok(32));

        var combined = await ResultExtensions.CombineAsync(result1, result2, (a, b) => a + b);

        Assert.True(combined.IsErr);
    }

    [Fact]
    public async Task CombineAsync_SecondErr_ReturnsErr()
    {
        var result1 = Task.FromResult(Result<int, string>.Ok(10));
        var result2 = Task.FromResult(Result<int, string>.Err("second error"));

        var combined = await ResultExtensions.CombineAsync(result1, result2, (a, b) => a + b);

        Assert.True(combined.IsErr);
    }

    #endregion
}

