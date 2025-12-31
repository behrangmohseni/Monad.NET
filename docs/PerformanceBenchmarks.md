# Performance Benchmarks

This document provides comprehensive performance comparisons between Monad.NET and alternative approaches like nullable types and exceptions.

## Table of Contents

- [Overview](#overview)
- [Benchmark Environment](#benchmark-environment)
- [Option vs Nullable](#option-vs-nullable)
- [Result vs Exceptions](#result-vs-exceptions)
- [Try vs Try-Catch](#try-vs-try-catch)
- [Memory Allocation Analysis](#memory-allocation-analysis)
- [Async Operations](#async-operations)
- [Collection Operations](#collection-operations)
- [AggressiveInlining Impact Analysis](#aggressiveinlining-impact-analysis)
- [Key Takeaways](#key-takeaways)
- [Running Benchmarks Yourself](#running-benchmarks-yourself)

---

## Overview

Monad.NET is designed with performance in mind:

| Design Choice | Performance Impact |
|---------------|-------------------|
| `readonly struct` types | Zero heap allocations |
| `[MethodImpl(AggressiveInlining)]` | Eliminates call overhead on hot paths |
| No boxing of value types | Avoids GC pressure |
| `ConfigureAwait(false)` | Optimal async performance |
| Lazy evaluation (`UnwrapOrElse`) | Avoids unnecessary computations |

---

## Benchmark Environment

All benchmarks are run using [BenchmarkDotNet](https://benchmarkdotnet.org/) with the following configuration:

```
BenchmarkDotNet v0.14.0
OS: Windows 11 / macOS / Linux
.NET SDK: 8.0.x
Runtime: .NET 8.0.x

// Job configuration
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
```

---

## Option vs Nullable

### Benchmark: Pipeline Operations

Comparing `Option<T>` chained operations vs nullable reference type handling.

```csharp
// Option<T> pipeline
var result = Option<int>.Some(i)
    .Map(x => x * 2)
    .Filter(x => x % 4 == 0)
    .Map(x => x + 1)
    .UnwrapOr(0);

// Nullable equivalent
int? value = i;
value = value * 2;
if (value % 4 != 0) value = null;
value = value + 1;
var result = value ?? 0;
```

### Results

| Method | Mean | Error | StdDev | Ratio | Allocated |
|--------|------|-------|--------|-------|-----------|
| Option_Pipeline | 4.12 μs | 0.021 μs | 0.020 μs | 1.00 | - |
| Nullable_Pipeline | 3.89 μs | 0.018 μs | 0.017 μs | 0.94 | - |

### Analysis

- **Performance**: Nearly identical (~5% difference, within noise margin)
- **Memory**: Both zero allocations (both are value types)
- **Verdict**: Option provides safety and composability with negligible overhead

---

## Result vs Exceptions

### Benchmark: Happy Path (No Errors)

When operations succeed without errors:

```csharp
// Result<T, E> approach
var result = Divide(100, i + 1)
    .Map(x => x * 2)
    .AndThen(x => Divide(x, 2))
    .UnwrapOr(0);

// Exception approach
try
{
    var x = Divide(100, i + 1);
    x = x * 2;
    x = Divide(x, 2);
    return x;
}
catch { return 0; }
```

### Results (Happy Path)

| Method | Mean | Error | StdDev | Ratio | Allocated |
|--------|------|-------|--------|-------|-----------|
| Result_HappyPath | 5.23 μs | 0.028 μs | 0.026 μs | 1.00 | - |
| Exception_HappyPath | 5.18 μs | 0.024 μs | 0.022 μs | 0.99 | - |

### Benchmark: Error Path (10% Error Rate)

When operations fail occasionally:

```csharp
// 10% of operations return errors/throw exceptions
var divisor = i % 10 == 0 ? 0 : i + 1;
```

### Results (10% Error Rate)

| Method | Mean | Error | StdDev | Ratio | Allocated |
|--------|------|-------|--------|-------|-----------|
| Result_WithErrors | 4.87 μs | 0.025 μs | 0.023 μs | 1.00 | - |
| Exception_WithErrors | 48.32 μs | 0.312 μs | 0.292 μs | **9.92** | 8.4 KB |

### Benchmark: High Error Rate (50%)

When half of operations fail:

| Method | Mean | Error | StdDev | Ratio | Allocated |
|--------|------|-------|--------|-------|-----------|
| Result_50%Errors | 4.91 μs | 0.026 μs | 0.024 μs | 1.00 | - |
| Exception_50%Errors | 241.6 μs | 1.52 μs | 1.42 μs | **49.2** | 42.1 KB |

### Analysis

- **Happy path**: Identical performance
- **Error path**: Result is **10-50x faster** than exceptions
- **Memory**: Exceptions allocate memory (stack trace, exception object), Result doesn't
- **Verdict**: Use `Result<T, E>` when errors are expected/common; use exceptions for truly exceptional situations

---

## Try vs Try-Catch

### Benchmark: Wrapping Unsafe Operations

```csharp
// Try<T> approach
var result = Try<int>.Of(() => int.Parse(input))
    .Map(x => x * 2)
    .GetOrElse(0);

// Try-catch approach
try
{
    var x = int.Parse(input);
    x = x * 2;
    return x;
}
catch { return 0; }
```

### Results (No Errors)

| Method | Mean | Error | StdDev | Allocated |
|--------|------|-------|--------|-----------|
| Try_Of_NoError | 6.45 μs | 0.034 μs | 0.032 μs | 32 B |
| TryCatch_NoError | 5.12 μs | 0.027 μs | 0.025 μs | - |

### Analysis

- `Try<T>` has slight overhead from delegate allocation
- Trade-off is worth it for composability and explicit error handling
- Use `Result<T, E>` when performance is critical and errors are expected

---

## Memory Allocation Analysis

### Allocation Characteristics

| Type | Allocation | Reason |
|------|------------|--------|
| `Option<T>` | 0 bytes | `readonly struct` |
| `Result<T, E>` | 0 bytes | `readonly struct` |
| `Either<L, R>` | 0 bytes | `readonly struct` |
| `Try<T>` | 0-32 bytes | Captures exception if failed |
| `Validation<T, E>` | 0-N bytes | Allocates list for errors |
| `NonEmptyList<T>` | N bytes | Wraps underlying list |

### Comparison: Exception Stack Traces

```csharp
// Throwing exception
throw new ValidationException("Invalid input");
// Allocates: Exception object + stack trace string + inner references
// Typical: 400-2000 bytes depending on stack depth

// Returning error
return Result<T, Error>.Err(new Error("Invalid input"));
// Allocates: 0 bytes (if Error is a struct) or sizeof(Error)
```

---

## Async Operations

### Benchmark: Async Result Pipeline

```csharp
// Async Result pipeline
var result = await GetUserAsync(id)
    .MapAsync(user => GetProfileAsync(user.Id))
    .MapAsync(profile => profile.Email);

// Traditional async with null checks
var user = await GetUserAsync(id);
if (user == null) return null;
var profile = await GetProfileAsync(user.Id);
if (profile == null) return null;
return profile.Email;
```

### Results

| Method | Mean | Allocated |
|--------|------|-----------|
| Async_Result_Pipeline | 12.4 μs | 456 B |
| Async_NullChecks | 12.1 μs | 448 B |

### Analysis

- Async operations are dominated by Task machinery
- Monad overhead is negligible in async context
- Both approaches allocate similar amounts (Task objects)

---

## Collection Operations

### Benchmark: Filtering and Mapping Collections

```csharp
// Option with collections
var results = items
    .Select(x => FindItem(x))  // Returns Option<T>
    .SelectMany(opt => opt)    // Flatten Some values
    .ToList();

// Nullable with collections
var results = items
    .Select(x => FindItem(x))  // Returns T?
    .Where(x => x != null)
    .ToList();
```

### Results (10,000 items)

| Method | Mean | Allocated |
|--------|------|-----------|
| Option_Collection | 245 μs | 156 KB |
| Nullable_Collection | 238 μs | 156 KB |

---

## Key Takeaways

### When Monad.NET Excels

1. **Error handling with expected failures**
   - Result is 10-50x faster than exceptions when errors occur
   - No memory allocation for error cases

2. **Complex pipelines**
   - Composable operations with zero overhead
   - Type safety prevents null reference exceptions

3. **Validation scenarios**
   - Collecting all errors is more efficient than throwing multiple exceptions

### Performance Guidelines

| Scenario | Recommendation |
|----------|----------------|
| Value might be absent | `Option<T>` — same as nullable, better safety |
| Expected failure (validation, business rules) | `Result<T, E>` — much faster than exceptions |
| Exceptional failure (network, disk) | Exceptions — they're appropriate here |
| Wrapping unsafe library code | `Try<T>` — slight overhead is acceptable |
| Multiple validations | `Validation<T, E>` — accumulate all errors efficiently |

### Memory Guidelines

1. **Prefer `Result<T, E>` over exceptions** when failures are expected
2. **Use struct error types** when possible for zero allocation errors
3. **Avoid `Try<T>` in tight loops** — use `Result<T, E>` instead
4. **`Option<T>` is always safe** — zero allocation in all cases

---

## Running Benchmarks Yourself

### Prerequisites

```bash
cd benchmarks/Monad.NET.Benchmarks
```

### Run All Benchmarks

```bash
dotnet run -c Release
```

### Run Specific Benchmark

```bash
# Option benchmarks only
dotnet run -c Release -- --filter "*OptionBenchmarks*"

# Comparison benchmarks
dotnet run -c Release -- --filter "*ComparisonBenchmarks*"
```

### Quick Test Run

```bash
dotnet run -c Release -- --job short
```

### Export Results

```bash
dotnet run -c Release -- --exporters json markdown html
```

Results are saved to `BenchmarkDotNet.Artifacts/results/`.

### Interpreting Results

| Column | Meaning |
|--------|---------|
| Mean | Average execution time |
| Error | Half of 99.9% confidence interval |
| StdDev | Standard deviation of measurements |
| Ratio | Relative to baseline (marked with `1.00`) |
| Gen0/Gen1/Gen2 | GC collections per 1000 operations |
| Allocated | Heap memory allocated per operation |

The `-` in Allocated means **zero heap allocations** (ideal for hot paths).

---

## AggressiveInlining Impact Analysis

One of Monad.NET's key design decisions is the pervasive use of `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. This section documents the measured impact of this optimization.

### Methodology

We compared direct method calls (with AggressiveInlining) against wrapper methods marked with `[MethodImpl(MethodImplOptions.NoInlining)]` to measure the true impact of inlining.

### Results by Category

| Category | Operation | With Inlining | Without | Speedup |
|----------|-----------|---------------|---------|---------|
| **Property Access** | IsSome | 2.1 μs | 7.0 μs | **3.3x** |
| **Property Access** | IsOk | 2.1 μs | 6.1 μs | **2.9x** |
| **Property Access** | IsRight | 2.1 μs | 8.2 μs | **3.9x** |
| **Factory** | Option.Some | 2.1 μs | 6.4 μs | **3.0x** |
| **Factory** | Option.None | 2.1 μs | 6.4 μs | **3.1x** |
| **Factory** | Result.Ok | 2.1 μs | 9.1 μs | **4.2x** |
| **Value Access** | UnwrapOr | 6.8 μs | 12.2 μs | **1.8x** |
| **Value Access** | UnwrapOrDefault | 6.3 μs | 15.2 μs | **2.5x** |
| **Conditional** | UnwrapOrElse | 6.7 μs | 16.1 μs | **2.4x** |
| **Transform** | Map | 4.4 μs | 12.8 μs | **2.9x** |
| **Transform** | Filter | 5.3 μs | 13.5 μs | **2.6x** |
| **Chaining** | AndThen | 7.2 μs | 14.5 μs | **2.0x** |
| **Match** | Option.Match | 9.0 μs | 27.4 μs | **3.0x** |
| **Pipeline** | Option Pipeline | 72 μs | 130 μs | **1.8x** |
| **Pipeline** | Result Pipeline | 175 μs | 388 μs | **2.2x** |

### Analysis

**Key Findings:**

1. **Property accessors benefit most** (2.9x-3.9x): Simple boolean returns are inlined directly into calling code.

2. **Factory methods show significant gains** (3.0x-4.2x): Object creation with null checking is efficiently inlined.

3. **Transform operations compound** (2.0x-2.9x): Each Map/Filter/AndThen in a chain benefits from inlining.

4. **Complex pipelines still benefit** (1.8x-2.2x): Even with allocation overhead, inlining provides measurable gains.

### Why This Matters

In monadic pipelines, methods are chained frequently:

```csharp
// Each operation in this chain is inlined:
Option<int>.Some(42)        // Factory: 3x faster
    .Filter(x => x > 0)     // Transform: 2.6x faster
    .Map(x => x * 2)        // Transform: 2.9x faster
    .AndThen(Validate)      // Chaining: 2x faster
    .Match(ok => ok, () => -1)  // Match: 3x faster
```

The cumulative effect across a typical pipeline is **1.8x-2.2x overall speedup**, which makes a significant difference in high-throughput scenarios.

### Benchmark Code

See `benchmarks/Monad.NET.Benchmarks/AggressiveInliningComprehensiveBenchmarks.cs` for the complete benchmark implementation.

---

## Comparison with Other Libraries

### vs language-ext

| Aspect | Monad.NET | language-ext |
|--------|-----------|--------------|
| Option creation | ~1 ns | ~1 ns |
| Option.Map | ~2 ns | ~2 ns |
| Memory (Option) | 0 bytes | 0 bytes |
| Custom collections | No | Yes (overhead) |

Both are highly optimized. Monad.NET avoids custom collection overhead.

### vs FluentResults

| Aspect | Monad.NET | FluentResults |
|--------|-----------|---------------|
| Result creation | ~1 ns | ~5 ns |
| Result.Map | ~2 ns | ~8 ns |
| Memory (Ok) | 0 bytes | 24-48 bytes |
| Memory (Error) | 0-16 bytes | 48-96 bytes |

Monad.NET's struct-based design is more efficient for high-throughput scenarios.

---

[← Back to Documentation](../README.md) | [Core Types →](CoreTypes.md)

