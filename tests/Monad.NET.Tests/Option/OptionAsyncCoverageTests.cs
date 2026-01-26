using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for OptionAsync to improve code coverage.
/// </summary>
public class OptionAsyncCoverageTests
{
    #region GetValueOrElseAsync Tests

    [Fact]
    public async Task GetValueOrElseAsync_Some_ReturnsValue()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var value = await optionTask.GetValueOrElseAsync(async () =>
        {
            await Task.Delay(1);
            return 0;
        });

        Assert.Equal(42, value);
    }

    [Fact]
    public async Task GetValueOrElseAsync_None_ExecutesFunc()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var value = await optionTask.GetValueOrElseAsync(async () =>
        {
            await Task.Delay(1);
            return 99;
        });

        Assert.Equal(99, value);
    }

    #endregion

    #region TapAsync Tests

    [Fact]
    public async Task TapAsync_Some_ExecutesAction()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var capturedValue = 0;

        var result = await optionTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });

        Assert.Equal(42, capturedValue);
        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task TapAsync_None_DoesNotExecuteAction()
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

    #endregion

    #region MapAsync Tests

    [Fact]
    public async Task MapAsync_Option_Some_TransformsValue()
    {
        var option = Option<int>.Some(21);
        var mapped = await option.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsSome);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_Option_None_ReturnsNone()
    {
        var option = Option<int>.None();
        var mapped = await option.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsNone);
    }

    [Fact]
    public async Task MapAsync_OptionTask_WithCancellationToken_Some_TransformsValue()
    {
        var optionTask = Task.FromResult(Option<int>.Some(21));
        var mapped = await optionTask.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsSome);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_OptionTask_WithCancellationToken_None_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var mapped = await optionTask.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsNone);
    }

    #endregion

    #region BindAsync Tests

    [Fact]
    public async Task BindAsync_Option_Some_Chains()
    {
        var option = Option<int>.Some(21);
        var chained = await option.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Option<int>.Some(x * 2);
        });

        Assert.True(chained.IsSome);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_Option_None_ReturnsNone()
    {
        var option = Option<int>.None();
        var chained = await option.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Option<int>.Some(x * 2);
        });

        Assert.True(chained.IsNone);
    }

    [Fact]
    public async Task BindAsync_OptionTask_WithCancellationToken_Some_Chains()
    {
        var optionTask = Task.FromResult(Option<int>.Some(21));
        var chained = await optionTask.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Option<int>.Some(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsSome);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_OptionTask_WithCancellationToken_None_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var chained = await optionTask.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Option<int>.Some(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsNone);
    }

    #endregion

    #region MatchAsync with CancellationToken Tests

    [Fact]
    public async Task MatchAsync_WithCancellationToken_Some_ExecutesSomeFunc()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var matched = await optionTask.MatchAsync(
            async (x, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Value: {x}";
            },
            async ct =>
            {
                await Task.Delay(1, ct);
                return "None";
            },
            CancellationToken.None);

        Assert.Equal("Value: 42", matched);
    }

    [Fact]
    public async Task MatchAsync_WithCancellationToken_None_ExecutesNoneFunc()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var matched = await optionTask.MatchAsync(
            async (x, ct) =>
            {
                await Task.Delay(1, ct);
                return $"Value: {x}";
            },
            async ct =>
            {
                await Task.Delay(1, ct);
                return "None";
            },
            CancellationToken.None);

        Assert.Equal("None", matched);
    }

    #endregion

    #region TapNoneAsync Tests

    [Fact]
    public async Task TapNoneAsync_Some_DoesNotExecuteAction()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var executed = false;

        var result = await optionTask.TapNoneAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task TapNoneAsync_None_ExecutesAction()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var executed = false;

        var result = await optionTask.TapNoneAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsNone);
    }

    #endregion

    #region OrElseAsync Tests

    [Fact]
    public async Task OrElseAsync_Option_Some_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);
        var result = await option.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.Some(99);
        });

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_Option_None_ReturnsAlternative()
    {
        var option = Option<int>.None();
        var result = await option.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.Some(99);
        });

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_OptionTask_Some_ReturnsOriginal()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.Some(99);
        });

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OrElseAsync_OptionTask_None_ReturnsAlternative()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.OrElseAsync(async () =>
        {
            await Task.Delay(1);
            return Option<int>.Some(99);
        });

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    #endregion

    #region OrAsync Tests

    [Fact]
    public async Task OrAsync_Some_ReturnsOriginal()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.OrAsync(Option<int>.Some(99));

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OrAsync_None_ReturnsAlternative()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.OrAsync(Option<int>.Some(99));

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    #endregion
}
