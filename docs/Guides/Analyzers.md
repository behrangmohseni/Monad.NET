# Monad.NET Analyzer Rules

This document describes the Roslyn analyzers included with Monad.NET. These analyzers help you use the library correctly and idiomatically.

## Installation

The analyzers are automatically included when you reference `Monad.NET.Analyzers`:

```bash
dotnet add package Monad.NET.Analyzers
```

## Rules

### MNT001: Unchecked GetValue call

**Severity:** Warning  
**Default:** Enabled

Calling `GetValue()`, `GetOrThrow()`, or similar methods without first checking the monad's state can throw exceptions at runtime.

**Bad:**
```csharp
var option = GetUser(id);
var user = option.GetValue(); // May throw!
```

**Good:**
```csharp
var option = GetUser(id);
var name = option.Match(
    some: u => u.Name,
    none: () => "Unknown"
);

// Or check first
if (option.IsSome)
{
    var user = option.GetValue();
}
```

---

### MNT002: Redundant Map chain

**Severity:** Info  
**Default:** Enabled

Consecutive `Map` calls can be combined into a single call for better readability.

**Bad:**
```csharp
option.Map(x => x.ToUpper()).Map(x => x.Trim());
```

**Good:**
```csharp
option.Map(x => x.ToUpper().Trim());
```

---

### MNT003: Map followed by GetValueOr can be simplified

**Severity:** Info  
**Default:** Enabled

Using `Map` followed by `GetValueOr` can be simplified to `MapOr`.

**Bad:**
```csharp
option.Map(x => x * 2).GetValueOr(0);
```

**Good:**
```csharp
option.MapOr(0, x => x * 2);
```

---

### MNT004: Bind can be simplified to Map

**Severity:** Info  
**Default:** Enabled

When `Bind` always wraps its result in `Some`/`Ok`, use `Map` instead.

**Bad:**
```csharp
option.Bind(x => Option<int>.Some(x * 2));
```

**Good:**
```csharp
option.Map(x => x * 2);
```

---

### MNT005: Discarded monad value

**Severity:** Warning  
**Default:** Enabled

Discarding a `Result` or `Option` value means errors or missing values will be silently ignored.

**Bad:**
```csharp
_ = ProcessOrder(order); // Result discarded!
SaveUser(user);          // Result ignored as statement
```

**Good:**
```csharp
var result = ProcessOrder(order);
result.Match(
    ok: _ => Log.Info("Success"),
    err: e => Log.Error(e)
);
```

---

### MNT007: Prefer Match over manual state checks

**Severity:** Info  
**Default:** Enabled

Using `Match()` is more idiomatic than checking `IsSome`/`IsOk` and then accessing values.

**Bad:**
```csharp
if (option.IsSome)
{
    Console.WriteLine(option.GetValue());
}
else
{
    Console.WriteLine("None");
}
```

**Good:**
```csharp
option.Match(
    some: x => Console.WriteLine(x),
    none: () => Console.WriteLine("None")
);
```

---

### MNT008: Async monad operation missing ConfigureAwait(false)

**Severity:** Info  
**Default:** Disabled

In library code, async operations should use `ConfigureAwait(false)` to avoid deadlocks.

Enable this rule in `.editorconfig`:
```ini
[*.cs]
dotnet_diagnostic.MNT008.severity = suggestion
```

---

### MNT009: Option.Some() may receive null

**Severity:** Warning  
**Default:** Enabled

`Option.Some(value)` throws if `value` is null. Use `ToOption()` for potentially null values.

**Bad:**
```csharp
var user = GetUser(); // May return null
var option = Option<User>.Some(user); // Throws if null!
```

**Good:**
```csharp
var user = GetUser();
var option = user.ToOption(); // null → None, value → Some
```

---

### MNT010: Empty Match branch

**Severity:** Info  
**Default:** Enabled

If one branch of `Match` is empty, consider using `Tap` instead.

**Bad:**
```csharp
option.Match(
    some: x => Console.WriteLine(x),
    none: () => { } // Empty!
);
```

**Good:**
```csharp
option.Tap(x => Console.WriteLine(x));
```

---

## Suppressing Rules

Suppress specific rules in code:
```csharp
#pragma warning disable MNT001
var value = option.GetValue();
#pragma warning restore MNT001
```

Or in `.editorconfig`:
```ini
[*.cs]
dotnet_diagnostic.MNT001.severity = none
```

## Rule Severity Levels

| Level | Description |
|-------|-------------|
| `error` | Build fails |
| `warning` | Shown in build output |
| `suggestion` | Shown in IDE only |
| `silent` | Hidden but fixable |
| `none` | Completely disabled |

