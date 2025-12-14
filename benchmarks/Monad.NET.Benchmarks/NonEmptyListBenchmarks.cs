using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for NonEmptyList&lt;T&gt; operations.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class NonEmptyListBenchmarks
{
    private NonEmptyList<int> _small = null!;
    private NonEmptyList<int> _medium = null!;
    private NonEmptyList<int> _large = null!;
    private int[] _mediumArray = null!;
    private int[] _largeArray = null!;

    [GlobalSetup]
    public void Setup()
    {
        _small = NonEmptyList<int>.Of(1, 2, 3);
        _mediumArray = Enumerable.Range(1, 100).ToArray();
        _largeArray = Enumerable.Range(1, 10000).ToArray();
        _medium = NonEmptyList<int>.Of(_mediumArray[0], _mediumArray.Skip(1).ToArray());
        _large = NonEmptyList<int>.Of(_largeArray[0], _largeArray.Skip(1).ToArray());
    }

    [Benchmark(Baseline = true)]
    public NonEmptyList<int> Create_Small()
    {
        return NonEmptyList<int>.Of(1, 2, 3);
    }

    [Benchmark]
    public NonEmptyList<int> Create_Medium()
    {
        return NonEmptyList<int>.Of(_mediumArray[0], _mediumArray.Skip(1).ToArray());
    }

    [Benchmark]
    public int Head()
    {
        return _medium.Head;
    }

    [Benchmark]
    public IEnumerable<int> Tail()
    {
        return _medium.Tail;
    }

    [Benchmark]
    public NonEmptyList<int> Map_Small()
    {
        return _small.Map(x => x * 2);
    }

    [Benchmark]
    public NonEmptyList<int> Map_Medium()
    {
        return _medium.Map(x => x * 2);
    }

    [Benchmark]
    public int Reduce_Small()
    {
        return _small.Reduce((a, b) => a + b);
    }

    [Benchmark]
    public int Reduce_Medium()
    {
        return _medium.Reduce((a, b) => a + b);
    }

    [Benchmark]
    public int Reduce_Large()
    {
        return _large.Reduce((a, b) => a + b);
    }

    [Benchmark]
    public NonEmptyList<int> Append()
    {
        return _small.Append(4);
    }

    [Benchmark]
    public NonEmptyList<int> Prepend()
    {
        return _small.Prepend(0);
    }

    [Benchmark]
    public NonEmptyList<int> Concat()
    {
        return _small.Concat(_small);
    }
}

