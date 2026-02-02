using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended LINQ tests for various monads to improve coverage.
/// </summary>
public class LinqExtendedTests
{
    #region TryLinq Extended Tests

    [Fact]
    public void Try_Select_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Error(exception);
        var result = @try.Select(x => x * 2);

        Assert.True(result.IsError);
        Assert.Same(exception, result.GetException());
    }

    [Fact]
    public void Try_SelectMany_WithoutResultSelector_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Error(exception);
        var result = @try.SelectMany(x => Try<int>.Ok(x + 5));

        Assert.True(result.IsError);
    }

    [Fact]
    public void Try_SelectMany_WithResultSelector_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Error(exception);
        var result = @try.SelectMany(
            x => Try<string>.Ok($"Value: {x}"),
            (x, y) => $"{y}!");

        Assert.True(result.IsError);
    }

    [Fact]
    public void Try_SelectMany_WithResultSelector_SecondFailure_ReturnsFailure()
    {
        var @try = Try<int>.Ok(10);
        var result = @try.SelectMany(
            _ => Try<string>.Error(new InvalidOperationException("second")),
            (x, y) => $"{y}!");

        Assert.True(result.IsError);
    }

    [Fact]
    public void Try_Where_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Error(exception);
        var result = @try.Where(x => x > 0);

        Assert.True(result.IsError);
    }

    #endregion

    #region OptionLinq Extended Tests

    [Fact]
    public void Option_SelectMany_WithoutResultSelector_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = option.SelectMany(x => Option<int>.Some(x + 5));

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_SelectMany_WithResultSelector_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = option.SelectMany(
            x => Option<string>.Some($"Value: {x}"),
            (x, y) => $"{y}!");

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_SelectMany_WithResultSelector_SecondNone_ReturnsNone()
    {
        var option = Option<int>.Some(10);
        var result = option.SelectMany(
            _ => Option<string>.None(),
            (x, y) => $"{y}!");

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_Where_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();
        var result = option.Where(x => x > 0);

        Assert.True(result.IsNone);
    }

    #endregion

    #region ResultLinq Extended Tests

    [Fact]
    public void Result_SelectMany_WithoutResultSelector_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Error("error");
        var mapped = result.SelectMany(x => Result<int, string>.Ok(x + 5));

        Assert.True(mapped.IsError);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public void Result_SelectMany_WithResultSelector_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Error("error");
        var mapped = result.SelectMany(
            x => Result<string, string>.Ok($"Value: {x}"),
            (x, y) => $"{y}!");

        Assert.True(mapped.IsError);
    }

    [Fact]
    public void Result_SelectMany_WithResultSelector_SecondErr_ReturnsErr()
    {
        var result = Result<int, string>.Ok(10);
        var mapped = result.SelectMany(
            _ => Result<string, string>.Error("second error"),
            (x, y) => $"{y}!");

        Assert.True(mapped.IsError);
        Assert.Equal("second error", mapped.GetError());
    }

    [Fact]
    public void Result_Where_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Error("existing error");
        var filtered = result.Where(x => x > 0, "should not happen");

        Assert.True(filtered.IsError);
        Assert.Equal("existing error", filtered.GetError());
    }

    [Fact]
    public void Result_Where_WithFactory_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Error("existing error");
        var filtered = result.Where(x => x > 0, x => $"value {x} is invalid");

        Assert.True(filtered.IsError);
        Assert.Equal("existing error", filtered.GetError());
    }

    #endregion

    #region WriterLinq Extended Tests

    [Fact]
    public void Writer_SelectMany_ListLog_ChainsWithListConcatenation()
    {
        var writer1 = Writer<List<string>, int>.Tell(10, new List<string> { "Step 1" });
        var result = writer1.SelectMany(x =>
            Writer<List<string>, int>.Tell(x + 5, new List<string> { "Step 2" }));

        Assert.Equal(15, result.Value);
        Assert.Equal(2, result.Log.Count);
    }

    [Fact]
    public void Writer_SelectMany_ListLog_WithResultSelector()
    {
        var writer1 = Writer<List<string>, int>.Tell(10, new List<string> { "Init" });
        var result = writer1.SelectMany(
            x => Writer<List<string>, string>.Tell($"Value: {x}", new List<string> { "Format" }),
            (x, y) => $"{y} (original: {x})");

        Assert.Equal("Value: 10 (original: 10)", result.Value);
        Assert.Contains("Init", result.Log);
        Assert.Contains("Format", result.Log);
    }

    #endregion
}

