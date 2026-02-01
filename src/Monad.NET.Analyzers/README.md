# Monad.NET.Analyzers

Roslyn analyzers for Monad.NET that catch common misuse patterns at compile-time.

## Installation

```bash
dotnet add package Monad.NET.Analyzers
```

## Diagnostics

### Safety Warnings

| ID | Title | Severity | Description |
|----|-------|----------|-------------|
| MNT001 | Unchecked GetValue | Warning | Detects `GetValue()` without checking `IsSome`/`IsOk` first |
| MNT005 | Discarded Monad | Warning | Detects when `Result`/`Option` return values are ignored |
| MNT006 | Throw in Match | Warning | Throwing in None/Err branch defeats pattern matching purpose |
| MNT007 | Nullable to Some | Warning | Passing nullable to `Option.Some` will throw at runtime |
| MNT010 | Null Comparison | Warning | Comparing `Option` (a struct) to null is always wrong |
| MNT011 | GetValue in LINQ | Warning | Using `GetValue` inside LINQ can throw unexpectedly |

### Style Suggestions

| ID | Title | Severity | Description |
|----|-------|----------|-------------|
| MNT002 | Redundant Map Chain | Info | Consecutive `Map` calls can be combined |
| MNT003 | Map+GetValueOr | Info | `Map().GetValueOr()` can be `Match()` or `MapOr()` |
| MNT004 | Bind to Map | Info | `Bind(x => Some(f(x)))` can be `Map(f)` |
| MNT008 | Filter Constant | Info | `Filter(_ => true)` or `Filter(_ => false)` is redundant |
| MNT009 | Map Identity | Info | `Map(x => x)` has no effect |
| MNT012 | Double Negation | Info | `!option.IsNone` should be `option.IsSome` |

## Examples

### MNT001: Unchecked GetValue

```csharp
// Warning: Option should be checked before calling GetValue()
var value = option.GetValue();

// Safe alternatives:
if (option.IsSome) { var value = option.GetValue(); }
var value = option.GetValueOr(defaultValue);
var value = option.Match(some: x => x, none: () => default);
```

### MNT006: Throw in Match

```csharp
// Warning: Throwing in None branch defeats pattern matching purpose
var value = option.Match(
    some: x => x,
    none: () => throw new Exception("No value")); // Use Expect() instead

// Better:
var value = option.GetOrThrow("No value");
```

### MNT007: Nullable to Some

```csharp
string? name = GetName(); // Might be null

// Warning: Will throw if name is null
var option = Option<string>.Some(name);

// Safe:
var option = Option<string>.FromNullable(name);
```

### MNT010: Null Comparison

```csharp
// Warning: Option is a struct, never null
if (option != null) { ... }

// Correct:
if (option.IsSome) { ... }
```

### MNT003: Map+GetValueOr

```csharp
// Info: Can be simplified
var result = option.Map(x => x.ToString()).GetValueOr("default");

// Simplified:
var result = option.Match(
    some: x => x.ToString(),
    none: () => "default");
// Or:
var result = option.MapOr("default", x => x.ToString());
```

### MNT004: Bind to Map

```csharp
// Info: Bind returning Some can be Map
var result = option.Bind(x => Option<int>.Some(x * 2));

// Simplified:
var result = option.Map(x => x * 2);
```

### MNT012: Double Negation

```csharp
// Info: Double negation
if (!option.IsNone) { ... }

// Simplified:
if (option.IsSome) { ... }
```

## Configuration

Configure severity in `.editorconfig`:

```ini
[*.cs]
# Disable specific rule
dotnet_diagnostic.MNT001.severity = none

# Make suggestion an error
dotnet_diagnostic.MNT002.severity = error

# Set all Monad.NET analyzers to warning
dotnet_analyzer_diagnostic.category-Monad.NET.severity = warning
```

## Suppression

```csharp
#pragma warning disable MNT001
var value = option.GetValue(); // I checked elsewhere
#pragma warning restore MNT001
```
