# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

---

## [1.1.2] - 2026-01-25

### Added

- **NuGet Packages Documentation** - New comprehensive documentation page
  - `docs/NuGetPackages.md` - Lists all packages with NuGet badges and descriptions
  - Updated README with link to NuGet packages page

### Changed

- **Code Style Improvements** - Standardized expression-bodied methods in ThrowHelper
  - Converted multi-line throw methods to expression-bodied members
  - Improved code consistency and readability

- **CodeFactor Configuration** - Enhanced duplicate code analysis exclusions
  - Extended benchmark exclusions for more accurate code quality metrics

---

## [1.1.1] - 2026-01-07

### Added

- **Architecture Tests** - Automated tests enforcing design decisions
  - Core monad types must be readonly structs
  - Core monad types must have only readonly fields
  - Hot path methods must have AggressiveInlining
  - Factory methods must have AggressiveInlining
  - ThrowHelper methods must have NoInlining
  - Core types must be serializable
  - Core types must implement IEquatable and IComparable
  - Core types must have DebuggerDisplay and DebuggerTypeProxy
  - Library must have zero runtime dependencies

- **Architectural Decision Records (ADRs)** - Comprehensive documentation
  - ADR-001: Struct-based Monad Types
  - ADR-002: Nullable Reference Type Integration
  - ADR-003: Aggressive Inlining Strategy
  - ADR-004: ThrowHelper Pattern
  - ADR-005: LINQ Query Syntax Support
  - ADR-006: Implicit Operator Design
  - ADR-007: Async Extension Pattern
  - ADR-008: Multi-Target Framework Strategy

### Changed

- **Improved Error Messages** - Specialized ThrowHelper methods for better diagnostics
  - `Option`: Use `ThrowCannotCreateSomeWithNull()`, `ThrowOptionIsNone()`
  - `Result`: Use `ThrowCannotCreateOkWithNull()`, `ThrowCannotCreateErrWithNull()`, `ThrowResultIsErr()`, `ThrowResultIsOk()`
  - `Either`: Use `ThrowEitherIsLeft()`, `ThrowEitherIsRight()`
  - `Try`: Use `ThrowTryIsFailure()`, `ThrowTryIsSuccess()`
  - `Validation`: Use `ThrowValidationIsInvalid()`, `ThrowValidationIsValid()`

- **Upgraded Analyzer Severities** - Stronger warnings for common issues
  - MNT009 (Map with identity function): Info → Warning
  - MNT014 (Prefer Match over manual state checks): Info → Warning

- **Code Organization** - Split `Collections.cs` (1126 lines) into focused files
  - `Collections.cs` (276 lines): ParallelHelper + Option collections
  - `Collections.Result.cs` (188 lines): Result collection extensions
  - `Collections.Either.cs` (74 lines): Either collection extensions
  - `Collections.Async.cs` (120 lines): Async collection extensions
  - `Collections.ParallelAsync.cs` (274 lines): Parallel async extensions
  - `Collections.Enumerable.cs` (166 lines): General enumerable extensions

### Fixed

- Fixed CodeFactor complexity and code quality issues
- Fixed duplicate code in analyzer Initialize methods
- Excluded benchmarks from CodeFactor duplicate code analysis (intentional duplication for benchmarking)

---

## [1.1.0] - 2025-12-30

### Added

#### Core Library Enhancements

- **Result.Filter** - Filter Ok values with predicate
  - `Filter(Func<T, bool> predicate, TErr error)` - Returns Err if predicate fails
  - `Filter(Func<T, bool> predicate, Func<TErr> errorFactory)` - Lazy error creation
  - `Filter(Func<T, bool> predicate, Func<T, TErr> errorFactory)` - Error from value

- **Enhanced Async Extensions**
  - New async combinators for `Result<T, E>` in `ResultAsync.cs`
  - New async combinators for `Option<T>` in `OptionAsync.cs`

- **Option Parsing Methods** - Type-safe parsing without exceptions
  - `Option.ParseInt`, `Option.ParseLong`, `Option.ParseDouble`, etc.
  - `Option.ParseGuid`, `Option.ParseDateTime`, `Option.ParseEnum<T>`
  - `ReadOnlySpan<char>` overloads for high-performance parsing

