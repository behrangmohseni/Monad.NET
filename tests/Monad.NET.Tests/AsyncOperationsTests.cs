using Xunit;

namespace Monad.NET.Tests;

public class AsyncOperationsTests
{
    #region Result.CombineAsync Tests

    [Fact]
    public async Task Result_CombineAsync_BothOk_ReturnsTuple()
    {
        var first = Task.FromResult(Result<int, string>.Ok(1));
        var second = Task.FromResult(Result<string, string>.Ok("hello"));

        var result = await ResultExtensions.CombineAsync(first, second);

        Assert.True(result.IsOk);
        Assert.Equal((1, "hello"), result.Unwrap());
    }

    [Fact]
    public async Task Result_CombineAsync_FirstErr_ReturnsErr()
    {
        var first = Task.FromResult(Result<int, string>.Err("error1"));
        var second = Task.FromResult(Result<string, string>.Ok("hello"));

        var result = await ResultExtensions.CombineAsync(first, second);

        Assert.True(result.IsErr);
        Assert.Equal("error1", result.UnwrapErr());
    }

    [Fact]
    public async Task Result_CombineAsync_SecondErr_ReturnsErr()
    {
        var first = Task.FromResult(Result<int, string>.Ok(1));
        var second = Task.FromResult(Result<string, string>.Err("error2"));

        var result = await ResultExtensions.CombineAsync(first, second);

        Assert.True(result.IsErr);
        Assert.Equal("error2", result.UnwrapErr());
    }

