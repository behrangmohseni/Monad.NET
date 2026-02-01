# Architectural Decisions

This document records the architectural decisions made for Monad.NET, explaining the reasoning and trade-offs behind key design choices. Each decision is documented in an ADR (Architectural Decision Record) format.

## Table of Contents

- [Type Hierarchy and Relationships](#type-hierarchy-and-relationships)
- [ADR-001: Struct-Based Monadic Types](#adr-001-struct-based-monadic-types)
- [ADR-002: AggressiveInlining Usage](#adr-002-aggressiveinlining-usage)
- [ADR-003: ThrowHelper Pattern](#adr-003-throwhelper-pattern)
- [ADR-004: Nullable Reference Type Support](#adr-004-nullable-reference-type-support)
- [ADR-005: Zero External Dependencies](#adr-005-zero-external-dependencies)
- [ADR-006: Roslyn Analyzers as Separate Package](#adr-006-roslyn-analyzers-as-separate-package)
- [ADR-007: Source Generator for Async Extensions](#adr-007-source-generator-for-async-extensions)
- [ADR-008: Multi-Target Framework Strategy](#adr-008-multi-target-framework-strategy)

---

## Type Hierarchy and Relationships

This section provides visual diagrams showing how Monad.NET types relate to each other and their design patterns.

### Core Type Categories

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           MONAD.NET TYPE ARCHITECTURE                            │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                              ERROR HANDLING TYPES                                │
│                                                                                  │
│  ┌─────────────┐    ┌─────────────────┐    ┌─────────────┐                      │
│  │  Result<T,E>│    │ Validation<T,E> │    │   Try<T>    │                      │
│  ├─────────────┤    ├─────────────────┤    ├─────────────┤                      │
│  │ • Ok(value) │    │ • Valid(value)  │    │ • Success   │                      │
│  │ • Err(error)│    │ • Invalid(errs) │    │ • Failure   │                      │
│  ├─────────────┤    ├─────────────────┤    ├─────────────┤                      │
│  │Short-circuit│    │Accumulates ALL  │    │ Captures    │                      │
│  │on first err │    │errors           │    │ exceptions  │                      │
│  └─────────────┘    └─────────────────┘    └─────────────┘                      │
│         │                   │                    │                               │
│         └───────────────────┴────────────────────┘                               │
│                                      │                                           │
│                           All are readonly structs                               │
│                           Zero heap allocation                                   │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                              OPTIONAL VALUE TYPES                                │
│                                                                                  │
│  ┌───────────────┐    ┌───────────────────┐    ┌─────────────────────────────┐  │
│  │   Option<T>   │    │  NonEmptyList<T>  │    │     RemoteData<T,E>         │  │
│  ├───────────────┤    ├───────────────────┤    ├─────────────────────────────┤  │
│  │ • Some(value) │    │ • First element   │    │ • NotAsked                  │  │
│  │ • None        │    │ • Rest elements   │    │ • Loading                   │  │
│  ├───────────────┤    ├───────────────────┤    │ • Success(data)             │  │
│  │ 0 or 1 value  │    │ 1 or more values  │    │ • Failure(error)            │  │
│  │ Nullable      │    │ Guaranteed Head   │    ├─────────────────────────────┤  │
│  │ replacement   │    │                   │    │ UI async state management   │  │
│  └───────────────┘    └───────────────────┘    └─────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                            COMPUTATIONAL CONTEXT TYPES                           │
│                                                                                  │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐       │
│  │ Reader<R,A> │    │ Writer<W,T> │    │ State<S,A>  │    │    IO<T>    │       │
│  ├─────────────┤    ├─────────────┤    ├─────────────┤    ├─────────────┤       │
│  │ Reads from  │    │ Writes to   │    │ Reads and   │    │ Defers side │       │
│  │ environment │    │ log/output  │    │ writes state│    │ effects     │       │
│  ├─────────────┤    ├─────────────┤    ├─────────────┤    ├─────────────┤       │
│  │ DI without  │    │ Audit logs  │    │ Pure state  │    │ Lazy effect │       │
│  │ containers  │    │ tracing     │    │ threading   │    │ execution   │       │
│  └─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘       │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                               SOURCE GENERATORS                                  │
│                                                                                  │
│  ┌─────────────────────────────────────┐    ┌─────────────────────────────────┐ │
│  │           [Union] Attribute          │    │       [ErrorUnion] Attribute    │ │
│  ├─────────────────────────────────────┤    ├─────────────────────────────────┤ │
│  │ Generates discriminated unions from │    │ Generates error hierarchies     │ │
│  │ partial record declarations         │    │ with predefined structure       │ │
│  ├─────────────────────────────────────┤    ├─────────────────────────────────┤ │
│  │ • Match() exhaustive matching       │    │ • Error code constants          │ │
│  │ • MatchAsync() for async handlers   │    │ • Category grouping             │ │
│  │ • Factory methods (Create*)         │    │ • Serialization support         │ │
│  │ • With*() for immutable updates     │    │                                 │ │
│  │ • Is* type checking properties      │    │                                 │ │
│  └─────────────────────────────────────┘    └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Type Relationship Diagram

```
                              ┌───────────────────────┐
                              │    Common Operations  │
                              │  (Functor/Monad Laws) │
                              └───────────────────────┘
                                         │
            ┌────────────────────────────┼────────────────────────────┐
            │                            │                            │
            ▼                            ▼                            ▼
    ┌───────────────┐           ┌───────────────┐           ┌───────────────┐
    │     Map()     │           │    Bind()     │           │   Match()     │
    │ Transform     │           │ Chain/Flatmap │           │ Pattern match │
    │ inner value   │           │ operations    │           │ on state      │
    └───────────────┘           └───────────────┘           └───────────────┘
            │                            │                            │
            └────────────────────────────┼────────────────────────────┘
                                         │
    ┌────────────────────────────────────┼────────────────────────────────────┐
    │                                    │                                    │
    │   ┌────────────┐  ┌────────────┐  │  ┌────────────┐  ┌────────────┐   │
    │   │ Option<T>  │  │Result<T,E> │  │  │Validation  │  │  Try<T>    │   │
    │   │            │  │            │  │  │   <T,E>    │  │            │   │
    │   │ ✓ Map      │  │ ✓ Map      │  │  │ ✓ Map      │  │ ✓ Map      │   │
    │   │ ✓ Bind     │  │ ✓ Bind     │  │  │ ✓ Bind     │  │ ✓ Bind     │   │
    │   │ ✓ Match    │  │ ✓ Match    │  │  │ ✓ Match    │  │ ✓ Match    │   │
    │   │ ✓ Filter   │  │ ✓ MapError │  │  │ ✓ Apply    │  │ ✓ Recover  │   │
    │   │ ✓ GetOr    │  │ ✓ GetOr    │  │  │ ✓ Combine  │  │ ✓ GetOr    │   │
    │   └────────────┘  └────────────┘  │  └────────────┘  └────────────┘   │
    │                                    │                                    │
    └────────────────────────────────────┼────────────────────────────────────┘
                                         │
                              ┌──────────┴──────────┐
                              │                     │
                              ▼                     ▼
                      ┌─────────────┐       ┌─────────────┐
                      │   Async     │       │   LINQ      │
                      │   Support   │       │ Integration │
                      ├─────────────┤       ├─────────────┤
                      │ Try.MapAsync│       │ from x in   │
                      │ IO.ToAsync  │       │ select      │
                      │ IOAsync     │       │ where       │
                      │             │       │ join        │
                      └─────────────┘       └─────────────┘
```

### Error Type Semantics Comparison

```
                    OPERATION FAILS
                          │
         ┌────────────────┼────────────────┐
         │                │                │
         ▼                ▼                ▼
   ┌──────────┐     ┌──────────┐     ┌──────────┐
   │Result<T,E│     │Validation│     │  Try<T>  │
   │          │     │   <T,E>  │     │          │
   └────┬─────┘     └────┬─────┘     └────┬─────┘
        │                │                │
        ▼                ▼                ▼
   ┌──────────┐     ┌──────────┐     ┌──────────┐
   │ STOPS    │     │CONTINUES │     │ CAPTURES │
   │processing│     │collecting│     │exception │
   │          │     │more errs │     │          │
   └────┬─────┘     └────┬─────┘     └────┬─────┘
        │                │                │
        ▼                ▼                ▼
   ┌──────────┐     ┌──────────┐     ┌──────────┐
   │ Returns  │     │ Returns  │     │ Returns  │
   │Err(first)│     │Invalid(  │     │Failure(  │
   │          │     │ all errs)│     │ ex)      │
   └──────────┘     └──────────┘     └──────────┘

   Use when:        Use when:        Use when:
   • First error    • Show ALL       • Wrapping code
     is sufficient    errors           that throws
   • Fail-fast      • Form/input     • Can't modify
     semantics        validation       the source
```

### Integration Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            MONAD.NET ECOSYSTEM                                   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
        ┌─────────────────────────────┼─────────────────────────────┐
        │                             │                             │
        ▼                             ▼                             ▼
┌───────────────────┐       ┌───────────────────┐       ┌───────────────────┐
│   Monad.NET       │       │ Monad.NET         │       │ Monad.NET         │
│   (Core)          │       │ .Analyzers        │       │ .SourceGenerators │
├───────────────────┤       ├───────────────────┤       ├───────────────────┤
│ • Option<T>       │       │ • Compile-time    │       │ • [Union]         │
│ • Result<T,E>     │       │   warnings        │       │ • [ErrorUnion]    │
│ • Validation<T,E> │       │ • Code fixes      │       │ • Async methods   │
│ • Try<T>          │       │ • Best practices  │       │                   │
│ • Reader/Writer   │       │                   │       │                   │
│ • State/IO        │       │                   │       │                   │
└───────────────────┘       └───────────────────┘       └───────────────────┘
        │                                                         │
        └────────────────────────┬────────────────────────────────┘
                                 │
        ┌────────────────────────┼────────────────────────────────┐
        │                        │                                │
        ▼                        ▼                                ▼
┌───────────────────┐   ┌───────────────────┐   ┌───────────────────────────┐
│ Monad.NET         │   │ Monad.NET         │   │ Monad.NET                 │
│ .AspNetCore       │   │ .EntityFramework  │   │ .MessagePack              │
├───────────────────┤   │  Core             │   ├───────────────────────────┤
│ • Result → HTTP   │   ├───────────────────┤   │ • Serialization           │
│ • Validation →    │   │ • DbContext       │   │ • Formatters for all      │
│   BadRequest      │   │   extensions      │   │   monadic types           │
│ • Option →        │   │ • LINQ to EF      │   │                           │
│   NotFound        │   │   integration     │   │                           │
└───────────────────┘   └───────────────────┘   └───────────────────────────┘
```

---

## ADR-001: Struct-Based Monadic Types

### Status
Accepted

### Context
When designing monadic types like `Option<T>`, `Result<T, E>`, and others, we needed to decide between reference types (classes) and value types (structs).

### Decision
All primary monadic types (`Option<T>`, `Result<T, E>`, `Try<T>`, `Validation<T, E>`, `NonEmptyList<T>`, `RemoteData<T, E>`) are implemented as **readonly structs**.

### Rationale

1. **Zero Heap Allocation**: Value types live on the stack, eliminating GC pressure for short-lived monadic operations. This is critical for functional pipelines that chain many operations.

2. **Cache Locality**: Structs are stored inline, improving CPU cache utilization when working with collections of monadic values.

3. **Default Value Safety**: `default(Option<T>)` produces a valid `None` state rather than `null`, preventing null reference exceptions.

4. **Immutability Guarantee**: `readonly struct` ensures the compiler enforces immutability, preventing accidental mutation.

### Consequences

**Positive:**
- Excellent performance in hot paths and tight loops
- No GC pressure for transient monadic operations
- Predictable memory layout

**Negative:**
- Copying overhead for large structs (mitigated by keeping types small)
- Cannot be null (actually a positive for safety)
- Boxing when used in non-generic contexts

### Benchmarks

```
| Scenario              | Struct (Option) | Class equivalent |
|-----------------------|-----------------|------------------|
| Creation (10K ops)    | 2.1 μs          | 15.3 μs          |
| Map chain (10K)       | 4.4 μs          | 28.7 μs          |
| Memory allocated      | 0 B             | 240 KB           |
```

---

## ADR-002: AggressiveInlining Usage

### Status
Accepted

### Context
Monadic types have many small methods (property getters, factory methods, transformations) that are called frequently in pipelines. The JIT compiler typically makes good inlining decisions, but hints can improve performance for known hot paths.

### Decision
Apply `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to **all** public methods on monadic types, including:
- Property getters (`IsSome`, `IsOk`, `IsRight`, etc.)
- Factory methods (`Some()`, `None()`, `Ok()`, `Err()`, etc.)
- Value accessors (`GetValueOr()`, `TryGet()`)
- Transform operations (`Map()`, `Filter()`, `Bind()`)
- Pattern matching (`Match()`)

### Rationale

Comprehensive benchmarking demonstrates **1.8x to 4.2x performance improvements** across all operation categories when using `AggressiveInlining`:

| Category | Direct (Inlined) | No Inline | Speedup |
|----------|-----------------|-----------|---------|
| Property Access | 2.1 μs | 7.0 μs | **3.3x** |
| Factory Methods | 2.1 μs | 7.6 μs | **3.6x** |
| Value Access | 6.5 μs | 14.5 μs | **2.2x** |
| Transform (Map) | 4.4 μs | 12.8 μs | **2.9x** |
| Transform (Filter) | 5.3 μs | 13.5 μs | **2.6x** |
| Chaining (Bind) | 7.2 μs | 14.5 μs | **2.0x** |
| Pattern Match | 9.0 μs | 27.4 μs | **3.0x** |
| Complete Pipeline | 72 μs | 130 μs | **1.8x** |

The benefits compound in real-world usage where multiple operations are chained:

```csharp
// This pipeline benefits from inlining at every step
var result = Option<User>.Some(user)
    .Filter(u => u.IsActive)        // Inlined
    .Map(u => u.Email)              // Inlined
    .Bind(ValidateEmail)         // Inlined
    .Match(                         // Inlined
        email => $"Valid: {email}",
        () => "No valid email"
    );
```

### Consequences

**Positive:**
- Significant performance improvements (1.8x-4.2x)
- Eliminates call overhead for small methods
- Enables additional JIT optimizations (constant folding, dead code elimination)
- Benefits compound in method chains

**Negative:**
- Slightly larger generated code size
- Hint may be ignored if method is too large (not an issue for our small methods)

### Benchmark Methodology

Benchmarks were run using BenchmarkDotNet with:
- .NET 10.0.1, X64 RyuJIT AVX2
- 10,000 iterations per operation
- Memory diagnostics enabled
- Multiple warm-up and actual runs for statistical validity

See `benchmarks/Monad.NET.Benchmarks/AggressiveInliningComprehensiveBenchmarks.cs` for the full benchmark code.

---

## ADR-003: ThrowHelper Pattern

### Status
Accepted

### Context
Exception throwing in hot paths can prevent JIT inlining because the JIT sees the throw as unlikely to be reached and may not inline the calling method to keep code size small.

### Decision
Use a centralized `ThrowHelper` class with `[MethodImpl(MethodImplOptions.NoInlining)]` and `[DoesNotReturn]` attributes for all exception throwing.

### Implementation

```csharp
internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperation(string message)
    {
        throw new InvalidOperationException(message);
    }
    
    // ... other throw helpers
}
```

Usage in hot path:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public T GetValue()
{
    if (!_isSome)
        ThrowHelper.ThrowOptionIsNone(); // NoInlining keeps exception path out
    
    return _value!;
}
```

### Rationale

1. **JIT Optimization**: By marking throw helpers as `NoInlining`, the exception path is kept out of the hot path, allowing the JIT to inline the main method body.

2. **Smaller Code Size**: Exception setup code is factored out, reducing the size of inlined methods.

3. **Consistent Error Messages**: Centralized messages ensure consistent, helpful error information across the library.

4. **DoesNotReturn Contract**: The `[DoesNotReturn]` attribute tells the compiler and analyzers that the method never returns normally, enabling better flow analysis.

### Consequences

**Positive:**
- Improved inlining of hot paths
- Consistent, descriptive error messages
- Better JIT optimization opportunities
- Cleaner main method implementations

**Negative:**
- Additional indirection for exception throwing (negligible - exceptions are slow anyway)
- Extra class to maintain

---

## ADR-004: Nullable Reference Type Support

### Status
Accepted

### Context
C# 8.0 introduced nullable reference types (NRT) to help eliminate null reference exceptions at compile time. Monadic types inherently handle "absence" through their structure rather than null.

### Decision
Enable nullable reference types throughout the library and design APIs that work harmoniously with NRT:

1. `Option<T>.Some(T value)` requires non-null value (analyzer warning if nullable passed)
2. `Result<T, E>.Ok(T value)` requires non-null value
3. Provide `ToOption()` extension for `T?` to safely convert nullable values
4. Use `T?` in `TryGet()` out parameters where absence needs to be represented

### Implementation

```csharp
// Some() enforces non-null at compile time
public static Option<T> Some(T value)
{
    if (value is null)
        ThrowHelper.ThrowCannotCreateSomeWithNull();
    return new Option<T>(value, true);
}

// Safe conversion from nullable
public static Option<T> ToOption<T>(this T? value) where T : class
    => value is null ? Option<T>.None() : Option<T>.Some(value);
```

### Rationale

1. **Compile-Time Safety**: NRT warnings help developers catch potential issues before runtime.

2. **Complementary Design**: Option/Result types complement NRT - use `T` for values that must exist, `Option<T>` for values that might not, and `Result<T, E>` for operations that might fail.

3. **Migration Path**: Provides clean migration for codebases moving from nullable types to explicit optionality.

### Consequences

**Positive:**
- Enhanced IDE support and warnings
- Clear contracts about nullability
- Works well with existing C# nullability features

**Negative:**
- Requires projects to enable nullable context for full benefit
- Some complexity in supporting both nullable and non-nullable contexts

---

## ADR-005: Zero External Dependencies

### Status
Accepted

### Context
Dependencies add complexity, potential security vulnerabilities, version conflicts, and increase package size. For a foundational library like Monad.NET, minimizing dependencies maximizes compatibility and reduces friction.

### Decision
The core `Monad.NET` package has **zero external dependencies** beyond the .NET runtime.

### Rationale

1. **Universal Compatibility**: No version conflicts with other libraries.
2. **Minimal Attack Surface**: Fewer dependencies mean fewer potential vulnerabilities.
3. **Smaller Package Size**: Only the code you need, nothing extra.
4. **Simpler Maintenance**: No need to track and update third-party dependencies.
5. **Easier Adoption**: Lower barrier to entry for new projects.

### Consequences

**Positive:**
- Maximum compatibility across projects
- No transitive dependency issues
- Minimal package size (~150KB)
- No security vulnerabilities from dependencies

**Negative:**
- Some features need to be implemented from scratch
- No leverage of existing utility libraries

---

## ADR-006: Roslyn Analyzers as Separate Package

### Status
Accepted

### Context
Roslyn analyzers provide compile-time warnings and suggestions for better monadic code. However, analyzers have different dependency requirements (Microsoft.CodeAnalysis) and are only useful during development.

### Decision
Ship analyzers as a separate NuGet package (`Monad.NET.Analyzers`) that can be optionally installed.

### Rationale

1. **Optional Installation**: Not all users want/need analyzers.
2. **Separate Dependencies**: Analyzer dependencies (Roslyn) don't pollute the main package.
3. **Independent Versioning**: Analyzers can be updated independently.
4. **CI/CD Flexibility**: Can enable/disable analyzers per build configuration.

### Analyzers Included

| ID | Description |
|----|-------------|
| MONAD001 | Missing null check before Option.Some() |
| MONAD002 | Potential null passed to Option.Some() |
| MONAD003 | Unsafe GetValue() without prior IsSome check |
| MONAD004 | Redundant .Map().Bind() chain |
| MONAD005 | throw expression inside Match() |
| MONAD006 | Empty Match branch |
| MONAD007 | Nullable value in Validation.Valid() |
| MONAD008 | Try.Of() with pure function |
| MONAD009 | Result error type could be more specific |
| MONAD010 | Unreachable Match branch |
| MONAD011 | Redundant Option.Some wrapping |
| MONAD012 | Use .ToOption() instead of conditional |
| MONAD013 | Use .ToResult() instead of try-catch |

### Consequences

**Positive:**
- Clean separation of concerns
- Optional enhancement, not requirement
- Smaller core package
- Independent release cycle

**Negative:**
- Users need to know to install separately
- Two packages to maintain

---

## ADR-007: Selective Async Extensions

### Status
Revised (v2.0)

### Context
Async variants of monadic operations (e.g., `MapAsync`, `BindAsync`) were initially provided for all types. However, this created API bloat and often led users toward anti-patterns like unnecessary async wrapping.

### Decision (v2.0)
Provide async extensions selectively:
- **`IO<T>`**: Full async support via `IOAsync<T>` and `ToAsync()`
- **`Try<T>`**: `MapAsync` and `BindAsync` for exception-prone async operations
- **`Validation<T,E>`**: `MapAsync` for async validation logic
- **`RemoteData<T,E>`**: `MapAsync` for UI state transformations
- **Removed from `Option<T>` and `Result<T,E>`**: Use standard `await` + sync methods

### Rationale

1. **Reduced API Surface**: ~150 async methods removed in v2.0.
2. **Clearer Intent**: Users explicitly handle async boundaries.
3. **Better Composition**: Standard async/await patterns integrate naturally.
4. **Focused Support**: Async extensions where they provide the most value.

### Migration

```csharp
// Before (v1.x)
var result = await option.MapAsync(async x => await ProcessAsync(x));

// After (v2.0) - explicit and clear
if (option.IsSome)
{
    var processed = await ProcessAsync(option.GetValue());
    // ...
}
```

### Consequences

**Positive:**
- No code duplication
- Automatic consistency
- Complete async API coverage
- Easier maintenance

**Negative:**
- Build-time dependency on generator
- Debugging generated code can be tricky
- Additional build complexity

---

## ADR-008: Multi-Target Framework Strategy

### Status
Accepted

### Context
.NET has multiple active versions (LTS releases, current releases) with different feature sets. Supporting multiple frameworks maximizes compatibility while leveraging newer features where available.

### Decision
Target multiple frameworks with conditional compilation:
- `.NET 10.0` (Current)
- `.NET 8.0` (LTS)
- `.NET 6.0` (Previous LTS)
- `.NET Standard 2.0` (Broad compatibility)

### Implementation

```xml
<TargetFrameworks>net10.0;net8.0;net6.0;netstandard2.0</TargetFrameworks>
```

With conditional compilation:
```csharp
#if NET6_0_OR_GREATER
    ArgumentNullException.ThrowIfNull(argument, paramName);
#else
    if (argument is null)
        ThrowArgumentNull(paramName ?? "argument");
#endif
```

### Framework-Specific Features

| Feature | .NET 10 | .NET 8 | .NET 6 | netstandard2.0 |
|---------|---------|--------|--------|----------------|
| CallerArgumentExpression | ✅ | ✅ | ✅ | Polyfill |
| ThrowIfNull | ✅ | ✅ | ✅ | Polyfill |
| Index/Range | ✅ | ✅ | ✅ | Polyfill |
| IAsyncEnumerable | ✅ | ✅ | ✅ | ❌ |
| Required members | ✅ | ✅ | ❌ | ❌ |

### Rationale

1. **Maximum Compatibility**: netstandard2.0 supports .NET Framework 4.6.1+, Mono, Xamarin.
2. **Optimal Performance**: Newer targets can use optimized APIs.
3. **Feature Access**: Modern C# features where supported.
4. **Gradual Adoption**: Projects can upgrade on their own timeline.

### Consequences

**Positive:**
- Wide compatibility across .NET ecosystem
- Best possible performance per framework
- Access to latest language features where available

**Negative:**
- More complex build configuration
- Conditional compilation complexity
- Testing across multiple frameworks

---

## Changelog

| Date | ADR | Change |
|------|-----|--------|
| 2025-01-01 | ADR-001 | Initial documentation |
| 2025-01-01 | ADR-002 | Added comprehensive benchmark data |
| 2025-01-01 | ADR-003 | Initial documentation |
| 2025-01-01 | ADR-004 | Initial documentation |
| 2025-01-01 | ADR-005 | Initial documentation |
| 2025-01-01 | ADR-006 | Initial documentation |
| 2025-01-01 | ADR-007 | Initial documentation |
| 2025-01-01 | ADR-008 | Initial documentation |

