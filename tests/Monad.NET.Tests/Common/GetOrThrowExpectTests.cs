using Xunit;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for GetOrThrow() and Expect() variants across all monad types.
/// </summary>
public class GetOrThrowExpectTests
{
    #region Option<T> Tests

    [Fact]
    public void Option_GetOrThrow_ReturnsSomeValue()
    {
        var option = Option<int>.Some(42);
        Assert.Equal(42, option.GetOrThrow());
    }

    [Fact]
    public void Option_GetOrThrow_ThrowsOnNone()
    {
        var option = Option<int>.None();
        var ex = Assert.Throws<InvalidOperationException>(() => option.GetOrThrow());
        Assert.Contains("None", ex.Message);
    }

    [Fact]
    public void Option_GetOrThrowWithMessage_ReturnsSomeValue()
    {
        var option = Option<int>.Some(42);
        Assert.Equal(42, option.GetOrThrow("Expected value"));
    }

    [Fact]
    public void Option_GetOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var option = Option<int>.None();
        var ex = Assert.Throws<InvalidOperationException>(() => option.GetOrThrow("Custom error message"));
        Assert.Equal("Custom error message", ex.Message);
    }

    #endregion

    #region Result<T, TErr> Tests

    [Fact]
    public void Result_GetOrThrow_ReturnsOkValue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Equal(42, result.GetOrThrow());
    }

    [Fact]
    public void Result_GetOrThrow_ThrowsOnErr()
    {
        var result = Result<int, string>.Err("error");
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
        Assert.Contains("error", ex.Message);
    }

    [Fact]
    public void Result_GetOrThrowWithMessage_ReturnsOkValue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Equal(42, result.GetOrThrow("Expected success"));
    }

    [Fact]
    public void Result_GetOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var result = Result<int, string>.Err("failed");
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow("Operation must succeed"));
        Assert.Equal("Operation must succeed: failed", ex.Message);
    }

    [Fact]
    public void Result_GetErrorOrThrow_ReturnsErrValue()
    {
        var result = Result<int, string>.Err("error");
        Assert.Equal("error", result.GetErrorOrThrow());
    }

    [Fact]
    public void Result_GetErrorOrThrow_ThrowsOnOk()
    {
        var result = Result<int, string>.Ok(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetErrorOrThrow());
        Assert.Contains("Ok", ex.Message);
    }

    [Fact]
    public void Result_GetErrorOrThrowWithMessage_ReturnsErrValue()
    {
        var result = Result<int, string>.Err("error");
        Assert.Equal("error", result.GetErrorOrThrow("Expected failure"));
    }

    [Fact]
    public void Result_GetErrorOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var result = Result<int, string>.Ok(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetErrorOrThrow("Should have failed"));
        Assert.Equal("Should have failed: 42", ex.Message);
    }

    #endregion

    #region Try<T> Tests

    [Fact]
    public void Try_Unwrap_ReturnsSuccessValue()
    {
        var result = Try<int>.Success(42);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Try_Unwrap_ThrowsOnFailure()
    {
        var result = Try<int>.Failure(new Exception("error"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetValue());
        Assert.Contains("error", ex.Message);
    }

    [Fact]
    public void Try_Expect_ReturnsSuccessValue()
    {
        var result = Try<int>.Success(42);
        Assert.Equal(42, result.GetOrThrow("Expected value"));
    }

    [Fact]
    public void Try_Expect_ThrowsWithCustomMessage()
    {
        var result = Try<int>.Failure(new Exception("failed"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow("Must succeed"));
        Assert.Equal("Must succeed: failed", ex.Message);
    }

    [Fact]
    public void Try_ExpectFailure_ReturnsException()
    {
        var exception = new InvalidOperationException("test error");
        var result = Try<int>.Failure(exception);
        Assert.Same(exception, result.GetExceptionOrThrow("Expected failure"));
    }

    [Fact]
    public void Try_ExpectFailure_ThrowsOnSuccess()
    {
        var result = Try<int>.Success(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetExceptionOrThrow("Should have failed"));
        Assert.Equal("Should have failed: 42", ex.Message);
    }

    [Fact]
    public void Try_GetOrThrow_ReturnsSuccessValue()
    {
        var result = Try<int>.Success(42);
        Assert.Equal(42, result.GetOrThrow());
    }

    [Fact]
    public void Try_GetOrThrow_ThrowsOnFailure()
    {
        var result = Try<int>.Failure(new Exception("error"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
        Assert.Contains("Failure", ex.Message);
    }

    [Fact]
    public void Try_GetOrThrowWithMessage_ReturnsSuccessValue()
    {
        var result = Try<int>.Success(42);
        Assert.Equal(42, result.GetOrThrow("Expected success"));
    }

    [Fact]
    public void Try_GetOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var result = Try<int>.Failure(new Exception("failed"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow("Operation must succeed"));
        Assert.Equal("Operation must succeed: failed", ex.Message);
    }

    [Fact]
    public void Try_GetExceptionOrThrow_ReturnsException()
    {
        var exception = new InvalidOperationException("test error");
        var result = Try<int>.Failure(exception);
        Assert.Same(exception, result.GetExceptionOrThrow());
    }

    [Fact]
    public void Try_GetExceptionOrThrow_ThrowsOnSuccess()
    {
        var result = Try<int>.Success(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetExceptionOrThrow());
        Assert.Contains("Success", ex.Message);
    }

    [Fact]
    public void Try_GetExceptionOrThrowWithMessage_ReturnsException()
    {
        var exception = new InvalidOperationException("test error");
        var result = Try<int>.Failure(exception);
        Assert.Same(exception, result.GetExceptionOrThrow("Expected failure"));
    }

    [Fact]
    public void Try_GetExceptionOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var result = Try<int>.Success(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetExceptionOrThrow("Should have failed"));
        Assert.Equal("Should have failed: 42", ex.Message);
    }

    #endregion

    #region Validation<T, TErr> Tests

    [Fact]
    public void Validation_Expect_ReturnsValidValue()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.Equal(42, validation.GetOrThrow("Expected value"));
    }

    [Fact]
    public void Validation_Expect_ThrowsWithCustomMessage()
    {
        var validation = Validation<int, string>.Invalid("error");
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetOrThrow("Must be valid"));
        Assert.Equal("Must be valid: error", ex.Message);
    }

    [Fact]
    public void Validation_ExpectErrors_ReturnsErrors()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var errors = validation.GetErrorsOrThrow("Expected errors");
        Assert.Equal(2, errors.Count);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void Validation_ExpectErrors_ThrowsOnValid()
    {
        var validation = Validation<int, string>.Valid(42);
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetErrorsOrThrow("Should be invalid"));
        Assert.Equal("Should be invalid: 42", ex.Message);
    }

    [Fact]
    public void Validation_GetOrThrow_ReturnsValidValue()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.Equal(42, validation.GetOrThrow());
    }

    [Fact]
    public void Validation_GetOrThrow_ThrowsOnInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetOrThrow());
        Assert.Contains("Invalid", ex.Message);
    }

    [Fact]
    public void Validation_GetOrThrowWithMessage_ReturnsValidValue()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.Equal(42, validation.GetOrThrow("Expected success"));
    }

    [Fact]
    public void Validation_GetOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var validation = Validation<int, string>.Invalid("failed");
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetOrThrow("Must be valid"));
        Assert.Equal("Must be valid: failed", ex.Message);
    }

    [Fact]
    public void Validation_GetErrorsOrThrow_ReturnsErrors()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var errors = validation.GetErrorsOrThrow();
        Assert.Equal(2, errors.Count);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void Validation_GetErrorsOrThrow_ThrowsOnValid()
    {
        var validation = Validation<int, string>.Valid(42);
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetErrorsOrThrow());
        Assert.Contains("Valid", ex.Message);
    }

    [Fact]
    public void Validation_GetErrorsOrThrowWithMessage_ReturnsErrors()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error" });
        var errors = validation.GetErrorsOrThrow("Expected errors");
        Assert.Single(errors);
        Assert.Contains("error", errors);
    }

    [Fact]
    public void Validation_GetErrorsOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var validation = Validation<int, string>.Valid(42);
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetErrorsOrThrow("Should be invalid"));
        Assert.Equal("Should be invalid: 42", ex.Message);
    }

    [Fact]
    public void Validation_GetOrThrow_IncludesAllErrorsInMessage()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2", "error3" });
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetOrThrow());
        Assert.Contains("error1", ex.Message);
        Assert.Contains("error2", ex.Message);
        Assert.Contains("error3", ex.Message);
    }

    #endregion
}
