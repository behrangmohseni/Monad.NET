using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for RemoteData<T, TErr> to improve code coverage.
/// </summary>
public class RemoteDataExtendedTests
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

        Assert.False(rd.IsNotAsked);
        Assert.True(rd.IsLoading);
        Assert.False(rd.IsSuccess);
        Assert.False(rd.IsFailure);
    }

    [Fact]
    public void Success_CreatesSuccessState()
    {
        var rd = RemoteData<int, string>.Success(42);

        Assert.False(rd.IsNotAsked);
        Assert.False(rd.IsLoading);
        Assert.True(rd.IsSuccess);
        Assert.False(rd.IsFailure);
        Assert.Equal(42, rd.GetValue());
    }

    [Fact]
    public void Failure_CreatesFailureState()
    {
        var rd = RemoteData<int, string>.Failure("error");

        Assert.False(rd.IsNotAsked);
        Assert.False(rd.IsLoading);
        Assert.False(rd.IsSuccess);
        Assert.True(rd.IsFailure);
        Assert.Equal("error", rd.GetError());
    }

    #endregion

    #region Unwrap Tests

    [Fact]
    public void Unwrap_NotAsked_Throws()
    {
        var rd = RemoteData<int, string>.NotAsked();
        Assert.Throws<InvalidOperationException>(() => rd.GetValue());
    }

    [Fact]
    public void Unwrap_Loading_Throws()
    {
        var rd = RemoteData<int, string>.Loading();
        Assert.Throws<InvalidOperationException>(() => rd.GetValue());
    }

    [Fact]
    public void Unwrap_Failure_Throws()
    {
        var rd = RemoteData<int, string>.Failure("error");
        Assert.Throws<InvalidOperationException>(() => rd.GetValue());
    }

    [Fact]
    public void UnwrapError_NotFailure_Throws()
    {
        var rd = RemoteData<int, string>.Success(42);
        Assert.Throws<InvalidOperationException>(() => rd.GetError());
    }

    [Fact]
    public void UnwrapOr_Success_ReturnsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        Assert.Equal(42, rd.GetValueOr(99));
    }

    [Fact]
    public void UnwrapOr_NotSuccess_ReturnsDefault()
    {
        var rd = RemoteData<int, string>.Loading();
        Assert.Equal(99, rd.GetValueOr(99));
    }

    [Fact]
    public void UnwrapOrElse_Success_ReturnsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        var factoryExecuted = false;
        var value = rd.GetValueOrElse(() =>
        {
            factoryExecuted = true;
            return 99;
        });

        Assert.False(factoryExecuted);
        Assert.Equal(42, value);
    }

    [Fact]
    public void UnwrapOrElse_NotSuccess_ExecutesFactory()
    {
        var rd = RemoteData<int, string>.Failure("error");
        Assert.Equal(99, rd.GetValueOrElse(() => 99));
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGet_Success_ReturnsTrue()
    {
        var rd = RemoteData<int, string>.Success(42);
        Assert.True(rd.TryGet(out var data));
        Assert.Equal(42, data);
    }

    [Fact]
    public void TryGet_NotSuccess_ReturnsFalse()
    {
        var rd = RemoteData<int, string>.Loading();
        Assert.False(rd.TryGet(out _));
    }

    [Fact]
    public void TryGetError_Failure_ReturnsTrue()
    {
        var rd = RemoteData<int, string>.Failure("error");
        Assert.True(rd.TryGetError(out var error));
        Assert.Equal("error", error);
    }

    [Fact]
    public void TryGetError_NotFailure_ReturnsFalse()
    {
        var rd = RemoteData<int, string>.Success(42);
        Assert.False(rd.TryGetError(out _));
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_Success_TransformsValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Map_NotAsked_PreservesState()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsNotAsked);
    }

    [Fact]
    public void Map_Loading_PreservesState()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void Map_Failure_PreservesError()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.GetError());
    }

    #endregion

    #region MapError Tests

    [Fact]
    public void MapError_Failure_TransformsError()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.MapError(e => e.ToUpper());

        Assert.True(result.IsFailure);
        Assert.Equal("ERROR", result.GetError());
    }

    [Fact]
    public void MapError_Success_PreservesValue()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.MapError(e => e.ToUpper());

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    #endregion

    #region BiMap Tests

    [Fact]
    public void BiMap_Success_MapsData()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.BiMap(x => x * 2, e => e.ToUpper());

        Assert.True(result.IsSuccess);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void BiMap_Failure_MapsError()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.BiMap(x => x * 2, e => e.ToUpper());

        Assert.True(result.IsFailure);
        Assert.Equal("ERROR", result.GetError());
    }

    #endregion

    #region AndThen/FlatMap/Bind Tests

    [Fact]
    public void AndThen_Success_Chains()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.Bind(x => RemoteData<string, string>.Success($"Value: {x}"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Value: 42", result.GetValue());
    }

    [Fact]
    public void AndThen_NotSuccess_PreservesState()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = rd.Bind(x => RemoteData<string, string>.Success($"Value: {x}"));

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void FlatMap_Success_Chains()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.Bind(x => RemoteData<string, string>.Success($"Value: {x}"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Bind_Success_Chains()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.Bind(x => RemoteData<string, string>.Success($"Value: {x}"));

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Or/OrElse Tests

    [Fact]
    public void Or_Success_ReturnsOriginal()
    {
        var rd = RemoteData<int, string>.Success(42);
        var alt = RemoteData<int, string>.Success(99);
        var result = rd.Or(alt);

        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Or_NotSuccess_ReturnsAlternative()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var alt = RemoteData<int, string>.Success(99);
        var result = rd.Or(alt);

        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void OrElse_Failure_RecoveryIsCalled()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.OrElse(e => RemoteData<int, string>.Success(99));

        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void OrElse_Success_RecoveryNotCalled()
    {
        var rd = RemoteData<int, string>.Success(42);
        var recoveryCalled = false;
        var result = rd.OrElse(e =>
        {
            recoveryCalled = true;
            return RemoteData<int, string>.Success(99);
        });

        Assert.False(recoveryCalled);
        Assert.Equal(42, result.GetValue());
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_Actions_NotAsked()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var state = "";

        rd.Match(
            () => state = "NotAsked",
            () => state = "Loading",
            x => state = $"Success: {x}",
            e => state = $"Failure: {e}");

        Assert.Equal("NotAsked", state);
    }

    [Fact]
    public void Match_Actions_Loading()
    {
        var rd = RemoteData<int, string>.Loading();
        var state = "";

        rd.Match(
            () => state = "NotAsked",
            () => state = "Loading",
            x => state = $"Success: {x}",
            e => state = $"Failure: {e}");

        Assert.Equal("Loading", state);
    }

    [Fact]
    public void Match_Actions_Success()
    {
        var rd = RemoteData<int, string>.Success(42);
        var state = "";

        rd.Match(
            () => state = "NotAsked",
            () => state = "Loading",
            x => state = $"Success: {x}",
            e => state = $"Failure: {e}");

        Assert.Equal("Success: 42", state);
    }

    [Fact]
    public void Match_Actions_Failure()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var state = "";

        rd.Match(
            () => state = "NotAsked",
            () => state = "Loading",
            x => state = $"Success: {x}",
            e => state = $"Failure: {e}");

        Assert.Equal("Failure: error", state);
    }

    [Fact]
    public void Match_Func_AllStates()
    {
        var notAsked = RemoteData<int, string>.NotAsked();
        var loading = RemoteData<int, string>.Loading();
        var success = RemoteData<int, string>.Success(42);
        var failure = RemoteData<int, string>.Failure("error");

        Assert.Equal("NotAsked", notAsked.Match(() => "NotAsked", () => "Loading", x => $"Success: {x}", e => $"Failure: {e}"));
        Assert.Equal("Loading", loading.Match(() => "NotAsked", () => "Loading", x => $"Success: {x}", e => $"Failure: {e}"));
        Assert.Equal("Success: 42", success.Match(() => "NotAsked", () => "Loading", x => $"Success: {x}", e => $"Failure: {e}"));
        Assert.Equal("Failure: error", failure.Match(() => "NotAsked", () => "Loading", x => $"Success: {x}", e => $"Failure: {e}"));
    }

    #endregion

    #region ToOption/ToResult Tests

    [Fact]
    public void ToOption_Success_ReturnsSome()
    {
        var rd = RemoteData<int, string>.Success(42);
        var option = rd.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void ToOption_NotSuccess_ReturnsNone()
    {
        var rd = RemoteData<int, string>.Loading();
        var option = rd.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void ToResult_Success_ReturnsOk()
    {
        var rd = RemoteData<int, string>.Success(42);
        var result = rd.ToResult();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_Failure_ReturnsErr()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var result = rd.ToResult();

        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void ToResult_NotAsked_Throws()
    {
        var rd = RemoteData<int, string>.NotAsked();
        Assert.Throws<InvalidOperationException>(() => rd.ToResult());
    }

    [Fact]
    public void ToResult_Loading_Throws()
    {
        var rd = RemoteData<int, string>.Loading();
        Assert.Throws<InvalidOperationException>(() => rd.ToResult());
    }

    [Fact]
    public void ToResult_WithDefaults_NotAsked()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var result = rd.ToResult("not asked", "loading");

        Assert.True(result.IsErr);
        Assert.Equal("not asked", result.GetError());
    }

    [Fact]
    public void ToResult_WithDefaults_Loading()
    {
        var rd = RemoteData<int, string>.Loading();
        var result = rd.ToResult("not asked", "loading");

        Assert.True(result.IsErr);
        Assert.Equal("loading", result.GetError());
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameState_ReturnsTrue()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(42);

        Assert.True(rd1.Equals(rd2));
        Assert.True(rd1 == rd2);
    }

    [Fact]
    public void Equals_DifferentState_ReturnsFalse()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Loading();

        Assert.False(rd1.Equals(rd2));
        Assert.True(rd1 != rd2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(99);

        Assert.False(rd1.Equals(rd2));
    }

    [Fact]
    public void Equals_NotAsked_ReturnsTrue()
    {
        var rd1 = RemoteData<int, string>.NotAsked();
        var rd2 = RemoteData<int, string>.NotAsked();

        Assert.True(rd1.Equals(rd2));
    }

    [Fact]
    public void Equals_Object_Works()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        object rd2 = RemoteData<int, string>.Success(42);
        object notRd = "not remote data";

        Assert.True(rd1.Equals(rd2));
        Assert.False(rd1.Equals(notRd));
    }

    [Fact]
    public void GetHashCode_SameForEqual()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(42);

        Assert.Equal(rd1.GetHashCode(), rd2.GetHashCode());
    }

    #endregion

    #region CompareTo Tests

    [Fact]
    public void CompareTo_SameState_ComparesValues()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(99);

        Assert.True(rd1.CompareTo(rd2) < 0);
    }

    [Fact]
    public void CompareTo_DifferentStates_ComparesStates()
    {
        var notAsked = RemoteData<int, string>.NotAsked();
        var loading = RemoteData<int, string>.Loading();
        var success = RemoteData<int, string>.Success(42);
        var failure = RemoteData<int, string>.Failure("error");

        // Enum order: NotAsked=0, Loading=1, Success=2, Failure=3
        Assert.True(notAsked.CompareTo(loading) < 0);
        Assert.True(loading.CompareTo(success) < 0);
        Assert.True(success.CompareTo(failure) < 0);
    }

    [Fact]
    public void CompareTo_Object_Works()
    {
        var rd = RemoteData<int, string>.Success(42);
        IComparable comparable = rd;

        Assert.True(comparable.CompareTo(null) > 0);
        Assert.Equal(0, comparable.CompareTo(RemoteData<int, string>.Success(42)));
    }

    [Fact]
    public void CompareTo_InvalidType_Throws()
    {
        var rd = RemoteData<int, string>.Success(42);
        IComparable comparable = rd;

        Assert.Throws<ArgumentException>(() => comparable.CompareTo("invalid"));
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_AllStates()
    {
        Assert.Equal("NotAsked", RemoteData<int, string>.NotAsked().ToString());
        Assert.Equal("Loading", RemoteData<int, string>.Loading().ToString());
        Assert.Equal("Success(42)", RemoteData<int, string>.Success(42).ToString());
        Assert.Equal("Failure(error)", RemoteData<int, string>.Failure("error").ToString());
    }

    #endregion

    #region Deconstruct Tests

    [Fact]
    public void Deconstruct_TwoArgs_Works()
    {
        var rd = RemoteData<int, string>.Success(42);
        var (data, isSuccess) = rd;

        Assert.Equal(42, data);
        Assert.True(isSuccess);
    }

    [Fact]
    public void Deconstruct_SixArgs_Works()
    {
        var rd = RemoteData<int, string>.Loading();
        var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = rd;

        Assert.Equal(default, data);
        Assert.Equal(default, error);
        Assert.False(isNotAsked);
        Assert.True(isLoading);
        Assert.False(isSuccess);
        Assert.False(isFailure);
    }

    #endregion

    #region Extension Tests

    [Fact]
    public void Tap_Success_ExecutesAction()
    {
        var rd = RemoteData<int, string>.Success(42);
        var capturedValue = 0;

        var result = rd.Tap(x => capturedValue = x);

        Assert.Equal(42, capturedValue);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_NotSuccess_DoesNotExecute()
    {
        var rd = RemoteData<int, string>.Loading();
        var executed = false;

        rd.Tap(x => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void TapFailure_Failure_ExecutesAction()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var capturedError = "";

        rd.TapFailure(e => capturedError = e);

        Assert.Equal("error", capturedError);
    }

    [Fact]
    public void TapNotAsked_NotAsked_ExecutesAction()
    {
        var rd = RemoteData<int, string>.NotAsked();
        var executed = false;

        rd.TapNotAsked(() => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void TapLoading_Loading_ExecutesAction()
    {
        var rd = RemoteData<int, string>.Loading();
        var executed = false;

        rd.TapLoading(() => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void TapError_Alias_Works()
    {
        var rd = RemoteData<int, string>.Failure("error");
        var capturedError = "";

        rd.TapFailure(e => capturedError = e);

        Assert.Equal("error", capturedError);
    }

    [Fact]
    public void ToRemoteData_Ok_ReturnsSuccess()
    {
        var result = Result<int, string>.Ok(42);
        var rd = result.ToRemoteData();

        Assert.True(rd.IsSuccess);
        Assert.Equal(42, rd.GetValue());
    }

    [Fact]
    public void ToRemoteData_Err_ReturnsFailure()
    {
        var result = Result<int, string>.Err("error");
        var rd = result.ToRemoteData();

        Assert.True(rd.IsFailure);
        Assert.Equal("error", rd.GetError());
    }

    [Fact]
    public async Task FromTaskAsync_Success_ReturnsSuccess()
    {
        var rd = await RemoteDataExtensions.FromTaskAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(rd.IsSuccess);
        Assert.Equal(42, rd.GetValue());
    }

    [Fact]
    public async Task FromTaskAsync_Exception_ReturnsFailure()
    {
        var rd = await RemoteDataExtensions.FromTaskAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("test");
        });

        Assert.True(rd.IsFailure);
        Assert.IsType<InvalidOperationException>(rd.GetError());
    }

    [Fact]
    public async Task MapAsync_Success_TransformsAsync()
    {
        var rd = RemoteData<int, string>.Success(21);
        var result = await rd.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void IsLoaded_SuccessOrFailure_ReturnsTrue()
    {
        Assert.True(RemoteData<int, string>.Success(42).IsLoaded());
        Assert.True(RemoteData<int, string>.Failure("error").IsLoaded());
        Assert.False(RemoteData<int, string>.NotAsked().IsLoaded());
        Assert.False(RemoteData<int, string>.Loading().IsLoaded());
    }

    [Fact]
    public void IsNotLoaded_NotAskedOrLoading_ReturnsTrue()
    {
        Assert.True(RemoteData<int, string>.NotAsked().IsNotLoaded());
        Assert.True(RemoteData<int, string>.Loading().IsNotLoaded());
        Assert.False(RemoteData<int, string>.Success(42).IsNotLoaded());
        Assert.False(RemoteData<int, string>.Failure("error").IsNotLoaded());
    }

    #endregion
}
