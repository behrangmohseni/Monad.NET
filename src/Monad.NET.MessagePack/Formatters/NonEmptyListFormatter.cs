namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="NonEmptyList{T}"/>.
/// Serializes as a standard array with at least one element.
/// </summary>
public sealed class NonEmptyListFormatter<T> : IMessagePackFormatter<NonEmptyList<T>>
{
    public void Serialize(ref MessagePackWriter writer, NonEmptyList<T> value, MessagePackSerializerOptions options)
    {
        var formatter = options.Resolver.GetFormatterWithVerify<T>();
        var count = value.Count;

        writer.WriteArrayHeader(count);

        foreach (var item in value)
        {
            formatter.Serialize(ref writer, item, options);
        }
    }

    public NonEmptyList<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        if (count == 0)
        {
            throw new MessagePackSerializationException("NonEmptyList cannot be empty.");
        }

        var formatter = options.Resolver.GetFormatterWithVerify<T>();
        var items = new T[count];

        for (var i = 0; i < count; i++)
        {
            items[i] = formatter.Deserialize(ref reader, options);
        }

        return NonEmptyList<T>.FromEnumerable(items).GetValue();
    }
}

