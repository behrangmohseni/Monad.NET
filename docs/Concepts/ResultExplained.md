# Result Explained

> **For C# developers:** This guide explains `Result<T, E>` - the type for operations that can succeed or fail with meaningful error information.

## Beyond Option: When You Need to Know Why

`Option<T>` tells you something is missing, but not *why*. Sometimes that's not enough:

```csharp
Option<User> user = FindUser(email);
// If None... why? 
// - Email not found?
// - Database connection failed?
// - Email format invalid?
```

`Result<T, E>` carries error information:

```csharp
Result<User, UserError> user = FindUser(email);
// If Error... we know exactly why
```

---

## What is Result?

`Result<T, E>` represents an operation that either:
- **Succeeds** with a value of type `T`
- **Fails** with an error of type `E`

```csharp
Result<User, string> success = Result<User, string>.Ok(new User("Alice"));
Result<User, string> failure = Result<User, string>.Error("User not found");
```

### Two States

| State | Meaning | Created With |
|-------|---------|--------------|
| **Ok** | Operation succeeded | `Result<T, E>.Ok(value)` |
| **Error** | Operation failed | `Result<T, E>.Error(error)` |

---

## Result vs Exceptions

### Exceptions (Hidden Failures)

```csharp
public User GetUser(int id)
{
    // What can go wrong? Who knows!
    var user = _db.Find(id);
    if (user == null)
        throw new UserNotFoundException(id);
    return user;
}

// Caller has no idea this can fail
var user = GetUser(42);  // 💥 Might explode
```

### Result (Explicit Failures)

```csharp
public Result<User, UserError> GetUser(int id)
{
    // Error possibility is in the signature
    var user = _db.Find(id);
    if (user == null)
        return Result<User, UserError>.Error(new UserError.NotFound(id));
    return Result<User, UserError>.Ok(user);
}

// Caller knows to handle both cases
var result = GetUser(42);  // Must be handled
```

---

## Basic Operations

### Creating Results

```csharp
// Success
var ok = Result<int, string>.Ok(42);

// Failure
var err = Result<int, string>.Error("Something went wrong");

// From try/catch (wrapping exceptions)
Result<int, Exception> parsed = Try<int>.Run(() => int.Parse(input)).ToResult();
```

### Checking State

```csharp
Result<int, string> result = DoSomething();

if (result.IsOk)
{
    int value = result.GetValue();
    Console.WriteLine($"Success: {value}");
}
else
{
    string error = result.GetError();
    Console.WriteLine($"Failed: {error}");
}

// Or use TryGet pattern
if (result.TryGetValue(out var value))
{
    Console.WriteLine($"Success: {value}");
}
```

### Extracting Values

```csharp
Result<int, string> result = Result<int, string>.Ok(42);

// Safe extraction with fallback
int value = result.GetValueOr(0);  // 42, or 0 if Error

// Lazy fallback
int lazy = result.GetValueOrElse(() => ComputeExpensiveDefault());

// Unsafe (throws if Error)
int risky = result.GetValue();  // Throws InvalidOperationException if Error
```

---

## Transforming Results

### Map: Transform Success Values

`Map` transforms the **success value only**:

```csharp
Result<int, string> number = Result<int, string>.Ok(21);

Result<int, string> doubled = number.Map(n => n * 2);  
// Ok(42)

Result<int, string> error = Result<int, string>.Error("oops");
Result<int, string> stillError = error.Map(n => n * 2);  
// Error("oops") - function not called
```

### MapError: Transform Error Values

`MapError` transforms the **error value only**:

```csharp
Result<int, string> error = Result<int, string>.Error("db timeout");

Result<int, ApiError> apiError = error.MapError(e => new ApiError(500, e));
// Error(ApiError { Code: 500, Message: "db timeout" })
```

Useful for converting between error types at layer boundaries.

### Bind: Chain Fallible Operations

`Bind` chains operations where **each step can fail**:

```csharp
Result<string, Error> ValidateEmail(string email) { ... }
Result<User, Error> CreateUser(string email) { ... }
Result<string, Error> SendWelcomeEmail(User user) { ... }

// Pipeline: any step can fail
Result<string, Error> result = ValidateEmail(input)
    .Bind(email => CreateUser(email))
    .Bind(user => SendWelcomeEmail(user));
```

If any step returns `Error`, subsequent steps are skipped.

---

## Pattern Matching

### Match: Handle Both Outcomes

```csharp
Result<User, string> result = GetUser(42);

// With return value
string message = result.Match(
    okFunc: user => $"Welcome, {user.Name}!",
    errFunc: error => $"Error: {error}"
);

// Side effects only
result.Match(
    okAction: user => _logger.Info($"Found user {user.Id}"),
    errAction: error => _logger.Error($"Failed: {error}")
);
```

### Switch on State

```csharp
var output = result switch
{
    { IsOk: true } r => $"Success: {r.GetValue()}",
    { IsError: true } r => $"Failed: {r.GetError()}",
    _ => "Unknown state"
};
```

---

## Error Types

### String Errors (Simple)

Good for quick prototyping:

```csharp
Result<User, string> GetUser(int id)
{
    if (id <= 0)
        return Result<User, string>.Error("Invalid ID");
    // ...
}
```

### Typed Errors (Recommended)

Better for production code:

