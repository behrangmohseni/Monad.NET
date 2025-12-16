using System.Text.Json;

namespace Monad.NET.Json;

/// <summary>
/// Extension methods for configuring JSON serialization for Monad.NET types.
/// </summary>
public static class MonadJsonExtensions
{
    /// <summary>
    /// Adds all Monad.NET JSON converters to the specified <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The JSON serializer options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static JsonSerializerOptions AddMonadConverters(this JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Converters.Add(new OptionJsonConverterFactory());
        options.Converters.Add(new ResultJsonConverterFactory());
        options.Converters.Add(new EitherJsonConverterFactory());
        options.Converters.Add(new TryJsonConverterFactory());
        options.Converters.Add(new ValidationJsonConverterFactory());
        options.Converters.Add(new NonEmptyListJsonConverterFactory());
        options.Converters.Add(new RemoteDataJsonConverterFactory());
        return options;
    }

    /// <summary>
    /// Creates a new <see cref="JsonSerializerOptions"/> with all Monad.NET converters configured.
    /// </summary>
    /// <returns>A new JsonSerializerOptions instance with Monad.NET converters.</returns>
    public static JsonSerializerOptions CreateMonadSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        return options.AddMonadConverters();
    }
}

