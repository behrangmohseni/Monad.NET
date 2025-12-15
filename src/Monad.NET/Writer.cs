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

    private Writer(T value, TLog log)
    {
        _value = value;
        _log = log;
    }

    /// <summary>
    /// Gets the computed value.
    /// </summary>
    public T Value => _value;

    /// <summary>
    /// Gets the accumulated log/output.
    /// </summary>
    public TLog Log => _log;

    /// <summary>
    /// Creates a Writer with a value and empty log.
    /// </summary>
    public static Writer<TLog, T> Of(T value, TLog emptyLog)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        if (emptyLog is null)
            throw new ArgumentNullException(nameof(emptyLog));

        return new Writer<TLog, T>(value, emptyLog);
    }

    /// <summary>
    /// Creates a Writer with a value and associated log entry.
    /// </summary>
    public static Writer<TLog, T> Tell(T value, TLog log)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        if (log is null)
            throw new ArgumentNullException(nameof(log));

        return new Writer<TLog, T>(value, log);
    }

    /// <summary>
    /// Creates a Writer with just a log entry (value is Unit).
    /// </summary>
    public static Writer<TLog, Unit> TellUnit(TLog log)
    {
        if (log is null)
            throw new ArgumentNullException(nameof(log));

        return new Writer<TLog, Unit>(Unit.Default, log);
    }

    /// <summary>
    /// Maps the value while preserving the log.
    /// </summary>
    public Writer<TLog, U> Map<U>(Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        return new Writer<TLog, U>(mapper(_value), _log);
    }

    /// <summary>
    /// Chains Writer computations, combining their logs.
    /// Requires a function to combine logs (append operation).
    /// </summary>
    public Writer<TLog, U> FlatMap<U>(Func<T, Writer<TLog, U>> binder, Func<TLog, TLog, TLog> combine)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        if (combine is null)
            throw new ArgumentNullException(nameof(combine));

        var result = binder(_value);
        var combinedLog = combine(_log, result._log);
        return new Writer<TLog, U>(result._value, combinedLog);
    }

    /// <summary>
    /// Maps both the value and the log.
    /// </summary>
    public Writer<ULog, U> BiMap<ULog, U>(Func<TLog, ULog> logMapper, Func<T, U> valueMapper)
    {
        if (logMapper is null)
            throw new ArgumentNullException(nameof(logMapper));
        if (valueMapper is null)
            throw new ArgumentNullException(nameof(valueMapper));

        return new Writer<ULog, U>(valueMapper(_value), logMapper(_log));
    }

    /// <summary>
    /// Maps only the log.
    /// </summary>
    public Writer<ULog, T> MapLog<ULog>(Func<TLog, ULog> logMapper)
    {
        if (logMapper is null)
            throw new ArgumentNullException(nameof(logMapper));

        return new Writer<ULog, T>(_value, logMapper(_log));
    }

    /// <summary>
    /// Extracts the value and log as a tuple.
    /// </summary>
    public (T Value, TLog Log) Run()
    {
        return (_value, _log);
    }

    /// <summary>
    /// Executes an action with the value and log.
    /// </summary>
    public void Run(Action<T, TLog> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        action(_value, _log);
    }

    /// <summary>
    /// Pattern matches and returns a result.
    /// </summary>
    public U Match<U>(Func<T, TLog, U> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));

        return func(_value, _log);
    }

    /// <inheritdoc />
    public bool Equals(Writer<TLog, T> other)
    {
        return EqualityComparer<T>.Default.Equals(_value, other._value)
               && EqualityComparer<TLog>.Default.Equals(_log, other._log);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Writer<TLog, T> other && Equals(other);
    }

    /// <inheritdoc />
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
    public static bool operator ==(Writer<TLog, T> left, Writer<TLog, T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Writer instances are not equal.
    /// </summary>
    public static bool operator !=(Writer<TLog, T> left, Writer<TLog, T> right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Represents a unit type (void replacement) for Writer monad.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// The singleton Unit value.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// The default Unit value (alias for Value).
    /// </summary>
    public static readonly Unit Default = default;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;
    /// <inheritdoc />
    public override int GetHashCode() => 0;
    /// <inheritdoc />
    public override string ToString() => "()";

    /// <summary>
    /// Determines whether two Unit instances are equal (always true).
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;
    /// <summary>
    /// Determines whether two Unit instances are not equal (always false).
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}

/// <summary>
/// Extension methods and helpers for Writer monad.
/// </summary>
public static class WriterExtensions
{
    /// <summary>
    /// Creates a Writer for string logs (most common case).
    /// </summary>
    public static Writer<string, T> WithLog<T>(this T value, string log)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Writer<string, T>.Tell(value, log ?? string.Empty);
    }

    /// <summary>
    /// Creates a Writer with an empty string log.
    /// </summary>
    public static Writer<string, T> ToWriter<T>(this T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Writer<string, T>.Of(value, string.Empty);
    }

    /// <summary>
    /// FlatMap for string-based Writers (concatenates logs).
    /// </summary>
    public static Writer<string, U> FlatMap<T, U>(
        this Writer<string, T> writer,
        Func<T, Writer<string, U>> binder)
    {
        return writer.FlatMap(binder, (log1, log2) => log1 + log2);
    }

    /// <summary>
    /// FlatMap for List-based Writers (concatenates lists).
    /// </summary>
    public static Writer<List<TLog>, U> FlatMap<T, U, TLog>(
        this Writer<List<TLog>, T> writer,
        Func<T, Writer<List<TLog>, U>> binder)
    {
        return writer.FlatMap(binder, (log1, log2) =>
        {
            var combined = new List<TLog>(log1);
            combined.AddRange(log2);
            return combined;
        });
    }

    /// <summary>
    /// Executes a side effect with the value, adding a log entry.
    /// </summary>
    public static Writer<string, T> TapLog<T>(
        this Writer<string, T> writer,
        Func<T, string> logger)
    {
        if (logger is null)
            throw new ArgumentNullException(nameof(logger));

        var additionalLog = logger(writer.Value);
        return Writer<string, T>.Tell(writer.Value, writer.Log + additionalLog);
    }

    /// <summary>
    /// Sequences a collection of Writers, combining all logs.
    /// </summary>
    public static Writer<string, IEnumerable<T>> Sequence<T>(
        this IEnumerable<Writer<string, T>> writers)
    {
        if (writers is null)
            throw new ArgumentNullException(nameof(writers));

        var values = new List<T>();
        var combinedLog = string.Empty;

        foreach (var writer in writers)
        {
            values.Add(writer.Value);
            combinedLog += writer.Log;
        }

        return Writer<string, IEnumerable<T>>.Tell(values, combinedLog);
    }

    /// <summary>
    /// Sequences a collection of Writers with list-based logs.
    /// </summary>
    public static Writer<List<TLog>, IEnumerable<T>> Sequence<T, TLog>(
        this IEnumerable<Writer<List<TLog>, T>> writers)
    {
        if (writers is null)
            throw new ArgumentNullException(nameof(writers));

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
    public static Writer<string, T> Pure<T>(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Writer<string, T>.Of(value, string.Empty);
    }

    /// <summary>
    /// Creates a Writer with a value and log message.
    /// </summary>
    public static Writer<string, T> Tell<T>(T value, string message)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Writer<string, T>.Tell(value, message ?? string.Empty);
    }

    /// <summary>
    /// Creates a log-only Writer (no meaningful value).
    /// </summary>
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
    public static Writer<List<TLog>, T> Pure<T, TLog>(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Writer<List<TLog>, T>.Of(value, new List<TLog>());
    }

    /// <summary>
    /// Creates a Writer with a value and log entry.
    /// </summary>
    public static Writer<List<TLog>, T> Tell<T, TLog>(T value, TLog logEntry)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        if (logEntry is null)
            throw new ArgumentNullException(nameof(logEntry));

        return Writer<List<TLog>, T>.Tell(value, new List<TLog> { logEntry });
    }

    /// <summary>
    /// Creates a Writer with a value and multiple log entries.
    /// </summary>
    public static Writer<List<TLog>, T> Tell<T, TLog>(T value, params TLog[] logEntries)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Writer<List<TLog>, T>.Tell(value, new List<TLog>(logEntries ?? Array.Empty<TLog>()));
    }
}

