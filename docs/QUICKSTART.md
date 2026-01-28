# Quick Start Guide

Get started with Monad.NET in 5 minutes!

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Monad.NET
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package Monad.NET
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Monad.NET" />
```

**Requires:** .NET Standard 2.0+ (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## When to Use Monad.NET

| Use Monad.NET when... | Stick with plain C# when... |
|-----------------------|-----------------------------|
| Chaining multiple nullable operations | Single null check with `is not null` |
| Errors are expected and typed (validation, business rules) | Errors are exceptional (network, disk) |
| You need ALL validation errors at once | First-error-wins is acceptable |
| Building composable data pipelines | Simple procedural code |

**The 80/20 rule:** You'll use `Option<T>` and `Result<T, E>` for 80% of cases. Start there.

## Basic Usage

Add the namespace:

```csharp
using Monad.NET;
```

### Option - Handle Missing Values

```csharp
// Instead of null checks
string? name = GetUserName();
if (name is not null)
{
    Console.WriteLine(name.ToUpper());
}

// Use Option
var name = GetUserName().ToOption();
name.Match(
    someFunc: n => Console.WriteLine(n.ToUpper()),
    noneFunc: () => Console.WriteLine("No name")
);

// Or chain operations
var result = GetUserName().ToOption()
    .Map(n => n.ToUpper())
    .GetValueOr("Anonymous");

// Conditional creation with When/Unless
var discount = OptionExtensions.When(order.Total > 100, () => 0.1m);
// Some(0.1m) if order > 100, None otherwise

var warning = OptionExtensions.Unless(user.HasVerifiedEmail, () => "Please verify email");
// Some("Please verify...") if NOT verified, None otherwise

// Replace None with a default (returns Option, not value)
var configOption = loadConfig().DefaultIfNone(defaultConfig);
// Some(loadedConfig) or Some(defaultConfig)

// Throw custom exceptions on None
var user = FindUser(id).ThrowIfNone(() => new UserNotFoundException(id));
```

### Result - Handle Errors

```csharp
// Instead of try/catch
try
{
    var value = int.Parse(input);
    Process(value);
}
catch (Exception ex)
{
    HandleError(ex);
}

// Use Result
var result = ResultExtensions.Try(() => int.Parse(input));
result.Match(
    okAction: value => Process(value),
    errAction: ex => HandleError(ex)
);

// Or chain operations
var output = ResultExtensions.Try(() => int.Parse(input))
    .Map(x => x * 2)
    .Bind(x => Validate(x))
    .GetValueOr(0);

// Transform both success and error types with BiMap
var adapted = apiResult.BiMap(
    dto => new DomainModel(dto),
    err => new DomainError(err.Code, err.Message)
);

// Throw custom exceptions with ThrowIfErr
var user = GetUser(id).ThrowIfErr(err => new UserNotFoundException($"User not found: {err}"));
```

### Validation - Collect All Errors

```csharp
// Validate a form and show ALL errors at once
var nameResult = ValidateName(form.Name);
var emailResult = ValidateEmail(form.Email);
var ageResult = ValidateAge(form.Age);

var user = nameResult
    .Apply(emailResult, (name, email) => (name, email))
    .Apply(ageResult, (partial, age) => new User(partial.name, partial.email, age));

user.Match(
    validAction: u => SaveUser(u),
    invalidAction: errors => ShowErrors(errors)  // Shows ALL validation errors!
);

// Chain validations with Ensure
var validatedAge = Validation<int, string>.Valid(age)
    .Ensure(x => x >= 18, "Must be at least 18")
    .Ensure(x => x <= 120, "Must be at most 120")
    .Ensure(x => x > 0, "Must be positive");
```

### Try - Capture Exceptions

```csharp
// Safely parse with recovery
var value = Try<int>.Of(() => int.Parse(userInput))
    .GetOrElse(0);

// Chain with recovery
var result = Try<int>.Of(() => int.Parse(input))
    .Map(x => x * 2)
    .Recover(ex => -1);
```

### RemoteData - Track Loading States

```csharp
// Perfect for UI state management
RemoteData<User, string> userData = RemoteData<User, string>.NotAsked();

// Render based on state
var ui = userData.Match(
    notAskedFunc: () => RenderLoadButton(),
    loadingFunc: () => RenderSpinner(),
    successFunc: user => RenderUser(user),
    failureFunc: error => RenderError(error)
);
```

### State - Thread State Through Computations

```csharp
// Counter without mutable variables
var increment = State<int, Unit>.Modify(s => s + 1);
var getCount = State<int, int>.Get();

var computation = 
    from _ in increment
    from __ in increment
    from count in getCount
    select count;

var (value, finalState) = computation.Run(0);
// value = 2, finalState = 2
```

### ReaderAsync - Async Dependency Injection

```csharp
// Define your environment
public record AppServices(IUserRepository Users, IEmailService Email);

// Build async computations that depend on services
var getUser = ReaderAsync<AppServices, User>.From(async services =>
    await services.Users.FindAsync(userId));

// Compose using LINQ
var program = 
    from user in getUser
    from orders in ReaderAsync<AppServices, List<Order>>.From(async s =>
        await s.Users.GetOrdersAsync(user.Id))
    select new UserWithOrders(user, orders);

