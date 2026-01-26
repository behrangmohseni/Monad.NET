using Monad.NET;

namespace Monad.NET.Tests;

public class OptionAsyncTests
{
    [Fact]
    public async Task MapAsync_OnSomeWithAsyncFunc_TransformsValue()
    {
        var option = Option<int>.Some(42);
        var result = await option.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(result.IsSome);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public async Task MapAsync_OnNoneWithAsyncFunc_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = await option.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task MapAsync_TaskOptionWithAsyncFunc_TransformsValue()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        Assert.True(result.IsSome);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public async Task FilterAsync_WithAsyncPredicate_FiltersCorrectly()
    {
        var option = Option<int>.Some(42);
        var result = await option.FilterAsync(async x =>
        {
            await Task.Delay(1);
            return x > 40;
        });

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task FilterAsync_WithFailingPredicate_ReturnsNone()
    {
        var option = Option<int>.Some(42);
        var result = await option.FilterAsync(async x =>
        {
            await Task.Delay(1);
            return x < 40;
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task BindAsync_WithAsyncFunc_ChainsCorrectly()
    {
        var option = Option<int>.Some(42);
        var result = await option.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Option<string>.Some(x.ToString());
        });

        Assert.True(result.IsSome);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public async Task BindAsync_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = await option.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Option<string>.Some(x.ToString());
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task MatchAsync_OnSomeWithAsyncHandlers_ExecutesSomeFunc()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.MatchAsync(
            someFunc: async x =>
            {
                await Task.Delay(1);
                return x.ToString();
            },
            noneFunc: async () =>
            {
                await Task.Delay(1);
                return "none";
            }
        );

        Assert.Equal("42", result);
    }

    [Fact]
    public async Task MatchAsync_OnNoneWithAsyncHandlers_ExecutesNoneFunc()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.MatchAsync(
            someFunc: async x =>
            {
                await Task.Delay(1);
                return x.ToString();
            },
            noneFunc: async () =>
            {
                await Task.Delay(1);
                return "none";
            }
        );

        Assert.Equal("none", result);
    }

    [Fact]
    public async Task TapAsync_OnSome_ExecutesAction()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var executed = false;

        var result = await optionTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task TapAsync_OnNone_DoesNotExecuteAction()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var executed = false;

        var result = await optionTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task UnwrapOrElseAsync_OnSome_ReturnsValue()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.GetValueOrElseAsync(async () =>
        {
            await Task.Delay(1);
            return 0;
        });

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task UnwrapOrElseAsync_OnNone_ExecutesDefaultFunc()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.GetValueOrElseAsync(async () =>
        {
            await Task.Delay(1);
            return 100;
        });

        Assert.Equal(100, result);
    }

    [Fact]
    public async Task OkOrElseAsync_OnSome_ReturnsOk()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.OkOrElseAsync(async () =>
        {
            await Task.Delay(1);
            return "error";
        });

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OkOrElseAsync_OnNone_ReturnsErr()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.OkOrElseAsync(async () =>
        {
            await Task.Delay(1);
            return "error";
        });

        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public async Task AsTask_WrapsOptionInTask()
    {
        var option = Option<int>.Some(42);
        var task = option.AsTask();

        var result = await task;
        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task ComplexAsyncChain_WorksCorrectly()
    {
        var result = await Option<int>.Some(10)
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            })
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return x > 15 ? Option<int>.Some(x) : Option<int>.None();
            })
            .FilterAsync(async x =>
            {
                await Task.Delay(1);
                return x < 30;
            });

        Assert.True(result.IsSome);
        Assert.Equal(20, result.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_OnSome_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);
        var alternativeExecuted = false;

        var result = await option.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            alternativeExecuted = true;
            return Option<int>.Some(100);
        });

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
        Assert.False(alternativeExecuted);
    }

    [Fact]
    public async Task OrElseAsync_OnNone_ReturnsAlternative()
    {
        var option = Option<int>.None();
        var alternativeExecuted = false;

        var result = await option.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            alternativeExecuted = true;
            return Option<int>.Some(100);
        });

        Assert.True(result.IsSome);
        Assert.Equal(100, result.GetValue());
        Assert.True(alternativeExecuted);
    }

    [Fact]
    public async Task OrElseAsync_OnNone_ReturnsNoneFromAlternative()
    {
        var option = Option<int>.None();

        var result = await option.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.None();
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task OrElseAsync_OnTaskSome_ReturnsOriginal()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));

        var result = await optionTask.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.Some(100);
        });

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_OnTaskNone_ReturnsAlternative()
    {
        var optionTask = Task.FromResult(Option<int>.None());

        var result = await optionTask.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.Some(100);
        });

        Assert.True(result.IsSome);
        Assert.Equal(100, result.GetValue());
    }

    [Fact]
    public async Task OrAsync_OnTaskSome_ReturnsOriginal()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var alternative = Option<int>.Some(100);

        var result = await optionTask.OrAsync(alternative);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OrAsync_OnTaskNone_ReturnsAlternative()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var alternative = Option<int>.Some(100);

        var result = await optionTask.OrAsync(alternative);

        Assert.True(result.IsSome);
        Assert.Equal(100, result.GetValue());
    }

    [Fact]
    public async Task OrAsync_OnTaskNone_ReturnsNoneAlternative()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var alternative = Option<int>.None();

        var result = await optionTask.OrAsync(alternative);

        Assert.True(result.IsNone);
    }
}

