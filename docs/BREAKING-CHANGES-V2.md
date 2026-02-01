# Breaking Changes in Monad.NET v2.0

This document describes the breaking changes in Monad.NET v2.0 and provides migration guidance.

## Overview

Version 2.0 significantly reduces the API surface to improve discoverability, reduce IntelliSense noise, and lower the maintenance burden. The core functionality remains intact, with removed methods having straightforward replacements.

## Removed Extensions

### Async Extensions (~150 methods removed)

The following async extension files have been removed:
- `OptionAsync.cs` - OptionAsync type and async Option methods
- `ResultAsync.cs` - Async Result methods  
- `ReaderAsync.cs` - Async Reader methods
- `AsyncLinq.cs` - Async LINQ methods
- `AsyncEnumerableExtensions.cs` - Async enumerable methods
- `Collections.Async.cs` - Async collection methods
- `Collections.ParallelAsync.cs` - Parallel async methods

**Migration:** Use `await` with synchronous methods:

```csharp
// Before
var result = await option.MapAsync(async x => await ProcessAsync(x));

// After  
var value = option.GetOrThrow();
var result = await ProcessAsync(value);
// Or use pattern matching
var result = option.IsSome 
    ? await ProcessAsync(option.GetValue())
    : default;
```

### Parsing Extensions (~64 methods removed)

The following parsing files have been removed:
- `OptionParse.cs` - ParseInt, ParseGuid, etc.
- `OptionParseSpan.cs` - Span-based parsing methods

**Migration:** Use standard parsing with Option wrapping:

```csharp
// Before
var result = Option.ParseInt("42");

// After
var result = int.TryParse("42", out var value) 
    ? Option<int>.Some(value) 
    : Option<int>.None();
```

### Collection Extensions (~46 methods removed)

The following collection files have been removed:
- `Collections.cs` - Option collection methods
- `Collections.Enumerable.cs` - Enumerable methods
- `Collections.Result.cs` - Result collection methods
- `Collections.Validation.cs` - Validation collection methods
- `Collections.Try.cs` - Try collection methods
- `Collections.RemoteData.cs` - RemoteData collection methods

**Migration:** Use LINQ and core methods:

```csharp
// Before
var values = options.Choose();

// After
var values = options.Where(o => o.IsSome).Select(o => o.GetValue());
```

## Removed Methods from Core Types

### Option<T>

| Removed Method | Replacement |
|----------------|-------------|
| `GetValueOrElse(Func<T>)` | `Match(x => x, () => fallback())` |
| `GetValueOrDefault()` | `GetValueOr(default)` |
| `GetOrThrow(string)` | `GetOrThrow()` |

### Result<T, TErr>

| Removed Method | Replacement |
|----------------|-------------|
| `GetValueOrElse(Func<TErr, T>)` | `Match(ok => ok, err => fallback(err))` |
| `GetValueOrDefault()` | `GetValueOr(default)` |
| `GetOrThrow(string)` | `GetOrThrow()` |

### Validation<T, TErr>

| Removed Method | Replacement |
|----------------|-------------|
| `GetOrThrow(string)` | `GetOrThrow()` |
| `GetErrorsOrThrow(string)` | `GetErrorsOrThrow()` |

### Try<T>

| Removed Method | Replacement |
|----------------|-------------|
| `GetValueOrElse(Func<T>)` | `Match(ok => ok, _ => fallback())` |
| `GetValueOrRecover(Func<Exception, T>)` | `Match(ok => ok, ex => recover(ex))` |
| `GetOrThrow(string)` | `GetOrThrow()` |
| `GetExceptionOrThrow(string)` | `GetExceptionOrThrow()` |

### RemoteData<T, TErr>

| Removed Method | Replacement |
|----------------|-------------|
| `GetValueOrElse(Func<T>)` | `Match(() => fallback(), () => fallback(), x => x, _ => fallback())` |

### Reader<R, A>

| Removed Method | Replacement |
|----------------|-------------|
| `ToAsync()` | Removed - use async computation directly |

## Hidden Methods

The following methods are still available but hidden from IntelliSense (marked with `[EditorBrowsable(EditorBrowsableState.Never)]`):

- `GetValue()` - use `GetOrThrow()` for more explicit semantics
- `GetException()` on Try - use `GetExceptionOrThrow()`

## LINQ Query Syntax

LINQ query syntax support remains in `Linq.cs` but is now documented as **advanced usage**. The recommended approach is to use core methods directly:

```csharp
// Recommended
var result = option
    .Map(x => x * 2)
    .Bind(Transform);

// Advanced (still works but less discoverable)
var result = from x in option
             let doubled = x * 2
             from r in Transform(doubled)
             select r;
```

## Summary of Changes

| Category | Methods Removed |
|----------|-----------------|
| Async extensions | ~150 |
| Parsing extensions | ~64 |
| Collection extensions | ~46 |
| Redundant getters | ~25 |
| **Total** | **~285** |

The API surface has been reduced from ~722 to ~437 public methods, bringing it closer to industry standards while maintaining all core functionality.
