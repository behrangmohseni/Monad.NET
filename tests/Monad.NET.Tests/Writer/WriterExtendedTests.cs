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
}
