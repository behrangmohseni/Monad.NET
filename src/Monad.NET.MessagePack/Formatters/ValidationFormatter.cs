namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Validation{T, TError}"/>.
/// Serializes as [isValid, value/errors].
/// </summary>
public sealed class ValidationFormatter<T, TError> : IMessagePackFormatter<Validation<T, TError>>
{
    public void Serialize(ref MessagePackWriter writer, Validation<T, TError> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write(value.IsOk);

        if (value.IsOk)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value.GetValue(), options);
        }
        else
        {
            var errors = value.GetErrors();
            var formatter = options.Resolver.GetFormatterWithVerify<IReadOnlyList<TError>>();
            formatter.Serialize(ref writer, errors, options);
        }
    }

    public Validation<T, TError> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException($"Invalid Validation format. Expected 2 elements, got {count}.");
        }

        var isValid = reader.ReadBoolean();

        if (isValid)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            var value = formatter.Deserialize(ref reader, options);
            return value is not null
                ? Validation<T, TError>.Ok(value)
                : throw new MessagePackSerializationException("Validation Valid value cannot be null.");
        }
        else
        {
            var formatter = options.Resolver.GetFormatterWithVerify<IReadOnlyList<TError>>();
            var errors = formatter.Deserialize(ref reader, options);
            return errors is not null && errors.Count > 0
                ? Validation<T, TError>.Error(errors)
                : throw new MessagePackSerializationException("Validation Invalid must have at least one error.");
        }
    }
}