```csharp
// Define error types
public abstract record UserError
{
    public record NotFound(int Id) : UserError;
    public record InvalidEmail(string Email) : UserError;
    public record AlreadyExists(string Email) : UserError;
    public record DatabaseError(Exception Inner) : UserError;
}

// Use them
Result<User, UserError> GetUser(int id)
{
    var user = _db.Find(id);
    return user != null
        ? Result<User, UserError>.Ok(user)
        : Result<User, UserError>.Error(new UserError.NotFound(id));
}

// Pattern match on error types
result.Match(
    okFunc: user => HandleSuccess(user),
    errFunc: error => error switch
    {
        UserError.NotFound e => NotFound($"User {e.Id} not found"),
        UserError.InvalidEmail e => BadRequest($"Invalid email: {e.Email}"),
        UserError.AlreadyExists e => Conflict($"Email taken: {e.Email}"),
        UserError.DatabaseError e => InternalError("Database error"),
        _ => InternalError("Unknown error")
    }
);
```

---

## Practical Examples

### Validation Pipeline

```csharp
public Result<ValidatedOrder, OrderError> ValidateOrder(OrderRequest request)
{
    return ValidateCustomerId(request.CustomerId)
        .Bind(_ => ValidateProductId(request.ProductId))
        .Bind(_ => ValidateQuantity(request.Quantity))
        .Map(_ => new ValidatedOrder(request));
}

private Result<int, OrderError> ValidateCustomerId(int id) =>
    id > 0
        ? Result<int, OrderError>.Ok(id)
        : Result<int, OrderError>.Error(new OrderError.InvalidCustomer("ID must be positive"));

private Result<string, OrderError> ValidateProductId(string id) =>
    !string.IsNullOrEmpty(id)
        ? Result<string, OrderError>.Ok(id)
        : Result<string, OrderError>.Error(new OrderError.InvalidProduct("Product ID required"));

private Result<int, OrderError> ValidateQuantity(int qty) =>
    qty > 0 && qty <= 100
        ? Result<int, OrderError>.Ok(qty)
        : Result<int, OrderError>.Error(new OrderError.InvalidQuantity($"Quantity must be 1-100, got {qty}"));
```

### Error Recovery

```csharp
// Try primary, fall back to secondary
Result<Data, Error> GetData(string key)
{
    return GetFromPrimaryCache(key)
        .OrElse(err => 
        {
            _logger.Warn($"Primary cache miss: {err}");
            return GetFromSecondaryCache(key);
        })
        .OrElse(err =>
        {
            _logger.Warn($"Secondary cache miss: {err}");
            return GetFromDatabase(key);
        });
}
```

### Converting to HTTP Responses

```csharp
public IActionResult GetUser(int id)
{
    return _userService.GetUser(id)
        .Match<IActionResult>(
            okFunc: user => Ok(user),
            errFunc: error => error switch
            {
                UserError.NotFound => NotFound(),
                UserError.InvalidEmail => BadRequest("Invalid email"),
                _ => StatusCode(500, "Internal error")
            }
        );
}
```

### Combining Multiple Results

```csharp
// All must succeed
public Result<Order, Error> CreateOrder(int userId, int productId, int quantity)
{
    return GetUser(userId)
        .Bind(user => GetProduct(productId)
            .Map(product => (user, product)))
        .Bind(tuple => CheckStock(tuple.product, quantity)
            .Map(stock => (tuple.user, tuple.product, stock)))
        .Map(tuple => new Order(tuple.user, tuple.product, tuple.stock));
}
```

---

## Side Effects

### Tap: Do Something on Success

```csharp
var result = GetUser(42)
    .Tap(user => _logger.Info($"Found user: {user.Id}"))
    .Tap(user => _metrics.Increment("users.found"))
    .Map(user => user.Email);
```

### TapError: Do Something on Failure

```csharp
var result = GetUser(42)
    .TapError(err => _logger.Error($"User lookup failed: {err}"))
    .TapError(err => _metrics.Increment("users.notfound"));
```

Tap methods return the original Result unchanged, allowing chaining.

---

## Result vs Option: When to Use Which

| Scenario | Use |
|----------|-----|
| Value might not exist | `Option<T>` |
| Need to know why it failed | `Result<T, E>` |
| Dictionary lookup | `Option<T>` |
| API call that can fail | `Result<T, E>` |
| Parsing/validation | `Result<T, E>` |
| First element of collection | `Option<T>` |
| Database operation | `Result<T, E>` |

**Rule of thumb:** If "not found" and "error" are different things, use `Result<T, E>`.

---

## Quick Reference

| Operation | Purpose | Example |
|-----------|---------|---------|
| `Ok(value)` | Create success | `Result<int, string>.Ok(42)` |
| `Error(err)` | Create failure | `Result<int, string>.Error("oops")` |
| `IsOk` / `IsError` | Check state | `if (result.IsOk)` |
| `GetValueOr(default)` | Extract with fallback | `result.GetValueOr(0)` |
| `GetErrorOr(default)` | Extract error with fallback | `result.GetErrorOr("unknown")` |
| `Map(f)` | Transform success | `result.Map(x => x * 2)` |
| `MapError(f)` | Transform error | `result.MapError(e => new ApiError(e))` |
| `Bind(f)` | Chain Result functions | `result.Bind(x => Validate(x))` |
| `Match(ok, err)` | Handle both cases | `result.Match(v => ..., e => ...)` |
| `OrElse(recovery)` | Try to recover | `result.OrElse(e => Fallback())` |
| `Tap(action)` | Side effect on success | `result.Tap(x => Log(x))` |
| `TapError(action)` | Side effect on error | `result.TapError(e => Log(e))` |

---

## What's Next?

- **[Validation Explained](../CoreTypes.md#validation)** - When you need ALL errors, not just the first
- **[Railway-Oriented Programming](RailwayOrientedProgramming.md)** - The mental model
- **[Composition Patterns](CompositionPatterns.md)** - Advanced techniques
