using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Monad.NET;

/// <summary>
/// Represents a computation that produces a value along with accumulated output (like logs).
/// The Writer monad allows you to accumulate values (logs, diagnostics, etc.) alongside computations.
/// Useful for logging, tracing, and collecting metadata without side effects.
/// </summary>
/// <typeparam name="TLog">The type of the accumulated output (must be a monoid)</typeparam>
/// <typeparam name="T">The type of the value</typeparam>
public readonly struct Writer<TLog, T> : IEquatable<Writer<TLog, T>>
{
    private readonly T _value;
    private readonly TLog _log;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Writer(T value, TLog log)
    {
        _value = value;
        _log = log;
    }

    /// <summary>
    /// Gets the computed value.
    /// </summary>
    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    /// <summary>
    /// Gets the accumulated log/output.
    /// </summary>
    public TLog Log
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _log;
    }

    /// <summary>
    /// Creates a Writer with a value and empty log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<TLog, T> Of(T value, TLog emptyLog)
    {
        ThrowHelper.ThrowIfNull(value);
        ThrowHelper.ThrowIfNull(emptyLog);

        return new Writer<TLog, T>(value, emptyLog);
    }

    /// <summary>
    /// Creates a Writer with a value and associated log entry.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<TLog, T> Tell(T value, TLog log)
    {
        ThrowHelper.ThrowIfNull(value);
        ThrowHelper.ThrowIfNull(log);

        return new Writer<TLog, T>(value, log);
    }

    /// <summary>
    /// Creates a Writer with just a log entry (value is Unit).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<TLog, Unit> TellUnit(TLog log)
    {
        ThrowHelper.ThrowIfNull(log);

        return new Writer<TLog, Unit>(Unit.Default, log);
    }

    /// <summary>
    /// Maps the value while preserving the log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Writer<TLog, U> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        return new Writer<TLog, U>(mapper(_value), _log);
    }

    /// <summary>
    /// Executes an action with the computed value without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the computed value.</param>
    /// <returns>The original Writer unchanged.</returns>
    /// <example>
    /// <code>
    /// writer.Tap(x => Console.WriteLine($"Value: {x}"))
    ///       .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Writer<TLog, T> Tap(Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        action(_value);
        return this;
    }

    /// <summary>
    /// Executes an action with the log without modifying the result, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute with the log.</param>
    /// <returns>The original Writer unchanged.</returns>
    /// <example>
    /// <code>
    /// writer.TapLog(log => Console.WriteLine($"Log: {log}"))
    ///       .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Writer<TLog, T> TapLog(Action<TLog> action)
    {
        ThrowHelper.ThrowIfNull(action);

        action(_log);
        return this;
    }

    /// <summary>
    /// Chains Writer computations, combining their logs.
    /// Requires a function to combine logs (append operation).
    /// This is the monadic bind operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Writer<TLog, U> Bind<U>(Func<T, Writer<TLog, U>> binder, Func<TLog, TLog, TLog> combine)
    {
        ThrowHelper.ThrowIfNull(binder);
        ThrowHelper.ThrowIfNull(combine);

        var result = binder(_value);
        var combinedLog = combine(_log, result._log);
        return new Writer<TLog, U>(result._value, combinedLog);
    }

    /// <summary>
    /// Maps both the value and the log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Writer<ULog, U> BiMap<ULog, U>(Func<TLog, ULog> logMapper, Func<T, U> valueMapper)
    {
        ThrowHelper.ThrowIfNull(logMapper);
        ThrowHelper.ThrowIfNull(valueMapper);

        return new Writer<ULog, U>(valueMapper(_value), logMapper(_log));
    }

    /// <summary>
    /// Maps only the log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Writer<ULog, T> MapLog<ULog>(Func<TLog, ULog> logMapper)
    {
        ThrowHelper.ThrowIfNull(logMapper);

        return new Writer<ULog, T>(_value, logMapper(_log));
    }

    /// <summary>
    /// Extracts the value and log as a tuple.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (T Value, TLog Log) Run()
    {
        return (_value, _log);
    }

    /// <summary>
    /// Executes an action with the value and log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(Action<T, TLog> action)
    {
        ThrowHelper.ThrowIfNull(action);

        action(_value, _log);
    }

    /// <summary>
    /// Pattern matches and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(Func<T, TLog, U> func)
    {
        ThrowHelper.ThrowIfNull(func);

        return func(_value, _log);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Writer<TLog, T> other)
    {
        return EqualityComparer<T>.Default.Equals(_value, other._value)
               && EqualityComparer<TLog>.Default.Equals(_log, other._log);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Writer<TLog, T> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(_value, _log);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Writer(Value: {_value}, Log: {_log})";
    }

    /// <summary>
    /// Determines whether two Writer instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Writer<TLog, T> left, Writer<TLog, T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Writer instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Writer<TLog, T> left, Writer<TLog, T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the Writer into its components for pattern matching.
    /// </summary>
    /// <param name="value">The computed value.</param>
    /// <param name="log">The accumulated log.</param>
    /// <example>
    /// <code>
    /// var (value, log) = writer;
    /// Console.WriteLine($"Value: {value}, Log: {log}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T value, out TLog log)
    {
        value = _value;
        log = _log;
    }
}

