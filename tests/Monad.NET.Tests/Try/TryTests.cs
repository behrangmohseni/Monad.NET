using Monad.NET;

namespace Monad.NET.Tests;

public class TryTests
{
    [Fact]
    public void Success_CreatesSuccessfulTry()
    {
        var tryValue = Try<int>.Success(42);

        Assert.True(tryValue.IsOk);
        Assert.False(tryValue.IsError);
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void Failure_CreatesFailedTry()
    {
        var exception = new InvalidOperationException("test error");
        var tryValue = Try<int>.Failure(exception);

        Assert.False(tryValue.IsOk);
        Assert.True(tryValue.IsError);
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
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void Get_OnFailure_ThrowsException()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        Assert.Throws<InvalidOperationException>(() => tryValue.GetValue());
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

        Assert.True(tryValue.IsOk);
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void Of_WithFailingFunc_ReturnsFailure()
    {
        var tryValue = Try<int>.Of(() => throw new InvalidOperationException("error"));

        Assert.True(tryValue.IsError);
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

        Assert.True(tryValue.IsOk);
        Assert.Equal(42, tryValue.GetValue());
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

        Assert.True(tryValue.IsError);
    }

    [Fact]
    public void GetOrElse_OnSuccess_ReturnsValue()
    {
        var tryValue = Try<int>.Success(42);
        Assert.Equal(42, tryValue.GetValueOr(0));
    }

    [Fact]
    public void GetOrElse_OnFailure_ReturnsDefault()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        Assert.Equal(0, tryValue.GetValueOr(0));
    }

    [Fact]
    public void GetValueOrElse_WithFunc_OnFailure_ExecutesFunc()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        Assert.Equal(100, tryValue.GetValueOrElse(() => 100));
    }

    [Fact]
    public void GetValueOrRecover_WithRecovery_OnFailure_UsesException()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var result = tryValue.GetValueOrRecover(ex => ex.Message.Length);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var tryValue = Try<int>.Success(42);
        var mapped = tryValue.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void Map_OnFailure_PreservesFailure()
    {
        var exception = new Exception("error");
        var tryValue = Try<int>.Failure(exception);
        var mapped = tryValue.Map(x => x * 2);

        Assert.True(mapped.IsError);
        Assert.Equal(exception, mapped.GetException());
    }

