# API Reference

Complete API documentation for Monad.NET.

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni)

**License:** MIT

---

## Table of Contents

- [Option\<T\>](#optiont)
- [Result\<T, E\>](#resultt-e)
- [Either\<L, R\>](#eitherl-r)
- [Validation\<T, E\>](#validationt-e)
- [Try\<T\>](#tryt)
- [RemoteData\<T, E\>](#remotedatat-e)
- [NonEmptyList\<T\>](#nonemptylistt)
- [Writer\<W, T\>](#writerw-t)
- [Reader\<R, A\>](#readerr-a)

---

## Option\<T\>

Represents an optional value - either `Some(value)` or `None`.

### Constructors

| Method | Description |
|--------|-------------|
| `Some(T value)` | Creates an Option containing a value |
| `None()` | Creates an empty Option |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsSome` | `bool` | True if contains a value |
| `IsNone` | `bool` | True if empty |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Unwrap()` | `T` | Gets value or throws |
| `Expect(string message)` | `T` | Gets value or throws with message |
| `UnwrapOr(T default)` | `T` | Gets value or returns default |
| `UnwrapOrElse(Func<T>)` | `T` | Gets value or computes default |
| `Map<U>(Func<T, U>)` | `Option<U>` | Transforms the value |
| `Filter(Func<T, bool>)` | `Option<T>` | Filters by predicate |
| `AndThen<U>(Func<T, Option<U>>)` | `Option<U>` | Chains operations (flatMap) |
| `Or(Option<T>)` | `Option<T>` | Returns this if Some, else other |
| `Xor(Option<T>)` | `Option<T>` | Returns Some if exactly one is Some |
| `Match<U>(someFunc, noneFunc)` | `U` | Pattern matching |
| `Match(someAction, noneAction)` | `void` | Pattern matching with actions |
| `OkOr<E>(E error)` | `Result<T, E>` | Converts to Result |
| `OkOrElse<E>(Func<E>)` | `Result<T, E>` | Converts to Result with lazy error |
| `Tap(Action<T>)` | `Option<T>` | Executes action if Some |

### Extension Methods

| Method | Description |
|--------|-------------|
| `ToOption<T>(this T?)` | Converts nullable to Option |
| `Flatten<T>(this Option<Option<T>>)` | Flattens nested Option |

### Async Extensions

| Method | Description |
|--------|-------------|
| `MapAsync<U>(Func<T, Task<U>>)` | Async map |
| `AndThenAsync<U>(Func<T, Task<Option<U>>>)` | Async chain |
| `FilterAsync(Func<T, Task<bool>>)` | Async filter |
| `MatchAsync<U>(someFunc, noneFunc)` | Async pattern match |

---

## Result\<T, E\>

Represents success (`Ok`) or failure (`Err`).

### Constructors

| Method | Description |
|--------|-------------|
| `Ok(T value)` | Creates a success Result |
| `Err(E error)` | Creates an error Result |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsOk` | `bool` | True if success |
| `IsErr` | `bool` | True if error |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Unwrap()` | `T` | Gets value or throws |
| `UnwrapErr()` | `E` | Gets error or throws |
| `Expect(string)` | `T` | Gets value or throws with message |
| `UnwrapOr(T)` | `T` | Gets value or returns default |
| `UnwrapOrElse(Func<E, T>)` | `T` | Gets value or computes from error |
| `Map<U>(Func<T, U>)` | `Result<U, E>` | Transforms the Ok value |
| `MapErr<F>(Func<E, F>)` | `Result<T, F>` | Transforms the Err value |
| `AndThen<U>(Func<T, Result<U, E>>)` | `Result<U, E>` | Chains operations |
| `OrElse<F>(Func<E, Result<T, F>>)` | `Result<T, F>` | Handles error |
| `Match<U>(okFunc, errFunc)` | `U` | Pattern matching |
| `Tap(Action<T>)` | `Result<T, E>` | Executes action if Ok |
| `TapErr(Action<E>)` | `Result<T, E>` | Executes action if Err |
| `Ok()` | `Option<T>` | Converts Ok to Some |
| `Err()` | `Option<E>` | Converts Err to Some |

### Static Methods

| Method | Description |
|--------|-------------|
| `Try(Func<T>)` | Captures exceptions as Err |
| `TryAsync(Func<Task<T>>)` | Async exception capture |

### Async Extensions

| Method | Description |
|--------|-------------|
| `MapAsync<U>(Func<T, Task<U>>)` | Async map |
| `MapErrAsync<F>(Func<E, Task<F>>)` | Async error map |
| `AndThenAsync<U>(Func<T, Task<Result<U, E>>>)` | Async chain |
| `TapAsync(Func<T, Task>)` | Async side effect |

---

## Either\<L, R\>

Represents a value of one of two types.

### Constructors

| Method | Description |
|--------|-------------|
| `Left(L value)` | Creates a Left Either |
| `Right(R value)` | Creates a Right Either |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsLeft` | `bool` | True if Left |
| `IsRight` | `bool` | True if Right |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `UnwrapLeft()` | `L` | Gets Left value or throws |
| `UnwrapRight()` | `R` | Gets Right value or throws |
| `MapLeft<L2>(Func<L, L2>)` | `Either<L2, R>` | Transforms Left |
| `MapRight<R2>(Func<R, R2>)` | `Either<L, R2>` | Transforms Right |
| `BiMap<L2, R2>(leftFunc, rightFunc)` | `Either<L2, R2>` | Transforms both |
| `AndThen<R2>(Func<R, Either<L, R2>>)` | `Either<L, R2>` | Chains on Right |
| `Swap()` | `Either<R, L>` | Swaps Left and Right |
| `Match<U>(leftFunc, rightFunc)` | `U` | Pattern matching |
| `ToResult()` | `Result<R, L>` | Converts to Result |
| `ToOption()` | `Option<R>` | Right to Some, Left to None |

---

## Validation\<T, E\>

Accumulates errors instead of short-circuiting.

### Constructors

| Method | Description |
|--------|-------------|
| `Valid(T value)` | Creates a valid Validation |
| `Invalid(E error)` | Creates invalid with one error |
| `Invalid(IEnumerable<E>)` | Creates invalid with multiple errors |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsValid` | `bool` | True if valid |
| `IsInvalid` | `bool` | True if has errors |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Unwrap()` | `T` | Gets value or throws |
| `UnwrapErrors()` | `IReadOnlyList<E>` | Gets errors or throws |
| `UnwrapOr(T)` | `T` | Gets value or default |
| `Map<U>(Func<T, U>)` | `Validation<U, E>` | Transforms value |
| `MapErrors<F>(Func<E, F>)` | `Validation<T, F>` | Transforms errors |
| `Apply<T2, U>(other, combiner)` | `Validation<U, E>` | Combines validations |
| `And(Validation<T, E>)` | `Validation<T, E>` | Combines, keeping errors |
| `Match<U>(validFunc, invalidFunc)` | `U` | Pattern matching |
| `ToResult()` | `Result<T, IReadOnlyList<E>>` | Converts to Result |
| `ToOption()` | `Option<T>` | Valid to Some |

---

## Try\<T\>

Captures exceptions as values.

### Constructors

| Method | Description |
|--------|-------------|
| `Success(T value)` | Creates successful Try |
| `Failure(Exception)` | Creates failed Try |
| `Of(Func<T>)` | Executes and captures exceptions |
| `OfAsync(Func<Task<T>>)` | Async execution with capture |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsSuccess` | `bool` | True if successful |
| `IsFailure` | `bool` | True if failed |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Get()` | `T` | Gets value or throws |
| `GetException()` | `Exception` | Gets exception or throws |
| `GetOrElse(T)` | `T` | Gets value or default |
| `GetOrElse(Func<T>)` | `T` | Gets value or computes |
| `GetOrElse(Func<Exception, T>)` | `T` | Gets value or computes from exception |
| `Map<U>(Func<T, U>)` | `Try<U>` | Transforms value |
| `FlatMap<U>(Func<T, Try<U>>)` | `Try<U>` | Chains operations |
| `Filter(predicate)` | `Try<T>` | Filters by predicate |
| `Filter(predicate, message)` | `Try<T>` | Filters with error message |
| `Recover(Func<Exception, T>)` | `Try<T>` | Recovers from failure |
| `Recover(Func<Exception, Try<T>>)` | `Try<T>` | Recovers with Try |
| `Match<U>(successFunc, failureFunc)` | `U` | Pattern matching |
| `ToResult<E>(Func<Exception, E>)` | `Result<T, E>` | Converts to Result |
| `ToOption()` | `Option<T>` | Success to Some |

---

## RemoteData\<T, E\>

Tracks async data loading states.

### Constructors

| Method | Description |
|--------|-------------|
| `NotAsked()` | Initial state |
| `Loading()` | Loading state |
| `Success(T data)` | Success with data |
| `Failure(E error)` | Failure with error |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsNotAsked` | `bool` | True if not asked |
| `IsLoading` | `bool` | True if loading |
| `IsSuccess` | `bool` | True if success |
| `IsFailure` | `bool` | True if failure |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Unwrap()` | `T` | Gets data or throws |
| `UnwrapError()` | `E` | Gets error or throws |
| `UnwrapOr(T)` | `T` | Gets data or default |
| `Map<U>(Func<T, U>)` | `RemoteData<U, E>` | Transforms data |
| `MapError<F>(Func<E, F>)` | `RemoteData<T, F>` | Transforms error |
| `FlatMap<U>(Func<T, RemoteData<U, E>>)` | `RemoteData<U, E>` | Chains operations |
| `Match<U>(notAsked, loading, success, failure)` | `U` | Pattern matching |
| `IsLoaded()` | `bool` | True if Success or Failure |
| `IsNotLoaded()` | `bool` | True if NotAsked or Loading |
| `ToResult(notAskedErr, loadingErr)` | `Result<T, E>` | Converts to Result |
| `ToOption()` | `Option<T>` | Success to Some |

---

## NonEmptyList\<T\>

List guaranteed to have at least one element.

### Constructors

| Method | Description |
|--------|-------------|
| `Of(T head)` | Creates with one element |
| `Of(T head, params T[] tail)` | Creates with multiple elements |
| `FromEnumerable(IEnumerable<T>)` | Returns `Option<NonEmptyList<T>>` |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Head` | `T` | First element (always exists) |
| `Tail` | `IReadOnlyList<T>` | Remaining elements |
| `Count` | `int` | Number of elements (≥ 1) |
| `this[int]` | `T` | Element at index |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Last()` | `T` | Last element (always exists) |
| `Map<U>(Func<T, U>)` | `NonEmptyList<U>` | Transforms elements |
| `FlatMap<U>(Func<T, NonEmptyList<U>>)` | `NonEmptyList<U>` | Chains operations |
| `Filter(Func<T, bool>)` | `Option<NonEmptyList<T>>` | Filters (may be empty) |
| `Reduce(Func<T, T, T>)` | `T` | Reduces without initial value |
| `Fold<U>(U seed, Func<U, T, U>)` | `U` | Folds with initial value |
| `Append(T)` | `NonEmptyList<T>` | Adds to end |
| `Prepend(T)` | `NonEmptyList<T>` | Adds to start |
| `Concat(NonEmptyList<T>)` | `NonEmptyList<T>` | Concatenates lists |
| `Reverse()` | `NonEmptyList<T>` | Reverses order |
| `ToList()` | `List<T>` | Converts to List |
| `ToArray()` | `T[]` | Converts to array |

---

## Writer\<W, T\>

Computations with accumulated output.

### Constructors

| Method | Description |
|--------|-------------|
| `Of(T value, W emptyLog)` | Creates with empty log |
| `Tell(T value, W log)` | Creates with log entry |
| `TellUnit(W log)` | Creates log-only (Unit value) |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `T` | The computed value |
| `Log` | `W` | The accumulated log |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Map<U>(Func<T, U>)` | `Writer<W, U>` | Transforms value |
| `FlatMap<U>(binder, combine)` | `Writer<W, U>` | Chains with log combination |
| `BiMap<W2, U>(logMapper, valueMapper)` | `Writer<W2, U>` | Transforms both |
| `Match<U>(Func<T, W, U>)` | `U` | Pattern matching |

---

## Reader\<R, A\>

Computations depending on environment.

### Constructors

| Method | Description |
|--------|-------------|
| `From(Func<R, A>)` | Creates from function |
| `Pure(A value)` | Creates constant value |
| `Ask()` | Returns environment itself |
| `Asks(Func<R, A>)` | Extracts from environment |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Run(R environment)` | `A` | Executes with environment |
| `Map<B>(Func<A, B>)` | `Reader<R, B>` | Transforms result |
| `FlatMap<B>(Func<A, Reader<R, B>>)` | `Reader<R, B>` | Chains operations |
| `WithEnvironment<R2>(Func<R2, R>)` | `Reader<R2, A>` | Transforms environment |

---

## Collection Extensions

### Option Collections

| Method | Description |
|--------|-------------|
| `Sequence()` | `IEnumerable<Option<T>>` → `Option<IEnumerable<T>>` |
| `Traverse(mapper)` | Map and sequence |
| `Choose()` | Filter and unwrap Some values |

### Result Collections

| Method | Description |
|--------|-------------|
| `Sequence()` | `IEnumerable<Result<T,E>>` → `Result<IEnumerable<T>,E>` |
| `Traverse(mapper)` | Map and sequence |
| `Partition()` | Separate into (oks, errors) |
| `CollectOk()` | Get all Ok values |
| `CollectErr()` | Get all Err values |

---

## LINQ Support

All monads support LINQ query syntax:

```csharp
// Select = Map
var result = from x in option select x * 2;

// SelectMany = FlatMap
var result = from x in option1
             from y in option2
             select x + y;

// Where = Filter (Option and Result)
var result = from x in option
             where x > 0
             select x;
```
