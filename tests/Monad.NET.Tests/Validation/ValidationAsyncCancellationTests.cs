using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Tests for Validation async methods with CancellationToken to improve code coverage.
/// </summary>
public class ValidationAsyncCancellationTests
{
    #region TapAsync with CancellationToken Tests

    [Fact]
    public async Task TapAsync_WithCancellationToken_Valid_ExecutesAction()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Valid(42));
        var capturedValue = 0;

        var result = await validationTask.TapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            capturedValue = x;
        }, CancellationToken.None);

        Assert.Equal(42, capturedValue);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task TapAsync_WithCancellationToken_Invalid_DoesNotExecuteAction()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Invalid("error"));
        var executed = false;

        var result = await validationTask.TapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            executed = true;
        }, CancellationToken.None);

        Assert.False(executed);
        Assert.True(result.IsInvalid);
    }

    #endregion

    #region TapErrorsAsync with CancellationToken Tests

    [Fact]
    public async Task TapErrorsAsync_WithCancellationToken_Invalid_ExecutesAction()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Invalid("error"));
        var capturedErrors = new List<string>();

        var result = await validationTask.TapErrorsAsync(async (errors, ct) =>
        {
            await Task.Delay(1, ct);
            capturedErrors.AddRange(errors);
        }, CancellationToken.None);

        Assert.Single(capturedErrors);
        Assert.Equal("error", capturedErrors[0]);
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public async Task TapErrorsAsync_WithCancellationToken_Valid_DoesNotExecuteAction()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Valid(42));
        var executed = false;

        var result = await validationTask.TapErrorsAsync(async (errors, ct) =>
        {
            await Task.Delay(1, ct);
            executed = true;
        }, CancellationToken.None);

        Assert.False(executed);
        Assert.True(result.IsValid);
    }

    #endregion

    #region MapAsync with CancellationToken Tests

    [Fact]
    public async Task MapAsync_Validation_WithCancellationToken_Valid_TransformsValue()
    {
        var validation = Validation<int, string>.Valid(21);
        var mapped = await validation.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsValid);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_Validation_WithCancellationToken_Invalid_ReturnsInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        var mapped = await validation.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsInvalid);
        Assert.Equal("error", mapped.GetErrors()[0]);
    }

    [Fact]
    public async Task MapAsync_ValidationTask_WithCancellationToken_Valid_TransformsValue()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Valid(21));
        var mapped = await validationTask.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsValid);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public async Task MapAsync_ValidationTask_WithCancellationToken_Invalid_ReturnsInvalid()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Invalid("error"));
        var mapped = await validationTask.MapAsync(async (x, ct) =>
        {
            await Task.Delay(1, ct);
            return x * 2;
        }, CancellationToken.None);

        Assert.True(mapped.IsInvalid);
    }

    #endregion

    #region ApplyAsync with CancellationToken Tests

    [Fact]
    public async Task ApplyAsync_WithCancellationToken_BothValid_AppliesCombiner()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Valid(10));

        var result = await firstTask.ApplyAsync(
            async (value, ct) =>
            {
                await Task.Delay(1, ct);
                return Validation<int, string>.Valid(value + 5);
            },
            (a, b) => a + b,
            CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(25, result.GetValue()); // 10 + (10 + 5)
    }

    [Fact]
    public async Task ApplyAsync_WithCancellationToken_FirstInvalid_ReturnsInvalid()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Invalid("first error"));

        var result = await firstTask.ApplyAsync(
            async (value, ct) =>
            {
                await Task.Delay(1, ct);
                return Validation<int, string>.Valid(value + 5);
            },
            (a, b) => a + b,
            CancellationToken.None);

        Assert.True(result.IsInvalid);
        Assert.Equal("first error", result.GetErrors()[0]);
    }

    [Fact]
    public async Task ApplyAsync_WithCancellationToken_SecondInvalid_ReturnsInvalid()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Valid(10));

        var result = await firstTask.ApplyAsync(
            async (value, ct) =>
            {
                await Task.Delay(1, ct);
                return Validation<int, string>.Invalid("second error");
            },
            (a, b) => a + b,
            CancellationToken.None);

        Assert.True(result.IsInvalid);
        Assert.Equal("second error", result.GetErrors()[0]);
    }

    #endregion

    #region ZipAsync with CancellationToken Tests

    [Fact]
    public async Task ZipAsync_WithCancellationToken_BothValid_ReturnsTuple()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Valid(10));

        var result = await firstTask.ZipAsync(
            async ct =>
            {
                await Task.Delay(1, ct);
                return Validation<string, string>.Valid("hello");
            },
            CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal((10, "hello"), result.GetValue());
    }

    [Fact]
    public async Task ZipAsync_WithCancellationToken_FirstInvalid_ReturnsInvalid()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Invalid("first error"));

        var result = await firstTask.ZipAsync(
            async ct =>
            {
                await Task.Delay(1, ct);
                return Validation<string, string>.Valid("hello");
            },
            CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    #endregion

    #region ZipWithAsync with CancellationToken Tests

    [Fact]
    public async Task ZipWithAsync_WithCancellationToken_BothValid_CombinesValues()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Valid(10));

        var result = await firstTask.ZipWithAsync(
            async ct =>
            {
                await Task.Delay(1, ct);
                return Validation<int, string>.Valid(32);
            },
            (a, b) => a + b,
            CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public async Task ZipWithAsync_WithCancellationToken_FirstInvalid_ReturnsInvalid()
    {
        var firstTask = Task.FromResult(Validation<int, string>.Invalid("first error"));

        var result = await firstTask.ZipWithAsync(
            async ct =>
            {
                await Task.Delay(1, ct);
                return Validation<int, string>.Valid(32);
            },
            (a, b) => a + b,
            CancellationToken.None);

        Assert.True(result.IsInvalid);
    }

    #endregion
}
