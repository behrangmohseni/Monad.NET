# Async Patterns Guide

This guide explains how to use Monad.NET types with async/await in v2.0.

## Background: Why v2.0 Changed Async

In v1.x, every monadic type had async variants (`MapAsync`, `BindAsync`, `MatchAsync`) — about 150 extra methods. This caused problems:

1. **API bloat** — IntelliSense was cluttered with duplicate methods
2. **Confusion** — Multiple ways to do the same thing
3. **Anti-patterns** — Users wrapped sync code in unnecessary async
4. **Maintenance burden** — Every sync method needed an async twin

v2.0 takes a **selective approach**: async extensions only where they provide genuine value.

---

## Async Support by Type

| Type | Async Support | Recommendation |
|------|--------------|----------------|
| `Option<T>` | None | Use `await` + sync methods, or `Match` with async lambdas |
| `Result<T,E>` | None | Use `await` + sync methods, or `Match` with async lambdas |
| `Try<T>` | `MapAsync`, `BindAsync` | Full support for async exception handling |
| `Validation<T,E>` | `MapAsync` | For async validation logic |
| `RemoteData<T,E>` | `MapAsync` | For UI state transformations |
| `IO<T>` | `IOAsync<T>`, `ToAsync()` | Full async support via dedicated type |

---

## Patterns for Option\<T\>

### Pattern 1: Match with Async Lambdas

```csharp
Option<int> userId = GetUserId();

// ✅ Use Match when you need different async behavior per case
User? result = await userId.Match(
    some: async id => await _userService.GetUserAsync(id),
    none: () => Task.FromResult<User?>(null)
);
```

### Pattern 2: TryGet + Conditional Await

```csharp
Option<int> userId = GetUserId();

// ✅ Use TryGet for simple conditional async
if (userId.TryGet(out var id))
{
    var user = await _userService.GetUserAsync(id);
    await _emailService.SendWelcomeAsync(user.Email);
}
```

### Pattern 3: Convert to Async Task First

```csharp
// ✅ When the Option itself comes from async
Option<User> userOption = await GetUserAsync(id).ToOption();

// Then use sync methods
var email = userOption
    .Filter(u => u.IsActive)
    .Map(u => u.Email);
```

### Anti-Pattern: Don't Do This

```csharp
// ❌ BAD: Wrapping sync operations in Task.Run
var result = await Task.Run(() => option.Map(x => x * 2));

// ❌ BAD: Creating unnecessary async wrappers
async Task<Option<T>> MapAsyncWrapper<T, U>(Option<T> opt, Func<T, Task<U>> f)
{
    // Don't recreate what was removed!
}
```

---

## Patterns for Result\<T, E\>

### Pattern 1: Sequential Async Pipeline

```csharp
// When each step depends on the previous
public async Task<Result<Order, OrderError>> ProcessOrderAsync(OrderRequest request)
{
    // Step 1: Validate (sync)
    var validated = ValidateRequest(request);
    if (validated.IsErr) return validated.MapError(e => (OrderError)e);
    
    // Step 2: Check inventory (async)
    var inventory = await CheckInventoryAsync(validated.GetValue());
    if (inventory.IsErr) return inventory;
    
    // Step 3: Charge payment (async)
    var payment = await ChargePaymentAsync(inventory.GetValue());
    if (payment.IsErr) return payment;
    
    // Step 4: Create order (async)
    return await CreateOrderAsync(payment.GetValue());
}
```

### Pattern 2: Using Match for Async Branching

```csharp
Result<int, string> result = GetResult();

// ✅ Different async operations based on success/failure
await result.Match(
    ok: async value => await ProcessSuccessAsync(value),
    err: async error => await LogErrorAsync(error)
);
```

### Pattern 3: Parallel Async with Results

```csharp
// Run multiple async operations, collect results
var tasks = orderIds.Select(async id => 
{
    try
    {
        var order = await _orderService.GetOrderAsync(id);
        return Result<Order, string>.Ok(order);
    }
    catch (Exception ex)
    {
        return Result<Order, string>.Error(ex.Message);
    }
});

var results = await Task.WhenAll(tasks);
var (successes, failures) = results.Partition();
```

---

## Patterns for Try\<T\>

`Try<T>` retains async support because it's designed for wrapping exception-prone code:

