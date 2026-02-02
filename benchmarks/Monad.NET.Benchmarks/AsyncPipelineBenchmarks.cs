using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

#pragma warning disable CS0162 // Unreachable code detected - DelayMs is intentionally 0 for benchmarks

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks for async operations with Monad.NET types.
/// Compares monadic async patterns with traditional async/await patterns.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class AsyncPipelineBenchmarks
{
    private const int DelayMs = 0; // Set to 0 for pure overhead measurement
    
    // Simulated async services
    private static async Task<User?> GetUserNullableAsync(int id)
    {
        if (DelayMs > 0) await Task.Delay(DelayMs);
        return id > 0 ? new User(id, $"User{id}", $"user{id}@example.com") : null;
    }

    private static async Task<Option<User>> GetUserOptionAsync(int id)
    {
        if (DelayMs > 0) await Task.Delay(DelayMs);
        return id > 0 ? Option<User>.Some(new User(id, $"User{id}", $"user{id}@example.com")) : Option<User>.None();
    }

    private static async Task<Result<User, string>> GetUserResultAsync(int id)
    {
        if (DelayMs > 0) await Task.Delay(DelayMs);
        return id > 0 
            ? Result<User, string>.Ok(new User(id, $"User{id}", $"user{id}@example.com"))
            : Result<User, string>.Error("User not found");
    }

    private static async Task<Order?> GetOrderNullableAsync(int userId)
    {
        if (DelayMs > 0) await Task.Delay(DelayMs);
        return userId > 0 ? new Order(userId * 10, userId, 99.99m) : null;
    }

    private static async Task<Option<Order>> GetOrderOptionAsync(int userId)
    {
        if (DelayMs > 0) await Task.Delay(DelayMs);
        return userId > 0 
            ? Option<Order>.Some(new Order(userId * 10, userId, 99.99m)) 
            : Option<Order>.None();
    }

    private static async Task<Result<Order, string>> GetOrderResultAsync(int userId)
    {
        if (DelayMs > 0) await Task.Delay(DelayMs);
        return userId > 0 
            ? Result<Order, string>.Ok(new Order(userId * 10, userId, 99.99m))
            : Result<Order, string>.Error("Order not found");
    }

    #region Simple Async Chain

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Simple Async Chain")]
    public async Task<string?> Traditional_SimpleAsyncChain()
    {
        var user = await GetUserNullableAsync(1);
        if (user == null) return null;
        
        var order = await GetOrderNullableAsync(user.Id);
        if (order == null) return null;
        
        return $"Order {order.Id} for {user.Name}";
    }

    [Benchmark]
    [BenchmarkCategory("Simple Async Chain")]
    public async Task<Option<string>> Option_SimpleAsyncChain()
    {
        return await GetUserOptionAsync(1)
            .BindAsync(async user =>
            {
                var order = await GetOrderOptionAsync(user.Id);
                return order.Map(o => (user, o));
            })
            .MapAsync(x => $"Order {x.o.Id} for {x.user.Name}");
    }

    [Benchmark]
    [BenchmarkCategory("Simple Async Chain")]
    public async Task<Result<string, string>> Result_SimpleAsyncChain()
    {
        return await GetUserResultAsync(1)
            .BindAsync(async user =>
            {
                var orderResult = await GetOrderResultAsync(user.Id);
                return orderResult.Map(o => (user, o));
            })
            .MapAsync(x => $"Order {x.o.Id} for {x.user.Name}");
    }

    #endregion

    #region Complex Async Pipeline

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Complex Async Pipeline")]
    public async Task<OrderSummary?> Traditional_ComplexPipeline()
    {
        var user = await GetUserNullableAsync(1);
        if (user == null) return null;
        
        if (!user.Email.Contains('@')) return null;
        
        var order = await GetOrderNullableAsync(user.Id);
        if (order == null) return null;
        
        if (order.Amount <= 0) return null;
        
        var summary = new OrderSummary(
            order.Id,
            user.Name,
            user.Email,
            order.Amount,
            CalculateDiscount(order.Amount));
        
        // Simulate logging side effect
        _ = summary.ToString();
        
        return summary;
    }

    [Benchmark]
    [BenchmarkCategory("Complex Async Pipeline")]
    public async Task<Option<OrderSummary>> Option_ComplexPipeline()
    {
        return await GetUserOptionAsync(1)
            .FilterAsync(user => Task.FromResult(user.Email.Contains("@")))
            .BindAsync(async user =>
            {
                var order = await GetOrderOptionAsync(user.Id);
                return order.Map(o => (user, order: o));
            })
            .FilterAsync(x => Task.FromResult(x.order.Amount > 0))
            .MapAsync(x => new OrderSummary(
                x.order.Id,
                x.user.Name,
                x.user.Email,
                x.order.Amount,
                CalculateDiscount(x.order.Amount)))
            .TapAsync(summary => Task.Run(() => _ = summary.ToString()));
    }

    [Benchmark]
    [BenchmarkCategory("Complex Async Pipeline")]
    public async Task<Result<OrderSummary, string>> Result_ComplexPipeline()
    {
        return await GetUserResultAsync(1)
            .BindAsync(async user =>
            {
                if (!user.Email.Contains("@"))
                    return Result<User, string>.Error("Invalid email");
                return Result<User, string>.Ok(user);
            })
            .BindAsync(async user =>
            {
                var orderResult = await GetOrderResultAsync(user.Id);
                return orderResult.Map(o => (user, order: o));
            })
            .BindAsync(async x =>
            {
                if (x.order.Amount <= 0)
                    return Result<(User user, Order order), string>.Error("Invalid order amount");
                return Result<(User user, Order order), string>.Ok(x);
            })
            .MapAsync(x => new OrderSummary(
                x.order.Id,
                x.user.Name,
                x.user.Email,
                x.order.Amount,
                CalculateDiscount(x.order.Amount)))
            .TapAsync(summary => Task.Run(() => _ = summary.ToString()));
    }

    #endregion

    #region Parallel Async Operations

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Parallel Operations")]
    public async Task<(User?, Order?)> Traditional_ParallelFetch()
    {
        var userTask = GetUserNullableAsync(1);
        var orderTask = GetOrderNullableAsync(1);
        
        await Task.WhenAll(userTask, orderTask);
        
        return (userTask.Result, orderTask.Result);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel Operations")]
    public async Task<Option<(User, Order)>> Option_ParallelFetch()
    {
        var userTask = GetUserOptionAsync(1);
        var orderTask = GetOrderOptionAsync(1);
        
        var user = await userTask;
        var order = await orderTask;
        
        return user.Zip(order);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel Operations")]
    public async Task<Result<(User, Order), string>> Result_ParallelFetch()
    {
        return await ResultExtensions.CombineAsync(
            GetUserResultAsync(1),
            GetOrderResultAsync(1));
    }

    #endregion

    #region Batch Processing

    private static readonly int[] UserIds = Enumerable.Range(1, 10).ToArray();

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Batch Processing")]
    public async Task<List<User>> Traditional_BatchFetch()
    {
        var results = new List<User>();
        foreach (var id in UserIds)
        {
            var user = await GetUserNullableAsync(id);
            if (user != null)
                results.Add(user);
        }
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Batch Processing")]
    public async Task<Option<IReadOnlyList<User>>> Option_BatchFetch_Sequential()
    {
        var tasks = UserIds.Select(id => GetUserOptionAsync(id));
        var results = new List<User>();
        
        foreach (var task in tasks)
        {
            var result = await task;
            if (result.IsNone) return Option<IReadOnlyList<User>>.None();
            results.Add(result.GetValue());
        }
        
        return Option<IReadOnlyList<User>>.Some(results);
    }

    [Benchmark]
    [BenchmarkCategory("Batch Processing")]
    public async Task<Result<IReadOnlyList<User>, string>> Result_BatchFetch()
    {
        var tasks = UserIds.Select(id => GetUserResultAsync(id));
        return await ResultExtensions.CombineAsync(tasks);
    }

    #endregion

    #region Error Recovery

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Error Recovery")]
    public async Task<User?> Traditional_WithFallback()
    {
        var user = await GetUserNullableAsync(-1); // Will fail
        if (user == null)
        {
            user = await GetUserNullableAsync(1); // Fallback
        }
        return user;
    }

    [Benchmark]
    [BenchmarkCategory("Error Recovery")]
    public async Task<Option<User>> Option_WithFallback()
    {
        return await GetUserOptionAsync(-1)
            .OrElseAsync(() => GetUserOptionAsync(1));
    }

    [Benchmark]
    [BenchmarkCategory("Error Recovery")]
    public async Task<Result<User, string>> Result_WithFallback()
    {
        return await GetUserResultAsync(-1)
            .OrElseAsync(err => GetUserResultAsync(1));
    }

    #endregion

    // Helper types and methods
    public record User(int Id, string Name, string Email);
    public record Order(int Id, int UserId, decimal Amount);
    public record OrderSummary(int OrderId, string UserName, string Email, decimal Amount, decimal Discount);

    private static decimal CalculateDiscount(decimal amount) => amount > 50 ? amount * 0.1m : 0;
}

