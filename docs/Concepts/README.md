# Functional Programming Concepts

This section explains functional programming concepts for C# developers who may be new to this paradigm.

## Reading Order

We recommend reading these guides in order if you're new to functional programming:

### 1. [Why Functional Error Handling?](WhyFunctionalErrorHandling.md)

Start here. Understand the problems with traditional exception-based error handling and why explicit error types are better.

**Key takeaways:**
- Exceptions hide errors from method signatures
- Result makes errors visible and composable
- The compiler becomes your friend

### 2. [Railway-Oriented Programming](RailwayOrientedProgramming.md)

The mental model that makes everything click. Think of your code as two parallel tracks: success and error.

**Key takeaways:**
- Data flows on two tracks: success and error
- Operations switch tracks on failure
- Map, Bind, and Match are railway switches

### 3. [Option Explained](OptionExplained.md)

Handle missing values without null. Learn how `Option<T>` improves upon nullable reference types.

**Key takeaways:**
- Option makes "might be missing" explicit
- Chain operations with Map and Bind
- No more NullReferenceException surprises

### 4. [Result Explained](ResultExplained.md)

Handle operations that can fail with meaningful error information.

**Key takeaways:**
- Result carries error information (Option just says "missing")
- Define typed error types for rich error handling
- Perfect for APIs and service layers

### 5. [Composition Patterns](CompositionPatterns.md)

Learn to build complex operations from simple, reusable pieces.

**Key takeaways:**
- Linear pipelines with Bind
- Accumulating data through transformations
- Error recovery and logging patterns

### 6. [From OOP to FP](FromOopToFp.md)

Bridge your OOP knowledge to functional concepts. See common C# patterns translated.

**Key takeaways:**
- You already know FP from LINQ
- Expressions over statements
- Transform, don't mutate

---

## Quick Reference

| If you want to... | Read this |
|-------------------|-----------|
| Understand why FP error handling | [Why Functional Error Handling?](WhyFunctionalErrorHandling.md) |
| Learn the mental model | [Railway-Oriented Programming](RailwayOrientedProgramming.md) |
| Handle missing values | [Option Explained](OptionExplained.md) |
| Handle operations that fail | [Result Explained](ResultExplained.md) |
| Chain operations elegantly | [Composition Patterns](CompositionPatterns.md) |
| Translate OOP patterns | [From OOP to FP](FromOopToFp.md) |

---

## Ready to Use Monad.NET?

After reading these concepts, check out:

- **[Quick Start Guide](../QUICKSTART.md)** - Get up and running in 5 minutes
- **[Core Types Reference](../CoreTypes.md)** - Full API documentation
- **[Examples](../Examples.md)** - Real-world code samples
