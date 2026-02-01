using System.Collections.Concurrent;
using Monad.NET;

namespace Monad.NET.Tests.Concurrency;

/// <summary>
/// Tests verifying thread-safety of monad types under concurrent access.
/// </summary>
public class ThreadSafetyTests
{
    private const int ConcurrentOperations = 1000;
    private const int ThreadCount = 10;

    #region Option Thread Safety

    [Fact]
    public async Task Option_ConcurrentReads_AreThreadSafe()
    {
        var option = Option<int>.Some(42);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var value = option.Match(v => v, () => -1);
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(42, v));
    }

    [Fact]
    public async Task Option_ConcurrentMap_IsThreadSafe()
    {
        var option = Option<int>.Some(10);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var mapped = option.Map(x => x * 2);
                    var value = mapped.Match(v => v, () => -1);
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(20, v));
    }

    #endregion

    #region Result Thread Safety

    [Fact]
    public async Task Result_ConcurrentReads_AreThreadSafe()
    {
        var result = Result<int, string>.Ok(42);
        var values = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var value = result.Match(v => v, _ => -1);
                    values.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(values, v => Assert.Equal(42, v));
    }

    [Fact]
    public async Task Result_ConcurrentMapAndBind_AreThreadSafe()
    {
        var result = Result<int, string>.Ok(5);
        var values = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var mapped = result
                        .Map(x => x * 2)
                        .Bind(x => Result<int, string>.Ok(x + 1));
                    var value = mapped.Match(v => v, _ => -1);
                    values.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(values, v => Assert.Equal(11, v));
    }

    #endregion

    #region Validation Thread Safety

    [Fact]
    public async Task Validation_ConcurrentApply_IsThreadSafe()
    {
        var v1 = Validation<int, string>.Valid(10);
        var v2 = Validation<int, string>.Valid(20);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var combined = v1.Apply(v2, (a, b) => a + b);
                    var value = combined.Match(v => v, _ => -1);
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(30, v));
    }

    [Fact]
    public async Task Validation_ConcurrentErrorAccumulation_IsThreadSafe()
    {
        var v1 = Validation<int, string>.Invalid("error1");
        var v2 = Validation<int, string>.Invalid("error2");
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var combined = v1.Apply(v2, (a, b) => a + b);
                    var errorCount = combined.Match(_ => 0, errors => errors.Length);
                    results.Add(errorCount);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, c => Assert.Equal(2, c));
    }

    #endregion

    #region Try Thread Safety

    [Fact]
    public async Task Try_ConcurrentExecution_IsThreadSafe()
    {
        var tryComputation = Try<int>.Of(() => 42);

        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var value = tryComputation.Match(v => v, _ => -1);
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(42, v));
    }

    #endregion

    #region NonEmptyList Thread Safety

    [Fact]
    public async Task NonEmptyList_ConcurrentReads_AreThreadSafe()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var head = list.Head;
                    var count = list.Count;
                    results.Add(head);
                    results.Add(count);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.Contains(1, results);
        Assert.Contains(5, results);
    }

    [Fact]
    public async Task NonEmptyList_ConcurrentMap_IsThreadSafe()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var mapped = list.Map(x => x * 10);
                    results.Add(mapped.Head);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(10, v));
    }

    #endregion

    #region Reader Thread Safety

    [Fact]
    public async Task Reader_ConcurrentRun_IsThreadSafe()
    {
        var reader = Reader<int, int>.From(env => env * 2);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var value = reader.Run(21);
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(42, v));
    }

    #endregion

    #region IO Thread Safety

    [Fact]
    public async Task IO_ConcurrentRun_IsThreadSafe()
    {
        var io = IO<int>.Of(() => 42);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var value = io.Run();
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(42, v));
    }

    #endregion

    #region RemoteData Thread Safety

    [Fact]
    public async Task RemoteData_ConcurrentReads_AreThreadSafe()
    {
        var data = RemoteData<int, string>.Success(42);
        var results = new ConcurrentBag<int>();
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    var value = data.Match(
                        notAskedFunc: () => -1,
                        loadingFunc: () => -2,
                        successFunc: v => v,
                        failureFunc: _ => -3);
                    results.Add(value);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, v => Assert.Equal(42, v));
    }

    #endregion

    #region Cross-Type Concurrent Operations

    [Fact]
    public async Task MixedTypes_ConcurrentOperations_AreThreadSafe()
    {
        var option = Option<int>.Some(10);
        var result = Result<int, string>.Ok(20);
        var validation = Validation<int, string>.Valid(30);
        var exceptions = new ConcurrentBag<Exception>();

        await RunConcurrently(() =>
        {
            try
            {
                for (int i = 0; i < ConcurrentOperations; i++)
                {
                    // Concurrent operations on different monad types
                    var o = option.Map(x => x + 1);
                    var r = result.Map(x => x + 1);
                    var v = validation.Map(x => x + 1);

                    Assert.Equal(11, o.Match(x => x, () => -1));
                    Assert.Equal(21, r.Match(x => x, _ => -1));
                    Assert.Equal(31, v.Match(x => x, _ => -1));
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
    }

    #endregion

    #region Helper Methods

    private static async Task RunConcurrently(Action action)
    {
        var tasks = Enumerable.Range(0, ThreadCount)
            .Select(_ => Task.Run(action))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    #endregion
}
