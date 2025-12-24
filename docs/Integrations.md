# Integrations

Monad.NET integrates with popular .NET frameworks and provides source generators for discriminated unions.

## Table of Contents

- [Source Generators](#source-generators)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Entity Framework Core Integration](#entity-framework-core-integration)

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

The generator creates utility methods for your union types:

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

The `Monad.NET.AspNetCore` package provides integration with ASP.NET Core:

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

The `Monad.NET.EntityFrameworkCore` package provides integration with EF Core:

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

[← Examples](Examples.md) | [Back to README](../README.md)

