using System.Collections.Immutable;
using Xunit;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for the cross-type API consistency aliases.
/// These aliases ensure that the same conceptual operation uses the same name
/// regardless of which monad type you're working with.
/// </summary>
public class ConsistentApiAliasTests
{
    #region Option.ToResult (alias for OkOr/OkOrElse)

    [Fact]
    public void Option_ToResult_Some_ReturnsOk()
    {
        var option = Option<int>.Some(42);
        var result = option.ToResult("error");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Option_ToResult_None_ReturnsError()
    {
        var option = Option<int>.None();
        var result = option.ToResult("missing");

        Assert.True(result.IsError);
        Assert.Equal("missing", result.GetError());
    }

    [Fact]
    public void Option_ToResult_MatchesOkOr()
    {
        var some = Option<int>.Some(42);
        var none = Option<int>.None();

        Assert.Equal(some.OkOr("err"), some.ToResult("err"));
        Assert.Equal(none.OkOr("err"), none.ToResult("err"));
    }

    [Fact]
    public void Option_ToResultFactory_Some_ReturnsOk()
    {
        var option = Option<int>.Some(42);
        var result = option.ToResult(() => "error");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Option_ToResultFactory_None_ReturnsError()
    {
        var option = Option<int>.None();
        var result = option.ToResult(() => "computed error");

        Assert.True(result.IsError);
        Assert.Equal("computed error", result.GetError());
    }

    [Fact]
    public void Option_ToResultFactory_MatchesOkOrElse()
    {
        var some = Option<int>.Some(42);
        var none = Option<int>.None();
        Func<string> factory = () => "err";

        Assert.Equal(some.OkOrElse(factory), some.ToResult(factory));
        Assert.Equal(none.OkOrElse(factory), none.ToResult(factory));
    }

    #endregion

    #region Try.GetError / GetErrorOrThrow / TryGetError (aliases for GetException variants)

    [Fact]
    public void Try_GetError_Failure_ReturnsException()
    {
        var ex = new InvalidOperationException("test");
        var tryResult = Try<int>.Error(ex);

        Assert.Same(ex, tryResult.GetError());
    }

    [Fact]
    public void Try_GetError_Success_Throws()
    {
        var tryResult = Try<int>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => tryResult.GetError());
    }

    [Fact]
    public void Try_GetError_MatchesGetException()
    {
        var ex = new InvalidOperationException("test");
        var tryResult = Try<int>.Error(ex);

        Assert.Same(tryResult.GetException(), tryResult.GetError());
    }

    [Fact]
    public void Try_GetErrorOrThrow_Failure_ReturnsException()
    {
        var ex = new InvalidOperationException("test");
        var tryResult = Try<int>.Error(ex);

        Assert.Same(ex, tryResult.GetErrorOrThrow());
    }

    [Fact]
    public void Try_GetErrorOrThrow_Success_Throws()
    {
        var tryResult = Try<int>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => tryResult.GetErrorOrThrow());
    }

    [Fact]
    public void Try_GetErrorOrThrow_MatchesGetExceptionOrThrow()
    {
        var ex = new InvalidOperationException("test");
        var tryResult = Try<int>.Error(ex);

        Assert.Same(tryResult.GetExceptionOrThrow(), tryResult.GetErrorOrThrow());
    }

    [Fact]
    public void Try_TryGetError_Failure_ReturnsTrueAndException()
    {
        var ex = new InvalidOperationException("test");
        var tryResult = Try<int>.Error(ex);

        Assert.True(tryResult.TryGetError(out var error));
        Assert.Same(ex, error);
    }

