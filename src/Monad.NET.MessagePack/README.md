# Monad.NET.MessagePack

High-performance MessagePack serialization support for Monad.NET types.

## Installation

```bash
dotnet add package Monad.NET.MessagePack
```

## Usage

### Option 1: Use the MonadResolver

```csharp
using Monad.NET.MessagePack;
using MessagePack;

// Configure MessagePack with Monad.NET resolver
var options = MessagePackSerializerOptions.Standard
    .WithResolver(MonadResolver.Instance);

// Serialize
var option = Option<int>.Some(42);
var bytes = MessagePackSerializer.Serialize(option, options);

// Deserialize
var deserialized = MessagePackSerializer.Deserialize<Option<int>>(bytes, options);
```

### Option 2: Combine with other resolvers

```csharp
using MessagePack;
using MessagePack.Resolvers;
using Monad.NET.MessagePack;

var resolver = CompositeResolver.Create(
    MonadResolver.Instance,
    StandardResolver.Instance
);

var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
```

## Supported Types

| Type | Serialization Format |
|:-----|:--------------------|
| `Option<T>` | `[isSome, value]` or `null` for None |
| `Result<T, E>` | `[isOk, value/error]` |
| `Either<L, R>` | `[isRight, value]` |
| `Try<T>` | `[isSuccess, value/errorMessage]` |
| `Validation<T, E>` | `[isValid, value/errors]` |
| `NonEmptyList<T>` | Array of elements |
| `RemoteData<T, E>` | `[state, data/error]` |
| `Unit` | `null` |

## Performance

MessagePack typically provides 2-4x better performance and 50-70% smaller payload sizes compared to JSON serialization.

