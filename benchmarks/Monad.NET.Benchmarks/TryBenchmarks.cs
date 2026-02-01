using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for Try&lt;T&gt; operations.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class TryBenchmarks
{
    private Try<int> _success;
    private Try<int> _failure;

    [GlobalSetup]
    public void Setup()
    {
        _success = Try<int>.Success(42);
        _failure = Try<int>.Failure(new InvalidOperationException("error"));
    }

    [Benchmark(Baseline = true)]
    public Try<int> CreateSuccess()
    {
        return Try<int>.Success(42);
    }

    [Benchmark]
    public Try<int> CreateFailure()
    {
        return Try<int>.Failure(new InvalidOperationException("error"));
    }

    [Benchmark]
    public Try<int> Of_Success()
    {
        return Try<int>.Of(() => 42);
    }

    [Benchmark]
    public Try<int> Of_Failure()
    {
        return Try<int>.Of(() => throw new InvalidOperationException("error"));
    }

    [Benchmark]
    public Try<int> Map_Success()
    {
        return _success.Map(x => x * 2);
    }

    [Benchmark]
    public Try<int> Map_Failure()
    {
        return _failure.Map(x => x * 2);
    }

    [Benchmark]
    public Try<int> Bind_Success()
    {
        return _success.Bind(x => Try<int>.Success(x * 2));
    }

    [Benchmark]
    public Try<int> Bind_Failure()
    {
        return _failure.Bind(x => Try<int>.Success(x * 2));
    }

    [Benchmark]
    public int Match_Success()
    {
        return _success.Match(x => x, _ => 0);
    }

    [Benchmark]
    public int Match_Failure()
    {
        return _failure.Match(x => x, _ => 0);
    }

    [Benchmark]
    public int GetValueOr_Success()
    {
        return _success.GetValueOr(0);
    }

    [Benchmark]
    public int GetValueOr_Failure()
    {
        return _failure.GetValueOr(0);
    }
}
