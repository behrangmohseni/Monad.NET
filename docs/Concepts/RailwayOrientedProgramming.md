# Railway-Oriented Programming

> **For C# developers:** This guide explains the "railway" mental model that makes functional error handling intuitive.

## The Railway Metaphor

Imagine your code as a railway track. Data flows along the track like a train.

```
[Input] ───────────────────────────────────────────> [Output]
```

In traditional programming, we assume the train stays on track. But what happens when something goes wrong?

### The Problem: Derailments

With exceptions, errors are like derailments:

```
[Input] ────────────────╳ CRASH!
                         💥 Exception thrown
```

The train jumps off the track, and someone up the line has to catch it with `try/catch`.

### The Solution: Two Tracks

Railway-Oriented Programming uses **two parallel tracks**:

```
SUCCESS TRACK (Ok)
[Input] ═══════════════════════════════════════════> [Output]
                                                     
ERROR TRACK (Error)
─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─> [Error]
```

Data starts on the success track. If something goes wrong, it **switches** to the error track. Once on the error track, it stays there - subsequent operations are skipped.

---

## Visualizing the Flow

Let's trace through an order processing example:

### Happy Path (All Operations Succeed)

```
                    ValidateUser    GetProduct      Charge         CreateOrder
SUCCESS ═══════════════╬═══════════════╬═══════════════╬═══════════════╬═══════> Order
                       │               │               │               │
ERROR   ─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─> 
```

The data flows through all four operations on the success track.

### Error Path (Validation Fails)

```
                    ValidateUser    GetProduct      Charge         CreateOrder
SUCCESS ═══════════════╳                                                        
                       │                                                        
                       ↓                                                        
ERROR   ─ ─ ─ ─ ─ ─ ─ ─●━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━> "Invalid user"
```

The first operation fails, so data switches to the error track. **GetProduct, Charge, and CreateOrder are never called.**

### Error Path (Payment Fails)

```
                    ValidateUser    GetProduct      Charge         CreateOrder
SUCCESS ════════════════════════════════════════════════╳                       
                                                        │                       
                                                        ↓                       
ERROR   ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─●━━━━━━━━━━━━━━━━━━━━> "Payment failed"
```

ValidateUser and GetProduct succeed, but Charge fails. CreateOrder is skipped.

---

## The Key Operations

### Map: Transform Success Values

`Map` transforms the value **on the success track only**:

```
                         Map(x => x * 2)
SUCCESS ═══[5]════════════════╬═══════════════[10]════════>
                              │
ERROR   ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─>
```

If we're on the error track, Map does nothing:

```
                         Map(x => x * 2)
SUCCESS ═══════════════════════════════════════════════════
                              │
ERROR   ━━━["error"]━━━━━━━━━━━━━━━━━━["error"]━━━━━━━━━━━━>
           (unchanged)
```

**In C#:**
```csharp
Result<int, string>.Ok(5)
    .Map(x => x * 2)  // Result<int, string>.Ok(10)

Result<int, string>.Error("oops")
    .Map(x => x * 2)  // Result<int, string>.Error("oops") - unchanged
```

### Bind: Chain Operations That Can Fail

`Bind` connects operations where **each step might fail**:

```
                        Bind(Validate)      Bind(Process)
SUCCESS ═══[input]═════════════╬═══[valid]═════════╬═══[result]═══>
                               │                   │
                               ↓                   ↓
ERROR   ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─>
                           (if validation fails, switch tracks)
```

**In C#:**
```csharp
Result<string, Error> ValidateEmail(string email) { ... }
Result<User, Error> CreateUser(string email) { ... }

// Chain them together
var result = ValidateEmail(input)
    .Bind(validEmail => CreateUser(validEmail));
```

### MapError: Transform Error Values

`MapError` transforms the value **on the error track only**:

```
SUCCESS ═══════════════════════════════════════════════════>
                              │
ERROR   ━━━["db error"]━━━━━━━━━━━━━━["User DB error"]━━━━━>
                        MapError(e => $"User {e}")
```

**In C#:**
```csharp
GetUser(id)
    .MapError(e => new ApiError($"Failed to get user: {e}"));
```

---

## Building a Pipeline

Let's build a real order processing pipeline:

