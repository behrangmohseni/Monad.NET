using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for RemoteDataLinq to improve code coverage.
/// </summary>
public class RemoteDataLinqTests
{
    #region RemoteDataLinq Select Tests

    [Fact]
    public void RemoteData_Select_OnSuccess_TransformsValue()
    {
        var data = RemoteData<int, string>.Ok(42);
        var result = from x in data select x * 2;

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void RemoteData_Select_OnNotAsked_ReturnsNotAsked()
    {
        var data = RemoteData<int, string>.NotAsked();
        var result = from x in data select x * 2;

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void RemoteData_Select_OnLoading_ReturnsLoading()
    {
        var data = RemoteData<int, string>.Loading();
        var result = from x in data select x * 2;

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void RemoteData_Select_OnFailure_ReturnsFailure()
    {
        var data = RemoteData<int, string>.Error("error");
        var result = from x in data select x * 2;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    #endregion

    #region RemoteDataLinq SelectMany Tests

    [Fact]
    public void RemoteData_SelectMany_BothSuccess_Chains()
    {
        var result = from x in RemoteData<int, string>.Ok(10)
                     from y in RemoteData<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void RemoteData_SelectMany_FirstNotAsked_ReturnsNotAsked()
    {
        var result = from x in RemoteData<int, string>.NotAsked()
                     from y in RemoteData<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void RemoteData_SelectMany_FirstLoading_ReturnsLoading()
    {
        var result = from x in RemoteData<int, string>.Loading()
                     from y in RemoteData<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void RemoteData_SelectMany_FirstFailure_ReturnsFailure()
    {
        var result = from x in RemoteData<int, string>.Error("error")
                     from y in RemoteData<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void RemoteData_SelectMany_SecondNotAsked_ReturnsNotAsked()
    {
        var result = from x in RemoteData<int, string>.Ok(10)
                     from y in RemoteData<int, string>.NotAsked()
                     select x + y;

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void RemoteData_SelectMany_SecondLoading_ReturnsLoading()
    {
        var result = from x in RemoteData<int, string>.Ok(10)
                     from y in RemoteData<int, string>.Loading()
                     select x + y;

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void RemoteData_SelectMany_SecondFailure_ReturnsFailure()
    {
        var result = from x in RemoteData<int, string>.Ok(10)
                     from y in RemoteData<int, string>.Error("error")
                     select x + y;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void RemoteData_SelectMany_ComplexChain_Success()
    {
        var result = from a in RemoteData<int, string>.Ok(1)
                     from b in RemoteData<int, string>.Ok(2)
                     from c in RemoteData<int, string>.Ok(3)
                     select a + b + c;

        Assert.True(result.IsOk);
        Assert.Equal(6, result.GetValue());
    }

    #endregion

    #region RemoteDataLinq with Result Selector Tests

    [Fact]
    public void RemoteData_SelectMany_WithResultSelector_Chains()
    {
        var first = RemoteData<int, string>.Ok(10);
        var second = RemoteData<string, string>.Ok("hello");

        var result = first.SelectMany(
            x => second,
            (x, y) => $"{y}: {x}");

        Assert.True(result.IsOk);
        Assert.Equal("hello: 10", result.GetValue());
    }

    [Fact]
    public void RemoteData_SelectMany_WithResultSelector_FirstNotAsked()
    {
        var first = RemoteData<int, string>.NotAsked();
        var second = RemoteData<string, string>.Ok("hello");

        var result = first.SelectMany(
            x => second,
            (x, y) => $"{y}: {x}");

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void RemoteData_SelectMany_WithResultSelector_FirstLoading()
    {
        var first = RemoteData<int, string>.Loading();
        var second = RemoteData<string, string>.Ok("hello");

        var result = first.SelectMany(
            x => second,
            (x, y) => $"{y}: {x}");

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void RemoteData_SelectMany_WithResultSelector_FirstFailure()
    {
        var first = RemoteData<int, string>.Error("error");
        var second = RemoteData<string, string>.Ok("hello");

        var result = first.SelectMany(
            x => second,
            (x, y) => $"{y}: {x}");

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    #endregion
}

