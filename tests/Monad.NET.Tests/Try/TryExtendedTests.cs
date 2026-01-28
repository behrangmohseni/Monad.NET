using Xunit;

namespace Monad.NET.Tests;

public class TryExtendedTests
{
    #region Try<T> Additional Coverage

    [Fact]
    public void Success_CreatesSuccessfulTry()
    {
        var result = Try<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Failure_CreatesFailedTry()
    {
        var ex = new InvalidOperationException("test");
        var result = Try<int>.Failure(ex);

        Assert.True(result.IsFailure);
        Assert.Equal(ex, result.GetException());
    }

    [Fact]
    public void Of_CapturesException()
    {
        var result = Try<int>.Of(() => throw new InvalidOperationException("test"));

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public void Of_ReturnsValueOnSuccess()
    {
        var result = Try<int>.Of(() => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Of_WithNullFunc_ReturnsFailure()
    {
        // Try.Of captures exceptions, null func creates NullReferenceException
        var result = Try<int>.Of(null!);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task OfAsync_CapturesException()
    {
        var result = await Try<int>.OfAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("async error");
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task OfAsync_ReturnsValueOnSuccess()
    {
        var result = await Try<int>.OfAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OfAsync_WithNullFunc_ReturnsFailure()
    {
        // Try.OfAsync captures exceptions
        Func<Task<int>>? nullFunc = null;
        var result = await Try<int>.OfAsync(nullFunc!);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Get_ThrowsOnFailure()
    {
        var result = Try<int>.Failure(new Exception("test"));

        Assert.Throws<InvalidOperationException>(() => result.GetValue());
    }

    [Fact]
    public void GetException_ThrowsOnSuccess()
    {
        var result = Try<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() => result.GetException());
    }

    [Fact]
    public void GetOrElse_ReturnsValueOnSuccess()
    {
        var result = Try<int>.Success(42);

        Assert.Equal(42, result.GetValueOr(0));
    }

    [Fact]
    public void GetOrElse_ReturnsDefaultOnFailure()
    {
        var result = Try<int>.Failure(new Exception());

        Assert.Equal(0, result.GetValueOr(0));
    }

    [Fact]
    public void GetValueOrElse_WithFunc_ReturnsComputedOnFailure()
    {
        var result = Try<int>.Failure(new Exception());

        Assert.Equal(100, result.GetValueOrElse(() => 100));
    }

    [Fact]
    public void GetValueOrRecover_WithExFunc_ReturnsComputedFromException()
    {
        var result = Try<int>.Failure(new Exception("error"));

        Assert.Equal(5, result.GetValueOrRecover(ex => ex.Message.Length));
    }

    [Fact]
    public void Map_TransformsValue()
    {
        var result = Try<int>.Success(10).Map(x => x * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.GetValue());
    }

    [Fact]
    public void Map_PreservesFailure()
    {
        var ex = new Exception("test");
        var result = Try<int>.Failure(ex).Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Equal(ex, result.GetException());
    }

    [Fact]
    public void Map_WithNullMapper_ReturnsFailure()
    {
        var result = Try<int>.Success(10);
        var mapped = result.Map<int>(null!);
        // Try.Map captures exceptions from the mapper
        Assert.True(mapped.IsFailure);
    }

    [Fact]
    public void Map_CapturesExceptionInMapper()
    {
        var result = Try<int>.Success(10).Map<int>(x => throw new InvalidOperationException("map error"));

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public void FlatMap_ChainsSuccesses()
    {
        var result = Try<int>.Success(10)
            .Bind(x => Try<int>.Success(x * 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.GetValue());
    }

    [Fact]
    public void FlatMap_PropagatesFailure()
    {
        var ex = new Exception("first");
        var result = Try<int>.Failure(ex)
            .Bind(x => Try<int>.Success(x * 2));

        Assert.True(result.IsFailure);
        Assert.Equal(ex, result.GetException());
    }

    [Fact]
    public void FlatMap_WithNullBinder_ReturnsFailure()
    {
        var result = Try<int>.Success(10);
        var flatMapped = result.Bind<int>(null!);
        // Try.Bind captures exceptions from the binder
        Assert.True(flatMapped.IsFailure);
    }

    [Fact]
    public void Filter_KeepsMatchingValue()
    {
        var result = Try<int>.Success(10).Filter(x => x > 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.GetValue());
    }

    [Fact]
    public void Filter_RejectsNonMatchingValue()
    {
        var result = Try<int>.Success(10).Filter(x => x > 20);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Filter_WithMessage_RejectsWithCustomMessage()
    {
        var result = Try<int>.Success(10).Filter(x => x > 20, "Value too small");

        Assert.True(result.IsFailure);
        Assert.Contains("Value too small", result.GetException().Message);
    }

    [Fact]
    public void Filter_PreservesFailure()
    {
        var ex = new Exception("original");
        var result = Try<int>.Failure(ex).Filter(x => x > 5);

        Assert.True(result.IsFailure);
        Assert.Equal(ex, result.GetException());
    }

    [Fact]
    public void Recover_RecoversFromFailure()
    {
        var result = Try<int>.Failure(new Exception())
            .Recover(ex => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Recover_PreservesSuccess()
    {
        var result = Try<int>.Success(10)
            .Recover(ex => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.GetValue());
    }

    [Fact]
    public void Recover_CapturesExceptionInRecovery()
    {
        var result = Try<int>.Failure(new Exception())
            .Recover(ex => throw new InvalidOperationException("recovery failed"));

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public void RecoverWith_RecoversWithTry()
    {
        var result = Try<int>.Failure(new Exception())
            .RecoverWith(ex => Try<int>.Success(42));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void RecoverWith_PreservesSuccess()
    {
        var result = Try<int>.Success(10)
            .RecoverWith(ex => Try<int>.Success(42));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.GetValue());
    }

    [Fact]
    public void Match_ExecutesSuccessFunc()
    {
        var result = Try<int>.Success(42)
            .Match(v => $"Value: {v}", ex => $"Error: {ex.Message}");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_ExecutesFailureFunc()
    {
        var result = Try<int>.Failure(new Exception("error"))
            .Match(v => $"Value: {v}", ex => $"Error: {ex.Message}");

        Assert.Equal("Error: error", result);
    }

    [Fact]
    public void ToResult_ConvertsSuccessToOk()
    {
        var result = Try<int>.Success(42).ToResult(ex => ex.Message);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_ConvertsFailureToErr()
    {
        var result = Try<int>.Failure(new Exception("error")).ToResult(ex => ex.Message);

        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void ToOption_ConvertsSucessToSome()
    {
        var result = Try<int>.Success(42).ToOption();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToOption_ConvertsFailureToNone()
    {
        var result = Try<int>.Failure(new Exception()).ToOption();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Tap_ExecutesOnSuccess()
    {
        int captured = 0;
        var result = Try<int>.Success(42).Tap(v => captured = v);

        Assert.Equal(42, captured);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_DoesNotExecuteOnFailure()
    {
        int captured = 0;
        var result = Try<int>.Failure(new Exception()).Tap(v => captured = v);

        Assert.Equal(0, captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapFailure_ExecutesOnFailure()
    {
        Exception? captured = null;
        var ex = new Exception("test");
        var result = Try<int>.Failure(ex).TapFailure(e => captured = e);

        Assert.Equal(ex, captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapFailure_DoesNotExecuteOnSuccess()
    {
        Exception? captured = null;
        var result = Try<int>.Success(42).TapFailure(e => captured = e);

        Assert.Null(captured);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualSuccesses()
    {
        var t1 = Try<int>.Success(42);
        var t2 = Try<int>.Success(42);

        Assert.True(t1.Equals(t2));
        Assert.True(t1 == t2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var t1 = Try<int>.Success(42);
        var t2 = Try<int>.Success(43);

        Assert.False(t1.Equals(t2));
        Assert.True(t1 != t2);
    }

    [Fact]
    public void Equals_ReturnsFalseForSuccessAndFailure()
    {
        var t1 = Try<int>.Success(42);
        var t2 = Try<int>.Failure(new Exception());

        Assert.False(t1.Equals(t2));
    }

    [Fact]
    public void Equals_WithObject()
    {
        var t = Try<int>.Success(42);

        Assert.False(t.Equals("not a try"));
        Assert.True(t.Equals((object)Try<int>.Success(42)));
    }

    [Fact]
    public void GetHashCode_SameForEqualSuccesses()
    {
        var t1 = Try<int>.Success(42);
        var t2 = Try<int>.Success(42);

        Assert.Equal(t1.GetHashCode(), t2.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsSuccess()
    {
        var t = Try<int>.Success(42);
        var str = t.ToString();

        Assert.Contains("Success", str);
        Assert.Contains("42", str);
    }

    [Fact]
    public void ToString_FormatsFailure()
    {
        var t = Try<int>.Failure(new Exception("error"));
        var str = t.ToString();

        Assert.Contains("Failure", str);
    }

    #endregion

    #region TryExtensions

    [Fact]
    public void ToTry_FromResult_ConvertsOkToSuccess()
    {
        var result = Result<int, Exception>.Ok(42);
        var tryResult = result.ToTry();

        Assert.True(tryResult.IsSuccess);
        Assert.Equal(42, tryResult.GetValue());
    }

    [Fact]
    public void ToTry_FromResult_ConvertsErrToFailure()
    {
        var ex = new Exception("error");
        var result = Result<int, Exception>.Err(ex);
        var tryResult = result.ToTry();

        Assert.True(tryResult.IsFailure);
        Assert.Equal(ex, tryResult.GetException());
    }

    #endregion
}
