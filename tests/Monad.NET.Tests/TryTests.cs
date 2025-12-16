using Monad.NET;

namespace Monad.NET.Tests;

public class TryTests
{
    [Fact]
    public void Success_CreatesSuccessfulTry()
    {
        var tryValue = Try<int>.Success(42);

        Assert.True(tryValue.IsSuccess);
        Assert.False(tryValue.IsFailure);
        Assert.Equal(42, tryValue.Get());
    }

    [Fact]
    public void Failure_CreatesFailedTry()
    {
        var exception = new InvalidOperationException("test error");
        var tryValue = Try<int>.Failure(exception);

        Assert.False(tryValue.IsSuccess);
        Assert.True(tryValue.IsFailure);
        Assert.Equal(exception, tryValue.GetException());
    }

    [Fact]
    public void Success_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Try<string>.Success(null!));
    }

    [Fact]
    public void Failure_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Try<int>.Failure(null!));
    }

    [Fact]
    public void Get_OnSuccess_ReturnsValue()
    {
        var tryValue = Try<int>.Success(42);
        Assert.Equal(42, tryValue.Get());
    }

    [Fact]
    public void Get_OnFailure_ThrowsException()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        Assert.Throws<InvalidOperationException>(() => tryValue.Get());
    }

    [Fact]
    public void GetException_OnFailure_ReturnsException()
    {
        var exception = new InvalidOperationException("test");
        var tryValue = Try<int>.Failure(exception);

        Assert.Equal(exception, tryValue.GetException());
    }

    [Fact]
    public void GetException_OnSuccess_ThrowsException()
    {
        var tryValue = Try<int>.Success(42);
        Assert.Throws<InvalidOperationException>(() => tryValue.GetException());
    }

    [Fact]
    public void Of_WithSuccessfulFunc_ReturnsSuccess()
    {
        var tryValue = Try<int>.Of(() => 42);

        Assert.True(tryValue.IsSuccess);
        Assert.Equal(42, tryValue.Get());
    }

    [Fact]
    public void Of_WithFailingFunc_ReturnsFailure()
    {
        var tryValue = Try<int>.Of(() => throw new InvalidOperationException("error"));

        Assert.True(tryValue.IsFailure);
        Assert.IsType<InvalidOperationException>(tryValue.GetException());
    }

    [Fact]
    public async Task OfAsync_WithSuccessfulFunc_ReturnsSuccess()
    {
        var tryValue = await Try<int>.OfAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(tryValue.IsSuccess);
        Assert.Equal(42, tryValue.Get());
    }

    [Fact]
    public async Task OfAsync_WithFailingFunc_ReturnsFailure()
    {
        var tryValue = await Try<int>.OfAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("error");
#pragma warning disable CS0162
            return 0;
#pragma warning restore CS0162
        });

        Assert.True(tryValue.IsFailure);
    }

    [Fact]
    public void GetOrElse_OnSuccess_ReturnsValue()
    {
        var tryValue = Try<int>.Success(42);
        Assert.Equal(42, tryValue.GetOrElse(0));
    }

    [Fact]
    public void GetOrElse_OnFailure_ReturnsDefault()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        Assert.Equal(0, tryValue.GetOrElse(0));
    }

    [Fact]
    public void GetOrElse_WithFunc_OnFailure_ExecutesFunc()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        Assert.Equal(100, tryValue.GetOrElse(() => 100));
    }

    [Fact]
    public void GetOrElse_WithRecovery_OnFailure_UsesException()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var result = tryValue.GetOrElse(ex => ex.Message.Length);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var tryValue = Try<int>.Success(42);
        var mapped = tryValue.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.Get());
    }

    [Fact]
    public void Map_OnFailure_PreservesFailure()
    {
        var exception = new Exception("error");
        var tryValue = Try<int>.Failure(exception);
        var mapped = tryValue.Map(x => x * 2);

        Assert.True(mapped.IsFailure);
        Assert.Equal(exception, mapped.GetException());
    }

    [Fact]
    public void Map_ThatThrows_CapturesException()
    {
        var tryValue = Try<int>.Success(42);
        var mapped = tryValue.Map<int>(x => throw new InvalidOperationException("map error"));

        Assert.True(mapped.IsFailure);
        Assert.IsType<InvalidOperationException>(mapped.GetException());
    }

    [Fact]
    public async Task MapAsync_OnSuccess_TransformsValue()
    {
        var tryValue = Try<int>.Success(42);
        var mapped = await tryValue.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.Get());
    }

    [Fact]
    public void FlatMap_OnSuccess_ChainsCorrectly()
    {
        var tryValue = Try<int>.Success(42);
        var result = tryValue.FlatMap(x => Try<string>.Success(x.ToString()));

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Get());
    }

    [Fact]
    public void FlatMap_OnFailure_PreservesFailure()
    {
        var exception = new Exception("error");
        var tryValue = Try<int>.Failure(exception);
        var result = tryValue.FlatMap(x => Try<string>.Success(x.ToString()));

        Assert.True(result.IsFailure);
        Assert.Equal(exception, result.GetException());
    }

    [Fact]
    public void Recover_OnFailure_Recovers()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var recovered = tryValue.Recover(ex => 100);

        Assert.True(recovered.IsSuccess);
        Assert.Equal(100, recovered.Get());
    }

    [Fact]
    public void Recover_OnSuccess_ReturnsOriginal()
    {
        var tryValue = Try<int>.Success(42);
        var recovered = tryValue.Recover(ex => 100);

        Assert.Equal(42, recovered.Get());
    }

    [Fact]
    public void RecoverWith_OnFailure_Recovers()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var recovered = tryValue.RecoverWith(ex => Try<int>.Success(100));

        Assert.True(recovered.IsSuccess);
        Assert.Equal(100, recovered.Get());
    }

    [Fact]
    public void Filter_WithMatchingPredicate_ReturnsSuccess()
    {
        var tryValue = Try<int>.Success(42);
        var filtered = tryValue.Filter(x => x > 40);

        Assert.True(filtered.IsSuccess);
        Assert.Equal(42, filtered.Get());
    }

    [Fact]
    public void Filter_WithNonMatchingPredicate_ReturnsFailure()
    {
        var tryValue = Try<int>.Success(42);
        var filtered = tryValue.Filter(x => x < 40);

        Assert.True(filtered.IsFailure);
    }

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessAction()
    {
        var tryValue = Try<int>.Success(42);
        var value = 0;

        tryValue.Match(
            successAction: x => value = x,
            failureAction: ex => value = -1
        );

        Assert.Equal(42, value);
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailureAction()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var errorMsg = string.Empty;

        tryValue.Match(
            successAction: x => errorMsg = "success",
            failureAction: ex => errorMsg = ex.Message
        );

        Assert.Equal("error", errorMsg);
    }

    [Fact]
    public void Match_WithReturn_OnSuccess_ReturnsSuccessValue()
    {
        var tryValue = Try<int>.Success(42);
        var result = tryValue.Match(
            successFunc: x => x.ToString(),
            failureFunc: ex => ex.Message
        );

        Assert.Equal("42", result);
    }

    [Fact]
    public void ToOption_OnSuccess_ReturnsSome()
    {
        var tryValue = Try<int>.Success(42);
        var option = tryValue.ToOption();

        Assert.True(option.IsSome);
        Assert.Equal(42, option.Unwrap());
    }

    [Fact]
    public void ToOption_OnFailure_ReturnsNone()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var option = tryValue.ToOption();

        Assert.True(option.IsNone);
    }

    [Fact]
    public void ToResult_OnSuccess_ReturnsOk()
    {
        var tryValue = Try<int>.Success(42);
        var result = tryValue.ToResult();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void ToResult_OnFailure_ReturnsErr()
    {
        var exception = new Exception("error");
        var tryValue = Try<int>.Failure(exception);
        var result = tryValue.ToResult();

        Assert.True(result.IsErr);
        Assert.Equal(exception, result.UnwrapErr());
    }

    [Fact]
    public void ToResult_WithMapper_TransformsException()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var result = tryValue.ToResult(ex => ex.Message);

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var tryValue = Try<int>.Success(42);
        var executed = false;

        var result = tryValue.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void TapFailure_OnFailure_ExecutesAction()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var executed = false;

        var result = tryValue.TapFailure(ex => executed = true);

        Assert.True(executed);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Flatten_NestedSuccess_ReturnsInnerSuccess()
    {
        var nested = Try<Try<int>>.Success(Try<int>.Success(42));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsSuccess);
        Assert.Equal(42, flattened.Get());
    }

    [Fact]
    public void Flatten_OuterFailure_ReturnsFailure()
    {
        var exception = new Exception("outer error");
        var nested = Try<Try<int>>.Failure(exception);
        var flattened = nested.Flatten();

        Assert.True(flattened.IsFailure);
        Assert.Equal(exception, flattened.GetException());
    }

    [Fact]
    public void ToTry_FromOk_ReturnsSuccess()
    {
        var result = Result<int, Exception>.Ok(42);
        var tryValue = result.ToTry();

        Assert.True(tryValue.IsSuccess);
        Assert.Equal(42, tryValue.Get());
    }

    [Fact]
    public void ToTry_FromErr_ReturnsFailure()
    {
        var exception = new Exception("error");
        var result = Result<int, Exception>.Err(exception);
        var tryValue = result.ToTry();

        Assert.True(tryValue.IsFailure);
        Assert.Equal(exception, tryValue.GetException());
    }

    [Fact]
    public void RealWorld_ParseInt_Success()
    {
        var tryValue = Try<int>.Of(() => int.Parse("42"));

        Assert.True(tryValue.IsSuccess);
        Assert.Equal(42, tryValue.Get());
    }

    [Fact]
    public void RealWorld_ParseInt_Failure()
    {
        var tryValue = Try<int>.Of(() => int.Parse("not a number"));

        Assert.True(tryValue.IsFailure);
        Assert.IsType<FormatException>(tryValue.GetException());
    }

    [Fact]
    public void RealWorld_ChainOperations()
    {
        var result = Try<int>.Of(() => int.Parse("42"))
            .Map(x => x * 2)
            .Filter(x => x > 50, "too small")
            .GetOrElse(0);

        Assert.Equal(84, result);
    }
}
