using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Additional tests for Result to improve code coverage.
/// </summary>
public class ResultCoverageTests
{
    #region GetOrThrow with Message Tests

    [Fact]
    public void GetOrThrow_WithMessage_Ok_ReturnsValue()
    {
        var result = Result<int, string>.Ok(42);
        var value = result.GetOrThrow("Should not fail");
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetOrThrow_WithMessage_Err_ThrowsWithMessage()
    {
        var result = Result<int, string>.Err("original error");
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("original error", ex.Message);
    }

    #endregion

    #region GetErrorOrThrow with Message Tests

    [Fact]
    public void GetErrorOrThrow_WithMessage_Err_ReturnsError()
    {
        var result = Result<int, string>.Err("error");
        var error = result.GetErrorOrThrow("Should not fail");
        Assert.Equal("error", error);
    }

    [Fact]
    public void GetErrorOrThrow_WithMessage_Ok_ThrowsWithMessage()
    {
        var result = Result<int, string>.Ok(42);
        var ex = Assert.Throws<InvalidOperationException>(() => result.GetErrorOrThrow("Custom message"));
        Assert.Contains("Custom message", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    #endregion

    #region Filter with Error Factory Tests

    [Fact]
    public void Filter_WithErrorFactory_Ok_PredicateTrue_ReturnsOriginal()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Filter(x => x > 40, () => "too small");
        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void Filter_WithErrorFactory_Ok_PredicateFalse_ReturnsErr()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Filter(x => x > 50, () => "too small");
        Assert.True(filtered.IsErr);
        Assert.Equal("too small", filtered.GetError());
    }

    [Fact]
    public void Filter_WithErrorFactory_Err_ReturnsErr()
    {
        var result = Result<int, string>.Err("original error");
        var filtered = result.Filter(x => x > 50, () => "too small");
        Assert.True(filtered.IsErr);
        // Filter on Err returns error from factory, not original
        Assert.Equal("too small", filtered.GetError());
    }

    #endregion

    #region Filter with Value-Based Error Factory Tests

    [Fact]
    public void Filter_WithValueErrorFactory_Ok_PredicateTrue_ReturnsOriginal()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Filter(x => x > 40, x => $"Value {x} is too small");
        Assert.True(filtered.IsOk);
        Assert.Equal(42, filtered.GetValue());
    }

    [Fact]
    public void Filter_WithValueErrorFactory_Ok_PredicateFalse_ReturnsErrWithValue()
    {
        var result = Result<int, string>.Ok(42);
        var filtered = result.Filter(x => x > 50, x => $"Value {x} is too small");
        Assert.True(filtered.IsErr);
        Assert.Equal("Value 42 is too small", filtered.GetError());
    }

    [Fact]
    public void Filter_WithValueErrorFactory_Err_ReturnsErr()
    {
        var result = Result<int, string>.Err("original error");
        var filtered = result.Filter(x => x > 50, x => $"Value {x} is too small");
        Assert.True(filtered.IsErr);
        Assert.Equal("original error", filtered.GetError());
    }

    #endregion

    #region IComparable Tests

    [Fact]
    public void CompareTo_Object_Null_ReturnsPositive()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Equal(1, ((IComparable)result).CompareTo(null));
    }

    [Fact]
    public void CompareTo_Object_InvalidType_ThrowsArgumentException()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Throws<ArgumentException>(() => ((IComparable)result).CompareTo("not a Result"));
    }

    [Fact]
    public void CompareTo_Object_ValidResult_ComparesCorrectly()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(50);
        Assert.True(((IComparable)result1).CompareTo(result2) < 0);
    }

    #endregion

    #region Combine Collection Tests

    [Fact]
    public void Combine_Collection_AllOk_ReturnsCombinedList()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.Combine(results);
        Assert.True(combined.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, combined.GetValue());
    }

    [Fact]
    public void Combine_Collection_HasErr_ReturnsFirstErr()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.Combine(results);
        Assert.True(combined.IsErr);
        Assert.Equal("error", combined.GetError());
    }

    [Fact]
    public void CombineAll_Collection_AllOk_ReturnsUnit()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.CombineAll(results);
        Assert.True(combined.IsOk);
        Assert.Equal(Unit.Value, combined.GetValue());
    }

    [Fact]
    public void CombineAll_Collection_HasErr_ReturnsFirstErr()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.CombineAll(results);
        Assert.True(combined.IsErr);
        Assert.Equal("error", combined.GetError());
    }

    #endregion

    #region Async Combine Tests

    [Fact]
    public async Task CombineAsync_ThreeTasks_AllOk_ReturnsCombined()
    {
        var task1 = Task.FromResult(Result<int, string>.Ok(1));
        var task2 = Task.FromResult(Result<int, string>.Ok(2));
        var task3 = Task.FromResult(Result<int, string>.Ok(3));

        var combined = await ResultExtensions.CombineAsync(task1, task2, task3);
        Assert.True(combined.IsOk);
        Assert.Equal((1, 2, 3), combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_ThreeTasks_WithCombiner_AllOk_ReturnsCombined()
    {
        var task1 = Task.FromResult(Result<int, string>.Ok(1));
        var task2 = Task.FromResult(Result<int, string>.Ok(2));
        var task3 = Task.FromResult(Result<int, string>.Ok(3));

        var combined = await ResultExtensions.CombineAsync(task1, task2, task3, (a, b, c) => a + b + c);
        Assert.True(combined.IsOk);
        Assert.Equal(6, combined.GetValue());
    }

    [Fact]
    public async Task CombineAsync_Collection_AllOk_ReturnsCombinedList()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        var combined = await ResultExtensions.CombineAsync(tasks);
        Assert.True(combined.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, combined.GetValue());
    }

    [Fact]
    public async Task CombineAllAsync_Collection_AllOk_ReturnsUnit()
    {
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        var combined = await ResultExtensions.CombineAllAsync(tasks);
        Assert.True(combined.IsOk);
        Assert.Equal(Unit.Value, combined.GetValue());
    }

    #endregion
}
