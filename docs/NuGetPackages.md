# NuGet Packages

Monad.NET is distributed as multiple NuGet packages, allowing you to install only what you need.

## All Packages

| Package | Version | Downloads | Description |
|---------|---------|-----------|-------------|
| **Monad.NET** | [![NuGet](https://img.shields.io/nuget/v/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/) | [![Downloads](https://img.shields.io/nuget/dt/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/) | Core library with Option, Result, Validation, Try, and more |
| **Monad.NET.Analyzers** | [![NuGet](https://img.shields.io/nuget/v/Monad.NET.Analyzers.svg)](https://www.nuget.org/packages/Monad.NET.Analyzers/) | [![Downloads](https://img.shields.io/nuget/dt/Monad.NET.Analyzers.svg)](https://www.nuget.org/packages/Monad.NET.Analyzers/) | Roslyn analyzers for common mistakes and best practices |
| **Monad.NET.SourceGenerators** | [![NuGet](https://img.shields.io/nuget/v/Monad.NET.SourceGenerators.svg)](https://www.nuget.org/packages/Monad.NET.SourceGenerators/) | [![Downloads](https://img.shields.io/nuget/dt/Monad.NET.SourceGenerators.svg)](https://www.nuget.org/packages/Monad.NET.SourceGenerators/) | Source generators for discriminated unions (`[Union]` attribute) |
| **Monad.NET.AspNetCore** | [![NuGet](https://img.shields.io/nuget/v/Monad.NET.AspNetCore.svg)](https://www.nuget.org/packages/Monad.NET.AspNetCore/) | [![Downloads](https://img.shields.io/nuget/dt/Monad.NET.AspNetCore.svg)](https://www.nuget.org/packages/Monad.NET.AspNetCore/) | ASP.NET Core integration (Result-to-ActionResult conversions) |
| **Monad.NET.EntityFrameworkCore** | [![NuGet](https://img.shields.io/nuget/v/Monad.NET.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Monad.NET.EntityFrameworkCore/) | [![Downloads](https://img.shields.io/nuget/dt/Monad.NET.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Monad.NET.EntityFrameworkCore/) | EF Core integration (Option value converters, queryable extensions) |
| **Monad.NET.MessagePack** | [![NuGet](https://img.shields.io/nuget/v/Monad.NET.MessagePack.svg)](https://www.nuget.org/packages/Monad.NET.MessagePack/) | [![Downloads](https://img.shields.io/nuget/dt/Monad.NET.MessagePack.svg)](https://www.nuget.org/packages/Monad.NET.MessagePack/) | MessagePack serialization support for all monad types |

---

## Installation

### Core Package (Required)

```bash
dotnet add package Monad.NET
```

### Optional Packages

```bash
# Roslyn analyzers for better code quality
dotnet add package Monad.NET.Analyzers

# Discriminated unions via source generators
dotnet add package Monad.NET.SourceGenerators

# ASP.NET Core integration
dotnet add package Monad.NET.AspNetCore

# Entity Framework Core integration
dotnet add package Monad.NET.EntityFrameworkCore

# MessagePack serialization
dotnet add package Monad.NET.MessagePack
```

---

## Package Details

### Monad.NET

The core library containing all monad types:

- `Option<T>` — Handle missing values without null
- `Result<T, E>` — Type-safe error handling
- `Validation<T, E>` — Accumulate all validation errors
- `Try<T>` — Wrap exception-throwing code
- `NonEmptyList<T>` — List guaranteed to have at least one element
- `RemoteData<T, E>` — Model async data loading states
- `Reader<R, A>` — Dependency injection without containers
- `Writer<W, T>` — Accumulate logs alongside results
- `State<S, A>` — Thread state through computations
- `IO<T>` — Defer and compose side effects

### Monad.NET.Analyzers

Roslyn analyzers that catch common mistakes:

- Discarded monad values (unused `Option`, `Result`, etc.)
- Unchecked unwrap calls
- Null comparisons on `Option`
- Redundant map chains
- Missing `ConfigureAwait(false)` on async methods
- And more...

See the [Analyzers Guide](Guides/Analyzers.md) for the full list.

### Monad.NET.SourceGenerators

Source generators for creating discriminated unions:

```csharp
[Union]
public abstract partial record PaymentResult
{
    public partial record Success(string TransactionId) : PaymentResult;
    public partial record Declined(string Reason) : PaymentResult;
    public partial record Error(Exception Ex) : PaymentResult;
}

// Generated: exhaustive Match method
result.Match(
    success: s => $"Paid: {s.TransactionId}",
    declined: d => $"Declined: {d.Reason}",
    error: e => $"Error: {e.Ex.Message}"
);
```

### Monad.NET.AspNetCore

ASP.NET Core extensions for seamless integration:

```csharp
// Convert Result to ActionResult
public IActionResult GetUser(int id)
{
    return _userService.GetById(id)
        .ToActionResult(user => Ok(user));
}

// Convert Option to ActionResult  
public IActionResult FindUser(string email)
{
    return _userService.FindByEmail(email)
        .ToActionResult(user => Ok(user), () => NotFound());
}
```

### Monad.NET.EntityFrameworkCore

Entity Framework Core integration:

```csharp
// Configure Option<T> properties
modelBuilder.Entity<User>()
    .Property(u => u.MiddleName)
    .HasOptionConversion();

// Query extensions
var users = await context.Users
    .WhereOption(u => u.Email, email => email.Contains("@"))
    .ToListAsync();
```

### Monad.NET.MessagePack

MessagePack serialization support:

```csharp
// Register the resolver
var options = MessagePackSerializerOptions.Standard
    .WithResolver(CompositeResolver.Create(
        MonadResolver.Instance,
        StandardResolver.Instance
    ));

// Serialize/deserialize monad types
var bytes = MessagePackSerializer.Serialize(Option.Some(42), options);
var result = MessagePackSerializer.Deserialize<Option<int>>(bytes, options);
```

---

## Compatibility

All packages target:

- .NET Standard 2.0+ (for broad compatibility)
- .NET 6.0+ (for modern features)
- .NET 8.0+ (latest LTS)

See [Compatibility](Compatibility.md) for detailed version support.

