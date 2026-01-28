# Core Types

This document provides detailed documentation for all monad types in Monad.NET.

## Table of Contents

- [Option\<T\>](#optiont)
- [Result\<T, E\>](#resultt-e)
- [Validation\<T, E\>](#validationt-e)
- [Try\<T\>](#tryt)
- [RemoteData\<T, E\>](#remotedatat-e)
- [NonEmptyList\<T\>](#nonemptylistt)
- [Writer\<W, T\>](#writerw-t)
- [Reader\<R, A\>](#readerr-a)
- [ReaderAsync\<R, A\>](#readerasyncr-a)
- [State\<S, A\>](#states-a)
- [IO\<T\>](#iot)

---

## Option\<T\>

Represents a value that may or may not exist. Use instead of `null`.

**Inspired by:** F# `Option<'T>`, Rust `Option<T>`, Haskell `Maybe a`

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
var chained = some.Bind(x => LookupValue(x));       // Chains Option-returning functions

// Extraction
var value = some.GetValueOr(0);                          // 42
var computed = none.GetValueOrElse(() => ComputeDefault()); // Lazy evaluation

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

## Result\<T, E\>

Represents either success (`Ok`) or failure (`Err`) with a typed error.

**Inspired by:** Rust `Result<T, E>`

```csharp
// Creation
var ok = Result<int, string>.Ok(42);
var err = Result<int, string>.Err("Something went wrong");

// Safe exception handling
var parsed = ResultExtensions.Try(() => int.Parse(input));
var fetched = await ResultExtensions.TryAsync(() => httpClient.GetAsync(url));

// Railway-oriented programming
var pipeline = ParseInput(raw)
    .Bind(Validate)
    .Bind(Transform)
    .Bind(Save)
    .Tap(result => _logger.LogInformation("Saved: {Id}", result.Id))
    .TapErr(error => _logger.LogError("Failed: {Error}", error));

// Recovery strategies
var recovered = err.OrElse(e => FallbackStrategy(e));
var withDefault = err.GetValueOr(defaultValue);

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

## Validation\<T, E\>

Unlike `Result`, validation **accumulates all errors** instead of short-circuiting.

**Inspired by:** Haskell `Validation` (from `Data.Validation`), Scala Cats `Validated`

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

## Try\<T\>

Wraps computations that might throw, converting exceptions to values.

**Inspired by:** Scala `Try[T]`, Vavr (Java) `Try<T>`

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

## RemoteData\<T, E\>

Models the four states of asynchronous data: **NotAsked**, **Loading**, **Success**, **Failure**.

**Inspired by:** Elm `RemoteData`, Haskell `remotedata` package

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

## NonEmptyList\<T\>

A list guaranteed to have at least one element. `Head` and `Reduce` are always safe.

**Inspired by:** Haskell `NonEmpty`, Scala `NonEmptyList`

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
var expanded = list.Bind(x => NonEmptyList<int>.Of(x, x * 10));

// Side effects with Tap
list.Tap(x => Console.WriteLine(x))              // Logs each element
    .TapIndexed((x, i) => Console.WriteLine($"{i}: {x}"));

// Filter returns Option (result might be empty)
var filtered = list.Filter(x => x > 10);         // None
```

**Methods:** `Map`, `MapIndexed`, `Bind`, `Filter`, `Reduce`, `Fold`, `Tap`, `TapIndexed`

**When to use:** When empty collections are invalid states (config items, selected options, etc.).

---

## Writer\<W, T\>

Computations that produce a value alongside accumulated output (logs, traces, metrics).

**Inspired by:** Haskell `Writer w a`

```csharp
// Computation with logging
var computation = Writer<List<string>, int>.Tell(1, new List<string> { "Started with 1" })
    .Bind(
        x => Writer<List<string>, int>.Tell(x * 2, new List<string> { $"Doubled to {x * 2}" }),
        (log1, log2) => log1.Concat(log2).ToList()
    )
    .Bind(
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

**Methods:** `Map`, `Bind`, `BiMap`, `Match`, `Tap`, `TapLog`

**When to use:** Audit trails, computation tracing, accumulating metadata.

---

## Reader\<R, A\>

Computations that depend on a shared environment. Functional dependency injection.

**Inspired by:** Haskell `Reader r a`

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
    .Bind(repo => Reader<AppServices, string>.From(s =>
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

**Methods:** `Map`, `Bind`, `Tap`, `TapEnv`, `WithEnvironment`, `ToAsync`

**When to use:** Passing configuration/services through call chains without parameter drilling.

---

## ReaderAsync\<R, A\>

Asynchronous computations that depend on a shared environment. The async variant of `Reader<R, A>`.

**Inspired by:** Haskell `ReaderT r IO a`, Scala Cats Effect `Kleisli[IO, R, A]`

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
- `Bind(f)` / `BindAsync(f)` — Chain computations
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

## State\<S, A\>

Computations that thread state through a sequence of operations without mutable variables.

**Inspired by:** Haskell `State s a`

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
- `State<S, A>.Return(value)` — Return value without modifying state
- `State<S, S>.Get()` — Get current state as the value
- `State<S, Unit>.Put(newState)` — Replace state
- `State<S, Unit>.Modify(f)` — Transform state using a function
- `State<S, A>.Gets(selector)` — Extract a value from the state

**Composition:**
- `Map(f)` — Transform the value
- `Bind(f)` — Chain computations
- `Zip(other)` / `ZipWith(other, f)` — Combine two state computations
- `Tap(action)` — Execute side effect with value (logging/debugging)
- `TapState(action)` — Execute side effect with state

**Execution:**
- `Run(initialState)` — Get both value and final state
- `Eval(initialState)` — Get only the value (discard state)
- `Exec(initialState)` — Get only the final state (discard value)

**When to use:** Simulators, interpreters, random number generators, stack-based computations, game state, any computation that needs to pass state through without mutable variables.

---

## IO\<T\>

Defers side effects, making code purely functional until explicitly executed.

**Inspired by:** Haskell `IO a`, Scala Cats Effect `IO[A]`

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
- `IO<T>.Return(value)` / `Return(value)` — Create with pure value
- `IO<T>.Delay(effect)` — Alias for `Of`, emphasizes laziness
- `IO.Execute(action)` — Execute action, return Unit

**Composition:**
- `Map(f)` — Transform the result
- `Bind(f)` — Chain IO operations
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

[← Back to README](../README.md) | [Advanced Usage →](AdvancedUsage.md)

