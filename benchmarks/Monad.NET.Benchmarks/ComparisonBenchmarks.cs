using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Compares Monad.NET patterns with native C# alternatives.
/// These benchmarks help users make informed decisions about when to use monadic types.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class ComparisonBenchmarks
{
    private const int Iterations = 1000;

    #region Option vs Nullable

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Option vs Nullable - Simple")]
    public int? Nullable_SimpleOperation()
    {
        int? value = 42;
        return value.HasValue ? value.Value * 2 : null;
    }

    [Benchmark]
    [BenchmarkCategory("Option vs Nullable - Simple")]
    public Option<int> Option_SimpleOperation()
    {
        var option = Option<int>.Some(42);
        return option.Map(x => x * 2);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Option vs Nullable - Chained")]
    public int? Nullable_ChainedOperations()
    {
        int? value = 42;
        if (!value.HasValue) return null;
        var v1 = value.Value * 2;
        if (v1 <= 0) return null;
        var v2 = v1 + 10;
        if (v2 <= 50) return null;
        return v2 * 3;
    }

    [Benchmark]
    [BenchmarkCategory("Option vs Nullable - Chained")]
    public Option<int> Option_ChainedOperations()
    {
        return Option<int>.Some(42)
            .Map(x => x * 2)
            .Filter(x => x > 0)
            .Map(x => x + 10)
            .Filter(x => x > 50)
            .Map(x => x * 3);
    }

    #endregion

    #region Result vs Exception

    private static int DivideWithException(int a, int b)
    {
        if (b == 0) throw new DivideByZeroException();
        return a / b;
    }

    private static Result<int, string> DivideWithResult(int a, int b)
    {
        return b == 0
            ? Result<int, string>.Err("Division by zero")
            : Result<int, string>.Ok(a / b);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Result vs Exception - Success Path")]
    public int Exception_SuccessPath()
    {
        try
        {
            return DivideWithException(100, 5);
        }
        catch
        {
            return 0;
        }
    }

    [Benchmark]
    [BenchmarkCategory("Result vs Exception - Success Path")]
    public int Result_SuccessPath()
    {
        return DivideWithResult(100, 5).UnwrapOr(0);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Result vs Exception - Error Path")]
    public int Exception_ErrorPath()
    {
        try
        {
            return DivideWithException(100, 0);
        }
        catch
        {
            return -1;
        }
    }

    [Benchmark]
    [BenchmarkCategory("Result vs Exception - Error Path")]
    public int Result_ErrorPath()
    {
        return DivideWithResult(100, 0).UnwrapOr(-1);
    }

    #endregion

    #region Pipeline Benchmarks

    private record User(int Id, string Name, string? Email);
    private record Order(int Id, int UserId, decimal Amount);

    private static readonly User TestUser = new(1, "Test", "test@example.com");
    private static readonly Order TestOrder = new(1, 1, 99.99m);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Real-World Pipeline")]
    public string? TraditionalPipeline()
    {
        User? user = TestUser;
        if (user == null) return null;
        if (!user.Name.StartsWith("T")) return null;

        Order? order = TestOrder;
        if (order == null) return null;
        if (order.UserId != user.Id) return null;

        var email = user.Email;
        if (string.IsNullOrEmpty(email)) return null;

        return $"Order {order.Id} for {user.Name}: {order.Amount:C}";
    }

    [Benchmark]
    [BenchmarkCategory("Real-World Pipeline")]
    public Option<string> MonadicPipeline()
    {
        return Option<User>.Some(TestUser)
            .Filter(u => u.Name.StartsWith("T"))
            .ZipWith(Option<Order>.Some(TestOrder), (user, order) => (user, order))
            .Filter(x => x.order.UserId == x.user.Id)
            .AndThen(x => x.user.Email.ToOption().Map(email => (x.user, x.order, email)))
            .Map(x => $"Order {x.order.Id} for {x.user.Name}: {x.order.Amount:C}");
    }

    #endregion

    #region Allocation Benchmarks

    [Benchmark]
    [BenchmarkCategory("Allocations")]
    public List<int> Option_CollectValues_Allocating()
    {
        var options = Enumerable.Range(0, Iterations)
            .Select(i => i % 2 == 0 ? Option<int>.Some(i) : Option<int>.None());

        return options.Where(o => o.IsSome).Select(o => o.Unwrap()).ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Allocations")]
    public List<int> Option_Choose_Optimized()
    {
        var options = Enumerable.Range(0, Iterations)
            .Select(i => i % 2 == 0 ? Option<int>.Some(i) : Option<int>.None());

        return options.Choose().ToList();
    }

    #endregion
}
