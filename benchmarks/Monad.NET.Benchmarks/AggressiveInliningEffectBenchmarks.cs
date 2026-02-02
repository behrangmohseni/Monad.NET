using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks to measure the actual effect of [MethodImpl(MethodImplOptions.AggressiveInlining)].
/// 
/// Methodology:
/// We create DUPLICATE implementations of core operations:
/// - One WITH AggressiveInlining (mirrors the library)
/// - One WITHOUT AggressiveInlining (baseline for comparison)
/// 
/// This gives a true measurement of whether AggressiveInlining provides benefit.
/// 
/// Expected outcomes:
/// - Property accessors: JIT usually inlines these anyway → minimal difference
/// - Small methods (&lt;32 bytes IL): JIT often auto-inlines → small difference  
/// - Medium methods: AggressiveInlining helps → noticeable difference
/// - Large methods: Won't inline regardless → no difference
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[DisassemblyDiagnoser(maxDepth: 2)] // Shows actual JIT output to verify inlining
public class AggressiveInliningEffectBenchmarks
{
    private const int Iterations = 100_000;

    // Test structs that mirror Option<T> behavior
    private TestOption_Inlined<int> _inlinedSome;
    private TestOption_NoInline<int> _noInlineSome;
    private TestResult_Inlined<int, string> _inlinedOk;
    private TestResult_NoInline<int, string> _noInlineOk;

    [GlobalSetup]
    public void Setup()
    {
        _inlinedSome = TestOption_Inlined<int>.Some(42);
        _noInlineSome = TestOption_NoInline<int>.Some(42);
        _inlinedOk = TestResult_Inlined<int, string>.Ok(42);
        _noInlineOk = TestResult_NoInline<int, string>.Ok(42);
    }

    #region IsSome Property - Comparing Inlined vs Not

    [BenchmarkCategory("IsSome")]
    [Benchmark(Baseline = true)]
    public int IsSome_WithAggressiveInlining()
    {
        var count = 0;
        var opt = _inlinedSome;
        for (var i = 0; i < Iterations; i++)
        {
            if (opt.IsSome) count++;
        }
        return count;
    }

    [BenchmarkCategory("IsSome")]
    [Benchmark]
    public int IsSome_WithoutAggressiveInlining()
    {
        var count = 0;
        var opt = _noInlineSome;
        for (var i = 0; i < Iterations; i++)
        {
            if (opt.IsSome) count++;
        }
        return count;
    }

    #endregion

    #region Some() Factory - Comparing Inlined vs Not

    [BenchmarkCategory("Some")]
    [Benchmark(Baseline = true)]
    public TestOption_Inlined<int> Some_WithAggressiveInlining()
    {
        TestOption_Inlined<int> result = default;
        for (var i = 0; i < Iterations; i++)
        {
            result = TestOption_Inlined<int>.Some(i);
        }
        return result;
    }

    [BenchmarkCategory("Some")]
    [Benchmark]
    public TestOption_NoInline<int> Some_WithoutAggressiveInlining()
    {
        TestOption_NoInline<int> result = default;
        for (var i = 0; i < Iterations; i++)
        {
            result = TestOption_NoInline<int>.Some(i);
        }
        return result;
    }

    #endregion

    #region None() Factory - Comparing Inlined vs Not

    [BenchmarkCategory("None")]
    [Benchmark(Baseline = true)]
    public TestOption_Inlined<int> None_WithAggressiveInlining()
    {
        TestOption_Inlined<int> result = default;
        for (var i = 0; i < Iterations; i++)
        {
            result = TestOption_Inlined<int>.None();
        }
        return result;
    }

    [BenchmarkCategory("None")]
    [Benchmark]
    public TestOption_NoInline<int> None_WithoutAggressiveInlining()
    {
        TestOption_NoInline<int> result = default;
        for (var i = 0; i < Iterations; i++)
        {
            result = TestOption_NoInline<int>.None();
        }
        return result;
    }

    #endregion

    #region UnwrapOr - Comparing Inlined vs Not

