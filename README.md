# Monad.NET

[![NuGet](https://img.shields.io/nuget/v/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![Build](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml)
[![CodeQL](https://github.com/behrangmohseni/Monad.NET/actions/workflows/codeql.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/behrangmohseni/Monad.NET/graph/badge.svg)](https://codecov.io/gh/behrangmohseni/Monad.NET)
[![CodeFactor](https://www.codefactor.io/repository/github/behrangmohseni/monad.net/badge)](https://www.codefactor.io/repository/github/behrangmohseni/monad.net)
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
  - [IO\<T\>](#iot)
- [Advanced Usage](#advanced-usage)
- [Source Generators](#source-generators)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Entity Framework Core Integration](#entity-framework-core-integration)
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
| Defer side effects for pure code | `IO<T>` |
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

// Side effects with Tap
var debugReader = Reader<AppServices, string>.Asks(s => s.Users.GetName())
    .Tap(name => Console.WriteLine($"Got user: {name}"))
    .TapEnv(env => Console.WriteLine($"Using logger: {env.Logger}"));
```

**Methods:** `Map`, `FlatMap`, `Tap`, `TapEnv`, `WithEnvironment`

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

## Source Generators

The `Monad.NET.SourceGenerators` package provides compile-time code generation for discriminated unions, reducing boilerplate and ensuring exhaustive pattern matching:

```bash
dotnet add package Monad.NET.SourceGenerators
```

### Creating Discriminated Unions

Mark your abstract record or class with `[Union]` and the generator creates `Match` methods automatically:

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
