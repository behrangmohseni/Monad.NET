using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="Validation{T, E}"/>.
/// Serializes as { "isValid": true, "value": ... } or { "isValid": false, "errors": [...] }.
/// </summary>
public class ValidationJsonConverter<T, E> : JsonConverter<Validation<T, E>>
{
    /// <inheritdoc />
    public override Validation<T, E> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        bool? isValid = null;
        T? value = default;
        List<E>? errors = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName?.ToLowerInvariant())
            {
                case "isvalid":
                    isValid = reader.GetBoolean();
                    break;
                case "value":
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
                    break;
                case "errors":
                    errors = JsonSerializer.Deserialize<List<E>>(ref reader, options);
                    break;
            }
        }

        if (isValid == true && value is not null)
        {
            return Validation<T, E>.Ok(value);
        }
        else if (isValid == false && errors is not null)
        {
            return Validation<T, E>.Error(errors);
        }

        throw new JsonException("Invalid Validation JSON: missing isValid, value, or errors");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Validation<T, E> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isValid", value.IsOk);

        if (value.IsOk)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Match(static v => v, static _ => default!), options);
        }
        else
        {
            writer.WritePropertyName("errors");
            JsonSerializer.Serialize(writer, value.Match(static _ => new List<E>(), static e => e.ToList()), options);
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating <see cref="ValidationJsonConverter{T, E}"/> instances.
/// </summary>
/// <remarks>
/// This factory uses reflection to create generic converter instances.
/// For full Native AOT support, register specific <see cref="ValidationJsonConverter{T, E}"/> 
/// instances directly in your JsonSerializerOptions.
/// </remarks>
public class ValidationJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Validation<,>);
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the ValidationJsonConverter<T,E> directly for AOT scenarios.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
#endif
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var typeArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(ValidationJsonConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1]);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

