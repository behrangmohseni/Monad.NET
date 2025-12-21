using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for Result&lt;T, E&gt; operations.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ResultBenchmarks
{
    private Result<int, string> _ok;
    private Result<int, string> _err;

    [GlobalSetup]
    public void Setup()
    {
        _ok = Result<int, string>.Ok(42);
        _err = Result<int, string>.Err("error");
    }

    [Benchmark(Baseline = true)]
    public Result<int, string> CreateOk()
    {
        return Result<int, string>.Ok(42);
    }

    [Benchmark]
    public Result<int, string> CreateErr()
    {
        return Result<int, string>.Err("error");
    }

    [Benchmark]
    public Result<int, string> Map_Ok()
    {
        return _ok.Map(x => x * 2);
    }

    [Benchmark]
    public Result<int, string> Map_Err()
    {
        return _err.Map(x => x * 2);
    }

    [Benchmark]
    public Result<int, string> AndThen_Ok()
    {
        return _ok.AndThen(x => Result<int, string>.Ok(x * 2));
    }

    [Benchmark]
    public Result<int, string> AndThen_Err()
    {
        return _err.AndThen(x => Result<int, string>.Ok(x * 2));
    }

    [Benchmark]
    public int Match_Ok()
    {
        return _ok.Match(x => x, _ => 0);
    }

    [Benchmark]
    public int Match_Err()
    {
        return _err.Match(x => x, _ => 0);
    }

    [Benchmark]
    public int UnwrapOr_Ok()
    {
        return _ok.UnwrapOr(0);
    }

    [Benchmark]
    public int UnwrapOr_Err()
    {
        return _err.UnwrapOr(0);
    }

    [Benchmark]
    public Result<int, string> MapErr_Ok()
    {
        return _ok.MapErr(e => e.ToUpper());
    }

    [Benchmark]
    public Result<int, string> MapErr_Err()
    {
        return _err.MapErr(e => e.ToUpper());
    }

    [Benchmark]
    public Result<int, string> ChainedOperations()
    {
        return _ok
            .Map(x => x * 2)
            .AndThen(x => Result<int, string>.Ok(x + 1))
            .Map(x => x * 3);
    }
}

