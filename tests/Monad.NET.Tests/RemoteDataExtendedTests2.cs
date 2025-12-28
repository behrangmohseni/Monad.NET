using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for RemoteData<T, E> to improve code coverage.
/// </summary>
public class RemoteDataExtendedTests2
{
    #region Factory Tests

    [Fact]
    public void NotAsked_CreatesNotAskedState()
    {
        var rd = RemoteData<int, string>.NotAsked();

        Assert.True(rd.IsNotAsked);
        Assert.False(rd.IsLoading);
        Assert.False(rd.IsSuccess);
        Assert.False(rd.IsFailure);
    }

    [Fact]
    public void Loading_CreatesLoadingState()
    {
        var rd = RemoteData<int, string>.Loading();

        Assert.True(rd.IsLoading);
        Assert.False(rd.IsNotAsked);
        Assert.False(rd.IsSuccess);
        Assert.False(rd.IsFailure);
    }

    [Fact]
    public void Success_CreatesSuccessState()
    {
        var rd = RemoteData<int, string>.Success(42);

        Assert.True(rd.IsSuccess);
        Assert.False(rd.IsNotAsked);
        Assert.False(rd.IsLoading);
        Assert.False(rd.IsFailure);
        Assert.Equal(42, rd.Unwrap());
    }

    [Fact]
    public void Failure_CreatesFailureState()
    {
        var rd = RemoteData<int, string>.Failure("error");

        Assert.True(rd.IsFailure);
        Assert.False(rd.IsNotAsked);
        Assert.False(rd.IsLoading);
        Assert.False(rd.IsSuccess);
        Assert.Equal("error", rd.UnwrapError());
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnNotAsked_ReturnsNotAsked()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void Map_OnLoading_ReturnsLoading()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public void Map_OnFailure_ReturnsFailure()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.UnwrapError());
    }

    #endregion

    #region AndThen Tests

    [Fact]
    public void AndThen_OnNotAsked_ReturnsNotAsked()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var result = rd.AndThen(x => RemoteData<string, string>.Success(x.ToString()));

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void AndThen_OnLoading_ReturnsLoading()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = rd.AndThen(x => RemoteData<string, string>.Success(x.ToString()));

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void AndThen_OnSuccess_Chains()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.AndThen(x => RemoteData<string, string>.Success(x.ToString()));

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Unwrap());
    }

    [Fact]
    public void AndThen_OnFailure_ReturnsFailure()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.AndThen(x => RemoteData<string, string>.Success(x.ToString()));

        Assert.True(result.IsFailure);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_OnNotAsked_ExecutesNotAskedFunc()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var result = rd.Match(
            () => "not asked",
            () => "loading",
            x => $"success: {x}",
            e => $"failure: {e}");

        Assert.Equal("not asked", result);
    }

    [Fact]
    public void Match_OnLoading_ExecutesLoadingFunc()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = rd.Match(
            () => "not asked",
            () => "loading",
            x => $"success: {x}",
            e => $"failure: {e}");

        Assert.Equal("loading", result);
    }

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessFunc()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.Match(
            () => "not asked",
            () => "loading",
            x => $"success: {x}",
            e => $"failure: {e}");

        Assert.Equal("success: 42", result);
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailureFunc()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.Match(
            () => "not asked",
            () => "loading",
            x => $"success: {x}",
            e => $"failure: {e}");

        Assert.Equal("failure: error", result);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var rd = RemoteData<int, string>.Success(42);
        var executed = false;

        var result = rd.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_OnNotSuccess_DoesNotExecuteAction()
    {
        var rd = RemoteData<int, string>.Loading();
        var executed = false;

        var result = rd.Tap(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsLoading);
    }

    [Fact]
    public void TapFailure_OnFailure_ExecutesAction()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var executed = false;

        var result = rd.TapFailure(e => executed = true);

        Assert.True(executed);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapFailure_OnSuccess_DoesNotExecuteAction()
    {
        var rd = RemoteData<int, string>.Success(42);
        var executed = false;

        var result = rd.TapFailure(e => executed = true);

        Assert.False(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void TapNotAsked_OnNotAsked_ExecutesAction()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var executed = false;

        var result = rd.TapNotAsked(() => executed = true);

        Assert.True(executed);
        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void TapLoading_OnLoading_ExecutesAction()
    {
        var rd = RemoteData<int, string>.Loading();
        var executed = false;

        var result = rd.TapLoading(() => executed = true);

        Assert.True(executed);
        Assert.True(result.IsLoading);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SuccessWithSameValue_ReturnsTrue()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(42);

        Assert.True(rd1.Equals(rd2));
        Assert.True(rd1 == rd2);
    }

    [Fact]
    public void Equals_SuccessWithDifferentValue_ReturnsFalse()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(99);

        Assert.False(rd1.Equals(rd2));
        Assert.True(rd1 != rd2);
    }

    [Fact]
    public void Equals_NotAskedWithNotAsked_ReturnsTrue()
    {
        var rd1 = RemoteData<int, string>.NotAsked();
        var rd2 = RemoteData<int, string>.NotAsked();

        Assert.True(rd1.Equals(rd2));
    }

    [Fact]
    public void Equals_LoadingWithLoading_ReturnsTrue()
    {
        var rd1 = RemoteData<int, string>.Loading();
        var rd2 = RemoteData<int, string>.Loading();

        Assert.True(rd1.Equals(rd2));
    }

    [Fact]
    public void Equals_FailureWithSameError_ReturnsTrue()
    {
        var rd1 = RemoteData<int, string>.Failure("error");
        var rd2 = RemoteData<int, string>.Failure("error");

        Assert.True(rd1.Equals(rd2));
    }

    [Fact]
    public void Equals_DifferentStates_ReturnsFalse()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Loading();

        Assert.False(rd1.Equals(rd2));
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnNotAsked_ContainsNotAsked()
    {
        var rd = RemoteData<int, string>.NotAsked();
        Assert.Contains("NotAsked", rd.ToString());
    }

    [Fact]
    public void ToString_OnLoading_ContainsLoading()
    {
        var rd = RemoteData<int, string>.Loading();
        Assert.Contains("Loading", rd.ToString());
    }

    [Fact]
    public void ToString_OnSuccess_ContainsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        var str = rd.ToString();

        Assert.Contains("42", str);
        Assert.Contains("Success", str);
    }

    [Fact]
    public void ToString_OnFailure_ContainsError()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var str = rd.ToString();

        Assert.Contains("error", str);
        Assert.Contains("Failure", str);
    }

    #endregion

    #region UnwrapOr Tests

    [Fact]
    public void UnwrapOr_OnSuccess_ReturnsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        Assert.Equal(42, rd.UnwrapOr(99));
    }

    [Fact]
    public void UnwrapOr_OnNotSuccess_ReturnsDefault()
    {
        var rd = RemoteData<int, string>.NotAsked();
        Assert.Equal(99, rd.UnwrapOr(99));
    }

    [Fact]
    public void UnwrapOrElse_OnSuccess_ReturnsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        var factoryExecuted = false;
        var value = rd.UnwrapOrElse(() =>
        {
            factoryExecuted = true;
            return 99;
        });

        Assert.False(factoryExecuted);
        Assert.Equal(42, value);
    }

    [Fact]
    public void UnwrapOrElse_OnNotSuccess_ExecutesFactory()
    {
        var rd = RemoteData<int, string>.Loading();
        var value = rd.UnwrapOrElse(() => 99);

        Assert.Equal(99, value);
    }

    #endregion
}

