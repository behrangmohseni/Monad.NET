using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for newly added methods: Ensure, BiMap, Flatten, DefaultIfNone, ThrowIfNone/ThrowIfErr.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class NewMethodsBenchmarks
{
    private Option<int> _some;
    private Option<int> _none;
    private Result<int, string> _ok;
    private Result<int, string> _err;
    private Validation<int, string> _valid;
    private Validation<int, string> _invalid;
    private Validation<Validation<int, string>, string> _nestedValid;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option<int>.Some(42);
        _none = Option<int>.None();
        _ok = Result<int, string>.Ok(42);
        _err = Result<int, string>.Err("error");
        _valid = Validation<int, string>.Valid(42);
        _invalid = Validation<int, string>.Invalid("error");
        _nestedValid = Validation<Validation<int, string>, string>.Valid(
            Validation<int, string>.Valid(42));
    }

    #region Option.DefaultIfNone Benchmarks

    [Benchmark]
    public Option<int> DefaultIfNone_Some_Value()
    {
        return _some.DefaultIfNone(0);
    }

    [Benchmark]
    public Option<int> DefaultIfNone_None_Value()
    {
        return _none.DefaultIfNone(0);
    }

    [Benchmark]
    public Option<int> DefaultIfNone_Some_Factory()
    {
        return _some.DefaultIfNone(() => 0);
    }

    [Benchmark]
    public Option<int> DefaultIfNone_None_Factory()
    {
        return _none.DefaultIfNone(() => 0);
    }

    #endregion

    #region Option.ThrowIfNone Benchmarks

    [Benchmark]
    public int ThrowIfNone_Some()
    {
        return _some.ThrowIfNone(new InvalidOperationException("No value"));
    }

    [Benchmark]
    public int ThrowIfNone_Some_Factory()
    {
        return _some.ThrowIfNone(() => new InvalidOperationException("No value"));
    }

    #endregion

    #region Result.BiMap Benchmarks

    [Benchmark]
    public Result<string, int> BiMap_Ok()
    {
        return _ok.BiMap(x => x.ToString(), e => e.Length);
    }

    [Benchmark]
    public Result<string, int> BiMap_Err()
    {
        return _err.BiMap(x => x.ToString(), e => e.Length);
    }

    #endregion

    #region Result.ThrowIfErr Benchmarks

    [Benchmark]
    public int ThrowIfErr_Ok()
    {
        return _ok.ThrowIfErr(new InvalidOperationException("Error"));
    }

    [Benchmark]
    public int ThrowIfErr_Ok_Factory()
    {
        return _ok.ThrowIfErr(e => new InvalidOperationException(e));
    }

    #endregion

    #region Validation.Ensure Benchmarks

    [Benchmark]
    public Validation<int, string> Ensure_Valid_Passes()
    {
        return _valid.Ensure(x => x > 0, "Must be positive");
    }

    [Benchmark]
    public Validation<int, string> Ensure_Valid_Fails()
    {
        return _valid.Ensure(x => x > 100, "Must be > 100");
    }

    [Benchmark]
    public Validation<int, string> Ensure_Invalid()
    {
        return _invalid.Ensure(x => x > 0, "Must be positive");
    }

    [Benchmark]
    public Validation<int, string> Ensure_Chained()
    {
        return _valid
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be < 100")
            .Ensure(x => x % 2 == 0, "Must be even");
    }

    [Benchmark]
    public Validation<int, string> Ensure_Factory_Valid_Passes()
    {
        return _valid.Ensure(x => x > 0, () => "Must be positive");
    }

    #endregion

    #region Validation.Flatten Benchmarks

    [Benchmark]
    public Validation<int, string> Flatten_Valid()
    {
        return _nestedValid.Flatten();
    }

    [Benchmark]
    public Validation<int, string> Flatten_OuterInvalid()
    {
        var nested = Validation<Validation<int, string>, string>.Invalid("outer error");
        return nested.Flatten();
    }

    [Benchmark]
    public Validation<int, string> Flatten_InnerInvalid()
    {
        var nested = Validation<Validation<int, string>, string>.Valid(
            Validation<int, string>.Invalid("inner error"));
        return nested.Flatten();
    }

    #endregion

    #region Comparison with Existing Methods

    /// <summary>
    /// Compare BiMap vs separate Map + MapErr calls
    /// </summary>
    [Benchmark]
    public Result<string, int> BiMap_Comparison()
    {
        return _ok.BiMap(x => x.ToString(), e => e.Length);
    }

    [Benchmark]
    public Result<string, int> SeparateMaps_Comparison()
    {
        return _ok.Map(x => x.ToString()).MapError(e => e.Length);
    }

    /// <summary>
    /// Compare DefaultIfNone vs OrElse + Some
    /// </summary>
    [Benchmark]
    public Option<int> DefaultIfNone_Comparison()
    {
        return _none.DefaultIfNone(42);
    }

    [Benchmark]
    public Option<int> OrElseSome_Comparison()
    {
        return _none.OrElse(() => Option<int>.Some(42));
    }

    /// <summary>
    /// Compare Ensure vs AndThen validation pattern
    /// </summary>
    [Benchmark]
    public Validation<int, string> Ensure_Comparison()
    {
        return _valid.Ensure(x => x > 0, "Must be positive");
    }

    [Benchmark]
    public Validation<int, string> AndThenValidation_Comparison()
    {
        return _valid.Bind(x => x > 0 
            ? Validation<int, string>.Valid(x) 
            : Validation<int, string>.Invalid("Must be positive"));
    }

    #endregion
}