```csharp
public Result<OrderConfirmation, OrderError> ProcessOrder(OrderRequest request)
{
    return ValidateRequest(request)                    // Step 1: Validate
        .Bind(req => GetUser(req.UserId))              // Step 2: Get user
        .Bind(user => GetProduct(request.ProductId)   
            .Map(product => (user, product)))          // Step 3: Get product
        .Bind(tuple => CheckInventory(tuple.product, request.Quantity)
            .Map(stock => (tuple.user, tuple.product, stock)))  // Step 4: Check stock
        .Bind(tuple => ChargePayment(tuple.user, CalculateTotal(tuple.product, request.Quantity))
            .Map(payment => (tuple.user, tuple.product, payment)))  // Step 5: Charge
        .Map(tuple => CreateConfirmation(tuple.user, tuple.product, tuple.payment));  // Step 6: Confirm
}
```

**What happens:**

1. If `ValidateRequest` fails → immediately return the error
2. If `GetUser` fails → immediately return the error
3. If `GetProduct` fails → immediately return the error
4. If `CheckInventory` fails → immediately return the error
5. If `ChargePayment` fails → immediately return the error
6. If all succeed → return the confirmation

**No try/catch. No null checks. No nested if statements.**

---

## Railway Operations Reference

| Operation | Purpose | Success Track | Error Track |
|-----------|---------|---------------|-------------|
| `Map` | Transform value | Applies function | Unchanged |
| `Bind` | Chain operations | Applies function (may switch tracks) | Unchanged |
| `MapError` | Transform error | Unchanged | Applies function |
| `Match` | Extract final value | Calls success function | Calls error function |
| `OrElse` | Recover from error | Unchanged | Tries recovery |
| `Tap` | Side effects | Executes action | Unchanged |
| `TapError` | Side effects on error | Unchanged | Executes action |

---

## Comparison with Traditional Code

### Traditional (Exception-Based)

```csharp
public OrderConfirmation ProcessOrder(OrderRequest request)
{
    try
    {
        ValidateRequest(request);
        var user = GetUser(request.UserId);
        var product = GetProduct(request.ProductId);
        CheckInventory(product, request.Quantity);
        var payment = ChargePayment(user, CalculateTotal(product, request.Quantity));
        return CreateConfirmation(user, product, payment);
    }
    catch (ValidationException ex)
    {
        throw new OrderException("Validation failed", ex);
    }
    catch (UserNotFoundException ex)
    {
        throw new OrderException("User not found", ex);
    }
    catch (ProductNotFoundException ex)
    {
        throw new OrderException("Product not found", ex);
    }
    catch (InsufficientStockException ex)
    {
        throw new OrderException("Out of stock", ex);
    }
    catch (PaymentException ex)
    {
        throw new OrderException("Payment failed", ex);
    }
}
```

**Problems:**
- Must know all exception types
- Easy to miss one
- Verbose
- Control flow is hidden

### Railway-Oriented

```csharp
public Result<OrderConfirmation, OrderError> ProcessOrder(OrderRequest request)
{
    return ValidateRequest(request)
        .Bind(req => GetUser(req.UserId))
        .Bind(user => GetProduct(request.ProductId).Map(p => (user, p)))
        .Bind(t => CheckInventory(t.p, request.Quantity).Map(s => (t.user, t.p, s)))
        .Bind(t => ChargePayment(t.user, CalculateTotal(t.p, request.Quantity)).Map(pay => (t.user, t.p, pay)))
        .Map(t => CreateConfirmation(t.user, t.p, t.pay));
}
```

**Benefits:**
- Error types explicit in signature
- Compiler ensures all handled
- Concise
- Control flow is visible

---

## Mental Model Summary

Think of your code as two parallel train tracks:

1. **Success Track (═══)**: The happy path where everything works
2. **Error Track (━━━)**: Where failures accumulate

Operations work like railway switches:
- **Map**: Transform cargo on the success track
- **Bind**: Switch to error track if operation fails
- **Match**: Final station - unload from whichever track you're on

Once you're on the error track, you stay there (unless you explicitly recover with `OrElse`).

---

## What's Next?

- **[Option Explained](OptionExplained.md)** - Railway for missing values (two tracks: Some/None)
- **[Result Explained](ResultExplained.md)** - Railway for errors (two tracks: Ok/Error)
- **[Composition Patterns](CompositionPatterns.md)** - Advanced railway techniques
