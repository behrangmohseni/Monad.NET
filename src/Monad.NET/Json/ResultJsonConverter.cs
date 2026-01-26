using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="Result{T, E}"/>.
/// Serializes as { "isOk": true, "value": ... } or { "isOk": false, "error": ... }.
/// </summary>
public class ResultJsonConverter<T, E> : JsonConverter<Result<T, E>>
{
    /// <inheritdoc />
    public override Result<T, E> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        bool? isOk = null;
        T? value = default;
        E? error = default;

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
                case "isok":
                    isOk = reader.GetBoolean();
                    break;
                case "value":
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
                    break;
                case "error":
                    error = JsonSerializer.Deserialize<E>(ref reader, options);
                    break;
            }
        }

        if (isOk == true && value is not null)
        {
            return Result<T, E>.Ok(value);
        }
        else if (isOk == false && error is not null)
        {
            return Result<T, E>.Err(error);
        }

        throw new JsonException("Invalid Result JSON: missing isOk, value, or error");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Result<T, E> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isOk", value.IsOk);

        if (value.IsOk)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.GetValue(), options);
        }
        else
        {
            writer.WritePropertyName("error");
            JsonSerializer.Serialize(writer, value.GetError(), options);
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating <see cref="ResultJsonConverter{T, E}"/> instances.
/// </summary>
public class ResultJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(ResultJsonConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1]);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