    [Fact]
    public void Map_ThatThrows_CapturesException()
    {
        var tryValue = Try<int>.Success(42);
        var mapped = tryValue.Map<int>(x => throw new InvalidOperationException("map error"));

        Assert.True(mapped.IsError);
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

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void FlatMap_OnSuccess_ChainsCorrectly()
    {
        var tryValue = Try<int>.Success(42);
        var result = tryValue.Bind(x => Try<string>.Success(x.ToString()));

        Assert.True(result.IsOk);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public void FlatMap_OnFailure_PreservesFailure()
    {
        var exception = new Exception("error");
        var tryValue = Try<int>.Failure(exception);
        var result = tryValue.Bind(x => Try<string>.Success(x.ToString()));

        Assert.True(result.IsError);
        Assert.Equal(exception, result.GetException());
    }

    [Fact]
    public void Zip_BothSuccess_ReturnsTuple()
    {
        var try1 = Try<int>.Success(42);
        var try2 = Try<string>.Success("hello");

        var result = try1.Zip(try2);

        Assert.True(result.IsOk);
        Assert.Equal((42, "hello"), result.GetValue());
    }

    [Fact]
    public void Zip_FirstFailure_ReturnsFirstException()
    {
        var exception = new Exception("first error");
        var try1 = Try<int>.Failure(exception);
        var try2 = Try<string>.Success("hello");

        var result = try1.Zip(try2);

        Assert.True(result.IsError);
        Assert.Equal(exception, result.GetException());
    }

    [Fact]
    public void Zip_SecondFailure_ReturnsSecondException()
    {
        var exception = new Exception("second error");
        var try1 = Try<int>.Success(42);
        var try2 = Try<string>.Failure(exception);

        var result = try1.Zip(try2);

        Assert.True(result.IsError);
        Assert.Equal(exception, result.GetException());
    }

    [Fact]
    public void ZipWith_BothSuccess_ReturnsCombinedValue()
    {
        var try1 = Try<int>.Success(10);
        var try2 = Try<int>.Success(20);

        var result = try1.ZipWith(try2, (a, b) => a + b);

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void ZipWith_FirstFailure_ReturnsFirstException()
    {
        var exception = new Exception("first error");
        var try1 = Try<int>.Failure(exception);
        var try2 = Try<int>.Success(20);

        var result = try1.ZipWith(try2, (a, b) => a + b);

        Assert.True(result.IsError);
        Assert.Equal(exception, result.GetException());
    }

    [Fact]
    public void ZipWith_CombinerThrows_ReturnsFailure()
    {
        var try1 = Try<int>.Success(10);
        var try2 = Try<int>.Success(20);

        var result = try1.ZipWith<int, int>(try2, (a, b) => throw new InvalidOperationException("combiner failed"));

        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public void Recover_OnFailure_Recovers()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var recovered = tryValue.Recover(ex => 100);

        Assert.True(recovered.IsOk);
        Assert.Equal(100, recovered.GetValue());
    }

    [Fact]
    public void Recover_OnSuccess_ReturnsOriginal()
    {
        var tryValue = Try<int>.Success(42);
        var recovered = tryValue.Recover(ex => 100);

        Assert.Equal(42, recovered.GetValue());
    }

    [Fact]
    public void RecoverWith_OnFailure_Recovers()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var recovered = tryValue.RecoverWith(ex => Try<int>.Success(100));

        Assert.True(recovered.IsOk);
        Assert.Equal(100, recovered.GetValue());
    }

    [Fact]
    public void Filter_WithMatchingPredicate_ReturnsSuccess()
    {
        var tryValue = Try<int>.Success(42);
        var filtered = tryValue.Filter(x => x > 40);

        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void Filter_WithNonMatchingPredicate_ReturnsFailure()
    {
        var tryValue = Try<int>.Success(42);
        var filtered = tryValue.Filter(x => x < 40);

        Assert.True(filtered.IsError);
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
        Assert.Equal(42, option.GetValue());
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
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_OnFailure_ReturnsErr()
    {
        var exception = new Exception("error");
        var tryValue = Try<int>.Failure(exception);
        var result = tryValue.ToResult();

        Assert.True(result.IsError);
        Assert.Equal(exception, result.GetError());
    }

    [Fact]
    public void ToResult_WithMapper_TransformsException()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var result = tryValue.ToResult(ex => ex.Message);

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var tryValue = Try<int>.Success(42);
        var executed = false;

        var result = tryValue.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsOk);
    }

    [Fact]
    public void TapFailure_OnFailure_ExecutesAction()
    {
        var tryValue = Try<int>.Failure(new Exception("error"));
        var executed = false;

        var result = tryValue.TapFailure(ex => executed = true);

        Assert.True(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void Flatten_NestedSuccess_ReturnsInnerSuccess()
    {
        var nested = Try<Try<int>>.Success(Try<int>.Success(42));
        var flattened = nested.Flatten();

        Assert.True(flattened.IsOk);
        Assert.Equal(42, flattened.GetValue());
    }

    [Fact]
    public void Flatten_OuterFailure_ReturnsFailure()
    {
        var exception = new Exception("outer error");
        var nested = Try<Try<int>>.Failure(exception);
        var flattened = nested.Flatten();

        Assert.True(flattened.IsError);
        Assert.Equal(exception, flattened.GetException());
    }

    [Fact]
    public void ToTry_FromOk_ReturnsSuccess()
    {
        var result = Result<int, Exception>.Ok(42);
        var tryValue = result.ToTry();

        Assert.True(tryValue.IsOk);
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void ToTry_FromErr_ReturnsFailure()
    {
        var exception = new Exception("error");
        var result = Result<int, Exception>.Err(exception);
        var tryValue = result.ToTry();

        Assert.True(tryValue.IsError);
        Assert.Equal(exception, tryValue.GetException());
    }

    [Fact]
    public void RealWorld_ParseInt_Success()
    {
        var tryValue = Try<int>.Of(() => int.Parse("42"));

        Assert.True(tryValue.IsOk);
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void RealWorld_ParseInt_Failure()
    {
        var tryValue = Try<int>.Of(() => int.Parse("not a number"));

        Assert.True(tryValue.IsError);
        Assert.IsType<FormatException>(tryValue.GetException());
    }

    [Fact]
    public void RealWorld_ChainOperations()
    {
        var result = Try<int>.Of(() => int.Parse("42"))
            .Map(x => x * 2)
            .Filter(x => x > 50, "too small")
            .GetValueOr(0);

        Assert.Equal(84, result);
    }
}
