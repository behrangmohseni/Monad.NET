using Xunit;

namespace Monad.NET.Tests;

public class EnumerableExtensionsTests
{
    #region Do Tests

    [Fact]
    public void Do_ExecutesActionForEachElement()
    {
        var items = new[] { 1, 2, 3 };
        var captured = new List<int>();

        var result = items.Do(x => captured.Add(x)).ToList();

        Assert.Equal(new[] { 1, 2, 3 }, captured);
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Do_IsLazy()
    {
        var items = new[] { 1, 2, 3 };
        var captured = new List<int>();

        var enumerable = items.Do(x => captured.Add(x));

        // Action should not have been executed yet
        Assert.Empty(captured);

        // Now enumerate
        _ = enumerable.ToList();
        Assert.Equal(3, captured.Count);
    }

    [Fact]
    public void Do_CanChainMultipleOperations()
    {
        var items = new[] { 1, 2, 3 };
        var log1 = new List<int>();
        var log2 = new List<int>();

        var result = items
            .Do(x => log1.Add(x))
            .Where(x => x > 1)
            .Do(x => log2.Add(x))
            .ToList();

        Assert.Equal(new[] { 1, 2, 3 }, log1);
        Assert.Equal(new[] { 2, 3 }, log2);
        Assert.Equal(new[] { 2, 3 }, result);
    }

    [Fact]
    public void Do_WithIndex_PassesCorrectIndex()
    {
        var items = new[] { "a", "b", "c" };
        var captured = new List<(string, int)>();

        var result = items.Do((x, i) => captured.Add((x, i))).ToList();

        Assert.Equal(new[] { ("a", 0), ("b", 1), ("c", 2) }, captured);
    }

    [Fact]
    public void Do_ThrowsOnNullSource()
    {
        IEnumerable<int>? items = null;

        Assert.Throws<ArgumentNullException>(() => items!.Do(x => { }).ToList());
    }

    [Fact]
    public void Do_ThrowsOnNullAction()
    {
        var items = new[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => items.Do((Action<int>)null!).ToList());
    }

    #endregion

    #region ForEach Tests

    [Fact]
    public void ForEach_ExecutesActionForEachElement()
    {
        var items = new[] { 1, 2, 3 };
        var captured = new List<int>();

        items.ForEach(x => captured.Add(x));

        Assert.Equal(new[] { 1, 2, 3 }, captured);
    }

    [Fact]
    public void ForEach_WithIndex_PassesCorrectIndex()
    {
        var items = new[] { "a", "b", "c" };
        var captured = new List<(string, int)>();

        items.ForEach((x, i) => captured.Add((x, i)));

        Assert.Equal(new[] { ("a", 0), ("b", 1), ("c", 2) }, captured);
    }

    [Fact]
    public void ForEach_ThrowsOnNullSource()
    {
        IEnumerable<int>? items = null;

        Assert.Throws<ArgumentNullException>(() => items!.ForEach(x => { }));
    }

    [Fact]
    public void ForEach_ThrowsOnNullAction()
    {
        var items = new[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => items.ForEach((Action<int>)null!));
    }

    [Fact]
    public void ForEach_IsEager()
    {
        var items = new[] { 1, 2, 3 };
        var captured = new List<int>();

        items.ForEach(x => captured.Add(x));

        // All items should have been processed immediately
        Assert.Equal(3, captured.Count);
    }

    #endregion

    #region ForEachAsync Tests

    [Fact]
    public async Task ForEachAsync_ExecutesActionForEachElement()
    {
        var items = new[] { 1, 2, 3 };
        var captured = new List<int>();

        await items.ForEachAsync(async x =>
        {
            await Task.Delay(1);
            captured.Add(x);
        });

        Assert.Equal(new[] { 1, 2, 3 }, captured);
    }

    [Fact]
    public async Task ForEachAsync_WithIndex_PassesCorrectIndex()
    {
        var items = new[] { "a", "b", "c" };
        var captured = new List<(string, int)>();

        await items.ForEachAsync(async (x, i) =>
        {
            await Task.Delay(1);
            captured.Add((x, i));
        });

        Assert.Equal(new[] { ("a", 0), ("b", 1), ("c", 2) }, captured);
    }

    [Fact]
    public async Task ForEachAsync_RespectsСancellation()
    {
        var items = new[] { 1, 2, 3, 4, 5 };
        var captured = new List<int>();
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await items.ForEachAsync(async x =>
            {
                await Task.Delay(1);
                captured.Add(x);
                if (x == 2)
                    cts.Cancel();
            }, cts.Token);
        });

        // Should have processed 1 and 2 before cancellation was checked
        Assert.Equal(2, captured.Count);
    }

    [Fact]
    public async Task ForEachAsync_ThrowsOnNullSource()
    {
        IEnumerable<int>? items = null;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            items!.ForEachAsync(async x => await Task.Delay(1)));
    }

    [Fact]
    public async Task ForEachAsync_ThrowsOnNullAction()
    {
        var items = new[] { 1, 2, 3 };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            items.ForEachAsync((Func<int, Task>)null!));
    }

    [Fact]
    public async Task ForEachAsync_ExecutesSequentially()
    {
        var items = new[] { 1, 2, 3 };
        var order = new List<int>();

        await items.ForEachAsync(async x =>
        {
            // Add small delay to verify sequential execution
            await Task.Delay(10);
            order.Add(x);
        });

        // Should be in order due to sequential execution
        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Do_WorksWithMonadCollections()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.Some(2),
            Option<int>.None(),
            Option<int>.Some(3)
        };

        var logged = new List<int>();

        var result = options
            .Choose()
            .Do(x => logged.Add(x))
            .ToList();

        Assert.Equal(new[] { 1, 2, 3 }, logged);
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void ForEach_WorksWithResultCollections()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(2)
        };

        var okValues = new List<int>();

        results
            .CollectOk()
            .ForEach(x => okValues.Add(x));

        Assert.Equal(new[] { 1, 2 }, okValues);
    }

    #endregion
}

