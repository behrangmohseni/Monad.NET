# Composition Patterns

> **For C# developers:** This guide shows how to combine operations elegantly using functional composition.

## What is Composition?

Composition is building complex operations from simple ones. You already do this with LINQ:

```csharp
var result = numbers
    .Where(n => n > 0)      // Filter
    .Select(n => n * 2)     // Transform
    .OrderBy(n => n)        // Sort
    .Take(5);               // Limit
```

Each operation takes input, does one thing, and passes output to the next. This is composition.

Monad.NET lets you compose operations that might fail the same way.

---

## The Building Blocks

### Map: Transform Values

`Map` applies a function to the value inside a container:

```csharp
// Option
Option<int>.Some(5).Map(x => x * 2);  // Some(10)

// Result
Result<int, string>.Ok(5).Map(x => x * 2);  // Ok(10)

// If empty/error, Map does nothing
Option<int>.None().Map(x => x * 2);  // None
Result<int, string>.Error("oops").Map(x => x * 2);  // Error("oops")
```

**Mental model:** Map is like LINQ's `Select` - it transforms what's inside without changing the container structure.

### Bind: Chain Dependent Operations

`Bind` chains operations where each step might also return a container:

```csharp
Option<int> ParseInt(string s) =>
    int.TryParse(s, out var n) ? Option<int>.Some(n) : Option<int>.None();

Option<int> Half(int n) =>
    n % 2 == 0 ? Option<int>.Some(n / 2) : Option<int>.None();

// Chain them
Option<int> result = ParseInt("10").Bind(Half);  // Some(5)
Option<int> result2 = ParseInt("abc").Bind(Half);  // None (parse failed)
Option<int> result3 = ParseInt("7").Bind(Half);  // None (7 is odd)
```

**Mental model:** Bind is like LINQ's `SelectMany` - it "flattens" nested containers.

### The Difference

```csharp
// Map: transform the value, keep the wrapper
Option<int>.Some(5).Map(x => x.ToString());  // Option<string>

// Bind: apply a function that also returns a wrapper
Option<int>.Some(5).Bind(x => FindUser(x));  // Option<User> (not Option<Option<User>>)
```

---

## Common Composition Patterns

### Pattern 1: Linear Pipeline

Chain operations in sequence:

```csharp
public Result<Order, Error> ProcessOrder(OrderRequest request)
{
    return Validate(request)
        .Bind(validated => EnrichWithCustomer(validated))
        .Bind(enriched => CalculatePricing(enriched))
        .Bind(priced => ApplyDiscounts(priced))
        .Bind(discounted => ReserveInventory(discounted))
        .Bind(reserved => ProcessPayment(reserved))
        .Map(paid => CreateOrderRecord(paid));
}
```

Each step has access to the previous step's output.

### Pattern 2: Accumulating Data

Carry data through the pipeline:

```csharp
public Result<OrderContext, Error> BuildOrderContext(int customerId, int productId)
{
    return GetCustomer(customerId)
        .Bind(customer => 
            GetProduct(productId)
                .Map(product => new { customer, product }))
        .Bind(ctx => 
            GetPricing(ctx.product)
                .Map(pricing => new { ctx.customer, ctx.product, pricing }))
        .Bind(ctx => 
            GetShipping(ctx.customer)
                .Map(shipping => new OrderContext(ctx.customer, ctx.product, ctx.pricing, shipping)));
}
```

Or more cleanly with tuples:

```csharp
return GetCustomer(customerId)
    .Bind(customer => GetProduct(productId).Map(product => (customer, product)))
    .Bind(t => GetPricing(t.product).Map(pricing => (t.customer, t.product, pricing)))
    .Bind(t => GetShipping(t.customer).Map(shipping => new OrderContext(t.customer, t.product, t.pricing, shipping)));
```

### Pattern 3: Independent Operations

When operations don't depend on each other:

```csharp
// These can run independently
var customerResult = GetCustomer(customerId);
var productResult = GetProduct(productId);
var inventoryResult = GetInventory(productId);

// Combine them
return customerResult
    .Bind(customer => productResult.Map(product => (customer, product)))
    .Bind(t => inventoryResult.Map(inv => new OrderContext(t.customer, t.product, inv)));
```

### Pattern 4: Conditional Operations

Apply operations conditionally:

```csharp
public Result<Order, Error> ProcessOrder(OrderRequest request)
{
    return Validate(request)
        .Bind(order => request.HasCoupon
            ? ApplyCoupon(order, request.CouponCode)
            : Result<Order, Error>.Ok(order))
        .Bind(order => request.IsRush
            ? AddRushFee(order)
            : Result<Order, Error>.Ok(order))
        .Bind(FinalizeOrder);
}
```

### Pattern 5: Error Recovery

Try alternatives on failure:

```csharp
public Result<Config, Error> LoadConfig()
{
    return LoadFromEnvironment()
        .OrElse(_ => LoadFromFile("config.json"))
        .OrElse(_ => LoadFromFile("config.default.json"))
        .OrElse(_ => Result<Config, Error>.Ok(Config.Default));
}
```

### Pattern 6: Logging/Debugging

Use `Tap` for side effects without changing the value:

