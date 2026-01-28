using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Validation<T, E> to improve code coverage.
/// </summary>
public class ValidationExtendedTests
{
    #region Factory Tests

    [Fact]
    public void Valid_CreatesValidInstance()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.True(validation.IsOk);
        Assert.Equal(42, validation.GetValue());
    }

    [Fact]
    public void Invalid_SingleError_CreatesInvalidInstance()
    {
        var validation = Validation<int, string>.Invalid("error");

        Assert.True(validation.IsError);
        var errors = validation.GetErrorsOrThrow();
        Assert.Single(errors);
        Assert.Equal("error", errors[0]);
    }

    [Fact]
    public void Invalid_MultipleErrors_CreatesInvalidInstance()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });

        Assert.True(validation.IsError);
        var errors = validation.GetErrorsOrThrow();
        Assert.Equal(2, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnValid_TransformsValue()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Map(x => x * 2);

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Map_OnInvalid_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        var result = validation.Map(x => x * 2);

        Assert.True(result.IsError);
    }

    #endregion

    #region AndThen Tests

    [Fact]
    public void AndThen_ValidToValid_ReturnsValid()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Bind(x => Validation<string, string>.Valid(x.ToString()));

        Assert.True(result.IsOk);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public void AndThen_ValidToInvalid_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Bind(_ => Validation<string, string>.Invalid("error"));

        Assert.True(result.IsError);
    }

    [Fact]
    public void AndThen_InvalidToAny_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Invalid("first error");
        var result = validation.Bind(x => Validation<string, string>.Valid(x.ToString()));

        Assert.True(result.IsError);
    }

    #endregion

    #region Zip Tests

    [Fact]
    public void Zip_BothValid_CombinesValues()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<string, string>.Valid("hello");

        var result = v1.Zip(v2);

        Assert.True(result.IsOk);
        var (a, b) = result.GetValue();
        Assert.Equal(42, a);
        Assert.Equal("hello", b);
    }

    [Fact]
    public void Zip_FirstInvalid_ReturnsInvalid()
    {
        var v1 = Validation<int, string>.Invalid("error1");
        var v2 = Validation<string, string>.Valid("hello");

        var result = v1.Zip(v2);

        Assert.True(result.IsError);
    }

    [Fact]
    public void Zip_SecondInvalid_ReturnsInvalid()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<string, string>.Invalid("error2");

        var result = v1.Zip(v2);

        Assert.True(result.IsError);
    }

    [Fact]
    public void Zip_BothInvalid_AccumulatesErrors()
    {
        var v1 = Validation<int, string>.Invalid("error1");
        var v2 = Validation<string, string>.Invalid("error2");

        var result = v1.Zip(v2);

        Assert.True(result.IsError);
        var errors = result.GetErrorsOrThrow();
        Assert.Equal(2, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void ZipWith_BothValid_AppliesCombiner()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<int, string>.Valid(8);

        var result = v1.ZipWith(v2, (a, b) => a + b);

        Assert.True(result.IsOk);
        Assert.Equal(50, result.GetValue());
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_OnValid_ExecutesAction()
    {
        var validation = Validation<int, string>.Valid(42);
        var executed = false;

        var result = validation.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsOk);
    }

    [Fact]
    public void Tap_OnInvalid_DoesNotExecuteAction()
    {
        var validation = Validation<int, string>.Invalid("error");
        var executed = false;

        var result = validation.Tap(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void TapErrors_OnInvalid_ExecutesAction()
    {
        var validation = Validation<int, string>.Invalid("error");
        var executed = false;

        var result = validation.TapErrors(errors => executed = true);

        Assert.True(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void TapErrors_OnValid_DoesNotExecuteAction()
    {
        var validation = Validation<int, string>.Valid(42);
        var executed = false;

        var result = validation.TapErrors(errors => executed = true);

        Assert.False(executed);
        Assert.True(result.IsOk);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_OnValid_ExecutesValidFunc()
    {
        var validation = Validation<int, string>.Valid(42);
        var result = validation.Match(
            x => $"Value: {x}",
            errors => $"Errors: {errors.Length}");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_OnInvalid_ExecutesInvalidFunc()
    {
        var validation = Validation<int, string>.Invalid(new[] { "e1", "e2" });
        var result = validation.Match(
            x => $"Value: {x}",
            errors => $"Errors: {errors.Length}");

        Assert.Equal("Errors: 2", result);
    }

    [Fact]
    public void Match_WithActions_OnValid_ExecutesValidAction()
    {
        var validation = Validation<int, string>.Valid(42);
        var capturedValue = 0;

        validation.Match(
            x => capturedValue = x,
            errors => { });

        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public void Match_WithActions_OnInvalid_ExecutesInvalidAction()
    {
        var validation = Validation<int, string>.Invalid("error");
        var capturedCount = 0;

        validation.Match(
            x => { },
            errors => capturedCount = errors.Length);

        Assert.Equal(1, capturedCount);
    }

    #endregion

    #region Apply Tests

    [Fact]
    public void Apply_ValidBothValues_CombinesValues()
    {
        var first = Validation<int, string>.Valid(10);
        var second = Validation<int, string>.Valid(32);

        var result = first.Apply(second, (a, b) => a + b);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Apply_FirstInvalidSecondValid_ReturnsInvalid()
    {
        var first = Validation<int, string>.Invalid("first error");
        var second = Validation<int, string>.Valid(42);

        var result = first.Apply(second, (a, b) => a + b);

        Assert.True(result.IsError);
    }

    [Fact]
    public void Apply_FirstValidSecondInvalid_ReturnsInvalid()
    {
        var first = Validation<int, string>.Valid(10);
        var second = Validation<int, string>.Invalid("second error");

        var result = first.Apply(second, (a, b) => a + b);

        Assert.True(result.IsError);
    }

    [Fact]
    public void Apply_BothInvalid_AccumulatesErrors()
    {
        var first = Validation<int, string>.Invalid("first error");
        var second = Validation<int, string>.Invalid("second error");

        var result = first.Apply(second, (a, b) => a + b);

        Assert.True(result.IsError);
        var errors = result.GetErrors();
        Assert.Equal(2, errors.Length);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_ValidWithSameValue_ReturnsTrue()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<int, string>.Valid(42);

        Assert.True(v1.Equals(v2));
        Assert.True(v1 == v2);
    }

    [Fact]
    public void Equals_ValidWithDifferentValue_ReturnsFalse()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<int, string>.Valid(99);

        Assert.False(v1.Equals(v2));
        Assert.True(v1 != v2);
    }

    [Fact]
    public void Equals_InvalidWithSameErrors_ReturnsTrue()
    {
        var v1 = Validation<int, string>.Invalid("error");
        var v2 = Validation<int, string>.Invalid("error");

        Assert.True(v1.Equals(v2));
    }

    [Fact]
    public void Equals_ValidWithInvalid_ReturnsFalse()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<int, string>.Invalid("error");

        Assert.False(v1.Equals(v2));
    }

    [Fact]
    public void GetHashCode_SameForEqualValidations()
    {
        var v1 = Validation<int, string>.Valid(42);
        var v2 = Validation<int, string>.Valid(42);

        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnValid_ContainsValue()
    {
        var validation = Validation<int, string>.Valid(42);
        var str = validation.ToString();

        Assert.Contains("42", str);
        Assert.Contains("Valid", str);
    }

    [Fact]
    public void ToString_OnInvalid_ContainsInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        var str = validation.ToString();

        Assert.Contains("Invalid", str);
    }

    #endregion
}

