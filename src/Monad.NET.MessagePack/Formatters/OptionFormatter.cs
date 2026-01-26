namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Option{T}"/>.
/// Serializes Some(value) as [true, value] and None as null.
/// </summary>
public sealed class OptionFormatter<T> : IMessagePackFormatter<Option<T>>
{
    public void Serialize(ref MessagePackWriter writer, Option<T> value, MessagePackSerializerOptions options)
    {
        if (value.IsNone)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(2);
        writer.Write(true);

        var formatter = options.Resolver.GetFormatterWithVerify<T>();
        formatter.Serialize(ref writer, value.GetValue(), options);
    }

    public Option<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return Option<T>.None();
        }

        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException($"Invalid Option format. Expected 2 elements, got {count}.");
        }

        var isSome = reader.ReadBoolean();
        if (!isSome)
        {
            reader.Skip();
            return Option<T>.None();
        }

        var formatter = options.Resolver.GetFormatterWithVerify<T>();
        var value = formatter.Deserialize(ref reader, options);

        return value is not null ? Option<T>.Some(value) : Option<T>.None();
    }
}

