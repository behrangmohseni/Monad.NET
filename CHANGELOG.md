# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
- Supports .NET 6.0, 7.0, 8.0, and 9.0
- Symbol packages for debugging
- Source Link integration

---

## [Unreleased]

### Planned
- Additional monad types
- Performance optimizations
- Extended framework support