    [BenchmarkCategory("UnwrapOr")]
    [Benchmark(Baseline = true)]
    public int UnwrapOr_WithAggressiveInlining()
    {
        var sum = 0;
        var opt = _inlinedSome;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.GetValueOr(0);
        }
        return sum;
    }

    [BenchmarkCategory("UnwrapOr")]
    [Benchmark]
    public int UnwrapOr_WithoutAggressiveInlining()
    {
        var sum = 0;
        var opt = _noInlineSome;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.GetValueOr(0);
        }
        return sum;
    }

    #endregion

    #region UnwrapOrElse (with Func) - Comparing Inlined vs Not

    [BenchmarkCategory("UnwrapOrElse")]
    [Benchmark(Baseline = true)]
    public int UnwrapOrElse_WithAggressiveInlining()
    {
        var sum = 0;
        var opt = _inlinedSome;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.GetValueOrElse(() => 0);
        }
        return sum;
    }

    [BenchmarkCategory("UnwrapOrElse")]
    [Benchmark]
    public int UnwrapOrElse_WithoutAggressiveInlining()
    {
        var sum = 0;
        var opt = _noInlineSome;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.GetValueOrElse(() => 0);
        }
        return sum;
    }

    #endregion

    #region Map - Comparing Inlined vs Not

    [BenchmarkCategory("Map")]
    [Benchmark(Baseline = true)]
    public TestOption_Inlined<int> Map_WithAggressiveInlining()
    {
        var opt = _inlinedSome;
        for (var i = 0; i < Iterations; i++)
        {
            opt = opt.Map(x => x + 1);
        }
        return opt;
    }

    [BenchmarkCategory("Map")]
    [Benchmark]
    public TestOption_NoInline<int> Map_WithoutAggressiveInlining()
    {
        var opt = _noInlineSome;
        for (var i = 0; i < Iterations; i++)
        {
            opt = opt.Map(x => x + 1);
        }
        return opt;
    }

    #endregion

    #region Filter - Comparing Inlined vs Not

    [BenchmarkCategory("Filter")]
    [Benchmark(Baseline = true)]
    public TestOption_Inlined<int> Filter_WithAggressiveInlining()
    {
        TestOption_Inlined<int> result = default;
        var opt = _inlinedSome;
        for (var i = 0; i < Iterations; i++)
        {
            result = opt.Filter(x => x > 0);
        }
        return result;
    }

    [BenchmarkCategory("Filter")]
    [Benchmark]
    public TestOption_NoInline<int> Filter_WithoutAggressiveInlining()
    {
        TestOption_NoInline<int> result = default;
        var opt = _noInlineSome;
        for (var i = 0; i < Iterations; i++)
        {
            result = opt.Filter(x => x > 0);
        }
        return result;
    }

    #endregion

    #region Match - Comparing Inlined vs Not

    [BenchmarkCategory("Match")]
    [Benchmark(Baseline = true)]
    public int Match_WithAggressiveInlining()
    {
        var sum = 0;
        var opt = _inlinedSome;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.Match(v => v, () => 0);
        }
        return sum;
    }

    [BenchmarkCategory("Match")]
    [Benchmark]
    public int Match_WithoutAggressiveInlining()
    {
        var sum = 0;
        var opt = _noInlineSome;
        for (var i = 0; i < Iterations; i++)
        {
            sum += opt.Match(v => v, () => 0);
        }
        return sum;
    }

    #endregion

    #region AndThen - Comparing Inlined vs Not

    [BenchmarkCategory("AndThen")]
    [Benchmark(Baseline = true)]
    public TestOption_Inlined<int> AndThen_WithAggressiveInlining()
    {
        var opt = _inlinedSome;
        TestOption_Inlined<int> result = default;
        for (var i = 0; i < Iterations; i++)
        {
            result = opt.Bind(x => TestOption_Inlined<int>.Some(x * 2));
        }
        return result;
    }

    [BenchmarkCategory("AndThen")]
    [Benchmark]
    public TestOption_NoInline<int> AndThen_WithoutAggressiveInlining()
    {
        var opt = _noInlineSome;
        TestOption_NoInline<int> result = default;
        for (var i = 0; i < Iterations; i++)
        {
            result = opt.Bind(x => TestOption_NoInline<int>.Some(x * 2));
        }
        return result;
    }

    #endregion

    #region Result.IsOk - Comparing Inlined vs Not

    [BenchmarkCategory("Result.IsOk")]
    [Benchmark(Baseline = true)]
    public int ResultIsOk_WithAggressiveInlining()
    {
        var count = 0;
        var res = _inlinedOk;
        for (var i = 0; i < Iterations; i++)
        {
            if (res.IsOk) count++;
        }
        return count;
    }

    [BenchmarkCategory("Result.IsOk")]
    [Benchmark]
    public int ResultIsOk_WithoutAggressiveInlining()
    {
        var count = 0;
        var res = _noInlineOk;
        for (var i = 0; i < Iterations; i++)
        {
            if (res.IsOk) count++;
        }
        return count;
    }

    #endregion

    #region Result.Map - Comparing Inlined vs Not

    [BenchmarkCategory("Result.Map")]
    [Benchmark(Baseline = true)]
    public TestResult_Inlined<int, string> ResultMap_WithAggressiveInlining()
    {
        var res = _inlinedOk;
        for (var i = 0; i < Iterations; i++)
        {
            res = res.Map(x => x + 1);
        }
        return res;
    }

    [BenchmarkCategory("Result.Map")]
    [Benchmark]
    public TestResult_NoInline<int, string> ResultMap_WithoutAggressiveInlining()
    {
        var res = _noInlineOk;
        for (var i = 0; i < Iterations; i++)
        {
            res = res.Map(x => x + 1);
        }
        return res;
    }

    #endregion

    #region Chained Operations - Real World

    [BenchmarkCategory("Chained")]
    [Benchmark(Baseline = true)]
    public int Chained_WithAggressiveInlining()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            sum += _inlinedSome
                .Map(x => x + i)
                .Filter(x => x > 0)
                .Map(x => x * 2)
                .GetValueOr(0);
        }
        return sum;
    }

    [BenchmarkCategory("Chained")]
    [Benchmark]
    public int Chained_WithoutAggressiveInlining()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            sum += _noInlineSome
                .Map(x => x + i)
                .Filter(x => x > 0)
                .Map(x => x * 2)
                .GetValueOr(0);
        }
        return sum;
    }

    #endregion
}

