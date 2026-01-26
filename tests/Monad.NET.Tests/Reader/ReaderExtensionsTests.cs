using Xunit;

namespace Monad.NET.Tests;

public class ReaderExtensionsTests
{
    public record TestEnvironment(string Name, int Value);

    #region Reader<R, A> Core Methods

    [Fact]
    public void Pure_CreatesReaderReturningValue()
    {
        var reader = Reader<TestEnvironment, int>.Return(42);
        var result = reader.Run(new TestEnvironment("test", 100));

        Assert.Equal(42, result);
    }

    [Fact]
    public void Ask_ReturnsEnvironment()
    {
        var reader = Reader<TestEnvironment, TestEnvironment>.Ask();
        var env = new TestEnvironment("test", 100);

        var result = reader.Run(env);

        Assert.Equal(env, result);
    }

    [Fact]
    public void Asks_SelectsFromEnvironment()
    {
        var reader = Reader<TestEnvironment, string>.Asks(e => e.Name);
        var result = reader.Run(new TestEnvironment("myname", 100));

        Assert.Equal("myname", result);
    }

    [Fact]
    public void Map_TransformsValue()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Value);

        var mapped = reader.Map(v => v * 2);
        var result = mapped.Run(new TestEnvironment("test", 21));

        Assert.Equal(42, result);
    }

    [Fact]
    public void Map_ThrowsOnNullMapper()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Value);

        Assert.Throws<ArgumentNullException>(() => reader.Map<int>(null!));
    }

    [Fact]
    public void FlatMap_ChainsReaders()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Value);

        var chained = reader.Bind(v =>
            Reader<TestEnvironment, string>.Asks(e => $"{e.Name}:{v}"));

        var result = chained.Run(new TestEnvironment("test", 42));

        Assert.Equal("test:42", result);
    }

    [Fact]
    public void FlatMap_ThrowsOnNullBinder()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Value);

        Assert.Throws<ArgumentNullException>(() =>
            reader.Bind<int>(null!));
    }

    [Fact]
    public void Zip_CombinesReadersWithFunction()
    {
        var reader1 = Reader<TestEnvironment, string>.Asks(e => e.Name);
        var reader2 = Reader<TestEnvironment, int>.Asks(e => e.Value);

        var zipped = reader1.Zip(reader2, (n, v) => $"{n}={v}");
        var result = zipped.Run(new TestEnvironment("test", 42));

        Assert.Equal("test=42", result);
    }

    [Fact]
    public void Tap_ExecutesActionAndReturnsOriginal()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Value);
        int captured = 0;

        var tapped = reader.Tap(v => captured = v);
        var result = tapped.Run(new TestEnvironment("test", 42));

        Assert.Equal(42, result);
        Assert.Equal(42, captured);
    }

    [Fact]
    public void TapEnv_ExecutesActionWithEnvironment()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Value);
        TestEnvironment? captured = null;

        var tapped = reader.TapEnv(e => captured = e);
        var env = new TestEnvironment("test", 42);
        var result = tapped.Run(env);

        Assert.Equal(42, result);
        Assert.Equal(env, captured);
    }

    #endregion

    #region ReaderExtensions

    [Fact]
    public void Flatten_UnwrapsNestedReader()
    {
        var inner = Reader<TestEnvironment, int>.Asks(e => e.Value);
        var outer = Reader<TestEnvironment, Reader<TestEnvironment, int>>.Return(inner);

        var flattened = outer.Bind(r => r);
        var result = flattened.Run(new TestEnvironment("test", 42));

        Assert.Equal(42, result);
    }

    #endregion
}
