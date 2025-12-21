using Monad.NET;

namespace Monad.NET.Tests;

public class CollectionTests
{
    #region Option Collection Tests

    [Fact]
    public void Option_Sequence_AllSome_ReturnsSomeOfList()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.Some(2),
            Option<int>.Some(3)
        };

        var result = options.Sequence();

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public void Option_Sequence_WithNone_ReturnsNone()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3)
        };

        var result = options.Sequence();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_Traverse_AllSucceed_ReturnsSome()
    {
        var numbers = new[] { "1", "2", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? Option<int>.Some(value)
                : Option<int>.None());

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public void Option_Traverse_OneFails_ReturnsNone()
    {
        var numbers = new[] { "1", "invalid", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? Option<int>.Some(value)
                : Option<int>.None());

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_Choose_FiltersAndUnwraps()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3),
            Option<int>.None(),
            Option<int>.Some(5)
        };

        var result = options.Choose().ToList();

        Assert.Equal(new[] { 1, 3, 5 }, result);
    }

    [Fact]
    public void Option_ChooseWithSelector_MapsAndFilters()
    {
        var numbers = new[] { "1", "invalid", "3", "not a number", "5" };

        var result = numbers.Choose(s =>
            int.TryParse(s, out var value)
                ? Option<int>.Some(value)
                : Option<int>.None()).ToList();

        Assert.Equal(new[] { 1, 3, 5 }, result);
    }

    [Fact]
    public void Option_FirstSome_ReturnsFirstSomeValue()
    {
        var options = new[]
        {
            Option<int>.None(),
            Option<int>.Some(1),
            Option<int>.Some(2)
        };

        var result = options.FirstSome();

        Assert.True(result.IsSome);
        Assert.Equal(1, result.Unwrap());
    }

    [Fact]
    public void Option_FirstSome_AllNone_ReturnsNone()
    {
        var options = new[]
        {
            Option<int>.None(),
            Option<int>.None(),
            Option<int>.None()
        };

        var result = options.FirstSome();

        Assert.True(result.IsNone);
    }

    #endregion

    #region Result Collection Tests

    [Fact]
    public void Result_Sequence_AllOk_ReturnsOkOfList()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        var result = results.Sequence();

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public void Result_Sequence_WithErr_ReturnsFirstErr()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2")
        };

        var result = results.Sequence();

        Assert.True(result.IsErr);
        Assert.Equal("error1", result.UnwrapErr());
    }

    [Fact]
    public void Result_Traverse_AllSucceed_ReturnsOk()
    {
        var numbers = new[] { "1", "2", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? Result<int, string>.Ok(value)
                : Result<int, string>.Err($"Invalid: {s}"));

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public void Result_Traverse_OneFails_ReturnsFirstErr()
    {
        var numbers = new[] { "1", "invalid", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? Result<int, string>.Ok(value)
                : Result<int, string>.Err($"Invalid: {s}"));

        Assert.True(result.IsErr);
        Assert.Equal("Invalid: invalid", result.UnwrapErr());
    }

    [Fact]
    public void Result_CollectOk_FiltersOkValues()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(5)
        };

        var oks = results.CollectOk().ToList();

        Assert.Equal(new[] { 1, 3, 5 }, oks);
    }

    [Fact]
    public void Result_CollectErr_FiltersErrValues()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2")
        };

        var errors = results.CollectErr().ToList();

        Assert.Equal(new[] { "error1", "error2" }, errors);
    }

    [Fact]
    public void Result_Partition_SeparatesOksAndErrs()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(3)
        };

        var (oks, errors) = results.Partition();

        Assert.Equal(new[] { 1, 2, 3 }, oks);
        Assert.Equal(new[] { "error1", "error2" }, errors);
    }

    [Fact]
    public void Result_FirstOk_ReturnsFirstOkValue()
    {
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2)
        };

        var result = results.FirstOk();

        Assert.True(result.IsOk);
        Assert.Equal(1, result.Unwrap());
    }

    [Fact]
    public void Result_FirstOk_AllErrs_ReturnsLastErr()
    {
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2"),
            Result<int, string>.Err("error3")
        };

        var result = results.FirstOk();

        Assert.True(result.IsErr);
        Assert.Equal("error3", result.UnwrapErr());
    }

    #endregion

    #region Either Collection Tests

    [Fact]
    public void Either_CollectRights_FiltersRightValues()
    {
        var eithers = new[]
        {
            Either<string, int>.Right(1),
            Either<string, int>.Left("error"),
            Either<string, int>.Right(3),
            Either<string, int>.Left("error2"),
            Either<string, int>.Right(5)
        };

        var rights = eithers.CollectRights().ToList();

        Assert.Equal(new[] { 1, 3, 5 }, rights);
    }

    [Fact]
    public void Either_CollectLefts_FiltersLeftValues()
    {
        var eithers = new[]
        {
            Either<string, int>.Right(1),
            Either<string, int>.Left("error1"),
            Either<string, int>.Right(3),
            Either<string, int>.Left("error2")
        };

        var lefts = eithers.CollectLefts().ToList();

        Assert.Equal(new[] { "error1", "error2" }, lefts);
    }

    [Fact]
    public void Either_Partition_SeparatesLeftsAndRights()
    {
        var eithers = new[]
        {
            Either<string, int>.Right(1),
            Either<string, int>.Left("error1"),
            Either<string, int>.Right(2),
            Either<string, int>.Left("error2"),
            Either<string, int>.Right(3)
        };

        var (lefts, rights) = eithers.Partition();

        Assert.Equal(new[] { "error1", "error2" }, lefts);
        Assert.Equal(new[] { 1, 2, 3 }, rights);
    }

    #endregion

    #region Async Collection Tests

    [Fact]
    public async Task Option_SequenceAsync_AllSome_ReturnsSome()
    {
        var optionTasks = new[]
        {
            Task.FromResult(Option<int>.Some(1)),
            Task.FromResult(Option<int>.Some(2)),
            Task.FromResult(Option<int>.Some(3))
        };

        var result = await optionTasks.SequenceAsync();

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public async Task Option_TraverseAsync_AllSucceed_ReturnsSome()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return Option<int>.Some(x * 2);
        });

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 2, 4, 6 }, result.Unwrap());
    }

    [Fact]
    public async Task Result_SequenceAsync_AllOk_ReturnsOk()
    {
        var resultTasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        var result = await resultTasks.SequenceAsync();

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public async Task Result_TraverseAsync_AllSucceed_ReturnsOk()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(x * 2);
        });

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 2, 4, 6 }, result.Unwrap());
    }

    [Fact]
    public async Task Result_TraverseAsync_OneFails_ReturnsErr()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return x == 2
                ? Result<int, string>.Err("error")
                : Result<int, string>.Ok(x * 2);
        });

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void RealWorld_ValidateMultipleInputs()
    {
        var inputs = new[] { "10", "20", "30", "40" };

        var result = inputs
            .Traverse(s => int.TryParse(s, out var v)
                ? Result<int, string>.Ok(v)
                : Result<int, string>.Err($"Invalid: {s}"))
            .Map(numbers => numbers.Sum());

        Assert.True(result.IsOk);
        Assert.Equal(100, result.Unwrap());
    }

    [Fact]
    public void RealWorld_ProcessUserInputs_StopOnFirstError()
    {
        var userInputs = new[] { "5", "10", "invalid", "20" };

        var result = userInputs.Traverse(input =>
            int.TryParse(input, out var value) && value > 0
                ? Result<int, string>.Ok(value)
                : Result<int, string>.Err($"Invalid input: {input}"));

        Assert.True(result.IsErr);
        Assert.Contains("invalid", result.UnwrapErr());
    }

    [Fact]
    public void RealWorld_FilterAndProcessOptions()
    {
        var possibleValues = new[] { "1", "not", "3", "nope", "5" };

        var processed = possibleValues
            .Choose(s => int.TryParse(s, out var v)
                ? Option<int>.Some(v)
                : Option<int>.None())
            .Where(x => x > 2)
            .Sum();

        Assert.Equal(8, processed); // 3 + 5
    }

    #endregion
}

