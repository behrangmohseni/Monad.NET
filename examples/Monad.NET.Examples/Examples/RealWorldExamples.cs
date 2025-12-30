using Monad.NET.Examples.Models;

namespace Monad.NET.Examples.Examples;

/// <summary>
/// Real-world examples showing practical applications of Monad.NET.
/// These patterns are commonly used in production applications.
/// </summary>
public static class RealWorldExamples
{
    public static void Run()
    {
        Console.WriteLine("Real-world patterns for production applications.\n");

        // 1. Railway-oriented programming
        Console.WriteLine("1. Railway-Oriented Pipeline:");
        var orderResult = ProcessOrder("valid-customer", "product-123", 5);
        Console.WriteLine($"   Valid order:   {orderResult}");

        var failedOrder = ProcessOrder("invalid", "product-123", 0);
        Console.WriteLine($"   Invalid order: {failedOrder}");

        // 2. Form validation with error accumulation
        Console.WriteLine("\n2. Form Validation (ALL errors):");
        ValidateRegistrationForm("", "invalid", -5);
        ValidateRegistrationForm("John Doe", "john@example.com", 25);

        // 3. Safe navigation
        Console.WriteLine("\n3. Safe Navigation:");
        var validOrder = new Order(new Customer(new Address("Seattle", "USA")));
        var nullCustomer = new Order(null);

        Console.WriteLine($"   Valid order city: {GetOrderCity(validOrder)}");
        Console.WriteLine($"   Null customer:    {GetOrderCity(nullCustomer)}");

        // 4. Configuration with fallbacks
        Console.WriteLine("\n4. Configuration Fallbacks:");
        var timeout = GetConfig("TIMEOUT")
            .OrElse(() => GetConfig("DEFAULT_TIMEOUT"))
            .OrElse(() => Option<int>.Some(30));
        Console.WriteLine($"   Timeout: {timeout.UnwrapOr(0)}s");

        // 5. Error recovery
        Console.WriteLine("\n5. Error Recovery:");
        var data = FetchFromPrimary()
            .OrElse(err => FetchFromSecondary())
            .OrElse(err => FetchFromCache());
        Console.WriteLine($"   Data: {data}");

        // 6. Parsing pipeline
        Console.WriteLine("\n6. Parsing Pipeline:");
        ParseConfigFile("timeout=30\nmax_retries=3\nbuffer=1024");
        ParseConfigFile("timeout=30\nmax_retries=abc\nbuffer=1024");

        // 7. Optional parameters
        Console.WriteLine("\n7. Optional Parameters:");
        var user1 = new User("john@test.com", "John", "Doe", 25);
        var user2 = new User("jane@test.com", "Jane", "Doe", 30) { MiddleName = "Marie" };
        Console.WriteLine($"   Without middle: {GetDisplayName(user1)}");
        Console.WriteLine($"   With middle:    {GetDisplayName(user2)}");

        // 8. Dependent operations
        Console.WriteLine("\n8. Dependent Operations:");
        var loginResult = ValidateCredentials("user", "password123")
            .AndThen(user => GenerateToken(user))
            .AndThen(token => CreateSession(token));
        Console.WriteLine($"   Login: {loginResult}");

        // 9. Batch operation with partial failures
        Console.WriteLine("\n9. Batch Processing:");
        var items = new[] { "1", "2", "invalid", "4", "error", "6" };
        var (successes, failures) = ProcessBatch(items);
        Console.WriteLine($"   Successes: [{string.Join(", ", successes)}]");
        Console.WriteLine($"   Failures:  [{string.Join(", ", failures)}]");

        // 10. State machine
        Console.WriteLine("\n10. Order State Machine:");
        var state = OrderState.Created;
        state = TransitionState(state, "submit");
        Console.WriteLine($"   After submit: {state}");
        state = TransitionState(state, "pay");
        Console.WriteLine($"   After pay:    {state}");
        state = TransitionState(state, "ship");
        Console.WriteLine($"   After ship:   {state}");
    }

    // Railway-oriented pipeline
    private static Result<OrderDto, string> ProcessOrder(string customerId, string productId, int quantity)
    {
        return ValidateCustomerId(customerId)
            .AndThen(_ => ValidateProductId(productId))
            .AndThen(_ => ValidateQuantity(quantity))
            .Map(_ => new OrderDto(
                Guid.NewGuid(),
                customerId,
                productId,
                quantity,
                quantity * 9.99m));
    }

    private static Result<string, string> ValidateCustomerId(string id) =>
        id == "invalid"
            ? Result<string, string>.Err("Invalid customer")
            : Result<string, string>.Ok(id);

