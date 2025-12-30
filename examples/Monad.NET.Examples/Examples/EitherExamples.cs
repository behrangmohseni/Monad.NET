namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Either&lt;L, R&gt; for representing one of two possible values.
/// Unlike Result, Either treats both sides equally - neither is inherently "error" or "success".
/// </summary>
public static class EitherExamples
{
    public static void Run()
    {
        Console.WriteLine("Either<L, R> represents one of two possible values.\n");
        
        // Creating Eithers
        Console.WriteLine("1. Creating Eithers:");
        var right = Either<string, int>.Right(42);
        var left = Either<string, int>.Left("fallback");
        Console.WriteLine($"   Right(42):     {right}");
        Console.WriteLine($"   Left(...):     {left}");
        Console.WriteLine($"   right.IsRight: {right.IsRight}");
        Console.WriteLine($"   left.IsLeft:   {left.IsLeft}");

        // Pattern matching
        Console.WriteLine("\n2. Pattern Matching:");
        var message = right.Match(
            leftFunc: l => $"Left: {l}",
            rightFunc: r => $"Right: {r}"
        );
        Console.WriteLine($"   Match result: {message}");

        // MapRight transforms Right value
        Console.WriteLine("\n3. MapRight (transforms Right):");
        var doubled = right.MapRight(x => x * 2);
        var leftMapped = left.MapRight(x => x * 2);
        Console.WriteLine($"   Right(42).MapRight(x * 2): {doubled}");
        Console.WriteLine($"   Left.MapRight(x * 2):      {leftMapped}");

        // MapLeft transforms Left value
        Console.WriteLine("\n4. MapLeft (transforms Left):");
        var leftTransformed = left.MapLeft(s => s.ToUpper());
        Console.WriteLine($"   Left.MapLeft(ToUpper): {leftTransformed}");

        // BiMap transforms both sides
        Console.WriteLine("\n5. BiMap (transform both sides):");
        var biMapped = right.BiMap(
            leftMapper: s => s.Length,
            rightMapper: n => n.ToString()
        );
        Console.WriteLine($"   BiMap result: {biMapped}");

        // Swap sides
        Console.WriteLine("\n6. Swap:");
        var swapped = right.Swap();
        Console.WriteLine($"   Right(42).Swap(): {swapped}");

        // Chaining with AndThen
        Console.WriteLine("\n7. Chaining:");
        var chained = right
            .AndThen(x => x > 10 
                ? Either<string, int>.Right(x * 2) 
                : Either<string, int>.Left("too small"));
        Console.WriteLine($"   Chained: {chained}");

        // Real-world: Cache hit or DB fetch
        Console.WriteLine("\n8. Real-World: Cache vs Database:");
        var cached = Either<FreshData, CachedData>.Right(new CachedData("cached result", DateTime.Now.AddMinutes(-5)));
        var fromDb = Either<FreshData, CachedData>.Left(new FreshData("fresh from DB"));
        
        string GetValue(Either<FreshData, CachedData> data) => data.Match(
            leftFunc: fresh => $"Fresh: {fresh.Value}",
            rightFunc: cache => $"Cached ({(DateTime.Now - cache.CachedAt).Minutes}m ago): {cache.Value}"
        );
        
        Console.WriteLine($"   Cached: {GetValue(cached)}");
        Console.WriteLine($"   FromDB: {GetValue(fromDb)}");

        // Convert to Result
        Console.WriteLine("\n9. Convert to Result:");
        var asResult = right.ToResult();
        Console.WriteLine($"   Right.ToResult(): {asResult}");

        // Convert to Option
        Console.WriteLine("\n10. Convert to Option:");
        var rightOption = right.Match(leftFunc: _ => Option<int>.None(), rightFunc: v => Option<int>.Some(v));
        var leftOption = left.Match(leftFunc: _ => Option<int>.None(), rightFunc: v => Option<int>.Some(v));
        Console.WriteLine($"   Right to Option: {rightOption}");
        Console.WriteLine($"   Left to Option:  {leftOption}");
    }

    record CachedData(string Value, DateTime CachedAt);
    record FreshData(string Value);
}

