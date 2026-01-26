using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Monad.NET;
using System.Runtime.CompilerServices;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Comprehensive benchmarks to measure the actual impact of AggressiveInlining
/// across all monad types and operation categories.
/// 
/// Categories tested:
/// 1. Property Accessors (IsSome, IsOk, etc.) - simple boolean returns
/// 2. Factory Methods (Some, None, Ok, Err) - instance creation
/// 3. Value Access (UnwrapOr, UnwrapOrDefault) - simple value retrieval
/// 4. Conditional Access (UnwrapOrElse) - value with callback fallback
/// 5. Transform Operations (Map, Filter) - single transformation
/// 6. Chaining Operations (AndThen) - monadic binding
/// 7. Pattern Matching (Match) - dual-path execution
/// 8. Complex Pipelines - real-world chains
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AggressiveInliningComprehensiveBenchmarks
{
    private const int Iterations = 10000;
    
    // Test data
    private Option<int> _someOption;
    private Option<int> _noneOption;
    private Result<int, string> _okResult;
    private Result<int, string> _errResult;
    private Either<string, int> _rightEither;
    private Either<string, int> _leftEither;
    private Try<int> _successTry;
    private Try<int> _failureTry;
    private Validation<int, string> _validValidation;
    private Validation<int, string> _invalidValidation;

    [GlobalSetup]
    public void Setup()
    {
        _someOption = Option<int>.Some(42);
        _noneOption = Option<int>.None();
        _okResult = Result<int, string>.Ok(42);
        _errResult = Result<int, string>.Err("error");
        _rightEither = Either<string, int>.Right(42);
        _leftEither = Either<string, int>.Left("left");
        _successTry = Try<int>.Of(() => 42);
        _failureTry = Try<int>.Of(() => throw new InvalidOperationException("test"));
        _validValidation = Validation<int, string>.Valid(42);
        _invalidValidation = Validation<int, string>.Invalid("error");
    }

    #region Category 1: Property Accessors
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("1-PropertyAccessor", "Option")]
    public int Option_IsSome_Direct()
    {
        int count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            if (_someOption.IsSome) count++;
            if (_noneOption.IsSome) count++;
        }
        return count;
    }
    
    [Benchmark]
    [BenchmarkCategory("1-PropertyAccessor", "Option")]
    public int Option_IsSome_NoInline()
    {
        int count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            if (GetIsSome_NoInline(_someOption)) count++;
            if (GetIsSome_NoInline(_noneOption)) count++;
        }
        return count;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("1-PropertyAccessor", "Result")]
    public int Result_IsOk_Direct()
    {
        int count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            if (_okResult.IsOk) count++;
            if (_errResult.IsOk) count++;
        }
        return count;
    }
    
    [Benchmark]
    [BenchmarkCategory("1-PropertyAccessor", "Result")]
    public int Result_IsOk_NoInline()
    {
        int count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            if (GetIsOk_NoInline(_okResult)) count++;
            if (GetIsOk_NoInline(_errResult)) count++;
        }
        return count;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("1-PropertyAccessor", "Either")]
    public int Either_IsRight_Direct()
    {
        int count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            if (_rightEither.IsRight) count++;
            if (_leftEither.IsRight) count++;
        }
        return count;
    }
    
    [Benchmark]
    [BenchmarkCategory("1-PropertyAccessor", "Either")]
    public int Either_IsRight_NoInline()
    {
        int count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            if (GetIsRight_NoInline(_rightEither)) count++;
            if (GetIsRight_NoInline(_leftEither)) count++;
        }
        return count;
    }

    #endregion

    #region Category 2: Factory Methods
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("2-Factory", "Option")]
    public Option<int> Option_Some_Direct()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Option<int>.Some(i);
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("2-Factory", "Option")]
    public Option<int> Option_Some_NoInline()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = CreateSome_NoInline(i);
        }
        return result;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("2-Factory", "Option-None")]
    public Option<int> Option_None_Direct()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Option<int>.None();
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("2-Factory", "Option-None")]
    public Option<int> Option_None_NoInline()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = CreateNone_NoInline<int>();
        }
        return result;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("2-Factory", "Result")]
    public Result<int, string> Result_Ok_Direct()
    {
        Result<int, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Result<int, string>.Ok(i);
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("2-Factory", "Result")]
    public Result<int, string> Result_Ok_NoInline()
    {
        Result<int, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = CreateOk_NoInline<int, string>(i);
        }
        return result;
    }

    #endregion

    #region Category 3: Value Access (Simple)
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("3-ValueAccess", "UnwrapOr")]
    public int Option_UnwrapOr_Direct()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += _someOption.GetValueOr(0);
            sum += _noneOption.GetValueOr(-1);
        }
        return sum;
    }
    
    [Benchmark]
    [BenchmarkCategory("3-ValueAccess", "UnwrapOr")]
    public int Option_UnwrapOr_NoInline()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += UnwrapOr_NoInline(_someOption, 0);
            sum += UnwrapOr_NoInline(_noneOption, -1);
        }
        return sum;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("3-ValueAccess", "UnwrapOrDefault")]
    public int Option_UnwrapOrDefault_Direct()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += _someOption.GetValueOrDefault();
            sum += _noneOption.GetValueOrDefault();
        }
        return sum;
    }
    
    [Benchmark]
    [BenchmarkCategory("3-ValueAccess", "UnwrapOrDefault")]
    public int Option_UnwrapOrDefault_NoInline()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += UnwrapOrDefault_NoInline(_someOption);
            sum += UnwrapOrDefault_NoInline(_noneOption);
        }
        return sum;
    }

    #endregion

    #region Category 4: Conditional Access (With Callback)
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("4-ConditionalAccess", "UnwrapOrElse")]
    public int Option_UnwrapOrElse_Direct()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += _someOption.GetValueOrElse(() => -1);
            sum += _noneOption.GetValueOrElse(() => -1);
        }
        return sum;
    }
    
    [Benchmark]
    [BenchmarkCategory("4-ConditionalAccess", "UnwrapOrElse")]
    public int Option_UnwrapOrElse_NoInline()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += UnwrapOrElse_NoInline(_someOption, () => -1);
            sum += UnwrapOrElse_NoInline(_noneOption, () => -1);
        }
        return sum;
    }

    #endregion

    #region Category 5: Transform Operations
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("5-Transform", "Map")]
    public Option<int> Option_Map_Direct()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = _someOption.Map(x => x * 2);
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("5-Transform", "Map")]
    public Option<int> Option_Map_NoInline()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Map_NoInline(_someOption, x => x * 2);
        }
        return result;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("5-Transform", "Filter")]
    public Option<int> Option_Filter_Direct()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = _someOption.Filter(x => x > 0);
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("5-Transform", "Filter")]
    public Option<int> Option_Filter_NoInline()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Filter_NoInline(_someOption, x => x > 0);
        }
        return result;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("5-Transform", "Result-Map")]
    public Result<int, string> Result_Map_Direct()
    {
        Result<int, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = _okResult.Map(x => x * 2);
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("5-Transform", "Result-Map")]
    public Result<int, string> Result_Map_NoInline()
    {
        Result<int, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = MapResult_NoInline(_okResult, x => x * 2);
        }
        return result;
    }

    #endregion

    #region Category 6: Chaining Operations (AndThen)
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("6-Chaining", "AndThen")]
    public Option<int> Option_AndThen_Direct()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = _someOption.Bind(x => Option<int>.Some(x * 2));
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("6-Chaining", "AndThen")]
    public Option<int> Option_AndThen_NoInline()
    {
        Option<int> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = AndThen_NoInline(_someOption, x => Option<int>.Some(x * 2));
        }
        return result;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("6-Chaining", "Result-AndThen")]
    public Result<int, string> Result_AndThen_Direct()
    {
        Result<int, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = _okResult.Bind(x => Result<int, string>.Ok(x * 2));
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("6-Chaining", "Result-AndThen")]
    public Result<int, string> Result_AndThen_NoInline()
    {
        Result<int, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = AndThenResult_NoInline(_okResult, x => Result<int, string>.Ok(x * 2));
        }
        return result;
    }

    #endregion

    #region Category 7: Pattern Matching
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("7-Match", "Option")]
    public int Option_Match_Direct()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += _someOption.Match(x => x, () => 0);
            sum += _noneOption.Match(x => x, () => -1);
        }
        return sum;
    }
    
    [Benchmark]
    [BenchmarkCategory("7-Match", "Option")]
    public int Option_Match_NoInline()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += Match_NoInline(_someOption, x => x, () => 0);
            sum += Match_NoInline(_noneOption, x => x, () => -1);
        }
        return sum;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("7-Match", "Result")]
    public int Result_Match_Direct()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += _okResult.Match(x => x, _ => 0);
            sum += _errResult.Match(x => x, _ => -1);
        }
        return sum;
    }
    
    [Benchmark]
    [BenchmarkCategory("7-Match", "Result")]
    public int Result_Match_NoInline()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += MatchResult_NoInline(_okResult, x => x, _ => 0);
            sum += MatchResult_NoInline(_errResult, x => x, _ => -1);
        }
        return sum;
    }

    #endregion

    #region Category 8: Complex Pipelines
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("8-Pipeline", "Option")]
    public Option<string> Option_Pipeline_Direct()
    {
        Option<string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Option<int>.Some(i)
                .Filter(x => x % 2 == 0)
                .Map(x => x * 2)
                .Bind(x => x > 0 ? Option<string>.Some($"Value: {x}") : Option<string>.None());
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("8-Pipeline", "Option")]
    public Option<string> Option_Pipeline_NoInline()
    {
        Option<string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            var opt = CreateSome_NoInline(i);
            opt = Filter_NoInline(opt, x => x % 2 == 0);
            var mapped = Map_NoInline(opt, x => x * 2);
            result = AndThen_NoInline(mapped, x => x > 0 ? CreateSome_NoInline($"Value: {x}") : CreateNone_NoInline<string>());
        }
        return result;
    }
    
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("8-Pipeline", "Result")]
    public Result<string, string> Result_Pipeline_Direct()
    {
        Result<string, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            result = Result<int, string>.Ok(i)
                .Map(x => x * 2)
                .Bind(x => x >= 0 
                    ? Result<string, string>.Ok($"Value: {x}") 
                    : Result<string, string>.Err("Negative"));
        }
        return result;
    }
    
    [Benchmark]
    [BenchmarkCategory("8-Pipeline", "Result")]
    public Result<string, string> Result_Pipeline_NoInline()
    {
        Result<string, string> result = default;
        for (int i = 0; i < Iterations; i++)
        {
            var res = CreateOk_NoInline<int, string>(i);
            var mapped = MapResult_NoInline(res, x => x * 2);
            result = AndThenResult_NoInline(mapped, x => x >= 0 
                ? CreateOk_NoInline<string, string>($"Value: {x}") 
                : CreateErr_NoInline<string, string>("Negative"));
        }
        return result;
    }

    #endregion

    #region NoInline Wrapper Methods
    
    // Property accessors
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool GetIsSome_NoInline<T>(Option<T> option) => option.IsSome;
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool GetIsOk_NoInline<T, E>(Result<T, E> result) => result.IsOk;
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool GetIsRight_NoInline<L, R>(Either<L, R> either) => either.IsRight;
    
    // Factory methods
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Option<T> CreateSome_NoInline<T>(T value) => Option<T>.Some(value);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Option<T> CreateNone_NoInline<T>() => Option<T>.None();
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<T, E> CreateOk_NoInline<T, E>(T value) => Result<T, E>.Ok(value);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<T, E> CreateErr_NoInline<T, E>(E error) => Result<T, E>.Err(error);
    
    // Value access
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T UnwrapOr_NoInline<T>(Option<T> option, T defaultValue) => option.GetValueOr(defaultValue);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T UnwrapOrDefault_NoInline<T>(Option<T> option) where T : struct => option.GetValueOrDefault();
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T UnwrapOrElse_NoInline<T>(Option<T> option, Func<T> defaultFunc) => option.GetValueOrElse(defaultFunc);
    
    // Transform operations
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Option<U> Map_NoInline<T, U>(Option<T> option, Func<T, U> mapper) => option.Map(mapper);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Option<T> Filter_NoInline<T>(Option<T> option, Func<T, bool> predicate) => option.Filter(predicate);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<U, E> MapResult_NoInline<T, U, E>(Result<T, E> result, Func<T, U> mapper) => result.Map(mapper);
    
    // Chaining operations
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Option<U> AndThen_NoInline<T, U>(Option<T> option, Func<T, Option<U>> binder) => option.Bind(binder);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<U, E> AndThenResult_NoInline<T, U, E>(Result<T, E> result, Func<T, Result<U, E>> binder) => result.Bind(binder);
    
    // Pattern matching
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static U Match_NoInline<T, U>(Option<T> option, Func<T, U> some, Func<U> none) => option.Match(some, none);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static U MatchResult_NoInline<T, E, U>(Result<T, E> result, Func<T, U> ok, Func<E, U> err) => result.Match(ok, err);

    #endregion
}

