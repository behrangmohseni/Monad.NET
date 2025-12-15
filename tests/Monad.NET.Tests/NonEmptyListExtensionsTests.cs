using Xunit;

namespace Monad.NET.Tests;

public class NonEmptyListExtensionsTests
{
    #region NonEmptyList Core

    [Fact]
    public void Of_SingleElement_CreatesListWithHead()
    {
        var list = NonEmptyList<int>.Of(42);

        Assert.Equal(1, list.Count);
        Assert.Equal(42, list.Head);
        Assert.Empty(list.Tail);
    }

    [Fact]
    public void Of_MultipleElements_CreatesListWithHeadAndTail()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list.Head);
        Assert.Equal(2, list.Tail.Count);
    }

    [Fact]
    public void FromEnumerable_NonEmpty_ReturnsSome()
    {
        var enumerable = new[] { 1, 2, 3 };
        var result = NonEmptyList<int>.FromEnumerable(enumerable);

        Assert.True(result.IsSome);
        Assert.Equal(3, result.Unwrap().Count);
    }

    [Fact]
    public void FromEnumerable_Empty_ReturnsNone()
    {
        var enumerable = Array.Empty<int>();
        var result = NonEmptyList<int>.FromEnumerable(enumerable);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Indexer_ReturnsCorrectElement()
    {
        var list = NonEmptyList<int>.Of(10, 20, 30);

        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Last_ReturnsTailLastOrHead()
    {
        Assert.Equal(42, NonEmptyList<int>.Of(42).Last());
        Assert.Equal(3, NonEmptyList<int>.Of(1, 2, 3).Last());
    }

    [Fact]
    public void Map_TransformsAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var mapped = list.Map(x => x * 10);

        Assert.Equal(10, mapped.Head);
        Assert.Equal(20, mapped[1]);
        Assert.Equal(30, mapped[2]);
    }

    [Fact]
    public void FlatMap_TransformsAndFlattens()
    {
        var list = NonEmptyList<int>.Of(1, 2);
        var result = list.FlatMap(x => NonEmptyList<int>.Of(x, x * 10));

        Assert.Equal(4, result.Count);
        Assert.Equal(1, result[0]);
        Assert.Equal(10, result[1]);
        Assert.Equal(2, result[2]);
        Assert.Equal(20, result[3]);
    }

    [Fact]
    public void Filter_MatchingElements_ReturnsSome()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var result = list.Filter(x => x > 2);

        Assert.True(result.IsSome);
        Assert.Equal(3, result.Unwrap().Count);
    }

    [Fact]
    public void Filter_NoMatchingElements_ReturnsNone()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Filter(x => x > 10);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Reduce_CombinesElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4);
        var sum = list.Reduce((a, b) => a + b);

        Assert.Equal(10, sum);
    }

    [Fact]
    public void Fold_CombinesWithSeed()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Fold("Start:", (acc, x) => acc + x);

        Assert.Equal("Start:123", result);
    }

    [Fact]
    public void Append_AddsToEnd()
    {
        var list = NonEmptyList<int>.Of(1, 2);
        var appended = list.Append(3);

        Assert.Equal(3, appended.Count);
        Assert.Equal(3, appended.Last());
    }

    [Fact]
    public void Prepend_AddsToStart()
    {
        var list = NonEmptyList<int>.Of(2, 3);
        var prepended = list.Prepend(1);

        Assert.Equal(3, prepended.Count);
        Assert.Equal(1, prepended.Head);
    }

    [Fact]
    public void Concat_CombinesLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2);
        var list2 = NonEmptyList<int>.Of(3, 4);
        var combined = list1.Concat(list2);

        Assert.Equal(4, combined.Count);
        Assert.Equal(1, combined[0]);
        Assert.Equal(4, combined[3]);
    }

    [Fact]
    public void Reverse_ReversesOrder()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var reversed = list.Reverse();

        Assert.Equal(3, reversed.Head);
        Assert.Equal(1, reversed.Last());
    }

    [Fact]
    public void ToList_ConvertsToList()
    {
        var nel = NonEmptyList<int>.Of(1, 2, 3);
        var list = nel.ToList();

        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { 1, 2, 3 }, list);
    }

    [Fact]
    public void ToArray_ConvertsToArray()
    {
        var nel = NonEmptyList<int>.Of(1, 2, 3);
        var array = nel.ToArray();

        Assert.Equal(3, array.Length);
        Assert.Equal(new[] { 1, 2, 3 }, array);
    }

    [Fact]
    public void Enumerable_Iterates()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var items = new List<int>();

        foreach (var item in list)
        {
            items.Add(item);
        }

        Assert.Equal(new[] { 1, 2, 3 }, items);
    }

    #endregion

    #region NonEmptyListExtensions

    [Fact]
    public void Zip_CombinesPairs()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<string>.Of("a", "b", "c");

        var zipped = list1.Zip(list2);

        Assert.Equal(3, zipped.Count);
        Assert.Equal((1, "a"), zipped.Head);
        Assert.Equal((2, "b"), zipped[1]);
        Assert.Equal((3, "c"), zipped[2]);
    }

    [Fact]
    public void Zip_TruncatesToShorterList()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var list2 = NonEmptyList<string>.Of("a", "b");

        var zipped = list1.Zip(list2);

        Assert.Equal(2, zipped.Count);
    }

    [Fact]
    public void GroupBy_GroupsElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5, 6);
        var grouped = list.GroupBy(x => x % 2);

        Assert.Equal(2, grouped.Count);
    }

    [Fact]
    public void Max_ReturnsMaximum()
    {
        var list = NonEmptyList<int>.Of(3, 1, 4, 1, 5);
        var max = list.Max();

        Assert.Equal(5, max);
    }

    [Fact]
    public void Min_ReturnsMinimum()
    {
        var list = NonEmptyList<int>.Of(3, 1, 4, 1, 5);
        var min = list.Min();

        Assert.Equal(1, min);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 3);

        Assert.True(list1.Equals(list2));
        Assert.True(list1 == list2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 4);

        Assert.False(list1.Equals(list2));
        Assert.True(list1 != list2);
    }

    [Fact]
    public void GetHashCode_SameForEqualLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 3);

        Assert.Equal(list1.GetHashCode(), list2.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var str = list.ToString();

        Assert.Contains("1", str);
        Assert.Contains("2", str);
        Assert.Contains("3", str);
    }

    #endregion
}
