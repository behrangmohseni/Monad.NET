namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Result{T, TErr}"/>.
/// Serializes as [isOk, value/error].
/// </summary>
public sealed class ResultFormatter<T, TErr> : IMessagePackFormatter<Result<T, TErr>>
{
    public void Serialize(ref MessagePackWriter writer, Result<T, TErr> value, MessagePackSerializerOptions options)
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
            var formatter = options.Resolver.GetFormatterWithVerify<TErr>();
            formatter.Serialize(ref writer, value.GetError(), options);
        }
    }

    public Result<T, TErr> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException($"Invalid Result format. Expected 2 elements, got {count}.");
        }

        var isOk = reader.ReadBoolean();

        if (isOk)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            var value = formatter.Deserialize(ref reader, options);
            return value is not null
                ? Result<T, TErr>.Ok(value)
                : throw new MessagePackSerializationException("Result Ok value cannot be null.");
        }
        else
        {
            var formatter = options.Resolver.GetFormatterWithVerify<TErr>();
            var error = formatter.Deserialize(ref reader, options);
            return error is not null
                ? Result<T, TErr>.Err(error)
                : throw new MessagePackSerializationException("Result Err value cannot be null.");
        }
    }
}

