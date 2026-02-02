using Monad.NET;

namespace Monad.NET.Tests;

public class ValidationTests
{
    [Fact]
    public void Valid_CreatesValidValidation()
    {
        var validation = Validation<int, string>.Ok(42);

        Assert.True(validation.IsOk);
        Assert.False(validation.IsError);
        Assert.Equal(42, validation.GetValue());
    }

    [Fact]
    public void Invalid_SingleError_CreatesInvalidValidation()
    {
        var validation = Validation<int, string>.Error("error");

        Assert.False(validation.IsOk);
        Assert.True(validation.IsError);
        Assert.Single(validation.GetErrors());
        Assert.Equal("error", validation.GetErrors()[0]);
    }

    [Fact]
    public void Invalid_MultipleErrors_CreatesInvalidValidation()
    {
        var errors = new[] { "error1", "error2", "error3" };
        var validation = Validation<int, string>.Error(errors);

        Assert.False(validation.IsOk);
        Assert.Equal(3, validation.GetErrors().Length);
        Assert.Equal(errors, validation.GetErrors());
    }

    [Fact]
    public void Valid_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Validation<string, int>.Ok(null!));
    }

    [Fact]
    public void Invalid_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Validation<int, string>.Error((string)null!));
    }

    [Fact]
    public void Unwrap_OnValid_ReturnsValue()
    {
        var validation = Validation<int, string>.Ok(42);
        Assert.Equal(42, validation.GetValue());
    }

    [Fact]
    public void Unwrap_OnInvalid_ThrowsException()
    {
        var validation = Validation<int, string>.Error("error");
        Assert.Throws<InvalidOperationException>(() => validation.GetValue());
    }

    [Fact]
    public void UnwrapErrors_OnInvalid_ReturnsErrors()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        var errors = validation.GetErrors();

        Assert.Equal(2, errors.Length);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void UnwrapErrors_OnValid_ThrowsException()
    {
        var validation = Validation<int, string>.Ok(42);
        Assert.Throws<InvalidOperationException>(() => validation.GetErrors());
    }

    [Fact]
    public void Map_OnValid_TransformsValue()
    {
        var validation = Validation<int, string>.Ok(42);
        var mapped = validation.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void Map_OnInvalid_PreservesErrors()
    {
        var validation = Validation<int, string>.Error("error");
        var mapped = validation.Map(x => x * 2);

        Assert.True(mapped.IsError);
        Assert.Equal("error", mapped.GetErrors()[0]);
    }

    [Fact]
    public void TryGet_OnValid_ReturnsTrueAndValue()
    {
        var validation = Validation<int, string>.Ok(42);

        var result = validation.TryGet(out var value);

        Assert.True(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGet_OnInvalid_ReturnsFalse()
    {
        var validation = Validation<int, string>.Error("error");

        var result = validation.TryGet(out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetErrors_OnInvalid_ReturnsTrueAndErrors()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });

        var result = validation.TryGetErrors(out var errors);

        Assert.True(result);
        Assert.Equal(2, errors.Length);
        Assert.Equal("error1", errors[0]);
        Assert.Equal("error2", errors[1]);
    }

    [Fact]
    public void TryGetErrors_OnValid_ReturnsFalseAndEmptyList()
    {
        var validation = Validation<int, string>.Ok(42);

        var result = validation.TryGetErrors(out var errors);

        Assert.False(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void MapErrors_OnInvalid_TransformsErrors()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        var mapped = validation.MapErrors(e => e.ToUpper());

        Assert.True(mapped.IsError);
        Assert.Equal(new[] { "ERROR1", "ERROR2" }, mapped.GetErrors());
    }

    [Fact]
    public void Apply_BothValid_CombinesValues()
    {
        var val1 = Validation<int, string>.Ok(10);
        var val2 = Validation<int, string>.Ok(20);

        var result = val1.Apply(val2, (a, b) => a + b);

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Apply_BothInvalid_AccumulatesErrors()
    {
        var val1 = Validation<int, string>.Error("error1");
        var val2 = Validation<int, string>.Error("error2");

        var result = val1.Apply(val2, (a, b) => a + b);

        Assert.True(result.IsError);
        Assert.Equal(2, result.GetErrors().Length);
        Assert.Contains("error1", result.GetErrors());
        Assert.Contains("error2", result.GetErrors());
    }

    [Fact]
    public void Apply_FirstInvalid_ReturnsFirstErrors()
    {
        var val1 = Validation<int, string>.Error("error1");
        var val2 = Validation<int, string>.Ok(20);

        var result = val1.Apply(val2, (a, b) => a + b);

        Assert.True(result.IsError);
        Assert.Single(result.GetErrors());
        Assert.Equal("error1", result.GetErrors()[0]);
    }

    [Fact]
    public void Zip_BothValid_ReturnsTuple()
    {
        var val1 = Validation<int, string>.Ok(42);
        var val2 = Validation<string, string>.Ok("hello");

        var result = val1.Zip(val2);

        Assert.True(result.IsOk);
        Assert.Equal((42, "hello"), result.GetValue());
    }

    [Fact]
    public void Zip_BothInvalid_AccumulatesAllErrors()
    {
        var val1 = Validation<int, string>.Error(new[] { "error1", "error2" });
        var val2 = Validation<string, string>.Error(new[] { "error3" });

        var result = val1.Zip(val2);

        Assert.True(result.IsError);
        Assert.Equal(3, result.GetErrors().Length);
        Assert.Contains("error1", result.GetErrors());
        Assert.Contains("error2", result.GetErrors());
        Assert.Contains("error3", result.GetErrors());
    }

    [Fact]
    public void Zip_FirstInvalid_ReturnsFirstErrors()
    {
        var val1 = Validation<int, string>.Error("error1");
        var val2 = Validation<string, string>.Ok("hello");

        var result = val1.Zip(val2);

        Assert.True(result.IsError);
        Assert.Single(result.GetErrors());
        Assert.Equal("error1", result.GetErrors()[0]);
    }

    [Fact]
    public void Zip_SecondInvalid_ReturnsSecondErrors()
    {
        var val1 = Validation<int, string>.Ok(42);
        var val2 = Validation<string, string>.Error("error2");

        var result = val1.Zip(val2);

        Assert.True(result.IsError);
        Assert.Single(result.GetErrors());
        Assert.Equal("error2", result.GetErrors()[0]);
    }

    [Fact]
    public void ZipWith_BothValid_ReturnsCombinedValue()
    {
        var val1 = Validation<int, string>.Ok(10);
        var val2 = Validation<int, string>.Ok(20);

        var result = val1.ZipWith(val2, (a, b) => a + b);

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void ZipWith_BothInvalid_AccumulatesAllErrors()
    {
        var val1 = Validation<int, string>.Error("error1");
        var val2 = Validation<int, string>.Error("error2");

        var result = val1.ZipWith(val2, (a, b) => a + b);

        Assert.True(result.IsError);
        Assert.Equal(2, result.GetErrors().Length);
        Assert.Contains("error1", result.GetErrors());
        Assert.Contains("error2", result.GetErrors());
    }

    [Fact]
    public void And_BothValid_ReturnsSecond()
    {
        var val1 = Validation<int, string>.Ok(10);
        var val2 = Validation<int, string>.Ok(20);

        var result = val1.And(val2);

        Assert.True(result.IsOk);
        Assert.Equal(20, result.GetValue());
    }

    [Fact]
    public void And_BothInvalid_AccumulatesAllErrors()
    {
        var val1 = Validation<int, string>.Error(new[] { "error1", "error2" });
        var val2 = Validation<int, string>.Error(new[] { "error3", "error4" });

        var result = val1.And(val2);

        Assert.True(result.IsError);
        Assert.Equal(4, result.GetErrors().Length);
        Assert.Equal(new[] { "error1", "error2", "error3", "error4" }, result.GetErrors());
    }

    [Fact]
    public void AndThen_OnValid_ExecutesFunction()
    {
        var validation = Validation<int, string>.Ok(42);
        var result = validation.Bind(x =>
            x > 40
                ? Validation<string, string>.Ok("large")
                : Validation<string, string>.Error("small"));

        Assert.True(result.IsOk);
        Assert.Equal("large", result.GetValue());
    }

    [Fact]
    public void AndThen_OnInvalid_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Error("error");
        var result = validation.Bind(x => Validation<string, string>.Ok(x.ToString()));

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetErrors()[0]);
    }

    [Fact]
    public void Match_OnValid_ExecutesValidAction()
    {
        var validation = Validation<int, string>.Ok(42);
        var value = 0;

        validation.Match(
            validAction: x => value = x,
            invalidAction: errors => value = -1
        );

        Assert.Equal(42, value);
    }

    [Fact]
    public void Match_OnInvalid_ExecutesInvalidAction()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        var errorCount = 0;

        validation.Match(
            validAction: x => errorCount = 0,
            invalidAction: errors => errorCount = errors.Length
        );

        Assert.Equal(2, errorCount);
    }

    [Fact]
    public void Match_WithReturn_OnValid_ReturnsValidValue()
    {
        var validation = Validation<int, string>.Ok(42);
        var result = validation.Match(
            validFunc: x => x.ToString(),
            invalidFunc: errors => string.Join(", ", errors)
        );

        Assert.Equal("42", result);
    }

    [Fact]
    public void Match_WithReturn_OnInvalid_ReturnsInvalidValue()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        var result = validation.Match(
            validFunc: x => x.ToString(),
            invalidFunc: errors => string.Join(", ", errors)
        );

        Assert.Equal("error1, error2", result);
    }

    [Fact]
    public void ToResult_OnValid_ReturnsOk()
    {
        var validation = Validation<int, string>.Ok(42);
        var result = validation.ToResult();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_OnInvalid_ReturnsErrWithFirstError()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        var result = validation.ToResult();

        Assert.True(result.IsError);
        Assert.Equal("error1", result.GetError());
    }

    [Fact]
    public void ToResult_WithCombiner_CombinesErrors()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        var result = validation.ToResult(errors => string.Join("; ", errors));

        Assert.True(result.IsError);
        Assert.Equal("error1; error2", result.GetError());
    }

    [Fact]
    public void ToOption_OnValid_ReturnsSome()
    {
        var validation = Validation<int, string>.Ok(42);
        var option = validation.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void ToOption_OnInvalid_ReturnsNone()
    {
        var validation = Validation<int, string>.Error("error");
        var option = validation.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void Combine_AllValid_ReturnsValidWithLastValue()
    {
        var validations = new[]
        {
            Validation<int, string>.Ok(10),
            Validation<int, string>.Ok(20),
            Validation<int, string>.Ok(30)
        };

        var combined = validations.Combine();

        Assert.True(combined.IsOk);
        Assert.Equal(30, combined.GetValue());
    }

    [Fact]
    public void Combine_SomeInvalid_AccumulatesAllErrors()
    {
        var validations = new[]
        {
            Validation<int, string>.Ok(10),
            Validation<int, string>.Error("error1"),
            Validation<int, string>.Error(new[] { "error2", "error3" })
        };

        var combined = validations.Combine();

        Assert.True(combined.IsError);
        Assert.Equal(3, combined.GetErrors().Length);
        Assert.Equal(new[] { "error1", "error2", "error3" }, combined.GetErrors());
    }

    [Fact]
    public void Tap_OnValid_ExecutesAction()
    {
        var validation = Validation<int, string>.Ok(42);
        var executed = false;

        var result = validation.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsOk);
    }

    [Fact]
    public void TapErrors_OnInvalid_ExecutesAction()
    {
        var validation = Validation<int, string>.Error("error");
        var executed = false;

        var result = validation.TapErrors(errors => executed = true);

        Assert.True(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void TapInvalid_OnInvalid_ExecutesAction()
    {
        var validation = Validation<int, string>.Error("error");
        var executed = false;

        var result = validation.TapErrors(errors => executed = true);

        Assert.True(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void TapInvalid_OnValid_DoesNotExecuteAction()
    {
        var validation = Validation<int, string>.Ok(42);
        var executed = false;

        var result = validation.TapErrors(errors => executed = true);

        Assert.False(executed);
        Assert.True(result.IsOk);
    }

    [Fact]
    public void ToValidation_FromOk_ReturnsValid()
    {
        var result = Result<int, string>.Ok(42);
        var validation = result.ToValidation();

        Assert.True(validation.IsOk);
        Assert.Equal(42, validation.GetValue());
    }

    [Fact]
    public void ToValidation_FromErr_ReturnsInvalid()
    {
        var result = Result<int, string>.Error("error");
        var validation = result.ToValidation();

        Assert.True(validation.IsError);
        Assert.Single(validation.GetErrors());
        Assert.Equal("error", validation.GetErrors()[0]);
    }

    [Fact]
    public void RealWorld_FormValidation_AccumulatesAllErrors()
    {
        // Simulate form validation where all return bool instead
        var nameValid = ValidateName2("");
        var emailValid = ValidateEmail2("invalid");
        var ageValid = ValidateAge2(15);

        // Manually accumulate errors
        var errors = new List<string>();
        if (nameValid.IsError)
            errors.AddRange(nameValid.GetErrors());
        if (emailValid.IsError)
            errors.AddRange(emailValid.GetErrors());
        if (ageValid.IsError)
            errors.AddRange(ageValid.GetErrors());

        Assert.Equal(3, errors.Count);
        Assert.Contains("Name is required", errors);
        Assert.Contains("Invalid email format", errors);
        Assert.Contains("Must be 18 or older", errors);
    }

    [Fact]
    public void RealWorld_FormValidation_AllValid()
    {
        var nameValidation = ValidateName2("John");
        var emailValidation = ValidateEmail2("john@example.com");
        var ageValidation = ValidateAge2(25);

        var errors = new List<string>();
        if (nameValidation.IsError)
            errors.AddRange(nameValidation.GetErrors());
        if (emailValidation.IsError)
            errors.AddRange(emailValidation.GetErrors());
        if (ageValidation.IsError)
            errors.AddRange(ageValidation.GetErrors());

        Assert.Empty(errors);
    }

    // Helper validation methods
    private static Validation<string, string> ValidateName2(string name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? Validation<string, string>.Error("Name is required")
            : Validation<string, string>.Ok(name);
    }

    private static Validation<string, string> ValidateEmail2(string email)
    {
        return email.Contains('@')
            ? Validation<string, string>.Ok(email)
            : Validation<string, string>.Error("Invalid email format");
    }

    private static Validation<int, string> ValidateAge2(int age)
    {
        return age >= 18
            ? Validation<int, string>.Ok(age)
            : Validation<int, string>.Error("Must be 18 or older");
    }

    [Fact]
    public void Equality_TwoValidsWithSameValue_AreEqual()
    {
        var val1 = Validation<int, string>.Ok(42);
        var val2 = Validation<int, string>.Ok(42);

        Assert.Equal(val1, val2);
        Assert.True(val1 == val2);
    }

    [Fact]
    public void Equality_TwoInvalidsWithSameErrors_AreEqual()
    {
        var val1 = Validation<int, string>.Error(new[] { "error1", "error2" });
        var val2 = Validation<int, string>.Error(new[] { "error1", "error2" });

        Assert.Equal(val1, val2);
        Assert.True(val1 == val2);
    }

    [Fact]
    public void ToString_OnValid_ReturnsFormattedString()
    {
        var validation = Validation<int, string>.Ok(42);
        Assert.Equal("Valid(42)", validation.ToString());
    }

    [Fact]
    public void ToString_OnInvalid_ReturnsFormattedString()
    {
        var validation = Validation<int, string>.Error(new[] { "error1", "error2" });
        Assert.Equal("Invalid([error1, error2])", validation.ToString());
    }
}

