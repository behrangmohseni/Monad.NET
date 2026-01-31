namespace Monad.NET;

/// <summary>
/// Represents a monoid - a type with an identity element and an associative binary operation.
/// Used with Writer monad to accumulate log values in a structured manner.
/// </summary>
/// <typeparam name="T">The type that forms a monoid</typeparam>
/// <remarks>
/// A monoid satisfies three laws:
/// <list type="bullet">
///   <item><description>Left identity: Empty.Append(x) == x</description></item>
///   <item><description>Right identity: x.Append(Empty) == x</description></item>
///   <item><description>Associativity: (x.Append(y)).Append(z) == x.Append(y.Append(z))</description></item>
/// </list>
/// 
/// The Writer monad works best with monoid log types. While not enforced at compile time
/// for broader framework compatibility, you should ensure your log type follows monoid laws.
/// Use the built-in StringMonoid or ListMonoid&lt;T&gt; for common cases.
/// </remarks>
/// <example>
/// <code>
/// // Using the built-in StringMonoid:
/// var writer = Writer&lt;StringMonoid, int&gt;.Tell(42, new StringMonoid("Started"));
/// var result = writer.Bind(x => Writer&lt;StringMonoid, int&gt;.Tell(x * 2, new StringMonoid(" doubled")),
///     (a, b) => a.Append(b));
/// 
/// // Using the built-in ListMonoid:
/// var writer = Writer&lt;ListMonoid&lt;string&gt;, int&gt;.Tell(42, ListMonoid.Of("Log entry"));
/// </code>
/// </example>
public interface IMonoid<T>
{
    /// <summary>
    /// Combines this value with another value of the same type.
    /// </summary>
    /// <param name="other">The value to append.</param>
    /// <returns>The combined value.</returns>
    T Append(T other);
}

