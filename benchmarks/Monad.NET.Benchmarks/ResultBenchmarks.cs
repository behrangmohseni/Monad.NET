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
        _err = Result<int, string>.Error("error");
    }

    [Benchmark(Baseline = true)]
    public Result<int, string> CreateOk()
    {
        return Result<int, string>.Ok(42);
    }

    [Benchmark]
    public Result<int, string> CreateErr()
    {
        return Result<int, string>.Error("error");
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
    public Result<int, string> Bind_Ok()
    {
        return _ok.Bind(x => Result<int, string>.Ok(x * 2));
    }

    [Benchmark]
    public Result<int, string> Bind_Err()
    {
        return _err.Bind(x => Result<int, string>.Ok(x * 2));
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
    public int GetValueOr_Ok()
    {
        return _ok.GetValueOr(0);
    }

    [Benchmark]
    public int GetValueOr_Err()
    {
        return _err.GetValueOr(0);
    }

    [Benchmark]
    public Result<int, string> MapError_Ok()
    {
        return _ok.MapError(e => e.ToUpper());
    }

    [Benchmark]
    public Result<int, string> MapError_Err()
    {
        return _err.MapError(e => e.ToUpper());
    }

    [Benchmark]
    public Result<int, string> ChainedOperations()
    {
        return _ok
            .Map(x => x * 2)
            .Bind(x => Result<int, string>.Ok(x + 1))
            .Map(x => x * 3);
    }
}

