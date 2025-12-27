using MessagePack.Resolvers;

namespace Monad.NET.MessagePack;

/// <summary>
/// Extension methods for configuring MessagePack serialization with Monad.NET types.
/// </summary>
public static class MonadResolverExtensions
{
    /// <summary>
    /// Creates MessagePack serializer options with Monad.NET resolver included.
    /// </summary>
    /// <param name="options">The base options to extend.</param>
    /// <returns>Options configured with MonadResolver.</returns>
    /// <example>
    /// <code>
    /// var options = MessagePackSerializerOptions.Standard.WithMonadResolver();
    /// </code>
    /// </example>
    public static MessagePackSerializerOptions WithMonadResolver(this MessagePackSerializerOptions options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var compositeResolver = CompositeResolver.Create(
            MonadResolver.Instance,
            options.Resolver
        );

        return options.WithResolver(compositeResolver);
    }

    /// <summary>
    /// Creates default MessagePack serializer options configured for Monad.NET types.
    /// </summary>
    /// <returns>Options with StandardResolver and MonadResolver.</returns>
    public static MessagePackSerializerOptions CreateMonadSerializerOptions()
    {
        var resolver = CompositeResolver.Create(
            MonadResolver.Instance,
            StandardResolver.Instance
        );

        return MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}

