#if NET8_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monad.NET.Json;

/// <summary>
/// JSON serializer context for Monad.NET types with AOT (Ahead-of-Time) compilation support.
/// Use this context with System.Text.Json source generators for trimming-safe and AOT-compatible serialization.
/// </summary>
/// <remarks>
/// <para>
/// This context provides source-generated JSON serialization for Monad.NET types,
/// which is essential for:
/// </para>
/// <list type="bullet">
/// <item><description>Native AOT compilation (.NET 8+)</description></item>
/// <item><description>IL trimming scenarios</description></item>
/// <item><description>Blazor WebAssembly applications</description></item>
/// <item><description>iOS/Android applications with limited reflection</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Option 1: Use the pre-configured context
/// var json = JsonSerializer.Serialize(Option&lt;int&gt;.Some(42), MonadJsonSerializerContext.DefaultOptions);
/// 
/// // Option 2: Create custom context inheriting from this
/// [JsonSerializable(typeof(MyDto))]
/// public partial class MyJsonContext : JsonSerializerContext { }
/// </code>
/// </example>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Unit))]
public partial class MonadJsonSerializerContext : JsonSerializerContext
{
    private static JsonSerializerOptions? _cachedOptions;

    /// <summary>
    /// Gets the default JSON serializer options configured with Monad.NET converters.
    /// </summary>
    /// <remarks>
    /// These options include all the custom JsonConverter implementations for monad types.
    /// For full AOT support with custom types, consider creating your own JsonSerializerContext.
    /// </remarks>
    public static JsonSerializerOptions DefaultOptions
    {
        get
        {
            if (_cachedOptions is not null)
                return _cachedOptions;

            var options = new JsonSerializerOptions(Default.Options);
            options.AddMonadConverters();
            _cachedOptions = options;
            return _cachedOptions;
        }
    }
}
#endif
