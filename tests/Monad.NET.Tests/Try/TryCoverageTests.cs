using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for Try to improve code coverage.
/// </summary>
public class TryCoverageTests
{
    #region GetOrThrow with Message Tests

    [Fact]
    public void GetOrThrow_WithMessage_Success_ReturnsValue()
    {
        var result = Try<int>.Success(42);
        var value = result.GetOrThrow("Should not fail");
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetOrThrow_WithMessage_Failure_ThrowsWithMessage()
    {
        var result = Try<int>.Failure(new InvalidOperationException("original error"));
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("original error", ex.Message);
    }

    #endregion

    #region GetExceptionOrThrow with Message Tests

    [Fact]
    public void GetExceptionOrThrow_WithMessage_Failure_ReturnsException()
    {
        var exception = new InvalidOperationException("test error");
        var result = Try<int>.Failure(exception);
        var ex = result.GetExceptionOrThrow("Should not fail");
        Assert.Same(exception, ex);
    }

    [Fact]
    public void GetExceptionOrThrow_WithMessage_Success_ThrowsWithMessage()
    {
        var result = Try<int>.Success(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetExceptionOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    #endregion

    #region OfAsync with CancellationToken Tests

    [Fact]
    public async Task OfAsync_WithCancellationToken_Success_ReturnsSuccess()
    {
        var result = await Try<int>.OfAsync(async ct =>
        {
            await Task.Delay(1, ct);
            return 42;
        }, CancellationToken.None);

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task OfAsync_WithCancellationToken_Exception_ReturnsFailure()
    {
        var result = await Try<int>.OfAsync(async ct =>
        {
            await Task.Delay(1, ct);
            throw new InvalidOperationException("test error");
        }, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public async Task OfAsync_WithCancellation_ThrowsCancelledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await Try<int>.OfAsync(async ct =>
            {
                await Task.Delay(100, ct);
                return 42;
            }, cts.Token));
    }

    #endregion

    #region MapAsync with CancellationToken Tests

    [Fact]
    public async Task MapAsync_WithCancellationToken_Success_TransformsValue()
    {
        var result = Try<int>.Success(21);
        var mapped = await result.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_WithCancellationToken_Failure_ReturnsFailure()
    {
        var result = Try<int>.Failure(new InvalidOperationException("error"));
        var mapped = await result.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsError);
    }

    [Fact]
    public async Task MapAsync_WithCancellationToken_MapperThrows_ReturnsFailure()
    {
        var result = Try<int>.Success(42);
        var mapped = await result.MapAsync<int, int>(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            throw new InvalidOperationException("mapper error");
        }, CancellationToken.None);

        Assert.True(mapped.IsError);
        Assert.IsType<InvalidOperationException>(mapped.GetException());
    }

    #endregion

    #region BindAsync with CancellationToken Tests

    [Fact]
    public async Task BindAsync_WithCancellationToken_Success_Chains()
    {
        var result = Try<int>.Success(21);
        var chained = await result.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Try<int>.Success(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsOk);
        Assert.Equal(42, chained.GetValue());
    }

    [Fact]
    public async Task BindAsync_WithCancellationToken_Failure_ReturnsFailure()
    {
        var result = Try<int>.Failure(new InvalidOperationException("error"));
        var chained = await result.BindAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return Try<int>.Success(x * 2);
        }, CancellationToken.None);

        Assert.True(chained.IsError);
    }

    [Fact]
    public async Task BindAsync_WithCancellationToken_BinderThrows_ReturnsFailure()
    {
        var result = Try<int>.Success(42);
        var chained = await result.BindAsync<int, int>(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            throw new InvalidOperationException("binder error");
        }, CancellationToken.None);

        Assert.True(chained.IsError);
        Assert.IsType<InvalidOperationException>(chained.GetException());
    }

    #endregion
}
