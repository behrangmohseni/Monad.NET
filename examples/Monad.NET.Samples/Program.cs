using Monad.NET;

// Option: explicit null handling
var maybeName = Option<string>.Some("Ada");
var greeting = maybeName.Match(
    some: name => $"Hello, {name}!",
    none: () => "Hello, guest!");
Console.WriteLine(greeting);

// Result: typed errors
Result<int, string> ParseInt(string s) =>
    int.TryParse(s, out var value)
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Err($"Invalid int: '{s}'");

var total = from a in ParseInt("10")
            from b in ParseInt("32")
            select a + b;
Console.WriteLine(total.Match(
    ok => $"Sum = {ok}",
    err => $"Error: {err}"));

// Validation: accumulate errors
Validation<string, string> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name)
        ? Validation<string, string>.Invalid("Name required")
        : Validation<string, string>.Valid(name);

Validation<int, string> ValidateAge(int age) =>
    age < 0 ? Validation<int, string>.Invalid("Age must be positive")
            : Validation<int, string>.Valid(age);

var userValidation = from name in ValidateName("Ada")
                     from age in ValidateAge(32)
                     select (name, age);
Console.WriteLine(userValidation.Match(
    valid => $"User: {valid.name}, Age: {valid.age}",
    invalid => $"Validation errors: {string.Join(", ", invalid)}"));

// Writer: accumulate logs with results
var computation =
    from x in Writer<string, int>.Tell(10, "Start 10\n")
    from y in Writer<string, int>.Tell(x * 2, "Double\n")
    select y + 1;
Console.WriteLine($"Writer result: {computation.Value}");
Console.WriteLine($"Writer log:\n{computation.Log}");

// RemoteData: model async load states
RemoteData<int, string> data = RemoteData<int, string>.Loading();
data.Match(
    notAsked: () => Console.WriteLine("Not asked"),
    loading: () => Console.WriteLine("Loading..."),
    success: d => Console.WriteLine($"Data: {d}"),
    failure: e => Console.WriteLine($"Error: {e}"));

// IO: defer side effects
var program =
    from _1 in IO.WriteLine("Enter your name:")
    from name in IO.ReadLine()
    from _2 in IO.WriteLine($"Hello, {name}!")
    select Unit.Default;
// Uncomment to run interactively:
// program.Run();

