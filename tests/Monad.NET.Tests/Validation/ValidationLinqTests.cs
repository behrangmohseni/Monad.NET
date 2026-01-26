using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for ValidationLinq to improve code coverage.
/// </summary>
public class ValidationLinqTests
{
    #region ValidationLinq Select Tests

    [Fact]
    public void Validation_Select_OnValid_TransformsValue()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Select(x => x * 2);

        Assert.True(result.IsValid);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Validation_Select_OnInvalid_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        var result = validation.Select(x => x * 2);

        Assert.True(result.IsInvalid);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("error", errors[0]);
    }

    [Fact]
    public void Validation_Select_OnMultipleErrors_PreservesAllErrors()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var result = validation.Select(x => x * 2);

        Assert.True(result.IsInvalid);
        var errors = result.GetErrorsOrThrow();
        Assert.Equal(2, errors.Count);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    #endregion

    #region ValidationLinq SelectMany Tests

    [Fact]
    public void Validation_SelectMany_BothValid_Chains()
    {
        var result = Validation<int, string>.Valid(10)
            .SelectMany(x => Validation<int, string>.Valid(x + 20));

        Assert.True(result.IsValid);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Validation_SelectMany_FirstInvalid_ReturnsInvalid()
    {
        var result = Validation<int, string>.Invalid("first error")
            .SelectMany(x => Validation<int, string>.Valid(x + 20));

        Assert.True(result.IsInvalid);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("first error", errors[0]);
    }

    [Fact]
    public void Validation_SelectMany_SecondInvalid_ReturnsInvalid()
    {
        var result = Validation<int, string>.Valid(10)
            .SelectMany(_ => Validation<int, string>.Invalid("second error"));

        Assert.True(result.IsInvalid);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("second error", errors[0]);
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_Chains()
    {
        var first = Validation<int, string>.Valid(10);
        var result = first.SelectMany(
            x => Validation<string, string>.Valid($"Value: {x}"),
            (x, y) => $"{y} (original: {x})");

        Assert.True(result.IsValid);
        Assert.Equal("Value: 10 (original: 10)", result.GetValue());
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_FirstInvalid()
    {
        var first = Validation<int, string>.Invalid("error");
        var result = first.SelectMany(
            x => Validation<string, string>.Valid($"Value: {x}"),
            (x, y) => $"{y} (original: {x})");

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_SecondInvalid()
    {
        var first = Validation<int, string>.Valid(10);
        var result = first.SelectMany(
            _ => Validation<string, string>.Invalid("error"),
            (x, y) => $"{y} (original: {x})");

        Assert.True(result.IsInvalid);
    }

    #endregion

    #region LINQ Query Syntax Tests

    [Fact]
    public void Validation_LinqQuery_SimpleChain()
    {
        var result = from a in Validation<int, string>.Valid(1)
                     from b in Validation<int, string>.Valid(2)
                     select a + b;

        Assert.True(result.IsValid);
        Assert.Equal(3, result.GetValue());
    }

    [Fact]
    public void Validation_LinqQuery_WithLet()
    {
        var result = from a in Validation<int, string>.Valid(5)
                     let doubled = a * 2
                     from b in Validation<int, string>.Valid(3)
                     select doubled + b;

        Assert.True(result.IsValid);
        Assert.Equal(13, result.GetValue());
    }

    [Fact]
    public void Validation_LinqQuery_TripleChain()
    {
        var result = from a in Validation<int, string>.Valid(1)
                     from b in Validation<int, string>.Valid(2)
                     from c in Validation<int, string>.Valid(3)
                     select a + b + c;

        Assert.True(result.IsValid);
        Assert.Equal(6, result.GetValue());
    }

    [Fact]
    public void Validation_LinqQuery_FirstInvalid()
    {
        var result = from a in Validation<int, string>.Invalid("first")
                     from b in Validation<int, string>.Valid(2)
                     select a + b;

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void Validation_LinqQuery_MiddleInvalid()
    {
        var result = from a in Validation<int, string>.Valid(1)
                     from b in Validation<int, string>.Invalid("middle")
                     from c in Validation<int, string>.Valid(3)
                     select a + b + c;

        Assert.True(result.IsInvalid);
    }

    #endregion
}

