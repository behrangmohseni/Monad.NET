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
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
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
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
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
        Assert.Equal(1, result.GetValue());
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
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
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
        Assert.Equal("error1", result.GetError());
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
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
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
        Assert.Equal("Invalid: invalid", result.GetError());
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
        Assert.Equal(1, result.GetValue());
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
        Assert.Equal("error3", result.GetError());
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
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
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
        Assert.Equal(new[] { 2, 4, 6 }, result.GetValue());
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
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
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
        Assert.Equal(new[] { 2, 4, 6 }, result.GetValue());
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
        Assert.Equal("error", result.GetError());
    }

    #endregion

    #region Validation Collection Tests

    [Fact]
    public void Validation_Sequence_AllValid_ReturnsValidOfList()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Valid(2),
            Validation<int, string>.Valid(3)
        };

        var result = validations.Sequence();

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void Validation_Sequence_WithInvalid_ReturnsAllErrors()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid("error1"),
            Validation<int, string>.Invalid("error2")
        };

        var result = validations.Sequence();

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Equal(2, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void Validation_Sequence_AccumulatesMultipleErrorsPerValidation()
    {
        var validations = new[]
        {
            Validation<int, string>.Invalid(new[] { "error1", "error2" }),
            Validation<int, string>.Invalid("error3")
        };

        var result = validations.Sequence();

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Equal(3, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
        Assert.Contains("error3", errors);
    }

    [Fact]
    public void Validation_Traverse_AllSucceed_ReturnsValid()
    {
        var numbers = new[] { "1", "2", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? Validation<int, string>.Valid(value)
                : Validation<int, string>.Invalid($"Invalid: {s}"));

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void Validation_Traverse_MultipleFail_ReturnsAllErrors()
    {
        var numbers = new[] { "1", "invalid", "3", "bad" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? Validation<int, string>.Valid(value)
                : Validation<int, string>.Invalid($"Invalid: {s}"));

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Equal(2, errors.Length);
        Assert.Contains("Invalid: invalid", errors);
        Assert.Contains("Invalid: bad", errors);
    }

    [Fact]
    public void Validation_CollectValid_FiltersValidValues()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid("error"),
            Validation<int, string>.Valid(3),
            Validation<int, string>.Invalid("error2"),
            Validation<int, string>.Valid(5)
        };

        var valids = validations.CollectValid().ToList();

        Assert.Equal(new[] { 1, 3, 5 }, valids);
    }

    [Fact]
    public void Validation_CollectErrors_FiltersAndFlattensErrors()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid(new[] { "error1", "error2" }),
            Validation<int, string>.Valid(3),
            Validation<int, string>.Invalid("error3")
        };

        var errors = validations.CollectErrors().ToList();

        Assert.Equal(new[] { "error1", "error2", "error3" }, errors);
    }

    [Fact]
    public void Validation_Partition_SeparatesValidsAndErrors()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid("error1"),
            Validation<int, string>.Valid(2),
            Validation<int, string>.Invalid(new[] { "error2", "error3" }),
            Validation<int, string>.Valid(3)
        };

        var (valids, errors) = validations.Partition();

        Assert.Equal(new[] { 1, 2, 3 }, valids);
        Assert.Equal(new[] { "error1", "error2", "error3" }, errors);
    }

    #endregion

    #region Try Collection Tests

    [Fact]
    public void Try_Sequence_AllSuccess_ReturnsSuccessOfList()
    {
        var tries = new[]
        {
            Try<int>.Success(1),
            Try<int>.Success(2),
            Try<int>.Success(3)
        };

        var result = tries.Sequence();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void Try_Sequence_WithFailure_ReturnsFirstFailure()
    {
        var exception1 = new InvalidOperationException("error1");
        var exception2 = new InvalidOperationException("error2");

        var tries = new[]
        {
            Try<int>.Success(1),
            Try<int>.Failure(exception1),
            Try<int>.Failure(exception2)
        };

        var result = tries.Sequence();

        Assert.True(result.IsFailure);
        Assert.Same(exception1, result.GetException());
    }

    [Fact]
    public void Try_Traverse_AllSucceed_ReturnsSuccess()
    {
        var numbers = new[] { "1", "2", "3" };

        var result = numbers.Traverse(s =>
            Try<int>.Of(() => int.Parse(s)));

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void Try_Traverse_OneFails_ReturnsFirstFailure()
    {
        var numbers = new[] { "1", "invalid", "3" };

        var result = numbers.Traverse(s =>
            Try<int>.Of(() => int.Parse(s)));

        Assert.True(result.IsFailure);
        Assert.IsType<FormatException>(result.GetException());
    }

    [Fact]
    public void Try_CollectSuccess_FiltersSuccessValues()
    {
        var tries = new[]
        {
            Try<int>.Success(1),
            Try<int>.Failure(new Exception("error")),
            Try<int>.Success(3),
            Try<int>.Failure(new Exception("error2")),
            Try<int>.Success(5)
        };

        var successes = tries.CollectSuccess().ToList();

        Assert.Equal(new[] { 1, 3, 5 }, successes);
    }

    [Fact]
    public void Try_CollectFailures_FiltersExceptions()
    {
        var ex1 = new InvalidOperationException("error1");
        var ex2 = new InvalidOperationException("error2");

        var tries = new[]
        {
            Try<int>.Success(1),
            Try<int>.Failure(ex1),
            Try<int>.Success(3),
            Try<int>.Failure(ex2)
        };

        var failures = tries.CollectFailures().ToList();

        Assert.Equal(2, failures.Count);
        Assert.Same(ex1, failures[0]);
        Assert.Same(ex2, failures[1]);
    }

    [Fact]
    public void Try_Partition_SeparatesSuccessesAndFailures()
    {
        var ex1 = new InvalidOperationException("error1");
        var ex2 = new InvalidOperationException("error2");

        var tries = new[]
        {
            Try<int>.Success(1),
            Try<int>.Failure(ex1),
            Try<int>.Success(2),
            Try<int>.Failure(ex2),
            Try<int>.Success(3)
        };

        var (successes, failures) = tries.Partition();

        Assert.Equal(new[] { 1, 2, 3 }, successes);
        Assert.Equal(2, failures.Count);
        Assert.Same(ex1, failures[0]);
        Assert.Same(ex2, failures[1]);
    }

    [Fact]
    public void Try_FirstSuccess_ReturnsFirstSuccessValue()
    {
        var tries = new[]
        {
            Try<int>.Failure(new Exception("error1")),
            Try<int>.Success(1),
            Try<int>.Success(2)
        };

        var result = tries.FirstSuccess();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void Try_FirstSuccess_AllFailures_ReturnsLastFailure()
    {
        var ex1 = new Exception("error1");
        var ex2 = new Exception("error2");
        var ex3 = new Exception("error3");

        var tries = new[]
        {
            Try<int>.Failure(ex1),
            Try<int>.Failure(ex2),
            Try<int>.Failure(ex3)
        };

        var result = tries.FirstSuccess();

        Assert.True(result.IsFailure);
        Assert.Same(ex3, result.GetException());
    }

    #endregion

    #region RemoteData Collection Tests

    [Fact]
    public void RemoteData_Sequence_AllSuccess_ReturnsSuccessOfList()
    {
        var items = new[]
        {
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Success(2),
            RemoteData<int, string>.Success(3)
        };

        var result = items.Sequence();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void RemoteData_Sequence_WithFailure_ReturnsFirstFailure()
    {
        var items = new[]
        {
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Failure("error1"),
            RemoteData<int, string>.Failure("error2")
        };

        var result = items.Sequence();

        Assert.True(result.IsFailure);
        Assert.Equal("error1", result.GetError());
    }

    [Fact]
    public void RemoteData_Sequence_WithLoading_ReturnsLoading()
    {
        var items = new[]
        {
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.Success(3)
        };

        var result = items.Sequence();

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void RemoteData_Sequence_FailureTakesPriorityOverLoading()
    {
        var items = new[]
        {
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.Failure("error"),
            RemoteData<int, string>.NotAsked()
        };

        var result = items.Sequence();

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void RemoteData_Sequence_LoadingTakesPriorityOverNotAsked()
    {
        var items = new[]
        {
            RemoteData<int, string>.NotAsked(),
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.NotAsked()
        };

        var result = items.Sequence();

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void RemoteData_Traverse_AllSucceed_ReturnsSuccess()
    {
        var numbers = new[] { "1", "2", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? RemoteData<int, string>.Success(value)
                : RemoteData<int, string>.Failure($"Invalid: {s}"));

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void RemoteData_Traverse_WithFailure_ReturnsFailure()
    {
        var numbers = new[] { "1", "invalid", "3" };

        var result = numbers.Traverse(s =>
            int.TryParse(s, out var value)
                ? RemoteData<int, string>.Success(value)
                : RemoteData<int, string>.Failure($"Invalid: {s}"));

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid: invalid", result.GetError());
    }

    [Fact]
    public void RemoteData_CollectSuccess_FiltersSuccessValues()
    {
        var items = new[]
        {
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Failure("error"),
            RemoteData<int, string>.Success(3),
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.Success(5)
        };

        var successes = items.CollectSuccess().ToList();

        Assert.Equal(new[] { 1, 3, 5 }, successes);
    }

    [Fact]
    public void RemoteData_CollectFailures_FiltersFailureErrors()
    {
        var items = new[]
        {
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Failure("error1"),
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.Failure("error2")
        };

        var failures = items.CollectFailures().ToList();

        Assert.Equal(new[] { "error1", "error2" }, failures);
    }

    [Fact]
    public void RemoteData_Partition_SeparatesByState()
    {
        var items = new[]
        {
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Failure("error1"),
            RemoteData<int, string>.Success(2),
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.NotAsked(),
            RemoteData<int, string>.Failure("error2"),
            RemoteData<int, string>.Success(3),
            RemoteData<int, string>.Loading()
        };

        var (successes, failures, loadingCount, notAskedCount) = items.Partition();

        Assert.Equal(new[] { 1, 2, 3 }, successes);
        Assert.Equal(new[] { "error1", "error2" }, failures);
        Assert.Equal(2, loadingCount);
        Assert.Equal(1, notAskedCount);
    }

    [Fact]
    public void RemoteData_FirstSuccess_ReturnsFirstSuccessValue()
    {
        var items = new[]
        {
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.Success(1),
            RemoteData<int, string>.Success(2)
        };

        var result = items.FirstSuccess();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public void RemoteData_FirstSuccess_NoSuccess_ReturnsFirstNonSuccess()
    {
        var items = new[]
        {
            RemoteData<int, string>.Loading(),
            RemoteData<int, string>.Failure("error"),
            RemoteData<int, string>.NotAsked()
        };

        var result = items.FirstSuccess();

        Assert.True(result.IsLoading);
    }

    #endregion

    #region Async Collection Tests - Validation

    [Fact]
    public async Task Validation_SequenceAsync_AllValid_ReturnsValid()
    {
        var validationTasks = new[]
        {
            Task.FromResult(Validation<int, string>.Valid(1)),
            Task.FromResult(Validation<int, string>.Valid(2)),
            Task.FromResult(Validation<int, string>.Valid(3))
        };

        var result = await validationTasks.SequenceAsync();

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public async Task Validation_SequenceAsync_WithInvalid_AccumulatesAllErrors()
    {
        var validationTasks = new[]
        {
            Task.FromResult(Validation<int, string>.Valid(1)),
            Task.FromResult(Validation<int, string>.Invalid("error1")),
            Task.FromResult(Validation<int, string>.Invalid("error2"))
        };

        var result = await validationTasks.SequenceAsync();

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Equal(2, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public async Task Validation_TraverseAsync_AllSucceed_ReturnsValid()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return Validation<int, string>.Valid(x * 2);
        });

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 2, 4, 6 }, result.GetValue());
    }

    [Fact]
    public async Task Validation_TraverseAsync_MultipleFail_AccumulatesAllErrors()
    {
        var numbers = new[] { 1, -2, 3, -4 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return x > 0
                ? Validation<int, string>.Valid(x)
                : Validation<int, string>.Invalid($"Negative: {x}");
        });

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Equal(2, errors.Length);
        Assert.Contains("Negative: -2", errors);
        Assert.Contains("Negative: -4", errors);
    }

    #endregion

    #region Async Collection Tests - Try

    [Fact]
    public async Task Try_SequenceAsync_AllSuccess_ReturnsSuccess()
    {
        var tryTasks = new[]
        {
            Task.FromResult(Try<int>.Success(1)),
            Task.FromResult(Try<int>.Success(2)),
            Task.FromResult(Try<int>.Success(3))
        };

        var result = await tryTasks.SequenceAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public async Task Try_SequenceAsync_WithFailure_ReturnsFirstFailure()
    {
        var exception = new InvalidOperationException("error");
        var tryTasks = new[]
        {
            Task.FromResult(Try<int>.Success(1)),
            Task.FromResult(Try<int>.Failure(exception)),
            Task.FromResult(Try<int>.Failure(new Exception("error2")))
        };

        var result = await tryTasks.SequenceAsync();

        Assert.True(result.IsFailure);
        Assert.Same(exception, result.GetException());
    }

    [Fact]
    public async Task Try_TraverseAsync_AllSucceed_ReturnsSuccess()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return Try<int>.Success(x * 2);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 2, 4, 6 }, result.GetValue());
    }

    [Fact]
    public async Task Try_TraverseAsync_OneFails_ReturnsFirstFailure()
    {
        var exception = new InvalidOperationException("middle failed");
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return x == 2
                ? Try<int>.Failure(exception)
                : Try<int>.Success(x * 2);
        });

        Assert.True(result.IsFailure);
        Assert.Same(exception, result.GetException());
    }

    #endregion

    #region Async Collection Tests - RemoteData

    [Fact]
    public async Task RemoteData_SequenceAsync_AllSuccess_ReturnsSuccess()
    {
        var tasks = new[]
        {
            Task.FromResult(RemoteData<int, string>.Success(1)),
            Task.FromResult(RemoteData<int, string>.Success(2)),
            Task.FromResult(RemoteData<int, string>.Success(3))
        };

        var result = await tasks.SequenceAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public async Task RemoteData_SequenceAsync_WithFailure_ReturnsFailure()
    {
        var tasks = new[]
        {
            Task.FromResult(RemoteData<int, string>.Success(1)),
            Task.FromResult(RemoteData<int, string>.Failure("error")),
            Task.FromResult(RemoteData<int, string>.Loading())
        };

        var result = await tasks.SequenceAsync();

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public async Task RemoteData_TraverseAsync_AllSucceed_ReturnsSuccess()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return RemoteData<int, string>.Success(x * 2);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 2, 4, 6 }, result.GetValue());
    }

    [Fact]
    public async Task RemoteData_TraverseAsync_WithLoading_ReturnsLoading()
    {
        var numbers = new[] { 1, 2, 3 };

        var result = await numbers.TraverseAsync(async x =>
        {
            await Task.Delay(1);
            return x == 2
                ? RemoteData<int, string>.Loading()
                : RemoteData<int, string>.Success(x * 2);
        });

        Assert.True(result.IsLoading);
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
        Assert.Equal(100, result.GetValue());
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
        Assert.Contains("invalid", result.GetError());
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

    [Fact]
    public void RealWorld_FormValidation_AccumulatesAllErrors()
    {
        var formFields = new Dictionary<string, string>
        {
            { "name", "" },
            { "email", "invalid-email" },
            { "age", "-5" }
        };

        Validation<string, string> ValidateNotEmpty(string field, string value) =>
            string.IsNullOrWhiteSpace(value)
                ? Validation<string, string>.Invalid($"{field} is required")
                : Validation<string, string>.Valid(value);

        Validation<string, string> ValidateEmail(string value) =>
            value.Contains('@')
                ? Validation<string, string>.Valid(value)
                : Validation<string, string>.Invalid("Invalid email format");

        Validation<int, string> ValidateAge(string value) =>
            int.TryParse(value, out var age) && age > 0
                ? Validation<int, string>.Valid(age)
                : Validation<int, string>.Invalid("Age must be a positive number");

        var validations = new Validation<object, string>[]
        {
            ValidateNotEmpty("name", formFields["name"]).Map(x => (object)x),
            ValidateEmail(formFields["email"]).Map(x => (object)x),
            ValidateAge(formFields["age"]).Map(x => (object)x)
        };

        var result = validations.Sequence();

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Equal(3, errors.Length);
        Assert.Contains("name is required", errors);
        Assert.Contains("Invalid email format", errors);
        Assert.Contains("Age must be a positive number", errors);
    }

    #endregion
}

