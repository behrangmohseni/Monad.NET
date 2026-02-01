using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for Option&lt;T&gt; operations.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class OptionBenchmarks
{
    private Option<int> _some;
    private Option<int> _none;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option<int>.Some(42);
        _none = Option<int>.None();
    }

    [Benchmark(Baseline = true)]
    public Option<int> CreateSome()
    {
        return Option<int>.Some(42);
    }

    [Benchmark]
    public Option<int> CreateNone()
    {
        return Option<int>.None();
    }

    [Benchmark]
    public Option<int> Map_Some()
    {
        return _some.Map(x => x * 2);
    }

    [Benchmark]
    public Option<int> Map_None()
    {
        return _none.Map(x => x * 2);
    }

    [Benchmark]
    public Option<int> Bind_Some()
    {
        return _some.Bind(x => Option<int>.Some(x * 2));
    }

    [Benchmark]
    public Option<int> Bind_None()
    {
        return _none.Bind(x => Option<int>.Some(x * 2));
    }

    [Benchmark]
    public int Match_Some()
    {
        return _some.Match(x => x, () => 0);
    }

    [Benchmark]
    public int Match_None()
    {
        return _none.Match(x => x, () => 0);
    }

    [Benchmark]
    public int GetValueOr_Some()
    {
        return _some.GetValueOr(0);
    }

    [Benchmark]
    public int GetValueOr_None()
    {
        return _none.GetValueOr(0);
    }

    [Benchmark]
    public Option<int> Filter_Some_Pass()
    {
        return _some.Filter(x => x > 0);
    }

    [Benchmark]
    public Option<int> Filter_Some_Fail()
    {
        return _some.Filter(x => x < 0);
    }

    [Benchmark]
    public Option<int> ChainedOperations()
    {
        return _some
            .Map(x => x * 2)
            .Filter(x => x > 0)
            .Map(x => x + 1)
            .Bind(x => Option<int>.Some(x * 3));
    }
}

