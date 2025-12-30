namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating LINQ query syntax with Monad.NET types.
/// All monads support Select (Map) and SelectMany (FlatMap) for LINQ integration.
/// </summary>
public static class LinqExamples
{
    public static void Run()
    {
        Console.WriteLine("LINQ query syntax works with all Monad.NET types!\n");
        
        // Option LINQ
        Console.WriteLine("1. Option LINQ:");
        var optionResult = from x in Option<int>.Some(10)
                           from y in Option<int>.Some(20)
                           where x > 5
                           select x + y;
        Console.WriteLine($"   from Some(10), Some(20) where x>5: {optionResult}");

        var optionNone = from x in Option<int>.Some(3)
                         from y in Option<int>.Some(20)
                         where x > 5  // Fails filter
                         select x + y;
        Console.WriteLine($"   from Some(3) where x>5: {optionNone}");

        // Result LINQ
        Console.WriteLine("\n2. Result LINQ:");
        var resultQuery = from x in Result<int, string>.Ok(10)
                          from y in Result<int, string>.Ok(20)
                          select x * y;
        Console.WriteLine($"   from Ok(10), Ok(20): {resultQuery}");

        var resultErr = from x in Result<int, string>.Ok(10)
                        from y in Result<int, string>.Err("Failed!")
                        select x * y;
        Console.WriteLine($"   from Ok(10), Err: {resultErr}");

        // Complex query with let
        Console.WriteLine("\n3. Query with 'let':");
        var complexQuery = from x in Option<int>.Some(5)
                           let doubled = x * 2
                           from y in Option<int>.Some(3)
                           let combined = doubled + y
                           select $"({x} * 2) + {y} = {combined}";
        Console.WriteLine($"   Result: {complexQuery}");

        // Try LINQ
        Console.WriteLine("\n4. Try LINQ:");
        var tryQuery = from x in Try<int>.Of(() => int.Parse("10"))
                       from y in Try<int>.Of(() => int.Parse("20"))
                       select x + y;
        Console.WriteLine($"   Parse \"10\" + \"20\": {tryQuery}");

        // Either LINQ
        Console.WriteLine("\n5. Either LINQ:");
        var eitherQuery = from x in Either<string, int>.Right(10)
                          from y in Either<string, int>.Right(20)
                          select x + y;
        Console.WriteLine($"   Right(10) + Right(20): {eitherQuery}");

        // Writer LINQ
        Console.WriteLine("\n6. Writer LINQ:");
        var writerQuery = from a in Writer<string, int>.Tell(10, "Got 10\n")
                          from b in Writer<string, int>.Tell(20, "Got 20\n")
                          select a + b;
        Console.WriteLine($"   Value: {writerQuery.Value}");
        Console.WriteLine($"   Log: {writerQuery.Log}");

        // Real-world: Parse and validate
        Console.WriteLine("\n7. Parse and Validate:");
        var userInput = new Dictionary<string, string>
        {
            ["name"] = "John",
            ["age"] = "25",
            ["email"] = "john@example.com"
        };

        var parsed = from name in GetField(userInput, "name")
                     from ageStr in GetField(userInput, "age")
                     from age in ParseInt(ageStr)
                     where age >= 18
                     from email in GetField(userInput, "email")
                     select new { Name = name, Age = age, Email = email };
        
        Console.WriteLine($"   Parsed: {parsed}");

        // Method syntax comparison
        Console.WriteLine("\n8. Method Syntax (equivalent):");
        var methodSyntax = Option<int>.Some(10)
            .SelectMany(x => Option<int>.Some(20), (x, y) => x + y)
            .Where(sum => sum > 20);
        Console.WriteLine($"   Method syntax result: {methodSyntax}");

        // Combining different operations
        Console.WriteLine("\n9. Mixing Operations:");
        var mixedResult = from value in Option<int>.Some(100)
                          let doubled = value * 2
                          from divisor in Option<int>.Some(4)
                          where divisor != 0
                          let result = doubled / divisor
                          select $"({value} * 2) / {divisor} = {result}";
        Console.WriteLine($"   Result: {mixedResult}");

        // Short-circuit demonstration
        Console.WriteLine("\n10. Short-Circuit Behavior:");
        var shortCircuit = from a in Option<int>.Some(1)
                           from b in Option<int>.None()  // Stops here
                           from c in Option<int>.Some(3)
                           select a + b + c;
        Console.WriteLine($"   With None in middle: {shortCircuit}");
    }

    private static Option<string> GetField(Dictionary<string, string> data, string field)
    {
        return data.TryGetValue(field, out var value) 
            ? Option<string>.Some(value) 
            : Option<string>.None();
    }

    private static Option<int> ParseInt(string value)
    {
        return int.TryParse(value, out var result) 
            ? Option<int>.Some(result) 
            : Option<int>.None();
    }
}

