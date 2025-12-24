# Monad.NET

[![NuGet](https://img.shields.io/nuget/v/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![Build](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml)
[![CodeQL](https://github.com/behrangmohseni/Monad.NET/actions/workflows/codeql.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/behrangmohseni/Monad.NET/graph/badge.svg)](https://codecov.io/gh/behrangmohseni/Monad.NET)
[![CodeFactor](https://www.codefactor.io/repository/github/behrangmohseni/monad.net/badge/main)](https://www.codefactor.io/repository/github/behrangmohseni/monad.net)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%2B-512BD4.svg)](https://dotnet.microsoft.com/)

**Monad.NET** is a functional programming library for .NET. Option, Result, Either, Validation, and more — with zero dependencies.

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
  - [ReaderAsync\<R, A\>](#readerasyncr-a)
  - [State\<S, A\>](#states-a)
  - [IO\<T\>](#iot)
- [Advanced Usage](#advanced-usage)
- [Source Generators](#source-generators)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Entity Framework Core Integration](#entity-framework-core-integration)
- [Real-World Examples](#real-world-examples)
- [Samples](#samples)
- [Performance](#performance)
- [Resources](#resources)
- [FAQ](#faq)
- [API Reference](#api-reference)
- [Guides](#guides)
- [Compatibility](#compatibility)
- [Contributing](#contributing)
- [License](#license)

---

## Why Monad.NET?

Modern C# has excellent features—nullable reference types, pattern matching, records. So why use Monad.NET?

**The short answer:** Composability. While C# handles individual cases well, chaining operations that might fail, be absent, or need validation quickly becomes verbose. Monad.NET provides a unified API for composing these operations elegantly.

### Honest Comparisons with Modern C#

#### Optional Values: `Option<T>` vs Nullable Reference Types

**Modern C# (NRT enabled):**
```csharp
User? user = FindUser(id);
if (user is not null)
{
    Profile? profile = user.GetProfile();
    if (profile is not null)
    {
        return profile.Email;  // Still might be null!
    }
}
return "default@example.com";
```

**With Monad.NET:**
```csharp
return FindUser(id)
    .AndThen(user => user.GetProfile())
    .Map(profile => profile.Email)
    .UnwrapOr("default@example.com");
```

**Verdict:** NRTs catch null issues at compile time—use them! But `Option<T>` shines when you need to *chain* operations or *transform* optional values. If you're writing nested null checks, Option is cleaner.

---

#### Error Handling: `Result<T, E>` vs Exceptions

**Modern C# with exceptions:**
```csharp
public Order ProcessOrder(OrderRequest request)
{
    try
    {
        var validated = ValidateOrder(request);      // throws ValidationException
        var inventory = ReserveInventory(validated); // throws InventoryException
        var payment = ChargePayment(inventory);      // throws PaymentException
        return CreateOrder(payment);
    }
    catch (ValidationException ex) { /* handle */ }
    catch (InventoryException ex) { /* handle */ }
    catch (PaymentException ex) { /* handle */ }
}
```

**With Monad.NET:**
```csharp
public Result<Order, OrderError> ProcessOrder(OrderRequest request)
{
    return ValidateOrder(request)
        .AndThen(ReserveInventory)
        .AndThen(ChargePayment)
        .AndThen(CreateOrder);
}
```

**Verdict:** Exceptions are fine for *exceptional* situations (network failures, disk errors). Use `Result<T, E>` when failure is *expected* (validation errors, business rule violations). The signature `Result<Order, OrderError>` tells callers exactly what can go wrong—no surprises.

---

#### Validation: `Validation<T, E>` vs FluentValidation

**With FluentValidation (industry standard):**
```csharp
public class UserValidator : AbstractValidator<UserRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}

// Usage
var result = await validator.ValidateAsync(request);
if (!result.IsValid)
    return BadRequest(result.Errors);
```

**With Monad.NET:**
```csharp
var user = ValidateName(request.Name)
    .Apply(ValidateEmail(request.Email), (name, email) => (name, email))
    .Apply(ValidateAge(request.Age), (partial, age) => new User(partial.name, partial.email, age));
```

**Verdict:** FluentValidation is battle-tested and has more features (async rules, dependency injection, localization). Use it for complex scenarios. `Validation<T, E>` is lighter, has no dependencies, and works well with other Monad.NET types. Choose based on your needs.

---

#### Discriminated Unions: The Missing Feature in C#

**The Problem:** C# still lacks native discriminated unions (sum types) as of [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14). Despite adding extension members, null-conditional assignment, field-backed properties, and other features—discriminated unions didn't make the cut. This remains one of the [most requested language features](https://github.com/dotnet/csharplang/issues/8928), with the proposal actively discussed by the C# Language Design Team. F#, Rust, Swift, Kotlin, and TypeScript all have this feature. C# developers have been waiting for years.

**Why it matters:**
```csharp
// Without discriminated unions, you're stuck with:
// 1. Class hierarchies (verbose, not exhaustive)
// 2. Enums + switch (no associated data)
// 3. Tuples (loses type safety)
// 4. Exceptions (wrong tool for expected outcomes)
```

**With OneOf library (popular workaround):**
```csharp
OneOf<Success, NotFound, ValidationError> GetUser(int id) { ... }

result.Switch(
    success => Ok(success.User),
    notFound => NotFound(),
    error => BadRequest(error.Message)
);
```

**With Monad.NET Source Generators:**
```csharp
[Union]
public abstract partial record GetUserResult
{
    public partial record Success(User User) : GetUserResult;
    public partial record NotFound : GetUserResult;
    public partial record ValidationError(string Message) : GetUserResult;
}

// Exhaustive matching - compiler ensures all cases handled
result.Match(
    success: s => Ok(s.User),
    notFound: _ => NotFound(),
    validationError: e => BadRequest(e.Message)
);

// Plus additional utilities generated automatically:
if (result.IsSuccess) { ... }
var user = result.AsSuccess().Map(s => s.User);
```

**Verdict:** Until C# gets native discriminated unions (proposed but no confirmed timeline), you need a library. OneOf is battle-tested. Monad.NET uses source generators for zero runtime overhead and generates richer utilities (`Is{Case}`, `As{Case}()`, `Map`, `Tap`). Both work well—pick Monad.NET if you're already using Option/Result from this library.

---

### Design Principles

1. **Explicit over implicit** — No hidden nulls, no surprise exceptions
2. **Composition over inheritance** — Small, focused types that combine well
3. **Immutability by default** — All types are immutable and thread-safe
4. **Zero dependencies** — Only the .NET runtime, nothing else

### Which Monad Should I Use?

**Sorted by how often you'll need them:**

| Frequency | Scenario | Use This |
|-----------|----------|----------|
| ⭐⭐⭐⭐⭐ | A value might be missing | `Option<T>` |
| ⭐⭐⭐⭐⭐ | An operation can fail with a typed error | `Result<T, E>` |
| ⭐⭐⭐⭐ | Need to show ALL validation errors at once | `Validation<T, E>` |
| ⭐⭐⭐⭐ | Wrapping code that throws exceptions | `Try<T>` |
| ⭐⭐⭐ | A list must have at least one item | `NonEmptyList<T>` |
| ⭐⭐⭐ | UI state for async data loading (Blazor) | `RemoteData<T, E>` |
| ⭐⭐ | Return one of two different types | `Either<L, R>` |
| ⭐⭐ | Compose async operations with shared dependencies | `ReaderAsync<R, A>` |
| ⭐⭐ | Dependency injection without DI container | `Reader<R, A>` |
| ⭐ | Need to accumulate logs/traces alongside results | `Writer<W, T>` |
| ⭐ | Thread state through pure computations | `State<S, A>` |
| ⭐ | Defer and compose side effects | `IO<T>` |

### Language Inspirations

These types come from functional programming languages. Here's the lineage:

| Monad.NET | F# | Rust | Haskell | Notes |
|-----------|-----|------|---------|-------|
| `Option<T>` | `Option<'T>` | `Option<T>` | `Maybe a` | Universal pattern for optional values |
| `Result<T, E>` | `Result<'T, 'E>` | `Result<T, E>` | `Either a b`* | *Haskell uses Either with Left=Error convention |
| `Either<L, R>` | `Choice<'T1, 'T2>` | — | `Either a b` | General sum type for two alternatives |
| `Validation<T, E>` | FsToolkit.ErrorHandling | — | `Validation` | Error accumulation (vs short-circuit) |
| `Try<T>` | `try...with` | `Result<T, Error>` | `IO`/`ExceptT` | Exception capture as values |
| `NonEmptyList<T>` | FSharpPlus | `NonEmpty<T>`† | `NonEmpty a` | †Via `nonempty` crate |
| `RemoteData<T, E>` | — | — | — | Originated in Elm (Kris Jenkins) |
| `Writer<W, T>` | FSharpPlus | — | `Writer w a` | Classic Haskell monad |
| `Reader<R, A>` | FSharpPlus | — | `Reader r a` | Dependency injection, FP style |
| `State<S, A>` | FSharpPlus | — | `State s a` | Threading state through computations |
| `IO<T>` | `Async<'T>` | `Future<T>` | `IO a` | The foundational effect type in Haskell |

**Key insight:** Rust's `Option` and `Result` are nearly identical to Monad.NET's versions—same names, same semantics. If you know Rust, you already know how to use these types.

---

## Installation

Requires **.NET 6.0** or later.

```bash
dotnet add package Monad.NET
```

**Package Manager Console:**
```powershell
Install-Package Monad.NET
```

**PackageReference:**
```xml
<PackageReference Include="Monad.NET" />
```

### ASP.NET Core Integration (Optional)

For ASP.NET Core projects, install the integration package:

```bash
dotnet add package Monad.NET.AspNetCore
```

This adds `IActionResult` extensions, middleware, and `ValidationProblemDetails` support.

### Source Generators (Optional)

For compile-time discriminated union support with exhaustive pattern matching:

```bash
dotnet add package Monad.NET.SourceGenerators
```

This generates `Match` methods for your custom union types, ensuring all cases are handled at compile time.

### Entity Framework Core Integration (Optional)

For EF Core support with `Option<T>` properties and query extensions:

```bash
dotnet add package Monad.NET.EntityFrameworkCore
```

This adds value converters, query extensions like `FirstOrNone()`, and model configuration helpers.

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

// Conditional creation with When/Unless guards
var discount = OptionExtensions.When(order.Total > 100, () => 0.1m);
// Some(0.1m) if order > 100, None otherwise

var warning = OptionExtensions.Unless(user.HasVerifiedEmail, () => "Please verify email");
// Some("Please verify...") if NOT verified, None otherwise

// Transformation
var doubled = some.Map(x => x * 2);                    // Some(84)
var filtered = some.Filter(x => x > 100);              // None
var chained = some.AndThen(x => LookupValue(x));       // Chains Option-returning functions

// Extraction
var value = some.UnwrapOr(0);                          // 42
var computed = none.UnwrapOrElse(() => ComputeDefault()); // Lazy evaluation

// TryGet pattern (familiar C# idiom)
if (some.TryGet(out var result))
{
    Console.WriteLine($"Got: {result}");               // Prints: Got: 42
}

// Side effects with Tap (logging, debugging)
var processed = some
    .Tap(x => Console.WriteLine($"Processing: {x}"))
    .Map(x => x * 2)
    .TapNone(() => Console.WriteLine("No value to process"));

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

// TryGet pattern (familiar C# idiom)
if (ok.TryGet(out var value))
{
    Console.WriteLine($"Success: {value}");
}
if (err.TryGetError(out var error))
{
    Console.WriteLine($"Error: {error}");
}

// Combine multiple results
var combined = ResultExtensions.Combine(
    GetUser(userId),
    GetOrder(orderId),
    (user, order) => new UserOrder(user, order)
);

// Or combine into tuples
var tuple = ResultExtensions.Combine(result1, result2, result3);
// → Result<(T1, T2, T3), Error>

// Batch operations
var allResults = ResultExtensions.Combine(ids.Select(GetById));
// → Result<IReadOnlyList<T>, Error>
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

// TryGet pattern (familiar C# idiom)
if (result.TryGet(out var value))
{
    Console.WriteLine($"Parsed: {value}");
}
if (result.TryGetException(out var ex))
{
    Console.WriteLine($"Exception: {ex.Message}");
}
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

// Side effects with Tap
list.Tap(x => Console.WriteLine(x))              // Logs each element
    .TapIndexed((x, i) => Console.WriteLine($"{i}: {x}"));

// Filter returns Option (result might be empty)
var filtered = list.Filter(x => x > 10);         // None
```

**Methods:** `Map`, `MapIndexed`, `FlatMap`, `Filter`, `Reduce`, `Fold`, `Tap`, `TapIndexed`

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

// Side effects with Tap
var debugWriter = Writer<string, int>.Of(42, "init")
    .Tap(x => Console.WriteLine($"Value: {x}"))
    .TapLog(log => Console.WriteLine($"Log so far: {log}"));
```

**Methods:** `Map`, `FlatMap`, `BiMap`, `Match`, `Tap`, `TapLog`

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
var getUserCount = Reader<AppServices, int>.Asks(s => s.Users.Count());

// Compose readers
var workflow = Reader<AppServices, string>.Asks(s => s.Users)
    .FlatMap(repo => Reader<AppServices, string>.From(s =>
    {
        var count = repo.Count();
        s.Logger.LogInformation("Total users: {Count}", count);
        return $"Processed {count} users";
    }));

// Execute with environment
var services = new AppServices(userRepo, emailService, logger);
var result = workflow.Run(services);

// Side effects with Tap
var debugReader = Reader<AppServices, string>.Asks(s => s.Users.GetName())
    .Tap(name => Console.WriteLine($"Got user: {name}"))
    .TapEnv(env => Console.WriteLine($"Using logger: {env.Logger}"));

// Convert to async
var asyncReader = reader.ToAsync();
```

**Methods:** `Map`, `FlatMap`, `Tap`, `TapEnv`, `WithEnvironment`, `ToAsync`

**When to use:** Passing configuration/services through call chains without parameter drilling.

---

### ReaderAsync\<R, A\>

Asynchronous computations that depend on a shared environment. The async variant of `Reader<R, A>`.

```csharp
// Define your environment
public record AppServices(
    IUserRepository Users,
    IEmailService Email,
    ILogger Logger
);

// Build async computations that depend on services
var getUser = ReaderAsync<AppServices, User>.From(async services =>
    await services.Users.FindAsync(userId));

// Compose async readers using LINQ
var program = 
    from user in getUser
    from orders in ReaderAsync<AppServices, List<Order>>.From(async s =>
        await s.Users.GetOrdersAsync(user.Id))
    select new UserWithOrders(user, orders);

// Execute with environment
var services = new AppServices(userRepo, emailService, logger);
var result = await program.RunAsync(services);

// Extract values from environment asynchronously
var userCount = ReaderAsync<AppServices, int>.AsksAsync(async s => 
    await s.Users.CountAsync());

// Error handling with Attempt
var safe = getUser.Attempt();  // ReaderAsync<AppServices, Try<User>>

// Retry with delay
var resilient = getUser.RetryWithDelay(retries: 3, delay: TimeSpan.FromSeconds(1));

// Parallel execution
var parallel = ReaderAsync.Parallel(getUser, getOrders);
// → ReaderAsync<AppServices, (User, List<Order>)>

// Side effects
var logged = getUser
    .Tap(user => Console.WriteLine($"Found: {user.Name}"))
    .TapAsync(async user => await LogAsync(user))
    .TapEnv(env => Console.WriteLine($"Using: {env.Logger}"));

// Transform environment
var narrowed = getUser.WithEnvironment<LargerServices>(larger => larger.App);
```

**Factory Methods:**
- `From(Func<R, Task<A>>)` — Create from async function
- `FromReader(Reader<R, A>)` — Convert from sync Reader
- `Pure(value)` — Constant value, ignores environment
- `Ask()` — Returns the environment itself
- `Asks(selector)` — Extract value from environment (sync)
- `AsksAsync(selector)` — Extract value from environment (async)

**Composition:**
- `Map(f)` / `MapAsync(f)` — Transform the result
- `FlatMap(f)` / `AndThen(f)` / `Bind(f)` — Chain computations
- `FlatMapAsync(f)` — Chain with async binder
- `Zip(other)` / `Zip(other, combiner)` — Combine two readers
- `Tap(action)` / `TapAsync(action)` — Side effects with result
- `TapEnv(action)` / `TapEnvAsync(action)` — Side effects with environment

**Error Handling:**
- `Attempt()` — Returns `ReaderAsync<R, Try<A>>`
- `OrElse(fallback)` — Use fallback reader on exception
- `OrElse(value)` — Use fallback value on exception
- `Retry(n)` — Retry n times on failure
- `RetryWithDelay(n, delay)` — Retry with delay between attempts

**Environment Transformation:**
- `WithEnvironment<R2>(transform)` — Transform environment type
- `WithEnvironmentAsync<R2>(transform)` — Async environment transformation

**Parallel Execution:**
- `ReaderAsync.Parallel(r1, r2)` — Run two readers in parallel
- `ReaderAsync.Parallel(r1, r2, r3)` — Run three readers in parallel
- `ReaderAsync.Parallel(readers)` — Run collection in parallel

**Collection Operations:**
- `readers.Sequence()` — Sequential execution
- `readers.SequenceParallel()` — Parallel execution
- `items.Traverse(selector)` — Map and sequence
- `items.TraverseParallel(selector)` — Map and parallel sequence

**When to use:** Async dependency injection, database access, HTTP clients, any async computation that depends on shared services.

---

### State\<S, A\>

Computations that thread state through a sequence of operations without mutable variables.

```csharp
// Counter example using State monad
var increment = State<int, Unit>.Modify(s => s + 1);
var getCount = State<int, int>.Get();

// Chain operations that read/write state
var computation = 
    from _ in increment
    from __ in increment
    from ___ in increment
    from count in getCount
    select count;

var (value, finalState) = computation.Run(0);
// value = 3, finalState = 3

// Stack example
State<List<int>, Unit> Push(int x) => 
    State<List<int>, Unit>.Modify(stack => new List<int>(stack) { x });

State<List<int>, Option<int>> Pop() => 
    State<List<int>, Option<int>>.Of(stack =>
    {
        if (stack.Count == 0)
            return (Option<int>.None(), stack);
        
        var value = stack[^1];
        var newStack = stack.Take(stack.Count - 1).ToList();
        return (Option<int>.Some(value), newStack);
    });

// Push 1, 2, 3 then pop twice
var stackOps = 
    from _ in Push(1)
    from __ in Push(2)
    from ___ in Push(3)
    from a in Pop()  // Returns 3
    from b in Pop()  // Returns 2
    select (a, b);

var result = stackOps.Run(new List<int>());
// result.Value = (Some(3), Some(2))
// result.State = [1]
```

**Factory Methods:**
- `State<S, A>.Pure(value)` — Return value without modifying state
- `State<S, S>.Get()` — Get current state as the value
- `State<S, Unit>.Put(newState)` — Replace state
- `State<S, Unit>.Modify(f)` — Transform state using a function
- `State<S, A>.Gets(selector)` — Extract a value from the state

**Composition:**
- `Map(f)` — Transform the value
- `AndThen(f)` / `FlatMap(f)` / `Bind(f)` — Chain computations
- `Zip(other)` / `ZipWith(other, f)` — Combine two state computations
- `Tap(action)` — Execute side effect with value (logging/debugging)
- `TapState(action)` — Execute side effect with state

**Execution:**
- `Run(initialState)` — Get both value and final state
- `Eval(initialState)` — Get only the value (discard state)
- `Exec(initialState)` — Get only the final state (discard value)

**When to use:** Simulators, interpreters, random number generators, stack-based computations, game state, any computation that needs to pass state through without mutable variables.

---

### IO\<T\>

Defers side effects, making code purely functional until explicitly executed.

```csharp
// Describe computations without executing them
var readLine = IO<string>.Of(() => Console.ReadLine()!);
IO<Unit> WriteLine(string msg) => 
    IO<Unit>.Of(() => { Console.WriteLine(msg); return Unit.Default; });

// Compose a program
var program = 
    from _ in WriteLine("What is your name?")
    from name in readLine
    from __ in WriteLine($"Hello, {name}!")
    select Unit.Default;

// Nothing happens until Run() is called
program.Run();

// Built-in helpers
var time = IO.Now().Run();              // Get current time
var guid = IO.NewGuid().Run();          // Generate GUID
var random = IO.Random(1, 100).Run();   // Random number
var env = IO.GetEnvironmentVariable("PATH").Run(); // Option<string>

// Error handling with Attempt
var riskyOp = IO<int>.Of(() => int.Parse("not a number"))
    .Attempt();  // IO<Try<int>>

var result = riskyOp.Run();
result.Match(
    success: n => Console.WriteLine($"Parsed: {n}"),
    failure: ex => Console.WriteLine($"Error: {ex.Message}")
);

// Retry with fallback
var resilient = IO<string>.Of(() => CallExternalApi())
    .Retry(3)                           // Retry up to 3 times
    .OrElse("fallback value");          // Fallback on failure

// Async version
var asyncOp = IOAsync<int>.Of(async () => 
{
    await Task.Delay(100);
    return 42;
});

var value = await asyncOp.RunAsync();

// Parallel execution
var (a, b) = IO.Parallel(
    IO.Of(() => ComputeA()),
    IO.Of(() => ComputeB())
).Run();

// Race - first to complete wins
var fastest = IO.Race(
    IO.Of(() => SlowOperation()),
    IO.Of(() => FastOperation())
).Run();
```

**Factory Methods:**
- `IO<T>.Of(effect)` — Create from effect function
- `IO<T>.Pure(value)` / `Return(value)` — Create with pure value
- `IO<T>.Delay(effect)` — Alias for `Of`, emphasizes laziness
- `IO.Execute(action)` — Execute action, return Unit

**Composition:**
- `Map(f)` — Transform the result
- `AndThen(f)` / `FlatMap(f)` / `Bind(f)` — Chain IO operations
- `Tap(action)` — Execute side effect, keep value
- `Apply(ioFunc)` — Apply function in IO to value
- `Zip(other)` / `ZipWith(other, f)` — Combine two IOs

**Execution:**
- `Run()` — Execute synchronously
- `RunAsync(ct)` — Execute asynchronously

**Error Handling:**
- `Attempt()` — Returns `IO<Try<T>>` (captures exceptions)
- `OrElse(fallback)` — Use fallback on exception
- `Retry(n)` — Retry n times on failure
- `RetryWithDelay(n, delay)` — Retry with delay (returns `IOAsync<T>`)

**Utility:**
- `Replicate(n)` — Repeat effect n times, collect results
- `ToAsync()` — Convert to `IOAsync<T>`

**When to use:** Deferring side effects, functional core/imperative shell pattern, testable IO operations, building DSLs, composing effectful computations.

---

## Advanced Usage

### LINQ Support

All monads support LINQ extension methods (`Select`, `SelectMany`, `Where`) for fluent composition.

#### Method Syntax (Recommended)

The method syntax is familiar to most .NET developers and works great with IntelliSense:

```csharp
// Option - chain transformations with Select and SelectMany
var userEmail = FindUser(id)
    .Select(user => user.Email)                    // Map: Option<User> → Option<string>
    .Where(email => email.Contains("@"))           // Filter: keep only valid emails
    .SelectMany(email => ValidateEmail(email));    // FlatMap: chain Option-returning functions

// Result - compose fallible operations
var order = ValidateCart(input)
    .SelectMany(cart => ProcessPayment(cart))      // Chain to next Result
    .Select(payment => CreateOrderDto(payment));   // Transform success value

// Try - safely chain operations that might throw
var parsed = Try<string>.Of(() => ReadFile(path))
    .Select(content => content.Trim())
    .SelectMany(content => Try<int>.Of(() => int.Parse(content)))
    .Where(value => value > 0);
```

#### Query Syntax

For complex compositions with multiple bindings, query syntax can be more readable:

```csharp
// Option - multiple from clauses bind values
var result = from user in FindUser(id)
             from profile in LoadProfile(user.Id)
             where profile.IsComplete
             select new UserView(user, profile);

// Result - railway-oriented composition
var order = from cart in ValidateCart(input)
            from payment in ProcessPayment(cart)
            from confirmation in CreateOrder(cart, payment)
            select confirmation;

// Validation - note: query syntax short-circuits; use Apply for error accumulation
var user = from name in ValidateName(input.Name)
           from email in ValidateEmail(input.Email)
           select new User(name, email);
```

#### Available LINQ Methods

| Monad | Select | SelectMany | Where |
|-------|--------|------------|-------|
| `Option<T>` | ✅ Map value | ✅ Chain Options | ✅ Filter by predicate |
| `Result<T,E>` | ✅ Map Ok value | ✅ Chain Results | ✅ With error value/factory |
| `Either<L,R>` | ✅ Map Right | ✅ Chain Eithers | ✅ With Left value |
| `Try<T>` | ✅ Map success | ✅ Chain Trys | ✅ Filter with predicate |
| `Validation<T,E>` | ✅ Map valid | ✅ Chain (short-circuits) | — |
| `RemoteData<T,E>` | ✅ Map success | ✅ Chain RemoteData | — |
| `Writer<W,T>` | ✅ Map value | ✅ Chain with log combine | — |

### When/Unless Guards

Create Options conditionally without verbose if/else:

```csharp
// When: returns Some if condition is true
var discount = OptionExtensions.When(order.Total > 100, () => 0.1m);
var adminPanel = OptionExtensions.When(user.IsAdmin, new AdminPanel());

// Unless: returns Some if condition is false (opposite of When)
var warning = OptionExtensions.Unless(user.HasVerifiedEmail, () => "Please verify email");
var fallback = OptionExtensions.Unless(cache.HasValue, () => LoadFromDatabase());

// Lazy evaluation - factory only called when needed
var expensive = OptionExtensions.When(shouldCompute, () => ExpensiveOperation());
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

### Parallel Collection Operations

Process collections in parallel with controlled concurrency:

```csharp
// TraverseParallelAsync for Options - process all items in parallel
var users = await userIds.TraverseParallelAsync(
    id => FindUserAsync(id),
    maxDegreeOfParallelism: 4
);
// Some([users]) if all found, None if any not found

// SequenceParallelAsync - await multiple Option tasks in parallel
var tasks = userIds.Select(id => GetUserAsync(id));
var result = await tasks.SequenceParallelAsync(maxDegreeOfParallelism: 4);

// TraverseParallelAsync for Results
var orders = await orderIds.TraverseParallelAsync(
    id => ProcessOrderAsync(id),
    maxDegreeOfParallelism: 8
);
// Ok([results]) if all succeed, Err(firstError) if any fail

// ChooseParallelAsync - filter Some values in parallel
var validUsers = await userIds.ChooseParallelAsync(
    id => TryGetUserAsync(id),
    maxDegreeOfParallelism: 4
);
// Returns only the users that were found

// PartitionParallelAsync - separate successes and failures
var (successes, failures) = await orders.PartitionParallelAsync(
    order => ProcessOrderAsync(order),
    maxDegreeOfParallelism: 8
);
```

**Available Parallel Methods:**
| Method | Description |
|--------|-------------|
| `TraverseParallelAsync<T, U>` (Option) | Map items to Options in parallel, return None if any None |
| `SequenceParallelAsync<T>` (Option) | Await Option tasks in parallel |
| `TraverseParallelAsync<T, U, TErr>` (Result) | Map items to Results in parallel, return first Err |
| `SequenceParallelAsync<T, TErr>` (Result) | Await Result tasks in parallel |
| `ChooseParallelAsync<T, U>` | Map to Options in parallel, collect Some values |
| `PartitionParallelAsync<T, U, TErr>` | Map to Results in parallel, separate Ok/Err |

All parallel methods accept `maxDegreeOfParallelism`:
- `-1` (default): Unlimited parallelism
- `> 0`: Limit concurrent operations to specified number

### Async Streams (IAsyncEnumerable)

Full support for async streams with monad-aware operations:

```csharp
// Filter and unwrap Some values from an async stream
IAsyncEnumerable<Option<User>> userStream = GetUserStreamAsync();
await foreach (var user in userStream.ChooseAsync())
{
    Console.WriteLine(user.Name);  // Only Some values
}

// Safe first element from async stream
var firstUser = await userStream.FirstOrNoneAsync();
firstUser.Match(
    someFunc: u => Console.WriteLine($"Found: {u.Name}"),
    noneFunc: () => Console.WriteLine("No users")
);

// Collect only successful results
IAsyncEnumerable<Result<Order, Error>> orderStream = ProcessOrdersAsync();
await foreach (var order in orderStream.CollectOkAsync())
{
    await SaveAsync(order);
}

// Partition results into successes and failures
var (orders, errors) = await orderStream.PartitionAsync();

// Sequence: Convert stream of Options to Option of list
var allUsers = await userStream.SequenceAsync();  // Option<IReadOnlyList<User>>

// General async stream operations
var result = await dataStream
    .WhereAsync(async x => await IsValidAsync(x))
    .SelectAsync(async x => await TransformAsync(x))
    .TapAsync(async x => await LogAsync(x))
    .ToListAsync();
```

### Deconstruction & Pattern Matching

All monads support C# deconstruction for clean pattern matching:

```csharp
// Option: var (value, isSome) = option
var option = Option<int>.Some(42);
var (value, isSome) = option;
if (isSome)
    Console.WriteLine($"Got: {value}");

// Result: var (value, error, isOk) = result
var result = Result<int, string>.Ok(100);
var (val, err, isOk) = result;
Console.WriteLine(isOk ? $"Success: {val}" : $"Error: {err}");

// Try: var (value, exception, isSuccess) = tryResult
var tryResult = Try<int>.Of(() => int.Parse(input));
var (parsed, ex, success) = tryResult;

// Either: var (left, right, isRight) = either
var either = Either<string, int>.Right(42);
var (l, r, isRight) = either;

// Validation: var (value, errors, isValid) = validation
var validation = ValidateName(name);
var (validValue, errors, isValid) = validation;
if (!isValid)
    foreach (var error in errors)
        Console.WriteLine(error);

// RemoteData: full state deconstruction
var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;
```

### Implicit Operators

Many monads support implicit conversion from values for cleaner code:

```csharp
// Option: value → Some
Option<int> opt = 42;                     // Same as Option<int>.Some(42)
Option<string> none = null!;              // Same as Option<string>.None()

// Result: value → Ok
Result<int, string> result = 42;          // Same as Result<int, string>.Ok(42)

// Either: right value → Right
Either<string, int> either = 42;          // Same as Either<string, int>.Right(42)

// Try: value → Success, Exception → Failure
Try<int> success = 42;                    // Same as Try<int>.Success(42)
Try<int> failure = new Exception("oops"); // Same as Try<int>.Failure(exception)

// Validation: value → Valid
Validation<int, string> valid = 42;       // Same as Validation<int, string>.Valid(42)

// NonEmptyList: single value → single-element list
NonEmptyList<int> list = 42;              // Same as NonEmptyList<int>.Of(42)

// RemoteData: value → Success
RemoteData<int, string> data = 42;        // Same as RemoteData<int, string>.Success(42)
```

This is especially useful in method returns:

```csharp
Result<int, string> ValidatePositive(int value)
{
    if (value <= 0)
        return Result<int, string>.Err("Must be positive");
    return value;  // Implicit conversion to Ok!
}

Try<int> SafeDivide(int a, int b)
{
    if (b == 0)
        return new DivideByZeroException();  // Implicit to Failure
    return a / b;  // Implicit to Success
}
```

---

## Real-World Examples

### API Response Handling

```csharp
public async Task<Result<UserDto, ApiError>> GetUserProfileAsync(int userId)
{
    // Chain multiple API calls with automatic error propagation
    return await _httpClient.GetUserAsync(userId)
        .AndThenAsync(user => _httpClient.GetUserPreferencesAsync(user.Id))
        .MapAsync(prefs => new UserDto(user, prefs))
        .TapAsync(dto => _cache.SetAsync($"user:{userId}", dto))
        .TapErrAsync(err => _logger.LogError("Failed to get user {Id}: {Error}", userId, err));
}

// Usage in controller
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    return await GetUserProfileAsync(id).Match(
        ok: user => Ok(user),
        err: error => error.Code switch
        {
            "NOT_FOUND" => NotFound(),
            "UNAUTHORIZED" => Unauthorized(),
            _ => StatusCode(500, error.Message)
        }
    );
}
```

### Form Validation Pipeline

```csharp
public record CreateUserRequest(string Name, string Email, int Age);

public Validation<User, ValidationError> ValidateCreateUser(CreateUserRequest request)
{
    return ValidateName(request.Name)
        .Apply(ValidateEmail(request.Email), (name, email) => (name, email))
        .Apply(ValidateAge(request.Age), (partial, age) => 
            new User(partial.name, partial.email, age));
}

Validation<string, ValidationError> ValidateName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Validation<string, ValidationError>.Invalid(
            new ValidationError("Name", "Name is required"));
    
    if (name.Length < 2)
        return Validation<string, ValidationError>.Invalid(
            new ValidationError("Name", "Name must be at least 2 characters"));
    
    return Validation<string, ValidationError>.Valid(name);
}

// Returns ALL validation errors at once
var result = ValidateCreateUser(request);
result.Match(
    valid: user => SaveUser(user),
    invalid: errors => BadRequest(new { Errors = errors })
);
```

### Async Configuration with ReaderAsync

```csharp
public record AppConfig(string ConnectionString, string ApiKey, int MaxRetries);

// Build composable async configuration-dependent operations
var getUsers = ReaderAsync<AppConfig, List<User>>.From(async config =>
{
    using var conn = new SqlConnection(config.ConnectionString);
    return await conn.QueryAsync<User>("SELECT * FROM Users").ToListAsync();
});

var enrichWithApi = ReaderAsync<AppConfig, Func<User, Task<UserWithDetails>>>.From(config =>
    async user =>
    {
        var client = new ApiClient(config.ApiKey);
        var details = await client.GetDetailsAsync(user.Id);
        return new UserWithDetails(user, details);
    });

// Compose and run with parallel execution
var workflow = 
    from users in getUsers
    from enricher in enrichWithApi
    from enrichedUsers in ReaderAsync<AppConfig, List<UserWithDetails>>.From(async config =>
        await Task.WhenAll(users.Select(enricher)))
    select enrichedUsers.ToList();

var config = new AppConfig("Server=...", "api-key-123", 3);
var enrichedUsers = await workflow.RunAsync(config);
```

### Blazor Component with RemoteData

```csharp
@page "/users/{Id:int}"

<div class="user-profile">
    @_userData.Match(
        notAsked: () => @<button @onclick="LoadUser">Load Profile</button>,
        loading: () => @<div class="skeleton-loader">
            <div class="skeleton-avatar"></div>
            <div class="skeleton-text"></div>
        </div>,
        success: user => @<article class="profile-card">
            <img src="@user.AvatarUrl" alt="@user.Name" />
            <h2>@user.Name</h2>
            <p>@user.Email</p>
            <span class="badge">@user.Role</span>
        </article>,
        failure: error => @<div class="error-state">
            <p>@error.Message</p>
            <button @onclick="LoadUser">Retry</button>
        </div>
    )
</div>

@code {
    [Parameter] public int Id { get; set; }
    
    private RemoteData<User, ApiError> _userData = RemoteData<User, ApiError>.NotAsked();
    
    private async Task LoadUser()
    {
        _userData = RemoteData<User, ApiError>.Loading();
        StateHasChanged();
        
        try
        {
            var user = await _userService.GetUserAsync(Id);
            _userData = RemoteData<User, ApiError>.Success(user);
        }
        catch (ApiException ex)
        {
            _userData = RemoteData<User, ApiError>.Failure(ex.Error);
        }
        
        StateHasChanged();
    }
}
```

### Data Pipeline with Try

```csharp
public Try<ProcessedData> ProcessDataPipeline(string rawInput)
{
    return Try<string>.Of(() => ValidateInput(rawInput))
        .FlatMap(input => Try<ParsedData>.Of(() => JsonSerializer.Deserialize<ParsedData>(input)!))
        .FlatMap(parsed => Try<EnrichedData>.Of(() => EnrichWithExternalData(parsed)))
        .FlatMap(enriched => Try<ProcessedData>.Of(() => ApplyBusinessRules(enriched)))
        .Recover(ex => ex switch
        {
            JsonException => new ProcessedData { Error = "Invalid JSON format" },
            ValidationException ve => new ProcessedData { Error = ve.Message },
            _ => throw ex  // Re-throw unexpected exceptions
        });
}

// Usage
var result = ProcessDataPipeline(userInput);
result.Match(
    success: data => Console.WriteLine($"Processed: {data}"),
    failure: ex => Console.WriteLine($"Pipeline failed: {ex.Message}")
);
```

### NonEmptyList for Business Rules

```csharp
// Ensure at least one admin exists
public Result<NonEmptyList<User>, BusinessError> GetSystemAdmins()
{
    var admins = _userRepository.GetAll()
        .Where(u => u.Role == Role.Admin)
        .ToList();
    
    return NonEmptyList<User>.FromEnumerable(admins)
        .OkOr(BusinessError.NoAdminsConfigured);
}

// Safe aggregation without null checks
public decimal CalculateAverageOrderValue(NonEmptyList<Order> orders)
{
    // Reduce is always safe — list is guaranteed non-empty
    var total = orders.Reduce((acc, order) => 
        new Order { Total = acc.Total + order.Total }).Total;
    
    return total / orders.Count;
}
```

### Parallel Batch Processing

```csharp
// Process orders in parallel with controlled concurrency
public async Task<(List<Order> Successes, List<OrderError> Failures)> ProcessOrders(
    IEnumerable<OrderRequest> requests)
{
    var (successes, failures) = await requests.PartitionParallelAsync(
        async request => await ProcessOrderAsync(request),
        maxDegreeOfParallelism: 8
    );
    
    return (successes.ToList(), failures.ToList());
}

// Fetch all users in parallel, fail fast if any not found
public async Task<Option<IReadOnlyList<User>>> GetAllUsers(IEnumerable<int> userIds)
{
    return await userIds.TraverseParallelAsync(
        id => FindUserAsync(id),
        maxDegreeOfParallelism: 4
    );
}
```

---

## Source Generators

**Why this exists:** Even [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14) doesn't include native discriminated unions—a feature available in F#, Rust, Swift, Kotlin, and TypeScript. The [proposal (csharplang #8928)](https://github.com/dotnet/csharplang/issues/8928) is under discussion but has no confirmed release date. `Monad.NET.SourceGenerators` fills the gap today with zero runtime overhead.

```bash
dotnet add package Monad.NET.SourceGenerators
```

### Creating Discriminated Unions

Mark your abstract record or class with `[Union]` and the generator creates exhaustive pattern matching automatically:

```csharp
using Monad.NET;

[Union]
public abstract partial record Shape
{
    public partial record Circle(double Radius) : Shape;
    public partial record Rectangle(double Width, double Height) : Shape;
    public partial record Triangle(double Base, double Height) : Shape;
}
```

### Generated Methods

The generator creates comprehensive utility methods for your union types:

```csharp
Shape shape = new Shape.Circle(5.0);

// Match with return value - exhaustive pattern matching
var area = shape.Match(
    circle: c => Math.PI * c.Radius * c.Radius,
    rectangle: r => r.Width * r.Height,
    triangle: t => 0.5 * t.Base * t.Height
);

// Match with side effects
shape.Match(
    circle: c => Console.WriteLine($"Circle: r={c.Radius}"),
    rectangle: r => Console.WriteLine($"Rectangle: {r.Width}x{r.Height}"),
    triangle: t => Console.WriteLine($"Triangle: b={t.Base}, h={t.Height}")
);

// Is{Case} properties - type checking
if (shape.IsCircle)
    Console.WriteLine("It's a circle!");

// As{Case}() methods - safe casting (returns Option<T>)
var circleArea = shape.AsCircle()
    .Map(c => Math.PI * c.Radius * c.Radius)
    .UnwrapOr(0);

// Map - transform cases
var doubled = shape.Map(
    circle: c => new Shape.Circle(c.Radius * 2),
    rectangle: r => new Shape.Rectangle(r.Width * 2, r.Height * 2),
    triangle: t => new Shape.Triangle(t.Base * 2, t.Height * 2)
);

// Tap - side effects (null handlers are skipped)
shape.Tap(circle: c => Console.WriteLine($"Logging circle: {c.Radius}"));

// Factory methods - cleaner construction
var circle = Shape.NewCircle(5.0);
var rect = Shape.NewRectangle(4.0, 5.0);
```

### Attribute Options

```csharp
// Customize generated code
[Union(
    GenerateFactoryMethods = true,      // Generate New{Case}() methods (default: true)
    GenerateAsOptionMethods = true      // Generate As{Case}() methods (default: true, requires Monad.NET)
)]
public abstract partial record MyUnion { ... }
```

### Real-World Examples

**Domain Events:**

```csharp
[Union]
public abstract partial record DomainEvent
{
    public partial record UserRegistered(Guid UserId, string Email) : DomainEvent;
    public partial record OrderPlaced(Guid OrderId, decimal Total) : DomainEvent;
    public partial record PaymentReceived(Guid PaymentId, decimal Amount) : DomainEvent;
}

// Exhaustive handling - compiler ensures all cases are covered
void HandleEvent(DomainEvent evt) => evt.Match(
    userRegistered: e => SendWelcomeEmail(e.Email),
    orderPlaced: e => NotifyWarehouse(e.OrderId),
    paymentReceived: e => UpdateLedger(e.PaymentId, e.Amount)
);
```

**Expression Trees:**

```csharp
[Union]
public abstract partial record Expr
{
    public partial record Literal(int Value) : Expr;
    public partial record Add(Expr Left, Expr Right) : Expr;
    public partial record Multiply(Expr Left, Expr Right) : Expr;
}

int Evaluate(Expr expr) => expr.Match(
    literal: l => l.Value,
    add: a => Evaluate(a.Left) + Evaluate(a.Right),
    multiply: m => Evaluate(m.Left) * Evaluate(m.Right)
);

// (2 + 3) * 4 = 20
var expr = new Expr.Multiply(
    new Expr.Add(new Expr.Literal(2), new Expr.Literal(3)),
    new Expr.Literal(4)
);
var result = Evaluate(expr); // 20
```

**HTTP Responses:**

```csharp
[Union]
public abstract partial record ApiResponse<T>
{
    public partial record Success(T Data) : ApiResponse<T>;
    public partial record NotFound(string Message) : ApiResponse<T>;
    public partial record ValidationError(IReadOnlyList<string> Errors) : ApiResponse<T>;
    public partial record ServerError(Exception Ex) : ApiResponse<T>;
}

IActionResult ToActionResult<T>(ApiResponse<T> response) => response.Match(
    success: s => new OkObjectResult(s.Data),
    notFound: n => new NotFoundObjectResult(n.Message),
    validationError: v => new BadRequestObjectResult(v.Errors),
    serverError: e => new ObjectResult(e.Ex.Message) { StatusCode = 500 }
);
```

## Samples

- `examples/Monad.NET.Samples` — minimal console samples demonstrating Option, Result, Validation, Writer, RemoteData, and IO.

### Requirements

- Types must be `abstract` and `partial`
- Nested types must inherit from the parent type
- Works with both `record` and `class` types

---

## ASP.NET Core Integration

The `Monad.NET.AspNetCore` package provides seamless integration with ASP.NET Core:

```bash
dotnet add package Monad.NET.AspNetCore
```

### IActionResult Extensions

Convert monad types directly to HTTP responses:

```csharp
using Monad.NET;
using Monad.NET.AspNetCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // Option → 200 OK or 404 Not Found
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        return _userService.FindUser(id)
            .ToActionResult("User not found");
    }

    // Result → 200 OK or error status code
    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        return _userService.CreateUser(request)
            .ToCreatedResult($"/api/users/{request.Id}");
    }

    // Validation → 422 with RFC 7807 ValidationProblemDetails
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UpdateUserRequest request)
    {
        return ValidateRequest(request)
            .ToValidationProblemResult();
    }

    // Async support
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(int id)
    {
        return await _userService.GetProfileAsync(id)
            .ToActionResultAsync();
    }
}
```

### Exception Handling Middleware

Catch unhandled exceptions and return consistent Result-style responses:

```csharp
var app = builder.Build();

app.UseResultExceptionHandler(options =>
{
    options.IncludeExceptionDetails = app.Environment.IsDevelopment();
});

app.MapControllers();
```

### Available Extensions

| Monad | Method | Success | Failure |
|-------|--------|---------|---------|
| `Option<T>` | `ToActionResult()` | 200 OK | 404 Not Found |
| `Result<T,E>` | `ToActionResult()` | 200 OK | Custom status code |
| `Result<T,E>` | `ToCreatedResult(location)` | 201 Created | Custom status code |
| `Result<T,E>` | `ToNoContentResult()` | 204 No Content | Custom status code |
| `Validation<T,E>` | `ToValidationProblemResult()` | 200 OK | 422 with ValidationProblemDetails |
| `Either<L,R>` | `ToActionResult()` | 200 OK (Right) | Custom status code (Left) |
| `Try<T>` | `ToActionResult()` | 200 OK | 500 Internal Server Error |

All extensions have async variants (`ToActionResultAsync`).

---

## Entity Framework Core Integration

The `Monad.NET.EntityFrameworkCore` package provides seamless integration with EF Core:

```bash
dotnet add package Monad.NET.EntityFrameworkCore
```

### Value Converters

Use `Option<T>` as entity properties with automatic conversion to nullable database columns:

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Option<string> Email { get; set; }    // Stored as nullable varchar
    public Option<int> Age { get; set; }         // Stored as nullable int
}

// In DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.Property(e => e.Email)
            .HasConversion(new OptionValueConverter<string>());

        entity.Property(e => e.Age)
            .HasConversion(new OptionStructValueConverter<int>());
    });
}
```

### Query Extensions

Safely query data with Option-returning methods:

```csharp
// Returns Option<User> instead of throwing or returning null
var user = await context.Users.FirstOrNoneAsync(u => u.Name == "John");

user.Match(
    some: u => Console.WriteLine($"Found: {u.Name}"),
    none: () => Console.WriteLine("User not found")
);

// Other query extensions
await context.Users.SingleOrNoneAsync(u => u.Id == id);
await context.Users.ElementAtOrNoneAsync(0);
await context.Users.LastOrNoneAsync(u => u.IsActive);
```

### Available Extensions

| Method | Description |
|--------|-------------|
| `FirstOrNone()` | First element or None |
| `FirstOrNoneAsync()` | Async variant |
| `SingleOrNone()` | Single element or None (throws if multiple) |
| `SingleOrNoneAsync()` | Async variant |
| `ElementAtOrNone(index)` | Element at index or None |
| `ElementAtOrNoneAsync(index)` | Async variant |
| `LastOrNone()` | Last element or None |
| `LastOrNoneAsync()` | Async variant |

---

## Performance

Monad.NET is designed for correctness and safety first, but performance is still a priority:

| Aspect | Details |
|--------|---------|
| **Struct-based** | `Option<T>`, `Result<T,E>`, `Try<T>`, etc. are `readonly struct` — no heap allocations |
| **No boxing** | Generic implementations avoid boxing value types |
| **Lazy evaluation** | `UnwrapOrElse`, `OrElse` use `Func<>` for deferred computation |
| **Zero allocations** | Most operations on value types are allocation-free |
| **Aggressive inlining** | Hot paths use `[MethodImpl(AggressiveInlining)]` |
| **ConfigureAwait(false)** | All async methods use `ConfigureAwait(false)` |

### When to Consider Alternatives

- **Hot paths with millions of iterations** — The abstraction has minimal overhead, but raw `if` statements may be faster in extreme cases
- **Interop with existing code** — If your codebase heavily uses exceptions, gradual adoption is recommended

### Benchmarks

For typical use cases, the overhead is negligible (nanoseconds). The safety guarantees and code clarity typically outweigh any micro-optimization concerns.

---

## Resources

Want to dive deeper into functional programming and these patterns? Here are some excellent resources:

### Books

| Book | Author | Why Read It |
|------|--------|-------------|
| [Functional Programming in C#](https://www.manning.com/books/functional-programming-in-c-sharp-second-edition) | Enrico Buonanno | The definitive guide to FP in C#. Covers Option, Either, validation, and more. |
| [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) | Scott Wlaschin | Uses F# but concepts translate directly. Excellent on making illegal states unrepresentable. |
| [Programming Rust](https://www.oreilly.com/library/view/programming-rust-2nd/9781492052586/) | Blandy, Orendorff, Tindall | Rust's `Option` and `Result` are nearly identical to Monad.NET's versions. |

### Online Resources

| Resource | Description |
|----------|-------------|
| [F# for Fun and Profit](https://fsharpforfunandprofit.com/) | Scott Wlaschin's legendary site. Start with [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/). |
| [Rust Error Handling](https://doc.rust-lang.org/book/ch09-00-error-handling.html) | Official Rust book chapter on `Option` and `Result`. |
| [Haskell Maybe/Either](https://wiki.haskell.org/Handling_errors_in_Haskell) | Haskell wiki on error handling patterns. |
| [Parse, Don't Validate](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/) | Alexis King's influential post on type-driven design. |

### Videos & Talks

| Talk | Speaker | Topics |
|------|---------|--------|
| [Functional Design Patterns](https://www.youtube.com/watch?v=srQt1NAHYC0) | Scott Wlaschin | Monads, Railway Oriented Programming, composition |
| [Domain Modeling Made Functional](https://www.youtube.com/watch?v=2JB1_e5wZmU) | Scott Wlaschin | Making illegal states unrepresentable |
| [The Power of Composition](https://www.youtube.com/watch?v=vDe-4o8Uwl8) | Scott Wlaschin | Why small, composable functions matter |

### Related C# Libraries

| Library | Description |
|---------|-------------|
| [language-ext](https://github.com/louthy/language-ext) | Comprehensive FP library for C#. More extensive than Monad.NET but steeper learning curve. |
| [OneOf](https://github.com/mcintyre321/OneOf) | Focused on discriminated unions. Lighter weight. |
| [FluentResults](https://github.com/altmann/FluentResults) | Result pattern with fluent API. Good for simple use cases. |
| [ErrorOr](https://github.com/amantinband/error-or) | Discriminated union for errors. Popular in Clean Architecture circles. |

### Key Concepts to Understand

1. **Railway Oriented Programming** — Treat errors as alternate tracks, not exceptions
2. **Making Illegal States Unrepresentable** — Use types to prevent bugs at compile time
3. **Parse, Don't Validate** — Push validation to the boundaries, work with valid types internally
4. **Composition over Inheritance** — Small, focused types that combine well

---

## FAQ

### Can I use Monad.NET with Entity Framework?

Yes! Use `Option<T>` for optional relationships and `Result<T, E>` for operations that might fail:

```csharp
public async Task<Result<User, DbError>> GetUserAsync(int id)
{
    try
    {
        var user = await _context.Users.FindAsync(id);
        return user is not null
            ? Result<User, DbError>.Ok(user)
            : Result<User, DbError>.Err(DbError.NotFound);
    }
    catch (Exception ex)
    {
        return Result<User, DbError>.Err(DbError.ConnectionFailed(ex.Message));
    }
}
```

### Can I use Monad.NET with ASP.NET Core?

Absolutely. It works well with minimal APIs and controllers:

```csharp
app.MapGet("/users/{id}", async (int id, UserService service) =>
{
    return await service.GetUserAsync(id)
        .Match(
            ok: user => Results.Ok(user),
            err: error => error switch
            {
                DbError.NotFound => Results.NotFound(),
                _ => Results.Problem(error.Message)
            }
        );
});
```

### How do I convert between monad types?

Each type provides conversion methods:

```csharp
// Option → Result
Option<int>.Some(42).OkOr("No value");  // Ok(42)
Option<int>.None().OkOr("No value");    // Err("No value")

// Result → Option
Result<int, string>.Ok(42).Ok();        // Some(42)
Result<int, string>.Err("oops").Ok();   // None

// Try → Result
Try<int>.Of(() => int.Parse("42")).ToResult(ex => ex.Message);

// Validation → Result
validation.ToResult();  // Errors become IReadOnlyList<E>

// Reader → ReaderAsync
reader.ToAsync();  // Converts sync Reader to async
```

### Is Monad.NET thread-safe?

Yes. All types are immutable `readonly struct` with no shared mutable state.

### What's the difference between `Result` and `Either`?

- **`Result<T, E>`** — Semantically means success or failure. Right-biased (operations work on `Ok`).
- **`Either<L, R>`** — General "one of two types" with no success/failure implication. Can work on either side.

Use `Result` for error handling. Use `Either` when both sides are valid outcomes (e.g., `Either<CachedValue, FreshValue>`).

### What's the difference between `Result` and `Validation`?

- **`Result`** — Short-circuits on first error (like `&&`)
- **`Validation`** — Accumulates ALL errors (for showing multiple validation messages)

```csharp
// Result: stops at first error
var result = ValidateName(name)
    .AndThen(_ => ValidateEmail(email))   // Won't run if name fails
    .AndThen(_ => ValidateAge(age));

// Validation: collects all errors
var validation = ValidateName(name)
    .Apply(ValidateEmail(email), (n, e) => (n, e))
    .Apply(ValidateAge(age), (partial, a) => new User(partial.n, partial.e, a));
// Shows: "Name required", "Invalid email", "Age must be positive"
```

### What's the difference between `Reader` and `ReaderAsync`?

- **`Reader<R, A>`** — Synchronous dependency injection. Use when computations don't need async.
- **`ReaderAsync<R, A>`** — Asynchronous dependency injection. Use when computations involve I/O, database access, HTTP calls, etc.

```csharp
// Sync: for pure computations
var syncReader = Reader<Config, int>.Asks(c => c.MaxRetries);
var value = syncReader.Run(config);

// Async: for I/O operations
var asyncReader = ReaderAsync<Config, User>.From(async c => 
    await database.FindUserAsync(c.DefaultUserId));
var user = await asyncReader.RunAsync(config);

// Convert sync to async
var asyncFromSync = syncReader.ToAsync();
```

---

## API Reference

Full API documentation is available in [docs/API.md](docs/API.md).

## Guides

- [Pitfalls & Gotchas](docs/Guides/Pitfalls.md)
- [Logging Guidance](docs/Guides/Logging.md)

## Compatibility

See the [Compatibility Matrix](docs/Compatibility.md) for supported target frameworks (currently .NET 6–10).

---

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

**Development requirements:**
- .NET 8.0 SDK or later
- Your preferred IDE (Visual Studio, Rider, VS Code)

```bash
git clone https://github.com/behrangmohseni/Monad.NET.git
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

[Documentation](https://www.nuget.org/packages/Monad.NET/) · [NuGet](https://www.nuget.org/packages/Monad.NET/) · [Issues](https://github.com/behrangmohseni/Monad.NET/issues)
