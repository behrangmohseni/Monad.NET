using Monad.NET;

namespace Monad.NET.Tests;

public class RemoteDataTests
{
    [Fact]
    public void NotAsked_CreatesNotAskedState()
    {
        var remoteData = RemoteData<int, string>.NotAsked();

        Assert.True(remoteData.IsNotAsked);
        Assert.False(remoteData.IsLoading);
        Assert.False(remoteData.IsSuccess);
        Assert.False(remoteData.IsFailure);
    }

    [Fact]
    public void Loading_CreatesLoadingState()
    {
        var remoteData = RemoteData<int, string>.Loading();

        Assert.False(remoteData.IsNotAsked);
        Assert.True(remoteData.IsLoading);
        Assert.False(remoteData.IsSuccess);
        Assert.False(remoteData.IsFailure);
    }

    [Fact]
    public void Success_CreatesSuccessState()
    {
        var remoteData = RemoteData<int, string>.Success(42);

        Assert.False(remoteData.IsNotAsked);
        Assert.False(remoteData.IsLoading);
        Assert.True(remoteData.IsSuccess);
        Assert.False(remoteData.IsFailure);
        Assert.Equal(42, remoteData.GetValue());
    }

    [Fact]
    public void Failure_CreatesFailureState()
    {
        var remoteData = RemoteData<int, string>.Failure("error");

        Assert.False(remoteData.IsNotAsked);
        Assert.False(remoteData.IsLoading);
        Assert.False(remoteData.IsSuccess);
        Assert.True(remoteData.IsFailure);
        Assert.Equal("error", remoteData.GetError());
    }

