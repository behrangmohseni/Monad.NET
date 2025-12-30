namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating collection extensions for working with monads.
/// These methods help bridge collections and monadic types.
/// </summary>
public static class CollectionExamples
{
    public static void Run()
    {
        Console.WriteLine("Collection extensions bridge collections with monads.\n");

        // Sequence - all or nothing
        Console.WriteLine("1. Sequence (all Some -> Some of list):");
        var allSome = new[] { Option<int>.Some(1), Option<int>.Some(2), Option<int>.Some(3) };
        var sequenced = allSome.Sequence();
        Console.WriteLine($"   [Some(1), Some(2), Some(3)] -> {FormatOptionList(sequenced)}");

        var withNone = new[] { Option<int>.Some(1), Option<int>.None(), Option<int>.Some(3) };
        var sequencedNone = withNone.Sequence();
        Console.WriteLine($"   [Some(1), None, Some(3)] -> {FormatOptionList(sequencedNone)}");

        // Traverse - map and sequence
        Console.WriteLine("\n2. Traverse (map then sequence):");
        var strings = new[] { "1", "2", "3" };
        var traversed = strings.Traverse(s =>
            int.TryParse(s, out var v) ? Option<int>.Some(v) : Option<int>.None());
        Console.WriteLine($"   [\"1\", \"2\", \"3\"] -> {FormatOptionList(traversed)}");

        var withInvalid = new[] { "1", "not-a-number", "3" };
        var traversedInvalid = withInvalid.Traverse(s =>
            int.TryParse(s, out var v) ? Option<int>.Some(v) : Option<int>.None());
        Console.WriteLine($"   [\"1\", \"not-a-number\", \"3\"] -> {FormatOptionList(traversedInvalid)}");

        // Choose - filter and unwrap
        Console.WriteLine("\n3. Choose (filter Somes, unwrap values):");
        var mixed = new[] { Option<int>.Some(1), Option<int>.None(), Option<int>.Some(3), Option<int>.None(), Option<int>.Some(5) };
        var chosen = mixed.Choose().ToList();
        Console.WriteLine($"   [Some(1), None, Some(3), None, Some(5)] -> [{string.Join(", ", chosen)}]");

        // Result Sequence
        Console.WriteLine("\n4. Result Sequence:");
        var allOk = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };
        var resultSeq = allOk.Sequence();
        Console.WriteLine($"   [Ok(1), Ok(2), Ok(3)] -> {resultSeq}");

        var withErr = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("Failed!"),
            Result<int, string>.Ok(3)
        };
        var resultSeqErr = withErr.Sequence();
        Console.WriteLine($"   [Ok(1), Err, Ok(3)] -> {resultSeqErr}");

        // Result Traverse
        Console.WriteLine("\n5. Result Traverse:");
        var items = new[] { "10", "20", "30" };
        var resultTraverse = items.Traverse(s =>
            int.TryParse(s, out var v)
                ? Result<int, string>.Ok(v)
                : Result<int, string>.Err($"Invalid: {s}"));
        Console.WriteLine($"   [\"10\", \"20\", \"30\"] -> {resultTraverse}");

        // Partition Results
        Console.WriteLine("\n6. Partition (separate Oks and Errs):");
        var mixedResults = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("Error A"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("Error B"),
            Result<int, string>.Ok(3)
        };
        var (oks, errors) = mixedResults.Partition();
        Console.WriteLine($"   Oks:    [{string.Join(", ", oks)}]");
        Console.WriteLine($"   Errors: [{string.Join(", ", errors)}]");

        // FirstSome - find first Some value
        Console.WriteLine("\n7. FirstSome:");
        var options = new[] { Option<int>.None(), Option<int>.None(), Option<int>.Some(42), Option<int>.Some(100) };
        var first = options.FirstSome();
        Console.WriteLine($"   [None, None, Some(42), Some(100)] -> {first}");

        // Aggregate with Option
        Console.WriteLine("\n8. Aggregate with Option:");
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var maybeSum = numbers.Select(n => Option<int>.Some(n)).Aggregate(
            Option<int>.Some(0),
            (acc, opt) => acc.Zip(opt).Map(t => t.Item1 + t.Item2)
        );
        Console.WriteLine($"   Sum of [1,2,3,4,5]: {maybeSum}");

        // Real-world: Batch processing
        Console.WriteLine("\n9. Real-World: Batch Processing:");
        var userIds = new[] { "1", "2", "invalid", "4" };
        var (validUsers, invalidIds) = ProcessUsers(userIds);
        Console.WriteLine($"   Valid users: [{string.Join(", ", validUsers)}]");
        Console.WriteLine($"   Invalid IDs: [{string.Join(", ", invalidIds)}]");

        // Real-world: Calculate total with validation
        Console.WriteLine("\n10. Real-World: Validated Calculation:");
        var prices = new[] { "10.99", "25.50", "15.00" };
        var total = prices
            .Traverse(p => decimal.TryParse(p, out var v)
                ? Result<decimal, string>.Ok(v)
                : Result<decimal, string>.Err($"Invalid price: {p}"))
            .Map(values => values.Sum());
        Console.WriteLine($"   Total: {total}");
    }

    private static string FormatOptionList(Option<IReadOnlyList<int>> opt)
    {
        return opt.Match(
            someFunc: vals => $"Some([{string.Join(", ", vals)}])",
            noneFunc: () => "None"
        );
    }

    private static (IReadOnlyList<string> ValidUsers, IReadOnlyList<string> InvalidIds) ProcessUsers(string[] userIds)
    {
        var results = userIds.Select(id =>
            int.TryParse(id, out var numId) && numId > 0
                ? Result<string, string>.Ok($"User-{numId}")
                : Result<string, string>.Err(id));

        return results.Partition();
    }
}

