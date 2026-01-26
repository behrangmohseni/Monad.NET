using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for IO to improve code coverage.
/// </summary>
public class IOCoverageTests
{
    #region IO.Return Tests

    [Fact]
    public void Return_CreatesIOWithValue()
    {
        var io = IO<int>.Return(42);
        var result = io.Run();
        Assert.Equal(42, result);
    }

    #endregion

    #region IO.OrElse with Value Tests

    [Fact]
    public void OrElse_WithValue_Success_ReturnsOriginal()
    {
        var io = IO<int>.Of(() => 42);
        var result = io.OrElse(0).Run();
        Assert.Equal(42, result);
    }

    [Fact]
    public void OrElse_WithValue_Failure_ReturnsFallback()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException("error"));
        var result = io.OrElse(99).Run();
        Assert.Equal(99, result);
    }

    #endregion

    #region IO.Replicate Tests

    [Fact]
    public void Replicate_ExecutesMultipleTimes()
    {
        var counter = 0;
        var io = IO<int>.Of(() => ++counter);
        var results = io.Replicate(3).Run();

        Assert.Equal(3, results.Count);
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    [Fact]
    public void Replicate_ZeroCount_ReturnsEmptyList()
    {
        var io = IO<int>.Of(() => 42);
        var results = io.Replicate(0).Run();

        Assert.Empty(results);
    }

    [Fact]
    public void Replicate_NegativeCount_ThrowsArgumentOutOfRange()
    {
        var io = IO<int>.Of(() => 42);
        Assert.Throws<ArgumentOutOfRangeException>(() => io.Replicate(-1));
    }

    #endregion

    #region IO.Retry Tests

    [Fact]
    public void Retry_SucceedsOnFirstAttempt_ReturnsValue()
    {
        var io = IO<int>.Of(() => 42);
        var result = io.Retry(3).Run();
        Assert.Equal(42, result);
    }

    [Fact]
    public void Retry_FailsAllAttempts_Throws()
    {
        var io = IO<int>.Of(() => throw new InvalidOperationException("error"));
        Assert.Throws<InvalidOperationException>(() => io.Retry(2).Run());
    }

    [Fact]
    public void Retry_SucceedsOnSecondAttempt_ReturnsValue()
    {
        var attempts = 0;
        var io = IO<int>.Of(() =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("error");
            return 42;
        });

        var result = io.Retry(3).Run();
        Assert.Equal(42, result);
        Assert.Equal(2, attempts);
    }

    #endregion

    #region IOAsync.RetryWhile Tests

    [Fact]
    public async Task RetryWhile_SucceedsOnFirstAttempt_ReturnsValue()
    {
        var io = IOAsync<int>.Of(() => Task.FromResult(42));
        var result = await io.RetryWhile((ex, attempt) => attempt < 3).RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task RetryWhile_FailsButConditionIsFalse_Throws()
    {
        var io = IOAsync<int>.Of(() => throw new InvalidOperationException("error"));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await io.RetryWhile((ex, attempt) => false).RunAsync());
    }

    [Fact]
    public async Task RetryWhile_SucceedsOnSecondAttempt_ReturnsValue()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("error");
            return 42;
        });

        var result = await io.RetryWhile((ex, attempt) => attempt < 5).RunAsync();
        Assert.Equal(42, result);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task RetryWhile_ExceedsMaxRetries_Throws()
    {
        var io = IOAsync<int>.Of(() => throw new InvalidOperationException("error"));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await io.RetryWhile((ex, attempt) => true, maxRetries: 2).RunAsync());
    }

    #endregion

    #region IOAsync.RetryWithExponentialBackoff Tests

    [Fact]
    public async Task RetryWithExponentialBackoff_SucceedsOnFirstAttempt_ReturnsValue()
    {
        var io = IOAsync<int>.Of(() => Task.FromResult(42));
        var result = await io.RetryWithExponentialBackoff(3, TimeSpan.FromMilliseconds(1)).RunAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task RetryWithExponentialBackoff_SucceedsOnSecondAttempt_ReturnsValue()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("error");
            return 42;
        });

        var result = await io.RetryWithExponentialBackoff(3, TimeSpan.FromMilliseconds(1)).RunAsync();
        Assert.Equal(42, result);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task RetryWithExponentialBackoff_WithMaxDelay_RespectsMaxDelay()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException("error");
            return 42;
        });

        var result = await io.RetryWithExponentialBackoff(
            5,
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(5)).RunAsync();

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    #endregion

    #region IO Static Helper Methods Tests

    [Fact]
    public void IO_Execute_ExecutesAction()
    {
        var executed = false;
        var io = IO.Execute(() => executed = true);
        io.Run();
        Assert.True(executed);
    }

    [Fact]
    public void IO_Random_Range_ReturnsValueInRange()
    {
        var io = IO.Random(10, 20);
        var result = io.Run();
        Assert.InRange(result, 10, 19);
    }

    #endregion

    #region IOAsync Static Helper Methods Tests

    [Fact]
    public async Task IOAsync_Execute_ExecutesAction()
    {
        var executed = false;
        var io = IOAsync.Execute(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });
        await io.RunAsync();
        Assert.True(executed);
    }

    [Fact]
    public async Task IOAsync_Delay_DelaysExecution()
    {
        var io = IOAsync.Delay(TimeSpan.FromMilliseconds(10));
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await io.RunAsync();
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds >= 5); // Allow some tolerance
    }

    #endregion
}
