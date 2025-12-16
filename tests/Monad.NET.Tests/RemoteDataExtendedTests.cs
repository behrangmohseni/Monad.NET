using Xunit;

namespace Monad.NET.Tests;

public class RemoteDataExtendedTests
{
    #region RemoteData Core Coverage

    [Fact]
    public void NotAsked_CreatesNotAskedState()
    {
        var data = RemoteData<int, string>.NotAsked();

        Assert.True(data.IsNotAsked);
        Assert.False(data.IsLoading);
        Assert.False(data.IsSuccess);
        Assert.False(data.IsFailure);
    }

    [Fact]
    public void Loading_CreatesLoadingState()
    {
        var data = RemoteData<int, string>.Loading();

        Assert.False(data.IsNotAsked);
        Assert.True(data.IsLoading);
        Assert.False(data.IsSuccess);
        Assert.False(data.IsFailure);
    }

    [Fact]
    public void Success_CreatesSuccessState()
    {
        var data = RemoteData<int, string>.Success(42);

        Assert.False(data.IsNotAsked);
        Assert.False(data.IsLoading);
        Assert.True(data.IsSuccess);
        Assert.False(data.IsFailure);
        Assert.Equal(42, data.Unwrap());
    }

    [Fact]
    public void Failure_CreatesFailureState()
    {
        var data = RemoteData<int, string>.Failure("error");

        Assert.False(data.IsNotAsked);
        Assert.False(data.IsLoading);
        Assert.False(data.IsSuccess);
        Assert.True(data.IsFailure);
        Assert.Equal("error", data.UnwrapError());
    }

