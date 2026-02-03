# From OOP to FP: A Mental Shift

> **For C# developers:** This guide helps you shift from object-oriented thinking to functional thinking.

## You Already Know More FP Than You Think

If you use LINQ, you already use functional programming:

```csharp
var adults = people
    .Where(p => p.Age >= 18)        // Filter (pure function)
    .Select(p => p.Name)            // Map (pure function)
    .OrderBy(name => name)          // Sort (pure function)
    .ToList();
```

This is functional code! It:
- Uses functions as arguments (`p => p.Age >= 18`)
- Doesn't mutate the original collection
- Chains operations together
- Returns a new value

Monad.NET extends this same style to error handling.

---

## The Key Mental Shifts

### Shift 1: Expressions Over Statements

**OOP Thinking (Statements):**
```csharp
string message;
if (user.IsAdmin)
{
    message = "Welcome, administrator!";
}
else
{
    message = "Welcome, user!";
}
return message;
```

**FP Thinking (Expressions):**
```csharp
return user.IsAdmin
    ? "Welcome, administrator!"
    : "Welcome, user!";
```

**With Monad.NET:**
```csharp
return GetUser(id).Match(
    okFunc: user => $"Welcome, {user.Name}!",
    errFunc: error => $"Error: {error.Message}"
);
```

Everything returns a value. No intermediate mutable state.

### Shift 2: Transform Instead of Mutate

**OOP Thinking (Mutation):**
```csharp
var order = new Order();
order.CustomerId = customerId;
order.Items = GetItems(productIds);
order.Total = CalculateTotal(order.Items);
order.Status = OrderStatus.Pending;
return order;
```

**FP Thinking (Transformation):**
```csharp
return new Order(
    customerId: customerId,
    items: GetItems(productIds),
    total: CalculateTotal(items),
    status: OrderStatus.Pending
);

// Or with records and `with`:
var order = baseOrder with { 
    Total = CalculateTotal(baseOrder.Items),
    Status = OrderStatus.Pending 
};
```

**With Monad.NET:**
```csharp
return GetCustomer(customerId)
    .Bind(customer => GetItems(productIds).Map(items => (customer, items)))
    .Map(t => new Order(t.customer, t.items, CalculateTotal(t.items)));
```

Data flows through transformations. Nothing is mutated.

### Shift 3: Functions as Values

**OOP Thinking:**
```csharp
// Create a class just to hold one method
public class PriceCalculator
{
    public decimal Calculate(Order order) => order.Items.Sum(i => i.Price);
}
```

**FP Thinking:**
```csharp
// Functions are just values
Func<Order, decimal> calculatePrice = order => order.Items.Sum(i => i.Price);

// Pass them around
var total = calculatePrice(order);

// Store them in collections
var calculators = new Dictionary<string, Func<Order, decimal>>
{
    ["standard"] = order => order.Items.Sum(i => i.Price),
    ["discounted"] = order => order.Items.Sum(i => i.Price) * 0.9m,
    ["premium"] = order => order.Items.Sum(i => i.Price) * 0.8m
};
```

### Shift 4: Handle All Cases Explicitly

**OOP Thinking (Happy Path Focus):**
```csharp
public User GetUser(int id)
{
    var user = _db.Find(id);
    // Hope this doesn't happen...
    if (user == null) throw new UserNotFoundException(id);
    return user;
}
```

**FP Thinking (All Paths Explicit):**
```csharp
public Result<User, UserError> GetUser(int id)
{
    var user = _db.Find(id);
    return user != null
        ? Result<User, UserError>.Ok(user)
        : Result<User, UserError>.Error(new UserError.NotFound(id));
}
```

The type signature tells the complete story.

### Shift 5: Compose, Don't Inherit

**OOP Thinking (Inheritance):**
```csharp
public abstract class OrderProcessor
{
    public Order Process(Order order)
    {
        order = Validate(order);
        order = CalculatePricing(order);
        order = ApplyDiscounts(order);
        return Finalize(order);
    }
    
    protected abstract Order Validate(Order order);
    protected abstract Order ApplyDiscounts(Order order);
    // ... override in subclasses
}
```

**FP Thinking (Composition):**
```csharp
public Result<Order, Error> ProcessOrder(Order order)
{
    return Validate(order)
        .Bind(CalculatePricing)
        .Bind(ApplyDiscounts)
        .Bind(Finalize);
}

// Change behavior by composing different functions
var standardPipeline = Validate.Then(CalculatePricing).Then(Finalize);
var premiumPipeline = Validate.Then(CalculatePricing).Then(ApplyPremiumDiscounts).Then(Finalize);
```

---

## Common Patterns Translated

### Null Check Chains

**Before:**
```csharp
if (user != null && user.Address != null && user.Address.City != null)
{
    return user.Address.City.ToUpper();
}
return "UNKNOWN";
```

**After:**
```csharp
return user.ToOption()
    .Bind(u => u.Address.ToOption())
    .Bind(a => a.City.ToOption())
    .Map(city => city.ToUpper())
    .GetValueOr("UNKNOWN");
```

### Try/Catch to Result

**Before:**
```csharp
try
{
    var user = GetUser(id);
    var order = CreateOrder(user);
    var payment = ProcessPayment(order);
    return new Success(payment);
}
catch (UserNotFoundException ex)
{
    return new Failure("User not found");
}
catch (PaymentException ex)
{
    return new Failure("Payment failed");
}
```

**After:**
```csharp
return GetUser(id)
    .Bind(user => CreateOrder(user))
    .Bind(order => ProcessPayment(order));
// Error type is explicit in the signature
```

