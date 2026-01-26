using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended LINQ tests for various monads to improve coverage.
/// </summary>
public class LinqExtendedTests
{
    #region EitherLinq Extended Tests

    [Fact]
    public void Either_Select_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("error");
        var result = either.Select(x => x * 2);

        Assert.True(result.IsLeft);
        Assert.Equal("error", result.GetLeft());
    }

    [Fact]
    public void Either_SelectMany_WithoutResultSelector_Chains()
    {
        var either = Either<string, int>.Right(10);
        var result = either.SelectMany(x => Either<string, int>.Right(x + 5));

        Assert.True(result.IsRight);
        Assert.Equal(15, result.GetRight());
    }

    [Fact]
    public void Either_SelectMany_WithoutResultSelector_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("error");
        var result = either.SelectMany(x => Either<string, int>.Right(x + 5));

        Assert.True(result.IsLeft);
        Assert.Equal("error", result.GetLeft());
    }

    [Fact]
    public void Either_SelectMany_WithResultSelector_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("error");
        var result = either.SelectMany(
            x => Either<string, string>.Right($"Value: {x}"),
            (x, y) => $"{y}!");

        Assert.True(result.IsLeft);
    }

    [Fact]
    public void Either_SelectMany_WithResultSelector_SecondLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Right(10);
        var result = either.SelectMany(
            _ => Either<string, string>.Left("second error"),
            (x, y) => $"{y}!");

        Assert.True(result.IsLeft);
        Assert.Equal("second error", result.GetLeft());
    }

    [Fact]
    public void Either_Where_OnLeft_ReturnsLeft()
    {
        var either = Either<string, int>.Left("existing error");
        var result = either.Where(x => x > 0, "should not happen");

        Assert.True(result.IsLeft);
        Assert.Equal("existing error", result.GetLeft());
    }

    #endregion

    #region TryLinq Extended Tests

    [Fact]
    public void Try_Select_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Failure(exception);
        var result = @try.Select(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Same(exception, result.GetException());
    }

    [Fact]
    public void Try_SelectMany_WithoutResultSelector_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Failure(exception);
        var result = @try.SelectMany(x => Try<int>.Success(x + 5));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Try_SelectMany_WithResultSelector_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Failure(exception);
        var result = @try.SelectMany(
            x => Try<string>.Success($"Value: {x}"),
            (x, y) => $"{y}!");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Try_SelectMany_WithResultSelector_SecondFailure_ReturnsFailure()
    {
        var @try = Try<int>.Success(10);
        var result = @try.SelectMany(
            _ => Try<string>.Failure(new InvalidOperationException("second")),
            (x, y) => $"{y}!");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Try_Where_OnFailure_ReturnsFailure()
    {
        var exception = new InvalidOperationException("test");
        var @try = Try<int>.Failure(exception);
        var result = @try.Where(x => x > 0);

        Assert.True(result.IsFailure);
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
        var result = Result<int, string>.Err("error");
        var mapped = result.SelectMany(x => Result<int, string>.Ok(x + 5));

        Assert.True(mapped.IsErr);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public void Result_SelectMany_WithResultSelector_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Err("error");
        var mapped = result.SelectMany(
            x => Result<string, string>.Ok($"Value: {x}"),
            (x, y) => $"{y}!");

        Assert.True(mapped.IsErr);
    }

    [Fact]
    public void Result_SelectMany_WithResultSelector_SecondErr_ReturnsErr()
    {
        var result = Result<int, string>.Ok(10);
        var mapped = result.SelectMany(
            _ => Result<string, string>.Err("second error"),
            (x, y) => $"{y}!");

        Assert.True(mapped.IsErr);
        Assert.Equal("second error", mapped.GetError());
    }

    [Fact]
    public void Result_Where_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Err("existing error");
        var filtered = result.Where(x => x > 0, "should not happen");

        Assert.True(filtered.IsErr);
        Assert.Equal("existing error", filtered.GetError());
    }

    [Fact]
    public void Result_Where_WithFactory_OnErr_ReturnsErr()
    {
        var result = Result<int, string>.Err("existing error");
        var filtered = result.Where(x => x > 0, x => $"value {x} is invalid");

        Assert.True(filtered.IsErr);
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