    [Fact]
    public void Try_TryGetError_Success_ReturnsFalse()
    {
        var tryResult = Try<int>.Ok(42);

        Assert.False(tryResult.TryGetError(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void Try_TryGetError_MatchesTryGetException()
    {
        var ex = new InvalidOperationException("test");
        var tryResult = Try<int>.Error(ex);

        tryResult.TryGetException(out var ex1);
        tryResult.TryGetError(out var ex2);

        Assert.Same(ex1, ex2);
    }

    #endregion

    #region Result.Ensure (alias for FilterOrElse)

    [Fact]
    public void Result_Ensure_Ok_PredicatePasses_ReturnsOk()
    {
        var result = Result<int, string>.Ok(42);
        var ensured = result.Ensure(x => x > 0, "must be positive");

        Assert.True(ensured.IsOk);
        Assert.Equal(42, ensured.GetValue());
    }

    [Fact]
    public void Result_Ensure_Ok_PredicateFails_ReturnsError()
    {
        var result = Result<int, string>.Ok(-1);
        var ensured = result.Ensure(x => x > 0, "must be positive");

        Assert.True(ensured.IsError);
        Assert.Equal("must be positive", ensured.GetError());
    }

    [Fact]
    public void Result_Ensure_Error_PreservesError()
    {
        var result = Result<int, string>.Error("original");
        var ensured = result.Ensure(x => x > 0, "must be positive");

        Assert.True(ensured.IsError);
        Assert.Equal("original", ensured.GetError());
    }

    [Fact]
    public void Result_Ensure_MatchesFilterOrElse()
    {
        var ok = Result<int, string>.Ok(42);
        var err = Result<int, string>.Error("oops");

        Assert.Equal(ok.FilterOrElse(x => x > 0, "err"), ok.Ensure(x => x > 0, "err"));
        Assert.Equal(ok.FilterOrElse(x => x > 100, "err"), ok.Ensure(x => x > 100, "err"));
        Assert.Equal(err.FilterOrElse(x => x > 0, "err"), err.Ensure(x => x > 0, "err"));
    }

    [Fact]
    public void Result_EnsureWithFactory_MatchesFilterOrElse()
    {
        var ok = Result<int, string>.Ok(42);
        Func<string> factory = () => "err";

        Assert.Equal(ok.FilterOrElse(x => x > 0, factory), ok.Ensure(x => x > 0, factory));
        Assert.Equal(ok.FilterOrElse(x => x > 100, factory), ok.Ensure(x => x > 100, factory));
    }

    [Fact]
    public void Result_EnsureWithValueFactory_MatchesFilterOrElse()
    {
        var ok = Result<int, string>.Ok(42);
        Func<int, string> factory = x => $"value {x} is invalid";

        Assert.Equal(ok.FilterOrElse(x => x > 0, factory), ok.Ensure(x => x > 0, factory));
        Assert.Equal(ok.FilterOrElse(x => x > 100, factory), ok.Ensure(x => x > 100, factory));
    }

    #endregion

    #region Result.ToErrorOption (alias for Err)

    [Fact]
    public void Result_ToErrorOption_Error_ReturnsSomeWithError()
    {
        var result = Result<int, string>.Error("oops");
        var errorOption = result.ToErrorOption();

        Assert.True(errorOption.IsSome);
        Assert.Equal("oops", errorOption.GetValue());
    }

    [Fact]
    public void Result_ToErrorOption_Ok_ReturnsNone()
    {
        var result = Result<int, string>.Ok(42);
        var errorOption = result.ToErrorOption();

        Assert.True(errorOption.IsNone);
    }

    [Fact]
    public void Result_ToErrorOption_MatchesErr()
    {
        var ok = Result<int, string>.Ok(42);
        var err = Result<int, string>.Error("oops");

        Assert.Equal(ok.Err(), ok.ToErrorOption());
        Assert.Equal(err.Err(), err.ToErrorOption());
    }

    #endregion

    #region Validation.MapError (alias for MapErrors)

    [Fact]
    public void Validation_MapError_Valid_ReturnsValid()
    {
        var validation = Validation<int, string>.Ok(42);
        var mapped = validation.MapError(e => e.Length);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public void Validation_MapError_Invalid_MapsEachError()
    {
        var validation = Validation<int, string>.Error(new[] { "err1", "err2" });
        var mapped = validation.MapError(e => e.ToUpper());

        Assert.True(mapped.IsError);
        var errors = mapped.GetErrors();
        Assert.Equal(2, errors.Length);
        Assert.Equal("ERR1", errors[0]);
        Assert.Equal("ERR2", errors[1]);
    }

    [Fact]
    public void Validation_MapError_MatchesMapErrors()
    {
        var validation = Validation<int, string>.Error(new[] { "err1", "err2" });

        var mapped1 = validation.MapErrors(e => e.Length);
        var mapped2 = validation.MapError(e => e.Length);

        Assert.Equal(mapped1, mapped2);
    }

    #endregion

    #region Validation.FilterOrElse (alias for Ensure)

    [Fact]
    public void Validation_FilterOrElse_Valid_PredicatePasses_ReturnsValid()
    {
        var validation = Validation<int, string>.Ok(42);
        var filtered = validation.FilterOrElse(x => x > 0, "must be positive");

        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void Validation_FilterOrElse_Valid_PredicateFails_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Ok(-1);
        var filtered = validation.FilterOrElse(x => x > 0, "must be positive");

        Assert.True(filtered.IsError);
        Assert.Contains("must be positive", (IEnumerable<string>)filtered.GetErrors());
    }

    [Fact]
    public void Validation_FilterOrElse_MatchesEnsure()
    {
        var ok = Validation<int, string>.Ok(42);

        Assert.Equal(ok.Ensure(x => x > 0, "err"), ok.FilterOrElse(x => x > 0, "err"));
        Assert.Equal(ok.Ensure(x => x > 100, "err"), ok.FilterOrElse(x => x > 100, "err"));
    }

    [Fact]
    public void Validation_FilterOrElseFactory_MatchesEnsure()
    {
        var ok = Validation<int, string>.Ok(42);
        Func<string> factory = () => "err";

        Assert.Equal(ok.Ensure(x => x > 0, factory), ok.FilterOrElse(x => x > 0, factory));
        Assert.Equal(ok.Ensure(x => x > 100, factory), ok.FilterOrElse(x => x > 100, factory));
    }

    #endregion

    #region Validation.TapError (alias for TapErrors)

    [Fact]
    public void Validation_TapError_Invalid_ExecutesAction()
    {
        var validation = Validation<int, string>.Error(new[] { "err1", "err2" });
        ImmutableArray<string>? captured = null;

        validation.TapError(errors => captured = errors);

        Assert.NotNull(captured);
        Assert.Equal(2, captured.Value.Length);
    }

    [Fact]
    public void Validation_TapError_Valid_DoesNotExecuteAction()
    {
        var validation = Validation<int, string>.Ok(42);
        var executed = false;

        validation.TapError(_ => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void Validation_TapError_MatchesTapErrors()
    {
        var validation = Validation<int, string>.Error("err1");
        ImmutableArray<string>? captured1 = null;
        ImmutableArray<string>? captured2 = null;

        validation.TapErrors(errors => captured1 = errors);
        validation.TapError(errors => captured2 = errors);

        Assert.Equal(captured1, captured2);
    }

    #endregion
}
