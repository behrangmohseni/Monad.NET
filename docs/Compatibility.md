# Compatibility

## Supported Frameworks

Monad.NET supports a wide range of .NET versions:

| Target Framework | Minimum Version | Notes |
|------------------|-----------------|-------|
| .NET Standard 2.0 | .NET Framework 4.6.1+ | Broadest compatibility |
| .NET Standard 2.1 | .NET Core 3.0+ | Span, Index, Range support |
| .NET 8.0 | .NET 8.0 | LTS release |
| .NET 10.0 | .NET 10.0 | Latest features |

## Package Compatibility Matrix

| Package | .NET Standard 2.0 | .NET Standard 2.1 | .NET 8.0 | .NET 10.0 |
|---------|-------------------|-------------------|----------|-----------|
| Monad.NET | ✅ | ✅ | ✅ | ✅ |
| Monad.NET.SourceGenerators | ✅ | ✅ | ✅ | ✅ |
| Monad.NET.Analyzers | ✅ | ✅ | ✅ | ✅ |
| Monad.NET.AspNetCore | ❌ | ❌ | ✅ | ✅ |
| Monad.NET.EntityFrameworkCore | ❌ | ❌ | ✅ | ✅ |
| Monad.NET.MessagePack | ✅ | ✅ | ✅ | ✅ |

## Dependencies

### Core Library (Monad.NET)

The core library has **zero external dependencies** for .NET 8.0+.

For older frameworks, minimal polyfill packages are included:

| Framework | Dependencies |
|-----------|-------------|
| .NET Standard 2.0 | `Microsoft.Bcl.AsyncInterfaces`, `System.Memory` |
| .NET Standard 2.1 | `Microsoft.Bcl.AsyncInterfaces` |
| .NET 8.0+ | None |

### Integration Packages

| Package | Dependencies |
|---------|-------------|
| Monad.NET.AspNetCore | ASP.NET Core 8.0+ |
| Monad.NET.EntityFrameworkCore | EF Core 8.0+ |
| Monad.NET.MessagePack | MessagePack 2.5+ |

## IDE Support

Monad.NET works with all major .NET IDEs:

- Visual Studio 2022 (17.0+)
- JetBrains Rider (2023.1+)
- Visual Studio Code with C# Dev Kit

## Language Version

Monad.NET uses `LangVersion=latest` and leverages modern C# features where available, with polyfills for older frameworks.
