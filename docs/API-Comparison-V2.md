# Monad.NET API Comparison - V2.0 Changes

This document provides a comprehensive comparison of APIs showing what changed in Monad.NET V2.0. Methods marked with ~~strikethrough~~ were removed in V2.0.

---

## 1. State Properties

| Type | Property 1 | Property 2 | Property 3 | Property 4 |
|------|------------|------------|------------|------------|
| **Option\<T\>** | `IsSome` | `IsNone` | - | - |
| **Result\<T,E\>** | `IsOk` | `IsErr` | - | - |
| **Validation\<T,E\>** | `IsValid` | `IsInvalid` | - | - |
| **Try\<T\>** | `IsSuccess` | `IsFailure` | - | - |
| **RemoteData\<T,E\>** | `IsSuccess` | `IsFailure` | `IsNotAsked` | `IsLoading` |

### Inconsistencies
- `Option`: `IsSome/IsNone` (Rust-inspired)
- `Result`: `IsOk/IsErr` (Rust-inspired)
- `Validation`: `IsValid/IsInvalid` (domain-specific)
- `Try`: `IsSuccess/IsFailure` (Scala-inspired)
- `RemoteData`: `IsSuccess/IsFailure` (matches Try)

---

## 2. Factory Methods

| Type | Success Factory | Failure Factory | Other |
|------|-----------------|-----------------|-------|
| **Option\<T\>** | `Some(T)` | `None()` | - |
| **Result\<T,E\>** | `Ok(T)` | `Err(E)` | - |
| **Validation\<T,E\>** | `Valid(T)` | `Invalid(E)`, `Invalid(IEnumerable<E>)` | - |
| **Try\<T\>** | `Success(T)` | `Failure(Exception)` | `Of(Func<T>)`, `OfAsync(...)` |
| **RemoteData\<T,E\>** | `Success(T)` | `Failure(E)` | `NotAsked()`, `Loading()` |
| **NonEmptyList\<T\>** | `Of(T)`, `Of(T, params T[])` | - | `FromEnumerable(...)` |
| **Writer\<W,T\>** | `Of(T, W empty)`, `Tell(T, W)` | - | `TellUnit(W)` |
| **Reader\<R,A\>** | `Return(A)`, `From(Func<R,A>)` | - | `Ask()`, `Asks(Func<R,A>)` |
| **State\<S,A\>** | `Return(A)`, `Of(...)` | - | `Get()`, `Put(S)`, `Modify(...)`, `Gets(...)` |
| **IO\<T\>** | `Return(T)`, `Of(Func<T>)` | - | - |
| **IOAsync\<T\>** | `Return(T)`, `Of(Func<Task<T>>)` | - | `FromIO(IO<T>)` |

### V1.0 Inconsistencies (Fixed in V2.0)
- ~~Some types use `Pure` (Reader, State, IO) while others don't~~ → **V2.0: All use `Return`**
- ~~`IO.Delay` redundant with `IO.Of`~~ → **V2.0: `Delay` removed**
- `Try` uses `Success/Failure` while `Result` uses `Ok/Err` (kept - semantic to each type)

---

## 3. Value Extraction Methods

### Basic Get Value (V2.0)

| Type | Get Value | Get Error |
|------|-----------|-----------|
| **Option\<T\>** | `GetValue()` | - |
| **Result\<T,E\>** | `GetValue()` | `GetError()` |
| **Validation\<T,E\>** | `GetValue()` | `GetErrors()` |
| **Try\<T\>** | `GetValue()` | `GetException()` |
| **RemoteData\<T,E\>** | `GetValue()` | `GetError()` |

### With Default Value (V2.0)

| Type | GetValueOr | ~~GetValueOrElse~~ | ~~GetValueOrDefault~~ |
|------|------------|----------------|-------------------|
| **Option\<T\>** | `GetValueOr(T)` | ~~Removed~~ - use `Match()` | ~~Removed~~ |
| **Result\<T,E\>** | `GetValueOr(T)` | ~~Removed~~ - use `Match()` | ~~Removed~~ |
| **Validation\<T,E\>** | `GetValueOr(T)` | - | - |
| **Try\<T\>** | `GetValueOr(T)` | ~~Removed~~ - use `Match()` | - |
| **RemoteData\<T,E\>** | `GetValueOr(T)` | ~~Removed~~ - use `Match()` | - |

