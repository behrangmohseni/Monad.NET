# Option Explained

> **For C# developers:** This guide explains `Option<T>` and how it improves upon nullable reference types.

## The Billion Dollar Mistake

Tony Hoare, who invented null references, famously called them his "billion dollar mistake." Every C# developer knows the pain:

```csharp
var user = GetUser(42);
Console.WriteLine(user.Name); // 💥 NullReferenceException
```

You've written defensive code like this thousands of times:

```csharp
if (user != null)
{
    if (user.Address != null)
    {
        if (user.Address.City != null)
        {
            Console.WriteLine(user.Address.City.ToUpper());
        }
    }
}
```

This is tedious, error-prone, and clutters your logic.

---

## What is Option?

`Option<T>` is a type that explicitly represents "a value that might not exist."

```csharp
Option<User> maybeUser = FindUser(42);
```

The type signature tells you: "This might be a User, or it might be nothing."

### Two States

An `Option<T>` is always in exactly one of two states:

| State | Meaning | Created With |
|-------|---------|--------------|
| **Some** | Contains a value | `Option<T>.Some(value)` |
| **None** | Empty, no value | `Option<T>.None()` |

```csharp
Option<int> hasValue = Option<int>.Some(42);  // Contains 42
Option<int> isEmpty = Option<int>.None();      // Empty
```

---

## Option vs Nullable Reference Types

C# 8 introduced nullable reference types (`string?`). How does `Option<T>` compare?

### Nullable Reference Types

```csharp
User? user = GetUser(42);

// Compiler warns, but doesn't prevent
string name = user.Name;  // Warning: possible null

// You can still ignore warnings
#nullable disable
string name = user.Name;  // No warning, still crashes 💥
```

**Problems:**
- Warnings can be suppressed
- Doesn't work with value types (`int` vs `int?` is different)
- No built-in composition methods
- `null` can sneak through via reflection, legacy code, etc.

### Option<T>

```csharp
Option<User> user = GetUser(42);

// This won't compile - Option<User> is not User
string name = user.Name;  // ❌ Compile error

// You must explicitly handle both cases
string name = user.Match(
    someFunc: u => u.Name,
    noneFunc: () => "Unknown"
);
```

**Benefits:**
- Impossible to accidentally use without handling
- Works uniformly for value and reference types
- Rich composition methods (`Map`, `Bind`, `Filter`, etc.)
- No null anywhere - `Option.None()` is a real value

---

## Basic Operations

### Creating Options

```csharp
// Explicit creation
var some = Option<int>.Some(42);
var none = Option<int>.None();

// From nullable values
string? nullableString = GetNullableString();
Option<string> option = nullableString.ToOption();  // Some or None

// Safe creation (null becomes None)
Option<string> safe = Option<string>.Some(nullableString!); // Throws if null
```

### Checking State

```csharp
Option<int> option = GetOption();

if (option.IsSome)
{
    int value = option.GetValue();  // Safe here
}

// Or use TryGet pattern
if (option.TryGet(out var value))
{
    Console.WriteLine(value);
}
```

### Getting Values

```csharp
Option<int> option = Option<int>.Some(42);

// Safe extraction
int value = option.GetValueOr(0);           // 42, or 0 if None
int lazy = option.GetValueOrElse(() => ComputeDefault()); // Lazy evaluation

// Unsafe (throws if None)
int risky = option.GetValue();     // 42, or throws InvalidOperationException
int alsoRisky = option.GetOrThrow(); // Same behavior
```

---

## Transforming Options

### Map: Transform the Inner Value

`Map` applies a function to the value **if it exists**:

```csharp
Option<string> name = Option<string>.Some("alice");

Option<string> upper = name.Map(s => s.ToUpper());  
// Some("ALICE")

Option<string> none = Option<string>.None();
Option<string> stillNone = none.Map(s => s.ToUpper());  
// None - function not called
```

**Use Map when:** Your transformation can't fail.

### Bind: Chain Option-Returning Operations

`Bind` chains operations where **each step might return None**:

```csharp
Option<User> GetUser(int id) { ... }
Option<Address> GetAddress(User user) { ... }
Option<string> GetCity(Address addr) { ... }

// Chain them together
Option<string> city = GetUser(42)
    .Bind(user => GetAddress(user))
    .Bind(addr => GetCity(addr));
```

If any step returns `None`, the whole chain returns `None`.

**Use Bind when:** Your transformation might also return None.

### Filter: Conditional None

`Filter` converts `Some` to `None` if a condition isn't met:

