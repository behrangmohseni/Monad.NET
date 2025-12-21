using Monad.NET;

namespace Monad.NET.Tests;

public class NonEmptyListTests
{
    [Fact]
    public void Of_SingleElement_CreatesNonEmptyList()
    {
        var list = NonEmptyList<int>.Of(42);

        Assert.Equal(42, list.Head);
        Assert.Empty(list.Tail);
        Assert.Equal(1, list.Count);
    }

    [Fact]
    public void Of_MultipleElements_CreatesNonEmptyList()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);

        Assert.Equal(1, list.Head);
        Assert.Equal(4, list.Tail.Count);
        Assert.Equal(5, list.Count);
    }

    [Fact]
    public void Of_WithNullHead_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => NonEmptyList<string>.Of(null!));
    }

    [Fact]
    public void Indexer_ReturnsCorrectElements()
    {
        var list = NonEmptyList<int>.Of(10, 20, 30);

        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_ThrowsException()
    {
        var list = NonEmptyList<int>.Of(10);

        Assert.Throws<IndexOutOfRangeException>(() => list[1]);
    }

    [Fact]
    public void Last_OnSingleElement_ReturnsHead()
    {
        var list = NonEmptyList<int>.Of(42);
        Assert.Equal(42, list.Last());
    }

    [Fact]
    public void Last_OnMultipleElements_ReturnsLastElement()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        Assert.Equal(5, list.Last());
    }

    [Fact]
    public void Map_TransformsAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var mapped = list.Map(x => x * 2);

        Assert.Equal(2, mapped.Head);
        Assert.Equal(new[] { 4, 6 }, mapped.Tail);
    }

    [Fact]
    public void MapIndexed_TransformsWithIndex()
    {
        var list = NonEmptyList<string>.Of("a", "b", "c");
        var mapped = list.MapIndexed((x, i) => $"{x}{i}");

        Assert.Equal("a0", mapped.Head);
        Assert.Equal(new[] { "b1", "c2" }, mapped.Tail);
    }

    [Fact]
    public void FlatMap_FlattensCorrectly()
    {
        var list = NonEmptyList<int>.Of(1, 2);
        var result = list.FlatMap(x => NonEmptyList<int>.Of(x, x * 10));

        Assert.Equal(1, result.Head);
        Assert.Equal(new[] { 10, 2, 20 }, result.Tail);
    }

    [Fact]
    public void Filter_WithAllMatching_ReturnsSome()
    {
        var list = NonEmptyList<int>.Of(2, 4, 6);
        var filtered = list.Filter(x => x % 2 == 0);

        Assert.True(filtered.IsSome);
        Assert.Equal(new[] { 2, 4, 6 }, filtered.Unwrap().ToList());
    }

    [Fact]
    public void Filter_WithNoneMatching_ReturnsNone()
    {
        var list = NonEmptyList<int>.Of(1, 3, 5);
        var filtered = list.Filter(x => x % 2 == 0);

        Assert.True(filtered.IsNone);
    }

    [Fact]
    public void Append_AddsElementToEnd()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var appended = list.Append(4);

        Assert.Equal(1, appended.Head);
        Assert.Equal(new[] { 2, 3, 4 }, appended.Tail);
    }

    [Fact]
    public void Prepend_AddsElementToBeginning()
    {
        var list = NonEmptyList<int>.Of(2, 3, 4);
        var prepended = list.Prepend(1);

        Assert.Equal(1, prepended.Head);
        Assert.Equal(new[] { 2, 3, 4 }, prepended.Tail);
    }

    [Fact]
    public void Concat_JoinsTwoLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2);
        var list2 = NonEmptyList<int>.Of(3, 4);
        var concatenated = list1.Concat(list2);

        Assert.Equal(4, concatenated.Count);
        Assert.Equal(new[] { 1, 2, 3, 4 }, concatenated.ToList());
    }

    [Fact]
    public void Reduce_AggregatesCorrectly()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var sum = list.Reduce((a, b) => a + b);

        Assert.Equal(15, sum);
    }

    [Fact]
    public void Fold_WithInitialValue_AggregatesCorrectly()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var sum = list.Fold(0, (acc, x) => acc + x);

        Assert.Equal(6, sum);
    }

    [Fact]
    public void Reverse_ReversesElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var reversed = list.Reverse();

        Assert.Equal(5, reversed.Head);
        Assert.Equal(new[] { 4, 3, 2, 1 }, reversed.Tail);
    }

    [Fact]
    public void Sort_SortsElements()
    {
        var list = NonEmptyList<int>.Of(5, 2, 8, 1, 9);
        var sorted = list.Sort();

        Assert.Equal(new[] { 1, 2, 5, 8, 9 }, sorted.ToList());
    }

    [Fact]
    public void SortBy_SortsElementsByKey()
    {
        var list = NonEmptyList<string>.Of("aaa", "a", "aa");
        var sorted = list.SortBy(s => s.Length);

        Assert.Equal(new[] { "a", "aa", "aaa" }, sorted.ToList());
    }

    [Fact]
    public void FromEnumerable_WithItems_ReturnsSome()
    {
        var items = new[] { 1, 2, 3 };
        var result = NonEmptyList<int>.FromEnumerable(items);

        Assert.True(result.IsSome);
        Assert.Equal(3, result.Unwrap().Count);
    }

    [Fact]
    public void FromEnumerable_WithEmpty_ReturnsNone()
    {
        var items = Array.Empty<int>();
        var result = NonEmptyList<int>.FromEnumerable(items);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void FromEnumerable_WithResult_WithItems_ReturnsOk()
    {
        var items = new[] { 1, 2, 3 };
        var result = NonEmptyList<int>.FromEnumerable(items, "List is empty");

        Assert.True(result.IsOk);
        Assert.Equal(3, result.Unwrap().Count);
    }

    [Fact]
    public void FromEnumerable_WithResult_WithEmpty_ReturnsErr()
    {
        var items = Array.Empty<int>();
        var result = NonEmptyList<int>.FromEnumerable(items, "List is empty");

        Assert.True(result.IsErr);
        Assert.Equal("List is empty", result.UnwrapErr());
    }

    [Fact]
    public void Zip_CombinesTwoLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<string>.Of("a", "b", "c");
        var zipped = list1.Zip(list2);

        Assert.Equal(3, zipped.Count);
        Assert.Equal((1, "a"), zipped.Head);
        Assert.Equal((2, "b"), zipped.Tail[0]);
        Assert.Equal((3, "c"), zipped.Tail[1]);
    }

    [Fact]
    public void Zip_WithDifferentLengths_TakesMinimum()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var list2 = NonEmptyList<string>.Of("a", "b");
        var zipped = list1.Zip(list2);

        Assert.Equal(2, zipped.Count);
    }

    [Fact]
    public void Max_ReturnsMaxElement()
    {
        var list = NonEmptyList<int>.Of(3, 1, 4, 1, 5, 9, 2);
        var max = list.Max();

        Assert.Equal(9, max);
    }

    [Fact]
    public void Min_ReturnsMinElement()
    {
        var list = NonEmptyList<int>.Of(3, 1, 4, 1, 5, 9, 2);
        var min = list.Min();

        Assert.Equal(1, min);
    }

    [Fact]
    public void ToList_ConvertsCorrectly()
    {
        var nonEmptyList = NonEmptyList<int>.Of(1, 2, 3);
        var list = nonEmptyList.ToList();

        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { 1, 2, 3 }, list);
    }

    [Fact]
    public void ToArray_ConvertsCorrectly()
    {
        var nonEmptyList = NonEmptyList<int>.Of(1, 2, 3);
        var array = nonEmptyList.ToArray();

        Assert.Equal(3, array.Length);
        Assert.Equal(new[] { 1, 2, 3 }, array);
    }

    [Fact]
    public void Enumeration_WorksCorrectly()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var items = new List<int>();

        foreach (var item in list)
            items.Add(item);

        Assert.Equal(new[] { 1, 2, 3 }, items);
    }

    [Fact]
    public void Equality_TwoListsWithSameElements_AreEqual()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 3);

        Assert.Equal(list1, list2);
        Assert.True(list1 == list2);
    }

    [Fact]
    public void Equality_TwoListsWithDifferentElements_AreNotEqual()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 4);

        Assert.NotEqual(list1, list2);
        Assert.True(list1 != list2);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        Assert.Equal("NonEmptyList[1, 2, 3]", list.ToString());
    }
}
