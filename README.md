# Monad.NET

[![NuGet](https://img.shields.io/nuget/v/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Monad.NET.svg)](https://www.nuget.org/packages/Monad.NET/)
[![Build](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/ci.yml)
[![CodeQL](https://github.com/behrangmohseni/Monad.NET/actions/workflows/codeql.yml/badge.svg)](https://github.com/behrangmohseni/Monad.NET/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/behrangmohseni/Monad.NET/graph/badge.svg)](https://codecov.io/gh/behrangmohseni/Monad.NET)
[![CodeFactor](https://www.codefactor.io/repository/github/behrangmohseni/monad.net/badge/main)](https://www.codefactor.io/repository/github/behrangmohseni/monad.net)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_Standard-2.0%2B-512BD4.svg)](https://dotnet.microsoft.com/)

**Monad.NET** is a functional programming library for .NET. Option, Result, Either, Validation, and more — with zero dependencies.

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
- [Which Monad Should I Use?](#which-monad-should-i-use)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Documentation](#documentation)
- [Examples](#examples)
- [Performance](#performance)
- [Resources](#resources)
- [FAQ](#faq)
- [Contributing](#contributing)

---

## Why Monad.NET?

Modern C# has excellent features—nullable reference types, pattern matching, records. So why use Monad.NET?

**The short answer:** Composability. While C# handles individual cases well, chaining operations that might fail, be absent, or need validation quickly becomes verbose. Monad.NET provides a unified API for composing these operations elegantly.

### Honest Comparisons with Modern C#

#### Optional Values: `Option<T>` vs Nullable Reference Types

**Modern C# (NRT enabled):**
```csharp
User? user = FindUser(id);
if (user is not null)
{
    Profile? profile = user.GetProfile();
    if (profile is not null)
    {
        return profile.Email;  // Still might be null!
    }
}
return "default@example.com";
```

**With Monad.NET:**
```csharp
return FindUser(id)
    .AndThen(user => user.GetProfile())
    .Map(profile => profile.Email)
    .UnwrapOr("default@example.com");
```

**Verdict:** NRTs catch null issues at compile time—use them! But `Option<T>` shines when you need to *chain* operations or *transform* optional values. If you're writing nested null checks, Option is cleaner.

---

#### Error Handling: `Result<T, E>` vs Exceptions

**Modern C# with exceptions:**
```csharp
public Order ProcessOrder(OrderRequest request)
{
    try
    {
        var validated = ValidateOrder(request);      // throws ValidationException
        var inventory = ReserveInventory(validated); // throws InventoryException
        var payment = ChargePayment(inventory);      // throws PaymentException
        return CreateOrder(payment);
    }
    catch (ValidationException ex) { /* handle */ }
    catch (InventoryException ex) { /* handle */ }
    catch (PaymentException ex) { /* handle */ }
}
```

**With Monad.NET:**
```csharp
public Result<Order, OrderError> ProcessOrder(OrderRequest request)
{
    return ValidateOrder(request)
        .AndThen(ReserveInventory)
        .AndThen(ChargePayment)
        .AndThen(CreateOrder);
}
```

**Verdict:** Exceptions are fine for *exceptional* situations (network failures, disk errors). Use `Result<T, E>` when failure is *expected* (validation errors, business rule violations). The signature `Result<Order, OrderError>` tells callers exactly what can go wrong—no surprises.

---

#### Validation: `Validation<T, E>` vs FluentValidation

**With FluentValidation (industry standard):**
```csharp
public class UserValidator : AbstractValidator<UserRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}

// Usage
var result = await validator.ValidateAsync(request);
if (!result.IsValid)
    return BadRequest(result.Errors);
```

**With Monad.NET:**
```csharp
var user = ValidateName(request.Name)
    .Apply(ValidateEmail(request.Email), (name, email) => (name, email))
    .Apply(ValidateAge(request.Age), (partial, age) => new User(partial.name, partial.email, age));
```

**Verdict:** FluentValidation is battle-tested and has more features (async rules, dependency injection, localization). Use it for complex scenarios. `Validation<T, E>` is lighter, has no dependencies, and works well with other Monad.NET types. Choose based on your needs.

---

#### Discriminated Unions: The Missing Feature in C#

**The Problem:** C# still lacks native discriminated unions (sum types) as of [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14). Despite adding extension members, null-conditional assignment, field-backed properties, and other features—discriminated unions didn't make the cut. This remains one of the [most requested language features](https://github.com/dotnet/csharplang/issues/8928), with the proposal actively discussed by the C# Language Design Team. F#, Rust, Swift, Kotlin, and TypeScript all have this feature. C# developers have been waiting for years.

**With Monad.NET Source Generators:**
```csharp
[Union]
public abstract partial record GetUserResult
{
    public partial record Success(User User) : GetUserResult;
    public partial record NotFound : GetUserResult;
    public partial record ValidationError(string Message) : GetUserResult;
}

// Exhaustive matching - compiler ensures all cases handled
result.Match(
    success: s => Ok(s.User),
    notFound: _ => NotFound(),
    validationError: e => BadRequest(e.Message)
);
```

---

### Design Principles

1. **Explicit over implicit** — No hidden nulls, no surprise exceptions
2. **Composition over inheritance** — Small, focused types that combine well
3. **Immutability by default** — All types are immutable and thread-safe
4. **Zero dependencies** — Only the .NET runtime, nothing else

---

## Which Monad Should I Use?

| Scenario | Use This |
|----------|----------|
| A value might be missing | `Option<T>` |
| An operation can fail with a typed error | `Result<T, E>` |
| Need to show ALL validation errors at once | `Validation<T, E>` |
| Wrapping code that throws exceptions | `Try<T>` |
| A list must have at least one item | `NonEmptyList<T>` |
| UI state for async data loading (Blazor) | `RemoteData<T, E>` |
| Return one of two different types | `Either<L, R>` |
| Compose async operations with shared dependencies | `ReaderAsync<R, A>` |
| Dependency injection without DI container | `Reader<R, A>` |
| Need to accumulate logs/traces alongside results | `Writer<W, T>` |
| Thread state through pure computations | `State<S, A>` |
| Defer and compose side effects | `IO<T>` |

### Language Inspirations

These types come from functional programming languages. Here's the lineage:

| Monad.NET | F# | Rust | Haskell |
|-----------|-----|------|---------|
| `Option<T>` | `Option<'T>` | `Option<T>` | `Maybe a` |
| `Result<T,E>` | `Result<'T,'E>` | `Result<T,E>` | `Either a b` |
| `Either<L,R>` | `Choice<'T1,'T2>` | — | `Either a b` |
| `Validation<T,E>` | — | — | `Validation e a` |
| `Try<T>` | — | — | — (Scala) |
| `RemoteData<T,E>` | — | — | — (Elm) |
| `NonEmptyList<T>` | — | — | `NonEmpty a` |
| `Writer<W,T>` | — | — | `Writer w a` |
| `Reader<R,A>` | — | — | `Reader r a` |
| `ReaderAsync<R,A>` | — | — | `ReaderT IO r a` |
| `State<S,A>` | — | — | `State s a` |
| `IO<T>` | — | — | `IO a` |

---

## Installation

```bash
# Core library
dotnet add package Monad.NET

# Optional: Discriminated unions via source generators
dotnet add package Monad.NET.SourceGenerators

# Optional: ASP.NET Core integration
dotnet add package Monad.NET.AspNetCore

# Optional: Entity Framework Core integration
dotnet add package Monad.NET.EntityFrameworkCore
```

---

## Quick Start

### Option — Handle missing values

```csharp
// Method syntax (recommended)
var email = FindUser(id)
    .Select(user => user.Email)
    .Where(email => email.Contains("@"))
    .SelectMany(email => ValidateEmail(email));

// Or use Map/Filter/AndThen
var email = FindUser(id)
    .Map(user => user.Email)
    .Filter(email => email.Contains("@"))
    .AndThen(email => ValidateEmail(email));
```

### Result — Handle expected failures

```csharp
public Result<Order, OrderError> ProcessOrder(OrderRequest request)
{
    return ValidateOrder(request)
        .AndThen(order => CheckInventory(order))
        .AndThen(order => ChargePayment(order))
        .Tap(order => _logger.LogInfo($"Order {order.Id} created"))
        .TapErr(err => _logger.LogError($"Order failed: {err}"));
}
```

### Validation — Collect all errors

```csharp
var user = ValidateName(form.Name)
    .Apply(ValidateEmail(form.Email), (name, email) => (name, email))
    .Apply(ValidateAge(form.Age), (partial, age) => new User(partial.name, partial.email, age));

// Shows ALL validation errors at once
user.Match(
    valid: u => CreateUser(u),
    invalid: errors => ShowErrors(errors)
);
```

> **Important:** LINQ query syntax (`from...select`) on Validation **short-circuits** on the first error. Use `Apply()` or `Zip()` to accumulate all errors.

### Discriminated Unions — Type-safe alternatives

```csharp
[Union]
public abstract partial record Shape
{
    public partial record Circle(double Radius) : Shape;
    public partial record Rectangle(double Width, double Height) : Shape;
}

// Exhaustive matching
var area = shape.Match(
    circle: c => Math.PI * c.Radius * c.Radius,
    rectangle: r => r.Width * r.Height
);
```

---

## Documentation

| Document | Description |
|----------|-------------|
| [Quick Start Guide](docs/QUICKSTART.md) | Get up and running in 5 minutes |
| [Core Types](docs/CoreTypes.md) | Detailed docs for Option, Result, Either, Validation, Try, and more |
| [Advanced Usage](docs/AdvancedUsage.md) | LINQ, async, collection operations, parallel processing |
| [Examples](docs/Examples.md) | Real-world code samples |
| [Integrations](docs/Integrations.md) | Source Generators, ASP.NET Core, Entity Framework Core |
| [API Reference](docs/API.md) | Complete API documentation |
| [Compatibility](docs/Compatibility.md) | Supported .NET versions |
| [Performance Benchmarks](docs/PerformanceBenchmarks.md) | Detailed performance comparisons and analysis |
| [Versioning Policy](docs/VersioningPolicy.md) | API versioning and deprecation policy |
| [Pitfalls & Gotchas](docs/Guides/Pitfalls.md) | Common mistakes to avoid |
| [Logging Guidance](docs/Guides/Logging.md) | Best practices for logging |
| [Type Selection Guide](docs/Guides/TypeSelectionGuide.md) | Decision flowchart for choosing the right type |
| [Migration Guide](docs/Guides/MigrationGuide.md) | Migrate from language-ext, OneOf, FluentResults |
| [Architectural Decisions](docs/ArchitecturalDecisions.md) | Design decisions, rationale, and trade-offs |

---

## Examples

The `examples/` folder contains a comprehensive example application:

- `examples/Monad.NET.Examples` — Interactive console app demonstrating all monad types with real-world patterns.

---

## Performance

Monad.NET is designed for correctness and safety first, but performance is still a priority:

| Aspect | Details |
|--------|---------|
| **Struct-based** | `Option<T>`, `Result<T,E>`, `Try<T>`, etc. are `readonly struct` — no heap allocations |
| **No boxing** | Generic implementations avoid boxing value types |
| **Lazy evaluation** | `UnwrapOrElse`, `OrElse` use `Func<>` for deferred computation |
| **Zero allocations** | Most operations on value types are allocation-free |
| **Aggressive inlining** | Hot paths use `[MethodImpl(AggressiveInlining)]` |
| **ConfigureAwait(false)** | All async methods use `ConfigureAwait(false)` |

For typical use cases, the overhead is negligible (nanoseconds). The safety guarantees and code clarity typically outweigh any micro-optimization concerns.

---

## Resources

Want to dive deeper into functional programming and these patterns?

### Books

| Book | Author | Why Read It |
|------|--------|-------------|
| [Functional Programming in C#](https://www.manning.com/books/functional-programming-in-c-sharp-second-edition) | Enrico Buonanno | The definitive guide to FP in C#. Covers Option, Either, validation, and more. |
| [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) | Scott Wlaschin | Uses F# but concepts translate directly. Excellent on making illegal states unrepresentable. |
| [Programming Rust](https://www.oreilly.com/library/view/programming-rust-2nd/9781492052586/) | Blandy, Orendorff, Tindall | Rust's `Option` and `Result` are nearly identical to Monad.NET's versions. |

### Online Resources

| Resource | Description |
|----------|-------------|
| [F# for Fun and Profit](https://fsharpforfunandprofit.com/) | Scott Wlaschin's legendary site. Start with [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/). |
| [Rust Error Handling](https://doc.rust-lang.org/book/ch09-00-error-handling.html) | Official Rust book chapter on `Option` and `Result`. |
| [Haskell Maybe/Either](https://wiki.haskell.org/Handling_errors_in_Haskell) | Haskell wiki on error handling patterns. |
| [Parse, Don't Validate](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/) | Alexis King's influential post on type-driven design. |

### Videos & Talks

| Talk | Speaker | Topics |
|------|---------|--------|
| [Functional Design Patterns](https://www.youtube.com/watch?v=srQt1NAHYC0) | Scott Wlaschin | Monads, Railway Oriented Programming, composition |
| [Domain Modeling Made Functional](https://www.youtube.com/watch?v=2JB1_e5wZmU) | Scott Wlaschin | Making illegal states unrepresentable |
| [The Power of Composition](https://www.youtube.com/watch?v=vDe-4o8Uwl8) | Scott Wlaschin | Why small, composable functions matter |

### Related C# Libraries

| Library | Description |
|---------|-------------|
| [language-ext](https://github.com/louthy/language-ext) | Extensive FP library for C#. More features than Monad.NET but steeper learning curve. |
| [OneOf](https://github.com/mcintyre321/OneOf) | Focused on discriminated unions. Lighter weight. |
| [FluentResults](https://github.com/altmann/FluentResults) | Result pattern with fluent API. Good for simple use cases. |
| [ErrorOr](https://github.com/amantinband/error-or) | Discriminated union for errors. Popular in Clean Architecture circles. |

### Key Concepts

1. **Railway Oriented Programming** — Treat errors as alternate tracks, not exceptions
2. **Making Illegal States Unrepresentable** — Use types to prevent bugs at compile time
3. **Parse, Don't Validate** — Push validation to the boundaries, work with valid types internally
4. **Composition over Inheritance** — Small, focused types that combine well

---

## When NOT to Use Monad.NET

Monad.NET is not always the right choice. Here's when to stick with native C#:

| Scenario | Recommendation |
|----------|----------------|
| Simple null checks | Use `??`, `?.`, and nullable reference types |
| Exceptional failures (IO, network) | Use exceptions — they're designed for this |
| Performance-critical hot loops | Avoid lambda allocations; use traditional control flow |
| Team unfamiliar with FP concepts | Consider the learning curve before adoption |
| Simple CRUD operations | Often overkill; use when composition benefits outweigh complexity |

**Good rule of thumb:** If you're writing `if (x != null) { ... }` once, use nullable. If you're chaining multiple such checks, use `Option`.

---

## FAQ

### Can I use Monad.NET with Entity Framework?

Yes! Use `Option<T>` for optional relationships and `Result<T, E>` for operations that might fail. See [EF Core Integration](docs/Integrations.md#entity-framework-core-integration).

### Can I use Monad.NET with ASP.NET Core?

Absolutely. See [ASP.NET Core Integration](docs/Integrations.md#aspnet-core-integration).

### What's the difference between `Result` and `Either`?

- **`Result<T, E>`** — Semantically means success or failure. Right-biased (operations work on `Ok`).
- **`Either<L, R>`** — General "one of two types" with no success/failure implication. Can work on either side.

Use `Result` for error handling. Use `Either` when both sides are valid outcomes.

### What's the difference between `Result` and `Validation`?

- **`Result`** — Short-circuits on first error (like `&&`)
- **`Validation`** — Accumulates ALL errors (for showing multiple validation messages)

### Is Monad.NET thread-safe?

Yes. All types are immutable `readonly struct` with no shared mutable state.

---

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

**Development requirements:**
- .NET 8.0 SDK or later (for building all targets)
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

[Documentation](docs/QUICKSTART.md) · [NuGet](https://www.nuget.org/packages/Monad.NET/) · [Issues](https://github.com/behrangmohseni/Monad.NET/issues)