> **V2.0 Migration:** Replace `GetValueOrElse(func)` with `Match(x => x, func)` for lazy evaluation.

### GetOrThrow Pattern (V2.0)

| Type | GetOrThrow() | ~~GetOrThrow(msg)~~ |
|------|--------------|-----------------|
| **Option\<T\>** | `GetOrThrow()` | ~~Removed~~ |
| **Result\<T,E\>** | `GetOrThrow()` | ~~Removed~~ |
| **Validation\<T,E\>** | `GetOrThrow()` | ~~Removed~~ |
| **Try\<T\>** | `GetOrThrow()` | ~~Removed~~ |
| **RemoteData\<T,E\>** | - | - |

> **V2.0 Migration:** Use `GetOrThrow()` which provides a descriptive message automatically.

### C# Idiomatic TryGet Pattern (V2.0)

| Type | TryGet() | TryGetError() |
|------|--------------|-----------------|-------------------|----------------------|
| **Option\<T\>** | ✅ | ✅ | - | - |
| **Result\<T,E\>** | ✅ | ✅ | ✅ | ✅ |
| **Validation\<T,E\>** | ✅ | ✅ | `GetErrorsOrThrow()` | `GetErrorsOrThrow(msg)` |
| **Try\<T\>** | ✅ | ✅ | `GetExceptionOrThrow()` | `GetExceptionOrThrow(msg)` |
| **RemoteData\<T,E\>** | ❌ | ❌ | ❌ | ❌ |

### TryGet Pattern (C# idiomatic)

| Type | TryGet Value | TryGet Error |
|------|--------------|--------------|
| **Option\<T\>** | `TryGet(out T?)` | - |
| **Result\<T,E\>** | `TryGet(out T?)` | `TryGetError(out E?)` |
| **Validation\<T,E\>** | `TryGet(out T?)` | `TryGetErrors(out list)` |
| **Try\<T\>** | `TryGet(out T?)` | `TryGetException(out Ex?)` |
| **RemoteData\<T,E\>** | `TryGet(out T?)` | `TryGetError(out E?)` |

---

## 4. Transformation Methods

### Map

| Type | Map Value | Map Error | BiMap |
|------|-----------|-----------|-------|
| **Option\<T\>** | `Map<U>(Func<T,U>)` | - | ❌ |
| **Result\<T,E\>** | `Map<U>(Func<T,U>)` | `MapErr<F>(Func<E,F>)` | `BiMap<U,F>(...)` |
| **Validation\<T,E\>** | `Map<U>(Func<T,U>)` | `MapErrors<F>(...)` | `BiMap<U,F>(...)` |
| **Try\<T\>** | `Map<U>(Func<T,U>)` | - | ❌ |
| **RemoteData\<T,E\>** | `Map<U>(Func<T,U>)` | `MapError<F>(...)` | `BiMap<U,F>(...)` |
| **Writer\<W,T\>** | `Map<U>(Func<T,U>)` | `MapLog<ULog>(...)` | `BiMap<ULog,U>(...)` |
| **Reader\<R,A\>** | `Map<B>(Func<A,B>)` | - | ❌ |
| **State\<S,T\>** | `Map<U>(Func<T,U>)` | - | ❌ |
| **IO\<T\>** | `Map<U>(Func<T,U>)` | - | ❌ |
| **NonEmptyList\<T\>** | `Map<U>(...)`, `MapIndexed<U>(...)` | - | ❌ |

### V1.0 Inconsistencies in Map Error Naming (Fixed in V2.0)
- `Result`: `MapErr` → **V2.0: `MapError`**
- `Validation`: `MapErrors` (plural - kept, multiple errors)
- `RemoteData`: `MapError` (singular - kept)

### Monadic Bind (V2.0 - Consolidated to `Bind`)

| Type | Bind (V2.0) | V1.0 Aliases (Removed) |
|------|-------------|------------------------|
| **Option\<T\>** | ✅ | `AndThen` |
| **Result\<T,E\>** | ✅ | `FlatMap`, `AndThen` |
| **Validation\<T,E\>** | ✅ | `FlatMap`, `AndThen` |
| **Try\<T\>** | ✅ | `FlatMap`, `AndThen` |
| **RemoteData\<T,E\>** | ✅ | `FlatMap`, `AndThen` |
| **Writer\<W,T\>** | ✅ (needs combine) | `FlatMap` |
| **Reader\<R,A\>** | ✅ | `FlatMap`, `AndThen` |
| **State\<S,T\>** | ✅ | `FlatMap`, `AndThen` |
| **IO\<T\>** | ✅ | `FlatMap`, `AndThen` |
| **NonEmptyList\<T\>** | ✅ | `FlatMap` |

