using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="Option{T}"/>.
/// Serializes Some(value) as the value itself, and None as null.
/// </summary>
public class OptionJsonConverter<T> : JsonConverter<Option<T>>
{
    /// <inheritdoc />
    public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Option<T>.None();
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return value is not null ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
    {
        if (value.IsSome)
        {
            JsonSerializer.Serialize(writer, value.GetValue(), options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// Factory for creating <see cref="OptionJsonConverter{T}"/> instances.
/// </summary>
/// <remarks>
/// This factory uses reflection to create generic converter instances.
/// For full Native AOT support, register specific <see cref="OptionJsonConverter{T}"/> 
/// instances directly in your JsonSerializerOptions.
/// </remarks>
public class OptionJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the OptionJsonConverter<T> directly for AOT scenarios.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
#endif
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

