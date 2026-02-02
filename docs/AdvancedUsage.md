# Advanced Usage

This document covers advanced patterns and features in Monad.NET.

## Table of Contents

- [LINQ Support](#linq-support)
- [When/Unless Guards](#whenunless-guards)
- [Collection Operations](#collection-operations)
- [Parallel Collection Operations](#parallel-collection-operations)
- [Async Streams (IAsyncEnumerable)](#async-streams-iasyncenumerable)
- [Deconstruction & Pattern Matching](#deconstruction--pattern-matching)
- [Implicit Operators](#implicit-operators)

---

## LINQ Support

All monads support LINQ extension methods (`Select`, `SelectMany`, `Where`) for fluent composition.

### Method Syntax (Recommended)

The method syntax is familiar to most .NET developers and works great with IntelliSense:

```csharp
// Option - chain transformations with Select and SelectMany
var userEmail = FindUser(id)
    .Select(user => user.Email)                    // Map: Option<User> → Option<string>
    .Where(email => email.Contains("@"))           // Filter: keep only valid emails
    .SelectMany(email => ValidateEmail(email));    // Bind: chain Option-returning functions

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

### Query Syntax

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

// ⚠️ Validation with LINQ - WARNING: Does NOT accumulate errors!
// Use Apply() instead for error accumulation:
var user = ValidateName(input.Name)
    .Apply(ValidateEmail(input.Email), (name, email) => new User(name, email));
```

### Available LINQ Methods

| Monad | Select | SelectMany | Where |
|-------|--------|------------|-------|
| `Option<T>` | Map value | Chain Options | Filter by predicate |
| `Result<T,E>` | Map Ok value | Chain Results | With error value/factory |
| `Try<T>` | Map success | Chain Trys | Filter with predicate |
| `Validation<T,E>` | Map valid | ⚠️ Short-circuits (use `Apply`) | — |
| `RemoteData<T,E>` | Map success | Chain RemoteData | — |
| `Writer<W,T>` | Map value | Chain with log combine | — |
| `State<S,A>` | Map value | Chain State | — |
| `IO<T>` | Map value | Chain IO | — |

---

## When/Unless Guards

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

---

## Collection Operations

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

## Parallel Collection Operations

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
| `TraverseParallelAsync<T, U, TError>` (Result) | Map items to Results in parallel, return first Err |
| `SequenceParallelAsync<T, TError>` (Result) | Await Result tasks in parallel |
| `ChooseParallelAsync<T, U>` | Map to Options in parallel, collect Some values |
| `PartitionParallelAsync<T, U, TError>` | Map to Results in parallel, separate Ok/Err |

All parallel methods accept `maxDegreeOfParallelism`:
- `-1` (default): Unlimited parallelism
- `> 0`: Limit concurrent operations to specified number

---

## Async Streams (IAsyncEnumerable)

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

---

## Deconstruction & Pattern Matching

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

// Validation: var (value, errors, isValid) = validation
var validation = ValidateName(name);
var (validValue, errors, isValid) = validation;
if (!isValid)
    foreach (var error in errors)
        Console.WriteLine(error);

// RemoteData: full state deconstruction
var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;
```

---

## Implicit Operators

Many monads support implicit conversion from values for cleaner code:

```csharp
// Option: value → Some
Option<int> opt = 42;                     // Same as Option<int>.Some(42)
Option<string> none = null!;              // Same as Option<string>.None()

// Result: value → Ok
Result<int, string> result = 42;          // Same as Result<int, string>.Ok(42)

// Try: value → Success, Exception → Failure
Try<int> success = 42;                    // Same as Try<int>.Ok(42)
Try<int> failure = new Exception("oops"); // Same as Try<int>.Error(exception)

// Validation: value → Valid
Validation<int, string> valid = 42;       // Same as Validation<int, string>.Ok(42)

// NonEmptyList: single value → single-element list
NonEmptyList<int> list = 42;              // Same as NonEmptyList<int>.Of(42)

// RemoteData: value → Success
RemoteData<int, string> data = 42;        // Same as RemoteData<int, string>.Ok(42)
```

This is especially useful in method returns:

```csharp
Result<int, string> ValidatePositive(int value)
{
    if (value <= 0)
        return Result<int, string>.Error("Must be positive");
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

[← Core Types](CoreTypes.md) | [Examples →](Examples.md) | [Back to README](../README.md)

