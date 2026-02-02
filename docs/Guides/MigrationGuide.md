# Migration Guide

This guide helps you migrate from other functional programming libraries to Monad.NET.

## Table of Contents

- [Migrating from language-ext](#migrating-from-language-ext)
- [Migrating from OneOf](#migrating-from-oneof)
- [Migrating from FluentResults](#migrating-from-fluentresults)
- [Migrating from ErrorOr](#migrating-from-erroror)
- [General Migration Tips](#general-migration-tips)

---

## Migrating from language-ext

[language-ext](https://github.com/louthy/language-ext) is a comprehensive functional programming library for C#. Monad.NET offers a lighter-weight alternative with a focus on simplicity and ease of adoption.

### Why Migrate?

| Aspect | language-ext | Monad.NET |
|--------|--------------|-----------|
| **Learning curve** | Steep — many concepts and overloads | Gentle — focused API surface |
| **Dependencies** | External dependencies | Zero on .NET 6+; polyfills on netstandard2.x |
| **API complexity** | Comprehensive but complex | Focused and pragmatic |
| **Naming** | Haskell-inspired (`Seq`, `Lst`, `Arr`) | C#-friendly (`IReadOnlyList`, etc.) |
| **Performance** | Good with custom collections | Struct-based, zero allocations |

### Type Mappings

| language-ext | Monad.NET | Notes |
|--------------|-----------|-------|
| `Option<A>` | `Option<T>` | Nearly identical API |
| `Either<L, R>` | `Result<T, E>` or `[Union]` | Use Result for error handling, [Union] for discriminated unions |
| `Try<A>` | `Try<T>` | Same concept, different API style |
| `Validation<FAIL, SUCCESS>` | `Validation<T, E>` | Same concept, simplified API |
| `Seq<A>` | `IEnumerable<T>` | Use standard .NET collections |
| `Lst<A>` | `IReadOnlyList<T>` | Use standard .NET collections |
| `Arr<A>` | `T[]` / `ImmutableArray<T>` | Use standard .NET types |
| `NonEmptyList<A>` | `NonEmptyList<T>` | Direct equivalent |
| `Reader<Env, A>` | `Reader<R, A>` | Similar API |
| `Writer<Out, A>` | `Writer<W, T>` | Similar API |
| `State<S, A>` | `State<S, A>` | Similar API |
| `IO<A>` | `IO<T>` | Similar concept |

### API Mapping

#### Option

```csharp
// language-ext
using LanguageExt;
using static LanguageExt.Prelude;

Option<int> opt = Some(42);
Option<int> none = None;
var result = opt.Match(Some: x => x.ToString(), None: () => "empty");
var mapped = opt.Map(x => x * 2);
var bound = opt.Bind(x => Some(x + 1));
var filtered = opt.Filter(x => x > 0);
var value = opt.IfNone(0);
var value2 = opt.IfNone(() => ComputeDefault());

// Monad.NET
using Monad.NET;

var opt = Option<int>.Some(42);
var none = Option<int>.None();
var result = opt.Match(some: x => x.ToString(), none: () => "empty");
var mapped = opt.Map(x => x * 2);
var bound = opt.Bind(x => Option<int>.Some(x + 1));
var filtered = opt.Filter(x => x > 0);
var value = opt.GetValueOr(0);
var value2 = opt.Match(x => x, () => ComputeDefault()); // Lazy evaluation
```

#### Either → Result

```csharp
// language-ext
Either<Error, User> result = Right<Error, User>(user);
result.Match(Left: err => HandleError(err), Right: user => Process(user));
result.Map(u => u.Name);          // Maps right
result.MapLeft(e => e.Message);   // Maps left
result.Bind(u => GetProfile(u));

// Monad.NET - Use Result<T, E> for error handling scenarios
var result = Result<User, Error>.Ok(user);
result.Match(ok: user => Process(user), err: err => HandleError(err));
result.Map(u => u.Name);           // Maps ok value
result.MapError(e => e.Message);   // Maps error
result.Bind(u => GetProfile(u));
```

#### Try

```csharp
// language-ext
using static LanguageExt.Prelude;

Try<int> t = Try(() => int.Parse("42"));
var result = t.Match(Succ: x => x, Fail: ex => -1);
t.Map(x => x * 2);
t.Bind(x => Try(() => x / 2));

// Monad.NET
var t = Try<int>.Of(() => int.Parse("42"));
var result = t.Match(success: x => x, failure: ex => -1);
t.Map(x => x * 2);
t.Bind(x => Try<int>.Of(() => x / 2));
```

#### Validation

```csharp
// language-ext
using LanguageExt;
using static LanguageExt.Prelude;

Validation<Error, string> ValidateName(string name) =>
    string.IsNullOrEmpty(name) 
        ? Fail<Error, string>(new Error("Name required"))
        : Success<Error, string>(name);

var result = (ValidateName(n), ValidateEmail(e), ValidateAge(a))
    .Apply((name, email, age) => new User(name, email, age));

// Monad.NET
Validation<string, Error> ValidateName(string name) =>
    string.IsNullOrEmpty(name)
        ? Validation<string, Error>.Error(new Error("Name required"))
        : Validation<string, Error>.Ok(name);

var result = ValidateName(n)
    .Apply(ValidateEmail(e), (name, email) => (name, email))
    .Apply(ValidateAge(a), (partial, age) => new User(partial.name, partial.email, age));
```

### Method Chain Comparison

```csharp
// language-ext uses LINQ query syntax
var result = from user in GetUser(id)
             from profile in GetProfile(user.Id)
             from address in GetAddress(profile.AddressId)
             select new UserDetails(user, profile, address);

// Monad.NET uses Map/Bind chains
var result = GetUser(id)
    .Bind(user => GetProfile(user.Id)
        .Bind(profile => GetAddress(profile.AddressId)
            .Map(address => new UserDetails(user, profile, address))));
```

> **Note:** Monad.NET removed LINQ support to avoid semantic confusion (especially with Validation where SelectMany short-circuits). Use `Map`/`Bind` chains instead.

### Key Differences to Note

1. **Type parameter order in Validation**: language-ext uses `Validation<FAIL, SUCCESS>`, Monad.NET uses `Validation<T, E>` (success type first)

2. **Naming conventions**:
   - `Bind` → `Bind` (same in Monad.NET v2.0)
   - `IfNone` → `GetValueOr` or `Match()` for lazy evaluation
   - `Match(Some:, None:)` → `Match(some:, none:)`

3. **No Prelude class**: Monad.NET doesn't use static imports like `Some(42)`. Instead use `Option<int>.Some(42)` or extension methods like `42.ToOption()`.

4. **Collections**: Monad.NET uses standard .NET collections instead of custom types like `Seq`, `Lst`, `Arr`.

---

## Migrating from OneOf

[OneOf](https://github.com/mcintyre321/OneOf) provides discriminated unions for C#.

### Type Mappings

| OneOf | Monad.NET |
|-------|-----------|
| `OneOf<T0, T1>` | `[Union]` attribute |
| `OneOf<T0, T1, T2>` | `[Union]` attribute for 3+ cases |

### Migration Examples

```csharp
// OneOf
public OneOf<User, NotFound, ValidationError> GetUser(int id)
{
    // ...
}

result.Switch(
    user => HandleUser(user),
    notFound => HandleNotFound(),
    error => HandleError(error)
);

// Monad.NET with Union attribute
[Union]
public abstract partial record GetUserResult
{
    public partial record Success(User User) : GetUserResult;
    public partial record NotFound : GetUserResult;
    public partial record ValidationError(string Message) : GetUserResult;
}

public GetUserResult GetUser(int id)
{
    // ...
}

result.Match(
    success: s => HandleUser(s.User),
    notFound: _ => HandleNotFound(),
    validationError: e => HandleError(e)
);
```

### Benefits of Migration

1. **Named cases**: Instead of positional types, you have named cases with clear semantics
2. **Data in cases**: Each case can carry different data types
3. **Exhaustive matching**: Compiler ensures all cases are handled
4. **Type safety**: No index-based access that could go wrong

---

## Migrating from FluentResults

[FluentResults](https://github.com/altmann/FluentResults) provides a Result pattern implementation.

### Type Mappings

| FluentResults | Monad.NET |
|---------------|-----------|
| `Result<T>` | `Result<T, E>` or `Try<T>` |
| `Result` (no value) | `Result<Unit, E>` |
| `IError` | Your error type `E` |
| `IReason` | N/A (use error type directly) |

### Migration Examples

```csharp
// FluentResults
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return Result.Fail<User>("Invalid ID");
    
    var user = _db.Find(id);
    if (user == null)
        return Result.Fail<User>("User not found");
    
    return Result.Ok(user);
}

var result = GetUser(id);
if (result.IsOk)
    Console.WriteLine(result.Value.Name);
else
    Console.WriteLine(result.Errors.First().Message);

// Monad.NET
public Result<User, UserError> GetUser(int id)
{
    if (id <= 0)
        return Result<User, UserError>.Error(UserError.InvalidId);
    
    var user = _db.Find(id);
    if (user == null)
        return Result<User, UserError>.Error(UserError.NotFound);
    
    return Result<User, UserError>.Ok(user);
}

var result = GetUser(id);
result.Match(
    ok: user => Console.WriteLine(user.Name),
    err: error => Console.WriteLine(error.ToString())
);

// Or use the familiar pattern
if (result.TryGet(out var user))
    Console.WriteLine(user.Name);
else if (result.TryGetError(out var error))
    Console.WriteLine(error.ToString());
```

### FluentResults Chaining

```csharp
// FluentResults
var result = GetUser(id)
    .Bind(user => GetProfile(user.Id))
    .Bind(profile => ValidateProfile(profile));

// Monad.NET
var result = GetUser(id)
    .Bind(user => GetProfile(user.Id))
    .Bind(profile => ValidateProfile(profile));
```

---

## Migrating from ErrorOr

[ErrorOr](https://github.com/amantinband/error-or) provides a discriminated union for error handling.

### Type Mappings

| ErrorOr | Monad.NET |
|---------|-----------|
| `ErrorOr<T>` | `Result<T, E>` or `Validation<T, E>` |
| `Error` | Your error type `E` |
| `IError` | Your error type `E` |

### Migration Examples

```csharp
// ErrorOr
public ErrorOr<User> CreateUser(CreateUserRequest request)
{
    if (string.IsNullOrEmpty(request.Name))
        return Error.Validation("Name is required");
    
    return new User(request.Name);
}

result.Match(
    value => HandleSuccess(value),
    errors => HandleErrors(errors)
);

// Monad.NET with Result (for single errors)
public Result<User, UserError> CreateUser(CreateUserRequest request)
{
    if (string.IsNullOrEmpty(request.Name))
        return Result<User, UserError>.Error(new UserError("Name is required"));
    
    return Result<User, UserError>.Ok(new User(request.Name));
}

result.Match(
    ok: user => HandleSuccess(user),
    err: error => HandleError(error)
);

// Monad.NET with Validation (for multiple errors)
public Validation<User, UserError> CreateUser(CreateUserRequest request)
{
    return ValidateName(request.Name)
        .Apply(ValidateEmail(request.Email), (name, email) => (name, email))
        .Map(t => new User(t.name, t.email));
}

result.Match(
    valid: user => HandleSuccess(user),
    invalid: errors => HandleErrors(errors)  // Gets ALL errors
);
```

---

## General Migration Tips

### 1. Start with the Most-Used Types

Begin by migrating `Option` and `Result` usages first. These are the most common and easiest to understand.

### 2. Use Find & Replace Carefully

Some common replacements:
```
Bind(          →  Bind(
IfNone(        →  GetValueOr( or Match(x => x, () =>
Match(Some:    →  Match(some:
Match(None:    →  Match(none:
Match(Succ:    →  Match(success:
Match(Fail:    →  Match(failure:
```

### 3. Convert LINQ to Map/Bind

Monad.NET does not support LINQ query syntax. Convert your LINQ queries to `Map`/`Bind` chains:

```csharp
// Before (LINQ)
var result = from x in option1
             from y in option2
             select x + y;

// After (Map/Bind)
var result = option1.Bind(x => option2.Map(y => x + y));
```

### 4. Migrate Error Types

Create domain-specific error types instead of using strings or generic error types:

```csharp
// Instead of Result<User, string>
public record UserError(string Code, string Message);
public Result<User, UserError> GetUser(int id);

// Or use an enum
public enum OrderError { NotFound, InvalidState, PaymentFailed }
public Result<Order, OrderError> ProcessOrder(OrderRequest request);

// Or use a union type
[Union]
public abstract partial record ApiError
{
    public partial record NotFound(string Resource) : ApiError;
    public partial record ValidationFailed(IReadOnlyList<string> Errors) : ApiError;
    public partial record Unauthorized : ApiError;
}
```

### 5. Gradual Migration

You can use both libraries side-by-side during migration:

```csharp
// Convert language-ext Option to Monad.NET
LanguageExt.Option<T> langExtOption = ...;
Monad.NET.Option<T> monadOption = langExtOption.Match(
    Some: x => Monad.NET.Option<T>.Some(x),
    None: () => Monad.NET.Option<T>.None()
);
```

### 6. Test Coverage

Ensure you have good test coverage before migrating. The behavior should remain identical:

```csharp
[Fact]
public void Migration_BehaviorPreserved()
{
    // Old code
    var oldResult = OldGetUser(id).Match(
        Some: u => u.Name,
        None: () => "Unknown"
    );
    
    // New code
    var newResult = NewGetUser(id).Match(
        some: u => u.Name,
        none: () => "Unknown"
    );
    
    Assert.Equal(oldResult, newResult);
}
```

---

## Need Help?

- Check the [Core Types](../CoreTypes.md) documentation for detailed API information
- See [Examples](../Examples.md) for real-world usage patterns
- Open an [issue](https://github.com/behrangmohseni/Monad.NET/issues) if you encounter migration problems

---

[← Back to Documentation](../../README.md) | [Pitfalls & Gotchas →](Pitfalls.md)

