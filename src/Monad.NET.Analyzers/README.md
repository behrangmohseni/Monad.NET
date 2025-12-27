# Monad.NET.Analyzers

Roslyn analyzers for Monad.NET that catch common misuse patterns at compile-time.

## Installation

```bash
dotnet add package Monad.NET.Analyzers
```

## Diagnostics

### MNT001: Unchecked Unwrap Call

**Severity**: Warning

Detects `Unwrap()` calls on monads without prior state checks.

```csharp
// Warning MNT001: Option should be checked before calling Unwrap()
var option = GetOption();
var value = option.Unwrap();

// Safe alternatives:
if (option.IsSome) { var value = option.Unwrap(); }
var value = option.GetOrElse(defaultValue);
```

### MNT002: Redundant Map Chain

**Severity**: Info

Detects consecutive `Map()` calls that can be combined.

```csharp
// Info MNT002: Consecutive Map calls can be combined
var result = option.Map(x => x * 2).Map(y => y + 1);

// Combined:
var result = option.Map(x => x * 2 + 1);
```

### MNT005: Discarded Monad Value

**Severity**: Warning

Detects when `Result`, `Option`, or other monad values are discarded.

```csharp
// Warning MNT005: Result value is discarded
GetResult();  // Return value not used!

// Handle the result:
var result = GetResult();
```

## Configuration

Configure in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.MNT001.severity = error
dotnet_diagnostic.MNT002.severity = none
```
