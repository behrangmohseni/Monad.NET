namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Option&lt;T&gt; for explicit null handling.
/// Option represents a value that may or may not exist.
/// </summary>
public static class OptionExamples
{
    public static void Run()
    {
        Console.WriteLine("Option<T> provides explicit null handling - no more NullReferenceExceptions!\n");
        
        // Creating Options
        Console.WriteLine("1. Creating Options:");
        var some = Option<int>.Some(42);
        var none = Option<int>.None();
        Console.WriteLine($"   Some(42): {some}");
        Console.WriteLine($"   None:     {none}");
        Console.WriteLine($"   some.IsSome: {some.IsSome}");
        Console.WriteLine($"   none.IsNone: {none.IsNone}");

        // From nullable values
        Console.WriteLine("\n2. From Nullable Values:");
        string? nullString = null;
        string? validString = "Hello";
        Console.WriteLine($"   null.ToOption(): {nullString.ToOption()}");
        Console.WriteLine($"   \"Hello\".ToOption(): {validString.ToOption()}");

        // Pattern matching with Match
        Console.WriteLine("\n3. Pattern Matching with Match:");
        var message = some.Match(
            someFunc: value => $"Got value: {value}",
            noneFunc: () => "No value present"
        );
        Console.WriteLine($"   Result: {message}");

        // Transforming with Map
        Console.WriteLine("\n4. Transforming with Map:");
        var doubled = some.Map(x => x * 2);
        var noneDoubled = none.Map(x => x * 2);
        Console.WriteLine($"   Some(42).Map(x => x * 2): {doubled}");
        Console.WriteLine($"   None.Map(x => x * 2):     {noneDoubled}");

        // Filtering values
        Console.WriteLine("\n5. Filtering:");
        var filtered = some.Filter(x => x > 50);
        var kept = some.Filter(x => x < 50);
        Console.WriteLine($"   Some(42).Filter(x => x > 50): {filtered}");
        Console.WriteLine($"   Some(42).Filter(x => x < 50): {kept}");

        // Chaining with AndThen (FlatMap)
        Console.WriteLine("\n6. Chaining with AndThen:");
        Option<string> GetEmailDomain(string email) =>
            email.Contains('@') 
                ? Option<string>.Some(email.Split('@')[1])
                : Option<string>.None();

        var email = Option<string>.Some("user@example.com");
        var domain = email.AndThen(GetEmailDomain);
        Console.WriteLine($"   Email: {email}");
        Console.WriteLine($"   Domain: {domain}");

        // Default values with UnwrapOr
        Console.WriteLine("\n7. Default Values:");
        Console.WriteLine($"   Some(42).UnwrapOr(0): {some.UnwrapOr(0)}");
        Console.WriteLine($"   None.UnwrapOr(0):     {none.UnwrapOr(0)}");
        Console.WriteLine($"   None.UnwrapOrElse(() => GetDefault()): {none.UnwrapOrElse(() => 100)}");

        // Method chaining
        Console.WriteLine("\n8. Method Chaining:");
        var result = Option<string>.Some("  Hello World  ")
            .Map(s => s.Trim())
            .Filter(s => s.Length > 5)
            .Map(s => s.ToUpper())
            .UnwrapOr("DEFAULT");
        Console.WriteLine($"   Result: {result}");

        // Combining Options with Zip
        Console.WriteLine("\n9. Combining with Zip:");
        var firstName = Option<string>.Some("John");
        var lastName = Option<string>.Some("Doe");
        var fullName = firstName.Zip(lastName).Map(t => $"{t.Item1} {t.Item2}");
        Console.WriteLine($"   Combined: {fullName}");

        // Convert to Result
        Console.WriteLine("\n10. Converting to Result:");
        var asResult = some.OkOr("Value was missing");
        var noneAsResult = none.OkOr("Value was missing");
        Console.WriteLine($"   Some.OkOr(...): {asResult}");
        Console.WriteLine($"   None.OkOr(...): {noneAsResult}");
    }
}