```csharp
public Result<Order, Error> ProcessOrder(OrderRequest request)
{
    return Validate(request)
        .Tap(order => _logger.Info($"Validated: {order.Id}"))
        .Bind(ApplyDiscounts)
        .Tap(order => _logger.Info($"Discounted total: {order.Total}"))
        .Bind(ProcessPayment)
        .Tap(order => _logger.Info($"Payment processed: {order.PaymentId}"))
        .TapError(error => _logger.Error($"Order failed: {error}"));
}
```

---

## Combining Options and Results

### Option to Result

Convert when you need error information:

```csharp
Option<User> maybeUser = FindUser(email);

// Convert with custom error
Result<User, string> userResult = maybeUser
    .ToResult("User not found");

// Or with typed error
Result<User, UserError> typedResult = maybeUser
    .Match(
        someFunc: user => Result<User, UserError>.Ok(user),
        noneFunc: () => Result<User, UserError>.Error(new UserError.NotFound(email))
    );
```

### Result to Option

When you only care about success:

```csharp
Result<User, Error> result = GetUser(42);

// Discard error information
Option<User> maybeUser = result.ToOption();
// Ok(user) -> Some(user)
// Error(_) -> None
```

### Mixed Pipelines

```csharp
public Result<OrderConfirmation, OrderError> CreateOrder(string email, string productCode)
{
    // FindUser returns Option, but we need Result
    return FindUser(email)
        .ToResult(new OrderError.UserNotFound(email))
        .Bind(user => 
            // FindProduct also returns Option
            FindProduct(productCode)
                .ToResult(new OrderError.ProductNotFound(productCode))
                .Map(product => (user, product)))
        .Bind(t => 
            // CreateOrder returns Result
            ProcessOrder(t.user, t.product));
}
```

---

## Validation Composition

When you want ALL errors, not just the first:

### Applicative Style with Apply

```csharp
public Validation<User, string> CreateUser(string name, string email, int age)
{
    var nameVal = ValidateName(name);
    var emailVal = ValidateEmail(email);
    var ageVal = ValidateAge(age);

    // Combine all validations - accumulates ALL errors
    return nameVal
        .Apply(emailVal, (n, e) => (name: n, email: e))
        .Apply(ageVal, (tuple, a) => new User(tuple.name, tuple.email, a));
}

Validation<string, string> ValidateName(string name) =>
    !string.IsNullOrWhiteSpace(name)
        ? Validation<string, string>.Ok(name)
        : Validation<string, string>.Error("Name is required");

Validation<string, string> ValidateEmail(string email) =>
    email.Contains("@")
        ? Validation<string, string>.Ok(email)
        : Validation<string, string>.Error("Invalid email format");

Validation<int, string> ValidateAge(int age) =>
    age >= 18
        ? Validation<int, string>.Ok(age)
        : Validation<int, string>.Error("Must be 18 or older");
```

If all validations fail, you get ALL error messages, not just the first.

---

## Advanced: Creating Reusable Pipelines

### Composable Functions

Create reusable steps:

```csharp
// Define reusable steps
Func<Order, Result<Order, Error>> applyTax = order =>
    Result<Order, Error>.Ok(order with { Total = order.Total * 1.1m });

Func<Order, Result<Order, Error>> applyDiscount = order =>
    order.Total > 100
        ? Result<Order, Error>.Ok(order with { Total = order.Total * 0.9m })
        : Result<Order, Error>.Ok(order);

Func<Order, Result<Order, Error>> validateTotal = order =>
    order.Total > 0
        ? Result<Order, Error>.Ok(order)
        : Result<Order, Error>.Error(new Error("Invalid total"));

// Compose them
public Result<Order, Error> ProcessOrder(Order order)
{
    return Result<Order, Error>.Ok(order)
        .Bind(applyTax)
        .Bind(applyDiscount)
        .Bind(validateTotal);
}
```

### Pipeline Builder Pattern

For complex pipelines:

```csharp
public class OrderPipeline
{
    private readonly List<Func<Order, Result<Order, Error>>> _steps = new();

    public OrderPipeline AddStep(Func<Order, Result<Order, Error>> step)
    {
        _steps.Add(step);
        return this;
    }

    public Result<Order, Error> Execute(Order order)
    {
        return _steps.Aggregate(
            Result<Order, Error>.Ok(order),
            (current, step) => current.Bind(step)
        );
    }
}

// Usage
var pipeline = new OrderPipeline()
    .AddStep(ApplyTax)
    .AddStep(ApplyDiscount)
    .AddStep(ValidateTotal)
    .AddStep(ReserveInventory);

var result = pipeline.Execute(order);
```

---

## Quick Reference

| Pattern | When to Use | Example |
|---------|-------------|---------|
| Linear Pipeline | Sequential steps | `A().Bind(B).Bind(C)` |
| Accumulating | Need data from earlier steps | `.Map(x => (x, prev))` |
| Recovery | Try alternatives | `.OrElse(e => Fallback())` |
| Conditional | Optional steps | `cond ? Step() : Ok(x)` |
| Logging | Debug without changing | `.Tap(x => Log(x))` |
| Validation | Collect all errors | `.Apply(other, combine)` |

---

## What's Next?

- **[From OOP to FP](FromOopToFp.md)** - Shifting your mental model
- **[Railway-Oriented Programming](RailwayOrientedProgramming.md)** - The underlying concept
- **[Core Types Reference](../CoreTypes.md)** - All available types and operations
