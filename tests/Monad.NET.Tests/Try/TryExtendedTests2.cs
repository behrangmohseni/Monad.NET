using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for Try<T> to improve code coverage.
/// </summary>
public class TryExtendedTests2
{
    #region Factory Tests

    [Fact]
    public void Success_CreatesSuccessInstance()
    {
        var @try = Try<int>.Ok(42);

        Assert.True(@try.IsOk);
        Assert.Equal(42, @try.GetValue());
    }

    [Fact]
    public void Failure_CreatesFailureInstance()
    {
        var ex = new InvalidOperationException("error");
        var @try = Try<int>.Error(ex);

        Assert.True(@try.IsError);
        Assert.Same(ex, @try.GetException());
    }

    [Fact]
    public void Of_WithSuccessfulFunc_ReturnsSuccess()
    {
        var @try = Try<int>.Of(() => 42);

        Assert.True(@try.IsOk);
        Assert.Equal(42, @try.GetValue());
    }

    [Fact]
    public void Of_WithThrowingFunc_ReturnsFailure()
    {
        var @try = Try<int>.Of(() => throw new InvalidOperationException("test"));

        Assert.True(@try.IsError);
        Assert.IsType<InvalidOperationException>(@try.GetException());
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Map(x => x * 2);

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Map_OnFailure_ReturnsFailure()
    {
        var ex = new InvalidOperationException("error");
        var @try = Try<int>.Error(ex);
        var result = @try.Map(x => x * 2);

        Assert.True(result.IsError);
    }

    [Fact]
    public void Map_WithThrowingMapper_ReturnsFailure()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Map<int>(x => throw new InvalidOperationException("mapper error"));

        Assert.True(result.IsError);
    }

    #endregion

    #region FlatMap Tests

    [Fact]
    public void FlatMap_OnSuccess_Chains()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Bind(x => Try<string>.Ok(x.ToString()));

        Assert.True(result.IsOk);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public void FlatMap_OnFailure_ReturnsFailure()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var result = @try.Bind(x => Try<string>.Ok(x.ToString()));

