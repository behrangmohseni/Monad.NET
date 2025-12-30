using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for OptionAsyncExtensions to improve code coverage.
/// </summary>
public class OptionAsyncExtendedTests
{
    #region Task<Option<T>> overloads with sync functions

    [Fact]
    public async Task MapAsync_TaskOption_WithSyncMapper_OnSome_TransformsValue()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.MapAsync(x => x * 2);

        Assert.True(result.IsSome);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public async Task MapAsync_TaskOption_WithSyncMapper_OnNone_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.MapAsync(x => x * 2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FilterAsync_TaskOption_WithSyncPredicate_OnSome_Passes()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(x => x > 40);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public async Task FilterAsync_TaskOption_WithSyncPredicate_OnSome_Fails()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(x => x < 40);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FilterAsync_TaskOption_WithSyncPredicate_OnNone_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.FilterAsync(x => x > 0);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FilterAsync_TaskOption_WithAsyncPredicate_OnNone_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.FilterAsync(async x =>
        {
            await Task.Delay(1);
            return x > 0;
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FilterAsync_TaskOption_WithAsyncPredicate_OnSome_Passes()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(async x =>
        {
            await Task.Delay(1);
            return x > 40;
        });

        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task FilterAsync_TaskOption_WithAsyncPredicate_OnSome_Fails()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(async x =>
        {
            await Task.Delay(1);
            return x < 40;
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task AndThenAsync_TaskOption_WithSyncBinder_OnSome_Chains()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.AndThenAsync(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsSome);
        Assert.Equal("42", result.Unwrap());
    }

    [Fact]
    public async Task AndThenAsync_TaskOption_WithSyncBinder_OnNone_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.AndThenAsync(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task AndThenAsync_TaskOption_WithAsyncBinder_OnNone_ReturnsNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.AndThenAsync(async x =>
        {
            await Task.Delay(1);
            return Option<string>.Some(x.ToString());
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task MatchAsync_TaskOption_WithSyncHandlers_OnSome()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        var result = await optionTask.MatchAsync(x => $"Value: {x}", () => "None");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public async Task MatchAsync_TaskOption_WithSyncHandlers_OnNone()
    {
        var optionTask = Task.FromResult(Option<int>.None());
        var result = await optionTask.MatchAsync(x => $"Value: {x}", () => "None");

        Assert.Equal("None", result);
    }

    #endregion

    #region Option<T> with async functions

    [Fact]
    public async Task FilterAsync_Option_WithAsyncPredicate_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = await option.FilterAsync(async x =>
        {
            await Task.Delay(1);
            return x > 0;
        });

        Assert.True(result.IsNone);
    }

    #endregion

    #region TapNoneAsync

    [Fact]
    public async Task TapNoneAsync_OnNone_ExecutesAction()
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

    [Fact]
    public async Task TapNoneAsync_OnSome_DoesNotExecuteAction()
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
        Assert.Equal(42, result.Unwrap());
    }

    #endregion

    #region ZipAsync

    [Fact]
    public async Task ZipAsync_BothSome_ReturnsTuple()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        var second = Task.FromResult(Option<string>.Some("hello"));

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsSome);
        var (a, b) = result.Unwrap();
        Assert.Equal(42, a);
        Assert.Equal("hello", b);
    }

    [Fact]
    public async Task ZipAsync_FirstNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.None());
        var second = Task.FromResult(Option<string>.Some("hello"));

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ZipAsync_SecondNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        var second = Task.FromResult(Option<string>.None());

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ZipAsync_BothNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.None());
        var second = Task.FromResult(Option<string>.None());

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsNone);
    }

    #endregion

    #region ZipWithAsync

    [Fact]
    public async Task ZipWithAsync_BothSome_AppliesCombiner()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        var second = Task.FromResult(Option<int>.Some(8));

        var result = await OptionAsyncExtensions.ZipWithAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsSome);
        Assert.Equal(50, result.Unwrap());
    }

    [Fact]
    public async Task ZipWithAsync_FirstNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.None());
        var second = Task.FromResult(Option<int>.Some(8));

        var result = await OptionAsyncExtensions.ZipWithAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ZipWithAsync_SecondNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        var second = Task.FromResult(Option<int>.None());

        var result = await OptionAsyncExtensions.ZipWithAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ZipWithAsync_BothNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.None());
        var second = Task.FromResult(Option<int>.None());

        var result = await OptionAsyncExtensions.ZipWithAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsNone);
    }

    #endregion

    #region FirstSomeAsync

    [Fact]
    public async Task FirstSomeAsync_WithSomeFirst_ReturnsFirst()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.Some(1)),
            Task.FromResult(Option<int>.Some(2)),
            Task.FromResult(Option<int>.Some(3))
        };

        var result = await tasks.FirstSomeAsync();

        Assert.True(result.IsSome);
        Assert.Equal(1, result.Unwrap());
    }

    [Fact]
    public async Task FirstSomeAsync_WithNoneFirst_SkipsToSome()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.None()),
            Task.FromResult(Option<int>.Some(2)),
            Task.FromResult(Option<int>.Some(3))
        };

        var result = await tasks.FirstSomeAsync();

        Assert.True(result.IsSome);
        Assert.Equal(2, result.Unwrap());
    }

    [Fact]
    public async Task FirstSomeAsync_AllNone_ReturnsNone()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.None()),
            Task.FromResult(Option<int>.None()),
            Task.FromResult(Option<int>.None())
        };

        var result = await tasks.FirstSomeAsync();

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FirstSomeAsync_Empty_ReturnsNone()
    {
        var tasks = Array.Empty<Task<Option<int>>>();

        var result = await tasks.FirstSomeAsync();

        Assert.True(result.IsNone);
    }

    #endregion

    #region ValueTask Overloads

    [Fact]
    public async Task ValueTask_MapAsync_Sync_CompletedSuccessfully_OnSome()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.MapAsync(x => x * 2);

        Assert.True(result.IsSome);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public async Task ValueTask_MapAsync_Sync_CompletedSuccessfully_OnNone()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.None());
        var result = await optionTask.MapAsync(x => x * 2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ValueTask_MapAsync_Sync_NotCompleted()
    {
        var optionTask = CreateDelayedValueTask(Option<int>.Some(42));
        var result = await optionTask.MapAsync(x => x * 2);

        Assert.True(result.IsSome);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public async Task ValueTask_MapAsync_Async_OnSome()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(result.IsSome);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public async Task ValueTask_MapAsync_Async_OnNone()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.None());
        var result = await optionTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ValueTask_AndThenAsync_Sync_CompletedSuccessfully_OnSome()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.AndThenAsync(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsSome);
        Assert.Equal("42", result.Unwrap());
    }

    [Fact]
    public async Task ValueTask_AndThenAsync_Sync_CompletedSuccessfully_OnNone()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.None());
        var result = await optionTask.AndThenAsync(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ValueTask_AndThenAsync_Sync_NotCompleted()
    {
        var optionTask = CreateDelayedValueTask(Option<int>.Some(42));
        var result = await optionTask.AndThenAsync(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsSome);
        Assert.Equal("42", result.Unwrap());
    }

    [Fact]
    public async Task ValueTask_AndThenAsync_Async_OnSome()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.AndThenAsync(async x =>
        {
            await Task.Delay(1);
            return Option<string>.Some(x.ToString());
        });

        Assert.True(result.IsSome);
        Assert.Equal("42", result.Unwrap());
    }

    [Fact]
    public async Task ValueTask_AndThenAsync_Async_OnNone()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.None());
        var result = await optionTask.AndThenAsync(async x =>
        {
            await Task.Delay(1);
            return Option<string>.Some(x.ToString());
        });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ValueTask_FilterAsync_CompletedSuccessfully_Passes()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(x => x > 40);

        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task ValueTask_FilterAsync_CompletedSuccessfully_Fails()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(x => x < 40);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ValueTask_FilterAsync_CompletedSuccessfully_OnNone()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.None());
        var result = await optionTask.FilterAsync(x => x > 0);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task ValueTask_FilterAsync_NotCompleted()
    {
        var optionTask = CreateDelayedValueTask(Option<int>.Some(42));
        var result = await optionTask.FilterAsync(x => x > 40);

        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task ValueTask_MatchAsync_CompletedSuccessfully_OnSome()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.Some(42));
        var result = await optionTask.MatchAsync(x => $"Value: {x}", () => "None");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public async Task ValueTask_MatchAsync_CompletedSuccessfully_OnNone()
    {
        var optionTask = new ValueTask<Option<int>>(Option<int>.None());
        var result = await optionTask.MatchAsync(x => $"Value: {x}", () => "None");

        Assert.Equal("None", result);
    }

    [Fact]
    public async Task ValueTask_MatchAsync_NotCompleted()
    {
        var optionTask = CreateDelayedValueTask(Option<int>.Some(42));
        var result = await optionTask.MatchAsync(x => $"Value: {x}", () => "None");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public async Task AsValueTask_WrapsOption()
    {
        var option = Option<int>.Some(42);
        var valueTask = option.AsValueTask();

        var result = await valueTask;
        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public async Task AsValueTask_WrapsNone()
    {
        var option = Option<int>.None();
        var valueTask = option.AsValueTask();

        var result = await valueTask;
        Assert.True(result.IsNone);
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public async Task MapAsync_TaskOption_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.MapAsync(async x => x));
    }

    [Fact]
    public async Task MapAsync_TaskOption_ThrowsOnNullMapper()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.MapAsync((Func<int, Task<int>>)null!));
    }

    [Fact]
    public async Task FilterAsync_TaskOption_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.FilterAsync(async x => true));
    }

    [Fact]
    public async Task FilterAsync_TaskOption_ThrowsOnNullPredicate()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.FilterAsync((Func<int, Task<bool>>)null!));
    }

    [Fact]
    public async Task AndThenAsync_TaskOption_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.AndThenAsync(async x => Option<int>.Some(x)));
    }

    [Fact]
    public async Task AndThenAsync_TaskOption_ThrowsOnNullBinder()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.AndThenAsync((Func<int, Task<Option<int>>>)null!));
    }

    [Fact]
    public async Task UnwrapOrElseAsync_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.UnwrapOrElseAsync(async () => 0));
    }

    [Fact]
    public async Task UnwrapOrElseAsync_ThrowsOnNullDefaultFunc()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.UnwrapOrElseAsync(null!));
    }

    [Fact]
    public async Task MatchAsync_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.MatchAsync(async x => x, async () => 0));
    }

    [Fact]
    public async Task MatchAsync_ThrowsOnNullSomeFunc()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.MatchAsync((Func<int, Task<int>>)null!, async () => 0));
    }

    [Fact]
    public async Task MatchAsync_ThrowsOnNullNoneFunc()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.MatchAsync(async x => x, (Func<Task<int>>)null!));
    }

    [Fact]
    public async Task TapAsync_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.TapAsync(async x => { }));
    }

    [Fact]
    public async Task TapAsync_ThrowsOnNullAction()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.TapAsync(null!));
    }

    [Fact]
    public async Task OkOrElseAsync_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.OkOrElseAsync(async () => "error"));
    }

    [Fact]
    public async Task OkOrElseAsync_ThrowsOnNullErrFunc()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.OkOrElseAsync((Func<Task<string>>)null!));
    }

    [Fact]
    public async Task MapAsync_Option_ThrowsOnNullMapper()
    {
        var option = Option<int>.Some(42);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            option.MapAsync((Func<int, Task<int>>)null!));
    }

    [Fact]
    public async Task AndThenAsync_Option_ThrowsOnNullBinder()
    {
        var option = Option<int>.Some(42);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            option.AndThenAsync((Func<int, Task<Option<int>>>)null!));
    }

    [Fact]
    public async Task FilterAsync_Option_ThrowsOnNullPredicate()
    {
        var option = Option<int>.Some(42);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            option.FilterAsync((Func<int, Task<bool>>)null!));
    }

    [Fact]
    public async Task TapNoneAsync_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.TapNoneAsync(async () => { }));
    }

    [Fact]
    public async Task TapNoneAsync_ThrowsOnNullAction()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.TapNoneAsync(null!));
    }

    [Fact]
    public async Task ZipAsync_ThrowsOnNullFirstTask()
    {
        Task<Option<int>> nullTask = null!;
        var second = Task.FromResult(Option<string>.Some("hello"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OptionAsyncExtensions.ZipAsync(nullTask, second));
    }

    [Fact]
    public async Task ZipAsync_ThrowsOnNullSecondTask()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        Task<Option<string>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OptionAsyncExtensions.ZipAsync(first, nullTask));
    }

    [Fact]
    public async Task ZipWithAsync_ThrowsOnNullFirstTask()
    {
        Task<Option<int>> nullTask = null!;
        var second = Task.FromResult(Option<int>.Some(8));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OptionAsyncExtensions.ZipWithAsync(nullTask, second, (a, b) => a + b));
    }

    [Fact]
    public async Task ZipWithAsync_ThrowsOnNullSecondTask()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OptionAsyncExtensions.ZipWithAsync(first, nullTask, (a, b) => a + b));
    }

    [Fact]
    public async Task ZipWithAsync_ThrowsOnNullCombiner()
    {
        var first = Task.FromResult(Option<int>.Some(42));
        var second = Task.FromResult(Option<int>.Some(8));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OptionAsyncExtensions.ZipWithAsync(first, second, (Func<int, int, int>)null!));
    }

    [Fact]
    public async Task FirstSomeAsync_ThrowsOnNullTasks()
    {
        IEnumerable<Task<Option<int>>> nullTasks = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTasks.FirstSomeAsync());
    }

    [Fact]
    public async Task OrElseAsync_Option_ThrowsOnNullAlternative()
    {
        var option = Option<int>.Some(42);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            option.OrElseAsync(null!));
    }

    [Fact]
    public async Task OrElseAsync_TaskOption_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.OrElseAsync(async () => Option<int>.Some(0)));
    }

    [Fact]
    public async Task OrElseAsync_TaskOption_ThrowsOnNullAlternative()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            optionTask.OrElseAsync(null!));
    }

    [Fact]
    public async Task OrAsync_ThrowsOnNullTask()
    {
        Task<Option<int>> nullTask = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullTask.OrAsync(Option<int>.Some(0)));
    }

    #endregion

    private static async ValueTask<Option<T>> CreateDelayedValueTask<T>(Option<T> value)
    {
        await Task.Delay(1);
        return value;
    }
}
