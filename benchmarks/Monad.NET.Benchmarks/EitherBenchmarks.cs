using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for Either&lt;L, R&gt; operations.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EitherBenchmarks
{
    private Either<string, int> _right;
    private Either<string, int> _left;

    [GlobalSetup]
    public void Setup()
    {
        _right = Either<string, int>.Right(42);
        _left = Either<string, int>.Left("error");
    }

    [Benchmark(Baseline = true)]
    public Either<string, int> CreateRight()
    {
        return Either<string, int>.Right(42);
    }

    [Benchmark]
    public Either<string, int> CreateLeft()
    {
        return Either<string, int>.Left("error");
    }

    [Benchmark]
    public Either<string, int> MapRight_Right()
    {
        return _right.MapRight(x => x * 2);
    }

    [Benchmark]
    public Either<string, int> MapRight_Left()
    {
        return _left.MapRight(x => x * 2);
    }

    [Benchmark]
    public Either<string, int> MapLeft_Right()
    {
        return _right.MapLeft(e => e.ToUpper());
    }

    [Benchmark]
    public Either<string, int> MapLeft_Left()
    {
        return _left.MapLeft(e => e.ToUpper());
    }

    [Benchmark]
    public int Match_Right()
    {
        return _right.Match(_ => 0, x => x);
    }

    [Benchmark]
    public int Match_Left()
    {
        return _left.Match(_ => 0, x => x);
    }

    [Benchmark]
    public Either<string, int> AndThen_Right()
    {
        return _right.AndThen(x => Either<string, int>.Right(x * 2));
    }

    [Benchmark]
    public Either<string, int> AndThen_Left()
    {
        return _left.AndThen(x => Either<string, int>.Right(x * 2));
    }
}