        Assert.True(result.IsError);
    }

    [Fact]
    public void FlatMap_SuccessToFailure_ReturnsFailure()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Bind(_ => Try<string>.Error(new InvalidOperationException("chained error")));

        Assert.True(result.IsError);
    }

    #endregion

    #region Recover Tests

    [Fact]
    public void Recover_OnFailure_RecoversWith()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var result = @try.Recover(_ => 99);

        Assert.True(result.IsOk);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void Recover_OnSuccess_ReturnsOriginal()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Recover(_ => 99);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void RecoverWith_OnFailure_RecoversWith()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var result = @try.RecoverWith(_ => Try<int>.Ok(99));

        Assert.True(result.IsOk);
        Assert.Equal(99, result.GetValue());
    }

    [Fact]
    public void RecoverWith_OnSuccess_ReturnsOriginal()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.RecoverWith(_ => Try<int>.Ok(99));

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    #endregion

    #region Filter Tests

    [Fact]
    public void Filter_OnSuccess_PredicatePasses_ReturnsSuccess()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Filter(x => x > 40);

        Assert.True(result.IsOk);
    }

    [Fact]
    public void Filter_OnSuccess_PredicateFails_ReturnsFailure()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Filter(x => x > 50);

        Assert.True(result.IsError);
    }

    [Fact]
    public void Filter_OnFailure_ReturnsFailure()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var result = @try.Filter(x => x > 40);

        Assert.True(result.IsError);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var @try = Try<int>.Ok(42);
        var executed = false;

        var result = @try.Tap(x => executed = true);

        Assert.True(executed);
        Assert.True(result.IsOk);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteAction()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var executed = false;

        var result = @try.Tap(x => executed = true);

        Assert.False(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void TapFailure_OnFailure_ExecutesAction()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var executed = false;

        var result = @try.TapError(ex => executed = true);

        Assert.True(executed);
        Assert.True(result.IsError);
    }

    [Fact]
    public void TapFailure_OnSuccess_DoesNotExecuteAction()
    {
        var @try = Try<int>.Ok(42);
        var executed = false;

        var result = @try.TapError(ex => executed = true);

        Assert.False(executed);
        Assert.True(result.IsOk);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessFunc()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.Match(x => $"Value: {x}", ex => $"Error: {ex.Message}");

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailureFunc()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var result = @try.Match(x => $"Value: {x}", ex => $"Error: {ex.Message}");

        Assert.Equal("Error: error", result);
    }

    [Fact]
    public void Match_WithActions_OnSuccess_ExecutesSuccessAction()
    {
        var @try = Try<int>.Ok(42);
        var capturedValue = 0;

        @try.Match(x => capturedValue = x, ex => { });

        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public void Match_WithActions_OnFailure_ExecutesFailureAction()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var capturedMessage = "";

        @try.Match(x => { }, ex => capturedMessage = ex.Message);

        Assert.Equal("error", capturedMessage);
    }

    #endregion

    #region ToResult Tests

    [Fact]
    public void ToResult_OnSuccess_ReturnsOk()
    {
        var @try = Try<int>.Ok(42);
        var result = @try.ToResult();

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void ToResult_OnFailure_ReturnsErr()
    {
        var ex = new InvalidOperationException("error");
        var @try = Try<int>.Error(ex);
        var result = @try.ToResult();

        Assert.True(result.IsError);
        Assert.Same(ex, result.GetError());
    }

    #endregion

    #region GetOrElse Tests

    [Fact]
    public void GetOrElse_OnSuccess_ReturnsValue()
    {
        var @try = Try<int>.Ok(42);
        Assert.Equal(42, @try.GetValueOr(99));
    }

    [Fact]
    public void GetOrElse_OnFailure_ReturnsDefault()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        Assert.Equal(99, @try.GetValueOr(99));
    }

    [Fact]
    public void Match_WithFunc_OnSuccess_ReturnsValue()
    {
        var @try = Try<int>.Ok(42);
        var factoryExecuted = false;
        var value = @try.Match(
            ok => ok,
            _ =>
            {
                factoryExecuted = true;
                return 99;
            });

        Assert.False(factoryExecuted);
        Assert.Equal(42, value);
    }

    [Fact]
    public void Match_WithFunc_OnFailure_ExecutesFactory()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var value = @try.Match(ok => ok, _ => 99);

        Assert.Equal(99, value);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SuccessWithSameValue_ReturnsTrue()
    {
        var t1 = Try<int>.Ok(42);
        var t2 = Try<int>.Ok(42);

        Assert.True(t1.Equals(t2));
        Assert.True(t1 == t2);
    }

    [Fact]
    public void Equals_SuccessWithDifferentValue_ReturnsFalse()
    {
        var t1 = Try<int>.Ok(42);
        var t2 = Try<int>.Ok(99);

        Assert.False(t1.Equals(t2));
        Assert.True(t1 != t2);
    }

    [Fact]
    public void Equals_SuccessWithFailure_ReturnsFalse()
    {
        var t1 = Try<int>.Ok(42);
        var t2 = Try<int>.Error(new InvalidOperationException("error"));

        Assert.False(t1.Equals(t2));
    }

    [Fact]
    public void GetHashCode_SameForEqualTries()
    {
        var t1 = Try<int>.Ok(42);
        var t2 = Try<int>.Ok(42);

        Assert.Equal(t1.GetHashCode(), t2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_OnSuccess_ContainsValue()
    {
        var @try = Try<int>.Ok(42);
        var str = @try.ToString();

        Assert.Contains("42", str);
        Assert.Contains("Success", str);
    }

    [Fact]
    public void ToString_OnFailure_ContainsFailure()
    {
        var @try = Try<int>.Error(new InvalidOperationException("error"));
        var str = @try.ToString();

        Assert.Contains("Failure", str);
    }

    #endregion
}