- **New Methods on Either, Try, Validation**
  - Additional combinators and convenience methods
  - Enhanced XML documentation with cross-references

#### Analyzers

- **MONAD010: EmptyMatchBranchAnalyzer** - Detects empty match branches
- **MONAD011: MissingConfigureAwaitAnalyzer** - Ensures ConfigureAwait usage
- **MONAD012: PreferMatchAnalyzer** - Suggests Match over if/else patterns
- **MONAD013: SomeWithPotentialNullAnalyzer** - Warns about potential null in Some()
- **MONAD014: ValidationLinqAnalyzer** - Warns about LINQ short-circuiting in Validation

#### Source Generators

- **Enhanced Union Diagnostics** - Better error messages for union generation
- **Improved Union Generator** - More robust code generation

#### Documentation

- **New Guides**
  - `docs/Guides/Analyzers.md` - Complete analyzer documentation
  - `docs/Guides/Recipes.md` - Common patterns and recipes

- **Improved Documentation**
  - Enhanced XML documentation with `<seealso>` cross-references
  - Fixed broken documentation links

#### Examples

- **Restructured Example Project** - Organized by monad type
  - Individual example files for each monad (Option, Result, Either, etc.)
  - New real-world scenario examples
  - Async pipeline examples

#### Tests

- **New Test Suites**
  - `ApiContractTests` - Ensures API stability
  - `BehavioralContractTests` - Validates expected behaviors
  - `MonadLawsTests` - Property-based monad law verification
- **Test Organization** - Reorganized tests into logical folders

### Fixed

- **Benchmark Data Flow Issues** - Fixed monadic pipeline benchmarks that incorrectly used static fields instead of threading data through the pipeline
- **RetryWithBackoff Off-by-One** - Fixed retry function that performed an extra attempt without backoff delay
- **RetryWithBackoff Edge Case** - Added guard for `maxRetries <= 0` to prevent invalid state
- **Documentation Links** - Fixed broken anchor and file path links in README and docs

### Changed

- **Internal Refactoring** - Moved `ThrowHelper` to dedicated file for better organization
- **Benchmark Improvements** - More accurate real-world comparison benchmarks

---

## [1.0.0] - 2025-12-28

### First Stable Release

This is the first stable release of Monad.NET! The library is now production-ready with a stable API.

#### Highlights

- **12 Monad Types**: Option, Result, Either, Validation, Try, RemoteData, NonEmptyList, Writer, Reader, ReaderAsync, State, IO
- **Full Async Support**: Async variants for all operations with `ConfigureAwait(false)`
- **LINQ Integration**: Query syntax and method syntax support
- **Zero Dependencies**: Core library has no external dependencies
- **Multi-Target**: Supports .NET Standard 2.0/2.1, .NET 8.0, and .NET 10.0
- **Integration Packages**: ASP.NET Core, Entity Framework Core, MessagePack
- **Source Generators**: Discriminated unions with `[Union]` attribute
- **Analyzers**: 10+ Roslyn analyzers with code fixes

#### All Features from Alpha/Beta

All features from the alpha releases (1.0.0-alpha.1 through 1.0.0-alpha.13) are included. See below for the complete feature list.

---

## [1.0.0-alpha.13] - 2025-12-21

### Added

- **Validation.Ensure** - Conditional validation with predicate
  - `Ensure(Func<T, bool> predicate, TErr error)` - Validates against predicate
  - `Ensure(Func<T, bool> predicate, Func<TErr> errorFactory)` - Lazy error creation
  - Chain multiple validations fluently
  - Short-circuits if already invalid (preserves original errors)

- **Result.BiMap** - Transform both success and error types
  - `BiMap<U, F>(Func<T, U> okMapper, Func<TErr, F> errMapper)` - Maps both sides
  - Equivalent to `Map().MapErr()` but in one operation
  - Useful for adapting Result types between layers

- **Validation.Flatten** - Flatten nested validations
  - `Flatten<T, TErr>(this Validation<Validation<T, TErr>, TErr>)` - Flattens nested structure
  - Preserves outer errors if outer is invalid
  - Returns inner errors if outer valid but inner invalid

