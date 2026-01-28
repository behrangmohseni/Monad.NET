using Monad.NET;

namespace Monad.NET.Tests;

public class ResultAsyncTests
{
    [Fact]
    public async Task MapAsync_OnOkWithAsyncFunc_TransformsValue()
    {
        var result = Result<int, string>.Ok(42);
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_OnErrWithAsyncFunc_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsError);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public async Task MapAsync_TaskResultWithAsyncFunc_TransformsValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        Assert.True(mapped.IsOk);
        Assert.Equal("42", mapped.GetValue());
    }

    [Fact]
    public async Task MapErrAsync_OnErrWithAsyncFunc_TransformsError()
    {
        var result = Result<int, string>.Err("error");
        var resultTask = Task.FromResult(result);
        var mapped = await resultTask.MapErrorAsync(async err =>
        {
            await Task.Delay(1);
            return err.Length;
        });

        Assert.True(mapped.IsError);
        Assert.Equal(5, mapped.GetError());
    }

    [Fact]
    public async Task BindAsync_WithAsyncFunc_ChainsCorrectly()
    {
        var result = Result<int, string>.Ok(42);
        var chained = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string, string>.Ok(x.ToString());
        });

        Assert.True(chained.IsOk);
        Assert.Equal("42", chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var chained = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string, string>.Ok(x.ToString());
        });

        Assert.True(chained.IsError);
        Assert.Equal("error", chained.GetError());
    }

    [Fact]
    public async Task OrElseAsync_OnErr_Recovers()
    {
        var result = Result<int, string>.Err("error");
        var recovered = await result.OrElseAsync(async err =>
        {
            await Task.Delay(1);
            return Result<int, int>.Ok(100);
        });

        Assert.True(recovered.IsOk);
        Assert.Equal(100, recovered.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_OnOk_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var recovered = await result.OrElseAsync(async err =>
        {
            await Task.Delay(1);
            return Result<int, int>.Ok(100);
        });

        Assert.True(recovered.IsOk);
        Assert.Equal(42, recovered.GetValue());
    }

    [Fact]
    public async Task MatchAsync_OnOkWithAsyncHandlers_ExecutesOkFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var output = await resultTask.MatchAsync(
            okFunc: async x =>
            {
                await Task.Delay(1);
                return x.ToString();
            },
            errFunc: async err =>
            {
                await Task.Delay(1);
                return err;
            }
        );

        Assert.Equal("42", output);
    }

    [Fact]
    public async Task MatchAsync_OnErrWithAsyncHandlers_ExecutesErrFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var output = await resultTask.MatchAsync(
            okFunc: async x =>
            {
                await Task.Delay(1);
                return x.ToString();
            },
            errFunc: async err =>
            {
                await Task.Delay(1);
                return err.ToUpper();
            }
        );

        Assert.Equal("ERROR", output);
    }

    [Fact]
    public async Task TapAsync_OnOk_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var executed = false;

        var result = await resultTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task TapAsync_OnErr_DoesNotExecuteAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var executed = false;

        var result = await resultTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task TapErrAsync_OnErr_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var executed = false;

        var result = await resultTask.TapErrAsync(async err =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task UnwrapOrElseAsync_OnOk_ReturnsValue()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var value = await resultTask.GetValueOrElseAsync(async err =>
        {
            await Task.Delay(1);
            return 0;
        });

        Assert.Equal(42, value);
    }

    [Fact]
    public async Task UnwrapOrElseAsync_OnErr_ExecutesFunc()
    {
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));
        var value = await resultTask.GetValueOrElseAsync(async err =>
        {
            await Task.Delay(1);
            return 100;
        });

        Assert.Equal(100, value);
    }

    [Fact]
    public async Task AsTask_WrapsResultInTask()
    {
        var result = Result<int, string>.Ok(42);
        var task = result.AsTask();

        var unwrapped = await task;
        Assert.True(unwrapped.IsOk);
        Assert.Equal(42, unwrapped.GetValue());
    }

    [Fact]
    public async Task ComplexAsyncChain_WorksCorrectly()
    {
        var result = await Result<int, string>.Ok(10)
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            })
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return x > 15
                    ? Result<int, string>.Ok(x)
                    : Result<int, string>.Err("too small");
            })
            .TapAsync(async x =>
            {
                await Task.Delay(1);
                // Side effect
            });

        Assert.True(result.IsOk);
        Assert.Equal(20, result.GetValue());
    }

    [Fact]
    public async Task ComplexAsyncChainWithRecovery_WorksCorrectly()
    {
        var result = await Result<int, string>.Err("initial error")
            .OrElseAsync(async err =>
            {
                await Task.Delay(1);
                return Result<int, string>.Ok(42);
            })
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            });

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }
}

