# Quick Start Guide

Get started with Monad.NET in 5 minutes!

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Monad.NET
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package Monad.NET
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Monad.NET" />
```

**Supported Frameworks:** .NET 6.0, 7.0, 8.0, 9.0, 10.0+

## Basic Usage

Add the namespace:

```csharp
using Monad.NET;
```

### Option - Handle Missing Values

```csharp
// Instead of null checks
string? name = GetUserName();
if (name is not null)
{
    Console.WriteLine(name.ToUpper());
}

// Use Option
var name = GetUserName().ToOption();
name.Match(
    someFunc: n => Console.WriteLine(n.ToUpper()),
    noneFunc: () => Console.WriteLine("No name")
);

// Or chain operations
var result = GetUserName().ToOption()
    .Map(n => n.ToUpper())
    .UnwrapOr("Anonymous");
```

### Result - Handle Errors

```csharp
// Instead of try/catch
try
{
    var value = int.Parse(input);
    Process(value);
}
catch (Exception ex)
{
    HandleError(ex);
}

// Use Result
var result = ResultExtensions.Try(() => int.Parse(input));
result.Match(
    okAction: value => Process(value),
    errAction: ex => HandleError(ex)
);

// Or chain operations
var output = ResultExtensions.Try(() => int.Parse(input))
    .Map(x => x * 2)
    .AndThen(x => Validate(x))
    .UnwrapOr(0);
```

### Validation - Collect All Errors

```csharp
// Validate a form and show ALL errors at once
var nameResult = ValidateName(form.Name);
var emailResult = ValidateEmail(form.Email);
var ageResult = ValidateAge(form.Age);

var user = nameResult
    .Apply(emailResult, (name, email) => (name, email))
    .Apply(ageResult, (partial, age) => new User(partial.name, partial.email, age));

user.Match(
    validAction: u => SaveUser(u),
    invalidAction: errors => ShowErrors(errors)  // Shows ALL validation errors!
);
```

### Try - Capture Exceptions

```csharp
// Safely parse with recovery
var value = Try<int>.Of(() => int.Parse(userInput))
    .GetOrElse(0);

// Chain with recovery
var result = Try<int>.Of(() => int.Parse(input))
    .Map(x => x * 2)
    .Recover(ex => -1);
```

### RemoteData - Track Loading States

```csharp
// Perfect for UI state management
RemoteData<User, string> userData = RemoteData<User, string>.NotAsked();

// Render based on state
var ui = userData.Match(
    notAskedFunc: () => RenderLoadButton(),
    loadingFunc: () => RenderSpinner(),
    successFunc: user => RenderUser(user),
    failureFunc: error => RenderError(error)
);
```

### State - Thread State Through Computations

```csharp
// Counter without mutable variables
var increment = State<int, Unit>.Modify(s => s + 1);
var getCount = State<int, int>.Get();

var computation = 
    from _ in increment
    from __ in increment
    from count in getCount
    select count;

var (value, finalState) = computation.Run(0);
// value = 2, finalState = 2
```

## Common Patterns

### Railway-Oriented Programming

Chain operations that might fail:

```csharp
var result = ParseInput(data)
    .AndThen(Validate)
    .AndThen(Transform)
    .AndThen(Save)
    .Tap(x => Log($"Success: {x}"))
    .TapErr(e => Log($"Error: {e}"));
```

### LINQ Query Syntax

```csharp
var result = from x in Option<int>.Some(10)
             from y in Option<int>.Some(20)
             where x > 5
             select x + y;
// Some(30)
```

### Async Operations

```csharp
var result = await Option<int>.Some(42)
    .MapAsync(async x => await ProcessAsync(x));
```

## Source Generators

Create type-safe discriminated unions with auto-generated `Match` methods:

```bash
dotnet add package Monad.NET.SourceGenerators
```

```csharp
using Monad.NET;

[Union]
public abstract partial record PaymentMethod
{
    public partial record CreditCard(string Number, string Expiry) : PaymentMethod;
    public partial record PayPal(string Email) : PaymentMethod;
    public partial record BankTransfer(string AccountNumber) : PaymentMethod;
}

// Exhaustive pattern matching - compiler error if case is missing
string Describe(PaymentMethod method) => method.Match(
    creditCard: cc => $"Card ending in {cc.Number[^4..]}",
    payPal: pp => $"PayPal: {pp.Email}",
    bankTransfer: bt => $"Account: {bt.AccountNumber}"
);
```

## Entity Framework Core Integration

Use `Option<T>` as entity properties with seamless database mapping:

```bash
dotnet add package Monad.NET.EntityFrameworkCore
```

```csharp
using Monad.NET;
using Monad.NET.EntityFrameworkCore;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Option<string> Email { get; set; }
    public Option<int> Age { get; set; }
}

// Query with Option-returning methods
var user = await context.Users.FirstOrNoneAsync(u => u.Id == id);
user.Match(
    someFunc: u => Console.WriteLine($"Found: {u.Name}"),
    noneFunc: () => Console.WriteLine("User not found")
);

// Available methods: FirstOrNone, SingleOrNone, ElementAtOrNone, LastOrNone
// All have async variants
```

## Next Steps

- Read the full [README](../README.md) for detailed documentation
- Check out the [examples](../examples/Monad.NET.Examples/Program.cs)
- See the [API reference](API.md) for all available methods

---

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni)

**License:** MIT
