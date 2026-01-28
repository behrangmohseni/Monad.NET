using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for the improvement methods: Ensure, BiMap, Flatten, DefaultIfNone, ThrowIfNone/ThrowIfErr.
/// </summary>
public class ImprovementTests
{
    #region Validation.Ensure Tests

    [Fact]
    public void Ensure_ValidPasses_ReturnsOriginal()
    {
        var validation = Validation<int, string>.Valid(42);

        var result = validation.Ensure(x => x > 0, "Must be positive");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Ensure_ValidFails_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Valid(42);

        var result = validation.Ensure(x => x > 100, "Must be greater than 100");

        Assert.True(result.IsError);
        Assert.Equal("Must be greater than 100", result.GetErrors()[0]);
    }

    [Fact]
    public void Ensure_Invalid_ReturnsOriginalInvalid()
    {
        var validation = Validation<int, string>.Invalid("original error");

        var result = validation.Ensure(x => x > 0, "Must be positive");

        Assert.True(result.IsError);
        Assert.Equal("original error", result.GetErrors()[0]);
    }

    [Fact]
    public void Ensure_Chained_AllPass()
    {
        var result = Validation<int, string>.Valid(50)
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be less than 100")
            .Ensure(x => x % 2 == 0, "Must be even");

        Assert.True(result.IsOk);
        Assert.Equal(50, result.GetValue());
    }

    [Fact]
    public void Ensure_Chained_FirstFails()
    {
        var result = Validation<int, string>.Valid(-5)
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be less than 100");

        Assert.True(result.IsError);
        Assert.Equal("Must be positive", result.GetErrors()[0]);
    }

