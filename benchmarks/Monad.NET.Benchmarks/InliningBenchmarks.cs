using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks to verify AggressiveInlining effectiveness.
/// These benchmarks compare tight loops with inline-able operations
/// to verify the JIT is properly inlining marked methods.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class InliningBenchmarks
{
    private const int Iterations = 10_000;
    
    private Option<int> _some;
    private Result<int, string> _ok;
    private Validation<int, string> _valid;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option<int>.Some(42);
        _ok = Result<int, string>.Ok(42);
        _valid = Validation<int, string>.Valid(42);
    }

    #region Option Inlining Tests

    [Benchmark(Baseline = true)]
    public int Option_IsSome_TightLoop()
    {
        var sum = 0;
        var opt = _some;
        for (var i = 0; i < Iterations; i++)
        {
            if (opt.IsSome)
                sum += i;
        }
        return sum;
    }

    [Benchmark]
    public int Option_Match_TightLoop()
    {
        var sum = 0;
        var opt = _some;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.Match(v => v, () => 0);
        }
        return sum;
    }

    [Benchmark]
    public int Option_Map_TightLoop()
    {
        var result = _some;
        for (var i = 0; i < Iterations; i++)
        {
            result = result.Map(x => x + 1);
        }
        return result.GetValue();
    }

    [Benchmark]
    public int Option_UnwrapOr_TightLoop()
    {
        var sum = 0;
        var opt = _some;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.GetValueOr(0);
        }
        return sum;
    }

    [Benchmark]
    public Option<int> Option_DefaultIfNone_TightLoop()
    {
        var opt = Option<int>.None();
        for (var i = 0; i < Iterations; i++)
        {
            opt = opt.DefaultIfNone(42);
        }
        return opt;
    }

    #endregion

    #region Result Inlining Tests

    [Benchmark]
    public int Result_IsOk_TightLoop()
    {
        var sum = 0;
        var res = _ok;
        for (var i = 0; i < Iterations; i++)
        {
            if (res.IsOk)
                sum += i;
        }
        return sum;
    }

    [Benchmark]
    public int Result_Match_TightLoop()
    {
        var sum = 0;
        var res = _ok;
        for (var i = 0; i < Iterations; i++)
        {
            sum += res.Match(v => v, _ => 0);
        }
        return sum;
    }

    [Benchmark]
    public int Result_Map_TightLoop()
    {
        var result = _ok;
        for (var i = 0; i < Iterations; i++)
        {
            result = result.Map(x => x + 1);
        }
        return result.GetValue();
    }

    [Benchmark]
    public Result<string, int> Result_BiMap_TightLoop()
    {
        var result = _ok;
        Result<string, int> mapped = Result<string, int>.Ok("0");
        for (var i = 0; i < Iterations; i++)
        {
            mapped = result.BiMap(x => x.ToString(), e => e.Length);
        }
        return mapped;
    }

    #endregion

    #region Validation Inlining Tests

    [Benchmark]
    public int Validation_IsValid_TightLoop()
    {
        var sum = 0;
        var val = _valid;
        for (var i = 0; i < Iterations; i++)
        {
            if (val.IsValid)
                sum += i;
        }
        return sum;
    }

    [Benchmark]
    public int Validation_Match_TightLoop()
    {
        var sum = 0;
        var val = _valid;
        for (var i = 0; i < Iterations; i++)
        {
            sum += val.Match(v => v, _ => 0);
        }
        return sum;
    }

    [Benchmark]
    public Validation<int, string> Validation_Ensure_TightLoop()
    {
        var val = _valid;
        for (var i = 0; i < Iterations; i++)
        {
            val = val.Ensure(x => x > 0, "Must be positive");
        }
        return val;
    }

    #endregion

    #region Chained Operations

    [Benchmark]
    public int Option_ChainedOperations_TightLoop()
    {
        var result = 0;
        for (var i = 0; i < Iterations; i++)
        {
            result += _some
                .Map(x => x + i)
                .Filter(x => x > 0)
                .Map(x => x * 2)
                .GetValueOr(0);
        }
        return result;
    }

    [Benchmark]
    public int Result_ChainedOperations_TightLoop()
    {
        var result = 0;
        for (var i = 0; i < Iterations; i++)
        {
            result += _ok
                .Map(x => x + i)
                .Map(x => x * 2)
                .GetValueOr(0);
        }
        return result;
    }

    [Benchmark]
    public int Validation_ChainedOperations_TightLoop()
    {
        var result = 0;
        for (var i = 0; i < Iterations; i++)
        {
            result += _valid
                .Ensure(x => x >= 0, "Must be non-negative")
                .Ensure(x => x < 1000, "Must be less than 1000")
                .Map(x => x + i)
                .GetValueOr(0);
        }
        return result;
    }

    #endregion
}

