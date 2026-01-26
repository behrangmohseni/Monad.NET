namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating async operations with Monad.NET.
/// All major types support async variants of their operations.
/// </summary>
public static class AsyncExamples
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Async operations integrate seamlessly with monads.\n");

        // Async Option mapping
        Console.WriteLine("1. Async Option Mapping:");
        var asyncOption = await Option<int>.Some(42)
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });
        Console.WriteLine($"   Result: {asyncOption}");

        // Async Result chaining
        Console.WriteLine("\n2. Async Result Chaining:");
        var asyncResult = await Result<int, string>.Ok(10)
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            })
            .BindAsync(async x =>
            {
                await Task.Delay(10);
                return x > 15
                    ? Result<int, string>.Ok(x + 5)
                    : Result<int, string>.Err("Too small");
            });
        Console.WriteLine($"   Result: {asyncResult}");

        // Async with Tap
        Console.WriteLine("\n3. Async Side Effects:");
        var logged = await Result<int, string>.Ok(42)
            .TapAsync(async x =>
            {
                await Task.Delay(10);
                Console.WriteLine($"   [Log] Processing value: {x}");
            });
        Console.WriteLine($"   Final: {logged}");

        // Parallel async operations
        Console.WriteLine("\n4. Parallel Operations:");
        var (user, orders) = await FetchDataInParallel();
        Console.WriteLine($"   User:   {user}");
        Console.WriteLine($"   Orders: {orders}");

        // Combining async results
        Console.WriteLine("\n5. Combining Async Results:");
        var combined = await CombineAsyncOperations();
        Console.WriteLine($"   Combined: {combined}");

        // Async Try
        Console.WriteLine("\n6. Async Try:");
        var asyncTry = await Try<string>.OfAsync(async () =>
        {
            await Task.Delay(10);
            return "Async operation completed";
        });
        Console.WriteLine($"   Result: {asyncTry}");

        // Async error handling
        Console.WriteLine("\n7. Async Error Handling:");
        var withRecovery = await SimulateApiCallWithRecovery();
        Console.WriteLine($"   Recovered: {withRecovery}");

        // Async pipeline
        Console.WriteLine("\n8. Async Pipeline:");
        var pipeline = await BuildAsyncPipeline("user-123");
        Console.WriteLine($"   Pipeline result: {pipeline}");

        // Cancellation support
        Console.WriteLine("\n9. Cancellation Support:");
        using var cts = new CancellationTokenSource();
        var cancellable = await Option<int>.Some(42)
            .MapAsync(async x =>
            {
                await Task.Delay(10, cts.Token);
                return x * 2;
            });
        Console.WriteLine($"   Cancellable result: {cancellable}");

        // Real-world: API orchestration
        Console.WriteLine("\n10. Real-World: API Orchestration:");
        var orchestrated = await OrchestrateCalls("order-456");
        orchestrated.Match(
            okAction: data => Console.WriteLine($"   Success: {data}"),
            errAction: err => Console.WriteLine($"   Error: {err}")
        );
    }

    private static async Task<(Option<string> User, Option<int> Orders)> FetchDataInParallel()
    {
        var userTask = FetchUserAsync();
        var ordersTask = FetchOrderCountAsync();

        await Task.WhenAll(userTask, ordersTask);

        return (await userTask, await ordersTask);
    }

    private static async Task<Option<string>> FetchUserAsync()
    {
        await Task.Delay(20);
        return Option<string>.Some("John Doe");
    }

    private static async Task<Option<int>> FetchOrderCountAsync()
    {
        await Task.Delay(15);
        return Option<int>.Some(5);
    }

    private static async Task<Option<string>> CombineAsyncOperations()
    {
        var user = await FetchUserAsync();
        var orders = await FetchOrderCountAsync();

        return user.Zip(orders)
            .Map(t => $"{t.Item1} has {t.Item2} orders");
    }

    private static async Task<Result<string, string>> SimulateApiCallWithRecovery()
    {
        // Simulate primary API failure
        var primary = await SimulateFailingApi();

        // Recover with fallback
        return await primary.OrElseAsync(async err =>
        {
            await Task.Delay(10);
            return Result<string, string>.Ok($"Recovered from: {err}");
        });
    }

    private static async Task<Result<string, string>> SimulateFailingApi()
    {
        await Task.Delay(10);
        return Result<string, string>.Err("Primary API failed");
    }

    private static async Task<Result<string, string>> BuildAsyncPipeline(string userId)
    {
        return await FetchUserByIdAsync(userId)
            .BindAsync(async user =>
            {
                await Task.Delay(10);
                return FetchUserPermissions(user);
            })
            .MapAsync(async perms =>
            {
                await Task.Delay(10);
                return $"User loaded with {perms.Length} permissions";
            });
    }

    private static async Task<Result<string, string>> FetchUserByIdAsync(string id)
    {
        await Task.Delay(10);
        return Result<string, string>.Ok($"User-{id}");
    }

    private static Result<string[], string> FetchUserPermissions(string user)
    {
        return Result<string[], string>.Ok(new[] { "read", "write", "admin" });
    }

    private static async Task<Result<string, string>> OrchestrateCalls(string orderId)
    {
        return await ValidateOrderAsync(orderId)
            .BindAsync(order => CheckInventoryAsync(order))
            .BindAsync(inventory => ProcessPaymentAsync(inventory))
            .MapAsync(async payment =>
            {
                await Task.Delay(10);
                return $"Order {orderId} completed: {payment}";
            });
    }

    private static async Task<Result<string, string>> ValidateOrderAsync(string orderId)
    {
        await Task.Delay(10);
        return Result<string, string>.Ok($"Validated-{orderId}");
    }

    private static async Task<Result<string, string>> CheckInventoryAsync(string order)
    {
        await Task.Delay(10);
        return Result<string, string>.Ok($"InStock-{order}");
    }

    private static async Task<Result<string, string>> ProcessPaymentAsync(string inventory)
    {
        await Task.Delay(10);
        return Result<string, string>.Ok($"Paid-{inventory}");
    }
}

