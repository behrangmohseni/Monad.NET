namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Validation{T, TErr}"/>.
/// Serializes as [isValid, value/errors].
/// </summary>
public sealed class ValidationFormatter<T, TErr> : IMessagePackFormatter<Validation<T, TErr>>
{
    public void Serialize(ref MessagePackWriter writer, Validation<T, TErr> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write(value.IsValid);

        if (value.IsValid)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value.GetValue(), options);
        }
        else
        {
            var errors = value.GetErrors();
            var formatter = options.Resolver.GetFormatterWithVerify<IReadOnlyList<TErr>>();
            formatter.Serialize(ref writer, errors, options);
        }
    }

    public Validation<T, TErr> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
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
                ? Validation<T, TErr>.Valid(value)
                : throw new MessagePackSerializationException("Validation Valid value cannot be null.");
        }
        else
        {
            var formatter = options.Resolver.GetFormatterWithVerify<IReadOnlyList<TErr>>();
            var errors = formatter.Deserialize(ref reader, options);
            return errors is not null && errors.Count > 0
                ? Validation<T, TErr>.Invalid(errors)
                : throw new MessagePackSerializationException("Validation Invalid must have at least one error.");
        }
    }
}