    [Fact]
    public void Ensure_NullPredicate_ThrowsArgumentNull()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.Throws<ArgumentNullException>(
            () => validation.Ensure(null!, "error"));
    }

    [Fact]
    public void Ensure_NullError_ThrowsArgumentNull()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.Throws<ArgumentNullException>(
            () => validation.Ensure(x => x > 0, (string)null!));
    }

    [Fact]
    public void Ensure_Factory_ValidPasses_ReturnsOriginal()
    {
        var validation = Validation<int, string>.Valid(42);
        var factoryCalled = false;

        var result = validation.Ensure(
            x => x > 0,
            () =>
            {
                factoryCalled = true;
                return "error";
            });

        Assert.True(result.IsOk);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void Ensure_Factory_ValidFails_CallsFactory()
    {
        var validation = Validation<int, string>.Valid(42);
        var factoryCalled = false;

        var result = validation.Ensure(
            x => x > 100,
            () =>
            {
                factoryCalled = true;
                return "Must be > 100";
            });

        Assert.True(result.IsError);
        Assert.True(factoryCalled);
        Assert.Equal("Must be > 100", result.GetErrors()[0]);
    }

    [Fact]
    public void Ensure_Factory_Invalid_DoesNotCallFactory()
    {
        var validation = Validation<int, string>.Invalid("original error");
        var factoryCalled = false;

        var result = validation.Ensure(
            x => x > 0,
            () =>
            {
                factoryCalled = true;
                return "error";
            });

        Assert.True(result.IsError);
        Assert.False(factoryCalled);
        Assert.Equal("original error", result.GetErrors()[0]);
    }

    #endregion

    #region Validation.Flatten Tests

    [Fact]
    public void Flatten_BothValid_ReturnsInnerValue()
    {
        var nested = Validation<Validation<int, string>, string>.Valid(
            Validation<int, string>.Valid(42));

        var result = nested.Flatten();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Flatten_OuterInvalid_ReturnsOuterErrors()
    {
        var nested = Validation<Validation<int, string>, string>.Invalid("outer error");

        var result = nested.Flatten();

        Assert.True(result.IsError);
        Assert.Equal("outer error", result.GetErrors()[0]);
    }

    [Fact]
    public void Flatten_InnerInvalid_ReturnsInnerErrors()
    {
        var nested = Validation<Validation<int, string>, string>.Valid(
            Validation<int, string>.Invalid("inner error"));

        var result = nested.Flatten();

        Assert.True(result.IsError);
        Assert.Equal("inner error", result.GetErrors()[0]);
    }

    [Fact]
    public void Flatten_MultipleNestedErrors()
    {
        var nested = Validation<Validation<int, string>, string>.Valid(
            Validation<int, string>.Invalid(new[] { "error1", "error2" }));

        var result = nested.Flatten();

        Assert.True(result.IsError);
        Assert.Equal(2, result.GetErrors().Length);
        Assert.Contains("error1", result.GetErrors());
        Assert.Contains("error2", result.GetErrors());
    }

    #endregion

    #region Result.BiMap Tests

    [Fact]
    public void BiMap_Ok_TransformsValue()
    {
        var result = Result<int, string>.Ok(42);

        var mapped = result.BiMap(
            x => x.ToString(),
            e => e.Length);

        Assert.True(mapped.IsOk);
        Assert.Equal("42", mapped.GetValue());
    }

    [Fact]
    public void BiMap_Err_TransformsError()
    {
        var result = Result<int, string>.Err("error");

        var mapped = result.BiMap(
            x => x.ToString(),
            e => e.Length);

        Assert.True(mapped.IsError);
        Assert.Equal(5, mapped.GetError());
    }

    [Fact]
    public void BiMap_NullOkMapper_ThrowsArgumentNull()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<ArgumentNullException>(
            () => result.BiMap<string, int>(null!, e => e.Length));
    }

    [Fact]
    public void BiMap_NullErrMapper_ThrowsArgumentNull()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<ArgumentNullException>(
            () => result.BiMap<string, int>(x => x.ToString(), null!));
    }

    [Fact]
    public void BiMap_EquivalentToSeparateMaps_Ok()
    {
        var result = Result<int, string>.Ok(42);

        var biMapped = result.BiMap(x => x.ToString(), e => e.Length);
        var separateMapped = result.Map(x => x.ToString()).MapError(e => e.Length);

        Assert.Equal(biMapped.IsOk, separateMapped.IsOk);
        Assert.Equal(biMapped.GetValue(), separateMapped.GetValue());
    }

    [Fact]
    public void BiMap_EquivalentToSeparateMaps_Err()
    {
        var result = Result<int, string>.Err("error");

        var biMapped = result.BiMap(x => x.ToString(), e => e.Length);
        var separateMapped = result.Map(x => x.ToString()).MapError(e => e.Length);

        Assert.Equal(biMapped.IsError, separateMapped.IsError);
        Assert.Equal(biMapped.GetError(), separateMapped.GetError());
    }

    #endregion

    #region Option.DefaultIfNone Tests

    [Fact]
    public void DefaultIfNone_Some_ReturnsOriginal()
    {
        var option = Option<int>.Some(42);

        var result = option.DefaultIfNone(0);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void DefaultIfNone_None_ReturnsDefault()
    {
        var option = Option<int>.None();

        var result = option.DefaultIfNone(99);

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void DefaultIfNone_Factory_Some_DoesNotCallFactory()
    {
        var option = Option<int>.Some(42);
        var factoryCalled = false;

        var result = option.DefaultIfNone(() =>
        {
            factoryCalled = true;
            return 0;
        });

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
        Assert.False(factoryCalled);
    }

    [Fact]
    public void DefaultIfNone_Factory_None_CallsFactory()
    {
        var option = Option<int>.None();
        var factoryCalled = false;

        var result = option.DefaultIfNone(() =>
        {
            factoryCalled = true;
            return 99;
        });

        Assert.True(result.IsSome);
        Assert.Equal(99, result.GetValue());
        Assert.True(factoryCalled);
    }

    [Fact]
    public void DefaultIfNone_Factory_NullFactory_ThrowsArgumentNull()
    {
        var option = Option<int>.None();

        Assert.Throws<ArgumentNullException>(
            () => option.DefaultIfNone((Func<int>)null!));
    }

    [Fact]
    public void DefaultIfNone_DifferentFromUnwrapOr()
    {
        var option = Option<int>.None();

        // DefaultIfNone returns Option<T>
        var resultOption = option.DefaultIfNone(42);
        Assert.IsType<Option<int>>(resultOption);

        // UnwrapOr returns T directly
        var resultValue = option.GetValueOr(42);
        Assert.Equal(42, resultValue);
    }

    #endregion

    #region Option.ThrowIfNone Tests

    [Fact]
    public void ThrowIfNone_Some_ReturnsValue()
    {
        var option = Option<int>.Some(42);

        var result = option.ThrowIfNone(new InvalidOperationException("No value"));

        Assert.Equal(42, result);
    }

    [Fact]
    public void ThrowIfNone_None_ThrowsException()
    {
        var option = Option<int>.None();
        var expectedException = new InvalidOperationException("No value");

        var thrownException = Assert.Throws<InvalidOperationException>(
            () => option.ThrowIfNone(expectedException));

        Assert.Same(expectedException, thrownException);
    }

    [Fact]
    public void ThrowIfNone_NullException_ThrowsArgumentNull()
    {
        var option = Option<int>.Some(42);

        Assert.Throws<ArgumentNullException>(
            () => option.ThrowIfNone((Exception)null!));
    }

    [Fact]
    public void ThrowIfNone_Factory_Some_DoesNotCallFactory()
    {
        var option = Option<int>.Some(42);
        var factoryCalled = false;

        var result = option.ThrowIfNone(() =>
        {
            factoryCalled = true;
            return new InvalidOperationException("No value");
        });

        Assert.Equal(42, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void ThrowIfNone_Factory_None_CallsFactory()
    {
        var option = Option<int>.None();
        var factoryCalled = false;

        Assert.Throws<InvalidOperationException>(
            () => option.ThrowIfNone(() =>
            {
                factoryCalled = true;
                return new InvalidOperationException("No value");
            }));

        Assert.True(factoryCalled);
    }

    [Fact]
    public void ThrowIfNone_Factory_NullFactory_ThrowsArgumentNull()
    {
        var option = Option<int>.Some(42);

        Assert.Throws<ArgumentNullException>(
            () => option.ThrowIfNone((Func<Exception>)null!));
    }

    [Fact]
    public void ThrowIfNone_CustomExceptionType()
    {
        var option = Option<int>.None();

        Assert.Throws<KeyNotFoundException>(
            () => option.ThrowIfNone(new KeyNotFoundException("Key not found")));
    }

    #endregion

    #region Result.ThrowIfErr Tests

    [Fact]
    public void ThrowIfErr_Ok_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);

        var value = result.ThrowIfErr(new InvalidOperationException("Error occurred"));

        Assert.Equal(42, value);
    }

    [Fact]
    public void ThrowIfErr_Err_ThrowsException()
    {
        var result = Result<int, string>.Err("error");
        var expectedException = new InvalidOperationException("Error occurred");

        var thrownException = Assert.Throws<InvalidOperationException>(
            () => result.ThrowIfErr(expectedException));

        Assert.Same(expectedException, thrownException);
    }

    [Fact]
    public void ThrowIfErr_NullException_ThrowsArgumentNull()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<ArgumentNullException>(
            () => result.ThrowIfErr((Exception)null!));
    }

    [Fact]
    public void ThrowIfErr_Factory_Ok_DoesNotCallFactory()
    {
        var result = Result<int, string>.Ok(42);
        var factoryCalled = false;

        var value = result.ThrowIfErr(err =>
        {
            factoryCalled = true;
            return new InvalidOperationException(err);
        });

        Assert.Equal(42, value);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void ThrowIfErr_Factory_Err_CallsFactoryWithError()
    {
        var result = Result<int, string>.Err("the error message");
        string? capturedError = null;

        Assert.Throws<InvalidOperationException>(
            () => result.ThrowIfErr(err =>
            {
                capturedError = err;
                return new InvalidOperationException(err);
            }));

        Assert.Equal("the error message", capturedError);
    }

    [Fact]
    public void ThrowIfErr_Factory_NullFactory_ThrowsArgumentNull()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<ArgumentNullException>(
            () => result.ThrowIfErr((Func<string, Exception>)null!));
    }

    [Fact]
    public void ThrowIfErr_CustomExceptionType()
    {
        var result = Result<int, string>.Err("not found");

        Assert.Throws<FileNotFoundException>(
            () => result.ThrowIfErr(new FileNotFoundException("File not found")));
    }

    [Fact]
    public void ThrowIfErr_Factory_CreatesExceptionFromError()
    {
        var result = Result<int, int>.Err(404);

        var ex = Assert.Throws<HttpRequestException>(
            () => result.ThrowIfErr(code => new HttpRequestException($"HTTP {code} error")));

        Assert.Equal("HTTP 404 error", ex.Message);
    }

    #endregion
}
