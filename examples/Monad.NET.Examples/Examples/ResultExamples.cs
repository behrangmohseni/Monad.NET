namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Result&lt;T, E&gt; for typed error handling.
/// Result represents either a success value or an error.
/// </summary>
public static class ResultExamples
{
    public static void Run()
    {
        Console.WriteLine("Result<T, E> provides typed error handling without exceptions!\n");

        // Creating Results
        Console.WriteLine("1. Creating Results:");
        var ok = Result<int, string>.Ok(42);
        var err = Result<int, string>.Err("Something went wrong");
        Console.WriteLine($"   Ok(42):  {ok}");
        Console.WriteLine($"   Err(...): {err}");

        // Safe division
        Console.WriteLine("\n2. Safe Division:");
        var success = Divide(10, 2);
        var failure = Divide(10, 0);
        Console.WriteLine($"   10 / 2 = {success}");
        Console.WriteLine($"   10 / 0 = {failure}");

        // Pattern matching
        Console.WriteLine("\n3. Pattern Matching:");
        success.Match(
            okAction: value => Console.WriteLine($"   Success: {value}"),
            errAction: error => Console.WriteLine($"   Error: {error}")
        );

        // Transforming with Map
        Console.WriteLine("\n4. Transforming with Map:");
        var doubled = success.Map(x => x * 2);
        var errDoubled = failure.Map(x => x * 2);
        Console.WriteLine($"   Ok(5).Map(x => x * 2): {doubled}");
        Console.WriteLine($"   Err.Map(x => x * 2):   {errDoubled}");

        // Chaining with AndThen
        Console.WriteLine("\n5. Chaining with AndThen:");
        var pipeline = Divide(20, 4)
            .AndThen(x => Divide((int)x, 2))
            .AndThen(x => Divide((int)x + 10, 3));
        Console.WriteLine($"   Pipeline result: {pipeline}");

        // Error propagation
        Console.WriteLine("\n6. Error Propagation:");
        var shortCircuit = Divide(10, 0)
            .AndThen(x => Divide(x, 2))  // Never executes
            .AndThen(x => Divide(x, 3)); // Never executes
        Console.WriteLine($"   Short-circuit: {shortCircuit}");

        // Error transformation with MapErr
        Console.WriteLine("\n7. Error Transformation:");
        var withHttpCode = failure.MapErr(msg => (Code: 500, Message: msg));
        Console.WriteLine($"   MapErr to HTTP: {withHttpCode}");

        // Recovery with OrElse
        Console.WriteLine("\n8. Recovery:");
        var recovered = failure.OrElse(err => Result<double, string>.Ok(-1));
        Console.WriteLine($"   Recovered from error: {recovered}");

        // Default values
        Console.WriteLine("\n9. Default Values:");
        Console.WriteLine($"   Ok(5).UnwrapOr(0): {success.UnwrapOr(0)}");
        Console.WriteLine($"   Err.UnwrapOr(0):   {failure.UnwrapOr(0)}");

        // From exceptions with Try
        Console.WriteLine("\n10. From Exceptions:");
        var parsed = ResultExtensions.Try(() => int.Parse("42"));
        var failed = ResultExtensions.Try(() => int.Parse("not-a-number"));
        Console.WriteLine($"   Parse '42':          {parsed}");
        Console.WriteLine($"   Parse 'not-a-number': IsErr = {failed.IsErr}");

        // Side effects with Tap
        Console.WriteLine("\n11. Side Effects with Tap:");
        _ = success.Tap(x => Console.WriteLine($"   Logging success: {x}"));
        _ = failure.TapErr(e => Console.WriteLine($"   Logging error: {e}"));
    }

    private static Result<double, string> Divide(double a, double b)
    {
        return b == 0
            ? Result<double, string>.Err("Division by zero")
            : Result<double, string>.Ok(a / b);
    }
}

