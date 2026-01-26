using Monad.NET;

namespace Monad.NET.Tests;

public class IOTests
{
    #region Construction

    [Fact]
    public void Of_CreatesIOFromEffect()
    {
        var counter = 0;
        var io = IO<int>.Of(() =>
        {
            counter++;
            return 42;
        });

        // Effect should not run until Run() is called
        Assert.Equal(0, counter);

        var result = io.Run();
        Assert.Equal(42, result);
        Assert.Equal(1, counter);
    }

    [Fact]
    public void Pure_CreatesIOWithValue()
    {
        var io = IO<int>.Return(42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Return_AliasForPure()
    {
        var io = IO<int>.Return(42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Delay_DefersExecution()
    {
        var counter = 0;
        var io = IO.Of(() =>
        {
            counter++;
            return 42;
        });

        Assert.Equal(0, counter);
        io.Run();
        Assert.Equal(1, counter);
    }

    #endregion

    #region Run and RunAsync

    [Fact]
    public void Run_ExecutesEffect()
    {
        var executed = false;
        var io = IO<Unit>.Of(() =>
        {
            executed = true;
            return Unit.Default;
        });

        Assert.False(executed);
        io.Run();
        Assert.True(executed);
    }

    [Fact]
    public async Task RunAsync_ExecutesEffectAsynchronously()
    {
        var io = IO<int>.Of(() => 42);
        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    #endregion

    #region Map

    [Fact]
    public void Map_TransformsResult()
    {
        var io = IO<int>.Return(21).Map(x => x * 2);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void Map_ChainedTransformations()
    {
        var io = IO<int>.Return(10)
            .Map(x => x + 5)
            .Map(x => x * 2)
            .Map(x => x.ToString());

        Assert.Equal("30", io.Run());
    }

    [Fact]
    public void Map_PreservesEffectOrder()
    {
        var log = new List<string>();
        var io = IO<int>.Of(() =>
            {
                log.Add("effect");
                return 1;
            })
            .Map(x =>
            {
                log.Add("map1");
                return x + 1;
            })
            .Map(x =>
            {
                log.Add("map2");
                return x + 1;
            });

        io.Run();
        Assert.Equal(new[] { "effect", "map1", "map2" }, log);
    }

    #endregion

    #region AndThen / FlatMap / Bind

    [Fact]
    public void AndThen_ChainsIOActions()
    {
        var io = IO<int>.Return(10)
            .Bind(x => IO<int>.Return(x + 5))
            .Bind(x => IO<string>.Return(x.ToString()));

        Assert.Equal("15", io.Run());
    }

    [Fact]
    public void FlatMap_AliasForAndThen()
    {
        var io = IO<int>.Return(10).Bind(x => IO<int>.Return(x * 2));
        Assert.Equal(20, io.Run());
    }

    [Fact]
    public void Bind_AliasForAndThen()
    {
        var io = IO<int>.Return(10).Bind(x => IO<int>.Return(x * 2));
        Assert.Equal(20, io.Run());
    }

    [Fact]
    public void AndThen_ExecutesEffectsInSequence()
    {
        var log = new List<string>();

        var io1 = IO<int>.Of(() =>
        {
            log.Add("io1");
            return 1;
        });

        var io2 = IO<int>.Of(() =>
        {
            log.Add("io2");
            return 2;
        });

        var combined = io1.Bind(_ => io2);
        combined.Run();

        Assert.Equal(new[] { "io1", "io2" }, log);
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_ExecutesSideEffectWithoutChangingValue()
    {
        var log = new List<int>();
        var io = IO<int>.Return(42)
            .Tap(x => log.Add(x))
            .Map(x => x * 2);

        var result = io.Run();
        Assert.Equal(84, result);
        Assert.Equal(new[] { 42 }, log);
    }

    #endregion

    #region Apply

    [Fact]
    public void Apply_AppliesFunctionInIO()
    {
        var ioFunc = IO<Func<int, int>>.Return(x => x * 2);
        var ioValue = IO<int>.Return(21);

        var result = ioValue.Apply(ioFunc);
        Assert.Equal(42, result.Run());
    }

    #endregion

    #region Zip and ZipWith

    [Fact]
    public void Zip_CombinesTwoIOs()
    {
        var io1 = IO<int>.Return(1);
        var io2 = IO<string>.Return("hello");

        var combined = io1.Zip(io2);
        Assert.Equal((1, "hello"), combined.Run());
    }

    [Fact]
    public void ZipWith_CombinesWithFunction()
    {
        var io1 = IO<int>.Return(10);
        var io2 = IO<int>.Return(5);

        var combined = io1.ZipWith(io2, (a, b) => a + b);
        Assert.Equal(15, combined.Run());
    }

    #endregion

    #region As and Void

    [Fact]
    public void As_ReplacesValue()
    {
        var io = IO<int>.Return(42).As("hello");
        Assert.Equal("hello", io.Run());
    }

    [Fact]
    public void Void_ReplacesValueWithUnit()
    {
        var io = IO<int>.Return(42).Void();
        Assert.Equal(Unit.Default, io.Run());
    }

    #endregion

    #region Attempt

    [Fact]
    public void Attempt_ReturnsSuccessOnSuccess()
    {
        var io = IO<int>.Return(42).Attempt();
        var result = io.Run();
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValueOr(-1));
    }

    [Fact]
    public void Attempt_ReturnsFailureOnException()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException("test")).Attempt();
        var result = io.Run();
        Assert.True(result.IsFailure);
    }

    #endregion

    #region ToAsync

    [Fact]
    public async Task ToAsync_ConvertsToAsyncIO()
    {
        var io = IO<int>.Return(42);
        var asyncIo = io.ToAsync();
        var result = await asyncIo.RunAsync();
        Assert.Equal(42, result);
    }

    #endregion

    #region OrElse

    [Fact]
    public void OrElse_ReturnsOriginalOnSuccess()
    {
        var io = IO<int>.Return(42).OrElse(IO<int>.Return(0));
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void OrElse_ReturnsFallbackOnException()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException())
            .OrElse(IO<int>.Return(0));
        Assert.Equal(0, io.Run());
    }

    [Fact]
    public void OrElseValue_ReturnsFallbackValueOnException()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException())
            .OrElse(99);
        Assert.Equal(99, io.Run());
    }

    #endregion

    #region Replicate

    [Fact]
    public void Replicate_RepeatsEffect()
    {
        var counter = 0;
        var io = IO<int>.Of(() => ++counter).Replicate(3);

        var results = io.Run();
        Assert.Equal(new[] { 1, 2, 3 }, results);
        Assert.Equal(3, counter);
    }

    [Fact]
    public void Replicate_ZeroTimes()
    {
        var io = IO<int>.Return(42).Replicate(0);
        Assert.Empty(io.Run());
    }

    #endregion

    #region Retry

    [Fact]
    public void Retry_SucceedsOnFirstAttempt()
    {
        var io = IO<int>.Return(42).Retry(3);
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
                throw new InvalidOperationException();
            return 42;
        }).Retry(5);

