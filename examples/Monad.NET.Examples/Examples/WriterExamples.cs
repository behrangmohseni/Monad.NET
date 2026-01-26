namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Writer&lt;W, T&gt; for accumulating logs alongside computations.
/// Writer lets you build up output (logs, traces) while computing values.
/// </summary>
public static class WriterExamples
{
    public static void Run()
    {
        Console.WriteLine("Writer<W, T> accumulates logs alongside computations.\n");

        // Creating Writer
        Console.WriteLine("1. Creating Writer:");
        var writer = Writer<string, int>.Tell(42, "Created value 42");
        Console.WriteLine($"   Value: {writer.Value}");
        Console.WriteLine($"   Log:   {writer.Log}");

        // Map preserves log
        Console.WriteLine("\n2. Map (preserves log):");
        var mapped = Writer<string, int>.Tell(5, "Initial").Map(x => x * 2);
        Console.WriteLine($"   Value: {mapped.Value}");
        Console.WriteLine($"   Log:   {mapped.Log}");

        // FlatMap combines logs
        Console.WriteLine("\n3. FlatMap (combines logs):");
        var chained = Writer<string, int>.Tell(10, "Step 1: Start with 10")
            .Bind(
                x => Writer<string, int>.Tell(x * 2, $"Step 2: Doubled to {x * 2}"),
                (log1, log2) => $"{log1} | {log2}"
            );
        Console.WriteLine($"   Value: {chained.Value}");
        Console.WriteLine($"   Log:   {chained.Log}");

        // List-based logging
        Console.WriteLine("\n4. List-Based Logging:");
        var listWriter = Writer<List<string>, int>.Tell(1, new List<string> { "Started" })
            .Bind(
                x => Writer<List<string>, int>.Tell(x + 1, new List<string> { "Incremented" }),
                (log1, log2) => log1.Concat(log2).ToList()
            )
            .Bind(
                x => Writer<List<string>, int>.Tell(x * 2, new List<string> { "Doubled" }),
                (log1, log2) => log1.Concat(log2).ToList()
            );
        Console.WriteLine($"   Final Value: {listWriter.Value}");
        Console.WriteLine($"   Log entries: [{string.Join(" -> ", listWriter.Log)}]");

        // Pattern matching
        Console.WriteLine("\n5. Pattern Matching:");
        var result = writer.Match((value, log) => $"Got {value} with log: '{log}'");
        Console.WriteLine($"   Result: {result}");

        // Computation pipeline
        Console.WriteLine("\n6. Computation Pipeline:");
        var pipeline = ComputeWithLogging(100);
        Console.WriteLine($"   Final: {pipeline.Value}");
        Console.WriteLine($"   Trace:\n{pipeline.Log}");

        // Using Writer for metrics
        Console.WriteLine("\n7. Collecting Metrics:");
        var metrics = ProcessWithMetrics(new[] { 1, 2, 3, 4, 5 });
        Console.WriteLine($"   Result: [{string.Join(", ", metrics.Value)}]");
        Console.WriteLine($"   Metrics: {metrics.Log}");

        // LINQ syntax
        Console.WriteLine("\n8. LINQ Query Syntax:");
        var linqResult = from a in Writer<string, int>.Tell(10, "Got 10")
                         from b in Writer<string, int>.Tell(20, "Got 20")
                         select a + b;
        Console.WriteLine($"   Value: {linqResult.Value}");
        Console.WriteLine($"   Log:   {linqResult.Log}");

        // Real-world: Audit trail
        Console.WriteLine("\n9. Real-World: Audit Trail:");
        var audit = PerformBusinessOperation("Order-123", 250.00m);
        Console.WriteLine($"   Operation Result: {audit.Value}");
        Console.WriteLine($"   Audit Log:");
        foreach (var entry in audit.Log)
            Console.WriteLine($"     {entry}");
    }

    private static Writer<string, decimal> ComputeWithLogging(decimal initial)
    {
        return Writer<string, decimal>.Tell(initial, $"   [LOG] Started with {initial}\n")
            .Bind(
                x => Writer<string, decimal>.Tell(x * 1.1m, $"   [LOG] Applied 10% increase: {x * 1.1m}\n"),
                (a, b) => a + b
            )
            .Bind(
                x => Writer<string, decimal>.Tell(Math.Round(x, 2), $"   [LOG] Rounded to {Math.Round(x, 2)}\n"),
                (a, b) => a + b
            );
    }

    private static Writer<Metrics, List<int>> ProcessWithMetrics(int[] items)
    {
        var processed = items.Select(x => x * 2).ToList();
        var metrics = new Metrics(
            ProcessedCount: items.Length,
            TotalValue: processed.Sum(),
            ProcessingTime: TimeSpan.FromMilliseconds(items.Length * 10)
        );
        return Writer<Metrics, List<int>>.Tell(processed, metrics);
    }

    private static Writer<List<string>, string> PerformBusinessOperation(string orderId, decimal amount)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");

        return Writer<List<string>, string>.Tell(
                orderId,
                new List<string> { $"[{timestamp}] Started processing order {orderId}" })
            .Bind(
                id => Writer<List<string>, string>.Tell(
                    $"{id}-VALIDATED",
                    new List<string> { $"[{timestamp}] Validated order {id} for ${amount:N2}" }),
                (log1, log2) => log1.Concat(log2).ToList()
            )
            .Bind(
                id => Writer<List<string>, string>.Tell(
                    $"{id}-COMPLETED",
                    new List<string> { $"[{timestamp}] Completed order {id}" }),
                (log1, log2) => log1.Concat(log2).ToList()
            );
    }

    record Metrics(int ProcessedCount, int TotalValue, TimeSpan ProcessingTime);
}

