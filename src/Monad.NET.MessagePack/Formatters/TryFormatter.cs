namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Try{T}"/>.
/// Serializes as [isSuccess, value/errorMessage].
/// </summary>
public sealed class TryFormatter<T> : IMessagePackFormatter<Try<T>>
{
    public void Serialize(ref MessagePackWriter writer, Try<T> value, MessagePackSerializerOptions options)
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
            writer.Write(value.GetException().Message);
        }
    }

    public Try<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException($"Invalid Try format. Expected 2 elements, got {count}.");
        }

        var isSuccess = reader.ReadBoolean();

        if (isSuccess)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            var value = formatter.Deserialize(ref reader, options);
            return value is not null
                ? Try<T>.Success(value)
                : throw new MessagePackSerializationException("Try Success value cannot be null.");
        }
        else
        {
            var errorMessage = reader.ReadString();
            return Try<T>.Failure(new Exception(errorMessage ?? "Unknown error"));
        }
    }
}

