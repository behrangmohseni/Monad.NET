using Monad.NET;
using Xunit;

namespace Monad.NET.Tests.PropertyBased;

public class OptionPropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-5)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void Map_Composition_Holds(int value)
    {
        var some = Option<int>.Some(value);
        int f(int x) => x + 1;
        int g(int x) => x * 2;

        var lhs = some.Map(f).Map(g);
        var rhs = some.Map(x => g(f(x)));

        Assert.Equal(lhs, rhs);
    }

    [Fact]
    public void Map_None_Is_None()
    {
        var none = Option<int>.None();
        int f(int x) => x + 1;
        Assert.True(none.Map(f).IsNone);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-5)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void Bind_Associativity_Holds(int value)
    {
        var some = Option<int>.Some(value);
        Option<int> f(int x) => Option<int>.Some(x + 1);
        Option<int> g(int x) => Option<int>.Some(x * 2);

        var lhs = some.Bind(f).Bind(g);
        var rhs = some.Bind(x => f(x).Bind(g));

        Assert.Equal(lhs, rhs);
    }

    [Fact]
    public void Bind_None_Is_None()
    {
        var none = Option<int>.None();
        Option<int> f(int x) => Option<int>.Some(x + 1);
        Assert.True(none.Bind(f).IsNone);
    }
}

