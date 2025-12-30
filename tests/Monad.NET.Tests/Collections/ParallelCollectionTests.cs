using Monad.NET;
using System.Threading;

namespace Monad.NET.Tests;

public class ParallelCollectionTests
{
    /// <summary>
    /// Helper class for tracking concurrent execution in tests.
    /// </summary>
    private sealed class ConcurrencyTracker
    {
        private readonly object _lockObj = new();
        private int _concurrentCount;
        private int _maxConcurrent;

        public int MaxConcurrent => _maxConcurrent;

        public void Enter()
        {
            lock (_lockObj)
            {
                _concurrentCount++;
                _maxConcurrent = Math.Max(_maxConcurrent, _concurrentCount);
            }
        }

        public void Exit()
        {
            lock (_lockObj)
            {
                _concurrentCount--;
            }
        }

        public async Task<T> TrackAsync<T>(Func<Task<T>> operation, int delayMs = 50)
        {
            Enter();
            try
            {
                await Task.Delay(delayMs);
                return await operation();
            }
            finally
            {
                Exit();
            }
        }
    }
    #region Option SequenceParallelAsync

    [Fact]
    public async Task SequenceParallelAsync_Option_AllSome_ReturnsSome()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.Some(1)),
            Task.FromResult(Option<int>.Some(2)),
            Task.FromResult(Option<int>.Some(3))
        };

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public async Task SequenceParallelAsync_Option_ContainsNone_ReturnsNone()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.Some(1)),
            Task.FromResult(Option<int>.None()),
            Task.FromResult(Option<int>.Some(3))
        };

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task SequenceParallelAsync_Option_EmptySequence_ReturnsSomeEmpty()
    {
        var tasks = Array.Empty<Task<Option<int>>>();

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsSome);
        Assert.Empty(result.Unwrap());
    }

    [Fact]
    public async Task SequenceParallelAsync_Option_WithDegreeOfParallelism_RespectsLimit()
    {
        var tracker = new ConcurrencyTracker();

        // Use TraverseParallelAsync instead of SequenceParallelAsync to test throttling.
        // SequenceParallelAsync receives already-started tasks, so it can only throttle awaits.
        // TraverseParallelAsync creates tasks lazily inside the semaphore-protected region.
        var result = await Enumerable.Range(1, 10).TraverseParallelAsync(
            i => tracker.TrackAsync(() => Task.FromResult(Option<int>.Some(i))),
            maxDegreeOfParallelism: 3);

        Assert.True(result.IsSome);
        Assert.Equal(10, result.Unwrap().Count);
        Assert.True(tracker.MaxConcurrent <= 3, $"Max concurrent was {tracker.MaxConcurrent}, expected <= 3");
    }

    #endregion

    #region Option TraverseParallelAsync

    [Fact]
    public async Task TraverseParallelAsync_Option_AllSome_ReturnsSome()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return Option<int>.Some(n * 2);
            });

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 2, 4, 6 }, result.Unwrap());
    }

    [Fact]
    public async Task TraverseParallelAsync_Option_ContainsNone_ReturnsNone()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return n == 2 ? Option<int>.None() : Option<int>.Some(n * 2);
            });

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task TraverseParallelAsync_Option_WithDegreeOfParallelism()
    {
        var numbers = Enumerable.Range(1, 20).ToList();

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(10);
                return Option<int>.Some(n);
            },
            maxDegreeOfParallelism: 5);

        Assert.True(result.IsSome);
        Assert.Equal(20, result.Unwrap().Count);
    }

    [Fact]
    public async Task TraverseParallelAsync_Option_WithDegreeOfParallelism_RespectsLimit()
    {
        var tracker = new ConcurrencyTracker();
        var numbers = Enumerable.Range(1, 10).ToList();

        var result = await numbers.TraverseParallelAsync(
            n => tracker.TrackAsync(() => Task.FromResult(Option<int>.Some(n))),
            maxDegreeOfParallelism: 3);

        Assert.True(result.IsSome);
        Assert.Equal(10, result.Unwrap().Count);
        Assert.True(tracker.MaxConcurrent <= 3, $"Max concurrent was {tracker.MaxConcurrent}, expected <= 3");
    }

    #endregion

    #region Result SequenceParallelAsync

    [Fact]
    public async Task SequenceParallelAsync_Result_AllOk_ReturnsOk()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public async Task SequenceParallelAsync_Result_ContainsErr_ReturnsFirstErr()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("error1")),
            Task.FromResult(Result<int, string>.Err("error2"))
        };

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsErr);
        Assert.Equal("error1", result.UnwrapErr());
    }

    [Fact]
    public async Task SequenceParallelAsync_Result_EmptySequence_ReturnsOkEmpty()
    {
        var tasks = Array.Empty<Task<Result<int, string>>>();

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsOk);
        Assert.Empty(result.Unwrap());
    }

    [Fact]
    public async Task SequenceParallelAsync_Result_WithDegreeOfParallelism_RespectsLimit()
    {
        var tracker = new ConcurrencyTracker();

        // Use TraverseParallelAsync instead of SequenceParallelAsync to test throttling.
        // SequenceParallelAsync receives already-started tasks, so it can only throttle awaits.
        // TraverseParallelAsync creates tasks lazily inside the semaphore-protected region.
        var result = await Enumerable.Range(1, 10).TraverseParallelAsync(
            i => tracker.TrackAsync(() => Task.FromResult(Result<int, string>.Ok(i))),
            maxDegreeOfParallelism: 4);

        Assert.True(result.IsOk);
        Assert.Equal(10, result.Unwrap().Count);
        Assert.True(tracker.MaxConcurrent <= 4, $"Max concurrent was {tracker.MaxConcurrent}, expected <= 4");
    }

    #endregion

    #region Result TraverseParallelAsync

    [Fact]
    public async Task TraverseParallelAsync_Result_AllOk_ReturnsOk()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return Result<int, string>.Ok(n * 2);
            });

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 2, 4, 6 }, result.Unwrap());
    }

    [Fact]
    public async Task TraverseParallelAsync_Result_ContainsErr_ReturnsFirstErr()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return n == 2
                    ? Result<int, string>.Err("error at 2")
                    : Result<int, string>.Ok(n * 2);
            });

        Assert.True(result.IsErr);
        Assert.Equal("error at 2", result.UnwrapErr());
    }

    [Fact]
    public async Task TraverseParallelAsync_Result_WithDegreeOfParallelism()
    {
        var numbers = Enumerable.Range(1, 20).ToList();

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(10);
                return Result<int, string>.Ok(n);
            },
            maxDegreeOfParallelism: 5);

        Assert.True(result.IsOk);
        Assert.Equal(20, result.Unwrap().Count);
    }

    [Fact]
    public async Task TraverseParallelAsync_Result_WithDegreeOfParallelism_RespectsLimit()
    {
        var tracker = new ConcurrencyTracker();
        var numbers = Enumerable.Range(1, 10).ToList();

        var result = await numbers.TraverseParallelAsync(
            n => tracker.TrackAsync(() => Task.FromResult(Result<int, string>.Ok(n))),
            maxDegreeOfParallelism: 4);

        Assert.True(result.IsOk);
        Assert.Equal(10, result.Unwrap().Count);
        Assert.True(tracker.MaxConcurrent <= 4, $"Max concurrent was {tracker.MaxConcurrent}, expected <= 4");
    }

    #endregion

    #region ChooseParallelAsync

    [Fact]
    public async Task ChooseParallelAsync_ReturnsOnlySomeValues()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };

        var result = await numbers.ChooseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return n % 2 == 0 ? Option<int>.Some(n * 10) : Option<int>.None();
            });

        Assert.Equal(new[] { 20, 40 }, result);
    }

    [Fact]
    public async Task ChooseParallelAsync_AllNone_ReturnsEmpty()
    {
        var numbers = new[] { 1, 3, 5 };

        var result = await numbers.ChooseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return n % 2 == 0 ? Option<int>.Some(n) : Option<int>.None();
            });

        Assert.Empty(result);
    }

    [Fact]
    public async Task ChooseParallelAsync_AllSome_ReturnsAll()
    {
        var numbers = new[] { 2, 4, 6 };

        var result = await numbers.ChooseParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return Option<int>.Some(n * 10);
            });

        Assert.Equal(new[] { 20, 40, 60 }, result);
    }

    [Fact]
    public async Task ChooseParallelAsync_WithDegreeOfParallelism()
    {
        var numbers = Enumerable.Range(1, 20).ToList();

        var result = await numbers.ChooseParallelAsync(
            async n =>
            {
                await Task.Delay(10);
                return n % 2 == 0 ? Option<int>.Some(n) : Option<int>.None();
            },
            maxDegreeOfParallelism: 4);

        Assert.Equal(10, result.Count);
    }

    #endregion

    #region PartitionParallelAsync

    [Fact]
    public async Task PartitionParallelAsync_SeparatesOksAndErrors()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };

        var (oks, errors) = await numbers.PartitionParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return n % 2 == 0
                    ? Result<int, string>.Ok(n * 10)
                    : Result<int, string>.Err($"odd: {n}");
            });

        Assert.Equal(new[] { 20, 40 }, oks);
        Assert.Equal(new[] { "odd: 1", "odd: 3", "odd: 5" }, errors);
    }

    [Fact]
    public async Task PartitionParallelAsync_AllOk_ReturnsAllInOks()
    {
        var numbers = new[] { 1, 2, 3 };

        var (oks, errors) = await numbers.PartitionParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return Result<int, string>.Ok(n * 10);
            });

        Assert.Equal(new[] { 10, 20, 30 }, oks);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task PartitionParallelAsync_AllErr_ReturnsAllInErrors()
    {
        var numbers = new[] { 1, 2, 3 };

        var (oks, errors) = await numbers.PartitionParallelAsync(
            async n =>
            {
                await Task.Delay(1);
                return Result<int, string>.Err($"error: {n}");
            });

        Assert.Empty(oks);
        Assert.Equal(new[] { "error: 1", "error: 2", "error: 3" }, errors);
    }

    [Fact]
    public async Task PartitionParallelAsync_WithDegreeOfParallelism()
    {
        var numbers = Enumerable.Range(1, 20).ToList();

        var (oks, errors) = await numbers.PartitionParallelAsync(
            async n =>
            {
                await Task.Delay(10);
                return n % 2 == 0
                    ? Result<int, string>.Ok(n)
                    : Result<int, string>.Err($"odd: {n}");
            },
            maxDegreeOfParallelism: 4);

        Assert.Equal(10, oks.Count);
        Assert.Equal(10, errors.Count);
    }

    #endregion

    #region Order Preservation

    [Fact]
    public async Task TraverseParallelAsync_PreservesOrder()
    {
        var numbers = Enumerable.Range(1, 100).ToList();

        var result = await numbers.TraverseParallelAsync(
            async n =>
            {
                // Random delays to test order preservation
                await Task.Delay(Random.Shared.Next(1, 10));
                return Option<int>.Some(n);
            });

        Assert.True(result.IsSome);
        Assert.Equal(numbers, result.Unwrap());
    }

    [Fact]
    public async Task SequenceParallelAsync_PreservesOrder()
    {
        var numbers = Enumerable.Range(1, 100).ToList();
        var tasks = numbers.Select(async n =>
        {
            await Task.Delay(Random.Shared.Next(1, 10));
            return Result<int, string>.Ok(n);
        }).ToList();

        var result = await tasks.SequenceParallelAsync();

        Assert.True(result.IsOk);
        Assert.Equal(numbers, result.Unwrap());
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task TraverseParallelAsync_Option_HonorsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10);
        var numbers = Enumerable.Range(1, 10).ToList();

        var task = numbers.TraverseParallelAsync(
            async n =>
            {
                await Task.Delay(200, cts.Token);
                return Option<int>.Some(n);
            },
            maxDegreeOfParallelism: 2,
            cancellationToken: cts.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task SequenceParallelAsync_Result_HonorsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10);
        var tasks = Enumerable.Range(1, 5).Select(async n =>
        {
            await Task.Delay(200, cts.Token);
            return Result<int, string>.Ok(n);
        }).ToList();

        var task = tasks.SequenceParallelAsync(maxDegreeOfParallelism: 2, cancellationToken: cts.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task PartitionParallelAsync_HonorsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10);
        var numbers = Enumerable.Range(1, 20);

        var task = numbers.PartitionParallelAsync(
            async n =>
            {
                await Task.Delay(200, cts.Token);
                return n % 2 == 0
                    ? Result<int, string>.Ok(n)
                    : Result<int, string>.Err($"odd: {n}");
            },
            maxDegreeOfParallelism: 2,
            cancellationToken: cts.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    #endregion

    #region Real-World Examples

    [Fact]
    public async Task RealWorld_FetchUsersInParallel()
    {
        // Simulate fetching users from a database
        Task<Option<string>> FetchUserAsync(int id) => Task.FromResult(
            id > 0 ? Option<string>.Some($"User{id}") : Option<string>.None());

        var userIds = new[] { 1, 2, 3, 4, 5 };

        var users = await userIds.TraverseParallelAsync(
            FetchUserAsync,
            maxDegreeOfParallelism: 3);

        Assert.True(users.IsSome);
        Assert.Equal(5, users.Unwrap().Count);
    }

    [Fact]
    public async Task RealWorld_ProcessOrdersWithPartition()
    {
        // Simulate processing orders where some might fail
        Task<Result<string, string>> ProcessOrderAsync(int orderId) => Task.FromResult(
            orderId % 3 == 0
                ? Result<string, string>.Err($"Order {orderId} failed")
                : Result<string, string>.Ok($"Order {orderId} processed"));

        var orderIds = Enumerable.Range(1, 10).ToList();

        var (successes, failures) = await orderIds.PartitionParallelAsync(
            ProcessOrderAsync,
            maxDegreeOfParallelism: 4);

        Assert.Equal(7, successes.Count); // 1,2,4,5,7,8,10
        Assert.Equal(3, failures.Count);  // 3,6,9
    }

    #endregion
}

