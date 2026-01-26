using Xunit;

namespace Monad.NET.Tests;

public class AsyncEnumerableExtensionsTests
{
    #region Option Extensions

    [Fact]
    public async Task ChooseAsync_FiltersAndUnwrapsSomeValues()
    {
        var source = CreateAsyncEnumerable(
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(2),
            Option<int>.None(),
            Option<int>.Some(3)
        );

        var result = await source.ChooseAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task ChooseAsync_WithSelector_MapsAndFilters()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source
            .ChooseAsync(x => x % 2 == 0 ? Option<int>.Some(x * 10) : Option<int>.None())
            .ToListAsync();

        Assert.Equal(new[] { 20, 40 }, result);
    }

    [Fact]
    public async Task ChooseAsync_WithAsyncSelector_MapsAndFilters()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source
            .ChooseAsync(async x =>
            {
                await Task.Yield();
                return x % 2 == 0 ? Option<int>.Some(x * 10) : Option<int>.None();
            })
            .ToListAsync();

        Assert.Equal(new[] { 20, 40 }, result);
    }

    [Fact]
    public async Task FirstOrNoneAsync_WithElements_ReturnsSome()
    {
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source.FirstOrNoneAsync();

        Assert.True(result.IsSome);
        Assert.Equal(1, result.GetValue());
    }

    [Fact]
    public async Task FirstOrNoneAsync_EmptySequence_ReturnsNone()
    {
        var source = CreateAsyncEnumerable<int>();

        var result = await source.FirstOrNoneAsync();

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FirstOrNoneAsync_WithPredicate_ReturnsMatchingElement()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source.FirstOrNoneAsync(x => x > 3);

        Assert.True(result.IsSome);
        Assert.Equal(4, result.GetValue());
    }

    [Fact]
    public async Task FirstOrNoneAsync_WithPredicateNoMatch_ReturnsNone()
    {
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source.FirstOrNoneAsync(x => x > 10);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task LastOrNoneAsync_WithElements_ReturnsSome()
    {
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source.LastOrNoneAsync();

        Assert.True(result.IsSome);
        Assert.Equal(3, result.GetValue());
    }

    [Fact]
    public async Task LastOrNoneAsync_EmptySequence_ReturnsNone()
    {
        var source = CreateAsyncEnumerable<int>();

        var result = await source.LastOrNoneAsync();

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task SequenceAsync_AllSome_ReturnsSomeWithList()
    {
        var source = CreateAsyncEnumerable(
            Option<int>.Some(1),
            Option<int>.Some(2),
            Option<int>.Some(3)
        );

        var result = await source.SequenceAsync();

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public async Task SequenceAsync_ContainsNone_ReturnsNone()
    {
        var source = CreateAsyncEnumerable(
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3)
        );

        var result = await source.SequenceAsync();

        Assert.True(result.IsNone);
    }

    #endregion

    #region Result Extensions

    [Fact]
    public async Task CollectOkAsync_FiltersAndUnwrapsOkValues()
    {
        var source = CreateAsyncEnumerable(
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(3)
        );

        var result = await source.CollectOkAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task CollectErrAsync_FiltersAndUnwrapsErrValues()
    {
        var source = CreateAsyncEnumerable(
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("error2")
        );

        var result = await source.CollectErrAsync().ToListAsync();

        Assert.Equal(new[] { "error1", "error2" }, result);
    }

    [Fact]
    public async Task PartitionAsync_SeparatesOksAndErrs()
    {
        var source = CreateAsyncEnumerable(
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(3)
        );

        var (oks, errs) = await source.PartitionAsync();

        Assert.Equal(new[] { 1, 2, 3 }, oks);
        Assert.Equal(new[] { "error1", "error2" }, errs);
    }

    [Fact]
    public async Task SequenceAsync_Result_AllOk_ReturnsOkWithList()
    {
        var source = CreateAsyncEnumerable(
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        );

        var result = await source.SequenceAsync();

        Assert.True(result.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public async Task SequenceAsync_Result_ContainsErr_ReturnsFirstErr()
    {
        var source = CreateAsyncEnumerable(
            Result<int, string>.Ok(1),
            Result<int, string>.Err("first error"),
            Result<int, string>.Err("second error"),
            Result<int, string>.Ok(3)
        );

        var result = await source.SequenceAsync();

        Assert.True(result.IsErr);
        Assert.Equal("first error", result.GetError());
    }

    #endregion

    #region Try Extensions

    [Fact]
    public async Task CollectSuccessAsync_FiltersAndUnwrapsSuccessValues()
    {
        var source = CreateAsyncEnumerable(
            Try<int>.Success(1),
            Try<int>.Failure(new Exception("error")),
            Try<int>.Success(2),
            Try<int>.Success(3)
        );

        var result = await source.CollectSuccessAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task CollectFailureAsync_FiltersAndUnwrapsExceptions()
    {
        var ex1 = new InvalidOperationException("error1");
        var ex2 = new ArgumentException("error2");

        var source = CreateAsyncEnumerable(
            Try<int>.Success(1),
            Try<int>.Failure(ex1),
            Try<int>.Success(2),
            Try<int>.Failure(ex2)
        );

        var result = await source.CollectFailureAsync().ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Same(ex1, result[0]);
        Assert.Same(ex2, result[1]);
    }

    #endregion

    #region General Extensions

    [Fact]
    public async Task SelectAsync_TransformsElements()
    {
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source
            .SelectAsync(async x =>
            {
                await Task.Yield();
                return x * 2;
            })
            .ToListAsync();

        Assert.Equal(new[] { 2, 4, 6 }, result);
    }

    [Fact]
    public async Task WhereAsync_FiltersWithAsyncPredicate()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source
            .WhereAsync(async x =>
            {
                await Task.Yield();
                return x % 2 == 0;
            })
            .ToListAsync();

        Assert.Equal(new[] { 2, 4 }, result);
    }

    [Fact]
    public async Task TapAsync_ExecutesSideEffect()
    {
        var sideEffects = new List<int>();
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source
            .TapAsync(async x =>
            {
                await Task.Yield();
                sideEffects.Add(x);
            })
            .ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result);
        Assert.Equal(new[] { 1, 2, 3 }, sideEffects);
    }

    [Fact]
    public async Task ToListAsync_ConvertsToList()
    {
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source.ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task CountAsync_CountsElements()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source.CountAsync();

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task AnyAsync_WithMatch_ReturnsTrue()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source.AnyAsync(x => x > 3);

        Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_NoMatch_ReturnsFalse()
    {
        var source = CreateAsyncEnumerable(1, 2, 3);

        var result = await source.AnyAsync(x => x > 10);

        Assert.False(result);
    }

    [Fact]
    public async Task AllAsync_AllMatch_ReturnsTrue()
    {
        var source = CreateAsyncEnumerable(2, 4, 6, 8);

        var result = await source.AllAsync(x => x % 2 == 0);

        Assert.True(result);
    }

    [Fact]
    public async Task AllAsync_SomeNotMatch_ReturnsFalse()
    {
        var source = CreateAsyncEnumerable(2, 4, 5, 8);

        var result = await source.AllAsync(x => x % 2 == 0);

        Assert.False(result);
    }

    [Fact]
    public async Task AggregateAsync_AccumulatesValues()
    {
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);

        var result = await source.AggregateAsync(0, (acc, x) => acc + x);

        Assert.Equal(15, result);
    }

    #endregion

    #region Helpers

    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }

    #endregion
}

