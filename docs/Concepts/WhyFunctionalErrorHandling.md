# Why Functional Error Handling?

> **For C# developers:** This guide explains why you might want to handle errors differently than the traditional `try/catch` approach you're used to.

## The Problem with Exceptions

As a C# developer, you've written code like this thousands of times:

```csharp
public User GetUser(int id)
{
    var user = _database.Find(id);
    if (user == null)
        throw new UserNotFoundException($"User {id} not found");
    return user;
}
```

This looks fine. But let's examine the problems hiding in plain sight.

### Problem 1: Hidden Control Flow

Look at this method signature:

```csharp
public User GetUser(int id)
```

What can go wrong? The signature says "give me an int, get a User." But that's a lie. This method might:

- Throw `UserNotFoundException`
- Throw `SqlException` (database timeout)
- Throw `InvalidOperationException` (connection not open)
- Throw `ArgumentException` (negative id)
- Return null (if you forgot the null check)

**The caller has no idea.** They must read the implementation, check documentation (if it exists), or discover exceptions at runtime.

```csharp
// Caller thinks this is safe
var user = GetUser(42);
Console.WriteLine(user.Name); // 💥 Might explode
```

### Problem 2: Exceptions Are Invisible to the Compiler

C# won't warn you about unhandled exceptions:

```csharp
public void ProcessOrder(int userId, int productId)
{
    var user = GetUser(userId);        // Might throw
    var product = GetProduct(productId); // Might throw
    var payment = ChargeCard(user);     // Might throw
    var order = CreateOrder(user, product, payment); // Might throw
    
    SendConfirmation(order); // If we get here...
}
```

Any line can explode. The compiler is perfectly happy. You discover the bug when a customer complains.

### Problem 3: Try/Catch Doesn't Compose

Want to handle errors from multiple operations? It gets ugly fast:

```csharp
public Order ProcessOrder(int userId, int productId)
{
    User user;
    try
    {
        user = GetUser(userId);
    }
    catch (UserNotFoundException ex)
    {
        _logger.Error(ex);
        throw new OrderException("Invalid user", ex);
    }
    
    Product product;
    try
    {
        product = GetProduct(productId);
    }
    catch (ProductNotFoundException ex)
    {
        _logger.Error(ex);
        throw new OrderException("Invalid product", ex);
    }
    
    // ... and on and on
}
```

This is:
- Hard to read
- Hard to maintain
- Easy to get wrong (did we handle all the exception types?)

### Problem 4: Expected vs. Unexpected Failures

Exceptions were designed for *exceptional* circumstances - things that shouldn't happen:
- Out of memory
- Stack overflow
- Network cable unplugged

But we use them for *expected* failures too:
- User not found (happens all the time)
- Invalid input (users make mistakes)
- Business rule violations (orders over limit)

This conflates two very different concepts.

---

## A Different Approach: Errors as Values

What if we made errors *visible* and *explicit*? What if the compiler could help us?

### Making Failure Explicit in the Signature

```csharp
public Result<User, UserError> GetUser(int id)
```

Now the signature tells the truth:
- "Give me an int"
- "You'll get *either* a User *or* a UserError"
- "You must handle both possibilities"

The caller *knows* this can fail:

```csharp
var result = GetUser(42);

// You can't just use it - you must handle both cases
var message = result.Match(
    okFunc: user => $"Hello, {user.Name}!",
    errFunc: error => $"Error: {error.Message}"
);
```

### The Compiler Becomes Your Friend

With `Result<T, E>`, you can't accidentally ignore errors:

```csharp
// This won't compile - Result<User, UserError> is not User
User user = GetUser(42); // ❌ Compile error

// You must explicitly handle the result
Result<User, UserError> result = GetUser(42); // ✅
```

### Composition Becomes Natural

Remember the ugly nested try/catch? With Result:

```csharp
public Result<Order, OrderError> ProcessOrder(int userId, int productId)
{
    return GetUser(userId)
        .MapError(e => new OrderError.InvalidUser(e))
        .Bind(user => GetProduct(productId)
            .MapError(e => new OrderError.InvalidProduct(e))
            .Map(product => (user, product)))
        .Bind(tuple => ChargeCard(tuple.user)
            .MapError(e => new OrderError.PaymentFailed(e))
            .Map(payment => CreateOrder(tuple.user, tuple.product, payment)));
}
```

This is:
- **Linear** - no nesting
- **Complete** - all errors handled
- **Type-safe** - compiler verifies everything

---

## When to Use Each Approach

Functional error handling isn't always better. Here's a guide:

### Use Exceptions When:

| Scenario | Why |
|----------|-----|
| **Truly exceptional** | Programmer errors, environment failures |
| **Can't recover** | Out of memory, stack overflow |
| **Crossing boundaries** | Framework requirements, library APIs |
| **Fail-fast desired** | Crash early in development |

```csharp
// Good use of exception - this should never happen
if (config == null)
    throw new InvalidOperationException("Config not initialized");
```

### Use Result/Option When:

| Scenario | Why |
|----------|-----|
| **Expected failures** | User not found, validation failed |
| **Caller should decide** | Different callers handle differently |
| **Multiple failure modes** | Rich error information needed |
| **Chaining operations** | Pipeline of dependent steps |

```csharp
// Good use of Result - this happens regularly
public Result<User, string> FindUser(string email)
{
    var user = _db.Users.FirstOrDefault(u => u.Email == email);
    return user != null
        ? Result<User, string>.Ok(user)
        : Result<User, string>.Error("User not found");
}
```

---

## The Key Insight

**Exceptions are about control flow. Results are about data.**

With exceptions, errors jump around your call stack invisibly. With Results, errors are just values you pass around, transform, and handle - like any other data.

This makes your code:
- **Honest** - signatures tell the truth
- **Predictable** - no hidden control flow
- **Composable** - operations chain naturally
- **Testable** - just assert on return values

---

## What's Next?

Now that you understand *why* functional error handling matters, learn *how* to use it:

- **[Railway-Oriented Programming](RailwayOrientedProgramming.md)** - The mental model for error handling
- **[Option Explained](OptionExplained.md)** - Handling missing values
- **[Result Explained](ResultExplained.md)** - Handling operations that can fail

---

## Quick Reference

| Traditional C# | Functional Approach |
|---------------|---------------------|
| `throw new Exception()` | `Result.Error(...)` |
| `try { } catch { }` | `.Match(ok, err)` |
| `return null` | `Option.None()` |
| Check for null | `.Map()`, `.Bind()` |
| Nested try/catch | Chain with `.Bind()` |
