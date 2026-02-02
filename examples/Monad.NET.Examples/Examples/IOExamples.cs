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
        var pureValue = IO.Return(42);
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
        var mapped = IO.Return(10).Map(x => x * 2);
        Console.WriteLine($"   Pure(10).Map(x * 2).Run(): {mapped.Run()}");

        // Bind for sequencing effects
        Console.WriteLine("\n4. Bind (sequence effects):");
        var sequence = IO.Of(() => { Console.WriteLine("   Step 1"); return 10; })
            .Bind(x => IO.Of(() => { Console.WriteLine("   Step 2"); return x * 2; }))
            .Bind(x => IO.Of(() => { Console.WriteLine("   Step 3"); return x.ToString(); }));
        Console.WriteLine("   Running sequence:");
        Console.WriteLine($"   Result: {sequence.Run()}");

        // IO.WriteLine and IO.ReadLine
        Console.WriteLine("\n5. Built-in IO Actions:");
        var writeAction = IO.WriteLine("   Hello from IO!");
        writeAction.Run();

        // Combining with Bind/Map
        Console.WriteLine("\n6. Chaining IO with Bind/Map:");
        var program = IO.WriteLine("   [IO] Starting computation")
            .Bind(_ => IO.Return(10)
                .Bind(a => IO.Return(20)
                    .Bind(b => IO.WriteLine($"   [IO] Adding {a} + {b}")
                        .Map(_ => a + b))));
        Console.WriteLine("   Running chained program:");
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
        var combined = IO.Return(1)
            .Zip(IO.Return(2))
            .Zip(IO.Return(3))
            .Map(t => t.Item1.Item1 + t.Item1.Item2 + t.Item2);
        Console.WriteLine($"   1 + 2 + 3 = {combined.Run()}");
    }

    private static IO<Unit> SimulateFileOperation()
    {
        return IO.WriteLine("   [FILE] Opening file...")
            .Bind(_ => IO.Return("File contents here"))
            .Bind(content => IO.WriteLine($"   [FILE] Read: {content}"))
            .Bind(_ => IO.WriteLine("   [FILE] Closing file..."))
            .Map(_ => Unit.Default);
    }
}

