# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- Performance benchmarks
- ASP.NET Core integration helpers
- Entity Framework extensions
- Source generators for boilerplate reduction
