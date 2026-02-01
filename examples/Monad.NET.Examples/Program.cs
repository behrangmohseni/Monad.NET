// Monad.NET Examples
// 
// This project demonstrates the core features and patterns of Monad.NET.
// Run this application to see live examples of each monad type.
//
// For more documentation, see: https://github.com/behrangmohseni/Monad.NET

using Monad.NET.Examples.Examples;

var runner = new ExampleRunner();
runner.Run(args);

/// <summary>
/// Handles running examples interactively or from command line arguments.
/// </summary>
internal class ExampleRunner
{
    private readonly (string Name, Action Run)[] _examples =
    [
        ("Option<T>", OptionExamples.Run),
        ("Result<T, E>", ResultExamples.Run),
        ("Validation<T, E>", ValidationExamples.Run),
        ("Try<T>", TryExamples.Run),
        ("RemoteData<T, E>", RemoteDataExamples.Run),
        ("NonEmptyList<T>", NonEmptyListExamples.Run),
        ("Writer<W, T>", WriterExamples.Run),
        ("Reader<R, A>", ReaderExamples.Run),
        ("IO<T>", IOExamples.Run),
        ("LINQ Integration", LinqExamples.Run),
        ("ErrorUnion (Source Gen)", ErrorUnionExamples.Run),
        ("Real-World Patterns", RealWorldExamples.Run),
    ];

    public void Run(string[] args)
    {
        PrintHeader();

        if (TryRunFromArgs(args))
            return;

        RunInteractiveMenu();
    }

    private static void PrintHeader()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    Monad.NET Examples                          ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    private bool TryRunFromArgs(string[] args)
    {
        if (args.Length > 0 && int.TryParse(args[0], out var index) && index >= 1 && index <= _examples.Length)
        {
            RunExample(index - 1);
            return true;
        }
        return false;
    }

    private void RunInteractiveMenu()
    {
        while (true)
        {
            DisplayMenu();
            var choice = GetUserChoice();

            if (choice == 0)
            {
                Console.WriteLine("\nGoodbye!");
                break;
            }

            HandleChoice(choice);
            Console.WriteLine();
        }
    }

    private void DisplayMenu()
    {
        Console.WriteLine("Select an example to run:\n");

        for (int i = 0; i < _examples.Length; i++)
        {
            Console.WriteLine($"  {i + 1,2}. {_examples[i].Name}");
        }

        Console.WriteLine($"  {_examples.Length + 1,2}. Run All Examples");
        Console.WriteLine($"   0. Exit\n");
        Console.Write("Enter choice: ");
    }

    private static int GetUserChoice()
    {
        var input = Console.ReadLine();
        if (int.TryParse(input, out var choice))
            return choice;

        Console.WriteLine("\nInvalid input. Please enter a number.\n");
        return -1;
    }

    private void HandleChoice(int choice)
    {
        if (choice == -1)
            return;

        if (choice == _examples.Length + 1)
        {
            RunAllExamples();
        }
        else if (choice >= 1 && choice <= _examples.Length)
        {
            RunExample(choice - 1);
        }
        else
        {
            Console.WriteLine("\nInvalid choice. Please try again.\n");
        }
    }

    private void RunAllExamples()
    {
        for (int i = 0; i < _examples.Length; i++)
        {
            RunExample(i);
            if (i < _examples.Length - 1)
            {
                Console.WriteLine("\nPress Enter to continue to the next example...");
                Console.ReadLine();
            }
        }
    }

    private void RunExample(int idx)
    {
        Console.WriteLine();
        Console.WriteLine($"═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  {_examples[idx].Name}");
        Console.WriteLine($"═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        try
        {
            _examples[idx].Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running example: {ex.Message}");
        }

        Console.WriteLine();
    }
}
