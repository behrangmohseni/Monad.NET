# Type Selection Guide

This guide helps you choose the right Monad.NET type for your specific use case. Use the decision flowcharts and comparison tables to make the best choice.

## Table of Contents

- [Quick Decision Flowchart](#quick-decision-flowchart)
- [Detailed Type Comparison](#detailed-type-comparison)
- [Common Scenarios](#common-scenarios)
- [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
- [Decision Trees by Domain](#decision-trees-by-domain)

---

## Quick Decision Flowchart

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     What problem are you solving?                        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ Value might   │         │ Operation might │         │ Need to handle  │
│ not exist     │         │ fail            │         │ side effects    │
└───────────────┘         └─────────────────┘         └─────────────────┘
        │                           │                           │
        ▼                           │                           ▼
┌───────────────┐                   │                 ┌─────────────────┐
│  Option<T>    │                   │                 │     IO<T>       │
└───────────────┘                   │                 └─────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ Single error  │         │ Multiple errors │         │ External code   │
│ short-circuit │         │ accumulated     │         │ throws          │
└───────────────┘         └─────────────────┘         └─────────────────┘
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ Result<T,E>   │         │ Validation<T,E> │         │    Try<T>       │
└───────────────┘         └─────────────────┘         └─────────────────┘
```

---

## Detailed Type Comparison

### Core Error Handling Types

| Question | Result<T,E> | Validation<T,E> | Try<T> |
|----------|-------------|-----------------|--------|
| **When to use?** | Expected failures | Show ALL validation errors | Wrap throwing code |
| **Error behavior?** | Short-circuits on first | Accumulates all | Captures exception |
| **Error type?** | Custom type `E` | Custom type `E` | `Exception` |
| **Performance?** | Zero allocations | Allocates error list | Captures stack trace |
| **Async support?** | Yes (`ResultAsync`) | Yes | Yes |

```
Choose Result<T,E> when:
  ✓ An operation can fail in expected ways
  ✓ You want typed errors in the signature
  ✓ First error is enough to stop processing
  ✓ Examples: API calls, database operations, business rules

Choose Validation<T,E> when:
  ✓ You need to collect ALL errors
  ✓ User needs to see everything wrong at once
  ✓ Form/input validation scenarios
  ✓ Examples: Registration forms, data import, config validation

Choose Try<T> when:
  ✓ Wrapping code that throws exceptions
  ✓ You can't change the throwing code
  ✓ You want to convert exceptions to values
  ✓ Examples: Parsing, file I/O, legacy library calls
```

### Optional Value Types

| Question | Option<T> | NonEmptyList<T> | RemoteData<T,E> |
|----------|-----------|-----------------|-----------------|
| **Represents?** | 0 or 1 value | 1+ values | Async data state |
| **When empty?** | `None` | Not possible! | `NotAsked`/`Loading` |
| **Use case?** | Nullable replacement | Guaranteed elements | UI state |

```
Choose Option<T> when:
  ✓ A value might not exist
  ✓ Replacing null/Nullable<T>
  ✓ Optional parameters or return values
  ✓ Examples: Dictionary lookups, find operations, optional fields

Choose NonEmptyList<T> when:
  ✓ Empty list would be invalid
  ✓ You need guaranteed .First() / .Head access
  ✓ Reduce operations without seed
  ✓ Examples: Config items, selected options, validation errors

Choose RemoteData<T,E> when:
  ✓ Building UI for async data
  ✓ Need to track NotAsked/Loading/Success/Failure
  ✓ Blazor, WPF, or similar UI frameworks
  ✓ Examples: API data loading, search results, user profiles
```

### Composition Types

| Question | Either<L,R> | Reader<R,A> | Writer<W,T> | State<S,A> |
|----------|-------------|-------------|-------------|------------|
| **Purpose?** | Two alternatives | Dependency injection | Logging/traces | Threading state |
| **Biased?** | Right-biased | N/A | N/A | N/A |
| **Use case?** | Different outcomes | Shared environment | Audit trails | Pure state |

```
Choose Either<L,R> when:
  ✓ Both outcomes are valid (not just success/failure)
  ✓ You need to work with "left" side too
  ✓ Return different types based on conditions
  ✓ Examples: Union of types, A-or-B returns

Choose Reader<R,A> when:
  ✓ Passing configuration/services through call chains
  ✓ Avoiding parameter drilling
  ✓ Functional dependency injection
  ✓ Examples: Database connection, logging, configuration

Choose Writer<W,T> when:
  ✓ Accumulating logs alongside computation
  ✓ Building audit trails
  ✓ Tracing computation steps
  ✓ Examples: Debug logging, metrics collection

Choose State<S,A> when:
  ✓ Threading state through pure computations
  ✓ Avoiding mutable variables
  ✓ State machine implementations
  ✓ Examples: Parsers, interpreters, game state
```

---

## Common Scenarios

### Scenario 1: API Response Handling

```
Q: What type of failure are you handling?
│
├─► Single endpoint, one error → Result<T, ApiError>
│   
├─► Need to validate request → Validation<T, ValidationError>
│   
└─► UI loading state → RemoteData<T, ApiError>
```

**Example:**
```csharp
// API call that might fail
public Result<User, ApiError> GetUser(int id);

// Validating input before API call
public Validation<CreateUserRequest, ValidationError> ValidateRequest(UserForm form);

// UI component state
public RemoteData<User, ApiError> UserData { get; set; }
```

### Scenario 2: Database Operations

```
Q: What's the nature of the operation?
│
├─► Record might not exist → Option<T>
│   
├─► Operation might fail → Result<T, DbError>
│   
├─► Need transaction logging → Writer<AuditLog, T>
│   
└─► Need connection context → Reader<DbContext, T>
```

**Example:**
```csharp
// Find operation (might not exist)
public Option<User> FindById(int id);

// Save operation (might fail)
public Result<Unit, DbError> Save(User user);

// Operation with audit trail
public Writer<List<AuditEntry>, User> UpdateWithAudit(User user);
```

### Scenario 3: Input Validation

```
Q: How should validation errors be reported?
│
├─► Show first error only → Result<T, ValidationError>
│   
├─► Show ALL errors → Validation<T, ValidationError>
│   
└─► Validate optional field → Option<T>.Filter(predicate)
```

**Example:**
```csharp
// Fail-fast validation
public Result<User, string> QuickValidate(UserForm form)
{
    if (string.IsNullOrEmpty(form.Name))
        return Result<User, string>.Err("Name required");
    // Stops here if name is empty
    
    if (!form.Email.Contains("@"))
        return Result<User, string>.Err("Invalid email");
    
    return Result<User, string>.Ok(new User(form.Name, form.Email));
}

// Accumulating validation (shows ALL errors)
public Validation<User, ValidationError> FullValidate(UserForm form)
{
    return ValidateName(form.Name)
        .Apply(ValidateEmail(form.Email), (name, email) => (name, email))
        .Apply(ValidateAge(form.Age), (partial, age) => 
            new User(partial.name, partial.email, age));
    // Returns all three errors if all fail
}
```

### Scenario 4: External Library Integration

```
Q: Does the external code throw exceptions?
│
├─► Yes, I need to wrap it → Try<T>
│   
├─► No, but returns null → .ToOption() extension
│   
└─► Returns error codes → Result<T, E>
```

**Example:**
```csharp
// Wrapping exception-throwing code
var parsed = Try<int>.Of(() => int.Parse(input));

// Converting nullable to Option
var user = externalApi.FindUser(id).ToOption();

// Mapping error codes to Result
public Result<Data, ApiError> CallExternalApi()
{
    var response = externalApi.Call();
    return response.StatusCode switch
    {
        200 => Result<Data, ApiError>.Ok(response.Data),
        404 => Result<Data, ApiError>.Err(ApiError.NotFound),
        _ => Result<Data, ApiError>.Err(ApiError.Unknown)
    };
}
```

---

## Anti-Patterns to Avoid

### Don't: Use Result for truly exceptional errors

```csharp
// Bad: OutOfMemory is exceptional, not expected
public Result<Data, OutOfMemoryException> ProcessLargeFile();

// Good: Use exceptions for exceptional cases
public Data ProcessLargeFile(); // Throws OutOfMemoryException if it happens
```

### Don't: Use Try when you can use Result

```csharp
// Bad: Unnecessary exception wrapping
public Try<User> GetUser(int id)
{
    return Try<User>.Of(() => {
        if (id <= 0) throw new ArgumentException();
        return _db.Find(id) ?? throw new NotFoundException();
    });
}

// Good: Return Result directly
public Result<User, UserError> GetUser(int id)
{
    if (id <= 0) return Result<User, UserError>.Err(UserError.InvalidId);
    return _db.Find(id).ToOption()
        .ToResult(() => UserError.NotFound);
}
```

### Don't: Use Either when you mean Result

```csharp
// Bad: Using Either for error handling (confusing semantics)
public Either<Error, User> GetUser(int id);

// Good: Use Result for success/failure semantics
public Result<User, Error> GetUser(int id);

// Good: Use Either when both sides are valid outcomes
public Either<CachedUser, FreshUser> GetUser(int id);
```

### Don't: Overuse Validation for single checks

```csharp
// Bad: Validation for single field with no accumulation benefit
public Validation<string, Error> ValidateSingleField(string input);

// Good: Use Result for single validation
public Result<string, Error> ValidateSingleField(string input);

// Good: Use Validation when combining multiple checks
public Validation<User, Error> ValidateAllFields(Form form);
```

### Don't: Nest Options/Results unnecessarily

```csharp
// Bad: Nested Option
public Option<Option<User>> GetUserProfile(int id);

// Good: Flatten with Bind
public Option<User> GetUserProfile(int id)
{
    return FindUser(id).Bind(user => user.GetProfile());
}
```

---

## Decision Trees by Domain

### Web API Development

```
Incoming Request
       │
       ├─► Validate request body → Validation<T, ValidationError>
       │
       ├─► Query database
       │   ├─► Record exists? → Option<T> or Result<T, NotFound>
       │   └─► Operation failed? → Result<T, DbError>
       │
       ├─► Call external service → Result<T, ApiError>
       │
       └─► Return response
           ├─► Success → Ok(data)
           ├─► Not found → NotFound()
           └─► Validation errors → BadRequest(errors)
```

### Domain-Driven Design

```
Domain Operation
       │
       ├─► Aggregate might not exist → Option<Aggregate>
       │
       ├─► Business rule validation → Result<T, DomainError>
       │
       ├─► Command handling → Result<Event[], CommandError>
       │
       └─► Event sourcing → Writer<List<Event>, State>
```

### UI/Blazor Applications

```
Component Data
       │
       ├─► Async data loading → RemoteData<T, Error>
       │
       ├─► Form validation → Validation<T, FieldError>
       │
       ├─► Optional user input → Option<T>
       │
       └─► Selected items (must have 1+) → NonEmptyList<T>
```

---

## Summary Table

| I want to... | Use this type |
|--------------|---------------|
| Handle optional values | `Option<T>` |
| Handle expected failures | `Result<T, E>` |
| Show ALL validation errors | `Validation<T, E>` |
| Wrap throwing code | `Try<T>` |
| Represent two valid alternatives | `Either<L, R>` |
| Model async UI state | `RemoteData<T, E>` |
| Guarantee non-empty collection | `NonEmptyList<T>` |
| Thread state through pure functions | `State<S, A>` |
| Pass shared environment/config | `Reader<R, A>` |
| Accumulate logs/traces | `Writer<W, T>` |
| Defer side effects | `IO<T>` |
| Define custom union types | `[Union]` attribute |

---

## Nuanced Type Selection Guide

The table above provides a quick reference, but real-world scenarios often require more nuanced decisions. Use this detailed guide when you need precise type selection.

### Error Handling Decision Matrix

| Scenario | Type | Why |
|----------|------|-----|
| An operation can fail with a typed error **AND** I want to short-circuit on first error | `Result<T, E>` | Fail-fast semantics; stops processing immediately on error |
| An operation can fail **AND** I want to accumulate ALL errors | `Validation<T, E>` | Applicative semantics; collects all errors before failing |
| I'm wrapping legacy code that throws exceptions | `Try<T>` | Converts exception-based APIs to value-based error handling |
| I need to defer error handling decisions to the caller | `Either<L, R>` | Both sides are equally valid; no inherent success/failure semantics |
| Error handling AND tracking async loading state (UI) | `RemoteData<T, E>` | Adds `NotAsked` and `Loading` states for UI rendering |

### Detailed Comparison: Result vs Validation vs Try

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Which Error Type Should I Use?                         │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
                    ┌───────────────────────────────┐
                    │ Does the code throw exceptions │
                    │ that I cannot modify?          │
                    └───────────────────────────────┘
                           │                │
                          YES              NO
                           │                │
                           ▼                ▼
                    ┌─────────────┐  ┌─────────────────────────────┐
                    │   Try<T>    │  │ Do I need ALL errors        │
                    │             │  │ reported at once?           │
                    └─────────────┘  └─────────────────────────────┘
                                           │                │
                                          YES              NO
                                           │                │
                                           ▼                ▼
                                    ┌──────────────┐  ┌──────────────┐
                                    │Validation<T,E│  │ Result<T,E>  │
                                    └──────────────┘  └──────────────┘
```

### When to Use Each Error Type

#### Use `Result<T, E>` when:
```
✓ Business logic validation where first error is sufficient
✓ API calls where you stop on first failure
✓ Database operations with single-point-of-failure
✓ Pipeline operations where subsequent steps depend on previous success
✓ You want maximum performance (zero allocation)

Example scenarios:
  - Authenticating a user (wrong password = stop)
  - Processing a payment (declined = stop)
  - Loading a configuration file (invalid = stop)
```

#### Use `Validation<T, E>` when:
```
✓ Form validation where users need to see all problems
✓ Data import validation where you want a complete error report
✓ Configuration validation at startup
✓ Any scenario where fixing one error might reveal more
✓ Combining multiple independent validations

Example scenarios:
  - User registration form (show all invalid fields)
  - CSV import (report all bad rows)
  - API request validation (return all constraint violations)
```

#### Use `Try<T>` when:
```
✓ Wrapping third-party libraries that throw
✓ Parsing operations (int.Parse, DateTime.Parse)
✓ File I/O operations with potential exceptions
✓ Network calls to legacy HTTP clients
✓ Any code you can't modify that uses exceptions

Example scenarios:
  - Parsing user input: Try<int>.Of(() => int.Parse(input))
  - Reading files: Try<string>.Of(() => File.ReadAllText(path))
  - Legacy library: Try<Data>.Of(() => legacyApi.GetData())
```

#### Use `Either<L, R>` when:
```
✓ Both outcomes are valid (not success/failure)
✓ You need to process both sides equally
✓ Return type can be genuinely one of two things
✓ Error handling decision should be made by caller
✓ Modeling union types without the [Union] attribute

Example scenarios:
  - CachedData | FreshData (both valid, different handling)
  - TextResponse | BinaryResponse (different processing)
  - LocalUser | RemoteUser (different capabilities)
```

### Optional Value Decision Guide

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    How Should I Model "Maybe" Values?                        │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
                    ▼               ▼               ▼
             ┌───────────┐  ┌─────────────┐  ┌──────────────┐
             │ Zero or   │  │ One or more │  │ Async with   │
             │ one value │  │ values      │  │ loading state│
             └───────────┘  └─────────────┘  └──────────────┘
                    │               │               │
                    ▼               ▼               ▼
             ┌───────────┐  ┌─────────────┐  ┌──────────────┐
             │ Option<T> │  │NonEmptyList │  │RemoteData<T,E│
             └───────────┘  └─────────────┘  └──────────────┘
```

---

[← Back to Documentation](../../README.md) | [Migration Guide →](MigrationGuide.md)