/// <summary>
/// A monoid wrapper for strings that concatenates values.
/// </summary>
/// <example>
/// <code>
/// var log = new StringMonoid("Hello, ");
/// var combined = log.Append(new StringMonoid("World!"));
/// // combined.Value == "Hello, World!"
/// </code>
/// </example>
public readonly struct StringMonoid : IMonoid<StringMonoid>, IEquatable<StringMonoid>, IComparable<StringMonoid>
{
    private readonly string? _value;

    /// <summary>
    /// The underlying string value. Returns empty string for default struct.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Creates a new StringMonoid with the specified value.
    /// </summary>
    /// <param name="value">The string value. Null is treated as empty string.</param>
    public StringMonoid(string? value)
    {
        _value = value ?? string.Empty;
    }

    /// <summary>
    /// Gets the identity element (empty string).
    /// </summary>
    public static StringMonoid Empty => new(string.Empty);

    /// <inheritdoc />
    public StringMonoid Append(StringMonoid other) => new(Value + other.Value);

    /// <summary>
    /// Implicitly converts a string to a StringMonoid.
    /// </summary>
    public static implicit operator StringMonoid(string? value) => new(value);

    /// <summary>
    /// Implicitly converts a StringMonoid to a string.
    /// </summary>
    public static implicit operator string(StringMonoid monoid) => monoid.Value;

    /// <inheritdoc />
    public bool Equals(StringMonoid other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is StringMonoid other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Compares this StringMonoid to another using ordinal string comparison.
    /// </summary>
    public int CompareTo(StringMonoid other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two StringMonoid instances are equal.
    /// </summary>
    public static bool operator ==(StringMonoid left, StringMonoid right) => left.Equals(right);

    /// <summary>
    /// Determines whether two StringMonoid instances are not equal.
    /// </summary>
    public static bool operator !=(StringMonoid left, StringMonoid right) => !left.Equals(right);

    /// <summary>
    /// Determines whether the left StringMonoid is less than the right.
    /// </summary>
    public static bool operator <(StringMonoid left, StringMonoid right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left StringMonoid is less than or equal to the right.
    /// </summary>
    public static bool operator <=(StringMonoid left, StringMonoid right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left StringMonoid is greater than the right.
    /// </summary>
    public static bool operator >(StringMonoid left, StringMonoid right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left StringMonoid is greater than or equal to the right.
    /// </summary>
    public static bool operator >=(StringMonoid left, StringMonoid right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// A monoid wrapper for lists that concatenates elements.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <example>
/// <code>
/// var log = ListMonoid.Of("First");
/// var combined = log.Append(ListMonoid.Of("Second"));
/// // combined.Value contains ["First", "Second"]
/// </code>
/// </example>
public readonly struct ListMonoid<T> : IMonoid<ListMonoid<T>>, IEquatable<ListMonoid<T>>, IComparable<ListMonoid<T>>
{
    private readonly IReadOnlyList<T>? _value;

    /// <summary>
    /// The underlying list value.
    /// </summary>
    public IReadOnlyList<T> Value => _value ?? Array.Empty<T>();

    /// <summary>
    /// Creates a new ListMonoid with the specified values.
    /// </summary>
    /// <param name="values">The list values.</param>
    public ListMonoid(IEnumerable<T>? values)
    {
        _value = values?.ToList() ?? new List<T>();
    }

    /// <summary>
    /// Creates a new ListMonoid with a single value.
    /// </summary>
    /// <param name="value">The single value.</param>
    public ListMonoid(T value)
    {
        _value = new List<T> { value };
    }

    /// <summary>
    /// Gets the identity element (empty list).
    /// </summary>
    public static ListMonoid<T> Empty => new(Array.Empty<T>());

    /// <inheritdoc />
    public ListMonoid<T> Append(ListMonoid<T> other)
    {
        var combined = new List<T>(Value);
        combined.AddRange(other.Value);
        return new ListMonoid<T>(combined);
    }

    /// <inheritdoc />
    public bool Equals(ListMonoid<T> other)
    {
        return Value.SequenceEqual(other.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ListMonoid<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in Value)
            hash.Add(item);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => $"[{string.Join(", ", Value)}]";

    /// <summary>
    /// Compares this ListMonoid to another using lexicographic ordering.
    /// Elements are compared in order; a shorter sequence is considered smaller
    /// only when it is a prefix of the longer sequence.
    /// </summary>
    /// <example>
    /// <code>
    /// // ["a"] &lt; ["a", "b"] (prefix)
    /// // ["z"] &gt; ["a", "b"] (because 'z' &gt; 'a')
    /// // ["a", "c"] &gt; ["a", "b"] (because 'c' &gt; 'b')
    /// </code>
    /// </example>
    public int CompareTo(ListMonoid<T> other)
    {
        var comparer = Comparer<T>.Default;
        var minCount = Math.Min(Value.Count, other.Value.Count);

        // Compare elements lexicographically
        for (int i = 0; i < minCount; i++)
        {
            var elementComparison = comparer.Compare(Value[i], other.Value[i]);
            if (elementComparison != 0)
                return elementComparison;
        }

        // If all compared elements are equal, shorter sequence is smaller (it's a prefix)
        return Value.Count.CompareTo(other.Value.Count);
    }

    /// <summary>
    /// Determines whether two ListMonoid instances are equal.
    /// </summary>
    public static bool operator ==(ListMonoid<T> left, ListMonoid<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two ListMonoid instances are not equal.
    /// </summary>
    public static bool operator !=(ListMonoid<T> left, ListMonoid<T> right) => !left.Equals(right);

    /// <summary>
    /// Determines whether the left ListMonoid is less than the right.
    /// </summary>
    public static bool operator <(ListMonoid<T> left, ListMonoid<T> right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left ListMonoid is less than or equal to the right.
    /// </summary>
    public static bool operator <=(ListMonoid<T> left, ListMonoid<T> right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left ListMonoid is greater than the right.
    /// </summary>
    public static bool operator >(ListMonoid<T> left, ListMonoid<T> right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left ListMonoid is greater than or equal to the right.
    /// </summary>
    public static bool operator >=(ListMonoid<T> left, ListMonoid<T> right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// Factory methods for creating ListMonoid instances.
/// </summary>
public static class ListMonoid
{
    /// <summary>
    /// Creates a ListMonoid with a single value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to include.</param>
    /// <returns>A new ListMonoid containing the single value.</returns>
    public static ListMonoid<T> Of<T>(T value) => new(value);

    /// <summary>
    /// Creates a ListMonoid from multiple values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="values">The values to include.</param>
    /// <returns>A new ListMonoid containing all values.</returns>
    public static ListMonoid<T> Of<T>(params T[] values) => new(values);

    /// <summary>
    /// Creates an empty ListMonoid.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <returns>An empty ListMonoid.</returns>
    public static ListMonoid<T> Empty<T>() => ListMonoid<T>.Empty;
}
