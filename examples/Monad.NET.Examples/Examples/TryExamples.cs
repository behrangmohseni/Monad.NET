namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Try&lt;T&gt; for capturing exceptions.
/// Try wraps operations that might throw, making error handling explicit.
/// </summary>
public static class TryExamples
{
    public static void Run()
    {
        Console.WriteLine("Try<T> captures exceptions and makes error handling explicit.\n");

        // Creating Try values
        Console.WriteLine("1. Creating Try Values:");
        var success = Try<int>.Success(42);
        var failure = Try<int>.Failure(new InvalidOperationException("Operation failed"));
        Console.WriteLine($"   Success(42): {success}");
        Console.WriteLine($"   Failure:     {failure}");

        // Using Of to capture exceptions
        Console.WriteLine("\n2. Capturing with Of:");
        var parsed = Try<int>.Of(() => int.Parse("123"));
        var failed = Try<int>.Of(() => int.Parse("not-a-number"));
        Console.WriteLine($"   Parse '123':          {parsed}");
        Console.WriteLine($"   Parse 'not-a-number': {failed}");

        // Safe operations
        Console.WriteLine("\n3. Safe Division:");
        var divOk = Try<double>.Of(() => 10.0 / 2);
        var divZero = Try<double>.Of(() => { int zero = 0; return 10 / zero; });
        Console.WriteLine($"   10 / 2 = {divOk}");
        Console.WriteLine($"   10 / 0 = {divZero}");

        // Chaining with Map
        Console.WriteLine("\n4. Chaining with Map:");
        var chain = Try<int>.Of(() => int.Parse("10"))
            .Map(x => x * 2)
            .Map(x => x + 5);
        Console.WriteLine($"   Parse + double + add 5: {chain}");

        // Chaining with Bind
        Console.WriteLine("\n5. Chaining with Bind:");
        var flatMapped = Try<string>.Of(() => "42")
            .Bind(s => Try<int>.Of(() => int.Parse(s)))
            .Bind(n => Try<double>.Of(() => n / 2.0));
        Console.WriteLine($"   \"42\" -> parse -> divide: {flatMapped}");

        // Recovery
        Console.WriteLine("\n6. Recovery from Failure:");
        var recovered = failed.Recover(ex => -1);
        Console.WriteLine($"   Recover with default: {recovered}");

        var recoveredWith = failed.RecoverWith(ex =>
            ex is FormatException
                ? Try<int>.Success(0)
                : Try<int>.Failure(ex));
        Console.WriteLine($"   RecoverWith: {recoveredWith}");

        // GetValueOr
        Console.WriteLine("\n7. Default Values:");
        Console.WriteLine($"   Success.GetValueOr(0):    {success.GetValueOr(0)}");
        Console.WriteLine($"   Failure.GetValueOr(0):    {failed.GetValueOr(0)}");
        Console.WriteLine($"   Failure.Match(ok, fn):    {failed.Match(ok => ok, _ => DateTime.Now.Second)}");

        // Filter
        Console.WriteLine("\n8. Filtering:");
        var filtered = Try<int>.Success(42).Filter(x => x > 50, "Must be > 50");
        var kept = Try<int>.Success(42).Filter(x => x < 50, "Must be < 50");
        Console.WriteLine($"   Filter(42 > 50): {filtered}");
        Console.WriteLine($"   Filter(42 < 50): {kept}");

        // Pattern matching
        Console.WriteLine("\n9. Pattern Matching:");
        parsed.Match(
            successAction: value => Console.WriteLine($"   Parsed: {value}"),
            failureAction: ex => Console.WriteLine($"   Failed: {ex.Message}")
        );

        // Convert to Result
        Console.WriteLine("\n10. Convert to Result:");
        var asResult = parsed.ToResult(ex => $"Parse error: {ex.Message}");
        Console.WriteLine($"   As Result: {asResult}");

        // Convert to Option
        Console.WriteLine("\n11. Convert to Option:");
        var asOption = parsed.ToOption();
        var failedOption = failed.ToOption();
        Console.WriteLine($"   Success.ToOption(): {asOption}");
        Console.WriteLine($"   Failure.ToOption(): {failedOption}");

        // Retry pattern
        Console.WriteLine("\n12. Retry Pattern:");
        var attempt = 0;
        var retried = RetryWithBackoff(() =>
        {
            attempt++;
            if (attempt < 3)
                throw new TimeoutException("Timeout");
            return "Success on attempt " + attempt;
        }, maxRetries: 5);
        Console.WriteLine($"   Retry result: {retried}");
    }

    private static Try<T> RetryWithBackoff<T>(Func<T> operation, int maxRetries = 3)
    {
        if (maxRetries <= 0)
            return Try<T>.Of(operation);

        Try<T> lastResult = default!;
        for (int i = 0; i < maxRetries; i++)
        {
            lastResult = Try<T>.Of(operation);
            if (lastResult.IsOk)
                return lastResult;
            if (i < maxRetries - 1)
                Thread.Sleep((int)Math.Pow(2, i) * 50);
        }
        return lastResult;
    }
}

