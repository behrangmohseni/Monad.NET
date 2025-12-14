using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="Try{T}"/>.
/// Serializes as { "isSuccess": true, "value": ... } or { "isSuccess": false, "error": "message" }.
/// </summary>
public class TryJsonConverter<T> : JsonConverter<Try<T>>
{
    /// <inheritdoc />
    public override Try<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        bool? isSuccess = null;
        T? value = default;
        string? errorMessage = null;

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
                case "issuccess":
                    isSuccess = reader.GetBoolean();
                    break;
                case "value":
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
                    break;
                case "error":
                    errorMessage = reader.GetString();
                    break;
            }
        }

        if (isSuccess == true && value is not null)
        {
            return Try<T>.Success(value);
        }
        else if (isSuccess == false)
        {
            return Try<T>.Failure(new Exception(errorMessage ?? "Unknown error"));
        }

        throw new JsonException("Invalid Try JSON: missing isSuccess or value/error");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Try<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);

        if (value.IsSuccess)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Get(), options);
        }
        else
        {
            writer.WriteString("error", value.Match(_ => string.Empty, ex => ex.Message));
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating <see cref="TryJsonConverter{T}"/> instances.
/// </summary>
public class TryJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Try<>);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(TryJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