    [Fact]
    public void Success_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => RemoteData<string, int>.Success(null!));
    }

    [Fact]
    public void Failure_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => RemoteData<int, string>.Failure(null!));
    }

    [Fact]
    public void Unwrap_OnSuccess_ReturnsData()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        Assert.Equal(42, remoteData.GetValue());
    }

    [Fact]
    public void Unwrap_OnNotAsked_ThrowsException()
    {
        var remoteData = RemoteData<int, string>.NotAsked();
        Assert.Throws<InvalidOperationException>(() => remoteData.GetValue());
    }

    [Fact]
    public void Unwrap_OnLoading_ThrowsException()
    {
        var remoteData = RemoteData<int, string>.Loading();
        Assert.Throws<InvalidOperationException>(() => remoteData.GetValue());
    }

    [Fact]
    public void Unwrap_OnFailure_ThrowsException()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        Assert.Throws<InvalidOperationException>(() => remoteData.GetValue());
    }

    [Fact]
    public void UnwrapError_OnFailure_ReturnsError()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        Assert.Equal("error", remoteData.GetError());
    }

    [Fact]
    public void UnwrapError_OnSuccess_ThrowsException()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        Assert.Throws<InvalidOperationException>(() => remoteData.GetError());
    }

    [Fact]
    public void UnwrapOr_OnSuccess_ReturnsData()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        Assert.Equal(42, remoteData.GetValueOr(0));
    }

    [Fact]
    public void UnwrapOr_OnLoading_ReturnsDefault()
    {
        var remoteData = RemoteData<int, string>.Loading();
        Assert.Equal(0, remoteData.GetValueOr(0));
    }

    [Fact]
    public void Map_OnSuccess_TransformsData()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var mapped = remoteData.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void Map_OnLoading_PreservesLoading()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var mapped = remoteData.Map(x => x * 2);

        Assert.True(mapped.IsLoading);
    }

    [Fact]
    public void Map_OnFailure_PreservesFailure()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        var mapped = remoteData.Map(x => x * 2);

        Assert.True(mapped.IsFailure);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public void MapError_OnFailure_TransformsError()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        var mapped = remoteData.MapError(e => e.ToUpper());

        Assert.True(mapped.IsFailure);
        Assert.Equal("ERROR", mapped.GetError());
    }

    [Fact]
    public void MapError_OnSuccess_PreservesSuccess()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var mapped = remoteData.MapError(e => e.ToUpper());

        Assert.True(mapped.IsSuccess);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public void AndThen_OnSuccess_ExecutesFunction()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var result = remoteData.Bind(x => RemoteData<string, string>.Success(x.ToString()));

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public void AndThen_OnLoading_ReturnsLoading()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var result = remoteData.Bind(x => RemoteData<string, string>.Success(x.ToString()));

        Assert.True(result.IsLoading);
    }

    [Fact]
    public void Or_OnSuccess_ReturnsThis()
    {
        var remoteData1 = RemoteData<int, string>.Success(42);
        var remoteData2 = RemoteData<int, string>.Success(100);
        var result = remoteData1.Or(remoteData2);

        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Or_OnLoading_ReturnsAlternative()
    {
        var remoteData1 = RemoteData<int, string>.Loading();
        var remoteData2 = RemoteData<int, string>.Success(100);
        var result = remoteData1.Or(remoteData2);

        Assert.Equal(100, result.GetValue());
    }

    [Fact]
    public void OrElse_OnFailure_Recovers()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        var recovered = remoteData.OrElse(err => RemoteData<int, string>.Success(100));

        Assert.True(recovered.IsSuccess);
        Assert.Equal(100, recovered.GetValue());
    }

    [Fact]
    public void OrElse_OnSuccess_ReturnsThis()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var recovered = remoteData.OrElse(err => RemoteData<int, string>.Success(100));

        Assert.Equal(42, recovered.GetValue());
    }

    [Fact]
    public void Match_OnNotAsked_ExecutesNotAskedAction()
    {
        var remoteData = RemoteData<int, string>.NotAsked();
        var state = string.Empty;

        remoteData.Match(
            notAskedAction: () => state = "not asked",
            loadingAction: () => state = "loading",
            successAction: x => state = "success",
            failureAction: err => state = "failure"
        );

        Assert.Equal("not asked", state);
    }

    [Fact]
    public void Match_OnLoading_ExecutesLoadingAction()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var state = string.Empty;

        remoteData.Match(
            notAskedAction: () => state = "not asked",
            loadingAction: () => state = "loading",
            successAction: x => state = "success",
            failureAction: err => state = "failure"
        );

        Assert.Equal("loading", state);
    }

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessAction()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var value = 0;

        remoteData.Match(
            notAskedAction: () => value = -1,
            loadingAction: () => value = -2,
            successAction: x => value = x,
            failureAction: err => value = -3
        );

        Assert.Equal(42, value);
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailureAction()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        var errorMsg = string.Empty;

        remoteData.Match(
            notAskedAction: () => errorMsg = "not asked",
            loadingAction: () => errorMsg = "loading",
            successAction: x => errorMsg = "success",
            failureAction: err => errorMsg = err
        );

        Assert.Equal("error", errorMsg);
    }

    [Fact]
    public void Match_WithReturn_OnSuccess_ReturnsSuccessValue()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var result = remoteData.Match(
            notAskedFunc: () => "not asked",
            loadingFunc: () => "loading",
            successFunc: x => $"success: {x}",
            failureFunc: err => $"error: {err}"
        );

        Assert.Equal("success: 42", result);
    }

    [Fact]
    public void ToOption_OnSuccess_ReturnsSome()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var option = remoteData.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void ToOption_OnLoading_ReturnsNone()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var option = remoteData.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void ToResult_OnSuccess_ReturnsOk()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var result = remoteData.ToResult();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_OnFailure_ReturnsErr()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        var result = remoteData.ToResult();

        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void ToResult_OnLoading_ThrowsException()
    {
        var remoteData = RemoteData<int, string>.Loading();
        Assert.Throws<InvalidOperationException>(() => remoteData.ToResult());
    }

    [Fact]
    public void ToResult_WithDefaults_OnLoading_ReturnsErr()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var result = remoteData.ToResult("not asked error", "loading error");

        Assert.True(result.IsErr);
        Assert.Equal("loading error", result.GetError());
    }

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var executed = false;

        var result = remoteData.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_OnLoading_DoesNotExecuteAction()
    {
        var remoteData = RemoteData<int, string>.Loading();
        var executed = false;

        var result = remoteData.Tap(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsLoading);
    }

    [Fact]
    public void TapError_OnFailure_ExecutesAction()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        var executed = false;

        var result = remoteData.TapFailure(err => executed = true);

        Assert.True(executed);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToRemoteData_FromOk_ReturnsSuccess()
    {
        var result = Result<int, string>.Ok(42);
        var remoteData = result.ToRemoteData();

        Assert.True(remoteData.IsSuccess);
        Assert.Equal(42, remoteData.GetValue());
    }

    [Fact]
    public void ToRemoteData_FromErr_ReturnsFailure()
    {
        var result = Result<int, string>.Err("error");
        var remoteData = result.ToRemoteData();

        Assert.True(remoteData.IsFailure);
        Assert.Equal("error", remoteData.GetError());
    }

    [Fact]
    public async Task FromTaskAsync_Success_ReturnsSuccess()
    {
        var remoteData = await RemoteDataExtensions.FromTaskAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(remoteData.IsSuccess);
        Assert.Equal(42, remoteData.GetValue());
    }

    [Fact]
    public async Task FromTaskAsync_Throws_ReturnsFailure()
    {
        var remoteData = await RemoteDataExtensions.FromTaskAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("test error");
        });

        Assert.True(remoteData.IsFailure);
        Assert.IsType<InvalidOperationException>(remoteData.GetError());
    }

    [Fact]
    public async Task MapAsync_OnSuccess_TransformsData()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        var mapped = await remoteData.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void IsLoaded_OnSuccess_ReturnsTrue()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        Assert.True(remoteData.IsLoaded());
    }

    [Fact]
    public void IsLoaded_OnFailure_ReturnsTrue()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        Assert.True(remoteData.IsLoaded());
    }

    [Fact]
    public void IsLoaded_OnLoading_ReturnsFalse()
    {
        var remoteData = RemoteData<int, string>.Loading();
        Assert.False(remoteData.IsLoaded());
    }

    [Fact]
    public void IsNotLoaded_OnNotAsked_ReturnsTrue()
    {
        var remoteData = RemoteData<int, string>.NotAsked();
        Assert.True(remoteData.IsNotLoaded());
    }

    [Fact]
    public void IsNotLoaded_OnSuccess_ReturnsFalse()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        Assert.False(remoteData.IsNotLoaded());
    }

    [Fact]
    public void Equality_TwoSuccessWithSameData_AreEqual()
    {
        var rd1 = RemoteData<int, string>.Success(42);
        var rd2 = RemoteData<int, string>.Success(42);

        Assert.Equal(rd1, rd2);
        Assert.True(rd1 == rd2);
    }

    [Fact]
    public void Equality_TwoLoading_AreEqual()
    {
        var rd1 = RemoteData<int, string>.Loading();
        var rd2 = RemoteData<int, string>.Loading();

        Assert.Equal(rd1, rd2);
        Assert.True(rd1 == rd2);
    }

    [Fact]
    public void ToString_OnNotAsked_ReturnsFormattedString()
    {
        var remoteData = RemoteData<int, string>.NotAsked();
        Assert.Equal("NotAsked", remoteData.ToString());
    }

    [Fact]
    public void ToString_OnLoading_ReturnsFormattedString()
    {
        var remoteData = RemoteData<int, string>.Loading();
        Assert.Equal("Loading", remoteData.ToString());
    }

    [Fact]
    public void ToString_OnSuccess_ReturnsFormattedString()
    {
        var remoteData = RemoteData<int, string>.Success(42);
        Assert.Equal("Success(42)", remoteData.ToString());
    }

    [Fact]
    public void ToString_OnFailure_ReturnsFormattedString()
    {
        var remoteData = RemoteData<int, string>.Failure("error");
        Assert.Equal("Failure(error)", remoteData.ToString());
    }

    [Fact]
    public void RealWorld_ApiCall_Workflow()
    {
        // Initial state
        var userData = RemoteData<User, string>.NotAsked();
        Assert.True(userData.IsNotAsked);

        // Start loading
        userData = RemoteData<User, string>.Loading();
        Assert.True(userData.IsLoading);

        // Success
        userData = RemoteData<User, string>.Success(new User { Name = "John", Age = 30 });
        Assert.True(userData.IsSuccess);
        Assert.Equal("John", userData.GetValue().Name);
    }

    private class User
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}