- **Option.DefaultIfNone** - Replace None with default value wrapped in Some
  - `DefaultIfNone(T defaultValue)` - Returns Some(default) if None
  - `DefaultIfNone(Func<T> factory)` - Lazy default creation
  - Unlike `UnwrapOr`, returns `Option<T>` not `T`

- **Option.ThrowIfNone** - Throw specific exceptions on None
  - `ThrowIfNone(Exception exception)` - Throws provided exception
  - `ThrowIfNone(Func<Exception> factory)` - Lazy exception creation
  - Alternative to `Expect` with custom exception types

- **Result.ThrowIfErr** - Throw specific exceptions on Err
  - `ThrowIfErr(Exception exception)` - Throws provided exception
  - `ThrowIfErr(Func<TErr, Exception> factory)` - Create exception from error
  - Alternative to `Expect` with custom exception types

- **Enhanced Benchmark Tests**
  - New `InliningBenchmarks` - Tests for AggressiveInlining effectiveness
  - New `NewMethodsBenchmarks` - Benchmarks for all new methods
  - Comparison benchmarks (BiMap vs separate maps, Ensure vs AndThen, etc.)

### Changed

- Updated API documentation with new methods
- Added comprehensive tests for all new features (50+ new tests)

---

## [1.0.0-alpha.12] - 2025-12-21

### Added

- **Parallel Collection Extensions** - High-performance parallel operations for monadic collections
  - `TraverseParallelAsync<T, U>` for `Option` - Map items to Options in parallel
  - `SequenceParallelAsync<T>` for `Option` - Await Option tasks in parallel
  - `TraverseParallelAsync<T, U, TErr>` for `Result` - Map items to Results in parallel
  - `SequenceParallelAsync<T, TErr>` for `Result` - Await Result tasks in parallel
  - `ChooseParallelAsync<T, U>` - Map to Options in parallel, collect Some values
  - `PartitionParallelAsync<T, U, TErr>` - Map to Results in parallel, separate Ok/Err
  - All methods support `maxDegreeOfParallelism` parameter for controlled concurrency
  - Comprehensive tests for all parallel operations

- **When/Unless Guard Extensions** for `Option<T>` - Conditional Option creation
  - `OptionExtensions.When<T>(bool condition, Func<T> factory)` - Some if condition true
  - `OptionExtensions.When<T>(bool condition, T value)` - Some if condition true
  - `OptionExtensions.Unless<T>(bool condition, Func<T> factory)` - Some if condition false
  - `OptionExtensions.Unless<T>(bool condition, T value)` - Some if condition false
  - Lazy evaluation for factory overloads
  - 12 comprehensive tests

### Changed

- Updated documentation with new features
- Improved README with parallel collection examples
- Enhanced API reference with complete method signatures

---

## [1.0.0-alpha.11] - 2025-12-21

### Added

- **ReaderAsync<R, A> monad** - An asynchronous Reader (environment-dependent) monad for composing async computations that depend on a shared environment.
  - Factory methods: `From`, `FromReader`, `Pure`, `Ask`, `Asks`, `AsksAsync`
  - Execution: `RunAsync` with CancellationToken support
  - Transformation: `Map`, `MapAsync`, `FlatMap`, `FlatMapAsync`, `AndThen`, `Bind`
  - Side effects: `Tap`, `TapAsync`, `TapEnv`, `TapEnvAsync`
  - Combination: `Zip`, `ZipWith`
  - Error handling: `Attempt`, `OrElse`, `Retry`, `RetryWithDelay`
  - Environment transformation: `WithEnvironment`, `WithEnvironmentAsync`
  - Static helpers: `ReaderAsync.Parallel`, `ReaderAsync.From`, `ReaderAsync.Ask`, etc.
  - Collection operations: `Sequence`, `SequenceParallel`, `Traverse`, `TraverseParallel`
  - LINQ query support with `Select` and `SelectMany`
  - Comprehensive XML documentation with examples
  - 40+ comprehensive tests

- **Reader.ToAsync()** - Convert synchronous Reader to ReaderAsync

---

## [1.0.0-alpha.10] - 2025-12-16

### Changed