### Factory Pattern

**Before:**
```csharp
public class UserFactory
{
    public User Create(string name, string email)
    {
        if (string.IsNullOrEmpty(name))
            throw new ValidationException("Name required");
        if (!email.Contains("@"))
            throw new ValidationException("Invalid email");
        return new User(name, email);
    }
}
```

**After:**
```csharp
public static Validation<User, string> CreateUser(string name, string email)
{
    var nameVal = !string.IsNullOrEmpty(name)
        ? Validation<string, string>.Ok(name)
        : Validation<string, string>.Error("Name required");
    
    var emailVal = email.Contains("@")
        ? Validation<string, string>.Ok(email)
        : Validation<string, string>.Error("Invalid email");
    
    return nameVal.Apply(emailVal, (n, e) => new User(n, e));
}
```

### Service Layer

**Before:**
```csharp
public class OrderService
{
    private readonly IUserRepository _users;
    private readonly IProductRepository _products;
    
    public OrderResult ProcessOrder(OrderRequest request)
    {
        try
        {
            var user = _users.GetById(request.UserId)
                ?? throw new NotFoundException("User not found");
            var product = _products.GetById(request.ProductId)
                ?? throw new NotFoundException("Product not found");
            
            if (product.Stock < request.Quantity)
                throw new BusinessException("Insufficient stock");
            
            // ... more logic
            return new OrderResult { Success = true };
        }
        catch (Exception ex)
        {
            return new OrderResult { Success = false, Error = ex.Message };
        }
    }
}
```

**After:**
```csharp
public class OrderService
{
    public Result<Order, OrderError> ProcessOrder(OrderRequest request)
    {
        return GetUser(request.UserId)
            .Bind(user => GetProduct(request.ProductId)
                .Map(product => (user, product)))
            .Bind(t => CheckStock(t.product, request.Quantity)
                .Map(_ => t))
            .Bind(t => CreateOrder(t.user, t.product, request.Quantity));
    }
    
    private Result<User, OrderError> GetUser(int id) =>
        _users.FindById(id)
            .ToOption()
            .Match(
                someFunc: user => Result<User, OrderError>.Ok(user),
                noneFunc: () => Result<User, OrderError>.Error(new OrderError.UserNotFound(id)));
    
    // ... similar patterns for other methods
}
```

---

## When to Use Which Style

### Prefer FP Style When:

| Scenario | Why |
|----------|-----|
| Data transformations | Pure functions are easier to test and reason about |
| Error handling | Explicit error types prevent surprises |
| Validation | Accumulating errors is better UX |
| Pipelines/workflows | Composition is more readable than nested calls |
| Concurrent code | Immutability prevents race conditions |

### Prefer OOP Style When:

| Scenario | Why |
|----------|-----|
| Stateful resources | Database connections, file handles need management |
| Framework requirements | ASP.NET, EF Core expect certain patterns |
| Team familiarity | Don't force FP on a team that doesn't know it |
| Simple CRUD | Sometimes a repository class is just simpler |
| Interop | External libraries expect OOP patterns |

### Hybrid Approach (Recommended)

Use FP inside OOP structures:

```csharp
// OOP structure for dependency injection
public class OrderService : IOrderService
{
    private readonly IUserRepository _users;
    private readonly IProductRepository _products;
    
    public OrderService(IUserRepository users, IProductRepository products)
    {
        _users = users;
        _products = products;
    }
    
    // FP style inside the method
    public Result<Order, OrderError> ProcessOrder(OrderRequest request)
    {
        return ValidateRequest(request)
            .Bind(GetOrderContext)
            .Bind(CheckBusinessRules)
            .Map(CreateOrder);
    }
}
```

---

## The Vocabulary

FP has its own jargon. Here's a translation:

| FP Term | C# Equivalent | Plain English |
|---------|---------------|---------------|
| Pure function | Method with no side effects | Same input → same output, always |
| Immutable | `readonly`, records | Can't be changed after creation |
| Functor | Has `Map`/`Select` | Something you can transform inside |
| Monad | Has `Bind`/`SelectMany` | Something you can chain |
| Applicative | Has `Apply` | Combine independent operations |
| Higher-order function | `Func<>` parameter | Function that takes/returns functions |
| Currying | Partial application | Breaking multi-arg into single-arg |
| Composition | Method chaining | Building big from small |

You don't need to memorize these. They're just names for things you already do.

---

## Start Small

Don't rewrite your codebase. Start with:

1. **One new feature** - Try `Result<T, E>` for a new endpoint
2. **Validation logic** - Use `Validation<T, E>` for form validation
3. **Null-heavy code** - Replace null checks with `Option<T>`
4. **Data pipelines** - Use `Map`/`Bind` for transformations

As you get comfortable, expand to more areas.

---

## Quick Reference

| OOP Pattern | FP Equivalent |
|-------------|---------------|
| `null` | `Option.None()` |
| `throw new Exception()` | `Result.Error()` |
| `try/catch` | `Match(ok, err)` |
| Mutable state | Immutable records + `with` |
| Inheritance | Composition |
| `void` methods with side effects | `Tap`, `TapError` |
| Factory class | Smart constructor returning `Result`/`Validation` |

---

## What's Next?

- **[Why Functional Error Handling](WhyFunctionalErrorHandling.md)** - The motivation
- **[Railway-Oriented Programming](RailwayOrientedProgramming.md)** - The mental model
- **[Core Types Reference](../CoreTypes.md)** - Start using Monad.NET
