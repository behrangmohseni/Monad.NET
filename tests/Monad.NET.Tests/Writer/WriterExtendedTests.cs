using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Writer monad to improve code coverage.
/// </summary>
public class WriterExtendedTests
{
    #region ListWriter Tests

    [Fact]
    public void ListWriter_Tell_MultipleEntries_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => ListWriter.Tell<string, string>(null!, "a", "b"));
    }

    [Fact]
    public void ListWriter_Tell_MultipleEntries_HandlesNullArray()
    {
        var writer = ListWriter.Tell<int, string>(42, (string[]?)null!);

        Assert.Equal(42, writer.Value);
        Assert.Empty(writer.Log);
    }

    [Fact]
    public void ListWriter_FlatMap_CombinesLogs()
    {
        var writer1 = ListWriter.Tell(10, "step1", "step1b");
        var writer2 = writer1.Bind(x => ListWriter.Tell(x * 2, "step2", "step2b"));

        Assert.Equal(20, writer2.Value);
        Assert.Equal(4, writer2.Log.Count);
        Assert.Contains("step1", writer2.Log);
        Assert.Contains("step2", writer2.Log);
    }

    #endregion

    #region WriterExtensions Tests

    [Fact]
    public void WithLog_HandlesNullLog()
    {
        var writer = 42.WithLog(null!);

        Assert.Equal(42, writer.Value);
        Assert.Equal("", writer.Log);
    }

    [Fact]
    public void TapLog_AddsLogEntry()
    {
        var writer = 42.WithLog("initial");
        var result = writer.TapLog(x => $" - computed {x}");

        Assert.Equal(42, result.Value);
        Assert.Equal("initial - computed 42", result.Log);
    }

    [Fact]
    public void TapLog_ThrowsOnNullLogger()
    {
        var writer = 42.WithLog("log");
        Assert.Throws<ArgumentNullException>(() => writer.TapLog(null!));
    }

    [Fact]
    public void Sequence_StringWriters_CombinesLogsAndValues()
    {
        var writers = new[]
        {
            1.WithLog("a"),
            2.WithLog("b"),
            3.WithLog("c")
        };

        var result = writers.Sequence();

        Assert.Equal("abc", result.Log);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToList());
    }

    [Fact]
    public void Sequence_StringWriters_ThrowsOnNull()
    {
        IEnumerable<Writer<string, int>> nullWriters = null!;
        Assert.Throws<ArgumentNullException>(() => nullWriters.Sequence());
    }

    [Fact]
    public void Sequence_StringWriters_EmptyList()
    {
        var writers = Array.Empty<Writer<string, int>>();

        var result = writers.Sequence();

        Assert.Equal("", result.Log);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void Sequence_ListWriters_CombinesLogsAndValues()
    {
        var writers = new[]
        {
            ListWriter.Tell(1, "a", "a2"),
            ListWriter.Tell(2, "b"),
            ListWriter.Tell(3, "c")
        };

        var result = writers.Sequence();

        Assert.Equal(new[] { "a", "a2", "b", "c" }, result.Log);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToList());
    }

    [Fact]
    public void Sequence_ListWriters_ThrowsOnNull()
    {
        IEnumerable<Writer<List<string>, int>> nullWriters = null!;
        Assert.Throws<ArgumentNullException>(() => nullWriters.Sequence());
    }

    #endregion

    #region Writer BiMap and MapLog Tests

    [Fact]
    public void BiMap_TransformsBothValueAndLog()
    {
        var writer = Writer<string, int>.Tell(10, "hello");
        var result = writer.BiMap(log => log.ToUpper(), val => val * 2);

        Assert.Equal(20, result.Value);
        Assert.Equal("HELLO", result.Log);
    }

    [Fact]
    public void BiMap_ThrowsOnNullLogMapper()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() =>
            writer.BiMap<string, int>(null!, val => val));
    }

    [Fact]
    public void BiMap_ThrowsOnNullValueMapper()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() =>
            writer.BiMap<string, int>(log => log, null!));
    }

    [Fact]
    public void MapLog_TransformsLogOnly()
    {
        var writer = Writer<string, int>.Tell(42, "hello");
        var result = writer.MapLog(log => log.Length);

        Assert.Equal(42, result.Value);
        Assert.Equal(5, result.Log);
    }

    [Fact]
    public void MapLog_ThrowsOnNullMapper()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() => writer.MapLog<string>(null!));
    }

    #endregion

    #region Writer Run Tests

    [Fact]
    public void Run_ReturnsValueAndLog()
    {
        var writer = Writer<string, int>.Tell(42, "mylog");
        var (value, log) = writer.Run();

        Assert.Equal(42, value);
        Assert.Equal("mylog", log);
    }

    [Fact]
    public void Run_WithAction_ExecutesAction()
    {
        var writer = Writer<string, int>.Tell(42, "mylog");
        int capturedValue = 0;
        string capturedLog = "";

        writer.Run((v, l) =>
        {
            capturedValue = v;
            capturedLog = l;
        });

        Assert.Equal(42, capturedValue);
        Assert.Equal("mylog", capturedLog);
    }

    [Fact]
    public void Run_WithAction_ThrowsOnNullAction()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() => writer.Run((Action<int, string>)null!));
    }

    #endregion

    #region Writer Deconstruct Tests

    [Fact]
    public void Deconstruct_WorksCorrectly()
    {
        var writer = Writer<string, int>.Tell(42, "log");

        var (value, log) = writer;

        Assert.Equal(42, value);
        Assert.Equal("log", log);
    }

    #endregion

    #region Writer IComparable Tests

    [Fact]
    public void CompareTo_ReturnsZeroForEqualWriters()
    {
        var writer1 = Writer<string, int>.Tell(42, "log");
        var writer2 = Writer<string, int>.Tell(42, "log");

        Assert.Equal(0, writer1.CompareTo(writer2));
    }

    [Fact]
    public void CompareTo_ReturnsNegativeWhenValueIsLess()
    {
        var writer1 = Writer<string, int>.Tell(10, "log");
        var writer2 = Writer<string, int>.Tell(20, "log");

        Assert.True(writer1.CompareTo(writer2) < 0);
    }

    [Fact]
    public void CompareTo_ReturnsPositiveWhenValueIsGreater()
    {
        var writer1 = Writer<string, int>.Tell(30, "log");
        var writer2 = Writer<string, int>.Tell(20, "log");

        Assert.True(writer1.CompareTo(writer2) > 0);
    }

    [Fact]
    public void CompareTo_ComparesLogWhenValuesAreEqual()
    {
        var writer1 = Writer<string, int>.Tell(42, "aaa");
        var writer2 = Writer<string, int>.Tell(42, "bbb");

        Assert.True(writer1.CompareTo(writer2) < 0);
    }

    [Fact]
    public void LessThanOperator_Works()
    {
        var writer1 = Writer<string, int>.Tell(10, "log");
        var writer2 = Writer<string, int>.Tell(20, "log");

        Assert.True(writer1 < writer2);
        Assert.False(writer2 < writer1);
    }

    [Fact]
    public void LessThanOrEqualOperator_Works()
    {
        var writer1 = Writer<string, int>.Tell(10, "log");
        var writer2 = Writer<string, int>.Tell(10, "log");
        var writer3 = Writer<string, int>.Tell(20, "log");

        Assert.True(writer1 <= writer2);
        Assert.True(writer1 <= writer3);
        Assert.False(writer3 <= writer1);
    }

    [Fact]
    public void GreaterThanOperator_Works()
    {
        var writer1 = Writer<string, int>.Tell(20, "log");
        var writer2 = Writer<string, int>.Tell(10, "log");

        Assert.True(writer1 > writer2);
        Assert.False(writer2 > writer1);
    }

    [Fact]
    public void GreaterThanOrEqualOperator_Works()
    {
        var writer1 = Writer<string, int>.Tell(20, "log");
        var writer2 = Writer<string, int>.Tell(20, "log");
        var writer3 = Writer<string, int>.Tell(10, "log");

        Assert.True(writer1 >= writer2);
        Assert.True(writer1 >= writer3);
        Assert.False(writer3 >= writer1);
    }

    #endregion

    #region StringMonoid Tests

    [Fact]
    public void StringMonoid_Empty_ReturnsEmptyString()
    {
        var empty = StringMonoid.Empty;

        Assert.Equal("", empty.Value);
    }

    [Fact]
    public void StringMonoid_Append_ConcatenatesStrings()
    {
        var a = new StringMonoid("Hello, ");
        var b = new StringMonoid("World!");

        var result = a.Append(b);

        Assert.Equal("Hello, World!", result.Value);
    }

    [Fact]
    public void StringMonoid_ImplicitConversionFromString_Works()
    {
        StringMonoid monoid = "test";

        Assert.Equal("test", monoid.Value);
    }

    [Fact]
    public void StringMonoid_ImplicitConversionToString_Works()
    {
        var monoid = new StringMonoid("test");
        string value = monoid;

        Assert.Equal("test", value);
    }

    [Fact]
    public void StringMonoid_HandlesNull_AsEmptyString()
    {
        var monoid = new StringMonoid(null);

        Assert.Equal("", monoid.Value);
    }

    [Fact]
    public void StringMonoid_Equals_Works()
    {
        var a = new StringMonoid("test");
        var b = new StringMonoid("test");
        var c = new StringMonoid("other");

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a.Equals(c));
        Assert.True(a != c);
    }

    [Fact]
    public void StringMonoid_GetHashCode_SameForEqualValues()
    {
        var a = new StringMonoid("test");
        var b = new StringMonoid("test");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void StringMonoid_ToString_ReturnsValue()
    {
        var monoid = new StringMonoid("hello");

        Assert.Equal("hello", monoid.ToString());
    }

    [Fact]
    public void StringMonoid_Default_ReturnsEmptyString()
    {
        var monoid = default(StringMonoid);

        Assert.Equal("", monoid.Value);
        Assert.Equal("", monoid.ToString());
    }

    [Fact]
    public void StringMonoid_CompareTo_Works()
    {
        var a = new StringMonoid("aaa");
        var b = new StringMonoid("bbb");
        var c = new StringMonoid("aaa");

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(c));
    }

    [Fact]
    public void StringMonoid_ComparisonOperators_Work()
    {
        var a = new StringMonoid("aaa");
        var b = new StringMonoid("bbb");

        Assert.True(a < b);
        Assert.True(a <= b);
        Assert.False(a > b);
        Assert.False(a >= b);
        Assert.True(b > a);
        Assert.True(b >= a);
    }

    [Fact]
    public void StringMonoid_Bind_Works()
    {
        var writer = Writer<StringMonoid, int>.Tell(10, new StringMonoid("a"));
        var result = writer.Bind(x => Writer<StringMonoid, int>.Tell(x * 2, new StringMonoid("b")));

        Assert.Equal(20, result.Value);
        Assert.Equal("ab", result.Log.Value);
    }

    #endregion

    #region ListMonoid Tests

    [Fact]
    public void ListMonoid_Empty_ReturnsEmptyList()
    {
        var empty = ListMonoid<string>.Empty;

        Assert.Empty(empty.Value);
    }

    [Fact]
    public void ListMonoid_Of_CreatesSingleElementList()
    {
        var monoid = ListMonoid.Of("test");

        Assert.Single(monoid.Value);
        Assert.Equal("test", monoid.Value[0]);
    }

    [Fact]
    public void ListMonoid_Of_CreatesMultiElementList()
    {
        var monoid = ListMonoid.Of("a", "b", "c");

        Assert.Equal(3, monoid.Value.Count);
        Assert.Equal(new[] { "a", "b", "c" }, monoid.Value);
    }

    [Fact]
    public void ListMonoid_Append_CombinesLists()
    {
        var a = ListMonoid.Of("a", "b");
        var b = ListMonoid.Of("c", "d");

        var result = a.Append(b);

        Assert.Equal(new[] { "a", "b", "c", "d" }, result.Value);
    }

    [Fact]
    public void ListMonoid_Equals_Works()
    {
        var a = ListMonoid.Of("a", "b");
        var b = ListMonoid.Of("a", "b");
        var c = ListMonoid.Of("a", "c");

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a.Equals(c));
        Assert.True(a != c);
    }

    [Fact]
    public void ListMonoid_GetHashCode_SameForEqualValues()
    {
        var a = ListMonoid.Of("a", "b");
        var b = ListMonoid.Of("a", "b");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ListMonoid_ToString_FormatsCorrectly()
    {
        var monoid = ListMonoid.Of("a", "b", "c");

        Assert.Equal("[a, b, c]", monoid.ToString());
    }

    [Fact]
    public void ListMonoid_Empty_FactoryMethod_Works()
    {
        var empty = ListMonoid.Empty<int>();

        Assert.Empty(empty.Value);
    }

    [Fact]
    public void ListMonoid_Bind_Works()
    {
        var writer = Writer<ListMonoid<string>, int>.Tell(10, ListMonoid.Of("step1"));
        var result = writer.Bind(x => Writer<ListMonoid<string>, int>.Tell(x * 2, ListMonoid.Of("step2")));

        Assert.Equal(20, result.Value);
        Assert.Equal(new[] { "step1", "step2" }, result.Log.Value);
    }

    [Fact]
    public void ListMonoid_HandlesNullEnumerable_AsEmptyList()
    {
        var monoid = new ListMonoid<string>((IEnumerable<string>?)null);

        Assert.Empty(monoid.Value);
    }

    [Fact]
    public void ListMonoid_Default_ReturnsEmptyList()
    {
        var monoid = default(ListMonoid<string>);

        Assert.Empty(monoid.Value);
        Assert.Equal("[]", monoid.ToString());
    }

    [Fact]
    public void ListMonoid_CompareTo_ByCount()
    {
        var a = ListMonoid.Of("a");
        var b = ListMonoid.Of("a", "b");

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
    }

    [Fact]
    public void ListMonoid_CompareTo_ByElement()
    {
        var a = ListMonoid.Of("a", "a");
        var b = ListMonoid.Of("a", "b");

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
    }

    [Fact]
    public void ListMonoid_CompareTo_Equal()
    {
        var a = ListMonoid.Of("a", "b");
        var b = ListMonoid.Of("a", "b");

        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void ListMonoid_ComparisonOperators_Work()
    {
        var a = ListMonoid.Of("a");
        var b = ListMonoid.Of("a", "b");

        Assert.True(a < b);
        Assert.True(a <= b);
        Assert.False(a > b);
        Assert.False(a >= b);
        Assert.True(b > a);
        Assert.True(b >= a);
    }

    #endregion

    #region Writer with Monoid Comparison Tests

    [Fact]
    public void Writer_StringMonoid_CompareTo_WithDifferentLogs()
    {
        // This was the main bug: Writer.CompareTo would fail with monoid types
        var writer1 = Writer<StringMonoid, int>.Tell(42, new StringMonoid("aaa"));
        var writer2 = Writer<StringMonoid, int>.Tell(42, new StringMonoid("bbb"));

        Assert.True(writer1.CompareTo(writer2) < 0);
        Assert.True(writer2.CompareTo(writer1) > 0);
    }

    [Fact]
    public void Writer_StringMonoid_ComparisonOperators_Work()
    {
        var writer1 = Writer<StringMonoid, int>.Tell(42, new StringMonoid("aaa"));
        var writer2 = Writer<StringMonoid, int>.Tell(42, new StringMonoid("bbb"));

        Assert.True(writer1 < writer2);
        Assert.True(writer1 <= writer2);
        Assert.False(writer1 > writer2);
        Assert.False(writer1 >= writer2);
    }

    [Fact]
    public void Writer_ListMonoid_CompareTo_WithDifferentLogs()
    {
        var writer1 = Writer<ListMonoid<string>, int>.Tell(42, ListMonoid.Of("a"));
        var writer2 = Writer<ListMonoid<string>, int>.Tell(42, ListMonoid.Of("a", "b"));

        Assert.True(writer1.CompareTo(writer2) < 0);
        Assert.True(writer2.CompareTo(writer1) > 0);
    }

    [Fact]
    public void Writer_ListMonoid_ComparisonOperators_Work()
    {
        var writer1 = Writer<ListMonoid<string>, int>.Tell(42, ListMonoid.Of("a"));
        var writer2 = Writer<ListMonoid<string>, int>.Tell(42, ListMonoid.Of("a", "b"));

        Assert.True(writer1 < writer2);
        Assert.True(writer1 <= writer2);
        Assert.False(writer1 > writer2);
        Assert.False(writer1 >= writer2);
    }

    #endregion

    #region StringMonoid/ListMonoid Sequence Tests

    [Fact]
    public void Sequence_StringMonoidWriters_CombinesLogs()
    {
        var writers = new[]
        {
            Writer<StringMonoid, int>.Tell(1, new StringMonoid("a")),
            Writer<StringMonoid, int>.Tell(2, new StringMonoid("b")),
            Writer<StringMonoid, int>.Tell(3, new StringMonoid("c"))
        };

        var result = writers.Sequence();

        Assert.Equal("abc", result.Log.Value);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToList());
    }

    [Fact]
    public void Sequence_ListMonoidWriters_CombinesLogs()
    {
        var writers = new[]
        {
            Writer<ListMonoid<string>, int>.Tell(1, ListMonoid.Of("a")),
            Writer<ListMonoid<string>, int>.Tell(2, ListMonoid.Of("b", "b2")),
            Writer<ListMonoid<string>, int>.Tell(3, ListMonoid.Of("c"))
        };

        var result = writers.Sequence();

        Assert.Equal(new[] { "a", "b", "b2", "c" }, result.Log.Value);
        Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToList());
    }

    [Fact]
    public void Sequence_StringMonoidWriters_ThrowsOnNull()
    {
        IEnumerable<Writer<StringMonoid, int>> nullWriters = null!;
        Assert.Throws<ArgumentNullException>(() => nullWriters.Sequence());
    }

    [Fact]
    public void Sequence_ListMonoidWriters_ThrowsOnNull()
    {
        IEnumerable<Writer<ListMonoid<string>, int>> nullWriters = null!;
        Assert.Throws<ArgumentNullException>(() => nullWriters.Sequence());
    }

    #endregion
}
