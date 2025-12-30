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

    #region Either<TLeft, TRight> Tests

    [Fact]
    public void Either_ExpectRight_ReturnsRightValue()
    {
        var either = Either<string, int>.Right(42);
        Assert.Equal(42, either.ExpectRight("Expected Right"));
    }

    [Fact]
    public void Either_ExpectRight_ThrowsOnLeft()
    {
        var either = Either<string, int>.Left("error");
        var ex = Assert.Throws<InvalidOperationException>(() => either.ExpectRight("Expected Right"));
        Assert.Equal("Expected Right: error", ex.Message);
    }

    [Fact]
    public void Either_ExpectLeft_ReturnsLeftValue()
    {
        var either = Either<string, int>.Left("error");
        Assert.Equal("error", either.ExpectLeft("Expected Left"));
    }

    [Fact]
    public void Either_ExpectLeft_ThrowsOnRight()
    {
        var either = Either<string, int>.Right(42);
        var ex = Assert.Throws<InvalidOperationException>(() => either.ExpectLeft("Expected Left"));
        Assert.Equal("Expected Left: 42", ex.Message);
    }

    [Fact]
    public void Either_GetRightOrThrow_ReturnsRightValue()
    {
        var either = Either<string, int>.Right(42);
        Assert.Equal(42, either.GetRightOrThrow());
    }

    [Fact]
    public void Either_GetRightOrThrow_ThrowsOnLeft()
    {
        var either = Either<string, int>.Left("error");
        var ex = Assert.Throws<InvalidOperationException>(() => either.GetRightOrThrow());
        Assert.Contains("Left", ex.Message);
    }

    [Fact]
    public void Either_GetRightOrThrowWithMessage_ReturnsRightValue()
    {
        var either = Either<string, int>.Right(42);
        Assert.Equal(42, either.GetRightOrThrow("Expected success"));
    }

    [Fact]
    public void Either_GetRightOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var either = Either<string, int>.Left("failed");
        var ex = Assert.Throws<InvalidOperationException>(() => either.GetRightOrThrow("Operation must succeed"));
        Assert.Equal("Operation must succeed: failed", ex.Message);
    }

    [Fact]
    public void Either_GetLeftOrThrow_ReturnsLeftValue()
    {
        var either = Either<string, int>.Left("error");
        Assert.Equal("error", either.GetLeftOrThrow());
    }

    [Fact]
    public void Either_GetLeftOrThrow_ThrowsOnRight()
    {
        var either = Either<string, int>.Right(42);
        var ex = Assert.Throws<InvalidOperationException>(() => either.GetLeftOrThrow());
        Assert.Contains("Right", ex.Message);
    }

    [Fact]
    public void Either_GetLeftOrThrowWithMessage_ReturnsLeftValue()
    {
        var either = Either<string, int>.Left("error");
        Assert.Equal("error", either.GetLeftOrThrow("Expected error"));
    }

    [Fact]
    public void Either_GetLeftOrThrowWithMessage_ThrowsWithCustomMessage()
    {
        var either = Either<string, int>.Right(42);
        var ex = Assert.Throws<InvalidOperationException>(() => either.GetLeftOrThrow("Should have failed"));
        Assert.Equal("Should have failed: 42", ex.Message);
    }

    #endregion

    #region Try<T> Tests

    [Fact]
    public void Try_Unwrap_ReturnsSuccessValue()
    {
        var result = Try<int>.Success(42);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Try_Unwrap_ThrowsOnFailure()
    {
        var result = Try<int>.Failure(new Exception("error"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.Unwrap());
        Assert.Contains("error", ex.Message);
    }

    [Fact]
    public void Try_Expect_ReturnsSuccessValue()
    {
        var result = Try<int>.Success(42);
        Assert.Equal(42, result.Expect("Expected value"));
    }

    [Fact]
    public void Try_Expect_ThrowsWithCustomMessage()
    {
        var result = Try<int>.Failure(new Exception("failed"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.Expect("Must succeed"));
        Assert.Equal("Must succeed: failed", ex.Message);
    }

    [Fact]
    public void Try_ExpectFailure_ReturnsException()
    {
        var exception = new InvalidOperationException("test error");
        var result = Try<int>.Failure(exception);
        Assert.Same(exception, result.ExpectFailure("Expected failure"));
    }

    [Fact]
    public void Try_ExpectFailure_ThrowsOnSuccess()
    {
        var result = Try<int>.Success(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.ExpectFailure("Should have failed"));
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
        Assert.Equal(42, validation.Expect("Expected value"));
    }

    [Fact]
    public void Validation_Expect_ThrowsWithCustomMessage()
    {
        var validation = Validation<int, string>.Invalid("error");
        var ex = Assert.Throws<InvalidOperationException>(() => validation.Expect("Must be valid"));
        Assert.Equal("Must be valid: error", ex.Message);
    }

    [Fact]
    public void Validation_ExpectErrors_ReturnsErrors()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var errors = validation.ExpectErrors("Expected errors");
        Assert.Equal(2, errors.Count);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void Validation_ExpectErrors_ThrowsOnValid()
    {
        var validation = Validation<int, string>.Valid(42);
        var ex = Assert.Throws<InvalidOperationException>(() => validation.ExpectErrors("Should be invalid"));
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
