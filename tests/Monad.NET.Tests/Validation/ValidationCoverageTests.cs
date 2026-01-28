using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for Validation to improve code coverage.
/// </summary>
public class ValidationCoverageTests
{
    #region GetOrThrow with Message Tests

    [Fact]
    public void GetOrThrow_WithMessage_Valid_ReturnsValue()
    {
        var validation = Validation<int, string>.Valid(42);
        var value = validation.GetOrThrow("Should not fail");
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetOrThrow_WithMessage_Invalid_ThrowsWithMessage()
    {
        var validation = Validation<int, string>.Invalid("error");
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("error", ex.Message);
    }

    #endregion

    #region GetErrorsOrThrow with Message Tests

    [Fact]
    public void GetErrorsOrThrow_WithMessage_Invalid_ReturnsErrors()
    {
        var validation = Validation<int, string>.Invalid("error");
        var errors = validation.GetErrorsOrThrow("Should not fail");
        Assert.Single(errors);
        Assert.Equal("error", errors[0]);
    }

    [Fact]
    public void GetErrorsOrThrow_WithMessage_Valid_ThrowsWithMessage()
    {
        var validation = Validation<int, string>.Valid(42);
        var ex = Assert.Throws<InvalidOperationException>(() => validation.GetErrorsOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    #endregion

    #region Ensure with Error Factory Tests

    [Fact]
    public void Ensure_WithErrorFactory_Valid_PredicateTrue_ReturnsOriginal()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Ensure(x => x > 40, () => "too small");
        Assert.True(result.IsValid);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Ensure_WithErrorFactory_Valid_PredicateFalse_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Ensure(x => x > 50, () => "too small");
        Assert.True(result.IsInvalid);
        Assert.Equal("too small", result.GetErrors()[0]);
    }

    [Fact]
    public void Ensure_WithErrorFactory_Invalid_ReturnsOriginal()
    {
        var validation = Validation<int, string>.Invalid("original error");
        var result = validation.Ensure(x => x > 50, () => "too small");
        Assert.True(result.IsInvalid);
        Assert.Equal("original error", result.GetErrors()[0]);
    }

    #endregion

    #region IComparable Tests

    [Fact]
    public void CompareTo_Object_Null_ReturnsPositive()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.Equal(1, ((IComparable)validation).CompareTo(null));
    }

    [Fact]
    public void CompareTo_Object_InvalidType_ThrowsArgumentException()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.Throws<ArgumentException>(() => ((IComparable)validation).CompareTo("not a Validation"));
    }

    [Fact]
    public void CompareTo_Object_ValidValidation_ComparesCorrectly()
    {
        var validation1 = Validation<int, string>.Valid(42);
        var validation2 = Validation<int, string>.Valid(50);
        Assert.True(((IComparable)validation1).CompareTo(validation2) < 0);
    }

    #endregion

    #region Combine Extension Tests

    [Fact]
    public void Combine_Extension_AllValid_ReturnsLastValue()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Valid(2),
            Validation<int, string>.Valid(3)
        };

        var combined = validations.Combine();
        Assert.True(combined.IsValid);
        Assert.Equal(3, combined.GetValue());
    }

    [Fact]
    public void Combine_Extension_HasInvalid_ReturnsAllErrors()
    {
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid("error1"),
            Validation<int, string>.Invalid("error2")
        };

        var combined = validations.Combine();
        Assert.True(combined.IsInvalid);
        Assert.Equal(2, combined.GetErrors().Length);
        Assert.Contains("error1", combined.GetErrors());
        Assert.Contains("error2", combined.GetErrors());
    }

    #endregion

    #region Async ZipAsync Three Tasks Tests

    [Fact]
    public async Task ZipAsync_ThreeTasks_AllValid_ReturnsTuple()
    {
        var task1 = Task.FromResult(Validation<int, string>.Valid(1));
        var task2 = Task.FromResult(Validation<int, string>.Valid(2));
        var task3 = Task.FromResult(Validation<int, string>.Valid(3));

        var combined = await ValidationExtensions.ZipAsync(task1, task2, task3);
        Assert.True(combined.IsValid);
        Assert.Equal((1, 2, 3), combined.GetValue());
    }

    [Fact]
    public async Task ZipAsync_ThreeTasks_HasInvalid_AccumulatesErrors()
    {
        var task1 = Task.FromResult(Validation<int, string>.Invalid("error1"));
        var task2 = Task.FromResult(Validation<int, string>.Valid(2));
        var task3 = Task.FromResult(Validation<int, string>.Invalid("error2"));

        var combined = await ValidationExtensions.ZipAsync(task1, task2, task3);
        Assert.True(combined.IsInvalid);
        Assert.Equal(2, combined.GetErrors().Length);
        Assert.Contains("error1", combined.GetErrors());
        Assert.Contains("error2", combined.GetErrors());
    }

    #endregion

    #region Async CombineAsync Tests

    [Fact]
    public async Task CombineAsync_Collection_AllValid_ReturnsList()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation<int, string>.Valid(1)),
            Task.FromResult(Validation<int, string>.Valid(2)),
            Task.FromResult(Validation<int, string>.Valid(3))
        };

        var combined = await tasks.CombineAsync();
        Assert.True(combined.IsValid);
        Assert.Equal(new[] { 1, 2, 3 }, combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_Collection_HasInvalid_AccumulatesAllErrors()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation<int, string>.Valid(1)),
            Task.FromResult(Validation<int, string>.Invalid("error1")),
            Task.FromResult(Validation<int, string>.Invalid("error2"))
        };

        var combined = await tasks.CombineAsync();
        Assert.True(combined.IsInvalid);
        Assert.Equal(2, combined.GetErrors().Length);
    }

    #endregion
}