    [Fact]
    public async Task Result_CombineAsync_WithCombiner_BothOk_AppliesCombiner()
    {
        var first = Task.FromResult(Result<int, string>.Ok(10));
        var second = Task.FromResult(Result<int, string>.Ok(5));

        var result = await ResultExtensions.CombineAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsOk);
        Assert.Equal(15, result.Unwrap());
    }

    [Fact]
    public async Task Result_CombineAsync_ThreeTasks_AllOk_ReturnsTuple()
    {
        var first = Task.FromResult(Result<int, string>.Ok(1));
        var second = Task.FromResult(Result<string, string>.Ok("two"));
        var third = Task.FromResult(Result<double, string>.Ok(3.0));

        var result = await ResultExtensions.CombineAsync(first, second, third);

        Assert.True(result.IsOk);
        Assert.Equal((1, "two", 3.0), result.Unwrap());
    }

    [Fact]
    public async Task Result_CombineAsync_Collection_AllOk_ReturnsList()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        var result = await ResultExtensions.CombineAsync(tasks);

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public async Task Result_CombineAsync_Collection_OneErr_ReturnsErr()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("error")),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        var result = await ResultExtensions.CombineAsync(tasks);

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public async Task Result_CombineAllAsync_AllOk_ReturnsUnit()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2))
        };

        var result = await ResultExtensions.CombineAllAsync(tasks);

        Assert.True(result.IsOk);
        Assert.Equal(Unit.Value, result.Unwrap());
    }

    #endregion

    #region Validation Async Tests

    [Fact]
    public async Task Validation_ApplyAsync_BothValid_AppliesCombiner()
    {
        var first = Task.FromResult(Validation<int, string>.Valid(10));
        var second = Task.FromResult(Validation<int, string>.Valid(5));

        var result = await ValidationExtensions.ApplyAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsValid);
        Assert.Equal(15, result.Unwrap());
    }

    [Fact]
    public async Task Validation_ApplyAsync_BothInvalid_AccumulatesErrors()
    {
        var first = Task.FromResult(Validation<int, string>.Invalid("error1"));
        var second = Task.FromResult(Validation<int, string>.Invalid("error2"));

        var result = await ValidationExtensions.ApplyAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsInvalid);
        Assert.Equal(new[] { "error1", "error2" }, result.UnwrapErrors());
    }

    [Fact]
    public async Task Validation_ZipAsync_BothValid_ReturnsTuple()
    {
        var first = Task.FromResult(Validation<int, string>.Valid(1));
        var second = Task.FromResult(Validation<string, string>.Valid("hello"));

        var result = await ValidationExtensions.ZipAsync(first, second);

        Assert.True(result.IsValid);
        Assert.Equal((1, "hello"), result.Unwrap());
    }

    [Fact]
    public async Task Validation_ZipWithAsync_BothValid_AppliesCombiner()
    {
        var first = Task.FromResult(Validation<int, string>.Valid(10));
        var second = Task.FromResult(Validation<int, string>.Valid(5));

        var result = await ValidationExtensions.ZipWithAsync(first, second, (a, b) => a * b);

        Assert.True(result.IsValid);
        Assert.Equal(50, result.Unwrap());
    }

    [Fact]
    public async Task Validation_ZipAsync_ThreeTasks_AllValid_ReturnsTuple()
    {
        var first = Task.FromResult(Validation<int, string>.Valid(1));
        var second = Task.FromResult(Validation<string, string>.Valid("two"));
        var third = Task.FromResult(Validation<double, string>.Valid(3.0));

        var result = await ValidationExtensions.ZipAsync(first, second, third);

        Assert.True(result.IsValid);
        Assert.Equal((1, "two", 3.0), result.Unwrap());
    }

    [Fact]
    public async Task Validation_ZipAsync_ThreeTasks_AllInvalid_AccumulatesAllErrors()
    {
        var first = Task.FromResult(Validation<int, string>.Invalid("e1"));
        var second = Task.FromResult(Validation<string, string>.Invalid("e2"));
        var third = Task.FromResult(Validation<double, string>.Invalid("e3"));

        var result = await ValidationExtensions.ZipAsync(first, second, third);

        Assert.True(result.IsInvalid);
        Assert.Equal(new[] { "e1", "e2", "e3" }, result.UnwrapErrors());
    }

    [Fact]
    public async Task Validation_CombineAsync_AllValid_ReturnsList()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation<int, string>.Valid(1)),
            Task.FromResult(Validation<int, string>.Valid(2)),
            Task.FromResult(Validation<int, string>.Valid(3))
        };

        var result = await tasks.CombineAsync();

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 1, 2, 3 }, result.Unwrap());
    }

    [Fact]
    public async Task Validation_CombineAsync_SomeInvalid_AccumulatesErrors()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation<int, string>.Valid(1)),
            Task.FromResult(Validation<int, string>.Invalid("e1")),
            Task.FromResult(Validation<int, string>.Invalid("e2"))
        };

        var result = await tasks.CombineAsync();

        Assert.True(result.IsInvalid);
        Assert.Equal(new[] { "e1", "e2" }, result.UnwrapErrors());
    }

    [Fact]
    public async Task Validation_TapAsync_Valid_ExecutesAction()
    {
        var executed = false;
        var validation = Task.FromResult(Validation<int, string>.Valid(42));

        var result = await validation.TapAsync(async v =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validation_TapAsync_Invalid_DoesNotExecuteAction()
    {
        var executed = false;
        var validation = Task.FromResult(Validation<int, string>.Invalid("error"));

        var result = await validation.TapAsync(async v =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Validation_TapErrorsAsync_Invalid_ExecutesAction()
    {
        var capturedErrors = new List<string>();
        var validation = Task.FromResult(Validation<int, string>.Invalid(new[] { "e1", "e2" }));

        var result = await validation.TapErrorsAsync(async errors =>
        {
            await Task.Delay(1);
            capturedErrors.AddRange(errors);
        });

        Assert.Equal(new[] { "e1", "e2" }, capturedErrors);
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task Validation_MapAsync_Valid_MapsValue()
    {
        var validation = Validation<int, string>.Valid(5);

        var result = await validation.MapAsync(async v =>
        {
            await Task.Delay(1);
            return v * 2;
        });

        Assert.True(result.IsValid);
        Assert.Equal(10, result.Unwrap());
    }

    [Fact]
    public async Task Validation_MapAsync_FromTask_Valid_MapsValue()
    {
        var validation = Task.FromResult(Validation<int, string>.Valid(5));

        var result = await validation.MapAsync(async v =>
        {
            await Task.Delay(1);
            return v * 2;
        });

        Assert.True(result.IsValid);
        Assert.Equal(10, result.Unwrap());
    }

    #endregion

    #region Option Async Tests

    [Fact]
    public async Task Option_TapNoneAsync_None_ExecutesAction()
    {
        var executed = false;
        var option = Task.FromResult(Option<int>.None());

        var result = await option.TapNoneAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task Option_TapNoneAsync_Some_DoesNotExecuteAction()
    {
        var executed = false;
        var option = Task.FromResult(Option<int>.Some(42));

        var result = await option.TapNoneAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task Option_ZipAsync_BothSome_ReturnsTuple()
    {
        var first = Task.FromResult(Option<int>.Some(1));
        var second = Task.FromResult(Option<string>.Some("hello"));

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsSome);
        Assert.Equal((1, "hello"), result.Unwrap());
    }

    [Fact]
    public async Task Option_ZipAsync_FirstNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.None());
        var second = Task.FromResult(Option<string>.Some("hello"));

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task Option_ZipAsync_SecondNone_ReturnsNone()
    {
        var first = Task.FromResult(Option<int>.Some(1));
        var second = Task.FromResult(Option<string>.None());

        var result = await OptionAsyncExtensions.ZipAsync(first, second);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task Option_ZipWithAsync_BothSome_AppliesCombiner()
    {
        var first = Task.FromResult(Option<int>.Some(10));
        var second = Task.FromResult(Option<int>.Some(5));

        var result = await OptionAsyncExtensions.ZipWithAsync(first, second, (a, b) => a + b);

        Assert.True(result.IsSome);
        Assert.Equal(15, result.Unwrap());
    }

    [Fact]
    public async Task Option_FirstSomeAsync_FindsFirstSome()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.None()),
            Task.FromResult(Option<int>.Some(42)),
            Task.FromResult(Option<int>.Some(100))
        };

        var result = await tasks.FirstSomeAsync();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public async Task Option_FirstSomeAsync_AllNone_ReturnsNone()
    {
        var tasks = new[]
        {
            Task.FromResult(Option<int>.None()),
            Task.FromResult(Option<int>.None())
        };

        var result = await tasks.FirstSomeAsync();

        Assert.True(result.IsNone);
    }

    #endregion

    #region IOAsync Retry Tests

    [Fact]
    public async Task IOAsync_Retry_SucceedsOnFirstAttempt()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            await Task.Delay(1);
            return 42;
        });

        var result = await io.Retry(3).RunAsync();

        Assert.Equal(42, result);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task IOAsync_Retry_SucceedsAfterFailures()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            await Task.Delay(1);
            if (attempts < 3)
                throw new InvalidOperationException("Not yet");
            return 42;
        });

        var result = await io.Retry(3).RunAsync();

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task IOAsync_Retry_FailsAfterMaxRetries()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            await Task.Delay(1);
            throw new InvalidOperationException("Always fail");
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await io.Retry(2).RunAsync());

        Assert.Equal(3, attempts); // 1 initial + 2 retries
    }

    [Fact]
    public async Task IOAsync_RetryWithDelay_WaitsBetweenAttempts()
    {
        var attempts = 0;
        var timestamps = new List<DateTime>();

        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            timestamps.Add(DateTime.Now);
            await Task.Delay(1);
            if (attempts < 3)
                throw new InvalidOperationException("Not yet");
            return 42;
        });

        await io.RetryWithDelay(3, TimeSpan.FromMilliseconds(50)).RunAsync();

        Assert.Equal(3, attempts);
        // Verify there was some delay between attempts
        for (int i = 1; i < timestamps.Count; i++)
        {
            var delay = timestamps[i] - timestamps[i - 1];
            Assert.True(delay.TotalMilliseconds >= 40, $"Delay was {delay.TotalMilliseconds}ms");
        }
    }

    [Fact]
    public async Task IOAsync_RetryWithExponentialBackoff_IncreasesDelay()
    {
        var attempts = 0;
        var timestamps = new List<DateTime>();

        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            timestamps.Add(DateTime.Now);
            await Task.Delay(1);
            if (attempts < 4)
                throw new InvalidOperationException("Not yet");
            return 42;
        });

        await io.RetryWithExponentialBackoff(5, TimeSpan.FromMilliseconds(20)).RunAsync();

        Assert.Equal(4, attempts);
        // Each delay should roughly double (with some tolerance)
        if (timestamps.Count >= 3)
        {
            var delay1 = timestamps[1] - timestamps[0];
            var delay2 = timestamps[2] - timestamps[1];
            Assert.True(delay2.TotalMilliseconds >= delay1.TotalMilliseconds,
                $"delay2 ({delay2.TotalMilliseconds}ms) should be >= delay1 ({delay1.TotalMilliseconds}ms)");
        }
    }

    [Fact]
    public async Task IOAsync_RetryWhile_RetriesWhileConditionMet()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            await Task.Delay(1);
            throw new InvalidOperationException($"Attempt {attempts}");
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await io.RetryWhile((ex, attempt) => attempt < 3).RunAsync());

        Assert.Equal(4, attempts); // Initial + 3 retries (attempts 0, 1, 2, 3)
    }

    [Fact]
    public async Task IOAsync_RetryWhile_StopsOnConditionFailure()
    {
        var attempts = 0;
        var io = IOAsync<int>.Of(async () =>
        {
            attempts++;
            await Task.Delay(1);
            if (attempts == 2)
                throw new ArgumentException("Specific error");
            throw new InvalidOperationException("Generic error");
        });

        // Only retry for InvalidOperationException, not ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await io.RetryWhile((ex, _) => ex is InvalidOperationException).RunAsync());

        Assert.Equal(2, attempts);
    }

    #endregion
}

