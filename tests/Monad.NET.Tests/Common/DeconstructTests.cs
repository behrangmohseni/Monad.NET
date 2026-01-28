using Xunit;

namespace Monad.NET.Tests;

public class DeconstructTests
{
    #region Option Deconstruction Tests

    [Fact]
    public void Option_Deconstruct_Some_ReturnsValueAndTrue()
    {
        var option = Option<int>.Some(42);
        var (value, isSome) = option;

        Assert.Equal(42, value);
        Assert.True(isSome);
    }

    [Fact]
    public void Option_Deconstruct_None_ReturnsDefaultAndFalse()
    {
        var option = Option<int>.None();
        var (value, isSome) = option;

        Assert.Equal(default, value);
        Assert.False(isSome);
    }

    [Fact]
    public void Option_Deconstruct_WithPatternMatching()
    {
        var option = Option<string>.Some("hello");
        var (value, isSome) = option;

        var result = isSome ? $"Got: {value}" : "None";

        Assert.Equal("Got: hello", result);
    }

    #endregion

    #region Result Deconstruction Tests

    [Fact]
    public void Result_Deconstruct_Ok_ReturnsValueAndTrue()
    {
        var result = Result<int, string>.Ok(42);
        var (value, isOk) = result;

        Assert.Equal(42, value);
        Assert.True(isOk);
    }

    [Fact]
    public void Result_Deconstruct_Err_ReturnsDefaultAndFalse()
    {
        var result = Result<int, string>.Err("error");
        var (value, isOk) = result;

        Assert.Equal(default, value);
        Assert.False(isOk);
    }

    [Fact]
    public void Result_DeconstructFull_Ok_ReturnsAllComponents()
    {
        var result = Result<int, string>.Ok(42);
        var (value, error, isOk) = result;

        Assert.Equal(42, value);
        Assert.Null(error);
        Assert.True(isOk);
    }

    [Fact]
    public void Result_DeconstructFull_Err_ReturnsAllComponents()
    {
        var result = Result<int, string>.Err("oops");
        var (value, error, isOk) = result;

        Assert.Equal(default, value);
        Assert.Equal("oops", error);
        Assert.False(isOk);
    }

    [Fact]
    public void Result_Deconstruct_WithPatternMatching()
    {
        var result = Result<int, string>.Ok(100);
        var (value, error, isOk) = result;

        var message = isOk ? $"Success: {value}" : $"Error: {error}";

        Assert.Equal("Success: 100", message);
    }

    #endregion

    #region Try Deconstruction Tests

    [Fact]
    public void Try_Deconstruct_Success_ReturnsValueAndTrue()
    {
        var tryResult = Try<int>.Success(42);
        var (value, isSuccess) = tryResult;

        Assert.Equal(42, value);
        Assert.True(isSuccess);
    }

    [Fact]
    public void Try_Deconstruct_Failure_ReturnsDefaultAndFalse()
    {
        var tryResult = Try<int>.Failure(new Exception("oops"));
        var (value, isSuccess) = tryResult;

        Assert.Equal(default, value);
        Assert.False(isSuccess);
    }

    [Fact]
    public void Try_DeconstructFull_Success_ReturnsAllComponents()
    {
        var tryResult = Try<int>.Success(42);
        var (value, exception, isSuccess) = tryResult;

        Assert.Equal(42, value);
        Assert.Null(exception);
        Assert.True(isSuccess);
    }

    [Fact]
    public void Try_DeconstructFull_Failure_ReturnsAllComponents()
    {
        var ex = new InvalidOperationException("test error");
        var tryResult = Try<int>.Failure(ex);
        var (value, exception, isSuccess) = tryResult;

        Assert.Equal(default, value);
        Assert.Same(ex, exception);
        Assert.False(isSuccess);
    }

    [Fact]
    public void Try_Deconstruct_WithPatternMatching()
    {
        var tryResult = Try<int>.Of(() => int.Parse("42"));
        var (value, exception, isSuccess) = tryResult;

        var message = isSuccess ? $"Parsed: {value}" : $"Failed: {exception?.Message}";

        Assert.Equal("Parsed: 42", message);
    }

    #endregion

    #region Validation Deconstruction Tests

