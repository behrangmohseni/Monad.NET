using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON converter for <see cref="Either{L, R}"/>.
/// Serializes as { "isRight": true, "value": ... } or { "isRight": false, "value": ... }.
/// </summary>
public class EitherJsonConverter<L, R> : JsonConverter<Either<L, R>>
{
    /// <inheritdoc />
    public override Either<L, R> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        bool? isRight = null;
        L? leftValue = default;
        R? rightValue = default;

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
                case "isright":
                    isRight = reader.GetBoolean();
                    break;
                case "left":
                    leftValue = JsonSerializer.Deserialize<L>(ref reader, options);
                    break;
                case "right":
                    rightValue = JsonSerializer.Deserialize<R>(ref reader, options);
                    break;
            }
        }

        if (isRight == true && rightValue is not null)
        {
            return Either<L, R>.Right(rightValue);
        }
        else if (isRight == false && leftValue is not null)
        {
            return Either<L, R>.Left(leftValue);
        }

        throw new JsonException("Invalid Either JSON: missing isRight or value");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Either<L, R> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isRight", value.IsRight);

        if (value.IsRight)
        {
            writer.WritePropertyName("right");
            JsonSerializer.Serialize(writer, value.Match(static l => default(R)!, static r => r), options);
        }
        else
        {
            writer.WritePropertyName("left");
            JsonSerializer.Serialize(writer, value.Match(static l => l, static r => default(L)!), options);
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating <see cref="EitherJsonConverter{L, R}"/> instances.
/// </summary>
public class EitherJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Either<,>);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(EitherJsonConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1]);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