- **Code quality improvements** - Standardized patterns across the library
  - Use `ArgumentNullException.ThrowIfNull` for .NET 6+ (modern idiom)
  - Added null checks to all extension method parameters
  - Standardized error messages with consistent "Cannot..." patterns
  - Added `[MethodImpl(AggressiveInlining)]` to hot-path methods
  - Added `static` keyword to lambdas that don't capture outer variables
  - Optimized source generator `StringBuilder` usage for better performance

- **Collection method return types** - Standardized to `IReadOnlyList<T>`
  - `Sequence` methods now return `Option<IReadOnlyList<T>>` / `Result<IReadOnlyList<T>, E>`
  - `Traverse` methods now return `Option<IReadOnlyList<U>>` / `Result<IReadOnlyList<U>, E>`
  - `Partition` methods now return `(IReadOnlyList<T>, IReadOnlyList<E>)`
  - Consistent with async variants and provides `Count`/indexer access

- **Source generator data classes converted to records**
  - `UnionInfo`, `UnionCase`, `UnionCaseParameter` now use record syntax
  - Added `IsExternalInit` polyfill for netstandard2.0 support

### Fixed

- `FirstOk` now throws `InvalidOperationException` on empty sequences (like LINQ's `First()`)
- Added `FirstOkOrDefault(defaultError)` for safe empty sequence handling

### Removed

- Duplicate `Recover(Func<Exception, Try<T>>)` method - use `RecoverWith` instead

---

## [1.0.0-alpha.9] - 2025-12-16

### Added

- **IO\<T\> monad** - Defer side effects for pure functional code
  - `IO<T>.Of`, `Pure`, `Return`, `Delay` for constructing IO actions
  - `Run`, `RunAsync` to execute effects
  - `Map`, `AndThen`/`FlatMap`/`Bind` for composition
  - `Tap`, `Apply`, `Zip`, `ZipWith` for combining operations
  - `Attempt` to capture exceptions as `Try<T>`
  - `OrElse` for fallback on failure
  - `Retry`, `RetryWithDelay` for resilience
  - `Replicate` to repeat effects
  - `ToAsync` to convert to async IO
  - `IOAsync<T>` for native async operations
  - `IO.Parallel`, `IO.Race` for concurrent execution
  - Built-in helpers: `IO.WriteLine`, `IO.ReadLine`, `IO.Now`, `IO.Random`, etc.
  - Full LINQ query syntax support
  - 78 comprehensive tests

- **Enhanced discriminated union source generator**
  - `Is{Case}` properties for checking union case type
  - `As{Case}()` methods returning `Option<CaseType>` for safe casting
  - `Map()` method for transforming union cases
  - `Tap()` method for side effects with optional handlers per case
  - `New{Case}()` factory methods for case construction
  - Configurable via `[Union(GenerateFactoryMethods = true, GenerateAsOptionMethods = true)]`
  - 12 new integration tests

---

## [1.0.0-alpha.8] - 2025-12-15

### Added

- **Async streams support** (`IAsyncEnumerable<T>` extensions)
  - `ChooseAsync` - Filter and unwrap Option values from async streams
  - `FirstOrNoneAsync`, `LastOrNoneAsync` - Safe element access
  - `SequenceAsync` - Convert async stream of Options/Results to single Option/Result
  - `CollectOkAsync`, `CollectErrAsync` - Filter Result streams
  - `PartitionAsync` - Separate Ok and Err values
  - `CollectSuccessAsync`, `CollectFailureAsync` - Filter Try streams
  - `SelectAsync`, `WhereAsync`, `TapAsync` - General async stream operations
  - `AnyAsync`, `AllAsync`, `AggregateAsync`, `CountAsync`, `ToListAsync`
  - 28 comprehensive tests

- **Monad.NET.SourceGenerators** - New source generator package for discriminated unions
  - `[Union]` attribute for marking abstract partial records/classes
  - Auto-generates `Match<TResult>` method for pattern matching with return values
  - Auto-generates `Match` (void) method for side effects
  - Compile-time exhaustiveness checking - all cases must be handled
  - Works with nested record and class types
  - 14 comprehensive tests

- **Monad.NET.EntityFrameworkCore** - New EF Core integration package
  - `OptionValueConverter<T>` for reference type Option properties
  - `OptionStructValueConverter<T>` for value type Option properties
  - Query extensions: `FirstOrNone`, `SingleOrNone`, `ElementAtOrNone`, `LastOrNone`
  - Async variants: `FirstOrNoneAsync`, `SingleOrNoneAsync`, etc.
  - Model builder extensions for automatic Option configuration
  - 25 comprehensive tests

---

## [1.0.0-alpha.7] - 2025-12-15

### Added

- **Implicit operators** for cleaner syntax across all monads
  - `Option<T>`: `Option<int> x = 42;` (value → Some, null → None)
  - `Result<T, E>`: `Result<int, string> r = 42;` (value → Ok)
  - `Either<L, R>`: `Either<string, int> e = 42;` (value → Right)
  - `Try<T>`: `Try<int> t = 42;` or `Try<int> t = new Exception("err");`
  - `Validation<T, E>`: `Validation<int, string> v = 42;` (value → Valid)
  - `NonEmptyList<T>`: `NonEmptyList<int> l = 42;` (value → single-element list)
  - `RemoteData<T, E>`: `RemoteData<int, string> d = 42;` (value → Success)

### Improved

- Test coverage increased from 77% to 84%
  - 195 new tests added (total: 780 tests)
  - Writer, Try, NonEmptyList, RemoteData, Reader coverage significantly improved

---

## [1.0.0-alpha.6] - 2025-12-15

### Added

- **Monad.NET.AspNetCore** - New ASP.NET Core integration package
  - IActionResult extensions for `Option`, `Result`, `Validation`, `Either`, `Try`
  - `ToActionResult()`, `ToCreatedResult()`, `ToNoContentResult()` extensions
  - `ToValidationProblemResult()` for RFC 7807 ValidationProblemDetails
  - `ResultExceptionMiddleware` for global exception handling
  - Async support with `ToActionResultAsync()` variants
  - 29 comprehensive tests

- **Deconstruction support** for all monad types
  - `Option<T>`: `var (value, isSome) = option`
  - `Result<T, E>`: `var (value, isOk)` or `var (value, error, isOk)`
  - `Either<L, R>`: `var (left, right, isRight)`
  - `Try<T>`: `var (value, isSuccess)` or `var (value, exception, isSuccess)`
  - `Validation<T, E>`: `var (value, isValid)` or `var (value, errors, isValid)`
  - `RemoteData<T, E>`: `var (data, isSuccess)` or full state deconstruction
  - 29 comprehensive tests

---

## [1.0.0-alpha.5] - 2025-12-15

### Added

- **State\<S, A\>** - Stateful computations without mutable variables
  - `Pure`, `Return`, `Get`, `Put`, `Modify`, `Gets` constructors
  - `Run`, `Eval`, `Exec` for execution
  - `Map`, `AndThen`/`FlatMap`/`Bind` for composition
  - `Apply`, `Zip`, `ZipWith` for combining computations
  - `Sequence`, `Traverse`, `Replicate`, `WhileM` extensions
  - Full LINQ query syntax support
  - 33 comprehensive tests

---

## [1.0.0-alpha.4] - 2025-12-14

### Performance

- Added `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to all hot path methods
- Added `ConfigureAwait(false)` to all async methods for library code
- Added `ThrowHelper` pattern to keep exception paths out of hot paths
- Used `static` lambdas to prevent closure allocations

### Changed

- Optimized core monad types: `Option<T>`, `Result<T,E>`, `Either<L,R>`, `Try<T>`
- Optimized async extensions: `OptionAsync`, `ResultAsync`
- Optimized `Collections` and `Linq` extension methods

### Fixed

- Fixed CI workflow to trigger on `master` branch
- Fixed code formatting and line endings across all source files

---

## [1.0.0-alpha.3] - 2025-12-14

### Changed

- Improved documentation with real-world examples
- Added "Which Monad Should I Use?" quick reference table
- Added Performance section with struct-based design details
- Added comprehensive FAQ section
- Updated code examples to use modern C# patterns (`is not null`)
- Removed version numbers from installation instructions for easier updates

### Fixed

- Corrected CHANGELOG date

---

## [1.0.0-alpha.2] - 2025-12-14

### Changed

- Minor documentation updates

---

## [1.0.0-alpha.1] - 2025-12-14

### Added

#### Core Monads
- **Option\<T\>** - Safe handling of optional values
  - `Some(value)` / `None()` constructors
  - `Map`, `Filter`, `AndThen`, `Or`, `Xor` operations
  - `Match` for pattern matching
  - `UnwrapOr`, `UnwrapOrElse`, `Expect` for safe unwrapping
  - `OkOr`, `OkOrElse` for conversion to Result
  - `ToOption()` extension for nullable types

- **Result\<T, E\>** - Explicit error handling without exceptions
  - `Ok(value)` / `Err(error)` constructors
  - `Map`, `MapErr`, `AndThen`, `OrElse` operations
  - `Match` for pattern matching
  - `Try()` / `TryAsync()` for exception capture
  - `Tap`, `TapErr` for side effects
  - `Transpose` for Option/Result conversion

- **Either\<L, R\>** - Value of one of two possible types
  - `Left(value)` / `Right(value)` constructors
  - `MapLeft`, `MapRight`, `BiMap` operations
  - `Swap` to exchange sides
  - `ToResult`, `ToOption` conversions

- **Validation\<T, E\>** - Error accumulation for form validation
  - `Valid(value)` / `Invalid(errors)` constructors
  - `Apply` for applicative combination (accumulates ALL errors)
  - `And` for combining validations
  - `ToResult`, `ToOption` conversions

- **Try\<T\>** - Exception capture as values
  - `Success(value)` / `Failure(exception)` constructors
  - `Of()` / `OfAsync()` for capturing exceptions
  - `Map`, `FlatMap`, `Filter` operations
  - `Recover` for error recovery
  - `GetOrElse` with multiple overloads
  - `ToResult` conversion

- **RemoteData\<T, E\>** - Async data state tracking
  - Four states: `NotAsked`, `Loading`, `Success`, `Failure`
  - `Map`, `MapError`, `FlatMap` operations
  - `Match` with handlers for all four states
  - `IsLoaded`, `IsNotLoaded` convenience properties
  - `ToResult`, `ToOption` conversions

- **NonEmptyList\<T\>** - List guaranteed to have at least one element
  - `Of(head, ...tail)` constructor
  - `FromEnumerable` returns `Option<NonEmptyList<T>>`
  - Safe `Head`, `Last()`, `Reduce` operations
  - `Map`, `FlatMap`, `Filter` operations
  - `Append`, `Prepend`, `Concat`, `Reverse`

- **Writer\<W, T\>** - Computations with accumulated output
  - `Tell(value, log)` constructor
  - `Map`, `FlatMap` with log combination
  - `BiMap` for transforming both value and log
  - `Match` for pattern matching

- **Reader\<R, A\>** - Dependency injection functional style
  - `From(func)`, `Pure(value)`, `Ask()`, `Asks(selector)` constructors
  - `Map`, `FlatMap` operations
  - `WithEnvironment` for environment transformation
  - `Run(environment)` to execute

#### Async Extensions
- `MapAsync`, `AndThenAsync`, `FilterAsync` for Option
- `MapAsync`, `MapErrAsync`, `AndThenAsync`, `OrElseAsync` for Result
- `TapAsync`, `TapErrAsync` for side effects
- `MatchAsync` for async pattern matching
- `MapAsync`, `FlatMapAsync` for Try

#### LINQ Support
- `Select` (Map) for Option, Result, Either
- `SelectMany` (FlatMap) for monadic binding
- `Where` (Filter) for Option and Result
- Full query syntax support: `from x in ... select ...`

#### Collection Extensions
- `Sequence` - Convert `IEnumerable<Option<T>>` to `Option<IEnumerable<T>>`
- `Traverse` - Map and sequence in one operation
- `Choose` - Filter and unwrap Options
- `Partition` - Separate Results into successes and failures
- `CollectOk`, `CollectErr` - Extract values from Results

### Technical
- Zero external dependencies (except Microsoft.SourceLink.GitHub for debugging)
- Full XML documentation
- 399 unit tests
- Supports .NET 6.0 or later
- Symbol packages for debugging
- Source Link integration

