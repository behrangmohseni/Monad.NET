# Monad.NET Recipes & Cookbook

Real-world patterns and solutions for common scenarios using Monad.NET.

---

## Table of Contents

1. [Form Validation with Error Accumulation](#1-form-validation-with-error-accumulation)
2. [HTTP Client with Fallback](#2-http-client-with-fallback)
3. [Database Queries with Option](#3-database-queries-with-option)
4. [Configuration Loading](#4-configuration-loading)
5. [Converting Between Types](#5-converting-between-types)
6. [Parallel Processing with Results](#6-parallel-processing-with-results)
7. [Retry with Exponential Backoff](#7-retry-with-exponential-backoff)
8. [Pipeline Pattern (Railway-Oriented)](#8-pipeline-pattern-railway-oriented)
9. [Dependency Injection with Reader](#9-dependency-injection-with-reader)
10. [State Machine with State Monad](#10-state-machine-with-state-monad)
11. [Logging Side Effects with Writer](#11-logging-side-effects-with-writer)
12. [API Response Handling](#12-api-response-handling)
13. [Safe Dictionary Access](#13-safe-dictionary-access)
14. [Nullable to Option Patterns](#14-nullable-to-option-patterns)
15. [Error Recovery Strategies](#15-error-recovery-strategies)

---

## 1. Form Validation with Error Accumulation

**Problem:** Validate multiple form fields and show ALL errors at once.

```csharp
public record UserForm(string Name, string Email, int Age);

// Individual validators
Validation<string, string> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name) 
        ? Validation<string, string>.Error("Name is required")
        : name.Length < 2 
            ? Validation<string, string>.Error("Name must be at least 2 characters")
            : Validation<string, string>.Ok(name.Trim());

Validation<string, string> ValidateEmail(string email) =>
    string.IsNullOrWhiteSpace(email)
        ? Validation<string, string>.Error("Email is required")
        : !email.Contains("@")
            ? Validation<string, string>.Error("Email must contain @")
            : Validation<string, string>.Ok(email.ToLower());

Validation<int, string> ValidateAge(int age) =>
    Validation<int, string>.Ok(age)
        .Ensure(a => a >= 18, "Must be at least 18 years old")
        .Ensure(a => a <= 120, "Age seems unrealistic");

// Combine all validations (accumulates errors!)
Validation<User, string> ValidateUser(UserForm form) =>
    ValidateName(form.Name)
        .Apply(ValidateEmail(form.Email), (name, email) => (name, email))
        .Apply(ValidateAge(form.Age), (partial, age) => new User(partial.name, partial.email, age));

// Usage
var result = ValidateUser(new UserForm("", "invalid", 15));
result.Match(
    valid: user => SaveUser(user),
    invalid: errors => {
        // Shows: ["Name is required", "Email must contain @", "Must be at least 18 years old"]
        foreach (var error in errors)
            Console.WriteLine($"Error: {error}");
    });
```

---

## 2. HTTP Client with Fallback

**Problem:** Make an HTTP call with retry and fallback to cache.

```csharp
public async Task<Result<User, ApiError>> GetUserWithFallback(int userId)
{
    // Primary: Try API
    var apiResult = await TryGetFromApi(userId);
    
    return await apiResult.OrElseAsync(async error =>
    {
        // Fallback 1: Try cache
        var cacheResult = await TryGetFromCache(userId);
        if (cacheResult.IsOk) return cacheResult;
        
        // Fallback 2: Return stale data with warning
        var staleResult = await TryGetStaleData(userId);
        return staleResult.Map(user => user with { IsStale = true });
    });
}

// With retry
public async Task<Result<T, ApiError>> CallWithRetry<T>(
    Func<Task<Result<T, ApiError>>> apiCall,
    int maxRetries = 3)
{
    Result<T, ApiError> lastResult = Result<T, ApiError>.Error(new ApiError("Not started"));
    
    for (int i = 0; i < maxRetries; i++)
    {
        lastResult = await apiCall();
        if (lastResult.IsOk) return lastResult;
        
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
    }
    
    return lastResult;
}
```

---

## 3. Database Queries with Option

**Problem:** Handle nullable database results cleanly.

```csharp
// Using Entity Framework Core with Monad.NET.EntityFrameworkCore
public async Task<Option<User>> FindUserByEmail(string email)
{
    return await _context.Users
        .FirstOrNoneAsync(u => u.Email == email);
}

// Chaining optional queries
public async Task<Option<OrderSummary>> GetUserOrderSummary(int userId)
{
    var user = await FindUser(userId);
    if (user.IsNone) return Option<OrderSummary>.None();
    
    var order = await GetLatestOrder(user.GetValue().Id);
    return order.Map(o => new OrderSummary(o.Id, o.Total));
}

// Handling missing related data
public async Task<UserProfile> GetUserProfile(int userId)
{
    var user = await _context.Users.FindAsync(userId);
    
    return new UserProfile
    {
        Name = user.Name,
        Email = user.Email,
        // Optional fields handled gracefully
        PhoneNumber = user.PhoneNumber.GetValueOr("Not provided"),
        ProfileImage = (await GetProfileImage(userId)).GetValueOr(defaultImage),
        LastOrder = (await GetLatestOrder(userId))
            .Map(o => o.Summary)
            .GetValueOr("No orders yet")
    };
}
```

---

## 4. Configuration Loading

**Problem:** Load configuration from multiple sources with fallbacks.

```csharp
public Result<AppConfig, ConfigError> LoadConfiguration()
{
    return LoadFromEnvironment()
        .OrElse(_ => LoadFromJsonFile("appsettings.json"))
        .OrElse(_ => LoadFromJsonFile("appsettings.default.json"))
        .Bind(ValidateConfiguration);
}

Result<AppConfig, ConfigError> LoadFromEnvironment()
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING").ToOption();
    var apiKey = Environment.GetEnvironmentVariable("API_KEY").ToOption();
    
    return connectionString
        .Zip(apiKey)
        .Match(
            some: pair => Result<AppConfig, ConfigError>.Ok(
                new AppConfig(pair.Item1, pair.Item2)),
            none: () => Result<AppConfig, ConfigError>.Error(
                new ConfigError("Missing environment variables")));
}

Result<AppConfig, ConfigError> ValidateConfiguration(AppConfig config)
{
    return Validation<AppConfig, string>.Ok(config)
        .Ensure(c => !string.IsNullOrEmpty(c.ConnectionString), "Connection string required")
        .Ensure(c => c.ApiKey.Length >= 32, "API key must be at least 32 characters")
        .ToResult()
        .MapError(errors => new ConfigError(string.Join("; ", errors)));
}
```

---

## 5. Converting Between Types

**Problem:** Convert between Option, Result, Validation, and Try.

```csharp
// Option → Result
Option<User> user = FindUser(id);
Result<User, string> result = user.OkOr("User not found");

// Result → Option
Result<int, Error> parsed = ValidatePositive(input);
Option<int> value = parsed.Ok(); // Discards error

// Result → Validation
Result<User, Error> userResult = GetUser(id);
Validation<User, Error> validation = userResult.ToValidation();

// Validation → Result
Validation<User, string> validated = ValidateUser(form);
Result<User, IReadOnlyList<string>> result = validated.ToResult();

// Try → Result
Try<int> tried = Try<int>.Of(() => int.Parse(input));
Result<int, string> result = tried.ToResult(ex => ex.Message);

// Try → Option
Option<int> value = tried.ToOption(); // Failure becomes None

// Option → Try
Option<User> user = FindUser(id);
Try<User> tried = user.Match(
    some: u => Try<User>.Ok(u),
    none: () => Try<User>.Error(new InvalidOperationException("User not found")));

// Chaining different types
var userOption = await GetUserOption(id);      // Option<User>
var userResult = userOption
    .OkOrElse(() => "User not found");         // Result<User, string>
    
if (userResult.IsError) 
    return Result<UserDto, string>.Error(userResult.GetError());

var validResult = await ValidateUserAsync(userResult.GetValue());
var dtoResult = validResult.Map(user => user.ToDto());
```

---

## 6. Parallel Processing with Results

**Problem:** Process multiple items in parallel and collect all results.

```csharp
// Process all items, fail on first error
public async Task<Result<IReadOnlyList<ProcessedItem>, Error>> ProcessAllOrFail(
    IEnumerable<int> itemIds)
{
    var tasks = itemIds.Select(id => ProcessItemAsync(id));
    return await ResultExtensions.CombineAsync(tasks);
}

// Process all items, collect successes and failures separately
public async Task<(List<ProcessedItem> Successes, List<Error> Errors)> ProcessAllWithPartition(
    IEnumerable<int> itemIds)
{
    return await itemIds.PartitionParallelAsync(
        id => ProcessItemAsync(id),
        maxDegreeOfParallelism: 4);
}

// Process with controlled concurrency
public async Task<Option<IReadOnlyList<User>>> FetchUsersParallel(int[] userIds)
{
    return await userIds.TraverseParallelAsync(
        id => FindUserAsync(id),
        maxDegreeOfParallelism: 10);
}
```

---

## 7. Retry with Exponential Backoff

**Problem:** Retry operations with increasing delays.

```csharp
public async Task<Try<T>> RetryWithBackoff<T>(
    Func<Task<T>> operation,
    int maxAttempts = 3,
    TimeSpan? initialDelay = null)
{
    var delay = initialDelay ?? TimeSpan.FromSeconds(1);
    Exception? lastException = null;
    
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        var result = await Try<T>.OfAsync(operation);
        
        if (result.IsOk)
            return result;
        
        lastException = result.GetException();
        
        if (attempt < maxAttempts)
        {
            await Task.Delay(delay);
            delay = TimeSpan.FromTicks(delay.Ticks * 2); // Exponential backoff
        }
    }
    
    return Try<T>.Error(lastException!);
}

// Using Reader with async operation
var userReader = Reader<HttpClient, Task<User?>>.From(
    client => client.GetFromJsonAsync<User>("/api/user"));

// Execute with retry logic using Try
var result = await RetryAsync(() => userReader.Run(httpClient), retries: 3);
```

---

## 8. Pipeline Pattern (Railway-Oriented)

**Problem:** Build a processing pipeline where any step can fail.

```csharp
public record OrderRequest(string CustomerId, string ProductId, int Quantity);
public record Order(string Id, string CustomerId, Product Product, int Quantity, decimal Total);

public async Task<Result<Order, OrderError>> ProcessOrder(OrderRequest request)
{
    var validated = ValidateRequest(request);
    if (validated.IsError) return validated.MapError(e => e);
    
    var customer = await FindCustomer(validated.GetValue().CustomerId);
    if (customer.IsError) return customer.MapError(e => e);
    
    var product = await FindProduct(request.ProductId);
    if (product.IsError) return product.MapError(e => e);
    
    var inventory = await CheckInventory(product.GetValue(), request.Quantity);
    if (inventory.IsError) return inventory.MapError(e => e);
    
    var price = await CalculatePrice(product.GetValue(), request.Quantity);
    if (price.IsError) return price.MapError(e => e);
    
    var payment = await ChargePayment(customer.GetValue(), price.GetValue());
    if (payment.IsError)
    {
        LogOrderFailure(request, payment.GetError());
        return payment.MapError(e => e);
    }
    
    var order = CreateOrder(customer.GetValue().Id, product.GetValue(), request.Quantity, price.GetValue());
    await SendConfirmationEmail(order);
    return Result<Order, OrderError>.Ok(order);
}

// Each step returns Result<T, OrderError>
Result<OrderRequest, OrderError> ValidateRequest(OrderRequest req) =>
    Validation<OrderRequest, string>.Ok(req)
        .Ensure(r => !string.IsNullOrEmpty(r.CustomerId), "Customer ID required")
        .Ensure(r => !string.IsNullOrEmpty(r.ProductId), "Product ID required")
        .Ensure(r => r.Quantity > 0, "Quantity must be positive")
        .ToResult()
        .MapError(errors => new OrderError.Validation(errors));
```

---

## 9. Dependency Injection with Reader

**Problem:** Pass dependencies through a computation without explicit parameters.

```csharp
public record AppServices(
    IUserRepository Users,
    IEmailService Email,
    ILogger Logger);

// Define computations that need services
var getUser = Reader<AppServices, Task<User>>.From(
    services => services.Users.FindAsync(userId));

var sendWelcome = Reader<AppServices, Task<Unit>>.From(
    async services => {
        await services.Email.SendWelcomeAsync(email);
        return Unit.Default;
    });

// Compose readers
var welcomeNewUser = getUser.Bind(user =>
    Reader<AppServices, User>.From(svc => {
        // Note: For async operations, run the reader first, then await
        svc.Logger.LogInformation("Processing user {Email}", user.Result.Email);
        return user;
    }));

// Run with environment
var services = new AppServices(userRepo, emailService, logger);
var userTask = getUser.Run(services);
var user = await userTask;

// Then send welcome email
await services.Email.SendWelcomeAsync(user.Email);
services.Logger.LogInformation("Welcome email sent to {Email}", user.Email);
```

---

## 10. State Machine with State Monad

**Problem:** Model stateful workflows without mutable variables.

```csharp
public enum OrderStatus { Created, Validated, Paid, Shipped, Delivered }

public record OrderState(OrderStatus Status, List<string> History);

// State transitions
State<OrderState, Unit> Validate() =>
    State<OrderState, Unit>.Modify(state => 
        state.Status == OrderStatus.Created
            ? new OrderState(OrderStatus.Validated, state.History.Append("Validated").ToList())
            : state);

State<OrderState, Unit> ProcessPayment() =>
    State<OrderState, Unit>.Modify(state =>
        state.Status == OrderStatus.Validated
            ? new OrderState(OrderStatus.Paid, state.History.Append("Payment processed").ToList())
            : state);

State<OrderState, Unit> Ship() =>
    State<OrderState, Unit>.Modify(state =>
        state.Status == OrderStatus.Paid
            ? new OrderState(OrderStatus.Shipped, state.History.Append("Shipped").ToList())
            : state);

// Compose the workflow
var processOrder =
    from _ in Validate()
    from __ in ProcessPayment()
    from ___ in Ship()
    from finalState in State<OrderState, OrderState>.Get()
    select finalState;

// Execute
var (result, finalState) = processOrder.Run(new OrderState(OrderStatus.Created, new List<string>()));
// result.Status == OrderStatus.Shipped
// result.History == ["Validated", "Payment processed", "Shipped"]
```

---

## 11. Logging Side Effects with Writer

**Problem:** Accumulate logs alongside computations.

```csharp
// Using Writer with string log
var computation = 
    from a in Writer<string, int>.Tell(10, "Started with 10\n")
    from b in Writer<string, int>.Tell(a * 2, "Doubled value\n")
    from c in Writer<string, int>.Tell(b + 5, "Added 5\n")
    select c;

var (result, log) = (computation.Value, computation.Log);
// result = 25
// log = "Started with 10\nDoubled value\nAdded 5\n"

// Using Writer with structured logs
public record LogEntry(DateTime Timestamp, string Level, string Message);

var structuredComputation =
    Writer<List<LogEntry>, Order>.Tell(order, new List<LogEntry> 
    { 
        new(DateTime.UtcNow, "INFO", "Order received") 
    })
    .Bind(o => ProcessOrder(o), (log1, log2) => log1.Concat(log2).ToList())
    .Tap(o => Console.WriteLine($"Order {o.Id} processed"));
```

---

## 12. API Response Handling

**Problem:** Handle different API response scenarios cleanly.

```csharp
public async Task<IActionResult> GetUserEndpoint(int id)
{
    return await _userService.FindUser(id)
        .MatchAsync(
            some: async user => {
                await _analytics.TrackUserAccess(id);
                return Ok(user.ToDto());
            },
            none: () => Task.FromResult<IActionResult>(NotFound(new { 
                Message = "User not found",
                UserId = id 
            })));
}

// With Monad.NET.AspNetCore
public IActionResult CreateUser(CreateUserRequest request)
{
    return ValidateUser(request)
        .Bind(CreateAndSaveUser)
        .Match(
            ok: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
            err: errors => BadRequest(new ValidationProblemDetails(
                errors.GroupBy(e => e.Field)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()))));
}
```

---

## 13. Safe Dictionary Access

**Problem:** Access dictionary values without null checks.

```csharp
var config = new Dictionary<string, string>
{
    ["api_url"] = "https://api.example.com",
    ["timeout"] = "30"
};

// Safe access with Option
var apiUrl = config.GetOption("api_url");           // Some("https://...")
var missing = config.GetOption("not_exists");        // None

// Chain with parsing
var timeout = config.GetOption("timeout")
    .Bind(s => int.TryParse(s, out var t) ? Option<int>.Some(t) : Option<int>.None())
    .Map(t => TimeSpan.FromSeconds(t))
    .GetValueOr(TimeSpan.FromSeconds(10));

// Multiple optional config values
var portOption = config.GetOption("port")
    .Bind(s => int.TryParse(s, out var p) ? Option<int>.Some(p) : Option<int>.None());
var serverConfig = config.GetOption("host")
    .Zip(portOption)
    .Map(pair => new ServerConfig(pair.Item1, pair.Item2));

```

---

## 14. Nullable to Option Patterns

**Problem:** Convert between C# nullable types and Option.

```csharp
// Nullable reference types
string? nullableName = GetNameOrNull();
Option<string> nameOption = nullableName.ToOption();

// Nullable value types
int? nullableAge = GetAgeOrNull();
Option<int> ageOption = nullableAge.ToOption();

// String validation shortcuts
Option<string> validName = input.ToOptionNotWhiteSpace();
Option<string> trimmedName = input.ToOptionTrimmed();

// Parsing with standard C# patterns  
Option<int> parsedInt = int.TryParse("42", out var i) 
    ? Option<int>.Some(i) : Option<int>.None();
Option<Guid> parsedGuid = Guid.TryParse("550e8400-e29b-41d4-a716-446655440000", out var g) 
    ? Option<Guid>.Some(g) : Option<Guid>.None();
Option<DateTime> parsedDate = DateTime.TryParse("2024-01-15", out var d) 
    ? Option<DateTime>.Some(d) : Option<DateTime>.None();
Option<DayOfWeek> parsedEnum = Enum.TryParse<DayOfWeek>("Monday", out var e) 
    ? Option<DayOfWeek>.Some(e) : Option<DayOfWeek>.None();

// From object graph
User? user = GetUser();
Option<string> email = user.ToOption()
    .Bind(u => u.Profile.ToOption())
    .Bind(p => p.Email.ToOption());
```

---

## 15. Error Recovery Strategies

**Problem:** Implement various error recovery patterns.

```csharp
// Simple fallback
var result = TryPrimary()
    .OrElse(_ => TrySecondary())
    .OrElse(_ => Result<Data, Error>.Ok(DefaultData));

// Recover with logging
var result = await operation
    .TapError(err => _logger.LogWarning("Operation failed: {Error}", err))
    .RecoverAsync(async err => {
        await _metrics.RecordFailure(err);
        return FallbackValue;
    });

// Conditional recovery
var result = FetchData()
    .OrElse(err => err.IsTransient 
        ? RetryFetch() 
        : Result<Data, Error>.Error(err));

// Try multiple strategies
Try<Connection> Connect() =>
    Try<Connection>.Of(() => ConnectPrimary())
        .RecoverWith(ex => Try<Connection>.Of(() => ConnectSecondary()))
        .RecoverWith(ex => Try<Connection>.Of(() => ConnectTertiary()));

// Map error type for recovery
Result<User, DomainError> GetUserWithRecovery(int id) =>
    _repository.FindUser(id)                          // Result<User, DbError>
        .MapError(dbErr => new DomainError.Database(dbErr))
        .OrElse(err => _cache.GetUser(id)             // Result<User, CacheError>
            .MapError(cacheErr => new DomainError.Cache(cacheErr)));
```

---

## Quick Reference: When to Use What

| Scenario | Use This |
|----------|----------|
| Value might be missing | `Option<T>` |
| Operation can fail with typed error | `Result<T, E>` |
| Need ALL validation errors | `Validation<T, E>` |
| Wrapping exception-throwing code | `Try<T>` |
| UI loading states (Blazor) | `RemoteData<T, E>` |
| Guaranteed non-empty collection | `NonEmptyList<T>` |
| Accumulating logs/traces | `Writer<W, T>` |
| Dependency injection without DI | `Reader<R, A>` |
| Stateful computations | `State<S, A>` |
| Deferring side effects | `IO<T>` |

---

**See Also:**
- [Core Types](../CoreTypes.md)
- [Advanced Usage](../AdvancedUsage.md)
- [Pitfalls & Gotchas](Pitfalls.md)

