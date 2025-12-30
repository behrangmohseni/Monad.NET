#if NET6_0_OR_GREATER
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Span-based parsing extension methods for creating Option types.
/// These methods provide zero-allocation parsing for performance-critical scenarios.
/// Available in .NET 6.0 and later.
/// </summary>
public static class OptionParseSpanExtensions
{
    /// <summary>
    /// Attempts to parse a span as an integer.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed integer if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<int> ParseInt(this ReadOnlySpan<char> span)
    {
        return int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<int>.Some(result)
            : Option<int>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a long integer.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed long if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<long> ParseLong(this ReadOnlySpan<char> span)
    {
        return long.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<long>.Some(result)
            : Option<long>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a double.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed double if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<double> ParseDouble(this ReadOnlySpan<char> span)
    {
        return double.TryParse(span, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result)
            ? Option<double>.Some(result)
            : Option<double>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a decimal.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed decimal if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<decimal> ParseDecimal(this ReadOnlySpan<char> span)
    {
        return decimal.TryParse(span, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? Option<decimal>.Some(result)
            : Option<decimal>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a boolean.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed boolean if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<bool> ParseBool(this ReadOnlySpan<char> span)
    {
        return bool.TryParse(span, out var result)
            ? Option<bool>.Some(result)
            : Option<bool>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a GUID.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed GUID if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Guid> ParseGuid(this ReadOnlySpan<char> span)
    {
        return Guid.TryParse(span, out var result)
            ? Option<Guid>.Some(result)
            : Option<Guid>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a DateTime.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed DateTime if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateTime> ParseDateTime(this ReadOnlySpan<char> span)
    {
        return DateTime.TryParse(span, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? Option<DateTime>.Some(result)
            : Option<DateTime>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a DateTimeOffset.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed DateTimeOffset if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateTimeOffset> ParseDateTimeOffset(this ReadOnlySpan<char> span)
    {
        return DateTimeOffset.TryParse(span, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? Option<DateTimeOffset>.Some(result)
            : Option<DateTimeOffset>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a TimeSpan.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed TimeSpan if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TimeSpan> ParseTimeSpan(this ReadOnlySpan<char> span)
    {
        return TimeSpan.TryParse(span, CultureInfo.InvariantCulture, out var result)
            ? Option<TimeSpan>.Some(result)
            : Option<TimeSpan>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a DateOnly.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed DateOnly if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateOnly> ParseDateOnly(this ReadOnlySpan<char> span)
    {
        return DateOnly.TryParse(span, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? Option<DateOnly>.Some(result)
            : Option<DateOnly>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a TimeOnly.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed TimeOnly if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TimeOnly> ParseTimeOnly(this ReadOnlySpan<char> span)
    {
        return TimeOnly.TryParse(span, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? Option<TimeOnly>.Some(result)
            : Option<TimeOnly>.None();
    }

    /// <summary>
    /// Attempts to parse a span as an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to.</typeparam>
    /// <param name="span">The span to parse.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing. Default is true.</param>
    /// <returns>Some containing the parsed enum if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TEnum> ParseEnum<TEnum>(this ReadOnlySpan<char> span, bool ignoreCase = true) 
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(span, ignoreCase, out var result)
            ? Option<TEnum>.Some(result)
            : Option<TEnum>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a float.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed float if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<float> ParseFloat(this ReadOnlySpan<char> span)
    {
        return float.TryParse(span, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result)
            ? Option<float>.Some(result)
            : Option<float>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a byte.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed byte if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<byte> ParseByte(this ReadOnlySpan<char> span)
    {
        return byte.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<byte>.Some(result)
            : Option<byte>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a short.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed short if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<short> ParseShort(this ReadOnlySpan<char> span)
    {
        return short.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<short>.Some(result)
            : Option<short>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a ushort.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed ushort if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<ushort> ParseUShort(this ReadOnlySpan<char> span)
    {
        return ushort.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<ushort>.Some(result)
            : Option<ushort>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a uint.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed uint if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<uint> ParseUInt(this ReadOnlySpan<char> span)
    {
        return uint.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<uint>.Some(result)
            : Option<uint>.None();
    }

    /// <summary>
    /// Attempts to parse a span as a ulong.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed ulong if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<ulong> ParseULong(this ReadOnlySpan<char> span)
    {
        return ulong.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<ulong>.Some(result)
            : Option<ulong>.None();
    }

    /// <summary>
    /// Attempts to parse a span as an sbyte.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>Some containing the parsed sbyte if successful; otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<sbyte> ParseSByte(this ReadOnlySpan<char> span)
    {
        return sbyte.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<sbyte>.Some(result)
            : Option<sbyte>.None();
    }
}

/// <summary>
/// Static helper methods for parsing ReadOnlySpan&lt;char&gt; into Option types.
/// Provides an alternative entry point for discovery via <c>OptionParseSpan.</c> instead of extension methods.
/// </summary>
/// <example>
/// <code>
/// ReadOnlySpan&lt;char&gt; input = "42";
/// var age = OptionParseSpan.Int(input);           // Some(42)
/// var guid = OptionParseSpan.Guid(userInput);     // Some(Guid) or None
/// </code>
/// </example>
public static class OptionParseSpan
{
    /// <summary>Attempts to parse a span as an integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<int> Int(ReadOnlySpan<char> span) => span.ParseInt();

    /// <summary>Attempts to parse a span as a long.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<long> Long(ReadOnlySpan<char> span) => span.ParseLong();

    /// <summary>Attempts to parse a span as a double.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<double> Double(ReadOnlySpan<char> span) => span.ParseDouble();

    /// <summary>Attempts to parse a span as a decimal.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<decimal> Decimal(ReadOnlySpan<char> span) => span.ParseDecimal();

    /// <summary>Attempts to parse a span as a boolean.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<bool> Bool(ReadOnlySpan<char> span) => span.ParseBool();

    /// <summary>Attempts to parse a span as a GUID.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Guid> Guid(ReadOnlySpan<char> span) => span.ParseGuid();

    /// <summary>Attempts to parse a span as a DateTime.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateTime> DateTime(ReadOnlySpan<char> span) => span.ParseDateTime();

    /// <summary>Attempts to parse a span as a DateTimeOffset.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateTimeOffset> DateTimeOffset(ReadOnlySpan<char> span) => span.ParseDateTimeOffset();

    /// <summary>Attempts to parse a span as a TimeSpan.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TimeSpan> TimeSpan(ReadOnlySpan<char> span) => span.ParseTimeSpan();

    /// <summary>Attempts to parse a span as a DateOnly.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<DateOnly> DateOnly(ReadOnlySpan<char> span) => span.ParseDateOnly();

    /// <summary>Attempts to parse a span as a TimeOnly.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TimeOnly> TimeOnly(ReadOnlySpan<char> span) => span.ParseTimeOnly();

    /// <summary>Attempts to parse a span as an enum value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TEnum> Enum<TEnum>(ReadOnlySpan<char> span, bool ignoreCase = true) where TEnum : struct, Enum 
        => span.ParseEnum<TEnum>(ignoreCase);

    /// <summary>Attempts to parse a span as a float.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<float> Float(ReadOnlySpan<char> span) => span.ParseFloat();

    /// <summary>Attempts to parse a span as a byte.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<byte> Byte(ReadOnlySpan<char> span) => span.ParseByte();

    /// <summary>Attempts to parse a span as a short.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<short> Short(ReadOnlySpan<char> span) => span.ParseShort();

    /// <summary>Attempts to parse a span as a ushort.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<ushort> UShort(ReadOnlySpan<char> span) => span.ParseUShort();

    /// <summary>Attempts to parse a span as a uint.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<uint> UInt(ReadOnlySpan<char> span) => span.ParseUInt();

    /// <summary>Attempts to parse a span as a ulong.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<ulong> ULong(ReadOnlySpan<char> span) => span.ParseULong();

    /// <summary>Attempts to parse a span as an sbyte.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<sbyte> SByte(ReadOnlySpan<char> span) => span.ParseSByte();
}
#endif

