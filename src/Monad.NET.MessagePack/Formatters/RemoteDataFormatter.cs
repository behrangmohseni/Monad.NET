namespace Monad.NET.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="RemoteData{T, TError}"/>.
/// Serializes as [state, data/error] where state is 0=NotAsked, 1=Loading, 2=Success, 3=Failure.
/// </summary>
public sealed class RemoteDataFormatter<T, TError> : IMessagePackFormatter<RemoteData<T, TError>>
{
    private const int NotAsked = 0;
    private const int Loading = 1;
    private const int Success = 2;
    private const int Failure = 3;

    public void Serialize(ref MessagePackWriter writer, RemoteData<T, TError> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);

        if (value.IsNotAsked)
        {
            writer.Write(NotAsked);
            writer.WriteNil();
        }
        else if (value.IsLoading)
        {
            writer.Write(Loading);
            writer.WriteNil();
        }
        else if (value.IsOk)
        {
            writer.Write(Success);
            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value.GetValue(), options);
        }
        else // IsFailure
        {
            writer.Write(Failure);
            var formatter = options.Resolver.GetFormatterWithVerify<TError>();
            formatter.Serialize(ref writer, value.GetError(), options);
        }
    }

    public RemoteData<T, TError> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException($"Invalid RemoteData format. Expected 2 elements, got {count}.");
        }

        var state = reader.ReadInt32();

        switch (state)
        {
            case NotAsked:
                reader.Skip();
                return RemoteData<T, TError>.NotAsked();
            case Loading:
                reader.Skip();
                return RemoteData<T, TError>.Loading();
            case Success:
                return DeserializeSuccess(ref reader, options);
            case Failure:
                return DeserializeFailure(ref reader, options);
            default:
                throw new MessagePackSerializationException($"Invalid RemoteData state: {state}.");
        }
    }

    private static RemoteData<T, TError> DeserializeSuccess(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var formatter = options.Resolver.GetFormatterWithVerify<T>();
        var data = formatter.Deserialize(ref reader, options);
        return data is not null
            ? RemoteData<T, TError>.Ok(data)
            : throw new MessagePackSerializationException("RemoteData Success value cannot be null.");
    }

    private static RemoteData<T, TError> DeserializeFailure(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var formatter = options.Resolver.GetFormatterWithVerify<TError>();
        var error = formatter.Deserialize(ref reader, options);
        return error is not null
            ? RemoteData<T, TError>.Error(error)
            : throw new MessagePackSerializationException("RemoteData Failure error cannot be null.");
    }
}
