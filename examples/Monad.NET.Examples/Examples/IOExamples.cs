namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating IO&lt;T&gt; for deferring and composing side effects.
/// IO describes what to do without doing it - execution happens only when Run() is called.
/// </summary>
public static class IOExamples
{
    public static void Run()
    {
        Console.WriteLine("IO<T> defers side effects - compose now, execute later.\n");

        // Creating IO
        Console.WriteLine("1. Creating IO:");
        var pureValue = IO.Pure(42);
        Console.WriteLine($"   Pure(42).Run(): {pureValue.Run()}");

        // IO with side effects
        Console.WriteLine("\n2. IO with Side Effects:");
        var ioAction = IO.Of(() =>
        {
            Console.WriteLine("   [Side Effect] This runs when .Run() is called");
            return "Effect completed";
        });
        Console.WriteLine("   Before Run():");
        var result = ioAction.Run();
        Console.WriteLine($"   Result: {result}");

        // Map
        Console.WriteLine("\n3. Map:");
        var mapped = IO.Pure(10).Map(x => x * 2);
        Console.WriteLine($"   Pure(10).Map(x * 2).Run(): {mapped.Run()}");

        // FlatMap for sequencing effects
        Console.WriteLine("\n4. FlatMap (sequence effects):");
        var sequence = IO.Of(() => { Console.WriteLine("   Step 1"); return 10; })
            .FlatMap(x => IO.Of(() => { Console.WriteLine("   Step 2"); return x * 2; }))
            .FlatMap(x => IO.Of(() => { Console.WriteLine("   Step 3"); return x.ToString(); }));
        Console.WriteLine("   Running sequence:");
        Console.WriteLine($"   Result: {sequence.Run()}");

        // IO.WriteLine and IO.ReadLine
        Console.WriteLine("\n5. Built-in IO Actions:");
        var writeAction = IO.WriteLine("   Hello from IO!");
        writeAction.Run();

        // Combining with Select/SelectMany (LINQ)
        Console.WriteLine("\n6. LINQ Query Syntax:");
        var program = from _ in IO.WriteLine("   [IO] Starting computation")
                      from a in IO.Pure(10)
                      from b in IO.Pure(20)
                      from __ in IO.WriteLine($"   [IO] Adding {a} + {b}")
                      select a + b;
        Console.WriteLine("   Running LINQ program:");
        Console.WriteLine($"   Result: {program.Run()}");

        // Lazy evaluation demonstration
        Console.WriteLine("\n7. Lazy Evaluation:");
        var lazyIO = IO.Of(() =>
        {
            Console.WriteLine("   [Lazy] Computing...");
            return DateTime.Now.Ticks;
        });
        Console.WriteLine("   IO created (nothing happened yet)");
        Console.WriteLine($"   First run:  {lazyIO.Run()}");
        Thread.Sleep(10);
        Console.WriteLine($"   Second run: {lazyIO.Run()}");

        // Error handling in IO
        Console.WriteLine("\n8. Error Handling:");
        var riskyIO = IO.Of<int>(() => throw new InvalidOperationException("Oops!"));
        try
        {
            riskyIO.Run();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   Caught: {ex.Message}");
        }

        // Real-world: File operations (simulated)
        Console.WriteLine("\n9. Real-World: File Operations (simulated):");
        var fileProgram = SimulateFileOperation();
        fileProgram.Run();

        // Combining multiple IOs
        Console.WriteLine("\n10. Combining IOs:");
        var combined = IO.Pure(1)
            .Zip(IO.Pure(2))
            .Zip(IO.Pure(3))
            .Map(t => t.Item1.Item1 + t.Item1.Item2 + t.Item2);
        Console.WriteLine($"   1 + 2 + 3 = {combined.Run()}");
    }

    private static IO<Unit> SimulateFileOperation()
    {
        return from _ in IO.WriteLine("   [FILE] Opening file...")
               from content in IO.Pure("File contents here")
               from __ in IO.WriteLine($"   [FILE] Read: {content}")
               from ___ in IO.WriteLine("   [FILE] Closing file...")
               select Unit.Default;
    }
}

