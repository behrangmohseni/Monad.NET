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

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Validation_Select_OnInvalid_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        var result = validation.Select(x => x * 2);

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("error", errors[0]);
    }

    [Fact]
    public void Validation_Select_OnMultipleErrors_PreservesAllErrors()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var result = validation.Select(x => x * 2);

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Equal(2, errors.Length);
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

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Validation_SelectMany_FirstInvalid_ShortCircuits()
    {
        // LINQ SelectMany uses short-circuit behavior (like Result) for safety.
        // It does NOT accumulate errors - use Apply() for error accumulation.
        var result = Validation<int, string>.Invalid("first error")
            .SelectMany(_ => Validation<int, string>.Invalid("second error"));

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        // Short-circuits: only first error, second selector not evaluated
        Assert.Single(errors);
        Assert.Equal("first error", errors[0]);
    }

    [Fact]
    public void Validation_SelectMany_FirstInvalid_SecondNotEvaluated()
    {
        // When first validation fails, second selector is not evaluated at all
        var result = Validation<int, string>.Invalid("first error")
            .SelectMany(x => Validation<int, string>.Valid(x + 20));

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("first error", errors[0]);
    }

    [Fact]
    public void Validation_SelectMany_SecondInvalid_ReturnsInvalid()
    {
        var result = Validation<int, string>.Valid(10)
            .SelectMany(_ => Validation<int, string>.Invalid("second error"));

        Assert.True(result.IsError);
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

        Assert.True(result.IsOk);
        Assert.Equal("Value: 10 (original: 10)", result.GetValue());
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_FirstInvalid_ShortCircuits()
    {
        // LINQ SelectMany uses short-circuit behavior for safety.
        // Use Apply() or Zip() for error accumulation.
        var first = Validation<int, string>.Invalid("first error");
        var result = first.SelectMany(
            _ => Validation<string, string>.Invalid("second error"),
            (x, y) => $"{y} (original: {x})");

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        // Short-circuits: only first error
        Assert.Single(errors);
        Assert.Equal("first error", errors[0]);
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_FirstInvalid_SecondNotEvaluated()
    {
        // When first validation fails, collection selector is not called
        var first = Validation<int, string>.Invalid("error");
        var result = first.SelectMany(
            x => Validation<string, string>.Valid($"Value: {x}"),
            (x, y) => $"{y} (original: {x})");

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("error", errors[0]);
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_SecondInvalid()
    {
        var first = Validation<int, string>.Valid(10);
        var result = first.SelectMany(
            _ => Validation<string, string>.Invalid("error"),
            (x, y) => $"{y} (original: {x})");

        Assert.True(result.IsError);
    }

    [Fact]
    public void Validation_SelectMany_NullSelectorThrows()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.Throws<ArgumentNullException>(() =>
            validation.SelectMany((Func<int, Validation<int, string>>)null!));
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_NullCollectionSelectorThrows()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.Throws<ArgumentNullException>(() =>
            validation.SelectMany(
                (Func<int, Validation<int, string>>)null!,
                (x, y) => x + y));
    }

    [Fact]
    public void Validation_SelectMany_WithResultSelector_NullResultSelectorThrows()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.Throws<ArgumentNullException>(() =>
            validation.SelectMany(
                _ => Validation<int, string>.Valid(10),
                (Func<int, int, int>)null!));
    }

    #endregion

    #region LINQ Query Syntax Tests

    [Fact]
    public void Validation_LinqQuery_SimpleChain()
    {
        var result = from a in Validation<int, string>.Valid(1)
                     from b in Validation<int, string>.Valid(2)
                     select a + b;

        Assert.True(result.IsOk);
        Assert.Equal(3, result.GetValue());
    }

    [Fact]
    public void Validation_LinqQuery_WithLet()
    {
        var result = from a in Validation<int, string>.Valid(5)
                     let doubled = a * 2
                     from b in Validation<int, string>.Valid(3)
                     select doubled + b;

        Assert.True(result.IsOk);
        Assert.Equal(13, result.GetValue());
    }

    [Fact]
    public void Validation_LinqQuery_TripleChain()
    {
        var result = from a in Validation<int, string>.Valid(1)
                     from b in Validation<int, string>.Valid(2)
                     from c in Validation<int, string>.Valid(3)
                     select a + b + c;

        Assert.True(result.IsOk);
        Assert.Equal(6, result.GetValue());
    }

    [Fact]
    public void Validation_LinqQuery_FirstInvalid_ShortCircuits()
    {
        // LINQ query syntax short-circuits on first error (like Result).
        // Use Apply() or Zip() for error accumulation.
        var result = from a in Validation<int, string>.Invalid("first")
                     from b in Validation<int, string>.Invalid("second")
                     select a + b;

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        // Short-circuits: only first error
        Assert.Single(errors);
        Assert.Equal("first", errors[0]);
    }

    [Fact]
    public void Validation_LinqQuery_FirstInvalid_SecondNotEvaluated()
    {
        // When first fails, second validation is not evaluated
        var result = from a in Validation<int, string>.Invalid("first")
                     from b in Validation<int, string>.Valid(2)
                     select a + b;

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("first", errors[0]);
    }

    [Fact]
    public void Validation_LinqQuery_MiddleInvalid()
    {
        var result = from a in Validation<int, string>.Valid(1)
                     from b in Validation<int, string>.Invalid("middle")
                     from c in Validation<int, string>.Valid(3)
                     select a + b + c;

        Assert.True(result.IsError);
    }

    [Fact]
    public void Validation_LinqQuery_MultipleInvalid_ShortCircuitsOnFirst()
    {
        // LINQ short-circuits: stops at first error, doesn't accumulate.
        // Use Apply().Apply().Apply() for accumulating multiple validations.
        var result = from a in Validation<int, string>.Invalid("error1")
                     from b in Validation<int, string>.Invalid("error2")
                     from c in Validation<int, string>.Invalid("error3")
                     select a + b + c;

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        // Short-circuits at first error only
        Assert.Single(errors);
        Assert.Equal("error1", errors[0]);
    }

    [Fact]
    public void Validation_Apply_AccumulatesAllErrors()
    {
        // Demonstrate the correct way to accumulate errors
        var v1 = Validation<int, string>.Invalid("error1");
        var v2 = Validation<int, string>.Invalid("error2");
        var v3 = Validation<int, string>.Invalid("error3");

        var result = v1
            .Apply(v2, (a, b) => a + b)
            .Apply(v3, (ab, c) => ab + c);

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Equal(3, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
        Assert.Contains("error3", errors);
    }

    #endregion

}

