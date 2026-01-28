using Monad.NET;

namespace Monad.NET.Tests;

public class ReaderAsyncTests
{
    private class TestEnvironment
    {
        public string ConnectionString { get; set; } = "Server=localhost";
        public int Timeout { get; set; } = 30;
        public bool Debug { get; set; } = false;
    }

    #region Factory Methods

    [Fact]
    public async Task Pure_CreatesReaderAsyncWithConstantValue()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(42);
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task From_CreatesReaderAsyncFromAsyncFunction()
    {
        var reader = ReaderAsync<TestEnvironment, int>.From(async env =>
        {
            await Task.Delay(1);
            return env.Timeout;
        });
        var env = new TestEnvironment { Timeout = 60 };

        var result = await reader.RunAsync(env);

        Assert.Equal(60, result);
    }

    [Fact]
    public async Task FromReader_CreatesReaderAsyncFromSynchronousReader()
    {
        var syncReader = Reader<TestEnvironment, int>.Asks(env => env.Timeout);
        var asyncReader = ReaderAsync<TestEnvironment, int>.FromReader(syncReader);
        var env = new TestEnvironment { Timeout = 45 };

        var result = await asyncReader.RunAsync(env);

        Assert.Equal(45, result);
    }

    [Fact]
    public async Task Ask_ReturnsTheEnvironment()
    {
        var reader = ReaderAsync<TestEnvironment, TestEnvironment>.Ask();
        var env = new TestEnvironment { Timeout = 60 };

        var result = await reader.RunAsync(env);

        Assert.Equal(60, result.Timeout);
    }

    [Fact]
    public async Task Asks_ExtractsValueFromEnvironmentSynchronously()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Asks(env => env.Timeout);
        var env = new TestEnvironment { Timeout = 45 };

        var result = await reader.RunAsync(env);

