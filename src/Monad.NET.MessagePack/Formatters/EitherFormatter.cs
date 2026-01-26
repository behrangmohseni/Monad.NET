namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Either{TLeft, TRight}"/>.
/// Serializes as [isRight, value].
/// </summary>
public sealed class EitherFormatter<TLeft, TRight> : IMessagePackFormatter<Either<TLeft, TRight>>
{
    public void Serialize(ref MessagePackWriter writer, Either<TLeft, TRight> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write(value.IsRight);

        if (value.IsRight)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<TRight>();
            formatter.Serialize(ref writer, value.GetRight(), options);
        }
        else
        {
            var formatter = options.Resolver.GetFormatterWithVerify<TLeft>();
            formatter.Serialize(ref writer, value.GetLeft(), options);
        }
    }

    public Either<TLeft, TRight> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException($"Invalid Either format. Expected 2 elements, got {count}.");
        }

        var isRight = reader.ReadBoolean();

        if (isRight)
        {
            var formatter = options.Resolver.GetFormatterWithVerify<TRight>();
            var value = formatter.Deserialize(ref reader, options);
            return value is not null
                ? Either<TLeft, TRight>.Right(value)
                : throw new MessagePackSerializationException("Either Right value cannot be null.");
        }
        else
        {
            var formatter = options.Resolver.GetFormatterWithVerify<TLeft>();
            var value = formatter.Deserialize(ref reader, options);
            return value is not null
                ? Either<TLeft, TRight>.Left(value)
                : throw new MessagePackSerializationException("Either Left value cannot be null.");
        }
    }
}

