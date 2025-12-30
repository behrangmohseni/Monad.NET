// Monad.NET Examples
// 
// This project demonstrates the core features and patterns of Monad.NET.
// Run this application to see live examples of each monad type.
//
// For more documentation, see: https://github.com/behrangmohseni/Monad.NET

using Monad.NET.Examples;
using Monad.NET.Examples.Examples;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("                    Monad.NET Examples                          ");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();

var examples = new (string Name, Action Run)[]
{
    ("Option<T>", OptionExamples.Run),
    ("Result<T, E>", ResultExamples.Run),
    ("Either<L, R>", EitherExamples.Run),
    ("Validation<T, E>", ValidationExamples.Run),
    ("Try<T>", TryExamples.Run),
    ("RemoteData<T, E>", RemoteDataExamples.Run),
    ("NonEmptyList<T>", NonEmptyListExamples.Run),
    ("Writer<W, T>", WriterExamples.Run),
    ("Reader<R, A>", ReaderExamples.Run),
    ("IO<T>", IOExamples.Run),
    ("LINQ Integration", LinqExamples.Run),
    ("Async Operations", () => AsyncExamples.RunAsync().GetAwaiter().GetResult()),
    ("Collection Extensions", CollectionExamples.Run),
    ("ErrorUnion (Source Gen)", ErrorUnionExamples.Run),
    ("Real-World Patterns", RealWorldExamples.Run),
};

if (args.Length > 0 && int.TryParse(args[0], out var index) && index >= 1 && index <= examples.Length)
{
    // Run specific example from command line argument
    RunExample(index - 1);
}
else
{
    // Interactive menu
    while (true)
    {
        Console.WriteLine("Select an example to run:\n");

        for (int i = 0; i < examples.Length; i++)
        {
            Console.WriteLine($"  {i + 1,2}. {examples[i].Name}");
        }

        Console.WriteLine($"  {examples.Length + 1,2}. Run All Examples");
        Console.WriteLine($"   0. Exit\n");

        Console.Write("Enter choice: ");
        var input = Console.ReadLine();

        if (!int.TryParse(input, out var choice))
        {
            Console.WriteLine("\nInvalid input. Please enter a number.\n");
            continue;
        }

        if (choice == 0)
        {
            Console.WriteLine("\nGoodbye!");
            break;
        }

        if (choice == examples.Length + 1)
        {
            // Run all examples
            for (int i = 0; i < examples.Length; i++)
            {
                RunExample(i);
                if (i < examples.Length - 1)
                {
                    Console.WriteLine("\nPress Enter to continue to the next example...");
                    Console.ReadLine();
                }
            }
        }
        else if (choice >= 1 && choice <= examples.Length)
        {
            RunExample(choice - 1);
        }
        else
        {
            Console.WriteLine("\nInvalid choice. Please try again.\n");
        }

        Console.WriteLine();
    }
}

void RunExample(int idx)
{
    Console.WriteLine();
    Console.WriteLine($"═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {examples[idx].Name}");
    Console.WriteLine($"═══════════════════════════════════════════════════════════════");
    Console.WriteLine();

    try
    {
        examples[idx].Run();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running example: {ex.Message}");
    }

    Console.WriteLine();
}
