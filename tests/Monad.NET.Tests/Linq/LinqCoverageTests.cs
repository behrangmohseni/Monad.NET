using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for LINQ support to improve code coverage.
/// </summary>
public class LinqCoverageTests
{
    #region Result Where Tests

    [Fact]
    public void ResultLinq_Where_Ok_PredicateTrue_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Where(x => x > 40, "Value too small");

        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void ResultLinq_Where_Ok_PredicateFalse_ReturnsErr()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Where(x => x > 50, "Value too small");

        Assert.True(filtered.IsError);
        Assert.Equal("Value too small", filtered.GetError());
    }

    [Fact]
    public void ResultLinq_Where_Err_ReturnsErr()
    {
        var result = Result<int, string>.Error("original error");
        var filtered = result.Where(x => x > 50, "Value too small");

        Assert.True(filtered.IsError);
        Assert.Equal("original error", filtered.GetError());
    }

    [Fact]
    public void ResultLinq_Where_WithFactory_Ok_PredicateTrue_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Where(x => x > 40, x => $"Value {x} is too small");

        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void ResultLinq_Where_WithFactory_Ok_PredicateFalse_ReturnsErrWithValue()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Where(x => x > 50, x => $"Value {x} is too small");

        Assert.True(filtered.IsError);
        Assert.Equal("Value 42 is too small", filtered.GetError());
    }

    [Fact]
    public void ResultLinq_Where_WithFactory_Err_ReturnsErr()
    {
        var result = Result<int, string>.Error("original error");
        var filtered = result.Where(x => x > 50, x => $"Value {x} is too small");

        Assert.True(filtered.IsError);
        Assert.Equal("original error", filtered.GetError());
    }

    #endregion

    #region Try Where Tests

    [Fact]
    public void TryLinq_Where_Success_PredicateTrue_ReturnsSuccess()
    {
        var result = Try<int>.Ok(42);
        var filtered = result.Where(x => x > 40);

        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void TryLinq_Where_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Try<int>.Ok(42);
        var filtered = result.Where(x => x > 50);

        Assert.True(filtered.IsError);
    }

    [Fact]
    public void TryLinq_Where_Failure_ReturnsFailure()
    {
        var result = Try<int>.Error(new InvalidOperationException("error"));
        var filtered = result.Where(x => x > 50);

        Assert.True(filtered.IsError);
    }

    #endregion

    #region Option Where Tests

    [Fact]
    public void OptionLinq_Where_Some_PredicateTrue_ReturnsSome()
    {
        var option = Option<int>.Some(42);
        var filtered = option.Where(x => x > 40);

        Assert.True(filtered.IsSome);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void OptionLinq_Where_Some_PredicateFalse_ReturnsNone()
    {
        var option = Option<int>.Some(42);
        var filtered = option.Where(x => x > 50);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public void OptionLinq_Where_None_ReturnsNone()
    {
        var option = Option<int>.None();
        var filtered = option.Where(x => x > 50);

        Assert.True(filtered.IsNone);
    }

    #endregion

    #region Writer SelectMany Tests

    [Fact]
    public void WriterLinq_SelectMany_String_ConcatenatesLogs()
    {
        var writer1 = Writer<string, int>.Tell(10, "Started");
        var writer2 = Writer<string, int>.Tell(20, "Continued");

        var result = writer1.SelectMany(x => Writer<string, int>.Tell(x + 5, "Added"), (a, b) => a + b);
        var (value, log) = result.Run();

        Assert.Equal(25, value);
        Assert.Equal("StartedAdded", log);
    }

    [Fact]
    public void WriterLinq_SelectMany_List_ConcatenatesLogs()
    {
        var writer1 = Writer<List<string>, int>.Tell(10, new List<string> { "Started" });
        var writer2 = Writer<List<string>, int>.Tell(20, new List<string> { "Continued" });

        var result = writer1.SelectMany(x => Writer<List<string>, int>.Tell(x + 5, new List<string> { "Added" }), (a, b) => a + b);
        var (value, log) = result.Run();

        Assert.Equal(25, value);
        Assert.Equal(2, log.Count);
        Assert.Contains("Started", log);
        Assert.Contains("Added", log);
    }

    #endregion

    #region RemoteData SelectMany Tests

    [Fact]
    public void RemoteDataLinq_SelectMany_Success_ChainsCorrectly()
    {
        var rd1 = RemoteData<int, string>.Ok(10);

        var result =
            from x in rd1
            from y in RemoteData<int, string>.Ok(x + 5)
            select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(25, result.GetValue()); // 10 + (10 + 5)
    }

    [Fact]
    public void RemoteDataLinq_SelectMany_Failure_ReturnsFailure()
    {
        var rd1 = RemoteData<int, string>.Error("error");

        var result =
            from x in rd1
            from y in RemoteData<int, string>.Ok(x + 5)
            select x + y;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void RemoteDataLinq_SelectMany_NotAsked_ReturnsNotAsked()
    {
        var rd1 = RemoteData<int, string>.NotAsked();

        var result =
            from x in rd1
            from y in RemoteData<int, string>.Ok(x + 5)
            select x + y;

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void RemoteDataLinq_SelectMany_Loading_ReturnsLoading()
    {
        var rd1 = RemoteData<int, string>.Loading();

        var result =
            from x in rd1
            from y in RemoteData<int, string>.Ok(x + 5)
            select x + y;

        Assert.True(result.IsLoading);
    }

    #endregion
}