#region Test Types - Identical implementations, different inlining

/// <summary>
/// Option implementation WITH AggressiveInlining on all methods.
/// Mirrors the actual Monad.NET Option implementation.
/// </summary>
public readonly struct TestOption_Inlined<T>
{
    private readonly T _value;
    private readonly bool _isSome;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TestOption_Inlined(T value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
    }

    public bool IsSome
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSome;
    }

    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isSome;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TestOption_Inlined<T> Some(T value) => new(value, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TestOption_Inlined<T> None() => new(default!, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue) => _isSome ? _value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrElse(Func<T> defaultFunc) => _isSome ? _value : defaultFunc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TestOption_Inlined<U> Map<U>(Func<T, U> mapper) =>
        _isSome ? TestOption_Inlined<U>.Some(mapper(_value)) : TestOption_Inlined<U>.None();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TestOption_Inlined<T> Filter(Func<T, bool> predicate) =>
        _isSome && predicate(_value) ? this : None();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> some, Func<U> none) =>
        _isSome ? some(_value) : none();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TestOption_Inlined<U> Bind<U>(Func<T, TestOption_Inlined<U>> binder) =>
        _isSome ? binder(_value) : TestOption_Inlined<U>.None();
}

/// <summary>
/// Option implementation WITHOUT AggressiveInlining.
/// Lets the JIT decide whether to inline based on heuristics.
/// </summary>
public readonly struct TestOption_NoInline<T>
{
    private readonly T _value;
    private readonly bool _isSome;

    // NO AggressiveInlining attribute
    private TestOption_NoInline(T value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
    }

    public bool IsSome => _isSome;

    public bool IsNone => !_isSome;

    public static TestOption_NoInline<T> Some(T value) => new(value, true);

    public static TestOption_NoInline<T> None() => new(default!, false);

    public T GetValueOr(T defaultValue) => _isSome ? _value : defaultValue;

    public T GetValueOrElse(Func<T> defaultFunc) => _isSome ? _value : defaultFunc();

    public TestOption_NoInline<U> Map<U>(Func<T, U> mapper) =>
        _isSome ? TestOption_NoInline<U>.Some(mapper(_value)) : TestOption_NoInline<U>.None();

    public TestOption_NoInline<T> Filter(Func<T, bool> predicate) =>
        _isSome && predicate(_value) ? this : None();

    public U Match<U>(Func<T, U> some, Func<U> none) =>
        _isSome ? some(_value) : none();

    public TestOption_NoInline<U> Bind<U>(Func<T, TestOption_NoInline<U>> binder) =>
        _isSome ? binder(_value) : TestOption_NoInline<U>.None();
}

/// <summary>
/// Result implementation WITH AggressiveInlining.
/// </summary>
public readonly struct TestResult_Inlined<T, E>
{
    private readonly T _value;
    private readonly E _error;
    private readonly bool _isOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TestResult_Inlined(T value, E error, bool isOk)
    {
        _value = value;
        _error = error;
        _isOk = isOk;
    }

    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isOk;
    }

    public bool IsErr
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !_isOk;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TestResult_Inlined<T, E> Ok(T value) => new(value, default!, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TestResult_Inlined<T, E> Err(E error) => new(default!, error, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue) => _isOk ? _value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TestResult_Inlined<U, E> Map<U>(Func<T, U> mapper) =>
        _isOk ? TestResult_Inlined<U, E>.Ok(mapper(_value)) : TestResult_Inlined<U, E>.Error(_error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, U> ok, Func<E, U> err) =>
        _isOk ? ok(_value) : err(_error);
}

/// <summary>
/// Result implementation WITHOUT AggressiveInlining.
/// </summary>
public readonly struct TestResult_NoInline<T, E>
{
    private readonly T _value;
    private readonly E _error;
    private readonly bool _isOk;

    private TestResult_NoInline(T value, E error, bool isOk)
    {
        _value = value;
        _error = error;
        _isOk = isOk;
    }

    public bool IsOk => _isOk;

    public bool IsErr => !_isOk;

    public static TestResult_NoInline<T, E> Ok(T value) => new(value, default!, true);

    public static TestResult_NoInline<T, E> Err(E error) => new(default!, error, false);

    public T GetValueOr(T defaultValue) => _isOk ? _value : defaultValue;

    public TestResult_NoInline<U, E> Map<U>(Func<T, U> mapper) =>
        _isOk ? TestResult_NoInline<U, E>.Ok(mapper(_value)) : TestResult_NoInline<U, E>.Error(_error);

    public U Match<U>(Func<T, U> ok, Func<E, U> err) =>
        _isOk ? ok(_value) : err(_error);
}

#endregion
