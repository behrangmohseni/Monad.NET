using Monad.NET.MessagePack.Formatters;

namespace Monad.NET.MessagePack;

/// <summary>
/// MessagePack resolver for Monad.NET types.
/// Provides formatters for Option, Result, Try, Validation, NonEmptyList, RemoteData, and Unit.
/// </summary>
/// <example>
/// <code>
/// var options = MessagePackSerializerOptions.Standard
///     .WithResolver(MonadResolver.Instance);
/// 
/// var bytes = MessagePackSerializer.Serialize(Option&lt;int&gt;.Some(42), options);
/// </code>
/// </example>
public sealed class MonadResolver : IFormatterResolver
{
    /// <summary>
    /// Gets the singleton instance of the MonadResolver.
    /// </summary>
    public static readonly MonadResolver Instance = new();

    private MonadResolver() { }

    /// <inheritdoc />
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter;

        static FormatterCache()
        {
            Formatter = (IMessagePackFormatter<T>?)MonadResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }
}

internal static class MonadResolverGetFormatterHelper
{
    private static readonly Dictionary<Type, Type> FormatterMap = new()
    {
        { typeof(Unit), typeof(UnitFormatter) }
    };

    internal static object? GetFormatter(Type t)
    {
        // Check for Unit first (non-generic)
        if (t == typeof(Unit))
        {
            return UnitFormatter.Instance;
        }

        if (!t.IsGenericType)
        {
            return null;
        }

        var genericType = t.GetGenericTypeDefinition();
        var typeArgs = t.GetGenericArguments();

        // Option<T>
        if (genericType == typeof(Option<>))
        {
            return CreateFormatter(typeof(OptionFormatter<>), typeArgs);
        }

        // Result<T, TErr>
        if (genericType == typeof(Result<,>))
        {
            return CreateFormatter(typeof(ResultFormatter<,>), typeArgs);
        }

        // Try<T>
        if (genericType == typeof(Try<>))
        {
            return CreateFormatter(typeof(TryFormatter<>), typeArgs);
        }

        // Validation<T, TErr>
        if (genericType == typeof(Validation<,>))
        {
            return CreateFormatter(typeof(ValidationFormatter<,>), typeArgs);
        }

        // NonEmptyList<T>
        if (genericType == typeof(NonEmptyList<>))
        {
            return CreateFormatter(typeof(NonEmptyListFormatter<>), typeArgs);
        }

        // RemoteData<T, TErr>
        if (genericType == typeof(RemoteData<,>))
        {
            return CreateFormatter(typeof(RemoteDataFormatter<,>), typeArgs);
        }

        return null;
    }

    private static object? CreateFormatter(Type formatterType, Type[] typeArgs)
    {
        var constructedType = formatterType.MakeGenericType(typeArgs);
        return Activator.CreateInstance(constructedType);
    }
}

