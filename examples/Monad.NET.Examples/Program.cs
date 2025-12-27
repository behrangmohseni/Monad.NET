using Monad.NET;

namespace Monad.NET.Examples;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Monad.NET Examples ===\n");

        OptionExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        ResultExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        EitherExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        ValidationExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        TryExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        RemoteDataExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        NonEmptyListExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        WriterExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        ReaderExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        LinqExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        AsyncExamples().GetAwaiter().GetResult();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        CollectionExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        ErrorUnionExamples();
        Console.WriteLine("\n" + new string('-', 50) + "\n");

        RealWorldExamples();
    }

    static void OptionExamples()
    {
        Console.WriteLine("### Option<T> Examples ###\n");

        // Creating Options
        var some = Option<int>.Some(42);
        var none = Option<int>.None();

        Console.WriteLine($"some.IsSome: {some.IsSome}"); // True
        Console.WriteLine($"none.IsNone: {none.IsNone}"); // True

        // Pattern matching
        var message = some.Match(
            someFunc: value => $"Got: {value}",
            noneFunc: () => "Got nothing"
        );
        Console.WriteLine($"Match result: {message}");

        // Mapping
        var doubled = some.Map(x => x * 2);
        Console.WriteLine($"Doubled: {doubled}"); // Some(84)

        // Chaining
        var result = some
            .Filter(x => x > 40)
            .Map(x => x.ToString())
            .UnwrapOr("default");
        Console.WriteLine($"Chained result: {result}"); // "42"

        // Nullable conversion
        string? nullableString = null;
        var optionFromNull = nullableString.ToOption();
        Console.WriteLine($"Option from null: {optionFromNull}"); // None

        int? nullableInt = 100;
        var optionFromValue = nullableInt.ToOption();
        Console.WriteLine($"Option from value: {optionFromValue}"); // Some(100)
    }

    static void ResultExamples()
    {
        Console.WriteLine("### Result<T, E> Examples ###\n");

        // Safe division
        var divideSuccess = Divide(10, 2);
        var divideError = Divide(10, 0);

        Console.WriteLine($"10 / 2 = {divideSuccess}"); // Ok(5)
        Console.WriteLine($"10 / 0 = {divideError}"); // Err(Cannot divide by zero)

        // Pattern matching
        divideSuccess.Match(
            okAction: value => Console.WriteLine($"Success: {value}"),
            errAction: error => Console.WriteLine($"Error: {error}")
        );

        // Chaining operations
        var pipeline = Divide(20, 4)
            .Map(x => x * 2)
            .AndThen(x => x > 5
                ? Result<string, string>.Ok($"Large: {x}")
                : Result<string, string>.Err("Too small"))
            .Match(
                okFunc: msg => msg,
                errFunc: err => $"Failed: {err}"
            );
        Console.WriteLine($"Pipeline result: {pipeline}");

        // Exception handling with Try
        var parseSuccess = ResultExtensions.Try(() => int.Parse("42"));
        var parseError = ResultExtensions.Try(() => int.Parse("not a number"));

        Console.WriteLine($"Parse '42': {parseSuccess}"); // Ok(42)
        Console.WriteLine($"Parse 'not a number': {parseError.IsErr}"); // True
    }

    static void EitherExamples()
    {
        Console.WriteLine("### Either<L, R> Examples ###\n");

        var right = Either<string, int>.Right(42);
        var left = Either<string, int>.Left("error");

        Console.WriteLine($"right.IsRight: {right.IsRight}"); // True
        Console.WriteLine($"left.IsLeft: {left.IsLeft}"); // True

        // Pattern matching
        var rightMsg = right.Match(
            leftFunc: error => $"Error: {error}",
            rightFunc: value => $"Success: {value}"
        );
        Console.WriteLine($"Right match: {rightMsg}");

        // BiMap - transform both sides
        var biMapped = right.BiMap(
            leftMapper: e => e.ToUpper(),
            rightMapper: v => v * 2
        );
        Console.WriteLine($"BiMapped: {biMapped}"); // Right(84)

        // Swap
        var swapped = right.Swap();
        Console.WriteLine($"Swapped: {swapped}"); // Left(42)

        // Convert to Result
        var asResult = right.ToResult();
        Console.WriteLine($"As Result: {asResult}"); // Ok(42)
    }

    static void RealWorldExamples()
    {
        Console.WriteLine("### Real-World Examples ###\n");

        // Example 1: Configuration parsing
        Console.WriteLine("1. Configuration Parsing:");
        var timeout = GetConfigValue("Timeout")
            .Filter(x => x > 0)
            .UnwrapOr(30);
        Console.WriteLine($"   Timeout: {timeout}");

        // Example 2: User validation
        Console.WriteLine("\n2. User Validation:");
        var validUser = ValidateAndCreateUser("john@example.com", 25);
        var invalidUser = ValidateAndCreateUser("invalid-email", 15);

        validUser.Match(
            okAction: user => Console.WriteLine($"   Created user: {user.Email}, Age: {user.Age}"),
            errAction: errors => Console.WriteLine($"   Validation failed: {string.Join(", ", errors)}")
        );

        invalidUser.Match(
            okAction: user => Console.WriteLine($"   Created user: {user.Email}"),
            errAction: errors => Console.WriteLine($"   Validation failed: {string.Join(", ", errors)}")
        );

        // Example 3: Chaining operations with error recovery
        Console.WriteLine("\n3. Operation Chain with Recovery:");
        var processResult = ProcessData("valid-input")
            .OrElse(err => Result<string, string>.Ok("recovered"))
            .Tap(result => Console.WriteLine($"   Processed: {result}"));

        // Example 4: Optional parameters
        Console.WriteLine("\n4. Optional Parameters:");
        var user = new User { Email = "test@example.com", MiddleName = "James" };
        var displayName = GetDisplayName(user);
        Console.WriteLine($"   Display name: {displayName}");
    }

    // Helper methods
    static Result<double, string> Divide(double numerator, double denominator)
    {
        if (denominator == 0)
            return Result<double, string>.Err("Cannot divide by zero");

        return Result<double, string>.Ok(numerator / denominator);
    }

    static Option<int> GetConfigValue(string key)
    {
        // Simulate configuration lookup
        var configs = new Dictionary<string, string>
        {
            { "MaxRetries", "3" },
            { "BufferSize", "1024" }
        };

        if (configs.TryGetValue(key, out var value) && int.TryParse(value, out var parsed))
            return Option<int>.Some(parsed);

        return Option<int>.None();
    }

    static Result<User, List<string>> ValidateAndCreateUser(string email, int age)
    {
        var errors = new List<string>();

        if (!email.Contains('@'))
            errors.Add("Invalid email format");

        if (age < 18)
            errors.Add("Must be 18 or older");

        if (errors.Count > 0)
            return Result<User, List<string>>.Err(errors);

        return Result<User, List<string>>.Ok(new User
        {
            Email = email,
            Age = age
        });
    }

    static Result<string, string> ProcessData(string input)
    {
        if (input == "valid-input")
            return Result<string, string>.Ok("Success!");

        return Result<string, string>.Err("Invalid input");
    }

    static string GetDisplayName(User user)
    {
        return user.MiddleName.ToOption()
            .Map(middle => $"{user.FirstName} {middle} {user.LastName}")
            .UnwrapOr($"{user.FirstName} {user.LastName}");
    }

    static void ValidationExamples()
    {
        Console.WriteLine("### Validation<T, E> Examples ###\n");
        Console.WriteLine("Validation accumulates ALL errors instead of stopping at the first one.\n");

        // Basic validation
        Console.WriteLine("1. Basic Validation:");
        var valid = Validation<int, string>.Valid(42);
        var invalid = Validation<int, string>.Invalid("Value is required");
        Console.WriteLine($"   Valid: {valid}");
        Console.WriteLine($"   Invalid: {invalid}");

        // Validation with multiple errors
        Console.WriteLine("\n2. Multiple Errors:");
        var multipleErrors = Validation<string, string>.Invalid(new[] { "Name required", "Email required", "Age must be positive" });
        Console.WriteLine($"   Errors: {multipleErrors}");

        // Form validation example
        Console.WriteLine("\n3. Form Validation (Accumulating Errors):");
        var nameValidation = ValidateName("");
        var emailValidation = ValidateEmail("invalid-email");
        var ageValidation = ValidateAge(-5);

        Console.WriteLine($"   Name: {nameValidation}");
        Console.WriteLine($"   Email: {emailValidation}");
        Console.WriteLine($"   Age: {ageValidation}");

        // Combining validations with Apply
        Console.WriteLine("\n4. Combining Validations with Apply:");
        var goodName = ValidateName("John");
        var goodEmail = ValidateEmail("john@example.com");
        var goodAge = ValidateAge(25);

        // All valid case - combine name and email first, then age
        var validUser = goodName
            .Apply(goodEmail, (name, email) => (Name: name, Email: email))
            .Apply(goodAge, (partial, age) => new UserDto(partial.Name, partial.Email, age));
        Console.WriteLine($"   Valid User: {validUser}");

        // Some invalid case - errors accumulate!
        var invalidUser = nameValidation
            .Apply(emailValidation, (name, email) => (Name: name, Email: email))
            .Apply(ageValidation, (partial, age) => new UserDto(partial.Name, partial.Email, age));
        Console.WriteLine($"   Invalid User: {invalidUser}");

        // Match for pattern matching
        Console.WriteLine("\n5. Pattern Matching:");
        validUser.Match(
            validAction: user => Console.WriteLine($"   Created: {user.Name}, {user.Email}, Age {user.Age}"),
            invalidAction: errors => Console.WriteLine($"   Errors: {string.Join("; ", errors)}")
        );

        // Map and FlatMap
        Console.WriteLine("\n6. Map and FlatMap:");
        var mapped = Validation<int, string>.Valid(10)
            .Map(x => x * 2)
            .Map(x => $"Result: {x}");
        Console.WriteLine($"   Mapped: {mapped}");

        // Convert to Result or Option
        Console.WriteLine("\n7. Convert to Other Types:");
        var asResult = valid.ToResult();
        var asOption = valid.ToOption();
        Console.WriteLine($"   As Result: {asResult}");
        Console.WriteLine($"   As Option: {asOption}");
    }

    static Validation<string, string> ValidateName(string name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? Validation<string, string>.Invalid("Name is required")
            : Validation<string, string>.Valid(name);
    }

    static Validation<string, string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Validation<string, string>.Invalid("Email is required");
        if (!email.Contains('@'))
            return Validation<string, string>.Invalid("Email must contain @");
        return Validation<string, string>.Valid(email);
    }

    static Validation<int, string> ValidateAge(int age)
    {
        return age < 0
            ? Validation<int, string>.Invalid("Age must be non-negative")
            : age < 18
                ? Validation<int, string>.Invalid("Must be 18 or older")
                : Validation<int, string>.Valid(age);
    }

    static void TryExamples()
    {
        Console.WriteLine("### Try<T> Examples ###\n");
        Console.WriteLine("Try captures exceptions and makes error handling explicit.\n");

        // Creating Try values
        Console.WriteLine("1. Creating Try Values:");
        var success = Try<int>.Success(42);
        var failure = Try<int>.Failure(new InvalidOperationException("Something went wrong"));
        Console.WriteLine($"   Success: {success}");
        Console.WriteLine($"   Failure: {failure}");

        // Using Of to capture exceptions
        Console.WriteLine("\n2. Capturing Exceptions with Of:");
        var parsed = Try<int>.Of(() => int.Parse("123"));
        var parseFailed = Try<int>.Of(() => int.Parse("not a number"));
        Console.WriteLine($"   Parse '123': {parsed}");
        Console.WriteLine($"   Parse 'not a number': {parseFailed}");

        // Safe division
        Console.WriteLine("\n3. Safe Division:");
        var divideOk = Try<double>.Of(() => 10.0 / 2);
        var divideByZero = Try<double>.Of(() =>
        {
            int zero = 0;
            return 10 / zero; // Will throw DivideByZeroException
        });
        Console.WriteLine($"   10 / 2 = {divideOk}");
        Console.WriteLine($"   10 / 0 = {divideByZero}");

        // Chaining with Map and FlatMap
        Console.WriteLine("\n4. Chaining Operations:");
        var chain = Try<int>.Of(() => int.Parse("10"))
            .Map(x => x * 2)
            .Map(x => x + 5)
            .FlatMap(x => Try<string>.Success($"Result: {x}"));
        Console.WriteLine($"   Chain result: {chain}");

        // Recovery
        Console.WriteLine("\n5. Recovery from Failure:");
        var recovered = Try<int>.Of(() => int.Parse("invalid"))
            .Recover(ex => 0);
        Console.WriteLine($"   Recovered: {recovered}");

        var recoveredWithTry = Try<int>.Of(() => int.Parse("invalid"))
            .RecoverWith(ex => Try<int>.Success(-1));
        Console.WriteLine($"   Recovered with Try: {recoveredWithTry}");

        // GetOrElse
        Console.WriteLine("\n6. GetOrElse:");
        var valueOrDefault = parseFailed.GetOrElse(100);
        var valueOrComputed = parseFailed.GetOrElse(() => DateTime.Now.Second);
        Console.WriteLine($"   GetOrElse(100): {valueOrDefault}");
        Console.WriteLine($"   GetOrElse(computed): {valueOrComputed}");

        // Filter
        Console.WriteLine("\n7. Filter:");
        var filtered = Try<int>.Success(42)
            .Filter(x => x > 50, "Value must be greater than 50");
        Console.WriteLine($"   Filter(42 > 50): {filtered}");

        // Pattern matching
        Console.WriteLine("\n8. Pattern Matching:");
        parsed.Match(
            successAction: value => Console.WriteLine($"   Success: {value}"),
            failureAction: ex => Console.WriteLine($"   Failure: {ex.Message}")
        );

        // Convert to Result
        Console.WriteLine("\n9. Convert to Result:");
        var asResult = parsed.ToResult(ex => $"Parse error: {ex.Message}");
        Console.WriteLine($"   As Result: {asResult}");

        // Real-world: JSON parsing simulation
        Console.WriteLine("\n10. Real-World: Safe JSON Parsing:");
        var jsonResult = SafeParseJson("{\"name\": \"John\"}");
        var badJsonResult = SafeParseJson("not json");
        Console.WriteLine($"   Valid JSON: {jsonResult}");
        Console.WriteLine($"   Invalid JSON: {badJsonResult}");
    }

    static Try<string> SafeParseJson(string json)
    {
        return Try<string>.Of(() =>
        {
            if (!json.StartsWith('{'))
                throw new FormatException("Invalid JSON format");
            return $"Parsed: {json}";
        });
    }

    static void RemoteDataExamples()
    {
        Console.WriteLine("### RemoteData<T, E> Examples ###\n");
        Console.WriteLine("RemoteData tracks the state of async data: NotAsked, Loading, Success, Failure.\n");

        // Four states
        Console.WriteLine("1. The Four States:");
        var notAsked = RemoteData<string, string>.NotAsked();
        var loading = RemoteData<string, string>.Loading();
        var success = RemoteData<string, string>.Success("Data loaded!");
        var failure = RemoteData<string, string>.Failure("Network error");

        Console.WriteLine($"   NotAsked: {notAsked}");
        Console.WriteLine($"   Loading: {loading}");
        Console.WriteLine($"   Success: {success}");
        Console.WriteLine($"   Failure: {failure}");

        // State checking
        Console.WriteLine("\n2. State Checking:");
        Console.WriteLine($"   notAsked.IsNotAsked: {notAsked.IsNotAsked}");
        Console.WriteLine($"   loading.IsLoading: {loading.IsLoading}");
        Console.WriteLine($"   success.IsSuccess: {success.IsSuccess}");
        Console.WriteLine($"   failure.IsFailure: {failure.IsFailure}");

        // Pattern matching for UI rendering
        Console.WriteLine("\n3. UI Rendering Pattern:");
        RenderData(notAsked);
        RenderData(loading);
        RenderData(success);
        RenderData(failure);

        // Map only transforms Success
        Console.WriteLine("\n4. Map (only transforms Success):");
        var mappedSuccess = success.Map(s => s.ToUpper());
        var mappedLoading = loading.Map(s => s.ToUpper());
        Console.WriteLine($"   Success mapped: {mappedSuccess}");
        Console.WriteLine($"   Loading mapped: {mappedLoading}");

        // UnwrapOr for default values
        Console.WriteLine("\n5. UnwrapOr:");
        Console.WriteLine($"   Success.UnwrapOr: {success.UnwrapOr("default")}");
        Console.WriteLine($"   Loading.UnwrapOr: {loading.UnwrapOr("default")}");
        Console.WriteLine($"   NotAsked.UnwrapOr: {notAsked.UnwrapOr("default")}");

        // Simulated API call flow
        Console.WriteLine("\n6. Simulated API Call Flow:");
        SimulateApiCallFlow();

        // Combining RemoteData using Map and FlatMap
        Console.WriteLine("\n7. Combining Multiple RemoteData:");
        var userData = RemoteData<string, string>.Success("John");
        var postsData = RemoteData<int, string>.Success(42);

        // Combine using Map - if both are Success, combine them
        var combined = userData.Map(user => $"{user} has {postsData.UnwrapOr(0)} posts");
        Console.WriteLine($"   Combined: {combined}");

        // Convert to Result
        Console.WriteLine("\n8. Convert to Result:");
        var asResult = success.ToResult(
            notAskedError: "Data not requested",
            loadingError: "Still loading"
        );
        Console.WriteLine($"   As Result: {asResult}");
    }

    static void RenderData(RemoteData<string, string> data)
    {
        var ui = data.Match(
            notAskedFunc: () => "   [Button: Load Data]",
            loadingFunc: () => "   [Spinner: Loading...]",
            successFunc: d => $"   [Content: {d}]",
            failureFunc: e => $"   [Error: {e}] [Button: Retry]"
        );
        Console.WriteLine(ui);
    }

    static void SimulateApiCallFlow()
    {
        var state = RemoteData<string, string>.NotAsked();
        Console.WriteLine($"   Initial: {state}");

        state = RemoteData<string, string>.Loading();
        Console.WriteLine($"   After click: {state}");

        state = RemoteData<string, string>.Success("API Response Data");
        Console.WriteLine($"   After load: {state}");
    }

    static void NonEmptyListExamples()
    {
        Console.WriteLine("### NonEmptyList<T> Examples ###\n");
        Console.WriteLine("NonEmptyList guarantees at least one element - no more empty checks!\n");

        // Creating NonEmptyList
        Console.WriteLine("1. Creating NonEmptyList:");
        var single = NonEmptyList<int>.Of(42);
        var multiple = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        Console.WriteLine($"   Single: {single}");
        Console.WriteLine($"   Multiple: {multiple}");

        // Head is always safe!
        Console.WriteLine("\n2. Safe Head Access (always works!):");
        Console.WriteLine($"   single.Head: {single.Head}");
        Console.WriteLine($"   multiple.Head: {multiple.Head}");
        Console.WriteLine($"   multiple.Tail: [{string.Join(", ", multiple.Tail)}]");

        // FromEnumerable returns Option
        Console.WriteLine("\n3. FromEnumerable (returns Option):");
        var fromList = NonEmptyList<int>.FromEnumerable(new[] { 10, 20, 30 });
        var fromEmpty = NonEmptyList<int>.FromEnumerable(Array.Empty<int>());
        Console.WriteLine($"   From [10,20,30]: {fromList}");
        Console.WriteLine($"   From []: {fromEmpty}");

        // Map
        Console.WriteLine("\n4. Map:");
        var doubled = multiple.Map(x => x * 2);
        Console.WriteLine($"   Doubled: {doubled}");

        // FlatMap
        Console.WriteLine("\n5. FlatMap:");
        var expanded = NonEmptyList<int>.Of(1, 2).FlatMap(x => NonEmptyList<int>.Of(x, x * 10));
        Console.WriteLine($"   Expanded: {expanded}");

        // Reduce (no initial value needed!)
        Console.WriteLine("\n6. Reduce (safe - always has elements!):");
        var sum = multiple.Reduce((a, b) => a + b);
        var product = multiple.Reduce((a, b) => a * b);
        Console.WriteLine($"   Sum: {sum}");
        Console.WriteLine($"   Product: {product}");

        // Last
        Console.WriteLine("\n7. Last (also always safe!):");
        Console.WriteLine($"   multiple.Last(): {multiple.Last()}");

        // Append and Prepend
        Console.WriteLine("\n8. Append and Prepend:");
        var appended = NonEmptyList<int>.Of(1, 2).Append(3);
        var prepended = NonEmptyList<int>.Of(2, 3).Prepend(1);
        Console.WriteLine($"   Appended: {appended}");
        Console.WriteLine($"   Prepended: {prepended}");

        // Concat
        Console.WriteLine("\n9. Concat:");
        var list1 = NonEmptyList<int>.Of(1, 2);
        var list2 = NonEmptyList<int>.Of(3, 4);
        var concatenated = list1.Concat(list2);
        Console.WriteLine($"   Concatenated: {concatenated}");

        // Filter returns Option<NonEmptyList>
        Console.WriteLine("\n10. Filter (returns Option - might be empty!):");
        var filtered = multiple.Filter(x => x > 2);
        var filteredAll = multiple.Filter(x => x > 100);
        Console.WriteLine($"   Filter(>2): {filtered}");
        Console.WriteLine($"   Filter(>100): {filteredAll}");

        // Reverse
        Console.WriteLine("\n11. Reverse:");
        var toReverse = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var reversed = toReverse.Reverse();
        Console.WriteLine($"   Original: {toReverse}");
        Console.WriteLine($"   Reversed: {reversed}");

        // Real-world: Ensure at least one recipient
        Console.WriteLine("\n12. Real-World: Email Recipients:");
        var recipients = NonEmptyList<string>.Of("admin@example.com", "user@example.com");
        SendEmail(recipients, "Hello!");
    }

    static void SendEmail(NonEmptyList<string> recipients, string message)
    {
        // No need to check if empty - type guarantees at least one recipient!
        Console.WriteLine($"   Sending '{message}' to {recipients.Count} recipient(s)");
        Console.WriteLine($"   Primary: {recipients.Head}");
        if (recipients.Tail.Count > 0)
            Console.WriteLine($"   CC: {string.Join(", ", recipients.Tail)}");
    }

    static void WriterExamples()
    {
        Console.WriteLine("### Writer<TLog, T> Examples ###\n");
        Console.WriteLine("Writer accumulates logs/output alongside computations.\n");

        // Basic Writer with string log
        Console.WriteLine("1. Basic Writer:");
        var writer = Writer<string, int>.Tell(42, "Created value 42");
        Console.WriteLine($"   Value: {writer.Value}");
        Console.WriteLine($"   Log: {writer.Log}");

        // Using Writer.Tell
        Console.WriteLine("\n2. Using Writer.Tell:");
        var logged = Writer<string, int>.Tell(10, "Starting with 10");
        Console.WriteLine($"   Value: {logged.Value}, Log: {logged.Log}");

        // Map preserves log
        Console.WriteLine("\n3. Map (preserves log):");
        var mapped = Writer<string, int>.Tell(5, "Initial")
            .Map(x => x * 2);
        Console.WriteLine($"   Value: {mapped.Value}, Log: {mapped.Log}");

        // FlatMap combines logs
        Console.WriteLine("\n4. FlatMap (combines logs):");
        var chained = Writer<string, int>.Tell(10, "Step 1: Got 10")
            .FlatMap(
                x => Writer<string, int>.Tell(x * 2, $"Step 2: Doubled to {x * 2}"),
                (log1, log2) => $"{log1} | {log2}"
            );
        Console.WriteLine($"   Value: {chained.Value}");
        Console.WriteLine($"   Log: {chained.Log}");

        // List-based logging
        Console.WriteLine("\n5. List-Based Logging:");
        var listWriter = Writer<List<string>, int>.Tell(1, new List<string> { "Started" });
        var listChained = listWriter.FlatMap(
            x => Writer<List<string>, int>.Tell(x + 1, new List<string> { "Incremented" }),
            (log1, log2) => log1.Concat(log2).ToList()
        ).FlatMap(
            x => Writer<List<string>, int>.Tell(x * 2, new List<string> { "Doubled" }),
            (log1, log2) => log1.Concat(log2).ToList()
        );
        Console.WriteLine($"   Final Value: {listChained.Value}");
        Console.WriteLine($"   Log entries: [{string.Join(", ", listChained.Log)}]");

        // Pattern matching
        Console.WriteLine("\n6. Pattern Matching:");
        var result = writer.Match((value, log) => $"Got {value} with log: '{log}'");
        Console.WriteLine($"   Result: {result}");

        // Real-world: Computation with audit trail
        Console.WriteLine("\n7. Real-World: Computation with Audit Trail:");
        var audit = CalculateWithAudit(100);
        Console.WriteLine($"   Final Value: {audit.Value}");
        Console.WriteLine($"   Audit Trail:\n{audit.Log}");
    }

    static Writer<string, decimal> CalculateWithAudit(decimal initial)
    {
        return Writer<string, decimal>.Tell(initial, $"[{DateTime.Now:HH:mm:ss}] Started with {initial}\n")
            .FlatMap(
                x => Writer<string, decimal>.Tell(x * 1.1m, $"[{DateTime.Now:HH:mm:ss}] Applied 10% increase: {x * 1.1m}\n"),
                (a, b) => a + b
            )
            .FlatMap(
                x => Writer<string, decimal>.Tell(Math.Round(x, 2), $"[{DateTime.Now:HH:mm:ss}] Rounded to {Math.Round(x, 2)}\n"),
                (a, b) => a + b
            );
    }

    static void ReaderExamples()
    {
        Console.WriteLine("### Reader<R, A> Examples ###\n");
        Console.WriteLine("Reader provides dependency injection in a functional way.\n");

        // Basic Reader
        Console.WriteLine("1. Basic Reader:");
        var reader = Reader<AppConfig, string>.From(config => $"App: {config.AppName}");
        var config = new AppConfig { AppName = "MyApp", ConnectionString = "Server=localhost", MaxRetries = 3 };
        var result = reader.Run(config);
        Console.WriteLine($"   Result: {result}");

        // Pure (constant value)
        Console.WriteLine("\n2. Pure (ignores environment):");
        var pure = Reader<AppConfig, int>.Pure(42);
        Console.WriteLine($"   Pure value: {pure.Run(config)}");

        // Ask (get entire environment)
        Console.WriteLine("\n3. Ask (get environment):");
        var ask = Reader<AppConfig, AppConfig>.Ask();
        var env = ask.Run(config);
        Console.WriteLine($"   Got config: {env.AppName}");

        // Asks (select from environment)
        Console.WriteLine("\n4. Asks (select from environment):");
        var getConnString = Reader<AppConfig, string>.Asks(c => c.ConnectionString);
        var getRetries = Reader<AppConfig, int>.Asks(c => c.MaxRetries);
        Console.WriteLine($"   Connection: {getConnString.Run(config)}");
        Console.WriteLine($"   Retries: {getRetries.Run(config)}");

        // Map
        Console.WriteLine("\n5. Map:");
        var mapped = Reader<AppConfig, string>.Asks(c => c.AppName)
            .Map(name => name.ToUpper());
        Console.WriteLine($"   Mapped: {mapped.Run(config)}");

        // FlatMap for composing readers
        Console.WriteLine("\n6. FlatMap (compose readers):");
        var composed = Reader<AppConfig, string>.Asks(c => c.AppName)
            .FlatMap(name => Reader<AppConfig, string>.Asks(c => $"{name} v1.0 (retries: {c.MaxRetries})"));
        Console.WriteLine($"   Composed: {composed.Run(config)}");

        // Real-world: Service composition
        Console.WriteLine("\n7. Real-World: Service Composition:");
        var userService = GetUserReader(1);
        var greeting = userService.FlatMap(user =>
            Reader<AppConfig, string>.Asks(c => $"Welcome to {c.AppName}, {user}!"));
        Console.WriteLine($"   Greeting: {greeting.Run(config)}");

        // Combine multiple readers using FlatMap
        Console.WriteLine("\n8. Combining Readers:");
        var combinedReader = Reader<AppConfig, string>.Asks(c => c.AppName)
            .FlatMap(name => Reader<AppConfig, string>.Asks(c => $"{name} configured with {c.MaxRetries} retries"));
        Console.WriteLine($"   Combined: {combinedReader.Run(config)}");

        // Local environment modification
        Console.WriteLine("\n9. WithEnvironment (transform environment):");
        var simpleReader = Reader<string, int>.From(s => s.Length);
        var adaptedReader = simpleReader.WithEnvironment<AppConfig>(c => c.AppName);
        Console.WriteLine($"   App name length: {adaptedReader.Run(config)}");
    }

    static Reader<AppConfig, string> GetUserReader(int userId)
    {
        return Reader<AppConfig, string>.From(config =>
        {
            // Simulate using config for database access
            return $"User_{userId}";
        });
    }

    static void LinqExamples()
    {
        Console.WriteLine("### LINQ Query Syntax Examples ###\n");

        // Option LINQ
        Console.WriteLine("1. Option LINQ Query:");
        var optionResult = from x in Option<int>.Some(10)
                           from y in Option<int>.Some(20)
                           where x > 5
                           select x + y;
        Console.WriteLine($"   Result: {optionResult}"); // Some(30)

        // Result LINQ
        Console.WriteLine("\n2. Result LINQ Query:");
        var resultQuery = from x in Result<int, string>.Ok(10)
                          from y in Result<int, string>.Ok(20)
                          select x * y;
        Console.WriteLine($"   Result: {resultQuery}"); // Ok(200)

        // Complex query with let
        Console.WriteLine("\n3. Complex Query with 'let':");
        var complexQuery = from x in Option<int>.Some(5)
                           let doubled = x * 2
                           from y in Option<int>.Some(3)
                           select doubled + y;
        Console.WriteLine($"   Result: {complexQuery}"); // Some(13)

        // Real-world: Parse and validate
        Console.WriteLine("\n4. Parse and Validate:");
        Option<int> TryParse(string s) =>
            int.TryParse(s, out var v) ? Option<int>.Some(v) : Option<int>.None();

        var parseResult = from a in TryParse("10")
                          from b in TryParse("20")
                          where a > 5
                          select a + b;
        Console.WriteLine($"   Parsed sum: {parseResult}"); // Some(30)
    }

    static async Task AsyncExamples()
    {
        Console.WriteLine("### Async/Await Examples ###\n");

        // Async Option mapping
        Console.WriteLine("1. Async Option Mapping:");
        var asyncOption = await Option<int>.Some(42)
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });
        Console.WriteLine($"   Result: {asyncOption}"); // Some(84)

        // Async Result chaining
        Console.WriteLine("\n2. Async Result Chaining:");
        var asyncResult = await Result<int, string>.Ok(10)
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            })
            .AndThenAsync(async x =>
            {
                await Task.Delay(10);
                return Result<int, string>.Ok(x + 5);
            });
        Console.WriteLine($"   Result: {asyncResult}"); // Ok(25)

        // Async with side effects
        Console.WriteLine("\n3. Async with TapAsync:");
        var logged = await Result<int, string>.Ok(42)
            .TapAsync(async x =>
            {
                await Task.Delay(10);
                Console.WriteLine($"   Logging value: {x}");
            });
        Console.WriteLine($"   Final result: {logged}"); // Ok(42)

        // Simulate async API call
        Console.WriteLine("\n4. Simulate Async API Call:");
        var apiResult = await SimulateApiCall(true);
        apiResult.Match(
            okAction: data => Console.WriteLine($"   API Success: {data}"),
            errAction: error => Console.WriteLine($"   API Error: {error}")
        );
    }

    static async Task<Result<string, string>> SimulateApiCall(bool success)
    {
        await Task.Delay(10);
        return success
            ? Result<string, string>.Ok("Data from API")
            : Result<string, string>.Err("API Error");
    }

    static void CollectionExamples()
    {
        Console.WriteLine("### Collection Extensions Examples ###\n");

        // Sequence - all or nothing
        Console.WriteLine("1. Sequence (All Some -> Some of List):");
        var allSome = new[] { Option<int>.Some(1), Option<int>.Some(2), Option<int>.Some(3) };
        var sequenced = allSome.Sequence();
        Console.WriteLine($"   Result: {sequenced.Match(
            someFunc: vals => $"Some([{string.Join(", ", vals)}])",
            noneFunc: () => "None"
        )}");

        // Traverse - map and sequence
        Console.WriteLine("\n2. Traverse (Parse all strings):");
        var strings = new[] { "1", "2", "3" };
        var traversed = strings.Traverse(s =>
            int.TryParse(s, out var v) ? Option<int>.Some(v) : Option<int>.None());
        Console.WriteLine($"   Result: {traversed.Match(
            someFunc: vals => $"Some([{string.Join(", ", vals)}])",
            noneFunc: () => "None"
        )}");

        // Choose - filter and unwrap
        Console.WriteLine("\n3. Choose (Filter Somes):");
        var mixed = new[] { Option<int>.Some(1), Option<int>.None(), Option<int>.Some(3) };
        var chosen = mixed.Choose().ToList();
        Console.WriteLine($"   Result: [{string.Join(", ", chosen)}]"); // [1, 3]

        // Result collections
        Console.WriteLine("\n4. Result Sequence (All Ok -> Ok of List):");
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };
        var resultSequence = results.Sequence();
        Console.WriteLine($"   Result: {resultSequence.Match(
            okFunc: vals => $"Ok([{string.Join(", ", vals)}])",
            errFunc: err => $"Err({err})"
        )}");

        // Partition Results
        Console.WriteLine("\n5. Partition Results:");
        var mixedResults = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("error2")
        };
        var (oks, errors) = mixedResults.Partition();
        Console.WriteLine($"   Oks: [{string.Join(", ", oks)}]");
        Console.WriteLine($"   Errors: [{string.Join(", ", errors)}]");

        // Real-world: Process multiple items
        Console.WriteLine("\n6. Real-World: Process multiple items:");
        var items = new[] { "10", "20", "30" };
        var processed = items
            .Traverse(s => int.TryParse(s, out var v)
                ? Result<int, string>.Ok(v)
                : Result<int, string>.Err($"Invalid: {s}"))
            .Map(numbers => numbers.Sum());

        Console.WriteLine($"   Total: {processed.Match(
            okFunc: sum => sum.ToString(),
            errFunc: err => err
        )}"); // 60
    }

    static void ErrorUnionExamples()
    {
        Console.WriteLine("### ErrorUnion Examples ###\n");
        Console.WriteLine("ErrorUnion generates typed error matching for Result<T, TError>.\n");

        // Define error types using [ErrorUnion]
        Console.WriteLine("1. Creating Results with Typed Errors:");
        var success = Result<User, UserError>.Ok(new User { Email = "john@example.com", FirstName = "John" });
        var notFound = Result<User, UserError>.Err(UserError.NewNotFound(Guid.NewGuid()));
        var invalidEmail = Result<User, UserError>.Err(UserError.NewInvalidEmail("bad-email"));
        var unauthorized = Result<User, UserError>.Err(UserError.NewUnauthorized());

        Console.WriteLine($"   Success: {success}");
        Console.WriteLine($"   NotFound: {notFound}");
        Console.WriteLine($"   InvalidEmail: {invalidEmail}");
        Console.WriteLine($"   Unauthorized: {unauthorized}");

        // Match on error type
        Console.WriteLine("\n2. Pattern Matching on Error Types:");
        var message = notFound.MatchError(
            ok: user => $"Found user: {user.Email}",
            notFound: e => $"User with ID {e.Id} not found",
            invalidEmail: e => $"Invalid email format: {e.Email}",
            unauthorized: _ => "Access denied - please login");
        Console.WriteLine($"   Message: {message}");

        // Error type checking using generated properties
        Console.WriteLine("\n3. Error Type Checking:");
        var error = notFound.UnwrapErr();
        Console.WriteLine($"   IsNotFound: {error.IsNotFound}");
        Console.WriteLine($"   IsInvalidEmail: {error.IsInvalidEmail}");
        Console.WriteLine($"   IsUnauthorized: {error.IsUnauthorized}");

        // Match on the error directly
        Console.WriteLine("\n4. Direct Error Matching:");
        error.Match(
            notFound: e => Console.WriteLine($"   Not found: {e.Id}"),
            invalidEmail: e => Console.WriteLine($"   Invalid email: {e.Email}"),
            unauthorized: _ => Console.WriteLine("   Unauthorized"));

        // Using As{Case}() for safe casting
        Console.WriteLine("\n5. Safe Casting with As{Case}():");
        var asNotFound = error.AsNotFound();
        var asInvalidEmail = error.AsInvalidEmail();
        Console.WriteLine($"   AsNotFound: {asNotFound}");
        Console.WriteLine($"   AsInvalidEmail: {asInvalidEmail}");

        // MapError for transforming errors
        Console.WriteLine("\n6. MapError for Error Transformation:");
        var httpResult = invalidEmail.MapError(
            notFound: e => 404,
            invalidEmail: e => 400,
            unauthorized: _ => 401);
        Console.WriteLine($"   HTTP Status: {httpResult}");

        // Recover from specific errors
        Console.WriteLine("\n7. Recover from Specific Errors:");
        var recovered = notFound.Recover(
            notFound: e => Result<User, UserError>.Ok(new User { Email = "default@example.com", FirstName = "Default" }));
        Console.WriteLine($"   Recovered: {recovered}");

        // ToResult helper on error
        Console.WriteLine("\n8. ToResult Helper:");
        var fromError = UserError.NewUnauthorized().ToResult<User>();
        Console.WriteLine($"   From error: {fromError}");

        // Real-world: API error handling
        Console.WriteLine("\n9. Real-World: API Error Handling:");
        var apiResult = GetUserFromApi(Guid.Empty);
        var response = apiResult.MatchError(
            ok: user => $"200 OK: {user.Email}",
            notFound: e => $"404 Not Found: User {e.Id}",
            invalidEmail: e => $"400 Bad Request: {e.Email}",
            unauthorized: _ => "401 Unauthorized");
        Console.WriteLine($"   Response: {response}");
    }

    static Result<User, UserError> GetUserFromApi(Guid id)
    {
        if (id == Guid.Empty)
            return UserError.NewNotFound(id).ToResult<User>();

        return Result<User, UserError>.Ok(new User { Email = "found@example.com", FirstName = "Found" });
    }
}

// Error union definition - generates Match, Is{Case}, As{Case}, and Result extensions
[ErrorUnion]
public abstract partial record UserError
{
    public sealed partial record NotFound(Guid Id) : UserError;
    public sealed partial record InvalidEmail(string Email) : UserError;
    public sealed partial record Unauthorized : UserError;
}

class User
{
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string FirstName { get; set; } = "John";
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = "Doe";
}

record UserDto(string Name, string Email, int Age);

class AppConfig
{
    public string AppName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}
