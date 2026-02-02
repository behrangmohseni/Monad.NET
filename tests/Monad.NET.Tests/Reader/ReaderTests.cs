using Monad.NET;

namespace Monad.NET.Tests;

public class ReaderTests
{
    private class TestEnvironment
    {
        public string ConnectionString { get; set; } = "Server=localhost";
        public int Timeout { get; set; } = 30;
        public bool Debug { get; set; } = false;
    }

    [Fact]
    public void Pure_CreatesReaderWithConstantValue()
    {
        var reader = Reader<TestEnvironment, int>.Return(42);
        var env = new TestEnvironment();

        Assert.Equal(42, reader.Run(env));
    }

    [Fact]
    public void Ask_ReturnsTheEnvironment()
    {
        var reader = Reader<TestEnvironment, TestEnvironment>.Ask();
        var env = new TestEnvironment { Timeout = 60 };

        var result = reader.Run(env);
        Assert.Equal(60, result.Timeout);
    }

    [Fact]
    public void Asks_ExtractsValueFromEnvironment()
    {
        var reader = Reader<TestEnvironment, int>.Asks(env => env.Timeout);
        var env = new TestEnvironment { Timeout = 45 };

        Assert.Equal(45, reader.Run(env));
    }

    [Fact]
    public void Map_TransformsResult()
    {
        var reader = Reader<TestEnvironment, int>.Return(42);
        var mapped = reader.Map(x => x * 2);
        var env = new TestEnvironment();

        Assert.Equal(84, mapped.Run(env));
    }

    [Fact]
    public void FlatMap_ChainsReaders()
    {
        var reader1 = Reader<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = reader1.Bind(timeout =>
            Reader<TestEnvironment, string>.Return($"Timeout: {timeout}")
        );
        var env = new TestEnvironment { Timeout = 30 };

        Assert.Equal("Timeout: 30", reader2.Run(env));
    }

    [Fact]
    public void WithEnvironment_TransformsEnvironmentType()
    {
        var reader = Reader<int, string>.From(x => x.ToString());
        var transformed = reader.WithEnvironment<TestEnvironment>(env => env.Timeout);
        var env = new TestEnvironment { Timeout = 42 };

        Assert.Equal("42", transformed.Run(env));
    }

    [Fact]
    public void Zip_CombinesTwoReaders()
    {
        var reader1 = Reader<TestEnvironment, int>.Asks(env => env.Timeout);
        var reader2 = Reader<TestEnvironment, string>.Asks(env => env.ConnectionString);
        var zipped = reader1.Zip(reader2, (timeout, conn) => $"{conn} with timeout {timeout}");
        var env = new TestEnvironment { Timeout = 30, ConnectionString = "Server=prod" };

        Assert.Equal("Server=prod with timeout 30", zipped.Run(env));
    }

    [Fact]
    public void Bind_ChainsReaders()
    {
        var reader = Reader<TestEnvironment, int>.Asks(e => e.Timeout)
            .Bind(timeout => Reader<TestEnvironment, bool>.Asks(e => e.Debug)
                .Map(debug => $"Timeout: {timeout}, Debug: {debug}"));

        var env = new TestEnvironment { Timeout = 60, Debug = true };
        Assert.Equal("Timeout: 60, Debug: True", reader.Run(env));
    }

    [Fact]
    public void Sequence_CombinesMultipleReaders()
    {
        var readers = new[]
        {
            Reader<TestEnvironment, int>.Return(1),
            Reader<TestEnvironment, int>.Return(2),
            Reader<TestEnvironment, int>.Return(3)
        };

        var sequenced = readers.Sequence();
        var env = new TestEnvironment();
        var result = sequenced.Run(env).ToList();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Traverse_MapsAndSequences()
    {
        var numbers = new[] { 1, 2, 3 };
        var traversed = numbers.Traverse(n =>
            Reader<TestEnvironment, int>.Asks(env => n * env.Timeout)
        );

        var env = new TestEnvironment { Timeout = 10 };
        var result = traversed.Run(env).ToList();

        Assert.Equal(new[] { 10, 20, 30 }, result);
    }

    [Fact]
    public void RealWorld_DependencyInjection()
    {
        // Simulate a service that depends on environment
        Reader<TestEnvironment, string> GetConnectionInfo() =>
            Reader<TestEnvironment, string>.Asks(e => e.ConnectionString)
                .Bind(conn => Reader<TestEnvironment, int>.Asks(e => e.Timeout)
                    .Map(timeout => $"Connecting to {conn} with timeout {timeout}s"));

        var env = new TestEnvironment
        {
            ConnectionString = "Server=production",
            Timeout = 60
        };

        var info = GetConnectionInfo().Run(env);
        Assert.Equal("Connecting to Server=production with timeout 60s", info);
    }
}