    [Fact]
    public void Success_ThrowsOnNullValue()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RemoteData<string, string>.Success(null!));
    }

    [Fact]
    public void Failure_ThrowsOnNullError()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RemoteData<int, string>.Failure(null!));
    }

    [Fact]
    public void Unwrap_ThrowsWhenNotSuccess()
    {
        var data = RemoteData<int, string>.Loading();

        Assert.Throws<InvalidOperationException>(() => data.Unwrap());
    }

    [Fact]
    public void UnwrapError_ThrowsWhenNotFailure()
    {
        var data = RemoteData<int, string>.Success(42);

        Assert.Throws<InvalidOperationException>(() => data.UnwrapError());
    }

    [Fact]
    public void UnwrapOr_ReturnsValueOnSuccess()
    {
        var data = RemoteData<int, string>.Success(42);

        Assert.Equal(42, data.UnwrapOr(0));
    }

    [Fact]
    public void UnwrapOr_ReturnsDefaultOnOtherStates()
    {
        Assert.Equal(0, RemoteData<int, string>.NotAsked().UnwrapOr(0));
        Assert.Equal(0, RemoteData<int, string>.Loading().UnwrapOr(0));
        Assert.Equal(0, RemoteData<int, string>.Failure("err").UnwrapOr(0));
    }

    [Fact]
    public void Map_TransformsSuccessValue()
    {
        var data = RemoteData<int, string>.Success(10);

        var mapped = data.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(20, mapped.Unwrap());
    }

    [Fact]
    public void Map_PreservesOtherStates()
    {
        Assert.True(RemoteData<int, string>.NotAsked().Map(x => x * 2).IsNotAsked);
        Assert.True(RemoteData<int, string>.Loading().Map(x => x * 2).IsLoading);
        Assert.True(RemoteData<int, string>.Failure("err").Map(x => x * 2).IsFailure);
    }

    [Fact]
    public void Map_ThrowsOnNullMapper()
    {
        var data = RemoteData<int, string>.Success(42);

        Assert.Throws<ArgumentNullException>(() => data.Map<int>(null!));
    }

    [Fact]
    public void MapError_TransformsFailureError()
    {
        var data = RemoteData<int, string>.Failure("error");

        var mapped = data.MapError(e => e.Length);

        Assert.True(mapped.IsFailure);
        Assert.Equal(5, mapped.UnwrapError());
    }

    [Fact]
    public void MapError_PreservesOtherStates()
    {
        Assert.True(RemoteData<int, string>.NotAsked().MapError(e => e.Length).IsNotAsked);
        Assert.True(RemoteData<int, string>.Loading().MapError(e => e.Length).IsLoading);
        Assert.True(RemoteData<int, string>.Success(42).MapError(e => e.Length).IsSuccess);
    }

    [Fact]
    public void AndThen_ChainsSuccess()
    {
        var data = RemoteData<int, string>.Success(10);

        var result = data.AndThen(x => RemoteData<int, string>.Success(x * 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Unwrap());
    }

    [Fact]
    public void AndThen_PropagatesOtherStates()
    {
        Assert.True(RemoteData<int, string>.NotAsked()
            .AndThen(x => RemoteData<int, string>.Success(x)).IsNotAsked);
        Assert.True(RemoteData<int, string>.Loading()
            .AndThen(x => RemoteData<int, string>.Success(x)).IsLoading);
        Assert.True(RemoteData<int, string>.Failure("err")
            .AndThen(x => RemoteData<int, string>.Success(x)).IsFailure);
    }

    [Fact]
    public void Match_ExecutesCorrectFunction()
    {
        Assert.Equal("notasked", RemoteData<int, string>.NotAsked()
            .Match(() => "notasked", () => "loading", _ => "success", _ => "failure"));
        Assert.Equal("loading", RemoteData<int, string>.Loading()
            .Match(() => "notasked", () => "loading", _ => "success", _ => "failure"));
        Assert.Equal("success", RemoteData<int, string>.Success(42)
            .Match(() => "notasked", () => "loading", _ => "success", _ => "failure"));
        Assert.Equal("failure", RemoteData<int, string>.Failure("err")
            .Match(() => "notasked", () => "loading", _ => "success", _ => "failure"));
    }

    [Fact]
    public void IsLoaded_ReturnsCorrectly()
    {
        Assert.False(RemoteData<int, string>.NotAsked().IsLoaded());
        Assert.False(RemoteData<int, string>.Loading().IsLoaded());
        Assert.True(RemoteData<int, string>.Success(42).IsLoaded());
        Assert.True(RemoteData<int, string>.Failure("err").IsLoaded());
    }

    [Fact]
    public void IsNotLoaded_ReturnsCorrectly()
    {
        Assert.True(RemoteData<int, string>.NotAsked().IsNotLoaded());
        Assert.True(RemoteData<int, string>.Loading().IsNotLoaded());
        Assert.False(RemoteData<int, string>.Success(42).IsNotLoaded());
        Assert.False(RemoteData<int, string>.Failure("err").IsNotLoaded());
    }

    [Fact]
    public void ToResult_ConvertsSuccessToOk()
    {
        var data = RemoteData<int, string>.Success(42);

        var result = data.ToResult("na", "loading");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void ToResult_ConvertsFailureToErr()
    {
        var data = RemoteData<int, string>.Failure("error");

        var result = data.ToResult("na", "loading");

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public void ToResult_ConvertsNotAskedToErr()
    {
        var data = RemoteData<int, string>.NotAsked();

        var result = data.ToResult("not asked", "loading");

        Assert.True(result.IsErr);
        Assert.Equal("not asked", result.UnwrapErr());
    }

    [Fact]
    public void ToResult_ConvertsLoadingToErr()
    {
        var data = RemoteData<int, string>.Loading();

        var result = data.ToResult("na", "loading");

        Assert.True(result.IsErr);
        Assert.Equal("loading", result.UnwrapErr());
    }

    [Fact]
    public void ToOption_ConvertsSuccessToSome()
    {
        var data = RemoteData<int, string>.Success(42);

        var option = data.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.Unwrap());
    }

    [Fact]
    public void ToOption_ConvertsOtherStatesToNone()
    {
        Assert.True(RemoteData<int, string>.NotAsked().ToOption().IsNone);
        Assert.True(RemoteData<int, string>.Loading().ToOption().IsNone);
        Assert.True(RemoteData<int, string>.Failure("err").ToOption().IsNone);
    }

    [Fact]
    public void Tap_ExecutesOnSuccess()
    {
        int captured = 0;
        var data = RemoteData<int, string>.Success(42);

        var result = data.Tap(v => captured = v);

        Assert.Equal(42, captured);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_DoesNotExecuteOnOtherStates()
    {
        int captured = 0;

        RemoteData<int, string>.NotAsked().Tap(v => captured = v);
        Assert.Equal(0, captured);

        RemoteData<int, string>.Loading().Tap(v => captured = v);
        Assert.Equal(0, captured);

        RemoteData<int, string>.Failure("err").Tap(v => captured = v);
        Assert.Equal(0, captured);
    }

    [Fact]
    public void TapError_ExecutesOnFailure()
    {
        string? captured = null;
        var data = RemoteData<int, string>.Failure("error");

        var result = data.TapError(e => captured = e);

        Assert.Equal("error", captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapFailure_ExecutesOnFailure()
    {
        string? captured = null;
        var data = RemoteData<int, string>.Failure("error");

        var result = data.TapFailure(e => captured = e);

        Assert.Equal("error", captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapFailure_DoesNotExecuteOnOtherStates()
    {
        string? captured = null;

        RemoteData<int, string>.NotAsked().TapFailure(e => captured = e);
        Assert.Null(captured);

        RemoteData<int, string>.Loading().TapFailure(e => captured = e);
        Assert.Null(captured);

        RemoteData<int, string>.Success(42).TapFailure(e => captured = e);
        Assert.Null(captured);
    }

    [Fact]
    public void TapNotAsked_ExecutesOnNotAsked()
    {
        bool executed = false;
        var data = RemoteData<int, string>.NotAsked();

        var result = data.TapNotAsked(() => executed = true);

        Assert.True(executed);
        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void TapNotAsked_DoesNotExecuteOnOtherStates()
    {
        bool executed = false;

        RemoteData<int, string>.Loading().TapNotAsked(() => executed = true);
        Assert.False(executed);

        RemoteData<int, string>.Success(42).TapNotAsked(() => executed = true);
        Assert.False(executed);

        RemoteData<int, string>.Failure("err").TapNotAsked(() => executed = true);
        Assert.False(executed);
    }

    [Fact]
    public void TapLoading_ExecutesOnLoading()
    {
        bool executed = false;
        var data = RemoteData<int, string>.Loading();

        var result = data.TapLoading(() => executed = true);

        Assert.True(executed);
        Assert.True(result.IsLoading);
    }

    [Fact]
    public void TapLoading_DoesNotExecuteOnOtherStates()
    {
        bool executed = false;

        RemoteData<int, string>.NotAsked().TapLoading(() => executed = true);
        Assert.False(executed);

        RemoteData<int, string>.Success(42).TapLoading(() => executed = true);
        Assert.False(executed);

        RemoteData<int, string>.Failure("err").TapLoading(() => executed = true);
        Assert.False(executed);
    }

    [Fact]
    public void Equals_WorksCorrectly()
    {
        Assert.True(RemoteData<int, string>.NotAsked() == RemoteData<int, string>.NotAsked());
        Assert.True(RemoteData<int, string>.Loading() == RemoteData<int, string>.Loading());
        Assert.True(RemoteData<int, string>.Success(42) == RemoteData<int, string>.Success(42));
        Assert.True(RemoteData<int, string>.Failure("err") == RemoteData<int, string>.Failure("err"));

        Assert.False(RemoteData<int, string>.Success(42) == RemoteData<int, string>.Success(43));
        Assert.True(RemoteData<int, string>.Success(42) != RemoteData<int, string>.Loading());
    }

    [Fact]
    public void GetHashCode_SameForEqualValues()
    {
        Assert.Equal(
            RemoteData<int, string>.Success(42).GetHashCode(),
            RemoteData<int, string>.Success(42).GetHashCode()
        );
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        Assert.Contains("NotAsked", RemoteData<int, string>.NotAsked().ToString());
        Assert.Contains("Loading", RemoteData<int, string>.Loading().ToString());
        Assert.Contains("Success", RemoteData<int, string>.Success(42).ToString());
        Assert.Contains("Failure", RemoteData<int, string>.Failure("err").ToString());
    }

    #endregion

    #region RemoteDataExtensions

    [Fact]
    public void ToRemoteData_FromResultOk_ReturnsSuccess()
    {
        var result = Result<int, string>.Ok(42);
        var data = result.ToRemoteData();

        Assert.True(data.IsSuccess);
        Assert.Equal(42, data.Unwrap());
    }

    [Fact]
    public void ToRemoteData_FromResultErr_ReturnsFailure()
    {
        var result = Result<int, string>.Err("error");
        var data = result.ToRemoteData();

        Assert.True(data.IsFailure);
        Assert.Equal("error", data.UnwrapError());
    }

    #endregion
}
