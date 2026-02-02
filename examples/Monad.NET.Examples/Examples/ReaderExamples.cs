using Monad.NET.Examples.Models;

namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Reader&lt;R, A&gt; for dependency injection in a functional way.
/// Reader lets you thread configuration/dependencies through computations.
/// </summary>
public static class ReaderExamples
{
    public static void Run()
    {
        Console.WriteLine("Reader<R, A> provides dependency injection in a functional way.\n");

        // Setup configuration
        var config = new AppConfig(
            AppName: "MyApp",
            ConnectionString: "Server=localhost;Database=mydb",
            MaxRetries: 3,
            Timeout: TimeSpan.FromSeconds(30)
        );

        // Basic Reader
        Console.WriteLine("1. Basic Reader:");
        var reader = Reader<AppConfig, string>.From(c => $"App: {c.AppName}");
        var result = reader.Run(config);
        Console.WriteLine($"   Result: {result}");

        // Pure (constant value)
        Console.WriteLine("\n2. Pure (ignores environment):");
        var pure = Reader<AppConfig, int>.Return(42);
        Console.WriteLine($"   Pure value: {pure.Run(config)}");

        // Ask (get entire environment)
        Console.WriteLine("\n3. Ask (get environment):");
        var ask = Reader<AppConfig, AppConfig>.Ask();
        var env = ask.Run(config);
        Console.WriteLine($"   Got config: {env.AppName}");

        // Asks (select from environment)
        Console.WriteLine("\n4. Asks (select from environment):");
        var getConnection = Reader<AppConfig, string>.Asks(c => c.ConnectionString);
        var getRetries = Reader<AppConfig, int>.Asks(c => c.MaxRetries);
        Console.WriteLine($"   Connection: {getConnection.Run(config)}");
        Console.WriteLine($"   Retries:    {getRetries.Run(config)}");

        // Map
        Console.WriteLine("\n5. Map:");
        var mapped = Reader<AppConfig, string>.Asks(c => c.AppName)
            .Map(name => name.ToUpper());
        Console.WriteLine($"   Mapped: {mapped.Run(config)}");

        // Bind for composing
        Console.WriteLine("\n6. Bind (compose readers):");
        var composed = Reader<AppConfig, string>.Asks(c => c.AppName)
            .Bind(name => Reader<AppConfig, string>.Asks(c =>
                $"{name} v1.0 (retries: {c.MaxRetries})"));
        Console.WriteLine($"   Composed: {composed.Run(config)}");

        // Method syntax composition
        Console.WriteLine("\n7. Method Syntax Composition:");
        var composedReader = Reader<AppConfig, string>.Asks(c => c.AppName)
            .Bind(name => Reader<AppConfig, int>.Asks(c => c.MaxRetries)
                .Map(retries => $"{name} configured with {retries} retries"));
        Console.WriteLine($"   Composed: {composedReader.Run(config)}");

        // Service composition
        Console.WriteLine("\n8. Service Composition:");
        var userService = GetUserReader(1);
        var greeting = userService.Bind(user =>
            Reader<AppConfig, string>.Asks(c => $"Welcome to {c.AppName}, {user}!"));
        Console.WriteLine($"   Greeting: {greeting.Run(config)}");

        // WithEnvironment
        Console.WriteLine("\n9. WithEnvironment (adapt readers):");
        var stringReader = Reader<string, int>.From(s => s.Length);
        var adapted = stringReader.WithEnvironment<AppConfig>(c => c.AppName);
        Console.WriteLine($"   App name length: {adapted.Run(config)}");

        // Real-world: Database service
        Console.WriteLine("\n10. Real-World: Database Service:");
        var dbService = CreateDatabaseService();
        var users = dbService.Run(config);
        Console.WriteLine($"   Query result: [{string.Join(", ", users)}]");

        // Real-world: Multi-layer architecture
        Console.WriteLine("\n11. Multi-Layer Architecture:");
        var businessLogic = ProcessOrder("order-123");
        var processResult = businessLogic.Run(config);
        Console.WriteLine($"   Process result: {processResult}");
    }

    private static Reader<AppConfig, string> GetUserReader(int userId)
    {
        return Reader<AppConfig, string>.From(config =>
        {
            // In real code, would use config.ConnectionString
            return $"User_{userId}";
        });
    }

    private static Reader<AppConfig, List<string>> CreateDatabaseService()
    {
        return Reader<AppConfig, List<string>>.From(config =>
        {
            // Simulate database query using connection string
            Console.WriteLine($"   [DB] Connecting to: {config.ConnectionString}");
            Console.WriteLine($"   [DB] Timeout: {config.Timeout.TotalSeconds}s");
            return new List<string> { "Alice", "Bob", "Charlie" };
        });
    }

    private static Reader<AppConfig, string> ProcessOrder(string orderId)
    {
        // Compose multiple operations that all need config
        return Reader<AppConfig, AppConfig>.Ask()
            .Bind(config =>
            {
                var validated = ValidateOrder(orderId, config.MaxRetries);
                return SaveOrder(orderId)
                    .Map(saved => $"Order {orderId}: validated={validated}, saved={saved}");
            });
    }

    private static bool ValidateOrder(string orderId, int maxRetries)
    {
        Console.WriteLine($"   [BIZ] Validating {orderId} with {maxRetries} retries");
        return true;
    }

    private static Reader<AppConfig, bool> SaveOrder(string orderId)
    {
        return Reader<AppConfig, bool>.From(config =>
        {
            Console.WriteLine($"   [DB] Saving {orderId} to database");
            return true;
        });
    }
}

