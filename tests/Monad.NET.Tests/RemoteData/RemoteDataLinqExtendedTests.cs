using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended LINQ tests for RemoteData to improve code coverage.
/// </summary>
public class RemoteDataLinqExtendedTests
{
    #region Select Tests

    [Fact]
    public void Select_Success_TransformsValue()
    {
        var rd = RemoteData<int, string>.Ok(21);
        var result = from x in rd
                     select x * 2;

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Select_NotAsked_PreservesState()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var result = from x in rd
                     select x * 2;

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void Select_Loading_PreservesState()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = from x in rd
                     select x * 2;

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void Select_Failure_PreservesError()
    {
        var rd = RemoteData<int, string>.Error("error");
        var result = from x in rd
                     select x * 2;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    #endregion

    #region SelectMany Tests

    [Fact]
    public void SelectMany_BothSuccess_ChainsValues()
    {
        var rd1 = RemoteData<int, string>.Ok(10);
        var rd2 = RemoteData<int, string>.Ok(32);

        var result = from x in rd1
                     from y in rd2
                     select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void SelectMany_FirstNotAsked_ReturnsNotAsked()
    {
        var rd1 = RemoteData<int, string>.NotAsked();
        var rd2 = RemoteData<int, string>.Ok(32);

        var result = from x in rd1
                     from y in rd2
                     select x + y;

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void SelectMany_SecondLoading_ReturnsLoading()
    {
        var rd1 = RemoteData<int, string>.Ok(10);
        var rd2 = RemoteData<int, string>.Loading();

        var result = from x in rd1
                     from y in rd2
                     select x + y;

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void SelectMany_SecondFailure_ReturnsFailure()
    {
        var rd1 = RemoteData<int, string>.Ok(10);
        var rd2 = RemoteData<int, string>.Error("error");

        var result = from x in rd1
                     from y in rd2
                     select x + y;

        Assert.True(result.IsError);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public void ComplexQuery_AllSuccess_Works()
    {
        var name = RemoteData<string, string>.Ok("John");
        var age = RemoteData<int, string>.Ok(30);
        var city = RemoteData<string, string>.Ok("NYC");

        var result = from n in name
                     from a in age
                     from c in city
                     select $"{n}, {a}, {c}";

        Assert.True(result.IsOk);
        Assert.Equal("John, 30, NYC", result.GetValue());
    }

    [Fact]
    public void ComplexQuery_OneLoading_ReturnsLoading()
    {
        var name = RemoteData<string, string>.Ok("John");
        var age = RemoteData<int, string>.Loading();
        var city = RemoteData<string, string>.Ok("NYC");

        var result = from n in name
                     from a in age
                     from c in city
                     select $"{n}, {a}, {c}";

        Assert.True(result.IsLoading);
    }

    #endregion
}

