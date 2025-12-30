using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Static helper methods for parsing strings into Option types.
/// Provides an alternative entry point for discovery via <c>OptionParse.</c> instead of extension methods.
/// </summary>
/// <example>
/// <code>
/// var age = OptionParse.Int("42");           // Some(42)
/// var guid = OptionParse.Guid(userInput);    // Some(Guid) or None
/// var date = OptionParse.DateTime(dateStr);  // Some(DateTime) or None
/// </code>
/// </example>
public static class OptionParse
{
    /// <summary>
    /// Attempts to parse a string as an integer.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed integer if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<int> Int(string? value) => value.ParseInt();

    /// <summary>
    /// Attempts to parse a string as a long integer.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed long if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<long> Long(string? value) => value.ParseLong();

    /// <summary>
    /// Attempts to parse a string as a double.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed double if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<double> Double(string? value) => value.ParseDouble();

    /// <summary>
    /// Attempts to parse a string as a decimal.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed decimal if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<decimal> Decimal(string? value) => value.ParseDecimal();

    /// <summary>
    /// Attempts to parse a string as a boolean.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed boolean if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<bool> Bool(string? value) => value.ParseBool();

    /// <summary>
    /// Attempts to parse a string as a GUID.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed GUID if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Guid> Guid(string? value) => value.ParseGuid();

    /// <summary>
    /// Attempts to parse a string as a DateTime.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed DateTime if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateTime> DateTime(string? value) => value.ParseDateTime();

    /// <summary>
    /// Attempts to parse a string as a DateTimeOffset.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed DateTimeOffset if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateTimeOffset> DateTimeOffset(string? value) => value.ParseDateTimeOffset();

    /// <summary>
    /// Attempts to parse a string as a TimeSpan.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed TimeSpan if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TimeSpan> TimeSpan(string? value) => value.ParseTimeSpan();

    /// <summary>
    /// Attempts to parse a string as an enum value.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing. Default is true.</param>
    /// <returns>Some containing the parsed enum if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TEnum> Enum<TEnum>(string? value, bool ignoreCase = true) where TEnum : struct, Enum 
        => value.ParseEnum<TEnum>(ignoreCase);

#if NET6_0_OR_GREATER
    /// <summary>
    /// Attempts to parse a string as a DateOnly.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed DateOnly if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateOnly> DateOnly(string? value)
    {
        return System.DateOnly.TryParse(value, out var result)
            ? Option<DateOnly>.Some(result)
            : Option<DateOnly>.None();
    }

    /// <summary>
    /// Attempts to parse a string as a TimeOnly.
    /// Returns Some if parsing succeeds; otherwise None.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>Some containing the parsed TimeOnly if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TimeOnly> TimeOnly(string? value)
    {
        return System.TimeOnly.TryParse(value, out var result)
            ? Option<TimeOnly>.Some(result)
            : Option<TimeOnly>.None();
    }
#endif
}

/// <summary>
/// Static helper methods for creating Option instances from various sources.
/// Provides an alternative entry point for discovery via <c>OptionFrom.</c> instead of extension methods.
/// </summary>
/// <example>
/// <code>
/// var user = OptionFrom.Nullable(GetUserOrNull());
/// var name = OptionFrom.String(input); // None if null/empty
/// var value = OptionFrom.Dictionary(dict, key);
/// </code>
/// </example>
public static class OptionFrom
{
    /// <summary>
    /// Creates an Option from a nullable value type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>Some if value has a value; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Nullable<T>(T? value) where T : struct 
        => value.ToOption();

    /// <summary>
    /// Creates an Option from a nullable reference type.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>Some if value is not null; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Nullable<T>(T? value) where T : class 
        => value.ToOption();

    /// <summary>
    /// Creates an Option from a string, returning None if null or empty.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>Some if string is not null or empty; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> String(string? value) 
        => value.ToOptionNotEmpty();

    /// <summary>
    /// Creates an Option from a string, returning None if null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>Some if string has content; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> NonWhiteSpace(string? value) 
        => value.ToOptionNotWhiteSpace();

    /// <summary>
    /// Creates an Option from a trimmed string, returning None if result is empty.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>Some containing trimmed string if not empty; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> Trimmed(string? value) 
        => value.ToOptionTrimmed();

    /// <summary>
    /// Gets an Option from a dictionary by key.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some if key exists; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TValue> Dictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) 
        => dictionary.GetOption(key);

    /// <summary>
    /// Creates an Option based on a condition.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The value to wrap if condition is true.</param>
    /// <returns>Some containing value if condition is true; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> When<T>(bool condition, T value) 
        => OptionExtensions.When(condition, value);

    /// <summary>
    /// Creates an Option based on a condition, with lazy value creation.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="factory">The factory to create the value if condition is true.</param>
    /// <returns>Some containing factory result if condition is true; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> When<T>(bool condition, Func<T> factory) 
        => OptionExtensions.When(condition, factory);

    /// <summary>
    /// Creates an Option based on a negated condition.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="condition">The condition to evaluate (negated).</param>
    /// <param name="value">The value to wrap if condition is false.</param>
    /// <returns>Some containing value if condition is false; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Unless<T>(bool condition, T value) 
        => OptionExtensions.Unless(condition, value);

    /// <summary>
    /// Creates an Option based on a negated condition, with lazy value creation.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="condition">The condition to evaluate (negated).</param>
    /// <param name="factory">The factory to create the value if condition is false.</param>
    /// <returns>Some containing factory result if condition is false; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Unless<T>(bool condition, Func<T> factory) 
        => OptionExtensions.Unless(condition, factory);
}