/// <summary>
/// Extension methods and helpers for Writer monad.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WriterExtensions
{
    /// <summary>
    /// Creates a Writer for string logs (most common case).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, T> WithLog<T>(this T value, string log)
    {
        ThrowHelper.ThrowIfNull(value);

        return Writer<string, T>.Tell(value, log ?? string.Empty);
    }

    /// <summary>
    /// Creates a Writer with an empty string log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, T> ToWriter<T>(this T value)
    {
        ThrowHelper.ThrowIfNull(value);

        return Writer<string, T>.Of(value, string.Empty);
    }

    /// <summary>
    /// Bind for string-based Writers (concatenates logs).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, U> Bind<T, U>(
        this Writer<string, T> writer,
        Func<T, Writer<string, U>> binder)
    {
        return writer.Bind(binder, static (log1, log2) => log1 + log2);
    }

    /// <summary>
    /// Bind for List-based Writers (concatenates lists).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<List<TLog>, U> Bind<T, U, TLog>(
        this Writer<List<TLog>, T> writer,
        Func<T, Writer<List<TLog>, U>> binder)
    {
        return writer.Bind(binder, static (log1, log2) =>
        {
            var combined = new List<TLog>(log1);
            combined.AddRange(log2);
            return combined;
        });
    }

    /// <summary>
    /// Executes a side effect with the value, adding a log entry.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, T> TapLog<T>(
        this Writer<string, T> writer,
        Func<T, string> logger)
    {
        ThrowHelper.ThrowIfNull(logger);

        var additionalLog = logger(writer.Value);
        return Writer<string, T>.Tell(writer.Value, writer.Log + additionalLog);
    }

    /// <summary>
    /// Sequences a collection of Writers, combining all logs.
    /// </summary>
    public static Writer<string, IEnumerable<T>> Sequence<T>(
        this IEnumerable<Writer<string, T>> writers)
    {
        ThrowHelper.ThrowIfNull(writers);

        var values = new List<T>();
        var logBuilder = new StringBuilder();

        foreach (var writer in writers)
        {
            values.Add(writer.Value);
            logBuilder.Append(writer.Log);
        }

        return Writer<string, IEnumerable<T>>.Tell(values, logBuilder.ToString());
    }

    /// <summary>
    /// Sequences a collection of Writers with list-based logs.
    /// </summary>
    public static Writer<List<TLog>, IEnumerable<T>> Sequence<T, TLog>(
        this IEnumerable<Writer<List<TLog>, T>> writers)
    {
        ThrowHelper.ThrowIfNull(writers);

        var values = new List<T>();
        var combinedLog = new List<TLog>();

        foreach (var writer in writers)
        {
            values.Add(writer.Value);
            combinedLog.AddRange(writer.Log);
        }

        return Writer<List<TLog>, IEnumerable<T>>.Tell(values, combinedLog);
    }
}

/// <summary>
/// Builder for string-based Writer monad (most common case).
/// </summary>
public static class StringWriter
{
    /// <summary>
    /// Creates a Writer with a value and no log.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, T> Return<T>(T value)
    {
        ThrowHelper.ThrowIfNull(value);

        return Writer<string, T>.Of(value, string.Empty);
    }

    /// <summary>
    /// Creates a Writer with a value and log message.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, T> Tell<T>(T value, string message)
    {
        ThrowHelper.ThrowIfNull(value);

        return Writer<string, T>.Tell(value, message ?? string.Empty);
    }

    /// <summary>
    /// Creates a log-only Writer (no meaningful value).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<string, Unit> Log(string message)
    {
        return Writer<string, Unit>.Tell(Unit.Default, message ?? string.Empty);
    }
}

/// <summary>
/// Builder for list-based Writer monad.
/// </summary>
public static class ListWriter
{
    /// <summary>
    /// Creates a Writer with a value and empty log list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<List<TLog>, T> Return<T, TLog>(T value)
    {
        ThrowHelper.ThrowIfNull(value);

        return Writer<List<TLog>, T>.Of(value, new List<TLog>());
    }

    /// <summary>
    /// Creates a Writer with a value and log entry.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<List<TLog>, T> Tell<T, TLog>(T value, TLog logEntry)
    {
        ThrowHelper.ThrowIfNull(value);
        ThrowHelper.ThrowIfNull(logEntry);

        return Writer<List<TLog>, T>.Tell(value, new List<TLog> { logEntry });
    }

    /// <summary>
    /// Creates a Writer with a value and multiple log entries.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Writer<List<TLog>, T> Tell<T, TLog>(T value, params TLog[] logEntries)
    {
        ThrowHelper.ThrowIfNull(value);

        return Writer<List<TLog>, T>.Tell(value, new List<TLog>(logEntries ?? Array.Empty<TLog>()));
    }
}