    [Fact]
    public void Validation_Deconstruct_Valid_ReturnsValueAndTrue()
    {
        var validation = Validation<int, string>.Valid(42);
        var (value, isValid) = validation;

        Assert.Equal(42, value);
        Assert.True(isValid);
    }

    [Fact]
    public void Validation_Deconstruct_Invalid_ReturnsDefaultAndFalse()
    {
        var validation = Validation<int, string>.Invalid("error");
        var (value, isValid) = validation;

        Assert.Equal(default, value);
        Assert.False(isValid);
    }

    [Fact]
    public void Validation_DeconstructFull_Valid_ReturnsAllComponents()
    {
        var validation = Validation<int, string>.Valid(42);
        var (value, errors, isValid) = validation;

        Assert.Equal(42, value);
        Assert.Empty(errors);
        Assert.True(isValid);
    }

    [Fact]
    public void Validation_DeconstructFull_Invalid_ReturnsAllComponents()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var (value, errors, isValid) = validation;

        Assert.Equal(default, value);
        Assert.Equal(2, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
        Assert.False(isValid);
    }

    [Fact]
    public void Validation_Deconstruct_WithPatternMatching()
    {
        var validation = Validation<int, string>.Valid(100);
        var (value, isValid) = validation;

        var message = isValid ? $"Valid: {value}" : "Invalid";

        Assert.Equal("Valid: 100", message);
    }

    #endregion

    #region RemoteData Deconstruction Tests

    [Fact]
    public void RemoteData_Deconstruct_Success_ReturnsDataAndTrue()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var (data, isSuccess) = remoteData;

        Assert.Equal(42, data);
        Assert.True(isSuccess);
    }

    [Fact]
    public void RemoteData_Deconstruct_NotSuccess_ReturnsDefaultAndFalse()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var (data, isSuccess) = remoteData;

        Assert.Equal(default, data);
        Assert.False(isSuccess);
    }

    [Fact]
    public void RemoteData_DeconstructFull_NotAsked_ReturnsAllComponents()
    {
        var remoteData = RemoteData<int, string>.NotAsked();
        var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;

        Assert.Equal(default, data);
        Assert.Null(error);
        Assert.True(isNotAsked);
        Assert.False(isLoading);
        Assert.False(isSuccess);
        Assert.False(isFailure);
    }

    [Fact]
    public void RemoteData_DeconstructFull_Loading_ReturnsAllComponents()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;

        Assert.Equal(default, data);
        Assert.Null(error);
        Assert.False(isNotAsked);
        Assert.True(isLoading);
        Assert.False(isSuccess);
        Assert.False(isFailure);
    }

    [Fact]
    public void RemoteData_DeconstructFull_Success_ReturnsAllComponents()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;

        Assert.Equal(42, data);
        Assert.Null(error);
        Assert.False(isNotAsked);
        Assert.False(isLoading);
        Assert.True(isSuccess);
        Assert.False(isFailure);
    }

    [Fact]
    public void RemoteData_DeconstructFull_Failure_ReturnsAllComponents()
    {
        var remoteData = RemoteData<int, string>.Failure("oops");
        var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;

        Assert.Equal(default, data);
        Assert.Equal("oops", error);
        Assert.False(isNotAsked);
        Assert.False(isLoading);
        Assert.False(isSuccess);
        Assert.True(isFailure);
    }

    [Fact]
    public void RemoteData_Deconstruct_WithPatternMatching()
    {
        var remoteData = RemoteData<int, string>.Success(99);

        var message = (remoteData.IsNotAsked, remoteData.IsLoading, remoteData.IsSuccess, remoteData.IsFailure) switch
        {
            (true, _, _, _) => "Not asked",
            (_, true, _, _) => "Loading",
            (_, _, true, _) => $"Success: {remoteData.GetValue()}",
            (_, _, _, true) => $"Failure: {remoteData.GetError()}",
            _ => "Unknown state"
        };

        Assert.Equal("Success: 99", message);
    }

    #endregion

    #region StateResult Deconstruction Tests

    [Fact]
    public void StateResult_Deconstruct_ReturnsComponents()
    {
        var stateResult = new StateResult<int, string>("value", 42);
        var (value, state) = stateResult;

        Assert.Equal("value", value);
        Assert.Equal(42, state);
    }

    #endregion
}

