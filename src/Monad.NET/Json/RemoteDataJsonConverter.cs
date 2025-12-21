using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="RemoteData{T, E}"/>.
/// Serializes with a "state" property indicating NotAsked, Loading, Success, or Failure.
/// </summary>
public class RemoteDataJsonConverter<T, E> : JsonConverter<RemoteData<T, E>>
{
    /// <inheritdoc />
    public override RemoteData<T, E> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        string? state = null;
        T? data = default;
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
                case "state":
                    state = reader.GetString();
                    break;
                case "data":
                    data = JsonSerializer.Deserialize<T>(ref reader, options);
                    break;
                case "error":
                    error = JsonSerializer.Deserialize<E>(ref reader, options);
                    break;
            }
        }

        return state?.ToLowerInvariant() switch
        {
            "notasked" => RemoteData<T, E>.NotAsked(),
            "loading" => RemoteData<T, E>.Loading(),
            "success" when data is not null => RemoteData<T, E>.Success(data),
            "failure" when error is not null => RemoteData<T, E>.Failure(error),
            _ => throw new JsonException($"Invalid RemoteData state: {state}")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, RemoteData<T, E> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        value.Match(
            () => writer.WriteString("state", "NotAsked"),
            () => writer.WriteString("state", "Loading"),
            data =>
            {
                writer.WriteString("state", "Success");
                writer.WritePropertyName("data");
                JsonSerializer.Serialize(writer, data, options);
            },
            error =>
            {
                writer.WriteString("state", "Failure");
                writer.WritePropertyName("error");
                JsonSerializer.Serialize(writer, error, options);
            }
        );

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating <see cref="RemoteDataJsonConverter{T, E}"/> instances.
/// </summary>
public class RemoteDataJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(RemoteData<,>);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(RemoteDataJsonConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1]);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