        Assert.Equal(42, io.Run());
        Assert.Equal(3, attempts);
    }

    [Fact]
    public void Retry_ThrowsAfterAllRetriesExhausted()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException("test")).Retry(2);

        Assert.Throws<InvalidOperationException>(() => io.Run());
    }

    #endregion

    #region RetryWithDelay

    [Fact]
    public async Task RetryWithDelay_RetriesWithDelay()
    {
        var attempts = 0;
        var io = IO<int>.Of(() =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException();
            return 42;
        }).RetryWithDelay(3, TimeSpan.FromMilliseconds(10));

        var result = await io.RunAsync();
        Assert.Equal(42, result);
        Assert.Equal(2, attempts);
    }

    #endregion

    #region LINQ Support

    [Fact]
    public void LinqQuery_SelectWorks()
    {
        var io = from x in IO<int>.Return(21)
                 select x * 2;

        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void LinqQuery_SelectManyWorks()
    {
        var io = from x in IO<int>.Return(10)
                 from y in IO<int>.Return(5)
                 select x + y;

        Assert.Equal(15, io.Run());
    }

    [Fact]
    public void LinqQuery_ComplexQuery()
    {
        var io = from a in IO<int>.Return(1)
                 from b in IO<int>.Return(2)
                 from c in IO<int>.Return(3)
                 let sum = a + b + c
                 select sum * 2;

        Assert.Equal(12, io.Run());
    }

    #endregion

    #region Extensions - Flatten

    [Fact]
    public void Flatten_UnwrapsNestedIO()
    {
        var nested = IO<IO<int>>.Return(IO<int>.Return(42));
        var flattened = nested.Flatten();
        Assert.Equal(42, flattened.Run());
    }

    #endregion

    #region Extensions - Sequence

    [Fact]
    public void Sequence_ExecutesAllIOs()
    {
        var counter = 0;
        var ios = new[]
        {
            IO<int>.Of(() => ++counter),
            IO<int>.Of(() => ++counter),
            IO<int>.Of(() => ++counter)
        };

        var sequenced = ios.Sequence();
        var results = sequenced.Run();

        Assert.Equal(new[] { 1, 2, 3 }, results);
        Assert.Equal(3, counter);
    }

    #endregion

    #region Extensions - Traverse

    [Fact]
    public void Traverse_MapsAndSequences()
    {
        var numbers = new[] { 1, 2, 3 };
        var io = numbers.Traverse(x => IO<int>.Return(x * 2));

        var results = io.Run();
        Assert.Equal(new[] { 2, 4, 6 }, results);
    }

    #endregion

    #region Static Helpers

    [Fact]
    public void IO_Of_CreatesIO()
    {
        var io = IO.Of(() => 42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void IO_Pure_CreatesPureIO()
    {
        var io = IO.Return(42);
        Assert.Equal(42, io.Run());
    }

    [Fact]
    public void IO_Execute_ExecutesAction()
    {
        var executed = false;
        var io = IO.Execute(() => executed = true);

        Assert.False(executed);
        io.Run();
        Assert.True(executed);
    }

    [Fact]
    public void IO_WriteLine_CreatesWriteLineIO()
    {
        // This just verifies it compiles and doesn't throw
        var io = IO.WriteLine("test");
        Assert.Equal(Unit.Default, io.Run());
    }

    [Fact]
    public void IO_Now_ReturnsCurrentTime()
    {
        var before = DateTime.Now;
        var io = IO.Now();
        var result = io.Run();
        var after = DateTime.Now;

        Assert.True(result >= before);
        Assert.True(result <= after);
    }

    [Fact]
    public void IO_UtcNow_ReturnsCurrentUtcTime()
    {
        var io = IO.UtcNow();
        var result = io.Run();
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void IO_NewGuid_GeneratesGuid()
    {
        var io = IO.NewGuid();
        var guid1 = io.Run();
        var guid2 = io.Run();

        Assert.NotEqual(Guid.Empty, guid1);
        Assert.NotEqual(Guid.Empty, guid2);
        Assert.NotEqual(guid1, guid2);
    }

    [Fact]
    public void IO_Random_GeneratesRandomInt()
    {
        var io = IO.Random();
        var r1 = io.Run();

        // Just verify we get an integer
        Assert.True(r1 >= int.MinValue && r1 <= int.MaxValue);
    }

    [Fact]
    public void IO_RandomWithRange_GeneratesInRange()
    {
        var io = IO.Random(1, 100);
        var result = io.Run();
        Assert.InRange(result, 1, 99); // maxValue is exclusive
    }

    [Fact]
    public void IO_GetEnvironmentVariable_ReturnsOption()
    {
        var io = IO.GetEnvironmentVariable("PATH");
        var result = io.Run();
        Assert.True(result.IsSome);

        var io2 = IO.GetEnvironmentVariable("NONEXISTENT_VAR_123456");
        var result2 = io2.Run();
        Assert.True(result2.IsNone);
    }

    #endregion

    #region Parallel Execution

    [Fact]
    public void IO_Parallel_TwoIOs()
    {
        var io1 = IO<int>.Return(1);
        var io2 = IO<int>.Return(2);

        var result = IO.Parallel(io1, io2).Run();
        Assert.Equal((1, 2), result);
    }

    [Fact]
    public void IO_Parallel_ThreeIOs()
    {
        var io1 = IO<int>.Return(1);
        var io2 = IO<int>.Return(2);
        var io3 = IO<int>.Return(3);

        var result = IO.Parallel(io1, io2, io3).Run();
        Assert.Equal((1, 2, 3), result);
    }

    [Fact]
    public void IO_Parallel_Collection()
    {
        var ios = new[] { IO<int>.Return(1), IO<int>.Return(2), IO<int>.Return(3) };
        var result = IO.Parallel(ios).Run();
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void IO_Race_ReturnsFirstCompleted()
    {
        var io1 = IO<int>.Of(() =>
        {
            Thread.Sleep(100);
            return 1;
        });
        var io2 = IO<int>.Return(2); // This should complete first

        var result = IO.Race(io1, io2).Run();
        Assert.Equal(2, result);
    }

    #endregion
}

public class IOAsyncTests
{
    #region Construction

    [Fact]
    public async Task Of_CreatesAsyncIO()
    {
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Pure_CreatesPureAsyncIO()
    {
        var io = IOAsync<int>.Return(42);
        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task FromIO_ConvertsFromSyncIO()
    {
        var syncIo = IO<int>.Return(42);
        var asyncIo = IOAsync<int>.FromIO(syncIo);
        var result = await asyncIo.RunAsync();
        Assert.Equal(42, result);
    }

    #endregion

    #region Map

    [Fact]
    public async Task Map_TransformsResult()
    {
        var io = IOAsync<int>.Return(21).Map(x => x * 2);
        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task MapAsync_TransformsWithAsyncMapper()
    {
        var io = IOAsync<int>.Return(21).MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    #endregion

    #region AndThen / FlatMap

    [Fact]
    public async Task AndThen_ChainsAsyncIOs()
    {
        var io = IOAsync<int>.Return(10)
            .Bind(x => IOAsync<int>.Return(x + 5));

        var result = await io.RunAsync();
        Assert.Equal(15, result);
    }

    [Fact]
    public async Task FlatMap_AliasForAndThen()
    {
        var io = IOAsync<int>.Return(10).Bind(x => IOAsync<int>.Return(x * 2));
        var result = await io.RunAsync();
        Assert.Equal(20, result);
    }

    #endregion

    #region Tap

    [Fact]
    public async Task Tap_ExecutesSideEffect()
    {
        var log = new List<int>();
        var io = IOAsync<int>.Return(42).Tap(x => log.Add(x));

        await io.RunAsync();
        Assert.Equal(new[] { 42 }, log);
    }

    [Fact]
    public async Task TapAsync_ExecutesAsyncSideEffect()
    {
        var log = new List<int>();
        var io = IOAsync<int>.Return(42).TapAsync(async x =>
        {
            await Task.Delay(1);
            log.Add(x);
        });

        await io.RunAsync();
        Assert.Equal(new[] { 42 }, log);
    }

    #endregion

    #region Zip

    [Fact]
    public async Task Zip_CombinesTwoAsyncIOs()
    {
        var io1 = IOAsync<int>.Return(1);
        var io2 = IOAsync<string>.Return("hello");

        var combined = io1.Zip(io2);
        var result = await combined.RunAsync();
        Assert.Equal((1, "hello"), result);
    }

    #endregion

    #region Void

    [Fact]
    public async Task Void_ReplacesValueWithUnit()
    {
        var io = IOAsync<int>.Return(42).Void();
        var result = await io.RunAsync();
        Assert.Equal(Unit.Default, result);
    }

    #endregion

    #region Attempt

    [Fact]
    public async Task Attempt_ReturnsSuccessOnSuccess()
    {
        var io = IOAsync<int>.Return(42).Attempt();
        var result = await io.RunAsync();
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValueOr(-1));
    }

    [Fact]
    public async Task Attempt_ReturnsFailureOnException()
    {
        var io = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("test");
#pragma warning disable CS0162 // Unreachable code detected
            return 0;
#pragma warning restore CS0162
        }).Attempt();

        var result = await io.RunAsync();
        Assert.True(result.IsFailure);
    }

    #endregion

    #region OrElse

    [Fact]
    public async Task OrElse_ReturnsFallbackOnException()
    {
        var io = IOAsync<int>.Of(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException();
#pragma warning disable CS0162 // Unreachable code detected
                return 0;
#pragma warning restore CS0162
            })
            .OrElse(IOAsync<int>.Return(99));

        var result = await io.RunAsync();
        Assert.Equal(99, result);
    }

    #endregion

    #region LINQ Support

    [Fact]
    public async Task LinqQuery_SelectWorks()
    {
        var io = from x in IOAsync<int>.Return(21)
                 select x * 2;

        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task LinqQuery_SelectManyWorks()
    {
        var io = from x in IOAsync<int>.Return(10)
                 from y in IOAsync<int>.Return(5)
                 select x + y;

        var result = await io.RunAsync();
        Assert.Equal(15, result);
    }

    #endregion

    #region Extensions

    [Fact]
    public async Task Flatten_UnwrapsNestedAsyncIO()
    {
        var nested = IOAsync<IOAsync<int>>.Return(IOAsync<int>.Return(42));
        var flattened = nested.Flatten();
        var result = await flattened.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Sequence_ExecutesAllAsyncIOs()
    {
        var ios = new[]
        {
            IOAsync<int>.Return(1),
            IOAsync<int>.Return(2),
            IOAsync<int>.Return(3)
        };

        var sequenced = ios.Sequence();
        var results = await sequenced.RunAsync();
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    [Fact]
    public async Task Traverse_MapsAndSequences()
    {
        var numbers = new[] { 1, 2, 3 };
        var io = numbers.Traverse(x => IOAsync<int>.Return(x * 2));

        var results = await io.RunAsync();
        Assert.Equal(new[] { 2, 4, 6 }, results);
    }

    #endregion

    #region Static Helpers

    [Fact]
    public async Task IOAsync_Of_CreatesAsyncIO()
    {
        var io = IOAsync.Of(async () =>
        {
            await Task.Delay(1);
            return 42;
        });
        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task IOAsync_Pure_CreatesPureAsyncIO()
    {
        var io = IOAsync.Return(42);
        var result = await io.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task IOAsync_FromIO_ConvertsFromSyncIO()
    {
        var syncIo = IO<int>.Return(42);
        var asyncIo = IOAsync.FromIO(syncIo);
        var result = await asyncIo.RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task IOAsync_Execute_ExecutesAsyncAction()
    {
        var executed = false;
        var io = IOAsync.Execute(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        await io.RunAsync();
        Assert.True(executed);
    }

    [Fact]
    public async Task IOAsync_Delay_DelaysExecution()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var io = IOAsync.Delay(TimeSpan.FromMilliseconds(50));
        await io.RunAsync();
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds >= 40); // Allow some tolerance
    }

    [Fact]
    public async Task IOAsync_Parallel_TwoIOs()
    {
        var io1 = IOAsync<int>.Return(1);
        var io2 = IOAsync<int>.Return(2);

        var result = await IOAsync.Parallel(io1, io2).RunAsync();
        Assert.Equal((1, 2), result);
    }

    [Fact]
    public async Task IOAsync_Parallel_Collection()
    {
        var ios = new[] { IOAsync<int>.Return(1), IOAsync<int>.Return(2), IOAsync<int>.Return(3) };
        var result = await IOAsync.Parallel(ios).RunAsync();
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task IOAsync_Race_ReturnsFirstCompleted()
    {
        var io1 = IOAsync<int>.Of(async () =>
        {
            await Task.Delay(100);
            return 1;
        });
        var io2 = IOAsync<int>.Return(2); // This should complete first

        var result = await IOAsync.Race(io1, io2).RunAsync();
        Assert.Equal(2, result);
    }

    #endregion
}
