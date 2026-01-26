using Xunit;

namespace Monad.NET.Tests;

public class WriterTests
{
    #region Writer<TLog, T> Core

    [Fact]
    public void Of_CreatesWriterWithValue()
    {
        var writer = Writer<string, int>.Of(42, "");

        Assert.Equal(42, writer.Value);
        Assert.Equal("", writer.Log);
    }

    [Fact]
    public void Of_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => Writer<string, string>.Of(null!, ""));
    }

    [Fact]
    public void Of_ThrowsOnNullLog()
    {
        Assert.Throws<ArgumentNullException>(() => Writer<string, int>.Of(42, null!));
    }

    [Fact]
    public void Tell_CreatesWriterWithValueAndLog()
    {
        var writer = Writer<string, int>.Tell(42, "computed");

        Assert.Equal(42, writer.Value);
        Assert.Equal("computed", writer.Log);
    }

    [Fact]
    public void Tell_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => Writer<string, string>.Tell(null!, "log"));
    }

    [Fact]
    public void Tell_ThrowsOnNullLog()
    {
        Assert.Throws<ArgumentNullException>(() => Writer<string, int>.Tell(42, null!));
    }

    [Fact]
    public void TellUnit_CreatesWriterWithUnitAndLog()
    {
        var writer = Writer<string, Unit>.TellUnit("log entry");

        Assert.Equal(Unit.Default, writer.Value);
        Assert.Equal("log entry", writer.Log);
    }

    [Fact]
    public void TellUnit_ThrowsOnNullLog()
    {
        Assert.Throws<ArgumentNullException>(() => Writer<string, Unit>.TellUnit(null!));
    }

    [Fact]
    public void Map_TransformsValue()
    {
        var writer = Writer<string, int>.Tell(10, "initial");

        var mapped = writer.Map(x => x * 2);

        Assert.Equal(20, mapped.Value);
        Assert.Equal("initial", mapped.Log);
    }

    [Fact]
    public void Map_ThrowsOnNullMapper()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() => writer.Map<int>(null!));
    }

    [Fact]
    public void FlatMap_ChainsAndCombinesLogs()
    {
        var writer = Writer<string, int>.Tell(10, "step1");

        var result = writer.Bind(
            x => Writer<string, int>.Tell(x * 2, "step2"),
            (log1, log2) => log1 + "|" + log2
        );

        Assert.Equal(20, result.Value);
        Assert.Equal("step1|step2", result.Log);
    }

    [Fact]
    public void FlatMap_ThrowsOnNullBinder()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() =>
            writer.Bind<int>(null!, (a, b) => a + b));
    }

    [Fact]
    public void FlatMap_ThrowsOnNullCombiner()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() =>
            writer.Bind(x => Writer<string, int>.Tell(x, ""), null!));
    }

    [Fact]
    public void Match_ExecutesCorrectly()
    {
        var writer = Writer<string, int>.Tell(42, "log");

        var result = writer.Match((v, l) => $"{v}:{l}");

        Assert.Equal("42:log", result);
    }

    [Fact]
    public void Match_ThrowsOnNullMatcher()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() => writer.Match<string>(null!));
    }

    [Fact]
    public void Tap_ExecutesActionWithValue()
    {
        var writer = Writer<string, int>.Tell(42, "log");
        int captured = 0;

        var result = writer.Tap(x => captured = x);

        Assert.Equal(42, captured);
        Assert.Equal(writer, result);
    }

    [Fact]
    public void Tap_ThrowsOnNullAction()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() => writer.Tap(null!));
    }

    [Fact]
    public void TapLog_ExecutesActionWithLog()
    {
        var writer = Writer<string, int>.Tell(42, "mylog");
        string captured = "";

        var result = writer.TapLog(l => captured = l);

        Assert.Equal("mylog", captured);
        Assert.Equal(writer, result);
    }

    [Fact]
    public void TapLog_ThrowsOnNullAction()
    {
        var writer = Writer<string, int>.Tell(10, "log");
        Assert.Throws<ArgumentNullException>(() => writer.TapLog(null!));
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualWriters()
    {
        var writer1 = Writer<string, int>.Tell(42, "log");
        var writer2 = Writer<string, int>.Tell(42, "log");

        Assert.True(writer1.Equals(writer2));
        Assert.True(writer1 == writer2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var writer1 = Writer<string, int>.Tell(42, "log");
        var writer2 = Writer<string, int>.Tell(43, "log");

        Assert.False(writer1.Equals(writer2));
        Assert.True(writer1 != writer2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentLogs()
    {
        var writer1 = Writer<string, int>.Tell(42, "log1");
        var writer2 = Writer<string, int>.Tell(42, "log2");

        Assert.False(writer1.Equals(writer2));
    }

    [Fact]
    public void Equals_WithObject()
    {
        var writer = Writer<string, int>.Tell(42, "log");

        Assert.False(writer.Equals(null));
        Assert.False(writer.Equals("not a writer"));
        Assert.True(writer.Equals((object)Writer<string, int>.Tell(42, "log")));
    }

    [Fact]
    public void GetHashCode_SameForEqualWriters()
    {
        var writer1 = Writer<string, int>.Tell(42, "log");
        var writer2 = Writer<string, int>.Tell(42, "log");

        Assert.Equal(writer1.GetHashCode(), writer2.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var writer = Writer<string, int>.Tell(42, "log");

        var str = writer.ToString();

        Assert.Contains("42", str);
        Assert.Contains("log", str);
    }

    #endregion

    #region StringWriter

    [Fact]
    public void StringWriter_Pure_CreatesWithEmptyLog()
    {
        var writer = StringWriter.Return(42);

        Assert.Equal(42, writer.Value);
        Assert.Equal("", writer.Log);
    }

    [Fact]
    public void StringWriter_Pure_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => StringWriter.Return<string>(null!));
    }

    [Fact]
    public void StringWriter_Tell_CreatesWithMessage()
    {
        var writer = StringWriter.Tell(42, "computed value");

        Assert.Equal(42, writer.Value);
        Assert.Equal("computed value", writer.Log);
    }

    [Fact]
    public void StringWriter_Tell_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => StringWriter.Tell<string>(null!, "msg"));
    }

    [Fact]
    public void StringWriter_Tell_HandlesNullMessage()
    {
        var writer = StringWriter.Tell(42, null!);

        Assert.Equal(42, writer.Value);
        Assert.Equal("", writer.Log);
    }

    [Fact]
    public void StringWriter_Log_CreatesUnitWithMessage()
    {
        var writer = StringWriter.Log("log entry");

        Assert.Equal(Unit.Default, writer.Value);
        Assert.Equal("log entry", writer.Log);
    }

    [Fact]
    public void StringWriter_Log_HandlesNullMessage()
    {
        var writer = StringWriter.Log(null!);

        Assert.Equal(Unit.Default, writer.Value);
        Assert.Equal("", writer.Log);
    }

    #endregion

    #region ListWriter

    [Fact]
    public void ListWriter_Pure_CreatesWithEmptyList()
    {
        var writer = ListWriter.Return<int, string>(42);

        Assert.Equal(42, writer.Value);
        Assert.Empty(writer.Log);
    }

    [Fact]
    public void ListWriter_Pure_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => ListWriter.Return<string, string>(null!));
    }

    [Fact]
    public void ListWriter_Tell_MultipleEntries()
    {
        var writer = ListWriter.Tell(42, "a", "b", "c");

        Assert.Equal(42, writer.Value);
        Assert.Equal(3, writer.Log.Count);
        Assert.Equal(new[] { "a", "b", "c" }, writer.Log);
    }

    #endregion

    #region WriterExtensions

    [Fact]
    public void WithLog_CreatesStringWriter()
    {
        var writer = 42.WithLog("computed");

        Assert.Equal(42, writer.Value);
        Assert.Equal("computed", writer.Log);
    }

    [Fact]
    public void WithLog_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => ((string)null!).WithLog("log"));
    }

    [Fact]
    public void ToWriter_CreatesEmptyLogWriter()
    {
        var writer = 42.ToWriter();

        Assert.Equal(42, writer.Value);
        Assert.Equal("", writer.Log);
    }

    [Fact]
    public void ToWriter_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => ((string)null!).ToWriter());
    }

    [Fact]
    public void StringWriter_FlatMap_ConcatenatesLogs()
    {
        var writer = 10.WithLog("a");

        var result = writer.Bind(x => (x * 2).WithLog("b"));

        Assert.Equal(20, result.Value);
        Assert.Equal("ab", result.Log);
    }

    #endregion
}
