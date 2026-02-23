# Compatibility

## Supported Frameworks

Monad.NET supports a wide range of .NET versions:

| Target Framework | Minimum Version | Notes |
|------------------|-----------------|-------|
| .NET Standard 2.0 | .NET Framework 4.6.1+ | Broadest compatibility |
| .NET Standard 2.1 | .NET Core 3.0+ | Span, Index, Range support |
| .NET 6.0 | .NET 6.0 | Previous LTS release |
| .NET 8.0 | .NET 8.0 | Current LTS release |
| .NET 10.0 | .NET 10.0 | Tested in CI (preview) |

## Package Compatibility Matrix

| Package | .NET Standard 2.0 | .NET Standard 2.1 | .NET 6.0 | .NET 8.0 |
|---------|-------------------|-------------------|----------|----------|
| Monad.NET | Yes | Yes | Yes | Yes |
| Monad.NET.SourceGenerators | Yes | Yes | Yes | Yes |
| Monad.NET.Analyzers | Yes | Yes | Yes | Yes |
| Monad.NET.AspNetCore | No | No | Yes | Yes |
| Monad.NET.EntityFrameworkCore | No | No | Yes | Yes |
| Monad.NET.MessagePack | Yes | Yes | Yes | Yes |

## Dependencies

### Core Library (Monad.NET)

The core library has **zero external dependencies** for .NET 6.0+.

For older frameworks, minimal polyfill packages are included:

| Framework | Dependencies |
|-----------|-------------|
| .NET Standard 2.0 | `Microsoft.Bcl.AsyncInterfaces`, `System.Collections.Immutable`, `System.Memory`, `System.Text.Json` |
| .NET Standard 2.1 | `Microsoft.Bcl.AsyncInterfaces`, `System.Collections.Immutable`, `System.Text.Json` |
| .NET 6.0+ | None |

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
