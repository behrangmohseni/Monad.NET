using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Reader monad to improve code coverage.
/// </summary>
public class ReaderExtendedTests
{
    private class TestConfig
    {
        public string Name { get; set; } = "default";
        public int Value { get; set; } = 42;
    }

    #region Reader Static Helper Tests

    [Fact]
    public void Reader_From_CreatesReader()
    {
        var reader = Reader.From<TestConfig, string>(cfg => cfg.Name);
        var config = new TestConfig { Name = "test" };

        Assert.Equal("test", reader.Run(config));
    }

    [Fact]
    public void Reader_Pure_CreatesConstantReader()
    {
        var reader = Reader.Return<TestConfig, int>(100);
        var config = new TestConfig();

        Assert.Equal(100, reader.Run(config));
    }

    [Fact]
    public void Reader_Ask_ReturnsEnvironment()
    {
        var reader = Reader.Ask<TestConfig>();
        var config = new TestConfig { Name = "env", Value = 99 };

        var result = reader.Run(config);

        Assert.Equal("env", result.Name);
        Assert.Equal(99, result.Value);
    }

    [Fact]
    public void Reader_Asks_ExtractsFromEnvironment()
    {
        var reader = Reader.Asks<TestConfig, int>(cfg => cfg.Value);
        var config = new TestConfig { Value = 123 };

        Assert.Equal(123, reader.Run(config));
    }

    #endregion

    #region Reader Null Argument Tests

    [Fact]
    public void Reader_From_ThrowsOnNullFunc()
    {
        Assert.Throws<ArgumentNullException>(() => Reader<TestConfig, int>.From(null!));
    }

    [Fact]
    public void Reader_Asks_ThrowsOnNullSelector()
    {
        Assert.Throws<ArgumentNullException>(() => Reader<TestConfig, int>.Asks(null!));
    }

    [Fact]
    public void Reader_Run_ThrowsOnNullEnvironment()
    {
        var reader = Reader<TestConfig, int>.Return(42);
        Assert.Throws<ArgumentNullException>(() => reader.Run(null!));
    }

    [Fact]
    public void Reader_Map_ThrowsOnNullMapper()
    {
        var reader = Reader<TestConfig, int>.Return(42);
        Assert.Throws<ArgumentNullException>(() => reader.Map<int>(null!));
    }

    [Fact]
    public void Reader_FlatMap_ThrowsOnNullBinder()
    {
        var reader = Reader<TestConfig, int>.Return(42);
        Assert.Throws<ArgumentNullException>(() => reader.Bind<int>(null!));
    }

    [Fact]
    public void Reader_WithEnvironment_ThrowsOnNullTransform()
    {
        var reader = Reader<int, string>.Return("test");
        Assert.Throws<ArgumentNullException>(() => reader.WithEnvironment<TestConfig>(null!));
    }

    [Fact]
    public void Reader_Zip_ThrowsOnNullCombiner()
    {
        var reader1 = Reader<TestConfig, int>.Return(1);
        var reader2 = Reader<TestConfig, int>.Return(2);
        Assert.Throws<ArgumentNullException>(() => reader1.Zip(reader2, (Func<int, int, int>)null!));
    }

    #endregion

    #region Reader Tap Tests

    [Fact]
    public void Reader_Tap_ExecutesActionWithResult()
    {
        int captured = 0;
        var reader = Reader<TestConfig, int>
            .Asks(cfg => cfg.Value)
            .Tap(x => captured = x);

        var config = new TestConfig { Value = 77 };
        var result = reader.Run(config);

        Assert.Equal(77, result);
        Assert.Equal(77, captured);
    }

    [Fact]
    public void Reader_Tap_ThrowsOnNullAction()
    {
        var reader = Reader<TestConfig, int>.Return(42);
        Assert.Throws<ArgumentNullException>(() => reader.Tap(null!));
    }

    [Fact]
    public void Reader_TapEnv_ExecutesActionWithEnvironment()
    {
        TestConfig? captured = null;
        var reader = Reader<TestConfig, int>
            .Return(42)
            .TapEnv(env => captured = env);

        var config = new TestConfig { Name = "captured", Value = 99 };
        var result = reader.Run(config);

        Assert.Equal(42, result);
        Assert.NotNull(captured);
        Assert.Equal("captured", captured!.Name);
    }