    private static Result<string, string> ValidateProductId(string id) =>
        id.StartsWith("product-")
            ? Result<string, string>.Ok(id)
            : Result<string, string>.Err("Invalid product ID");

    private static Result<int, string> ValidateQuantity(int qty) =>
        qty > 0
            ? Result<int, string>.Ok(qty)
            : Result<int, string>.Err("Quantity must be positive");

    // Form validation
    private static void ValidateRegistrationForm(string name, string email, int age)
    {
        var nameVal = string.IsNullOrWhiteSpace(name)
            ? Validation<string, string>.Invalid("Name required")
            : Validation<string, string>.Valid(name);

        var emailVal = !email.Contains('@')
            ? Validation<string, string>.Invalid("Invalid email")
            : Validation<string, string>.Valid(email);

        var ageVal = age < 0
            ? Validation<int, string>.Invalid("Age must be non-negative")
            : age < 18
                ? Validation<int, string>.Invalid("Must be 18+")
                : Validation<int, string>.Valid(age);

        var result = nameVal
            .Apply(emailVal, (n, e) => (n, e))
            .Apply(ageVal, (x, a) => new UserDto(x.n, x.e, a));

        result.Match(
            validAction: u => Console.WriteLine($"   Valid: {u.Name}, {u.Email}, {u.Age}"),
            invalidAction: errs => Console.WriteLine($"   Errors: [{string.Join(", ", errs)}]"));
    }

    // Safe navigation
    private static Option<string> GetOrderCity(Order order) =>
        order.Customer.ToOption()
            .AndThen(c => c.Address.ToOption())
            .Map(a => a.City);

    // Configuration
    private static Option<int> GetConfig(string key)
    {
        var configs = new Dictionary<string, int> { ["DEFAULT_TIMEOUT"] = 60 };
        return configs.TryGetValue(key, out var value)
            ? Option<int>.Some(value)
            : Option<int>.None();
    }

    // Error recovery
    private static Result<string, string> FetchFromPrimary() =>
        Result<string, string>.Err("Primary unavailable");

    private static Result<string, string> FetchFromSecondary() =>
        Result<string, string>.Err("Secondary unavailable");

    private static Result<string, string> FetchFromCache() =>
        Result<string, string>.Ok("Cached data");

    // Parsing
    private static void ParseConfigFile(string content)
    {
        var lines = content.Split('\n');
        var values = lines
            .Select(l => l.Split('='))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim());

        var config = ParseInt(values, "timeout")
            .Apply(ParseInt(values, "max_retries"), (t, r) => (t, r))
            .Apply(ParseInt(values, "buffer"), (x, b) => new ParsedConfig(x.t, x.r, b));

        config.Match(
            validAction: c => Console.WriteLine($"   Parsed: timeout={c.Timeout}, retries={c.MaxRetries}, buffer={c.BufferSize}"),
            invalidAction: errs => Console.WriteLine($"   Parse errors: [{string.Join(", ", errs)}]"));
    }

    private static Validation<int, string> ParseInt(Dictionary<string, string> d, string key) =>
        d.TryGetValue(key, out var v) && int.TryParse(v, out var n)
            ? Validation<int, string>.Valid(n)
            : Validation<int, string>.Invalid($"Invalid {key}");

    // Optional parameters
    private static string GetDisplayName(User user) =>
        user.MiddleName.ToOption()
            .Map(m => $"{user.FirstName} {m} {user.LastName}")
            .UnwrapOr($"{user.FirstName} {user.LastName}");

    // Dependent operations
    private static Result<string, string> ValidateCredentials(string user, string pass) =>
        pass.Length >= 8
            ? Result<string, string>.Ok(user)
            : Result<string, string>.Err("Password too short");

    private static Result<string, string> GenerateToken(string user) =>
        Result<string, string>.Ok($"token-{user}-{Guid.NewGuid():N}");

    private static Result<string, string> CreateSession(string token) =>
        Result<string, string>.Ok($"session:{token}");

    // Batch processing
    private static (IReadOnlyList<int> Successes, IReadOnlyList<string> Failures) ProcessBatch(string[] items)
    {
        var results = items.Select(item =>
            int.TryParse(item, out var n)
                ? Result<int, string>.Ok(n)
                : Result<int, string>.Err(item));
        return results.Partition();
    }

    // State machine
    private enum OrderState { Created, Submitted, Paid, Shipped, Delivered }

    private static OrderState TransitionState(OrderState current, string action)
    {
        return (current, action) switch
        {
            (OrderState.Created, "submit") => OrderState.Submitted,
            (OrderState.Submitted, "pay") => OrderState.Paid,
            (OrderState.Paid, "ship") => OrderState.Shipped,
            (OrderState.Shipped, "deliver") => OrderState.Delivered,
            _ => current
        };
    }
}

