# Contributing to Monad.NET

Thank you for your interest in contributing to Monad.NET! This document provides guidelines and instructions for contributing.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Documentation](#documentation)

## Code of Conduct

This project follows a standard code of conduct. Please be respectful and constructive in all interactions.

## Branching Strategy

Monad.NET uses a multi-version branching strategy to support both active development and maintenance releases:

| Branch | Purpose | Version |
|--------|---------|---------|
| `main` | Active development of latest major version | v2.x |
| `v1.x` | Maintenance branch for v1.x patches and fixes | v1.x |

### Release Process

Releases are triggered by pushing version tags:

```bash
# For v1.x maintenance releases (from v1.x branch)
git checkout v1.x
git tag v1.1.3
git push origin v1.1.3

# For v2.x releases (from main branch)
git checkout main
git tag v2.0.0
git push origin v2.0.0
```

### Contributing to Different Versions

- **New features**: Target the `main` branch (v2.x)
- **Bug fixes for v1.x**: Target the `v1.x` branch
- **Bug fixes for both versions**: Submit to `main` first, then we'll backport to `v1.x` if applicable

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/behrangmohseni/Monad.NET.git
   cd Monad.NET
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/behrangmohseni/Monad.NET.git
   ```

## Development Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later (for building all targets)
- Your preferred IDE (Visual Studio, VS Code, Rider)

### Building the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Project Structure

```
Monad.NET/
├── src/
│   └── Monad.NET/           # Main library
│       ├── Option.cs        # Option<T> monad
│       ├── Result.cs        # Result<T, E> monad
│       ├── Validation.cs    # Validation<T, E> monad
│       ├── Try.cs           # Try<T> monad
│       ├── RemoteData.cs    # RemoteData<T, E> monad
│       ├── NonEmptyList.cs  # NonEmptyList<T> monad
│       ├── Writer.cs        # Writer<W, T> monad
│       ├── Reader.cs        # Reader<R, A> monad
│       ├── OptionAsync.cs   # Async extensions for Option
│       ├── ResultAsync.cs   # Async extensions for Result
│       ├── Linq.cs          # LINQ query syntax support
│       └── Collections.cs   # Collection extensions
├── tests/
│   └── Monad.NET.Tests/     # Unit tests
├── examples/
│   └── Monad.NET.Examples/  # Example usage
└── docs/                    # Documentation
```

## Making Changes

### Branch Naming

Use descriptive branch names:
- `feature/add-state-monad` - New features
- `fix/option-null-handling` - Bug fixes
- `docs/update-readme` - Documentation updates
- `refactor/simplify-result` - Code refactoring

### Commit Messages

Follow conventional commit format:

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or updating tests
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `chore`: Maintenance tasks

Examples:
```
feat(validation): add Combine method for multiple validations
fix(option): handle null in ToOption extension
docs(readme): add Writer monad examples
test(result): add async operation tests
```

## Pull Request Process

1. **Update your fork** with the latest upstream changes:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes** following the coding standards

4. **Write/update tests** for your changes

5. **Run the test suite**:
   ```bash
   dotnet test
   ```

6. **Push your branch**:
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Open a Pull Request** on GitHub

### PR Requirements

- [ ] All tests pass
- [ ] Code follows project style guidelines
- [ ] New features include tests
- [ ] Documentation is updated if needed
- [ ] No warnings in build output
- [ ] PR description explains the changes

## Coding Standards

### General Guidelines

- Use meaningful names for types, methods, and variables
- Keep methods focused and small
- Prefer immutability
- Document public APIs with XML comments
- Follow existing code patterns

### C# Style

```csharp
// Use expression-bodied members for simple operations
public T Head => _head;

// Use pattern matching where appropriate
public TResult Match<TResult>(
    Func<T, TResult> someFunc,
    Func<TResult> noneFunc)
{
    return _isSome ? someFunc(_value!) : noneFunc();
}

// Validate arguments
public static Option<T> Some(T value)
{
    if (value is null)
        throw new ArgumentNullException(nameof(value));
    
    return new Option<T>(value, true);
}

// Use XML documentation
/// <summary>
/// Maps the contained value using the specified function.
/// </summary>
/// <typeparam name="U">The type of the mapped value</typeparam>
/// <param name="mapper">The mapping function</param>
/// <returns>A new Option containing the mapped value</returns>
public Option<U> Map<U>(Func<T, U> mapper)
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Types | PascalCase | `Option<T>`, `RemoteData<T, E>` |
| Methods | PascalCase | `Map`, `AndThen`, `UnwrapOr` |
| Properties | PascalCase | `IsSome`, `IsOk`, `Head` |
| Private fields | _camelCase | `_value`, `_isSuccess` |
| Parameters | camelCase | `mapper`, `defaultValue` |
| Type parameters | Single uppercase or descriptive | `T`, `TErr`, `TResult` |

## Testing

### Test Organization

- One test class per source class
- Test class name: `{ClassName}Tests`
- Test method name: `{Method}_{Scenario}_{ExpectedResult}`

```csharp
public class OptionTests
{
    [Fact]
    public void Some_WithValue_ReturnsSome()
    {
        var option = Option<int>.Some(42);
        Assert.True(option.IsSome);
    }

    [Fact]
    public void Map_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var mapped = option.Map(x => x * 2);
        Assert.True(mapped.IsNone);
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~OptionTests"

# Run with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

## Documentation

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Represents an optional value that is either Some (contains a value) or None.
/// </summary>
/// <typeparam name="T">The type of the contained value</typeparam>
/// <remarks>
/// This type provides safe handling of potentially missing values without using null.
/// </remarks>
/// <example>
/// <code>
/// var some = Option&lt;int&gt;.Some(42);
/// var none = Option&lt;int&gt;.None();
/// 
/// var result = some.Match(
///     someFunc: v => $"Got {v}",
///     noneFunc: () => "Nothing"
/// );
/// </code>
/// </example>
public readonly struct Option<T>
```

### README Updates

When adding new features:
1. Add to the feature table in README.md
2. Include usage examples
3. Update the API reference if applicable

## Questions?

If you have questions about contributing, feel free to:
- Open an issue for discussion
- Start a GitHub Discussion
- Reach out to the maintainer

Thank you for contributing!

---

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni)
