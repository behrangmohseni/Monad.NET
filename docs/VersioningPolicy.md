# API Versioning and Deprecation Policy

This document outlines the versioning strategy, backward compatibility guarantees, and deprecation policy for Monad.NET.

## Table of Contents

- [Semantic Versioning](#semantic-versioning)
- [Compatibility Guarantees](#compatibility-guarantees)
- [Deprecation Process](#deprecation-process)
- [Breaking Changes Policy](#breaking-changes-policy)
- [Migration Support](#migration-support)
- [Version History](#version-history)

---

## Semantic Versioning

Monad.NET follows [Semantic Versioning 2.0.0](https://semver.org/) (SemVer):

```
MAJOR.MINOR.PATCH

Examples:
  1.0.0 → 1.0.1  (patch: bug fix)
  1.0.0 → 1.1.0  (minor: new feature, backward compatible)
  1.0.0 → 2.0.0  (major: breaking changes)
```

### Version Components

| Component | When Incremented | Compatibility |
|-----------|------------------|---------------|
| **MAJOR** | Breaking API changes | May require code changes |
| **MINOR** | New features added | Fully backward compatible |
| **PATCH** | Bug fixes only | Fully backward compatible |

### Pre-release Versions

Pre-release versions use suffixes:
- `-alpha.N` — Early development, API may change significantly
- `-beta.N` — Feature complete, API stabilizing
- `-rc.N` — Release candidate, final testing

```
1.2.0-alpha.1  →  1.2.0-beta.1  →  1.2.0-rc.1  →  1.2.0
```

---

## Compatibility Guarantees

### What We Guarantee (Stable API)

The following are considered **stable API** and will not change without a major version bump:

| Category | Examples | Stability |
|----------|----------|-----------|
| **Public types** | `Option<T>`, `Result<T,E>`, etc. | ✅ Stable |
| **Public methods** | `.Map()`, `.AndThen()`, `.Match()` | ✅ Stable |
| **Method signatures** | Parameter types, return types | ✅ Stable |
| **Behavior** | Short-circuit semantics, error accumulation | ✅ Stable |
| **Extension methods** | `.ToOption()`, `.ToResult()` | ✅ Stable |

### What May Change (Minor Versions)

The following may change in minor versions (backward compatible additions):

| Category | Examples | Change Type |
|----------|----------|-------------|
| **New methods** | Additional helper methods | Addition |
| **New overloads** | More flexible parameter options | Addition |
| **New types** | Additional monad types | Addition |
| **Performance** | Optimizations, inlining | Improvement |
| **Default parameters** | New optional parameters | Addition |

### What Is Not Guaranteed (Internal API)

The following are **not part of the public API** and may change without notice:

| Category | Examples | Stability |
|----------|----------|-----------|
| **Internal types** | Types in internal namespaces | ❌ Unstable |
| **Private members** | Private fields, methods | ❌ Unstable |
| **Implementation details** | Internal data structures | ❌ Unstable |
| **Debugging output** | `ToString()` format | ❌ Unstable |
| **Exception messages** | Error message text | ❌ Unstable |

---

## Deprecation Process

When API elements need to be removed or changed, we follow a structured deprecation process:

### Phase 1: Deprecation Warning (Minor Version)

```csharp
/// <summary>
/// Gets the value or throws if None.
/// </summary>
/// <remarks>
/// Consider using <see cref="UnwrapOr"/> or <see cref="Match"/> instead.
/// </remarks>
[Obsolete("Use UnwrapOr() or Match() instead. This will be removed in v3.0.0.")]
public T Value => IsSome ? _value : throw new InvalidOperationException();
```

**In this phase:**
- The deprecated API still works
- Compiler warning is generated
- Documentation explains the alternative
- Deprecation timeline is specified

### Phase 2: Escalated Warning (Next Minor Version)

```csharp
[Obsolete("Use UnwrapOr() or Match() instead. This will be removed in v3.0.0.", error: false)]
#pragma warning disable CS0618 // Suppress in implementation
public T Value => ...
#pragma warning restore CS0618
```

**In this phase:**
- Warning becomes more prominent in release notes
- Migration guide is published
- Examples are updated to use new API

### Phase 3: Removal (Next Major Version)

```csharp
// Property removed entirely in v3.0.0
// public T Value => ...  // REMOVED
```

**In this phase:**
- Deprecated API is removed
- Breaking change is documented in release notes
- Major version is incremented

### Deprecation Timeline

| Deprecation Type | Minimum Warning Period |
|------------------|------------------------|
| Minor API changes | 1 minor version (3+ months) |
| Major API changes | 2 minor versions (6+ months) |
| Type removal | 1 major version cycle |
| Behavior changes | 2 minor versions with opt-in flag |

---

## Breaking Changes Policy

### What Constitutes a Breaking Change

| Change Type | Breaking? | Requires Major Version? |
|-------------|-----------|------------------------|
| Removing public type | ✅ Yes | ✅ Yes |
| Removing public method | ✅ Yes | ✅ Yes |
| Changing method signature | ✅ Yes | ✅ Yes |
| Changing behavior | ✅ Yes | ✅ Yes |
| Adding required parameter | ✅ Yes | ✅ Yes |
| Adding new type | ❌ No | ❌ No |
| Adding new method | ❌ No | ❌ No |
| Adding optional parameter | ❌ No | ❌ No |
| Bug fix (unintended behavior) | ⚠️ Maybe | ❌ No |
| Performance optimization | ❌ No | ❌ No |

### Breaking Change Documentation

All breaking changes are documented in:

1. **CHANGELOG.md** — Full list of changes per version
2. **Release notes** — GitHub release description
3. **Migration guide** — Step-by-step upgrade instructions

Example CHANGELOG entry:

```markdown
## [3.0.0] - 2025-06-01

### ⚠️ Breaking Changes

- **BREAKING**: Removed `Option<T>.Value` property
  - Use `UnwrapOr(default)` or `Match(some: v => v, none: () => default)` instead
  - Migration: Replace `.Value` with `.UnwrapOr(defaultValue)`

- **BREAKING**: Changed `Validation<T, E>` type parameter order
  - Old: `Validation<Error, Value>`
  - New: `Validation<Value, Error>`
  - Migration: Swap type parameters in all usages
```

### Bug Fix Policy

Bug fixes that change observable behavior:

1. **Clearly unintended behavior** — Fixed in patch version
2. **Behavior someone might depend on** — Documented, optional flag in minor version
3. **Widely relied upon behavior** — Treated as breaking change

---

## Migration Support

### Upgrade Guides

For each major version, we provide:

| Resource | Description |
|----------|-------------|
| **Migration Guide** | Step-by-step instructions |
| **Breaking Changes List** | All changes with examples |
| **Code Examples** | Before/after code snippets |
| **Codefixes (Analyzers)** | Automatic code updates where possible |

### Analyzer-Assisted Migration

Monad.NET.Analyzers includes migration helpers:

```csharp
// Analyzer detects deprecated usage
var value = option.Value;  // MONAD001: Use UnwrapOr() instead

// Code fix provides automatic replacement
var value = option.UnwrapOr(default);
```

### Supported Versions

| Version | Status | Support Until |
|---------|--------|---------------|
| 2.x | Current | Active development |
| 1.x | Maintenance | 6 months after 2.0 release |
| 0.x | End of life | No longer supported |

**Maintenance mode** means:
- Critical bug fixes only
- No new features
- Security patches

---

## Version History

### Versioning Milestones

| Version | Date | Significance |
|---------|------|--------------|
| 0.1.0 | Initial | First public release |
| 1.0.0 | Stable | First stable API |
| 2.0.0 | TBD | Major improvements (see roadmap) |

### API Stability by Version

| Feature | Introduced | Stable Since |
|---------|------------|--------------|
| `Option<T>` | 0.1.0 | 1.0.0 |
| `Result<T,E>` | 0.1.0 | 1.0.0 |
| `Either<L,R>` | 0.1.0 | 1.0.0 |
| `Validation<T,E>` | 0.2.0 | 1.0.0 |
| `Try<T>` | 0.2.0 | 1.0.0 |
| `NonEmptyList<T>` | 0.3.0 | 1.0.0 |
| `RemoteData<T,E>` | 0.4.0 | 1.0.0 |
| `Writer<W,T>` | 0.5.0 | 1.0.0 |
| `Reader<R,A>` | 0.5.0 | 1.0.0 |
| `ReaderAsync<R,A>` | 0.6.0 | 1.0.0 |
| `State<S,A>` | 0.6.0 | 1.0.0 |
| `IO<T>` | 0.7.0 | 1.0.0 |
| `[Union]` generator | 0.8.0 | 1.0.0 |

---

## Package Versioning

### NuGet Packages

All Monad.NET packages share the same version number:

| Package | Description |
|---------|-------------|
| `Monad.NET` | Core library |
| `Monad.NET.SourceGenerators` | Union type generators |
| `Monad.NET.Analyzers` | Code analysis and fixes |
| `Monad.NET.AspNetCore` | ASP.NET Core integration |
| `Monad.NET.EntityFrameworkCore` | EF Core integration |
| `Monad.NET.MessagePack` | MessagePack serialization |

**Versioning rule**: All packages are released together with the same version number to ensure compatibility.

### Compatibility Matrix

| Monad.NET | .NET 6.0 | .NET 7.0 | .NET 8.0 | .NET 9.0 |
|-----------|----------|----------|----------|----------|
| 1.x | ✅ | ✅ | ✅ | ✅ |
| 2.x | ✅ | ✅ | ✅ | ✅ |

---

## Getting Help

### Reporting Issues

If you encounter problems during upgrade:

1. Check the [Migration Guide](Guides/MigrationGuide.md)
2. Search [existing issues](https://github.com/behrangmohseni/Monad.NET/issues)
3. Open a [new issue](https://github.com/behrangmohseni/Monad.NET/issues/new) with:
   - Current version
   - Target version
   - Error messages
   - Minimal reproduction

### Community Support

- **GitHub Discussions**: Questions and discussions
- **Issues**: Bug reports and feature requests

---

[← Back to Documentation](README.md) | [Changelog →](../CHANGELOG.md)

