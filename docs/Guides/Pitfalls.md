# Pitfalls & Gotchas

This guide covers common mistakes and how to avoid them.

## Table of Contents

- [Struct Default Values (Critical)](#struct-default-values-critical)
- [Validation vs Result](#validation-vs-result)
- [RemoteData State Guards](#remotedata-state-guards)
- [Try Wrapping](#try-wrapping)
- [Option.Some(null)](#optionsomenull)
- [Writer Performance](#writer-performance)
- [Async Patterns](#async-patterns)

---

## Struct Default Values (Critical)

### The Problem

All Monad.NET types are `readonly struct` for performance. However, C# allows creating structs via `default(T)`, which bypasses constructors:

```csharp
// These create INVALID instances:
Result<int, string> invalid1 = default;
Result<int, string> invalid2 = new Result<int, string>[10][0]; // Array elements are default
Result<int, string> invalid3;  // Uninitialized field (if not assigned before use)

// Throws InvalidOperationException on ANY operation:
invalid1.IsOk;      // THROWS
invalid1.GetValue(); // THROWS
invalid1.Match(...); // THROWS
```

### Why This Happens

When you use `default(Result<T,E>)`:
- `_isOk` is `false` (bool default)
- `_value` is `default(T)` 
- `_error` is `default(E)` (often `null`)

This creates an ambiguous state that looks like an error but has no valid error value. Rather than silently return garbage, v2.0 throws to prevent bugs.

### Types Affected

| Type | `default(T)` Behavior |
|------|----------------------|
| `Option<T>` | Safe — treated as `None` |
| `Result<T,E>` | **Throws** on any operation |
| `Validation<T,E>` | **Throws** on any operation |
| `Try<T>` | **Throws** on any operation |
| `RemoteData<T,E>` | Safe — treated as `NotAsked` |

### How to Avoid

```csharp
// ❌ BAD: Default values
Result<int, string> result = default;
var results = new Result<int, string>[10]; // All defaults!

// ✅ GOOD: Factory methods
Result<int, string> result = Result<int, string>.Ok(42);
Result<int, string> error = Result<int, string>.Error("failed");

// ✅ GOOD: Initialize arrays explicitly
var results = Enumerable.Range(0, 10)
    .Select(_ => Result<int, string>.Ok(0))
    .ToArray();

// ✅ GOOD: Use List<T> and add items
var results = new List<Result<int, string>>();
results.Add(Result<int, string>.Ok(42));
```

### Detection with Analyzers

Install `Monad.NET.Analyzers` to catch these at compile time:

```bash
dotnet add package Monad.NET.Analyzers
```

The analyzer will warn on:
- `default(Result<T,E>)` expressions
- Uninitialized `Result<T,E>` fields
- Array creation of Result types

---

## Validation vs Result

### The Trap

`Validation<T,E>.Bind()` **short-circuits on first error** — just like `Result`:

```csharp
// ❌ This stops at first error!
var user = ValidateName(name)
    .Bind(n => ValidateEmail(email).Map(e => (n, e)))
    .Bind(pair => ValidateAge(age).Map(a => new User(pair.n, pair.e, a)));
// If name is invalid, email and age are never validated
```

### The Solution

Use `Apply`, `Zip`, or `Combine` to accumulate errors:

```csharp
// ✅ Accumulates ALL errors
var user = ValidateName(name)
    .Apply(ValidateEmail(email), (n, e) => (n, e))
    .Apply(ValidateAge(age), (pair, a) => new User(pair.n, pair.e, a));
// All three validations run; all errors collected
```

### When to Use Which

| Method | Behavior | Use When |
|--------|----------|----------|
| `Bind` | Short-circuits | Later validation depends on earlier results |
| `Apply`/`Zip` | Accumulates | Independent validations; show all errors |
| `Combine` | Accumulates | Combining a collection of validations |

---

## RemoteData State Guards

### The Problem

`RemoteData<T,E>` has four states, but `GetValue()` only works for `Success`:

```csharp
RemoteData<User, Error> data = RemoteData<User, Error>.Loading();

// ❌ THROWS — not in Success state
var user = data.GetValue();

// ❌ THROWS — IsSuccess is false
var user = data.GetValueOr(defaultUser); // Still throws for NotAsked/Loading
```

### The Solution

Use `Match` for exhaustive handling:

```csharp
// ✅ Handles all four states
var display = data.Match(
    notAsked: () => "Click to load",
    loading: () => "Loading...",
    success: user => $"Hello, {user.Name}",
    failure: error => $"Error: {error.Message}"
);

// ✅ Convert to Result first if you only care about Success/Failure
var result = data.ToResult(
    notAskedError: new Error("Data not requested"),
    loadingError: new Error("Still loading")
);
```

---

## Try Wrapping

### The Trap

`Try<T>` captures exceptions **as values**. They won't propagate unless you explicitly rethrow:

```csharp
var result = Try<int>.Of(() => int.Parse("not a number"));
// Exception is captured, NOT thrown

// ❌ This silently swallows the exception
var value = result.GetValueOr(0);
```

### When to Rethrow

```csharp
// ✅ Explicit rethrow when needed
var value = result.GetOrThrow(); // Throws FormatException

// ✅ Convert to Result for typed error handling
var asResult = result.ToResult(ex => new ParseError(ex.Message));
```

---

## Option.Some(null)

### The Trap

`Option<T>.Some(null)` throws `ArgumentNullException`:

```csharp
string? name = null;

// ❌ THROWS ArgumentNullException
var option = Option<string>.Some(name!);
```

### The Solution

```csharp
// ✅ Use None() for absence
var option = name is null ? Option<string>.None() : Option<string>.Some(name);

// ✅ Or use the extension method (handles null safely)
var option = name.ToOption(); // None if null, Some if not null

// ✅ Implicit conversion also handles null
Option<string> option = name; // None if null
```

---

## Writer Performance

### The Trap

`Writer<string, T>` concatenates strings, which is O(n²) for many operations:

```csharp
// ❌ SLOW — O(n²) string concatenation
var result = Writer<string, int>.Tell(1, "step 1")
    .Bind(x => Writer<string, int>.Tell(x + 1, "step 2"), (a, b) => a + b)
    .Bind(x => Writer<string, int>.Tell(x + 1, "step 3"), (a, b) => a + b)
    // ... hundreds more steps
```

### The Solution

```csharp
// ✅ Use List<T> for better performance
var result = Writer<List<string>, int>.Tell(1, new List<string> { "step 1" })
    .Bind(
        x => Writer<List<string>, int>.Tell(x + 1, new List<string> { "step 2" }),
        (a, b) => a.Concat(b).ToList()
    );
```

---

## Async Patterns

### The Trap

v2.0 removed async extensions from `Option<T>` and `Result<T,E>`. This code no longer works:

```csharp
// ❌ DOES NOT COMPILE in v2.0
var result = await option.MapAsync(async x => await ProcessAsync(x));
```

### The Solution

Use standard `await` inside `Map`/`Bind`, or use `Match`:

```csharp
// ✅ Await inside Map (if the lambda returns Task)
// Note: This requires the Map to understand Task<T> — use Match instead

// ✅ RECOMMENDED: Use Match for async operations
var result = await option.Match(
    some: async x => Option<Result>.Some(await ProcessAsync(x)),
    none: () => Task.FromResult(Option<Result>.None())
);

// ✅ Or handle explicitly
if (option.TryGet(out var value))
{
    var processed = await ProcessAsync(value);
    // ...
}
```

See [Async Patterns Guide](AsyncPatterns.md) for comprehensive async guidance.

---

## Quick Reference

| Pitfall | Solution |
|---------|----------|
| `default(Result<T,E>)` | Use factory methods: `Result.Ok()`, `Result.Error()` |
| `Validation.Bind` loses errors | Use `Apply`, `Zip`, or `Combine` |
| `RemoteData.GetValue()` throws | Use `Match` for all four states |
| `Try` swallows exceptions | Use `GetOrThrow()` or `ToResult()` |
| `Option.Some(null)` throws | Use `.ToOption()` or `None()` |
| `Writer<string>` is slow | Use `Writer<List<string>>` |
| No `MapAsync` on Option/Result | Use `Match` with async lambdas |

---

[← Back to Guides](../README.md) | [Type Selection Guide →](TypeSelectionGuide.md)