        Assert.Equal(45, result);
    }

    [Fact]
    public async Task AsksAsync_ExtractsValueFromEnvironmentAsynchronously()
    {
        var reader = ReaderAsync<TestEnvironment, int>.AsksAsync(async env =>
        {
            await Task.Delay(1);
            return env.Timeout * 2;
        });
        var env = new TestEnvironment { Timeout = 20 };

        var result = await reader.RunAsync(env);

        Assert.Equal(40, result);
    }

    #endregion

    #region Map

    [Fact]
    public async Task Map_TransformsResultSynchronously()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(42);
        var mapped = reader.Map(x => x * 2);
        var env = new TestEnvironment();

        var result = await mapped.RunAsync(env);

        Assert.Equal(84, result);
    }

    [Fact]
    public async Task MapAsync_TransformsResultAsynchronously()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(42);
        var mapped = reader.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });
        var env = new TestEnvironment();

        var result = await mapped.RunAsync(env);

        Assert.Equal(84, result);
    }

    #endregion

    #region FlatMap / Bind / AndThen

    [Fact]
    public async Task FlatMap_ChainsReaderAsyncComputations()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = reader1.Bind(timeout =>
            ReaderAsync<TestEnvironment, string>.Return($"Timeout: {timeout}")
        );
        var env = new TestEnvironment { Timeout = 30 };

        var result = await reader2.RunAsync(env);

        Assert.Equal("Timeout: 30", result);
    }

    [Fact]
    public async Task BindAsync_ChainsWithAsyncBinder()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Return(10);
        var reader2 = reader1.BindAsync(async x =>
        {
            await Task.Delay(1);
            return ReaderAsync<TestEnvironment, int>.Return(x * 2);
        });
        var env = new TestEnvironment();

        var result = await reader2.RunAsync(env);

        Assert.Equal(20, result);
    }

    [Fact]
    public async Task AndThen_IsSameAsFlatMap()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Return(10);
        var reader2 = reader1.Bind(x =>
            ReaderAsync<TestEnvironment, int>.Return(x + 5)
        );
        var env = new TestEnvironment();

        var result = await reader2.RunAsync(env);

        Assert.Equal(15, result);
    }

    [Fact]
    public async Task Bind_IsSameAsFlatMap()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Return(10);
        var reader2 = reader1.Bind(x =>
            ReaderAsync<TestEnvironment, int>.Return(x + 5)
        );
        var env = new TestEnvironment();

        var result = await reader2.RunAsync(env);

        Assert.Equal(15, result);
    }

    #endregion

    #region Tap

    [Fact]
    public async Task Tap_ExecutesActionWithoutModifyingResult()
    {
        var sideEffect = 0;
        var reader = ReaderAsync<TestEnvironment, int>.Return(42)
            .Tap(x => sideEffect = x);
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(42, result);
        Assert.Equal(42, sideEffect);
    }

    [Fact]
    public async Task TapAsync_ExecutesAsyncActionWithoutModifyingResult()
    {
        var sideEffect = 0;
        var reader = ReaderAsync<TestEnvironment, int>.Return(42)
            .TapAsync(async x =>
            {
                await Task.Delay(1);
                sideEffect = x;
            });
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(42, result);
        Assert.Equal(42, sideEffect);
    }

    [Fact]
    public async Task TapEnv_ExecutesActionWithEnvironment()
    {
        var capturedTimeout = 0;
        var reader = ReaderAsync<TestEnvironment, int>.Return(42)
            .TapEnv(env => capturedTimeout = env.Timeout);
        var env = new TestEnvironment { Timeout = 100 };

        var result = await reader.RunAsync(env);

        Assert.Equal(42, result);
        Assert.Equal(100, capturedTimeout);
    }

    [Fact]
    public async Task TapEnvAsync_ExecutesAsyncActionWithEnvironment()
    {
        var capturedTimeout = 0;
        var reader = ReaderAsync<TestEnvironment, int>.Return(42)
            .TapEnvAsync(async env =>
            {
                await Task.Delay(1);
                capturedTimeout = env.Timeout;
            });
        var env = new TestEnvironment { Timeout = 100 };

        var result = await reader.RunAsync(env);

        Assert.Equal(42, result);
        Assert.Equal(100, capturedTimeout);
    }

    #endregion

    #region WithEnvironment

    [Fact]
    public async Task WithEnvironment_TransformsEnvironmentType()
    {
        var reader = ReaderAsync<int, string>.From(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });
        var transformed = reader.WithEnvironment<TestEnvironment>(env => env.Timeout);
        var env = new TestEnvironment { Timeout = 42 };

        var result = await transformed.RunAsync(env);

        Assert.Equal("42", result);
    }

    [Fact]
    public async Task WithEnvironmentAsync_TransformsEnvironmentTypeAsynchronously()
    {
        var reader = ReaderAsync<int, string>.From(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });
        var transformed = reader.WithEnvironmentAsync<TestEnvironment>(async env =>
        {
            await Task.Delay(1);
            return env.Timeout;
        });
        var env = new TestEnvironment { Timeout = 42 };

        var result = await transformed.RunAsync(env);

        Assert.Equal("42", result);
    }

    #endregion

    #region Zip

    [Fact]
    public async Task Zip_CombinesTwoReaderAsyncComputations()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = ReaderAsync<TestEnvironment, string>.Asks(env => env.ConnectionString);
        var zipped = reader1.Zip(reader2, (timeout, conn) => $"{conn} with timeout {timeout}");
        var env = new TestEnvironment { Timeout = 30, ConnectionString = "Server=prod" };

        var result = await zipped.RunAsync(env);

        Assert.Equal("Server=prod with timeout 30", result);
    }

    [Fact]
    public async Task Zip_WithoutCombiner_ReturnsTuple()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = ReaderAsync<TestEnvironment, string>.Asks(env => env.ConnectionString);
        var zipped = reader1.Zip(reader2);
        var env = new TestEnvironment { Timeout = 30, ConnectionString = "Server=prod" };

        var result = await zipped.RunAsync(env);

        Assert.Equal((30, "Server=prod"), result);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task Attempt_WrapsSuccessInTry()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(42);
        var attempted = reader.Attempt();
        var env = new TestEnvironment();

        var result = await attempted.RunAsync(env);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task Attempt_WrapsExceptionInTry()
    {
        var reader = ReaderAsync<TestEnvironment, int>.From(
            _ => Task.FromException<int>(new InvalidOperationException("Test error")));
        var attempted = reader.Attempt();
        var env = new TestEnvironment();

        var result = await attempted.RunAsync(env);

        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetExceptionOrThrow());
    }

    [Fact]
    public async Task OrElse_ReturnsOriginalValueOnSuccess()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(42);
        var withFallback = reader.OrElse(ReaderAsync<TestEnvironment, int>.Return(0));
        var env = new TestEnvironment();

        var result = await withFallback.RunAsync(env);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task OrElse_ReturnsFallbackReaderOnFailure()
    {
        var reader = ReaderAsync<TestEnvironment, int>.From(
            _ => Task.FromException<int>(new InvalidOperationException()));
        var withFallback = reader.OrElse(ReaderAsync<TestEnvironment, int>.Return(100));
        var env = new TestEnvironment();

        var result = await withFallback.RunAsync(env);

        Assert.Equal(100, result);
    }

    [Fact]
    public async Task OrElse_ReturnsFallbackValueOnFailure()
    {
        var reader = ReaderAsync<TestEnvironment, int>.From(
            _ => Task.FromException<int>(new InvalidOperationException()));
        var withFallback = reader.OrElse(100);
        var env = new TestEnvironment();

        var result = await withFallback.RunAsync(env);

        Assert.Equal(100, result);
    }

    [Fact]
    public async Task Retry_RetriesOnFailure()
    {
        var attempts = 0;
        var reader = ReaderAsync<TestEnvironment, int>.From(async _ =>
        {
            await Task.Delay(1);
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException("Transient error");
            return 42;
        });
        var retrying = reader.Retry(3);
        var env = new TestEnvironment();

        var result = await retrying.RunAsync(env);

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task Retry_ThrowsAfterAllRetriesExhausted()
    {
        var reader = ReaderAsync<TestEnvironment, int>.From(
            _ => Task.FromException<int>(new InvalidOperationException("Permanent error")));
        var retrying = reader.Retry(2);
        var env = new TestEnvironment();

        await Assert.ThrowsAsync<InvalidOperationException>(() => retrying.RunAsync(env));
    }

    [Fact]
    public async Task RetryWithDelay_RetriesWithDelayBetweenAttempts()
    {
        var attempts = 0;
        var reader = ReaderAsync<TestEnvironment, int>.From(async _ =>
        {
            await Task.Delay(1);
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("Transient error");
            return 42;
        });
        var retrying = reader.RetryWithDelay(2, TimeSpan.FromMilliseconds(10));
        var env = new TestEnvironment();

        var result = await retrying.RunAsync(env);

        Assert.Equal(42, result);
        Assert.Equal(2, attempts);
    }

    #endregion

    #region LINQ Support

    [Fact]
    public async Task Linq_QuerySyntax_WorksCorrectly()
    {
        var reader = from timeout in ReaderAsync<TestEnvironment, int>.Asks(e => e.Timeout)
                     from debug in ReaderAsync<TestEnvironment, bool>.Asks(e => e.Debug)
                     select $"Timeout: {timeout}, Debug: {debug}";

        var env = new TestEnvironment { Timeout = 60, Debug = true };
        var result = await reader.RunAsync(env);

        Assert.Equal("Timeout: 60, Debug: True", result);
    }

    [Fact]
    public async Task Select_WorksLikeMap()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(10)
            .Select(x => x * 2);
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(20, result);
    }

    [Fact]
    public async Task SelectMany_WorksLikeFlatMap()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(10)
            .SelectMany(x => ReaderAsync<TestEnvironment, int>.Return(x * 2));
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(20, result);
    }

    #endregion

    #region Sequence and Traverse

    [Fact]
    public async Task Sequence_CombinesMultipleReaderAsyncs()
    {
        var readers = new[]
        {
            ReaderAsync<TestEnvironment, int>.Return(1),
            ReaderAsync<TestEnvironment, int>.Return(2),
            ReaderAsync<TestEnvironment, int>.Return(3)
        };

        var sequenced = readers.Sequence();
        var env = new TestEnvironment();
        var result = await sequenced.RunAsync(env);

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task SequenceParallel_RunsInParallel()
    {
        var readers = new[]
        {
            ReaderAsync<TestEnvironment, int>.From(async _ => { await Task.Delay(10); return 1; }),
            ReaderAsync<TestEnvironment, int>.From(async _ => { await Task.Delay(10); return 2; }),
            ReaderAsync<TestEnvironment, int>.From(async _ => { await Task.Delay(10); return 3; })
        };

        var sequenced = readers.SequenceParallel();
        var env = new TestEnvironment();
        var result = await sequenced.RunAsync(env);

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task Traverse_MapsAndSequences()
    {
        var numbers = new[] { 1, 2, 3 };
        var traversed = numbers.Traverse(n =>
            ReaderAsync<TestEnvironment, int>.Asks(env => n * env.Timeout)
        );

        var env = new TestEnvironment { Timeout = 10 };
        var result = await traversed.RunAsync(env);

        Assert.Equal(new[] { 10, 20, 30 }, result);
    }

    [Fact]
    public async Task TraverseParallel_MapsAndSequencesInParallel()
    {
        var numbers = new[] { 1, 2, 3 };
        var traversed = numbers.TraverseParallel(n =>
            ReaderAsync<TestEnvironment, int>.From(async env =>
            {
                await Task.Delay(10);
                return n * env.Timeout;
            })
        );

        var env = new TestEnvironment { Timeout = 10 };
        var result = await traversed.RunAsync(env);

        Assert.Equal(new[] { 10, 20, 30 }, result);
    }

    [Fact]
    public async Task Flatten_UnwrapsNestedReaderAsync()
    {
        var nested = ReaderAsync<TestEnvironment, ReaderAsync<TestEnvironment, int>>.Return(
            ReaderAsync<TestEnvironment, int>.Return(42));
        var flattened = nested.Flatten();
        var env = new TestEnvironment();

        var result = await flattened.RunAsync(env);

        Assert.Equal(42, result);
    }

    #endregion

    #region Static Helpers

    [Fact]
    public async Task ReaderAsyncStatic_From_CreatesReaderAsync()
    {
        var reader = ReaderAsync.From<TestEnvironment, int>(async env =>
        {
            await Task.Delay(1);
            return env.Timeout;
        });
        var env = new TestEnvironment { Timeout = 50 };

        var result = await reader.RunAsync(env);

        Assert.Equal(50, result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_FromReader_ConvertsReader()
    {
        var syncReader = Reader<TestEnvironment, int>.Return(42);
        var asyncReader = ReaderAsync.FromReader(syncReader);
        var env = new TestEnvironment();

        var result = await asyncReader.RunAsync(env);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_Pure_CreatesConstantReader()
    {
        var reader = ReaderAsync.Return<TestEnvironment, int>(42);
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_Ask_ReturnsEnvironment()
    {
        var reader = ReaderAsync.Ask<TestEnvironment>();
        var env = new TestEnvironment { Timeout = 99 };

        var result = await reader.RunAsync(env);

        Assert.Equal(99, result.Timeout);
    }

    [Fact]
    public async Task ReaderAsyncStatic_Asks_ExtractsValue()
    {
        var reader = ReaderAsync.Asks<TestEnvironment, string>(env => env.ConnectionString);
        var env = new TestEnvironment { ConnectionString = "Server=test" };

        var result = await reader.RunAsync(env);

        Assert.Equal("Server=test", result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_AsksAsync_ExtractsValueAsync()
    {
        var reader = ReaderAsync.AsksAsync<TestEnvironment, int>(async env =>
        {
            await Task.Delay(1);
            return env.Timeout;
        });
        var env = new TestEnvironment { Timeout = 75 };

        var result = await reader.RunAsync(env);

        Assert.Equal(75, result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_Parallel_RunsTwoInParallel()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = ReaderAsync<TestEnvironment, string>.Asks(env => env.ConnectionString);
        var parallel = ReaderAsync.Parallel(reader1, reader2);
        var env = new TestEnvironment { Timeout = 30, ConnectionString = "Server=test" };

        var result = await parallel.RunAsync(env);

        Assert.Equal((30, "Server=test"), result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_Parallel_RunsThreeInParallel()
    {
        var reader1 = ReaderAsync<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = ReaderAsync<TestEnvironment, string>.Asks(env => env.ConnectionString);
        var reader3 = ReaderAsync<TestEnvironment, bool>.Asks(env => env.Debug);
        var parallel = ReaderAsync.Parallel(reader1, reader2, reader3);
        var env = new TestEnvironment { Timeout = 30, ConnectionString = "Server=test", Debug = true };

        var result = await parallel.RunAsync(env);

        Assert.Equal((30, "Server=test", true), result);
    }

    [Fact]
    public async Task ReaderAsyncStatic_Parallel_RunsCollectionInParallel()
    {
        var readers = new[]
        {
            ReaderAsync<TestEnvironment, int>.Return(1),
            ReaderAsync<TestEnvironment, int>.Return(2),
            ReaderAsync<TestEnvironment, int>.Return(3)
        };
        var parallel = ReaderAsync.Parallel(readers);
        var env = new TestEnvironment();

        var result = await parallel.RunAsync(env);

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    #endregion

    #region Void

    [Fact]
    public async Task Void_ReturnsUnit()
    {
        var reader = ReaderAsync<TestEnvironment, int>.Return(42).Void();
        var env = new TestEnvironment();

        var result = await reader.RunAsync(env);

        Assert.Equal(Unit.Default, result);
    }

    #endregion

    #region Reader.ToAsync

    [Fact]
    public async Task Reader_ToAsync_ConvertsToReaderAsync()
    {
        var reader = Reader<TestEnvironment, int>.Asks(env => env.Timeout);
        var asyncReader = reader.ToAsync();
        var env = new TestEnvironment { Timeout = 42 };

        var result = await asyncReader.RunAsync(env);

        Assert.Equal(42, result);
    }

    #endregion

    #region Real-World Examples

    [Fact]
    public async Task RealWorld_AsyncDependencyInjection()
    {
        // Simulate an async service that depends on environment
        ReaderAsync<TestEnvironment, string> GetConnectionInfoAsync() =>
            from conn in ReaderAsync<TestEnvironment, string>.Asks(e => e.ConnectionString)
            from timeout in ReaderAsync<TestEnvironment, int>.Asks(e => e.Timeout)
            from validated in ReaderAsync<TestEnvironment, string>.From(async _ =>
            {
                await Task.Delay(1); // Simulate async validation
                return "validated";
            })
            select $"Connecting to {conn} with timeout {timeout}s ({validated})";

        var env = new TestEnvironment
        {
            ConnectionString = "Server=production",
            Timeout = 60
        };

        var info = await GetConnectionInfoAsync().RunAsync(env);
        Assert.Equal("Connecting to Server=production with timeout 60s (validated)", info);
    }

    [Fact]
    public async Task RealWorld_DatabaseQuerySimulation()
    {
        // Simulate async database operations
        static ReaderAsync<TestEnvironment, int> GetUserCountAsync() =>
            ReaderAsync<TestEnvironment, int>.From(async env =>
            {
                await Task.Delay(1); // Simulate DB call
                return env.Debug ? 0 : 100; // Return 0 in debug mode
            });

        static ReaderAsync<TestEnvironment, int> GetOrderCountAsync() =>
            ReaderAsync<TestEnvironment, int>.From(async env =>
            {
                await Task.Delay(1); // Simulate DB call
                return env.Timeout; // Use timeout as mock order count
            });

        var program = ReaderAsync.Parallel(GetUserCountAsync(), GetOrderCountAsync());

        var prodEnv = new TestEnvironment { Debug = false, Timeout = 50 };
        var prodResult = await program.RunAsync(prodEnv);
        Assert.Equal((100, 50), prodResult);

        var debugEnv = new TestEnvironment { Debug = true, Timeout = 10 };
        var debugResult = await program.RunAsync(debugEnv);
        Assert.Equal((0, 10), debugResult);
    }

    #endregion
}

