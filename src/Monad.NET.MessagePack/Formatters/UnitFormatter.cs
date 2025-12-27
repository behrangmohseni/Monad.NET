namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Unit"/>.
/// Serializes as null since Unit has only one value.
/// </summary>
public sealed class UnitFormatter : IMessagePackFormatter<Unit>
{
    public static readonly UnitFormatter Instance = new();

    private UnitFormatter() { }

    public void Serialize(ref MessagePackWriter writer, Unit value, MessagePackSerializerOptions options)
    {
        writer.WriteNil();
    }

    public Unit Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        reader.TryReadNil();
        return Unit.Value;
    }
}