### V2.0 Status
- All types now use `Bind` consistently (aligns with LINQ's `SelectMany`)
- `FlatMap` and `AndThen` removed from all types

---

## 5. Filter Methods

| Type | Filter | Filter with error | Ensure |
|------|--------|-------------------|--------|
| **Option\<T\>** | `Filter(pred)` | ❌ | ❌ |
| **Result\<T,E\>** | `Filter(pred, E)` | `Filter(pred, Func<E>)`, `Filter(pred, Func<T,E>)` | ❌ |
| **Validation\<T,E\>** | ❌ | ❌ | `Ensure(pred, E)`, `Ensure(pred, Func<E>)` |
| **Try\<T\>** | `Filter(pred)` | `Filter(pred, string)`, `Filter(pred, Func<Ex>)` | ❌ |
| **NonEmptyList\<T\>** | `Filter(pred)` → `Option<NonEmptyList<T>>` | ❌ | ❌ |

### Inconsistency
- `Validation` uses `Ensure` instead of `Filter`
- `Option` Filter doesn't allow specifying a "None reason"

---

## 6. Combination Methods

### Zip / ZipWith

| Type | Zip | ZipWith |
|------|-----|---------|
| **Option\<T\>** | `Zip<U>(Option<U>)` | `ZipWith<U,V>(Option<U>, Func<T,U,V>)` |
| **Result\<T,E\>** | `Zip<U>(Result)` | `ZipWith<U,V>(...)` |
| **Validation\<T,E\>** | `Zip<U>(...)` | `ZipWith<U,V>(...)` |
| **Try\<T\>** | `Zip<U>(...)` | `ZipWith<U,V>(...)` |
| **RemoteData\<T,E\>** | ❌ | ❌ |
| **State\<S,T\>** | `Zip<U>(...)` | `ZipWith<U,V>(...)` |
| **IO\<T\>** | `Zip<U>(...)` | `ZipWith<U,V>(...)` |
| **Reader\<R,A\>** | ❌ | `Zip<B,C>(other, combiner)` |

### Inconsistency
- `RemoteData` missing `Zip/ZipWith`
- `Reader` has `Zip` but with different signature

### And / Or / OrElse

| Type | And | Or | OrElse |
|------|-----|----|--------|
| **Option\<T\>** | `And<U>(Option<U>)` | `Or(Option<T>)` | `OrElse(Func<Option<T>>)` |
| **Result\<T,E\>** | `And<U>(Result)` | `Or(Result)` | `OrElse<F>(Func)` |
| **Validation\<T,E\>** | `And(Validation)` | ❌ | ❌ |
| **Try\<T\>** | ❌ | ❌ | ❌ |
| **RemoteData\<T,E\>** | ❌ | `Or(RemoteData)` | `OrElse(Func)` |
| **IO\<T\>** | ❌ | ❌ | `OrElse(IO)`, `OrElse(T)` |

---

## 7. Side Effect Methods (Tap)

| Type | Tap Success | Tap Failure | Location |
|------|-------------|-------------|----------|
| **Option\<T\>** | `Tap(Action<T>)` | `TapNone(Action)` | Instance |
| **Result\<T,E\>** | `Tap(Action<T>)` | `TapErr(Action<E>)` | Extension |
| **Validation\<T,E\>** | `Tap(Action<T>)` | `TapErrors(Action<list>)` | Extension |
| **Try\<T\>** | `Tap(Action<T>)` | `TapFailure(Action<Ex>)` | Extension |
| **RemoteData\<T,E\>** | `Tap(Action<T>)` | `TapFailure`, `TapNotAsked`, `TapLoading` | Extension |
| **Writer\<W,T\>** | `Tap(Action<T>)` | `TapLog(Action<W>)` | Instance |
| **Reader\<R,A\>** | `Tap(Action<A>)` | `TapEnv(Action<R>)` | Instance |
| **State\<S,T\>** | `Tap(Action<T>)` | `TapState(Action<S>)` | Instance |
| **IO\<T\>** | `Tap(Action<T>)` | ❌ | Instance |
| **IOAsync\<T\>** | `Tap(Action<T>)` | ❌ | Instance |
| **NonEmptyList\<T\>** | `Tap(Action<T>)`, `TapIndexed(...)` | ❌ | Instance |

### Inconsistencies (V1.0)
- Mix of instance methods and extension methods
- Different naming: `TapNone`, `TapErr`, `TapLeft`, `TapFailure`, `TapErrors`
- `IO/IOAsync` missing failure tap

### V2.0 Status
- Removed `TapInvalid` alias (use `TapErrors`)
- Removed `TapError` alias (use `TapFailure`)

---

## 8. Match Methods

| Type | Match Action | Match Func |
|------|--------------|------------|
| **Option\<T\>** | `Match(someAction, noneAction)` | `Match<U>(someFunc, noneFunc)` |
| **Result\<T,E\>** | `Match(okAction, errAction)` | `Match<U>(okFunc, errFunc)` |
| **Validation\<T,E\>** | `Match(validAction, invalidAction)` | `Match<U>(validFunc, invalidFunc)` |
| **Try\<T\>** | `Match(successAction, failureAction)` | `Match<U>(successFunc, failureFunc)` |
| **RemoteData\<T,E\>** | 4-way match | 4-way match |
| **Writer\<W,T\>** | ❌ | `Match<U>(Func<T,W,U>)` |

### Note
All types consistently have Match, but parameter naming varies with the type semantics.

---

## 9. Conversion Methods

| Type | ToOption | ToResult | ToValidation | Other |
|------|----------|----------|--------------|-------|
| **Option\<T\>** | - | `OkOr<E>(E)`, `OkOrElse<E>(Func)` | ❌ | `AsEnumerable()`, `ToArray()`, `ToList()` |
| **Result\<T,E\>** | `Ok()` | - | `ToValidation()` | `Err()`, `AsEnumerable()`, `ToArray()`, `ToList()` |
| **Validation\<T,E\>** | `ToOption()` | `ToResult()`, `ToResult(combine)` | - | ❌ |
| **Try\<T\>** | `ToOption()` | `ToResult()`, `ToResult<E>(mapper)` | ❌ | ❌ | ❌ |
| **RemoteData\<T,E\>** | `ToOption()` | `ToResult()`, `ToResult(notAsked, loading)` | ❌ | ❌ | ❌ |

### Inconsistencies
- `Option` doesn't have `ToOption()` (identity)
- `Option.OkOr` is different naming from `ToResult`
- Missing `ToValidation` on most types

---

## 10. Contains / Exists Methods

| Type | Contains | ContainsError | Exists | ExistsError |
|------|----------|---------------|--------|-------------|
| **Option\<T\>** | ✅ | - | ✅ | - |
| **Result\<T,E\>** | ✅ | ✅ | ✅ | ✅ |
| **Validation\<T,E\>** | ✅ | ❌ | ✅ | ❌ |
| **Try\<T\>** | ✅ | ❌ | ✅ | ❌ |
| **RemoteData\<T,E\>** | ❌ | ❌ | ❌ | ❌ |

### Inconsistencies
- `RemoteData` missing all Contains/Exists methods
- `Validation` and `Try` missing error variants

---

## 11. Recovery Methods

| Type | Recover | RecoverWith / OrElse |
|------|---------|----------------------|
| **Option\<T\>** | ❌ | `Or(Option)`, `OrElse(Func)` |
| **Result\<T,E\>** | ❌ | `Or(Result)`, `OrElse<F>(Func)` |
| **Try\<T\>** | `Recover(Func<Ex,T>)` | `RecoverWith(Func<Ex,Try<T>>)` |
| **RemoteData\<T,E\>** | ❌ | `Or(RemoteData)`, `OrElse(Func)` |
| **IO\<T\>** | ❌ | `OrElse(IO)`, `OrElse(T)` |
| **IOAsync\<T\>** | ❌ | `OrElse(IOAsync)` |

### Inconsistency
- Only `Try` has `Recover/RecoverWith`
- Others use `Or/OrElse` but semantics differ

---

## 12. Deconstruct Support

| Type | Deconstruct 1 | Deconstruct 2 |
|------|---------------|---------------|
| **Option\<T\>** | `(T? value, bool isSome)` | - |
| **Result\<T,E\>** | `(T? value, bool isOk)` | `(T? value, E? error, bool isOk)` |
| **Validation\<T,E\>** | `(T? value, bool isValid)` | `(T? value, IReadOnlyList<E> errors, bool isValid)` |
| **Try\<T\>** | `(T? value, bool isSuccess)` | `(T? value, Exception? ex, bool isSuccess)` |
| **RemoteData\<T,E\>** | `(T? data, bool isSuccess)` | 6-tuple with all states |
| **Writer\<W,T\>** | `(T value, W log)` | - |
| **NonEmptyList\<T\>** | `(T head, IReadOnlyList<T> tail)` | - |
| **StateResult\<S,T\>** | `(T value, S state)` | - |

---

## 13. Implicit Conversions

| Type | From Value | From Error |
|------|------------|------------|
| **Option\<T\>** | `T` → `Option<T>` (handles null → None) | - |
| **Result\<T,E\>** | `T` → `Result<T,E>` (Ok) | ❌ |
| **Validation\<T,E\>** | `T` → `Validation<T,E>` (Valid) | ❌ |
| **Try\<T\>** | `T` → `Try<T>` (Success) | `Exception` → `Try<T>` (Failure) |
| **RemoteData\<T,E\>** | `T` → `RemoteData<T,E>` (Success) | ❌ |
| **NonEmptyList\<T\>** | `T` → `NonEmptyList<T>` (single) | - |

---

## 14. Async Extensions (V2.0)

| Type | MapAsync | BindAsync | Notes |
|------|----------|-----------|-------|
| **Option\<T\>** | ~~Removed~~ | ~~Removed~~ | Use standard async/await |
| **Result\<T,E\>** | ~~Removed~~ | ~~Removed~~ | Use standard async/await |
| **Validation\<T,E\>** | ✅ | - | For async validation logic |
| **Try\<T\>** | ✅ | ✅ | For exception-prone async ops |
| **RemoteData\<T,E\>** | ✅ | - | For UI state transformations |
| **IO\<T\>** | - | - | `RunAsync`, `ToAsync` |
| **IOAsync\<T\>** | ✅ | ✅ | Full async support |
| **Reader\<R,A\>** | - | - | ~~ReaderAsync removed~~ |

### V2.0 Changes
- Removed ~150 async extension methods for cleaner API
- `Option<T>` and `Result<T,E>` async extensions removed - use explicit async/await
- `ReaderAsync<R,A>` type removed entirely

---

## Summary of Key Inconsistencies for V2.0

### 1. Naming Conventions
| Concept | Current Variations | Suggested V2.0 (C#-Idiomatic) |
|---------|-------------------|-------------------------------|
| Success state | `IsSome`, `IsOk`, `IsRight`, `IsValid`, `IsSuccess` | Keep type-specific (semantic) |
| Failure state | `IsNone`, `IsErr`, `IsLeft`, `IsInvalid`, `IsFailure` | Keep type-specific (semantic) |
| Map error | `MapErr`, `MapErrors`, `MapError`, `MapLeft` | `MapError` (full word) |
| Tap failure | `TapNone`, `TapErr`, `TapLeft`, `TapErrors`, `TapFailure` | `TapError` (consistent) |
| Filter with error | `Filter`, `Ensure` | `Filter` (standard name) |
| Get value | `Unwrap`, `Get` | `GetValue` (C# style) |
| Get with default | `UnwrapOr`, `GetOrElse` | `GetValueOr` (matches BCL) |
| Get or throw | `Expect`, `GetOrThrow` | `GetOrThrow` (explicit) |
| Monadic bind | `FlatMap`, `Bind`, `AndThen` | `Bind` (LINQ convention) |

### 2. Missing APIs
- `Option`: Missing `FlatMap`, `Bind`, `BiMap`, async support
- `RemoteData`: Missing `GetOrThrow`, `Contains`, `Exists`
- `IO`: Missing `TapError`

### 3. Instance vs Extension Methods
- `Tap` is instance on some types, extension on others
- Should be consistent

### 4. Error Extraction Naming
- `UnwrapErr` vs `UnwrapErrors` vs `UnwrapError` vs `GetException` vs `UnwrapLeft`

### 5. Async Gaps
- `Option` has no async support at all
- Inconsistent async method availability across types

---

## 15. Duplicated APIs (Same Functionality, Multiple Names)

This section identifies APIs that perform the same function but exist under different names, either within the same type or across types. These should be consolidated in V2.0.

### 15.1 Monadic Bind Operation (FlatMap/Bind/AndThen)

All three names refer to the same monadic bind operation:

| Type | AndThen | FlatMap | Bind | Duplication Level |
|------|---------|---------|------|-------------------|
| **Option\<T\>** | ✅ | ❌ | ❌ | None (only 1) |
| **Result\<T,E\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |
| **Validation\<T,E\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |
| **Try\<T\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |
| **RemoteData\<T,E\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |
| **Reader\<R,A\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |
| **State\<S,T\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |
| **IO\<T\>** | ✅ | ✅ | ✅ | **HIGH - 3 aliases** |

**Recommendation**: Keep `Bind` (C#/LINQ convention, aligns with `SelectMany`).

### 15.2 Factory Methods (Pure/Return/Of)

Multiple names for creating a "successful" value:

| Type | Pure | Return | Of | Other | Duplication Level |
|------|------|--------|----|----|-------------------|
| **State\<S,A\>** | ✅ | ✅ | ✅ (Func variant) | - | **HIGH - 3 names** |
| **IO\<T\>** | ✅ | ✅ | ✅ (Func variant) | `Delay` (same as Of) | **VERY HIGH - 4 names** |
| **IOAsync\<T\>** | ✅ | ❌ | ✅ | - | Medium - 2 names |
| **Reader\<R,A\>** | ✅ | ❌ | ❌ | `From` | Medium - 2 names |
| **Writer\<W,T\>** | ❌ | ❌ | ✅ | `Tell` | Medium - 2 names |

**Recommendation**: Standardize on `Return` for value lifting (C# convention) and `Create` or `Of` for deferred/lazy creation.

### 15.3 Value Extraction (Get vs Unwrap)

| Type | Get() | Unwrap() | Same Behavior? |
|------|-------|----------|----------------|
| **Try\<T\>** | ✅ | ✅ | **YES - Duplicate** |

**Recommendation**: Keep `GetValue()` (C#-idiomatic), remove Rust-style `Unwrap()`.

### 15.4 Value Extraction with Default (GetOrElse vs UnwrapOr)

| Type | GetOrElse(T) | UnwrapOr(T) | GetOrElse(Func) | UnwrapOrElse(Func) |
|------|--------------|-------------|-----------------|-------------------|
| **Option\<T\>** | ❌ | ✅ | ❌ | ✅ |
| **Result\<T,E\>** | ❌ | ✅ | ❌ | ✅ |
| **Try\<T\>** | ✅ | ❌ | ✅ | ❌ |
| **RemoteData\<T,E\>** | ❌ | ✅ | ❌ | ✅ |

**Issue**: `Try` uses `GetOrElse` while all other types use `UnwrapOr/UnwrapOrElse`.

**V2.0 Resolution**: Standardized on `GetValueOr(T)` only. `GetValueOrElse` and `GetValueOrDefault` were removed - use `Match()` for lazy evaluation.

### 15.5 Expect vs GetOrThrow

Both patterns throw an exception with a custom message:

| Type | Expect(msg) | GetOrThrow(msg) | Same Behavior? |
|------|-------------|-----------------|----------------|
| **Option\<T\>** | ✅ | ✅ | **YES - Duplicate** |
| **Result\<T,E\>** | ✅ | ✅ | **YES - Duplicate** |
| **Validation\<T,E\>** | ✅ | ✅ | **YES - Duplicate** |
| **Try\<T\>** | ✅ | ✅ | **YES - Duplicate** |

**Recommendation**: Keep `GetOrThrow` (C#-idiomatic, explicit about behavior), remove Rust-style `Expect`.

### 15.6 Conversion Method Naming Inconsistency

Same operation, different names for Option → Result conversion:

| Type | Method 1 | Method 2 | Notes |
|------|----------|----------|-------|
| **Option\<T\>** | `OkOr(E)` | `OkOrElse(Func<E>)` | Different pattern from others |
| **Other types** | `ToResult()` | - | Standard naming |

**Issue**: `Option` uses `OkOr` instead of `ToResult` pattern.

**Recommendation**: Add `ToResult(E)` / `ToResult(Func<E>)` to `Option` for consistency.

### 15.7 IO Delay vs Of

| Method | Behavior |
|--------|----------|
| `IO.Of(Func<T>)` | Creates deferred IO computation |
| `IO.Delay(Func<T>)` | Creates deferred IO computation |

**These are identical.** `Delay` is just an alias for `Of`.

**Recommendation**: Remove `Delay`, keep `Of`.

---

## 16. Summary: APIs to Remove/Consolidate in V2.0

### Remove These Duplicates (C#-Idiomatic Naming)

| Type | Remove | Keep | Reason |
|------|--------|------|--------|
| All types | `FlatMap`, `AndThen` | `Bind` | C#/LINQ convention (SelectMany) |
| State, IO | `Pure` | `Return` | C# convention |
| IO | `Delay` | `Of` | Redundant alias |
| All types | `Unwrap()` | `GetValue()` | C#-idiomatic naming |
| All types | `UnwrapOr/UnwrapOrElse` | `GetValueOr` | ~~GetValueOrElse removed~~ - use `Match()` |
| All types | `Expect(msg)` | `GetOrThrow(msg)` | C#-idiomatic, explicit |
| All types | `UnwrapErr`, `UnwrapError` | `GetError()` | C#-idiomatic naming |
| All types | `MapErr` | `MapError` | Full word, C# style |

### Total Duplicate Methods to Remove

| Category | Count | Impact |
|----------|-------|--------|
| FlatMap/AndThen → Bind consolidation | ~16 methods | Major cleanup |
| Pure → Return consolidation | ~4 methods | Medium cleanup |
| Unwrap → GetValue | ~6 methods | Medium cleanup |
| UnwrapOr → GetValueOr alignment | ~8 methods | Medium cleanup |
| Expect → GetOrThrow | ~8 methods | Medium cleanup |
| MapErr → MapError | ~3 methods | Minor cleanup |
| **Total** | **~45 methods** | Significant API reduction |

### Alternative: Keep Both with Clear Documentation

If removing methods is too breaking, consider:
1. Mark duplicates as `[Obsolete]` in V2.0
2. Remove in V3.0
3. Document the "preferred" method clearly

---

## 17. V2.0 Changes Implemented

### 1. Monadic Bind Consolidation

| Type | Methods Removed | Method Kept |
|------|-----------------|-------------|
| All types | `FlatMap`, `AndThen` | `Bind` |
| All types | `FlatMapAsync`, `AndThenAsync` | `BindAsync` |

### 2. Value Extraction Renamed (Rust-style → C#-style)

| Old Name (Removed) | New Name (Kept) | Types |
|-------------------|-----------------|-------|
| `Unwrap()` | `GetValue()` | Option, Result, Validation, Try, RemoteData |
| `Get()` | `GetValue()` | Try (was duplicate) |
| `UnwrapOr(T)` | `GetValueOr(T)` | Option, Result, Validation, Try, RemoteData |
| `UnwrapOrElse(Func)` | ~~Removed~~ | Use `Match()` for lazy evaluation |
| `GetOrElse(T)` | `GetValueOr(T)` | Try (naming alignment) |
| `GetOrElse(Func<T>)` | ~~Removed~~ | Use `Match()` for lazy evaluation |
| `GetOrElse(Func<Ex,T>)` | ~~Removed~~ | Use `Match()` for recovery |
| `UnwrapOrDefault()` | ~~Removed~~ | Use `GetValueOr(default)` |
| `Expect(msg)` | ~~Removed~~ | Use `GetOrThrow()` (provides good default message) |
| `UnwrapOrElseAsync` | ~~Removed~~ | Async extensions simplified in v2.0 |

### 3. Error Extraction Renamed

| Old Name (Removed) | New Name (Kept) | Types |
|-------------------|-----------------|-------|
| `UnwrapErr()` | `GetError()` | Result, RemoteData |
| `UnwrapErrors()` | `GetErrors()` | Validation |
| `ExpectErr(msg)` | `GetErrorOrThrow(msg)` | Result |
| `ExpectErrors(msg)` | `GetErrorsOrThrow(msg)` | Validation |

### 4. Pure → Return Consolidation

| Type | Removed | Kept |
|------|---------|------|
| State | `Pure` | `Return` |
| IO | `Pure`, `Delay` | `Return`, `Of` |
| IOAsync | `Pure` | `Return` |
| Reader | `Pure` | `Return` |
| ~~ReaderAsync~~ | ~~Removed~~ | Type removed in v2.0 |
| StringWriter | `Pure` | `Return` |
| ListWriter | `Pure` | `Return` |
| ReaderExtensions | `Pure` | `Return` |

### 6. Error Mapping Renamed

| Old Name (Removed) | New Name (Kept) | Types |
|-------------------|-----------------|-------|
| `MapErr` | `MapError` | Result |
| `MapErrAsync` | `MapErrorAsync` | ResultAsync |

### 7. Tap Alias Consolidation

| Old Name (Removed) | New Name (Kept) | Type |
|-------------------|-----------------|------|
| `TapInvalid` | `TapErrors` | ValidationExtensions |
| `TapError` | `TapFailure` | RemoteDataExtensions |

### 8. Analyzer Updates

| Old Name | New Name |
|----------|----------|
| `FlatMapToMapAnalyzer` | `BindToMapAnalyzer` |
| `DiagnosticDescriptors.FlatMapToMap` | `DiagnosticDescriptors.BindToMap` |

### Impact Summary

- **~50+ duplicate/redundant methods removed** across all monad types
- **All tests updated** to use new C#-idiomatic naming
- **All examples updated** to use the new naming
- **Analyzers updated** to detect `Bind` patterns
- **All 2,042 tests pass** (1,968 core + 33 source generators + 25 EF Core + 16 analyzers)

---

## 18. V2.0 C#-Idiomatic API Summary

### Chosen Naming Conventions

| Concept | Rust/FP Style (Remove) | C# Style (Keep) | Rationale |
|---------|------------------------|-----------------|-----------|
| Monadic bind | `FlatMap`, `AndThen` | `Bind` | Aligns with LINQ's `SelectMany` |
| Value lifting | `Pure` | `Return` | C# convention |
| Get value (throws) | `Unwrap()` | `GetValue()` | Explicit, discoverable |
| Get value or default | `UnwrapOr(T)` | `GetValueOr(T)` | Direct value fallback |
| Get value or compute | `UnwrapOrElse(Func)` | `Match()` | Lazy evaluation via pattern matching |
| Get with message | `Expect(msg)` | `GetOrThrow()` | Auto-generates descriptive message |
| Get error | `UnwrapErr()`, `UnwrapError()` | `GetError()` | Clean, consistent |
| Map error | `MapErr` | `MapError` | Full word, readable |

### C# Naming Principles Applied

1. **Use full words** - `GetError` not `GetErr`, `MapError` not `MapErr`
2. **Explicit behavior in name** - `GetOrThrow` makes it clear an exception is thrown
3. **Match BCL patterns** - `TryGet`, `GetValueOr` patterns
4. **Verb-Noun structure** - `GetValue`, `GetError`, `MapError`
5. **Consistent prefixes** - All value extraction starts with `Get`

### Implemented V2.0 Core API (Per Type)

```csharp
// Value extraction
GetValue()                          // throws if not success
GetValueOr(T default)               // returns default if not success  
Match(ok, err)                      // lazy evaluation via pattern matching
GetOrThrow()                        // throws with descriptive message
TryGet(out T value)                 // C# idiomatic pattern

// Error extraction (for types with errors)
GetError()                          // throws if success
GetErrors()                         // for Validation (returns IReadOnlyList<E>)
GetException()                      // for Try (returns Exception)
GetErrorOrThrow(msg)                // throws with custom message

// TryGet pattern (C# idiomatic)
TryGet(out T?)                      // returns bool
TryGetError(out E?)                 // returns bool

// Transformation
Map(Func<T,U>)                      // transform value
MapError(Func<E,F>)                 // transform error
Bind(Func<T,M<U>>)                  // monadic bind

// Side effects
Tap(Action<T>)                      // side effect on value
TapFailure(Action<E>)               // side effect on error (RemoteData, Try)
TapErrors(Action<IReadOnlyList<E>>) // side effect on errors (Validation)
TapErr(Action<E>)                   // side effect on error (Result)

// Matching
Match(onSuccess, onFailure)         // pattern match

// Factory methods (for computational types)
Return(T)                           // lift value into monad
Of(Func<T>)                         // deferred computation
```
