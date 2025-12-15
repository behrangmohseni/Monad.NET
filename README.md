# Monad.NET

[![NuGet](https://img.shields.io/nuget/v/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![Build](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/behrangmohseni/Monad.NET/graph/badge.svg)](https://codecov.io/gh/behrangmohseni/Monad.NET)
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
  - [State\<S, A\>](#states-a)
- [Advanced Usage](#advanced-usage)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Real-World Examples](#real-world-examples)
- [Performance](#performance)
- [FAQ](#faq)
- [API Reference](#api-reference)
- [Contributing](#contributing)
- [License](#license)

---

## Why Monad.NET?

Modern .NET applications demand reliability. Yet we continue to fight the same battles: null reference exceptions, swallowed errors, inconsistent error handling, and code that's difficult to reason about.

**Monad.NET addresses these challenges:**

| Problem             | Traditional Approach       | Monad.NET Solution                     |
|---------------------|----------------------------|----------------------------------------|
| Null references | `if (x is not null)` checks scattered everywhere | `Option<T>` makes absence explicit and composable |
| Error handling | Try-catch blocks, exceptions as control flow | `Result<T, E>` treats errors as data |
| Validation | Return on first error, lose context | `Validation<T, E>` accumulates all errors |
| Async state | Boolean flags (`isLoading`, `hasError`) | `RemoteData<T, E>` models all four states |
| Empty collections | Runtime exceptions on `.First()` | `NonEmptyList<T>` guarantees at least one element |

### Design Principles

1. **Explicit over implicit** — No hidden nulls, no surprise exceptions
2. **Composition over inheritance** — Small, focused types that combine well
3. **Immutability by default** — All types are immutable and thread-safe
4. **Zero dependencies** — Only the .NET runtime, nothing else

### Which Monad Should I Use?

| Scenario | Use This |
|----------|----------|
| A value might be missing | `Option<T>` |
| An operation can fail with an error | `Result<T, E>` |
| Need to show ALL validation errors | `Validation<T, E>` |
| Wrapping code that throws exceptions | `Try<T>` |
| UI state for async data loading | `RemoteData<T, E>` |
| A list must have at least one item | `NonEmptyList<T>` |
| Need to accumulate logs/traces | `Writer<W, T>` |
| Dependency injection without DI container | `Reader<R, A>` |
| Thread state through computations | `State<S, A>` |
| Return one of two different types | `Either<L, R>` |

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

**Execution:**
- `Run(initialState)` — Get both value and final state
- `Eval(initialState)` — Get only the value (discard state)
- `Exec(initialState)` — Get only the final state (discard value)

**When to use:** Simulators, interpreters, random number generators, stack-based computations, game state, any computation that needs to pass state through without mutable variables.

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

### Configuration with Reader Monad

```csharp
public record AppConfig(string ConnectionString, string ApiKey, int MaxRetries);

// Build composable configuration-dependent operations
var getUsers = Reader<AppConfig, Task<List<User>>>.From(async config =>
{
    using var conn = new SqlConnection(config.ConnectionString);
    return await conn.QueryAsync<User>("SELECT * FROM Users").ToListAsync();
});

var enrichWithApi = Reader<AppConfig, Func<User, Task<UserWithDetails>>>.From(config =>
    async user =>
    {
        var client = new ApiClient(config.ApiKey);
        var details = await client.GetDetailsAsync(user.Id);
        return new UserWithDetails(user, details);
    });

// Compose and run
var workflow = getUsers.FlatMap(users =>
    Reader<AppConfig, Task<List<UserWithDetails>>>.From(async config =>
    {
        var enricher = enrichWithApi.Run(config);
        return await Task.WhenAll(users.Select(enricher));
    }));

var config = new AppConfig("Server=...", "api-key-123", 3);
var enrichedUsers = await workflow.Run(config);
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

## Performance

Monad.NET is designed for correctness and safety first, but performance is still a priority:

| Aspect | Details |
|--------|---------|
| **Struct-based** | `Option<T>`, `Result<T,E>`, `Try<T>`, etc. are `readonly struct` — no heap allocations |
| **No boxing** | Generic implementations avoid boxing value types |
| **Lazy evaluation** | `UnwrapOrElse`, `OrElse` use `Func<>` for deferred computation |
| **Zero allocations** | Most operations on value types are allocation-free |

### When to Consider Alternatives

- **Hot paths with millions of iterations** — The abstraction has minimal overhead, but raw `if` statements may be faster in extreme cases
- **Interop with existing code** — If your codebase heavily uses exceptions, gradual adoption is recommended

### Benchmarks

For typical use cases, the overhead is negligible (nanoseconds). The safety guarantees and code clarity typically outweigh any micro-optimization concerns.

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