```csharp
Option<int> number = Option<int>.Some(42);

Option<int> big = number.Filter(n => n > 100);  // None (42 is not > 100)
Option<int> positive = number.Filter(n => n > 0);  // Some(42)
```

---

## Practical Examples

### Safe Navigation

**Before (null checks everywhere):**
```csharp
string GetUserCity(int userId)
{
    var user = _db.FindUser(userId);
    if (user == null) return "Unknown";
    
    var address = user.Address;
    if (address == null) return "Unknown";
    
    var city = address.City;
    if (city == null) return "Unknown";
    
    return city;
}
```

**After (Option chaining):**
```csharp
string GetUserCity(int userId)
{
    return FindUser(userId)
        .Bind(user => user.Address.ToOption())
        .Bind(addr => addr.City.ToOption())
        .GetValueOr("Unknown");
}
```

### Dictionary Lookup

**Before:**
```csharp
if (_cache.TryGetValue(key, out var value))
{
    return ProcessValue(value);
}
return defaultValue;
```

**After:**
```csharp
return _cache.TryGetValue(key, out var value)
    ? Option<T>.Some(value)
    : Option<T>.None()
    .Map(ProcessValue)
    .GetValueOr(defaultValue);
```

### Configuration with Fallbacks

```csharp
string GetConnectionString()
{
    return GetEnvVar("DB_CONNECTION")
        .OrElse(() => GetConfigFile("connectionString"))
        .OrElse(() => Option<string>.Some("Server=localhost;Database=dev"))
        .GetValue();
}
```

### First Match from Multiple Sources

```csharp
Option<User> FindUser(string email)
{
    return FindInCache(email)
        .OrElse(() => FindInDatabase(email))
        .OrElse(() => FindInLegacySystem(email));
}
```

---

## Pattern Matching

### Match: Handle Both Cases

```csharp
Option<int> option = GetOption();

// With return value
string message = option.Match(
    someFunc: value => $"Got: {value}",
    noneFunc: () => "Nothing here"
);

// Side effects only
option.Match(
    someAction: value => Console.WriteLine($"Got: {value}"),
    noneAction: () => Console.WriteLine("Nothing here")
);
```

### Switch Expression (C# 8+)

```csharp
var result = option switch
{
    { IsSome: true } o => $"Got: {o.GetValue()}",
    _ => "Nothing here"
};
```

---

## Common Patterns

### Converting Collections

```csharp
// First element as Option
Option<int> first = numbers.FirstOrNone();

// Single element (None if 0 or 2+ elements)
Option<int> single = numbers.SingleOrNone();

// Filter then get first
Option<User> admin = users
    .Where(u => u.IsAdmin)
    .FirstOrNone();
```

### Aggregating Options

```csharp
// If any is None, result is None
Option<int> a = Option<int>.Some(1);
Option<int> b = Option<int>.Some(2);
Option<int> c = Option<int>.Some(3);

Option<int> sum = a
    .Bind(av => b.Map(bv => av + bv))
    .Bind(ab => c.Map(cv => ab + cv));
// Some(6)
```

---

## When to Use Option vs Nullable

| Use Option<T> When | Use T? When |
|-------------------|-------------|
| Building domain models | Simple DTOs/POCOs |
| Chaining operations | One-off null checks |
| APIs you control | Interop with existing code |
| Need Filter/Map/Bind | Just need null coalescing |
| Teaching intent matters | Performance critical paths |

---

## Quick Reference

| Operation | Purpose | Example |
|-----------|---------|---------|
| `Some(value)` | Create with value | `Option<int>.Some(42)` |
| `None()` | Create empty | `Option<int>.None()` |
| `IsSome` / `IsNone` | Check state | `if (opt.IsSome)` |
| `GetValueOr(default)` | Extract with fallback | `opt.GetValueOr(0)` |
| `Map(f)` | Transform value | `opt.Map(x => x * 2)` |
| `Bind(f)` | Chain Option functions | `opt.Bind(x => TryParse(x))` |
| `Filter(predicate)` | Conditional None | `opt.Filter(x => x > 0)` |
| `Match(some, none)` | Handle both cases | `opt.Match(v => ..., () => ...)` |
| `OrElse(alternative)` | Fallback Option | `opt.OrElse(() => other)` |
| `ToOption()` | From nullable | `nullableValue.ToOption()` |

---

## What's Next?

- **[Result Explained](ResultExplained.md)** - When you need to know *why* something failed
- **[Railway-Oriented Programming](RailwayOrientedProgramming.md)** - The mental model
- **[Composition Patterns](CompositionPatterns.md)** - Advanced techniques