```csharp
// ✅ MapAsync for transforming async results
var result = await Try<string>.OfAsync(() => httpClient.GetStringAsync(url))
    .MapAsync(async json => await ParseAsync(json));

// ✅ BindAsync for chaining async Try operations
var data = await Try<Config>.OfAsync(() => LoadConfigAsync())
    .BindAsync(async config => await Try<Data>.OfAsync(() => FetchDataAsync(config)));

// ✅ Recovery patterns
var value = await Try<int>.OfAsync(() => primaryService.GetValueAsync())
    .RecoverWith(ex => Try<int>.OfAsync(() => fallbackService.GetValueAsync()));
```

---

## Patterns for Validation\<T, E\>

`Validation<T,E>` has `MapAsync` for async validation logic:

```csharp
// ✅ Async validation that checks a database
Validation<string, ValidationError> email = ValidateEmailFormat(input);

var validated = await email.MapAsync(async e => 
{
    var exists = await _userRepo.EmailExistsAsync(e);
    return exists ? e : throw new Exception("Email not registered");
});
```

### Combining Async Validations

```csharp
// Run validations in parallel, then combine
var nameTask = ValidateNameAsync(form.Name);
var emailTask = ValidateEmailAsync(form.Email);
var ageTask = ValidateAgeAsync(form.Age);

await Task.WhenAll(nameTask, emailTask, ageTask);

var result = (await nameTask)
    .Apply(await emailTask, (n, e) => (n, e))
    .Apply(await ageTask, (pair, a) => new User(pair.n, pair.e, a));
```

---

## Patterns for IO\<T\>

`IO<T>` has full async support via `IOAsync<T>`:

```csharp
// ✅ Create async IO operations
var fetchUser = IOAsync<User>.Of(async () => 
    await _httpClient.GetFromJsonAsync<User>(url));

// ✅ Compose async IO
var program = fetchUser
    .Bind(user => IOAsync<Order>.Of(async () => 
        await _orderService.GetLatestOrderAsync(user.Id)))
    .Map(order => order.Total);

// ✅ Execute
var total = await program.RunAsync();

// ✅ Convert sync IO to async
var syncIO = IO<int>.Of(() => ComputeValue());
var asyncIO = syncIO.ToAsync();
```

---

## Common Mistakes

### Mistake 1: Blocking on Async

```csharp
// ❌ BAD: .Result or .Wait() causes deadlocks
var result = option.Match(
    some: id => _service.GetUserAsync(id).Result,  // DEADLOCK RISK
    none: () => null
);

// ✅ GOOD: Properly await
var result = await option.Match(
    some: async id => await _service.GetUserAsync(id),
    none: () => Task.FromResult<User?>(null)
);
```

### Mistake 2: Fire-and-Forget

```csharp
// ❌ BAD: Async operation not awaited
option.Match(
    some: async id => await SendEmailAsync(id),  // Not awaited!
    none: () => { }
);

// ✅ GOOD: Await the Match
await option.Match(
    some: async id => { await SendEmailAsync(id); },
    none: () => Task.CompletedTask
);
```

### Mistake 3: Unnecessary Async Wrappers

```csharp
// ❌ BAD: Wrapping sync code in async
var result = await Task.FromResult(option.Map(x => x * 2));

// ✅ GOOD: Just use sync
var result = option.Map(x => x * 2);
```

---

## Decision Guide

```
Is your operation async?
│
├─► NO: Use sync methods (Map, Bind, Match)
│
└─► YES: What type are you using?
    │
    ├─► Try<T>: Use MapAsync, BindAsync
    │
    ├─► IO<T>: Use IOAsync<T> 
    │
    ├─► Validation<T,E>: Use MapAsync
    │
    └─► Option<T> or Result<T,E>:
        │
        ├─► Simple check + await: Use TryGet or IsOk + await
        │
        └─► Different async per branch: Use Match with async lambdas
```

---

## Migration from v1.x

| v1.x Pattern | v2.0 Equivalent |
|--------------|-----------------|
| `option.MapAsync(f)` | `option.Match(some: f, none: () => Task.FromResult(default))` |
| `result.BindAsync(f)` | Check `IsOk`, then `await f(value)`, or use `Match` |
| `option.MatchAsync(s, n)` | `await option.Match(s, n)` |

---

## Summary

1. **Option/Result**: Use `Match` with async lambdas or `TryGet` + conditional await
2. **Try**: Full async support with `MapAsync`, `BindAsync`
3. **Validation**: Use `MapAsync` for async validation
4. **IO**: Use `IOAsync<T>` for full async composition
5. **Always await** — don't use `.Result` or `.Wait()`
6. **Keep it simple** — don't create unnecessary async wrappers

---

[← Pitfalls](Pitfalls.md) | [Back to Guides](../README.md)