    [Fact]
    public void Reader_TapEnv_ThrowsOnNullAction()
    {
        var reader = Reader<TestConfig, int>.Return(42);
        Assert.Throws<ArgumentNullException>(() => reader.TapEnv(null!));
    }

    #endregion

    #region Reader AndThen/Bind Aliases

    [Fact]
    public void Reader_AndThen_IsSameAsFlatMap()
    {
        var reader = Reader<TestConfig, int>
            .Asks(cfg => cfg.Value)
            .Bind(x => Reader<TestConfig, string>.Return($"Value: {x}"));

        var config = new TestConfig { Value = 42 };
        Assert.Equal("Value: 42", reader.Run(config));
    }

    [Fact]
    public void Reader_Bind_IsSameAsFlatMap()
    {
        var reader = Reader<TestConfig, int>
            .Asks(cfg => cfg.Value)
            .Bind(x => Reader<TestConfig, string>.Return($"Value: {x}"));

        var config = new TestConfig { Value = 42 };
        Assert.Equal("Value: 42", reader.Run(config));
    }

    #endregion

    #region Reader ToAsync Tests

    [Fact]
    public async Task Reader_ToAsync_ConvertsToReaderAsync()
    {
        var reader = Reader<TestConfig, int>.Asks(cfg => cfg.Value);
        var asyncReader = reader.ToAsync();

        var config = new TestConfig { Value = 42 };
        var result = await asyncReader.RunAsync(config);

        Assert.Equal(42, result);
    }

    #endregion

    #region ReaderExtensions LINQ Tests

    [Fact]
    public void Reader_Select_TransformsResult()
    {
        var reader = Reader<TestConfig, int>.Asks(cfg => cfg.Value);
        var mapped = reader.Select(x => x * 2);

        var config = new TestConfig { Value = 21 };
        Assert.Equal(42, mapped.Run(config));
    }

    [Fact]
    public void Reader_SelectMany_ChainsReaders()
    {
        var reader = Reader<TestConfig, int>
            .Asks(cfg => cfg.Value)
            .SelectMany(x => Reader<TestConfig, string>.Return($"Value: {x}"));

        var config = new TestConfig { Value = 42 };
        Assert.Equal("Value: 42", reader.Run(config));
    }

    [Fact]
    public void Reader_SelectMany_WithResultSelector_ChainsReaders()
    {
        var reader = Reader<TestConfig, int>
            .Asks(cfg => cfg.Value)
            .SelectMany(
                x => Reader<TestConfig, string>.Asks(cfg => cfg.Name),
                (value, name) => $"{name}: {value}");

        var config = new TestConfig { Name = "Answer", Value = 42 };
        Assert.Equal("Answer: 42", reader.Run(config));
    }

    [Fact]
    public void Reader_SelectMany_WithResultSelector_ThrowsOnNullSelector()
    {
        var reader = Reader<TestConfig, int>.Return(42);
        Assert.Throws<ArgumentNullException>(() =>
            reader.SelectMany(
                x => Reader<TestConfig, string>.Return("test"),
                (Func<int, string, string>)null!));
    }

    [Fact]
    public void Reader_Sequence_ThrowsOnNull()
    {
        IEnumerable<Reader<TestConfig, int>> nullReaders = null!;
        Assert.Throws<ArgumentNullException>(() => nullReaders.Sequence());
    }

    [Fact]
    public void Reader_Traverse_ThrowsOnNullItems()
    {
        IEnumerable<int> nullItems = null!;
        Assert.Throws<ArgumentNullException>(() =>
            nullItems.Traverse(x => Reader<TestConfig, int>.Return(x)));
    }

    [Fact]
    public void Reader_Traverse_ThrowsOnNullSelector()
    {
        var items = new[] { 1, 2, 3 };
        Assert.Throws<ArgumentNullException>(() =>
            items.Traverse((Func<int, Reader<TestConfig, int>>)null!));
    }

    #endregion
}

