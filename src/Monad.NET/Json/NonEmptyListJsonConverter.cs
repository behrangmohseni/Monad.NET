using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="NonEmptyList{T}"/>.
/// Serializes as a JSON array.
/// </summary>
public class NonEmptyListJsonConverter<T> : JsonConverter<NonEmptyList<T>>
{
    /// <inheritdoc />
    public override NonEmptyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        var items = new List<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            var item = JsonSerializer.Deserialize<T>(ref reader, options);
            if (item is not null)
            {
                items.Add(item);
            }
        }

        if (items.Count == 0)
        {
            throw new JsonException("NonEmptyList cannot be empty");
        }

        return NonEmptyList<T>.Of(items[0], items.Skip(1).ToArray());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, NonEmptyList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }

        writer.WriteEndArray();
    }
}

/// <summary>
/// Factory for creating <see cref="NonEmptyListJsonConverter{T}"/> instances.
/// </summary>
/// <remarks>
/// This factory uses reflection to create generic converter instances.
/// For full Native AOT support, register specific <see cref="NonEmptyListJsonConverter{T}"/> 
/// instances directly in your JsonSerializerOptions.
/// </remarks>
public class NonEmptyListJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(NonEmptyList<>);
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the NonEmptyListJsonConverter<T> directly for AOT scenarios.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
#endif
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(NonEmptyListJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
