# API Reference

Complete API documentation for Monad.NET.

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni)

**License:** MIT

---

## Table of Contents

- [Option\<T\>](#optiont)
- [Result\<T, E\>](#resultt-e)
- [Validation\<T, E\>](#validationt-e)
- [Try\<T\>](#tryt)
- [RemoteData\<T, E\>](#remotedatat-e)
- [NonEmptyList\<T\>](#nonemptylistt)
- [Writer\<W, T\>](#writerw-t)
- [Reader\<R, A\>](#readerr-a)
- [State\<S, A\>](#states-a)
- [IO\<T\>](#iot)
- [Collection Extensions](#collection-extensions)
- [Async Streams](#async-streams-iasyncenumerable)
- [Source Generators](#source-generators)
- [Entity Framework Core](#entity-framework-core)
- [Language Inspirations](#language-inspirations)

---

## Language Inspirations

These types are proven patterns from functional programming. Here's the lineage:

| Monad.NET | F# | Rust | Haskell |
|-----------|-----|------|---------|
| `Option<T>` | `Option<'T>` | `Option<T>` | `Maybe a` |
| `Result<T, E>` | `Result<'T, 'E>` | `Result<T, E>` | `Either a b` |
| `Validation<T, E>` | — | — | `Validation e a` |
| `Try<T>` | — | — | — (Scala) |
| `RemoteData<T, E>` | — | — | — (Elm) |
| `NonEmptyList<T>` | — | — | `NonEmpty a` |
| `Writer<W, T>` | — | — | `Writer w a` |
| `Reader<R, A>` | — | — | `Reader r a` |
| `State<S, A>` | — | — | `State s a` |
| `IO<T>` | — | — | `IO a` |
| `IO<T>` | `Async<'T>` | `Future<T>` | `IO a` |

---

## Option\<T\>

Represents an optional value - either `Some(value)` or `None`.

> **Inspired by:** Rust's `Option<T>`, F#'s `Option<'T>`, Haskell's `Maybe a`

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
| `GetValue()` | `T` | Gets value or throws |
| `GetOrThrow()` | `T` | Gets value or throws (alias) |
| `GetValueOr(T default)` | `T` | Gets value or returns default |
| `TryGet(out T? value)` | `bool` | C#-style TryGet pattern |
| `Contains(T value)` | `bool` | True if Some and contains value |
| `Exists(Func<T, bool>)` | `bool` | True if Some and predicate passes |
| `Map<U>(Func<T, U>)` | `Option<U>` | Transforms the value |
| `Filter(Func<T, bool>)` | `Option<T>` | Filters by predicate |
| `MapOr<U>(U default, Func<T, U>)` | `U` | Maps or returns default |
| `MapOrElse<U>(Func<U>, Func<T, U>)` | `U` | Maps or computes default |
| `Bind<U>(Func<T, Option<U>>)` | `Option<U>` | Chains operations (flatMap) |
| `And<U>(Option<U>)` | `Option<U>` | Returns other if this is Some |
| `Or(Option<T>)` | `Option<T>` | Returns this if Some, else other |
| `OrElse(Func<Option<T>>)` | `Option<T>` | Returns this if Some, else computes |
| `Xor(Option<T>)` | `Option<T>` | Returns Some if exactly one is Some |
| `Zip<U>(Option<U>)` | `Option<(T, U)>` | Combines into tuple |
| `ZipWith<U, V>(Option<U>, Func<T, U, V>)` | `Option<V>` | Combines with function |
| `Match<U>(someFunc, noneFunc)` | `U` | Pattern matching |
| `Match(someAction, noneAction)` | `void` | Pattern matching with actions |
| `OkOr<E>(E error)` | `Result<T, E>` | Converts to Result |
| `OkOrElse<E>(Func<E>)` | `Result<T, E>` | Converts to Result with lazy error |
| `Tap(Action<T>)` | `Option<T>` | Executes action if Some |
| `TapNone(Action)` | `Option<T>` | Executes action if None |
| `AsEnumerable()` | `IEnumerable<T>` | Returns 0 or 1 element sequence |
| `ToArray()` | `T[]` | Converts to array |
| `ToList()` | `List<T>` | Converts to list |
| `Deconstruct(out T?, out bool)` | `void` | Deconstructs to `(value, isSome)` |

### Extension Methods (OptionExtensions)

#### When/Unless Guards

| Method | Description |
|--------|-------------|
| `When<T>(bool condition, Func<T> factory)` | Returns Some(factory()) if condition is true, else None |
| `When<T>(bool condition, T value)` | Returns Some(value) if condition is true, else None |
| `Unless<T>(bool condition, Func<T> factory)` | Returns Some(factory()) if condition is false, else None |
| `Unless<T>(bool condition, T value)` | Returns Some(value) if condition is false, else None |

#### DefaultIfNone Extensions

| Method | Description |
|--------|-------------|
| `DefaultIfNone<T>(this Option<T>, T default)` | Returns original if Some, else Some(default) |
| `DefaultIfNone<T>(this Option<T>, Func<T> factory)` | Returns original if Some, else Some(factory()) |

#### ThrowIfNone Extensions

| Method | Description |
|--------|-------------|
| `ThrowIfNone<T>(this Option<T>, Exception)` | Returns value if Some, throws exception if None |
| `ThrowIfNone<T>(this Option<T>, Func<Exception>)` | Returns value if Some, throws factory() if None |

#### Conversion Extensions

| Method | Description |
|--------|-------------|
| `ToOption<T>(this T?)` | Converts nullable to Option |
| `Flatten<T>(this Option<Option<T>>)` | Flattens nested Option |
| `Transpose<T, E>(this Option<Result<T, E>>)` | Transposes Option of Result |
| `OfType<TSource, TTarget>(this Option<TSource>)` | Safe cast to reference type |
| `OfTypeValue<TSource, TTarget>(this Option<TSource>)` | Safe cast to value type |

#### String Conversions

| Method | Description |
|--------|-------------|
| `ToOptionNotEmpty(this string?)` | None if null or empty |
| `ToOptionNotWhiteSpace(this string?)` | None if null, empty, or whitespace |
| `ToOptionTrimmed(this string?)` | Trimmed value or None if empty |

#### Collection Lookups

| Method | Description |
|--------|-------------|
| `GetOption<K, V>(this IReadOnlyDictionary<K, V>, K key)` | Dictionary lookup |
| `FirstOption<T>(this IEnumerable<T>)` | First element or None |
| `FirstOption<T>(this IEnumerable<T>, predicate)` | First matching element or None |
| `LastOption<T>(this IEnumerable<T>)` | Last element or None |
| `SingleOption<T>(this IEnumerable<T>)` | Single element or None |
| `ElementAtOption<T>(this IEnumerable<T>, int)` | Element at index or None |

### Operators

| Operator | Description |
|----------|-------------|
| `implicit operator Option<T>(T value)` | Converts value to `Some`, null to `None` |

---

## Result\<T, E\>

Represents success (`Ok`) or failure (`Err`).

> **Inspired by:** Rust's `Result<T, E>`, F#'s `Result<'T, 'E>`

> **v2.0 Note:** `default(Result<T,E>)` is now protected. All operations throw `InvalidOperationException` if the struct was not properly initialized via `Ok()` or `Err()`.

### Constructors

| Method | Description |
|--------|-------------|
| `Ok(T value)` | Creates a success Result |
| `Err(E error)` | Creates an error Result |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsOk` | `bool` | True if success (throws if uninitialized) |
| `IsError` | `bool` | True if error (throws if uninitialized) |
| `IsInitialized` | `bool` | True if properly constructed via `Ok()` or `Err()` |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetValue()` | `T` | Gets value or throws |
| `GetError()` | `E` | Gets error or throws |
| `GetOrThrow()` | `T` | Gets value or throws (alias) |
| `GetValueOr(T)` | `T` | Gets value or returns default |
| `TryGet(out T? value)` | `bool` | C#-style TryGet pattern |
| `TryGetError(out E? error)` | `bool` | C#-style TryGet for error |
| `Map<U>(Func<T, U>)` | `Result<U, E>` | Transforms the Ok value |
| `MapError<F>(Func<E, F>)` | `Result<T, F>` | Transforms the Err value |
| `BiMap<U, F>(Func<T, U>, Func<E, F>)` | `Result<U, F>` | Transforms both Ok and Err values |
| `Bind<U>(Func<T, Result<U, E>>)` | `Result<U, E>` | Chains operations |
| `OrElse<F>(Func<E, Result<T, F>>)` | `Result<T, F>` | Handles error |
| `Match<U>(okFunc, errFunc)` | `U` | Pattern matching |
| `Tap(Action<T>)` | `Result<T, E>` | Executes action if Ok |
| `TapErr(Action<E>)` | `Result<T, E>` | Executes action if Err |
| `Ok()` | `Option<T>` | Converts Ok to Some |
| `Err()` | `Option<E>` | Converts Err to Some |
| `Deconstruct(out T?, out bool)` | `void` | Deconstructs to `(value, isOk)` |
| `Deconstruct(out T?, out E?, out bool)` | `void` | Deconstructs to `(value, error, isOk)` |

### Static Methods

| Method | Description |
|--------|-------------|
| `Try(Func<T>)` | Captures exceptions as Err |
| `TryAsync(Func<Task<T>>)` | Async exception capture |
| `Combine(r1, r2)` | Combines 2 Results into tuple |
| `Combine(r1, r2, combiner)` | Combines 2 Results with function |
| `Combine(r1, r2, r3)` | Combines 3 Results into tuple |
| `Combine(r1, r2, r3, combiner)` | Combines 3 Results with function |
| `Combine(r1, r2, r3, r4)` | Combines 4 Results into tuple |
| `Combine(r1, r2, r3, r4, combiner)` | Combines 4 Results with function |
| `Combine(IEnumerable<Result>)` | Combines collection into list |
| `CombineAll(IEnumerable<Result>)` | Combines ignoring values (returns Unit) |

### ThrowIfErr Extensions

| Method | Description |
|--------|-------------|
| `ThrowIfErr<T, E>(this Result<T, E>, Exception)` | Returns value if Ok, throws exception if Err |
| `ThrowIfErr<T, E>(this Result<T, E>, Func<E, Exception>)` | Returns value if Ok, throws factory(err) if Err |

### Operators

| Operator | Description |
|----------|-------------|
| `implicit operator Result<T, E>(T value)` | Converts value to `Ok` |

---

## Validation\<T, E\>

Accumulates errors instead of short-circuiting.

> **Inspired by:** Haskell's `Validation` (from Data.Validation), F#'s FsToolkit.ErrorHandling

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
| `GetValue()` | `T` | Gets value or throws |
| `GetErrors()` | `IReadOnlyList<E>` | Gets errors or throws |
| `GetValueOr(T)` | `T` | Gets value or default |
| `Map<U>(Func<T, U>)` | `Validation<U, E>` | Transforms value |
| `MapErrors<F>(Func<E, F>)` | `Validation<T, F>` | Transforms errors |
| `Apply<T2, U>(other, combiner)` | `Validation<U, E>` | Combines validations |
| `And(Validation<T, E>)` | `Validation<T, E>` | Combines, keeping errors |
| `Ensure(Func<T, bool>, E)` | `Validation<T, E>` | Validates against predicate |
| `Ensure(Func<T, bool>, Func<E>)` | `Validation<T, E>` | Validates with lazy error |
| `Match<U>(validFunc, invalidFunc)` | `U` | Pattern matching |
| `ToResult()` | `Result<T, IReadOnlyList<E>>` | Converts to Result |
| `ToOption()` | `Option<T>` | Valid to Some |
| `Deconstruct(out T?, out bool)` | `void` | Deconstructs to `(value, isValid)` |
| `Deconstruct(out T?, out IReadOnlyList<E>, out bool)` | `void` | Deconstructs to `(value, errors, isValid)` |

### Extension Methods (ValidationExtensions)

| Method | Description |
|--------|-------------|
| `Flatten<T, E>(this Validation<Validation<T, E>, E>)` | Flattens nested Validation |
| `Combine(this IEnumerable<Validation<T, E>>)` | Combines multiple validations, accumulating errors |
| `ToValidation<T, E>(this Result<T, E>)` | Converts Result to Validation |

### Operators

| Operator | Description |
|----------|-------------|
| `implicit operator Validation<T, E>(T value)` | Converts value to `Valid` |

### LINQ Support

> **⚠️ WARNING: LINQ query syntax does NOT accumulate errors!**
>
> LINQ uses `Bind` internally which **short-circuits on first error**.
> This defeats the main purpose of Validation over Result.
>
> **For error accumulation (the whole point of Validation), use `Apply` or `Zip` instead.**

```csharp
// ❌ DON'T DO THIS - only shows FIRST error
var result = from name in ValidateName(input.Name)
             from email in ValidateEmail(input.Email)
             select new User(name, email);

// ✅ DO THIS - accumulates ALL errors
var result = ValidateName(input.Name)
    .Apply(ValidateEmail(input.Email), (name, email) => new User(name, email));

---

## Try\<T\>

Captures exceptions as values.

> **Inspired by:** Rust's `Result<T, Box<dyn Error>>` pattern, Haskell's `ExceptT`, F#'s `try...with` expressions

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
| `GetValue()` | `T` | Gets value or throws |
| `GetException()` | `Exception` | Gets exception or throws |
| `GetValueOr(T)` | `T` | Gets value or default |
| `TryGet(out T? value)` | `bool` | C#-style TryGet pattern |
| `TryGetException(out Exception? ex)` | `bool` | C#-style TryGet for exception |
| `Map<U>(Func<T, U>)` | `Try<U>` | Transforms value |
| `Bind<U>(Func<T, Try<U>>)` | `Try<U>` | Chains operations |
| `Filter(predicate)` | `Try<T>` | Filters by predicate |
| `Filter(predicate, message)` | `Try<T>` | Filters with error message |
| `Recover(Func<Exception, T>)` | `Try<T>` | Recovers from failure |
| `RecoverWith(Func<Exception, Try<T>>)` | `Try<T>` | Recovers with Try |
| `Match<U>(successFunc, failureFunc)` | `U` | Pattern matching |
| `ToResult<E>(Func<Exception, E>)` | `Result<T, E>` | Converts to Result |
| `ToOption()` | `Option<T>` | Success to Some |
| `Deconstruct(out T?, out bool)` | `void` | Deconstructs to `(value, isSuccess)` |
| `Deconstruct(out T?, out Exception?, out bool)` | `void` | Deconstructs to `(value, exception, isSuccess)` |

### Operators

| Operator | Description |
|----------|-------------|
| `implicit operator Try<T>(T value)` | Converts value to `Success` |
| `implicit operator Try<T>(Exception ex)` | Converts exception to `Failure` |

### LINQ Support

Try supports LINQ query syntax:

```csharp
var result = from x in Try<int>.Of(() => int.Parse("42"))
             from y in Try<int>.Of(() => int.Parse("10"))
             where x > 0
             select x + y;
```

---

## RemoteData\<T, E\>

Tracks async data loading states.

> **Inspired by:** Elm's RemoteData pattern (by Kris Jenkins) — represents NotAsked, Loading, Success, and Failure states

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
| `GetValue()` | `T` | Gets data or throws |
| `GetError()` | `E` | Gets error or throws |
| `GetValueOr(T)` | `T` | Gets data or default |
| `Map<U>(Func<T, U>)` | `RemoteData<U, E>` | Transforms data |
| `MapError<F>(Func<E, F>)` | `RemoteData<T, F>` | Transforms error |
| `Bind<U>(Func<T, RemoteData<U, E>>)` | `RemoteData<U, E>` | Chains operations |
| `Match<U>(notAsked, loading, success, failure)` | `U` | Pattern matching |
| `IsLoaded()` | `bool` | True if Success or Failure |
| `IsNotLoaded()` | `bool` | True if NotAsked or Loading |
| `ToResult(notAskedErr, loadingErr)` | `Result<T, E>` | Converts to Result |
| `ToOption()` | `Option<T>` | Success to Some |
| `Deconstruct(out T?, out bool)` | `void` | Deconstructs to `(data, isSuccess)` |
| `Deconstruct(out T?, out E?, out bool, out bool, out bool, out bool)` | `void` | Full state deconstruction |

### Operators

| Operator | Description |
|----------|-------------|
| `implicit operator RemoteData<T, E>(T data)` | Converts value to `Success` |

---

## NonEmptyList\<T\>

List guaranteed to have at least one element.

> **Inspired by:** Haskell's `NonEmpty` (from Data.List.NonEmpty), F#'s FSharpPlus, Rust's `nonempty` crate

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
| `MapIndexed<U>(Func<T, int, U>)` | `NonEmptyList<U>` | Transforms with index |
| `Bind<U>(Func<T, NonEmptyList<U>>)` | `NonEmptyList<U>` | Chains operations |
| `Filter(Func<T, bool>)` | `Option<NonEmptyList<T>>` | Filters (may be empty) |
| `Reduce(Func<T, T, T>)` | `T` | Reduces without initial value |
| `Fold<U>(U seed, Func<U, T, U>)` | `U` | Folds with initial value |
| `Append(T)` | `NonEmptyList<T>` | Adds to end |
| `Prepend(T)` | `NonEmptyList<T>` | Adds to start |
| `Concat(NonEmptyList<T>)` | `NonEmptyList<T>` | Concatenates lists |
| `Reverse()` | `NonEmptyList<T>` | Reverses order |
| `ToList()` | `List<T>` | Converts to List |
| `ToArray()` | `T[]` | Converts to array |
| `Tap(Action<T>)` | `NonEmptyList<T>` | Executes action for each element |
| `TapIndexed(Action<T, int>)` | `NonEmptyList<T>` | Executes action with index for each element |
| `Deconstruct(out T, out IReadOnlyList<T>)` | `void` | Deconstructs for pattern matching `var (head, tail) = list;` |

### Operators

| Operator | Description |
|----------|-------------|
| `implicit operator NonEmptyList<T>(T value)` | Converts value to single-element list |

---

## Writer\<W, T\>

Computations with accumulated output.

> **Inspired by:** Haskell's `Writer w a` (from Control.Monad.Writer), F#'s FSharpPlus

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
| `Bind<U>(binder, combine)` | `Writer<W, U>` | Chains with log combination |
| `BiMap<W2, U>(logMapper, valueMapper)` | `Writer<W2, U>` | Transforms both |
| `Match<U>(Func<T, W, U>)` | `U` | Pattern matching |
| `Tap(Action<T>)` | `Writer<W, T>` | Executes action with value |
| `TapLog(Action<W>)` | `Writer<W, T>` | Executes action with log |
| `Deconstruct(out T, out W)` | `void` | Deconstructs for pattern matching `var (value, log) = writer;` |

### LINQ Support

Writer supports LINQ query syntax for `Writer<string, T>` and `Writer<List<TLog>, T>`:

```csharp
var result = from x in Writer<string, int>.Tell(10, "Started\n")
             from y in Writer<string, int>.Tell(20, "Added 20\n")
             select x + y;
// result.Value = 30, result.Log = "Started\nAdded 20\n"
```

---

## Reader\<R, A\>

Computations depending on environment.

> **Inspired by:** Haskell's `Reader r a` (from Control.Monad.Reader), F#'s FSharpPlus

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
| `Bind<B>(Func<A, Reader<R, B>>)` | `Reader<R, B>` | Chains operations |
| `WithEnvironment<R2>(Func<R2, R>)` | `Reader<R2, A>` | Transforms environment |
| `Tap(Action<A>)` | `Reader<R, A>` | Executes action with result |
| `TapEnv(Action<R>)` | `Reader<R, A>` | Executes action with environment |
| `Zip<B, C>(Reader<R, B>, Func<A, B, C>)` | `Reader<R, C>` | Combines with function |

---

## State\<S, A\>

Stateful computations that thread state through operations.

> **Inspired by:** Haskell's `State s a` (from Control.Monad.State), F#'s FSharpPlus

### Constructors

| Method | Description |
|--------|-------------|
| `Pure(A value)` | Creates State that returns value without modifying state |
| `Return(A value)` | Alias for Pure |
| `Get()` | Creates State that returns the current state |
| `Put(S newState)` | Creates State that replaces the state |
| `Modify(Func<S, S>)` | Creates State that transforms the state |
| `Gets<U>(Func<S, U>)` | Creates State that extracts a value from state |
| `Of(Func<S, StateResult<S, A>>)` | Creates State from a function |
| `Of(Func<S, (A, S)>)` | Creates State from a tuple-returning function |

### Properties

`StateResult<S, A>` is returned by `Run`:

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `A` | The computed value |
| `State` | `S` | The resulting state |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Run(S initialState)` | `StateResult<S, A>` | Executes with initial state |
| `Eval(S initialState)` | `A` | Executes and returns only value |
| `Exec(S initialState)` | `S` | Executes and returns only final state |
| `Map<U>(Func<A, U>)` | `State<S, U>` | Transforms the value |
| `Bind<U>(Func<A, State<S, U>>)` | `State<S, U>` | Chains operations |
| `Apply<U>(State<S, Func<A, U>>)` | `State<S, U>` | Applicative apply |
| `Zip<U>(State<S, U>)` | `State<S, (A, U)>` | Combines two computations |
| `ZipWith<U, V>(State<S, U>, Func<A, U, V>)` | `State<S, V>` | Combines with function |
| `As<U>(U value)` | `State<S, U>` | Replaces value |
| `Void()` | `State<S, Unit>` | Discards value |
| `Tap(Action<A>)` | `State<S, A>` | Executes action with value |
| `TapState(Action<S>)` | `State<S, A>` | Executes action with state |

### Extension Methods

| Method | Description |
|--------|-------------|
| `Flatten<S, A>(this State<S, State<S, A>>)` | Flattens nested State |
| `Sequence<S, A>(this IEnumerable<State<S, A>>)` | Sequences collection of States |
| `Traverse<S, T, U>(this IEnumerable<T>, Func<T, State<S, U>>)` | Map and sequence |
| `Replicate<S, A>(this State<S, A>, int count)` | Repeats computation n times |
| `WhileM<S, A>(this State<S, A>, Func<S, bool>)` | Repeats while condition holds |

### LINQ Support

| Method | Description |
|--------|-------------|
| `Select` | Map operation |
| `SelectMany` | Bind operation |

---

## IO\<T\>

Defers side effects for pure functional code.

> **Inspired by:** Haskell's `IO a` — the foundational effect type that separates pure code from side effects

### Constructors

| Method | Description |
|--------|-------------|
| `IO<T>.Of(Func<T> effect)` | Creates IO from effect function |
| `IO<T>.Return(T value)` | Creates IO with pure value (no side effects) |
| `IO<T>.Return(T value)` | Alias for `Pure` |
| `IO<T>.Delay(Func<T> effect)` | Alias for `Of`, emphasizes lazy evaluation |

### Static Helpers (IO class)

| Method | Description |
|--------|-------------|
| `IO.Of<T>(effect)` | Create IO from effect |
| `IO.Pure<T>(value)` | Create pure IO |
| `IO.Execute(action)` | Execute action, return `IO<Unit>` |
| `IO.WriteLine(msg)` | Write to console |
| `IO.ReadLine()` | Read line from console (`IO<string?>`) |
| `IO.Now()` | Get current time (`IO<DateTime>`) |
| `IO.UtcNow()` | Get current UTC time |
| `IO.NewGuid()` | Generate new GUID (`IO<Guid>`) |
| `IO.Random()` | Random integer (`IO<int>`) |
| `IO.Random(min, max)` | Random integer in range |
| `IO.GetEnvironmentVariable(name)` | Get env var (`IO<Option<string>>`) |
| `IO.Parallel(io1, io2)` | Run IOs in parallel |
| `IO.Parallel(io1, io2, io3)` | Run three IOs in parallel |
| `IO.Parallel(ios)` | Run collection of IOs in parallel |
| `IO.Race(io1, io2)` | Return first completed IO |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Run()` | `T` | Execute the IO and return result |
| `RunAsync(ct)` | `Task<T>` | Execute asynchronously |
| `Map(f)` | `IO<U>` | Transform the result |
| `Bind(f)` | `IO<U>` | Chain with another IO |
| `Tap(action)` | `IO<T>` | Execute side effect, keep value |
| `Apply(ioFunc)` | `IO<U>` | Apply function in IO to value |
| `Zip(other)` | `IO<(T, U)>` | Combine with another IO |
| `ZipWith(other, f)` | `IO<V>` | Combine using function |
| `As(value)` | `IO<U>` | Replace result with value |
| `Void()` | `IO<Unit>` | Replace result with Unit |
| `Attempt()` | `IO<Try<T>>` | Capture exceptions as Try |
| `ToAsync()` | `IOAsync<T>` | Convert to async IO |
| `OrElse(fallback)` | `IO<T>` | Use fallback IO on exception |
| `OrElse(value)` | `IO<T>` | Use fallback value on exception |
| `Replicate(n)` | `IO<IReadOnlyList<T>>` | Repeat effect n times |
| `Retry(n)` | `IO<T>` | Retry on failure up to n times |
| `RetryWithDelay(n, delay)` | `IOAsync<T>` | Retry with delay between attempts |

### IOAsync\<T\>

Async version of IO for native async operations.

| Method | Return Type | Description |
|--------|-------------|-------------|
| `IOAsync<T>.Of(effect)` | `IOAsync<T>` | Create from async effect |
| `IOAsync<T>.Return(value)` | `IOAsync<T>` | Create with pure value |
| `IOAsync<T>.FromIO(io)` | `IOAsync<T>` | Convert from sync IO |
| `RunAsync(ct)` | `Task<T>` | Execute the async IO |
| `Map(f)` | `IOAsync<U>` | Transform result |
| `MapAsync(f)` | `IOAsync<U>` | Transform with async function |
| `Bind(f)` | `IOAsync<U>` | Chain with another async IO |
| `Tap(action)` | `IOAsync<T>` | Execute side effect |
| `TapAsync(action)` | `IOAsync<T>` | Execute async side effect |
| `Zip(other)` | `IOAsync<(T, U)>` | Combine with another async IO |
| `Void()` | `IOAsync<Unit>` | Replace result with Unit |
| `Attempt()` | `IOAsync<Try<T>>` | Capture exceptions as Try |
| `OrElse(fallback)` | `IOAsync<T>` | Use fallback on exception |

### Static Helpers (IOAsync class)

| Method | Description |
|--------|-------------|
| `IOAsync.Of<T>(effect)` | Create async IO from effect |
| `IOAsync.Pure<T>(value)` | Create pure async IO |
| `IOAsync.FromIO<T>(io)` | Convert from sync IO |
| `IOAsync.Execute(action)` | Execute async action |
| `IOAsync.Delay(timespan)` | Delay for specified duration |
| `IOAsync.Parallel(io1, io2)` | Run in parallel |
| `IOAsync.Parallel(ios)` | Run collection in parallel |
| `IOAsync.Race(io1, io2)` | First to complete wins |

### Extension Methods

| Method | Description |
|--------|-------------|
| `Flatten()` | Flatten nested `IO<IO<T>>` or `IOAsync<IOAsync<T>>` |
| `Sequence()` | `IEnumerable<IO<T>>` → `IO<IReadOnlyList<T>>` |
| `Traverse(f)` | Map and sequence in one operation |
| `Select(f)` | LINQ query support |
| `SelectMany(f)` | LINQ query support |

### LINQ Support

```csharp
var program = 
    from _1 in IO.WriteLine("Enter your name:")
    from name in IO.ReadLine()
    from _2 in IO.WriteLine($"Hello, {name}!")
    select Unit.Default;

program.Run();
```

---

## Collection Extensions

### Option Collections

| Method | Description |
|--------|-------------|
| `Sequence()` | `IEnumerable<Option<T>>` → `Option<IReadOnlyList<T>>` |
| `Traverse(mapper)` | Map and sequence |
| `Choose()` | Filter and unwrap Some values |
| `Choose(selector)` | Map then filter and unwrap |
| `FirstSome()` | First Some value or None |

### Result Collections

| Method | Description |
|--------|-------------|
| `Sequence()` | `IEnumerable<Result<T,E>>` → `Result<IReadOnlyList<T>,E>` |
| `Traverse(mapper)` | Map and sequence |
| `Partition()` | Separate into (oks, errors) |
| `CollectOk()` | Get all Ok values |
| `CollectErr()` | Get all Err values |
| `FirstOk()` | First Ok or last Err (throws if empty) |
| `FirstOkOrDefault(defaultError)` | First Ok or last Err or default |

### Async Collection Extensions

| Method | Description |
|--------|-------------|
| `SequenceAsync()` (Option) | `IEnumerable<Task<Option<T>>>` → `Task<Option<IReadOnlyList<T>>>` |
| `TraverseAsync(selector)` (Option) | Map async and sequence |
| `SequenceAsync()` (Result) | `IEnumerable<Task<Result<T,E>>>` → `Task<Result<IReadOnlyList<T>,E>>` |
| `TraverseAsync(selector)` (Result) | Map async and sequence |

### Parallel Async Collection Extensions

| Method | Description |
|--------|-------------|
| `SequenceParallelAsync(maxDegreeOfParallelism, cancellationToken)` (Option) | Await Option tasks in parallel |
| `TraverseParallelAsync(selector, maxDegreeOfParallelism, cancellationToken)` (Option) | Map to Options in parallel |
| `SequenceParallelAsync(maxDegreeOfParallelism, cancellationToken)` (Result) | Await Result tasks in parallel |
| `TraverseParallelAsync(selector, maxDegreeOfParallelism, cancellationToken)` (Result) | Map to Results in parallel |
| `ChooseParallelAsync(selector, maxDegreeOfParallelism, cancellationToken)` | Map to Options in parallel, collect Some values |
| `PartitionParallelAsync(selector, maxDegreeOfParallelism, cancellationToken)` | Map to Results in parallel, separate Ok/Err |

**Parameters:**
- `maxDegreeOfParallelism`: Maximum concurrent operations (-1 for unlimited)
- `cancellationToken`: Abort in-flight operations cooperatively

### General Enumerable Extensions

| Method | Description |
|--------|-------------|
| `Do(action)` | Execute action for each element (lazy, chainable) |
| `Do(action with index)` | Execute action with index (lazy, chainable) |
| `ForEach(action)` | Execute action for each element (eager) |
| `ForEach(action with index)` | Execute action with index (eager) |
| `ForEachAsync(asyncAction, ct)` | Execute async action sequentially |
| `ForEachAsync(asyncAction with index, ct)` | Execute async action with index |

---

## LINQ Support

All monads support LINQ extension methods for fluent composition.

### Method Syntax (Recommended)

Most developers prefer method syntax for its IntelliSense support and familiar chaining style:

```csharp
// Select = Map - transform the value
var doubled = option.Select(x => x * 2);
var userDto = result.Select(user => new UserDto(user));

// SelectMany = Bind - chain operations that return monads
var email = FindUser(id)
    .SelectMany(user => GetEmail(user.Id))
    .SelectMany(email => ValidateEmail(email));

// Where = Filter - keep values matching predicate
var positive = option.Where(x => x > 0);
var validOrder = result.Where(order => order.IsOk, OrderError.Invalid);

// Combine them fluently
var result = GetUserId(request)
    .SelectMany(id => FindUser(id))
    .Select(user => user.Profile)
    .Where(profile => profile.IsComplete);
```

### Query Syntax

For complex compositions with multiple bindings, query syntax improves readability:

```csharp
// Multiple from clauses bind sequential operations
var result = from user in FindUser(id)
             from profile in LoadProfile(user.Id)
             where profile.IsComplete
             select new UserView(user, profile);

// Equivalent to method syntax:
var result = FindUser(id)
    .SelectMany(user => LoadProfile(user.Id), (user, profile) => (user, profile))
    .Where(x => x.profile.IsComplete)
    .Select(x => new UserView(x.user, x.profile));
```

### Available LINQ Methods by Monad

| Monad | Select | SelectMany | Where |
|-------|--------|------------|-------|
| `Option<T>` | Yes | Yes | `Where(predicate)` |
| `Result<T,E>` | Yes | Yes | `Where(predicate, error)` |
| `Try<T>` | Yes | Yes | `Where(predicate)` |
| `Validation<T,E>` | Yes | Yes (short-circuits) | — |
| `RemoteData<T,E>` | Yes | Yes | — |
| `Writer<W,T>` | Yes | Yes (string, List<T>) | — |
| `State<S,A>` | Yes | Yes | — |
| `IO<T>` | Yes | Yes | — |

---

## Async Streams (IAsyncEnumerable)

Extension methods for working with `IAsyncEnumerable<T>` and monad types.

### Option Extensions

| Method | Description |
|--------|-------------|
| `ChooseAsync()` | Filters to Some values and unwraps |
| `ChooseAsync(selector)` | Maps with sync selector, filters and unwraps |
| `ChooseAsync(asyncSelector)` | Maps with async selector, filters and unwraps |
| `FirstOrNoneAsync()` | First element or None |
| `FirstOrNoneAsync(predicate)` | First matching element or None |
| `LastOrNoneAsync()` | Last element or None |
| `SequenceAsync()` | `IAsyncEnumerable<Option<T>>` → `Task<Option<IReadOnlyList<T>>>` |

### Result Extensions

| Method | Description |
|--------|-------------|
| `CollectOkAsync()` | Filters to Ok values and unwraps |
| `CollectErrAsync()` | Filters to Err values and unwraps |
| `PartitionAsync()` | Separates into (oks, errs) tuple |
| `SequenceAsync()` | `IAsyncEnumerable<Result<T,E>>` → `Task<Result<IReadOnlyList<T>,E>>` |

### Try Extensions

| Method | Description |
|--------|-------------|
| `CollectSuccessAsync()` | Filters to Success values and unwraps |
| `CollectFailureAsync()` | Filters to Failure exceptions |

### General Extensions

| Method | Description |
|--------|-------------|
| `SelectAsync(asyncSelector)` | Async map operation |
| `WhereAsync(asyncPredicate)` | Async filter operation |
| `TapAsync(asyncAction)` | Async side effect |
| `ToListAsync()` | Converts to `Task<List<T>>` |
| `CountAsync()` | Counts elements |
| `AnyAsync(predicate)` | Checks if any match predicate |
| `AllAsync(predicate)` | Checks if all match predicate |
| `AggregateAsync(seed, accumulator)` | Reduces to single value |

---

## Source Generators

Even [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14) doesn't include native discriminated unions (sum types)—a feature present in F#, Rust, Swift, Kotlin, and TypeScript. The [C# proposal (csharplang #8928)](https://github.com/dotnet/csharplang/issues/8928) is under active discussion but has no confirmed release date.

`Monad.NET.SourceGenerators` fills this gap today with compile-time code generation, zero runtime overhead, and exhaustive pattern matching.

### Installation

```bash
dotnet add package Monad.NET.SourceGenerators
```

### UnionAttribute

Marks a type for source generation.

| Requirement | Description |
|-------------|-------------|
| `abstract` | Type must be abstract |
| `partial` | Type must be partial |
| Nested types | Cases must inherit from parent type |

| Property | Default | Description |
|----------|---------|-------------|
| `GenerateFactoryMethods` | `true` | Generate `New{Case}()` factory methods |
| `GenerateAsOptionMethods` | `true` | Generate `As{Case}()` methods (requires Monad.NET) |

### Generated Members

| Member | Return Type | Description |
|--------|-------------|-------------|
| `Match<TResult>(...)` | `TResult` | Pattern match with return value |
| `Match(...)` | `void` | Pattern match with side effects |
| `Is{Case}` | `bool` | Property to check if union is a specific case |
| `As{Case}()` | `Option<CaseType>` | Safely cast to a specific case |
| `Map(...)` | `UnionType` | Transform each case |
| `Tap(...)` | `UnionType` | Execute side effect per case (nullable handlers) |
| `New{Case}(...)` | `CaseType` | Factory method for case construction |

### Example

```csharp
using Monad.NET;

[Union]
public abstract partial record Shape
{
    public partial record Circle(double Radius) : Shape;
    public partial record Rectangle(double Width, double Height) : Shape;
    public partial record Triangle(double Base, double Height) : Shape;
}

Shape shape = new Shape.Circle(5.0);

// Match with return value - exhaustive
var area = shape.Match(
    circle: c => Math.PI * c.Radius * c.Radius,
    rectangle: r => r.Width * r.Height,
    triangle: t => 0.5 * t.Base * t.Height
);

// Is{Case} properties
if (shape.IsCircle)
    Console.WriteLine("It's a circle!");

// As{Case}() - safe casting
var radius = shape.AsCircle()
    .Map(c => c.Radius)
    .GetValueOr(0);

// Map - transform
var doubled = shape.Map(
    circle: c => new Shape.Circle(c.Radius * 2),
    rectangle: r => new Shape.Rectangle(r.Width * 2, r.Height * 2),
    triangle: t => new Shape.Triangle(t.Base * 2, t.Height * 2)
);

// Tap - side effects (nulls are skipped)
shape.Tap(circle: c => Console.WriteLine($"Circle: {c.Radius}"));

// Factory methods
var circle = Shape.NewCircle(5.0);
var rect = Shape.NewRectangle(4.0, 5.0);
```

---

## Entity Framework Core

The `Monad.NET.EntityFrameworkCore` package provides EF Core integration for `Option<T>`.

### Installation

```bash
dotnet add package Monad.NET.EntityFrameworkCore
```

### Value Converters

| Converter | Description |
|-----------|-------------|
| `OptionValueConverter<T>` | Converts `Option<T>` for reference types |
| `OptionStructValueConverter<T>` | Converts `Option<T>` for value types |

### Usage

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Option<string> Email { get; set; }
    public Option<int> Age { get; set; }
}

// In DbContext.OnModelCreating
modelBuilder.Entity<User>(entity =>
{
    entity.Property(e => e.Email)
        .HasConversion(new OptionValueConverter<string>());

    entity.Property(e => e.Age)
        .HasConversion(new OptionStructValueConverter<int>());
});
```

### Query Extensions

| Method | Description |
|--------|-------------|
| `FirstOrNone()` | Returns first element or `None` |
| `FirstOrNone(predicate)` | Returns first matching element or `None` |
| `SingleOrNone()` | Returns single element or `None` (throws if multiple) |
| `SingleOrNone(predicate)` | Returns single matching element or `None` |
| `ElementAtOrNone(index)` | Returns element at index or `None` |
| `LastOrNone()` | Returns last element or `None` |
| `LastOrNone(predicate)` | Returns last matching element or `None` |

### Async Query Extensions

| Method | Description |
|--------|-------------|
| `FirstOrNoneAsync()` | Async first element or `None` |
| `FirstOrNoneAsync(predicate)` | Async first matching element or `None` |
| `SingleOrNoneAsync()` | Async single element or `None` |
| `SingleOrNoneAsync(predicate)` | Async single matching element or `None` |
| `ElementAtOrNoneAsync(index)` | Async element at index or `None` |
| `LastOrNoneAsync()` | Async last element or `None` |
| `LastOrNoneAsync(predicate)` | Async last matching element or `None` |

### Example

```csharp
// Sync query
var user = context.Users.FirstOrNone(u => u.Id == id);
user.Match(
    someFunc: u => Console.WriteLine($"Found: {u.Name}"),
    noneFunc: () => Console.WriteLine("Not found")
);

// Async query
var user = await context.Users.FirstOrNoneAsync(u => u.Email.IsSome);

// Safe chaining
var email = await context.Users
    .FirstOrNoneAsync(u => u.Id == id)
    .Map(u => u.Email)
    .Flatten();
```
