using Monad.NET;

Console.WriteLine("=== Monad.NET Samples ===\n");

// ============================================
// Option: explicit null handling
// ============================================
Console.WriteLine("--- Option ---");

var maybeName = Option<string>.Some("Ada");

// Method syntax (recommended) - familiar to LINQ users
var upperName = maybeName
    .Select(name => name.ToUpper())           // Transform: Option<string> → Option<string>
    .Where(name => name.Length > 2);          // Filter: None if predicate fails

Console.WriteLine($"Upper name: {upperName.UnwrapOr("N/A")}");

// Chaining with SelectMany (FlatMap)
Option<string> GetEmail(string name) => 
    name == "Ada" ? Option<string>.Some("ada@example.com") : Option<string>.None();

var email = maybeName
    .SelectMany(name => GetEmail(name))       // Chain: Option<string> → Option<string>
    .Select(e => e.ToLower());

Console.WriteLine($"Email: {email.UnwrapOr("not found")}");

// Match for exhaustive handling
var greeting = maybeName.Match(
    some: name => $"Hello, {name}!",
    none: () => "Hello, guest!");
Console.WriteLine(greeting);

// ============================================
// Result: typed errors with LINQ
// ============================================
Console.WriteLine("\n--- Result ---");

Result<int, string> ParseInt(string s) =>
    int.TryParse(s, out var value)
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Err($"Invalid int: '{s}'");

// Method syntax - chain fallible operations
var calculated = ParseInt("10")
    .SelectMany(a => ParseInt("32").Select(b => a + b))  // Combine two Results
    .Select(sum => sum * 2);                              // Transform the success value

Console.WriteLine($"Calculated: {calculated.Match(ok => $"{ok}", err => $"Error: {err}")}");

// Query syntax - cleaner for multiple bindings
var total = from a in ParseInt("10")
            from b in ParseInt("32")
            select a + b;

Console.WriteLine($"Total (query syntax): {total.Match(ok => $"Sum = {ok}", err => $"Error: {err}")}");

// ============================================
// Try: capture exceptions
// ============================================
Console.WriteLine("\n--- Try ---");

var parsed = Try<int>.Of(() => int.Parse("42"))
    .Select(x => x * 2)                        // Transform if successful
    .Where(x => x > 0);                        // Filter with predicate

Console.WriteLine($"Parsed and doubled: {parsed.GetOrElse(-1)}");

var failed = Try<int>.Of(() => int.Parse("not-a-number"))
    .Select(x => x * 2)
    .Recover(ex => -1);                        // Recover from failure

Console.WriteLine($"Recovered from failure: {failed.Get()}");

// ============================================
// Validation: accumulate ALL errors
// ============================================
Console.WriteLine("\n--- Validation ---");

Validation<string, string> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name)
        ? Validation<string, string>.Invalid("Name required")
        : Validation<string, string>.Valid(name);

Validation<int, string> ValidateAge(int age) =>
    age < 0 ? Validation<int, string>.Invalid("Age must be positive")
            : Validation<int, string>.Valid(age);

// Note: Query syntax short-circuits. For accumulating errors, use Apply:
var userValidation = ValidateName("Ada")
    .Apply(ValidateAge(32), (name, age) => (name, age));

Console.WriteLine(userValidation.Match(
    valid => $"User: {valid.name}, Age: {valid.age}",
    invalid => $"Validation errors: {string.Join(", ", invalid)}"));

// Query syntax (short-circuits on first error - use Apply for accumulation)
var userQuery = from name in ValidateName("Ada")
                from age in ValidateAge(32)
                select (name, age);

Console.WriteLine($"Query validation: {userQuery.Match(v => $"{v.name}, {v.age}", e => string.Join(", ", e))}");

// ============================================
// Writer: accumulate logs with results
// ============================================
Console.WriteLine("\n--- Writer ---");

// Method syntax with SelectMany
var writerResult = Writer<string, int>.Tell(10, "Started with 10\n")
    .SelectMany(x => Writer<string, int>.Tell(x * 2, "Doubled\n"))
    .Select(x => x + 1);

Console.WriteLine($"Writer result: {writerResult.Value}");
Console.WriteLine($"Writer log:\n{writerResult.Log}");

// ============================================
// RemoteData: model async load states
// ============================================
Console.WriteLine("\n--- RemoteData ---");

RemoteData<int, string> data = RemoteData<int, string>.Loading();

// Method syntax - transform when successful
var transformed = data.Select(d => d * 2);

data.Match(
    notAsked: () => Console.WriteLine("Not asked"),
    loading: () => Console.WriteLine("Loading..."),
    success: d => Console.WriteLine($"Data: {d}"),
    failure: e => Console.WriteLine($"Error: {e}"));

// ============================================
// IO: defer side effects
// ============================================
Console.WriteLine("\n--- IO ---");

// Method syntax
var ioProgram = IO.WriteLine("Hello from IO!")
    .SelectMany(_ => IO.Pure(42))
    .Select(x => x * 2);

var ioResult = ioProgram.Run();
Console.WriteLine($"IO result: {ioResult}");

// Query syntax (for complex compositions)
var interactiveProgram =
    from _1 in IO.WriteLine("Enter your name:")
    from name in IO.ReadLine()
    from _2 in IO.WriteLine($"Hello, {name}!")
    select Unit.Default;

// Uncomment to run interactively:
// interactiveProgram.Run();

Console.WriteLine("\n=== Samples Complete ===");
