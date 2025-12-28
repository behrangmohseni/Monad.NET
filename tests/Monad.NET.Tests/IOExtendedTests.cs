using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for IO<T> and IOAsync<T> to improve code coverage.
/// </summary>
public class IOExtendedTests
{
    #region IO Factory Tests

    [Fact]
    public void Of_CreatesIOFromFunc()
    {
        var io = IO<int>.Of(() => 42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Pure_CreatesIOWithValue()
    {
        var io = IO<int>.Pure(42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Return_CreatesIOWithValue()
    {
        var io = IO<int>.Return(42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Delay_CreatesLazyIO()
    {
        var evaluated = false;
        var io = IO<int>.Delay(() =>
        {
            evaluated = true;
            return 42;
        });

        Assert.False(evaluated);
        Assert.Equal(42, io.Run());
        Assert.True(evaluated);
    }

    #endregion

    #region IO Map Tests

    [Fact]
    public void Map_TransformsResult()
    {
        var io = IO<int>.Pure(10).Map(x => x * 2);
        Assert.Equal(20, io.Run());
    }

    #endregion

    #region IO AndThen/FlatMap/Bind Tests

    [Fact]
    public void AndThen_ChainsIO()
    {
        var io = IO<int>.Pure(10)
            .AndThen(x => IO<string>.Pure($"Value: {x}"));
        Assert.Equal("Value: 10", io.Run());
    }

    [Fact]
    public void FlatMap_ChainsIO()
    {
        var io = IO<int>.Pure(10)
            .FlatMap(x => IO<string>.Pure($"Value: {x}"));
        Assert.Equal("Value: 10", io.Run());
    }

    [Fact]
    public void Bind_ChainsIO()
    {
        var io = IO<int>.Pure(10)
            .Bind(x => IO<string>.Pure($"Value: {x}"));
        Assert.Equal("Value: 10", io.Run());
    }

    #endregion

    #region IO Tap Tests

    [Fact]
    public void Tap_ExecutesActionWithResult()
    {
        var capturedValue = 0;
        var io = IO<int>.Pure(42).Tap(x => capturedValue = x);

        Assert.Equal(42, io.Run());
        Assert.Equal(42, capturedValue);
    }

    #endregion

    #region IO Apply Tests

    [Fact]
    public void Apply_AppliesWrappedFunction()
    {
        var ioFunc = IO<Func<int, int>>.Pure(x => x * 2);
        var ioValue = IO<int>.Pure(21);
        Assert.Equal(42, ioValue.Apply(ioFunc).Run());
    }

    #endregion

    #region IO Zip Tests

    [Fact]
    public void Zip_CombinesTwoIOs()
    {
        var io1 = IO<int>.Pure(10);
        var io2 = IO<string>.Pure("hello");
        Assert.Equal((10, "hello"), io1.Zip(io2).Run());
    }

    [Fact]
    public void ZipWith_CombinesWithFunction()
    {
        var io1 = IO<int>.Pure(10);
        var io2 = IO<int>.Pure(32);
        Assert.Equal(42, io1.ZipWith(io2, (a, b) => a + b).Run());
    }

    #endregion

    #region IO As and Void Tests

    [Fact]
    public void As_ReplacesResult()
    {
        var io = IO<int>.Pure(42).As("replaced");
        Assert.Equal("replaced", io.Run());
    }

    [Fact]
    public void Void_ReplacesResultWithUnit()
    {
        var io = IO<int>.Pure(42).Void();
        Assert.Equal(Unit.Default, io.Run());
    }

    #endregion

    #region IO Attempt Tests

    [Fact]
    public void Attempt_Success_ReturnsSuccessTry()
    {
        var io = IO<int>.Pure(42).Attempt();
        var tryResult = io.Run();

        Assert.True(tryResult.IsSuccess);
        Assert.Equal(42, tryResult.Get());
    }

    [Fact]
    public void Attempt_Failure_ReturnsFailureTry()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException("test")).Attempt();
        var tryResult = io.Run();

        Assert.True(tryResult.IsFailure);
        Assert.IsType<InvalidOperationException>(tryResult.GetException());
    }

    #endregion

    #region IO OrElse Tests

    [Fact]
    public void OrElse_Success_ReturnsOriginal()
    {
        var io = IO<int>.Pure(42).OrElse(IO<int>.Pure(99));
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void OrElse_Failure_ReturnsFallback()
    {
        var io = IO<int>.Of(() => throw new Exception("fail")).OrElse(IO<int>.Pure(99));
        Assert.Equal(99, io.Run());
    }

    [Fact]
    public void OrElse_WithValue_Failure_ReturnsFallbackValue()
    {
        var io = IO<int>.Of(() => throw new Exception("fail")).OrElse(99);
        Assert.Equal(99, io.Run());
    }

    #endregion

    #region IO Replicate Tests

    [Fact]
    public void Replicate_RepeatsIO()
    {
        var counter = 0;
        var io = IO<int>.Of(() => ++counter);
        var results = io.Replicate(3).Run();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    #endregion

    #region IO Retry Tests

    [Fact]
    public void Retry_SucceedsImmediately()
    {
        var io = IO<int>.Pure(42).Retry(3);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Retry_RetriesOnFailure()
    {
        var attempts = 0;
        var io = IO<int>.Of(() =>
        {
            attempts++;
            if (attempts < 3)
                throw new Exception("fail");
            return 42;
        }).Retry(3);

        Assert.Equal(42, io.Run());
        Assert.Equal(3, attempts);
    }

    #endregion

    #region IO Static Helpers Tests

    [Fact]
    public void IO_Of_CreatesIO()
    {
        var io = IO.Of(() => 42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void IO_Pure_CreatesIO()
    {
        var io = IO.Pure(42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void IO_Execute_CreatesUnitIO()
    {
        var executed = false;
        var io = IO.Execute(() => executed = true);
        io.Run();

        Assert.True(executed);
    }

    [Fact]
    public void IO_Now_ReturnsCurrentTime()
    {
        var before = DateTime.Now;
        var io = IO.Now();
        var time = io.Run();
        var after = DateTime.Now;

        Assert.True(time >= before && time <= after);
    }

    [Fact]
    public void IO_UtcNow_ReturnsCurrentUtcTime()
    {
        var before = DateTime.UtcNow;
        var io = IO.UtcNow();
        var time = io.Run();
        var after = DateTime.UtcNow;

        Assert.True(time >= before && time <= after);
    }

    [Fact]
    public void IO_NewGuid_ReturnsNewGuid()
    {
        var io = IO.NewGuid();
        var guid1 = io.Run();
        var guid2 = io.Run();

        Assert.NotEqual(Guid.Empty, guid1);
        Assert.NotEqual(guid1, guid2);
    }

    [Fact]
    public void IO_Random_ReturnsRandomNumber()
    {
        var io = IO.Random();
        var num = io.Run();

        Assert.True(num >= 0);
    }

    [Fact]
    public void IO_Random_WithRange_ReturnsInRange()
    {
        var io = IO.Random(10, 20);
        var num = io.Run();

        Assert.True(num >= 10 && num < 20);
    }

    [Fact]
    public void IO_Parallel_TwoIOs_RunsBoth()
    {
        var io1 = IO<int>.Pure(1);
        var io2 = IO<int>.Pure(2);
        var result = IO.Parallel(io1, io2).Run();

        Assert.Equal((1, 2), result);
    }

    [Fact]
    public void IO_Parallel_ThreeIOs_RunsAll()
    {
        var io1 = IO<int>.Pure(1);
        var io2 = IO<int>.Pure(2);
        var io3 = IO<int>.Pure(3);
        var result = IO.Parallel(io1, io2, io3).Run();

        Assert.Equal((1, 2, 3), result);
    }

    [Fact]
    public void IO_Parallel_Collection_RunsAll()
    {
        var ios = new[] { IO<int>.Pure(1), IO<int>.Pure(2), IO<int>.Pure(3) };
        var result = IO.Parallel(ios).Run();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void IO_Race_ReturnsFirstCompleted()
    {
        var io1 = IO<int>.Pure(1);
        var io2 = IO<int>.Pure(2);
        var result = IO.Race(io1, io2).Run();

        Assert.True(result == 1 || result == 2);
    }

    #endregion

    #region IOExtensions Tests

    [Fact]
    public void Flatten_UnwrapsNestedIO()
    {
        var nested = IO<IO<int>>.Pure(IO<int>.Pure(42));
        var flattened = nested.Flatten();

        Assert.Equal(42, flattened.Run());
    }

    [Fact]
    public void Sequence_CombinesIOs()
    {
        var ios = new[] { IO<int>.Pure(1), IO<int>.Pure(2), IO<int>.Pure(3) };
        var sequenced = ios.Sequence();

        Assert.Equal(new[] { 1, 2, 3 }, sequenced.Run());
    }

    [Fact]
    public void Traverse_AppliesFunctionToEach()
    {
        var source = new[] { 1, 2, 3 };
        var traversed = source.Traverse(x => IO<int>.Pure(x * 10));

        Assert.Equal(new[] { 10, 20, 30 }, traversed.Run());
    }

    [Fact]
    public void Select_LinqSupport()
    {
        var io = from x in IO<int>.Pure(21)
                 select x * 2;

        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void SelectMany_LinqSupport()
    {
        var io = from x in IO<int>.Pure(10)
                 from y in IO<int>.Pure(32)
                 select x + y;

        Assert.Equal(42, io.Run());
    }

    #endregion

    #region IOAsync Tests

    [Fact]
    public async Task IOAsync_Of_CreatesAsync()
    {
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            return 42;
        });
        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Pure_CreatesAsync()
    {
        var io = IOAsync<int>.Pure(42);
        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_FromIO_ConvertsSync()
    {
        var syncIO = IO<int>.Pure(42);
        var asyncIO = IOAsync<int>.FromIO(syncIO);
        Assert.Equal(42, await asyncIO.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Map_TransformsResult()
    {
        var io = IOAsync<int>.Pure(10).Map(x => x * 2);
        Assert.Equal(20, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_MapAsync_TransformsAsync()
    {
        var io = IOAsync<int>.Pure(10).MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });
        Assert.Equal(20, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_AndThen_ChainsAsync()
    {
        var io = IOAsync<int>.Pure(10)
            .AndThen(x => IOAsync<string>.Pure($"Value: {x}"));
        Assert.Equal("Value: 10", await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Tap_ExecutesAction()
    {
        var capturedValue = 0;
        var io = IOAsync<int>.Pure(42).Tap(x => capturedValue = x);
        await io.RunAsync();

        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public async Task IOAsync_TapAsync_ExecutesAsyncAction()
    {
        var capturedValue = 0;
        var io = IOAsync<int>.Pure(42).TapAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });
        await io.RunAsync();

        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public async Task IOAsync_Zip_CombinesAsync()
    {
        var io1 = IOAsync<int>.Pure(10);
        var io2 = IOAsync<string>.Pure("hello");
        Assert.Equal((10, "hello"), await io1.Zip(io2).RunAsync());
    }

    [Fact]
    public async Task IOAsync_Void_ReturnsUnit()
    {
        var io = IOAsync<int>.Pure(42).Void();
        Assert.Equal(Unit.Default, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Attempt_Success()
    {
        var io = IOAsync<int>.Pure(42).Attempt();
        var tryResult = await io.RunAsync();

        Assert.True(tryResult.IsSuccess);
    }

    [Fact]
    public async Task IOAsync_Attempt_Failure()
    {
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("test");
        }).Attempt();
        var tryResult = await io.RunAsync();

        Assert.True(tryResult.IsFailure);
    }

    [Fact]
    public async Task IOAsync_OrElse_Success()
    {
        var io = IOAsync<int>.Pure(42).OrElse(IOAsync<int>.Pure(99));
        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_OrElse_Failure()
    {
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            throw new Exception("fail");
        }).OrElse(IOAsync<int>.Pure(99));
        Assert.Equal(99, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Retry_Success()
    {
        var io = IOAsync<int>.Pure(42).Retry(3);
        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Retry_RetriesOnFailure()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            attempts++;
            if (attempts < 3)
                throw new Exception("fail");
            return 42;
        }).Retry(3);

        Assert.Equal(42, await io.RunAsync());
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task IOAsync_RetryWhile_RetriesBasedOnCondition()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException("retry");
            return 42;
        }).RetryWhile((ex, attempt) => ex is InvalidOperationException && attempt < 5);

        Assert.Equal(42, await io.RunAsync());
        Assert.Equal(3, attempts);
    }

    #endregion

    #region IOAsync Static Helpers Tests

    [Fact]
    public async Task IOAsync_Of_StaticHelper()
    {
        var io = IOAsync.Of(async () =>
        {
            await Task.Delay(1);
            return 42;
        });
        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Pure_StaticHelper()
    {
        var io = IOAsync.Pure(42);
        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Execute_CreatesUnitAsync()
    {
        var executed = false;
        var io = IOAsync.Execute(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });
        await io.RunAsync();

        Assert.True(executed);
    }

    [Fact]
    public async Task IOAsync_Delay_WaitsForDuration()
    {
        var before = DateTime.Now;
        var io = IOAsync.Delay(TimeSpan.FromMilliseconds(50));
        await io.RunAsync();
        var after = DateTime.Now;

        Assert.True(after - before >= TimeSpan.FromMilliseconds(40)); // allow some tolerance
    }

    [Fact]
    public async Task IOAsync_Race_ReturnsFirst()
    {
        var io1 = IOAsync<int>.Pure(1);
        var io2 = IOAsync<int>.Pure(2);
        var result = await IOAsync.Race(io1, io2).RunAsync();

        Assert.True(result == 1 || result == 2);
    }

    #endregion

    #region IOAsync Extension Tests

    [Fact]
    public async Task IOAsync_Flatten_Unwraps()
    {
        var nested = IOAsync<IOAsync<int>>.Pure(IOAsync<int>.Pure(42));
        var flattened = nested.Flatten();

        Assert.Equal(42, await flattened.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Sequence_CombinesAsync()
    {
        var ios = new[] { IOAsync<int>.Pure(1), IOAsync<int>.Pure(2), IOAsync<int>.Pure(3) };
        var sequenced = ios.Sequence();

        Assert.Equal(new[] { 1, 2, 3 }, await sequenced.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Traverse_AppliesFunction()
    {
        var source = new[] { 1, 2, 3 };
        var traversed = source.Traverse(x => IOAsync<int>.Pure(x * 10));

        Assert.Equal(new[] { 10, 20, 30 }, await traversed.RunAsync());
    }

    [Fact]
    public async Task IOAsync_Select_LinqSupport()
    {
        var io = from x in IOAsync<int>.Pure(21)
                 select x * 2;

        Assert.Equal(42, await io.RunAsync());
    }

    [Fact]
    public async Task IOAsync_SelectMany_LinqSupport()
    {
        var io = from x in IOAsync<int>.Pure(10)
                 from y in IOAsync<int>.Pure(32)
                 select x + y;

        Assert.Equal(42, await io.RunAsync());
    }

    #endregion
}

