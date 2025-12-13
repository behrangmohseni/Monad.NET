# Monad.NET

[![NuGet](https://img.shields.io/nuget/v/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%2B-512BD4.svg)](https://dotnet.microsoft.com/)

**Monad.NET** is a functional programming library for .NET that provides a robust set of monadic types for building reliable, composable, and maintainable applications.

```csharp
// Transform nullable chaos into composable clarity
var result = user.ToOption()
    .Filter(u => u.IsActive)
    .Map(u => u.Email)
    .AndThen(email => SendWelcome(email))
    .Match(
        some: _ => "Email sent",
        none: () => "User not found or inactive"
    );
```

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni/)  
**License:** MIT — Free for commercial and personal use

---

## Table of Contents

- [Why Monad.NET?](#why-monadnet)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Types](#core-types)
  - [Option\<T\>](#optiont)
  - [Result\<T, E\>](#resultt-e)
  - [Either\<L, R\>](#eitherl-r)
  - [Validation\<T, E\>](#validationt-e)
  - [Try\<T\>](#tryt)
  - [RemoteData\<T, E\>](#remotedatat-e)
  - [NonEmptyList\<T\>](#nonemptylistt)
  - [Writer\<W, T\>](#writerw-t)
  - [Reader\<R, A\>](#readerr-a)
- [Advanced Usage](#advanced-usage)
- [API Reference](#api-reference)
- [Contributing](#contributing)
- [License](#license)

---

## Why Monad.NET?

Modern .NET applications demand reliability. Yet we continue to fight the same battles: null reference exceptions, swallowed errors, inconsistent error handling, and code that's difficult to reason about.

**Monad.NET addresses these challenges:**

| Problem             | Traditional Approach       | Monad.NET Solution                     |
|---------------------|----------------------------|----------------------------------------|
| Null references | `if (x != null)` checks scattered everywhere | `Option<T>` makes absence explicit and composable |
| Error handling | Try-catch blocks, exceptions as control flow | `Result<T, E>` treats errors as data |
| Validation | Return on first error, lose context | `Validation<T, E>` accumulates all errors |
| Async state | Boolean flags (`isLoading`, `hasError`) | `RemoteData<T, E>` models all four states |
| Empty collections | Runtime exceptions on `.First()` | `NonEmptyList<T>` guarantees at least one element |

### Design Principles

1. **Explicit over implicit** — No hidden nulls, no surprise exceptions
2. **Composition over inheritance** — Small, focused types that combine well
3. **Immutability by default** — All types are immutable and thread-safe
4. **Zero dependencies** — Only the .NET runtime, nothing else

---

## Installation

Requires **.NET 6.0** or later.

```bash
dotnet add package Monad.NET --version 1.0.0-alpha.1
```

**Package Manager Console:**
```powershell
Install-Package Monad.NET -Version 1.0.0-alpha.1
```

**PackageReference:**
```xml
<PackageReference Include="Monad.NET" Version="1.0.0-alpha.1" />
```

---

## Quick Start

```csharp
using Monad.NET;

// Option: Handle missing values without null
Option<User> FindUser(int id) => 
    _users.TryGetValue(id, out var user) 
        ? Option<User>.Some(user) 
        : Option<User>.None();

var greeting = FindUser(42)
    .Map(u => u.Name)
    .Map(name => $"Hello, {name}!")
    .UnwrapOr("Hello, guest!");

// Result: Explicit error handling
Result<Order, OrderError> PlaceOrder(Cart cart)
{
    if (cart.IsEmpty)
        return Result<Order, OrderError>.Err(OrderError.EmptyCart);
    
    if (!cart.HasValidPayment)
        return Result<Order, OrderError>.Err(OrderError.InvalidPayment);
    
    return Result<Order, OrderError>.Ok(new Order(cart));
}

var outcome = PlaceOrder(cart)
    .Map(order => order.Id)
    .Match(
        ok: id => $"Order #{id} placed successfully",
        err: error => $"Failed: {error}"
    );

// Validation: Collect ALL errors at once
var registration = ValidateUsername(form.Username)
    .Apply(ValidateEmail(form.Email), (u, e) => (u, e))
    .Apply(ValidatePassword(form.Password), (partial, p) => 
        new Registration(partial.u, partial.e, p));

registration.Match(
    valid: reg => CreateAccount(reg),
    invalid: errors => ShowErrors(errors) // Shows ALL validation errors
);
```

---

## Core Types

### Option\<T\>

Represents a value that may or may not exist. Use instead of `null`.

```csharp
// Creation
var some = Option<int>.Some(42);
var none = Option<int>.None();
var fromNullable = possiblyNull.ToOption();  // Extension method

// Transformation
var doubled = some.Map(x => x * 2);                    // Some(84)
var filtered = some.Filter(x => x > 100);              // None
var chained = some.AndThen(x => LookupValue(x));       // Chains Option-returning functions

// Extraction
var value = some.UnwrapOr(0);                          // 42
var computed = none.UnwrapOrElse(() => ComputeDefault()); // Lazy evaluation

// Pattern matching
var message = some.Match(
    some: v => $"Found: {v}",
    none: () => "Not found"
);
```

**When to use:** Any time you would return `null` or use `Nullable<T>`.

---

### Result\<T, E\>

Represents either success (`Ok`) or failure (`Err`) with a typed error.

```csharp
// Creation
var ok = Result<int, string>.Ok(42);
var err = Result<int, string>.Err("Something went wrong");

// Safe exception handling
var parsed = ResultExtensions.Try(() => int.Parse(input));
var fetched = await ResultExtensions.TryAsync(() => httpClient.GetAsync(url));

// Railway-oriented programming
var pipeline = ParseInput(raw)
    .AndThen(Validate)
    .AndThen(Transform)
    .AndThen(Save)
    .Tap(result => _logger.LogInformation("Saved: {Id}", result.Id))
    .TapErr(error => _logger.LogError("Failed: {Error}", error));

// Recovery strategies
var recovered = err.OrElse(e => FallbackStrategy(e));
var withDefault = err.UnwrapOr(defaultValue);
```

**When to use:** Operations that can fail with meaningful error information.

---

### Either\<L, R\>

Represents a value of one of two types. More general than `Result`.

```csharp
var right = Either<ValidationError, User>.Right(user);
var left = Either<ValidationError, User>.Left(error);

// Transform either side
var mapped = either.BiMap(
    left: err => err.Message,
    right: user => user.Id
);

// Swap sides
var swapped = either.Swap();

// Conversions
var asResult = either.ToResult();
var asOption = either.ToOption();  // Right → Some, Left → None
```

**When to use:** When both sides represent valid outcomes, not just success/failure.

---

### Validation\<T, E\>

Unlike `Result`, validation **accumulates all errors** instead of short-circuiting.

```csharp
Validation<string, ValidationError> ValidateEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return Validation<string, ValidationError>.Invalid(
            new ValidationError("Email", "Email is required"));
    
    if (!email.Contains('@'))
        return Validation<string, ValidationError>.Invalid(
            new ValidationError("Email", "Invalid email format"));
    
    return Validation<string, ValidationError>.Valid(email);
}

// Combine validations — errors accumulate!
var user = ValidateName(form.Name)
    .Apply(ValidateEmail(form.Email), (name, email) => (name, email))
    .Apply(ValidateAge(form.Age), (partial, age) => 
        new UserDto(partial.name, partial.email, age));

// All three can fail, and you'll see ALL errors
user.Match(
    valid: dto => CreateUser(dto),
    invalid: errors => 
    {
        foreach (var error in errors)
            Console.WriteLine($"{error.Field}: {error.Message}");
    }
);
```

**When to use:** Form validation, input validation, anywhere you need to show all errors at once.

---

### Try\<T\>

Wraps computations that might throw, converting exceptions to values.

```csharp
// Capture exceptions
var result = Try<int>.Of(() => int.Parse("not a number"));
// → Failure(FormatException)

var asyncResult = await Try<string>.OfAsync(() => 
    httpClient.GetStringAsync(url));

// Recovery
var recovered = result
    .Recover(ex => -1)                     // Returns Try<int>.Success(-1)
    .Map(x => x * 2);

// Filtering with custom exception
var positive = Try<int>.Of(() => int.Parse(input))
    .Filter(x => x > 0, "Value must be positive");

// Conversion
var asResult = result.ToResult(ex => ex.Message);
var asOption = result.ToOption();  // Success → Some, Failure → None
```

**When to use:** Interfacing with code that throws exceptions, parsing, I/O operations.

---

### RemoteData\<T, E\>

Models the four states of asynchronous data: **NotAsked**, **Loading**, **Success**, **Failure**.

```csharp
// State management
RemoteData<User, ApiError> userData = RemoteData<User, ApiError>.NotAsked();

async Task LoadUser(int userId)
{
    userData = RemoteData<User, ApiError>.Loading();
    StateHasChanged();
    
    try
    {
        var user = await _api.GetUserAsync(userId);
        userData = RemoteData<User, ApiError>.Success(user);
    }
    catch (ApiException ex)
    {
        userData = RemoteData<User, ApiError>.Failure(ex.Error);
    }
    
    StateHasChanged();
}

// Rendering
@userData.Match(
    notAsked: () => @<button @onclick="() => LoadUser(1)">Load User</button>,
    loading: () => @<div class="spinner">Loading...</div>,
    success: user => @<UserProfile User="@user" />,
    failure: error => @<ErrorDisplay Error="@error" OnRetry="() => LoadUser(1)" />
)
```

**When to use:** UI state for async operations, replacing boolean flag combinations.

---

### NonEmptyList\<T\>

A list guaranteed to have at least one element. `Head` and `Reduce` are always safe.

```csharp
// Creation
var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
var single = NonEmptyList<int>.Of(42);

// From existing collection (returns Option)
var maybeList = NonEmptyList<int>.FromEnumerable(existingList);
// → Some(list) or None if empty

// Safe operations — no exceptions possible
var first = list.Head;                           // 1 (always exists)
var last = list.Last();                          // 5 (always exists)
var sum = list.Reduce((a, b) => a + b);          // 15 (no seed needed)

// Transformations
var doubled = list.Map(x => x * 2);
var expanded = list.FlatMap(x => NonEmptyList<int>.Of(x, x * 10));

// Filter returns Option (result might be empty)
var filtered = list.Filter(x => x > 10);         // None
```

**When to use:** When empty collections are invalid states (config items, selected options, etc.).

---

### Writer\<W, T\>

Computations that produce a value alongside accumulated output (logs, traces, metrics).

```csharp
// Computation with logging
var computation = Writer<List<string>, int>.Tell(1, new List<string> { "Started with 1" })
    .FlatMap(
        x => Writer<List<string>, int>.Tell(x * 2, new List<string> { $"Doubled to {x * 2}" }),
        (log1, log2) => log1.Concat(log2).ToList()
    )
    .FlatMap(
        x => Writer<List<string>, int>.Tell(x + 10, new List<string> { $"Added 10, result: {x + 10}" }),
        (log1, log2) => log1.Concat(log2).ToList()
    );

Console.WriteLine($"Result: {computation.Value}");  // 12
Console.WriteLine($"Log: {string.Join(" → ", computation.Log)}");
// Started with 1 → Doubled to 2 → Added 10, result: 12
```

**When to use:** Audit trails, computation tracing, accumulating metadata.

---

### Reader\<R, A\>

Computations that depend on a shared environment. Functional dependency injection.

```csharp
// Define your environment
public record AppServices(
    IUserRepository Users,
    IEmailService Email,
    ILogger Logger
);

// Build computations that depend on services
var sendWelcome = Reader<AppServices, Task>.From(async services =>
{
    var users = await services.Users.GetNewUsersAsync();
    
    foreach (var user in users)
    {
        await services.Email.SendWelcomeAsync(user.Email);
        services.Logger.LogInformation("Sent welcome to {Email}", user.Email);
    }
});

// Compose readers
var workflow = Reader<AppServices, string>.Asks(s => s.Users)
    .FlatMap(repo => Reader<AppServices, string>.From(async s =>
    {
        var count = await repo.CountAsync();
        s.Logger.LogInformation("Total users: {Count}", count);
        return $"Processed {count} users";
    }));

// Execute with environment
var services = new AppServices(userRepo, emailService, logger);
await sendWelcome.Run(services);
```

**When to use:** Passing configuration/services through call chains without parameter drilling.

---

## Advanced Usage

### LINQ Query Syntax

All monads support LINQ for natural composition:

```csharp
// Option
var result = from user in FindUser(id)
             from profile in LoadProfile(user.Id)
             where profile.IsComplete
             select new UserView(user, profile);

// Result
var order = from cart in ValidateCart(input)
            from payment in ProcessPayment(cart)
            from confirmation in CreateOrder(cart, payment)
            select confirmation;
```

### Async Extensions

Seamless async/await integration:

```csharp
var result = await Option<int>.Some(userId)
    .MapAsync(async id => await _repo.FindAsync(id))
    .AndThenAsync(async user => await ValidateAsync(user))
    .MapAsync(async user => await EnrichAsync(user));
```

### Collection Operations

Work with sequences of monads:

```csharp
// Sequence: [Option<T>] → Option<[T]>
var options = new[] { Option<int>.Some(1), Option<int>.Some(2), Option<int>.Some(3) };
var sequenced = options.Sequence();  // Some([1, 2, 3])

// Traverse: Map + Sequence in one pass
var validated = userIds.Traverse(id => ValidateUser(id));

// Partition: Separate successes and failures
var results = items.Select(Process);
var (successes, failures) = results.Partition();

// Choose: Filter and unwrap
var values = options.Choose();  // Only the Some values
```

---

## API Reference

Full API documentation is available in [docs/API.md](docs/API.md).

---

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

**Development requirements:**
- .NET 8.0 SDK or later
- Your preferred IDE (Visual Studio, Rider, VS Code)

```bash
git clone https://github.com/AlirezaEiji191379/Monad.NET.git
cd Monad.NET
dotnet build
dotnet test
```

---

## License

This project is licensed under the **MIT License**.

You are free to use, modify, and distribute this library in both commercial and open-source projects. See [LICENSE](LICENSE) for details.

---

**Monad.NET** — Functional programming for the pragmatic .NET developer.

[Documentation](https://behrangmohseni.com/monad_dot_net) · [NuGet](https://www.nuget.org/packages/Monad.NET/) · [Issues](https://github.com/AlirezaEiji191379/Monad.NET/issues)