// Execute with environment
var services = new AppServices(userRepo, emailService);
var result = await program.RunAsync(services);

// Error handling with retry
var resilient = getUser.RetryWithDelay(retries: 3, delay: TimeSpan.FromSeconds(1));
```

## Common Patterns

### Railway-Oriented Programming

Chain operations that might fail:

```csharp
var result = ParseInput(data)
    .Bind(Validate)
    .Bind(Transform)
    .Bind(Save)
    .Tap(x => Log($"Success: {x}"))
    .TapErr(e => Log($"Error: {e}"));
```

### LINQ Method Syntax (Recommended)

Use familiar `Select`, `SelectMany`, and `Where` methods for fluent composition:

```csharp
// Chain Option operations
var userEmail = FindUser(id)
    .Select(u => u.Email)                      // Transform: Option<User> → Option<string>
    .Where(email => email.Contains("@"))       // Filter: None if predicate fails
    .SelectMany(email => SendWelcome(email));  // Chain: bind to another Option

// Chain Result operations  
var order = ParseOrderId(input)
    .SelectMany(id => FetchOrder(id))          // Chain fallible operations
    .Select(order => order.Total);             // Transform success value

// Chain Try operations
var config = Try<string>.Of(() => File.ReadAllText("config.json"))
    .Select(json => JsonSerializer.Deserialize<Config>(json))
    .Where(cfg => cfg.IsOk);
```

### LINQ Query Syntax

For complex compositions, query syntax can be more readable:

```csharp
var result = from x in Option<int>.Some(10)
             from y in Option<int>.Some(20)
             where x > 5
             select x + y;
// Some(30)
```

### Async Operations

```csharp
var result = await Option<int>.Some(42)
    .MapAsync(async x => await ProcessAsync(x));
```

### Parallel Collection Operations

```csharp
// Process items in parallel with controlled concurrency
var users = await userIds.TraverseParallelAsync(
    id => FindUserAsync(id),
    maxDegreeOfParallelism: 4
);
// Some([users]) if all found, None if any not found

// Partition successes and failures in parallel
var (successes, failures) = await orders.PartitionParallelAsync(
    order => ProcessOrderAsync(order),
    maxDegreeOfParallelism: 8
);
```

## Source Generators

Create type-safe discriminated unions with auto-generated `Match` methods:

```bash
dotnet add package Monad.NET.SourceGenerators
```

```csharp
using Monad.NET;

[Union]
public abstract partial record PaymentMethod
{
    public partial record CreditCard(string Number, string Expiry) : PaymentMethod;
    public partial record PayPal(string Email) : PaymentMethod;
    public partial record BankTransfer(string AccountNumber) : PaymentMethod;
}

// Exhaustive pattern matching - compiler error if case is missing
string Describe(PaymentMethod method) => method.Match(
    creditCard: cc => $"Card ending in {cc.Number[^4..]}",
    payPal: pp => $"PayPal: {pp.Email}",
    bankTransfer: bt => $"Account: {bt.AccountNumber}"
);

// Safe casting with As{Case}()
var cardNumber = method.AsCreditCard()
    .Map(cc => cc.Number)
    .GetValueOr("N/A");
```

## Entity Framework Core Integration

Use `Option<T>` as entity properties with seamless database mapping:

```bash
dotnet add package Monad.NET.EntityFrameworkCore
```

```csharp
using Monad.NET;
using Monad.NET.EntityFrameworkCore;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Option<string> Email { get; set; }
    public Option<int> Age { get; set; }
}

// Query with Option-returning methods
var user = await context.Users.FirstOrNoneAsync(u => u.Id == id);
user.Match(
    someFunc: u => Console.WriteLine($"Found: {u.Name}"),
    noneFunc: () => Console.WriteLine("User not found")
);

// Available methods: FirstOrNone, SingleOrNone, ElementAtOrNone, LastOrNone
// All have async variants
```

## ASP.NET Core Integration

Convert monad types directly to HTTP responses:

```bash
dotnet add package Monad.NET.AspNetCore
```

```csharp
using Monad.NET.AspNetCore;

[HttpGet("{id}")]
public IActionResult GetUser(int id)
{
    return _userService.FindUser(id)
        .ToActionResult("User not found");
    // Returns 200 OK with user, or 404 Not Found
}

[HttpPost]
public IActionResult CreateUser(CreateUserRequest request)
{
    return ValidateRequest(request)
        .ToValidationProblemResult();
    // Returns 200 OK or 422 with ValidationProblemDetails
}
```

## Next Steps

- Read the full [README](../README.md) for detailed documentation
- Check out the [examples](../examples/Monad.NET.Examples/Program.cs)
- See the [API reference](API.md) for all available methods

## Learn More

New to functional programming patterns? These resources will help:

- [Functional Programming in C#](https://www.manning.com/books/functional-programming-in-c-sharp-second-edition) by Enrico Buonanno
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) by Scott Wlaschin
- [Parse, Don't Validate](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/) by Alexis King
- [Rust Error Handling](https://doc.rust-lang.org/book/ch09-00-error-handling.html) — Rust's `Option` and `Result` are nearly identical

---

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni)

**License:** MIT
