using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for NonEmptyList<T> to improve code coverage.
/// </summary>
public class NonEmptyListExtendedTests
{
    #region Factory Tests

    [Fact]
    public void Of_SingleElement_CreatesListWithOneElement()
    {
        var list = NonEmptyList<int>.Of(42);

        Assert.Equal(1, list.Count);
        Assert.Equal(42, list.Head);
        Assert.Empty(list.Tail);
    }

    [Fact]
    public void Of_MultipleElements_CreatesListWithAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);

        Assert.Equal(5, list.Count);
        Assert.Equal(1, list.Head);
        Assert.Equal(new[] { 2, 3, 4, 5 }, list.Tail.ToArray());
    }

    [Fact]
    public void FromEnumerable_WithElements_ReturnsSome()
    {
        var option = NonEmptyList<int>.FromEnumerable(new[] { 1, 2, 3 });

        Assert.True(option.IsSome);
        Assert.Equal(3, option.GetValue().Count);
    }

    [Fact]
    public void FromEnumerable_Empty_ReturnsNone()
    {
        var option = NonEmptyList<int>.FromEnumerable(Array.Empty<int>());

        Assert.True(option.IsNone);
    }

    [Fact]
    public void FromEnumerable_WithError_Success()
    {
        var result = NonEmptyList<int>.FromEnumerable(new[] { 1, 2, 3 }, "empty");

        Assert.True(result.IsOk);
    }

    [Fact]
    public void FromEnumerable_WithError_Failure()
    {
        var result = NonEmptyList<int>.FromEnumerable(Array.Empty<int>(), "empty");

        Assert.True(result.IsErr);
        Assert.Equal("empty", result.GetError());
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_TransformsAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Map(x => x * 2);

        Assert.Equal(new[] { 2, 4, 6 }, result.ToArray());
    }

    [Fact]
    public void Map_PreservesOrder()
    {
        var list = NonEmptyList<string>.Of("a", "b", "c");
        var result = list.Map(x => x.ToUpper());

        Assert.Equal(new[] { "A", "B", "C" }, result.ToArray());
    }

    [Fact]
    public void MapIndexed_TransformsWithIndex()
    {
        var list = NonEmptyList<string>.Of("a", "b", "c");
        var result = list.MapIndexed((x, i) => $"{i}:{x}");

        Assert.Equal(new[] { "0:a", "1:b", "2:c" }, result.ToArray());
    }

    #endregion

    #region FlatMap Tests

    [Fact]
    public void FlatMap_ChainsCorrectly()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Bind(x => NonEmptyList<int>.Of(x, x * 10));

        Assert.Equal(new[] { 1, 10, 2, 20, 3, 30 }, result.ToArray());
    }

    #endregion

    #region Append/Prepend Tests

    [Fact]
    public void Append_AddsToEnd()
    {
        var list = NonEmptyList<int>.Of(1, 2);
        var result = list.Append(3);

        Assert.Equal(new[] { 1, 2, 3 }, result.ToArray());
    }

    [Fact]
    public void Prepend_AddsToBeginning()
    {
        var list = NonEmptyList<int>.Of(2, 3);
        var result = list.Prepend(1);

        Assert.Equal(new[] { 1, 2, 3 }, result.ToArray());
    }

    [Fact]
    public void Concat_CombinesLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2);
        var list2 = NonEmptyList<int>.Of(3, 4);
        var result = list1.Concat(list2);

        Assert.Equal(new[] { 1, 2, 3, 4 }, result.ToArray());
    }

    #endregion

    #region Head/Tail/Last Tests

    [Fact]
    public void Head_ReturnsFirstElement()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        Assert.Equal(1, list.Head);
    }

    [Fact]
    public void Last_ReturnsLastElement()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        Assert.Equal(3, list.Last());
    }

    [Fact]
    public void Tail_ReturnsAllExceptFirst()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        Assert.Equal(new[] { 2, 3 }, list.Tail.ToArray());
    }

    [Fact]
    public void Last_SingleElement_ReturnsHead()
    {
        var list = NonEmptyList<int>.Of(42);
        Assert.Equal(42, list.Last());
    }

    #endregion

    #region Reduce/Fold Tests

    [Fact]
    public void Reduce_CombinesAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4);
        var result = list.Reduce((a, b) => a + b);

        Assert.Equal(10, result);
    }

    [Fact]
    public void Fold_CombinesWithInitial()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Fold(10, (acc, x) => acc + x);

        Assert.Equal(16, result);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_ExecutesForEachElement()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var sum = 0;

        var result = list.Tap(x => sum += x);

        Assert.Equal(6, sum);
        Assert.Equal(list, result);
    }

    [Fact]
    public void TapIndexed_ExecutesWithIndex()
    {
        var list = NonEmptyList<string>.Of("a", "b", "c");
        var indices = new List<int>();

        list.TapIndexed((x, i) => indices.Add(i));

        Assert.Equal(new[] { 0, 1, 2 }, indices);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public void Filter_WithMatchingElements_ReturnsSome()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var result = list.Filter(x => x > 2);

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 3, 4, 5 }, result.GetValue().ToArray());
    }

    [Fact]
    public void Filter_WithNoMatchingElements_ReturnsNone()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Filter(x => x > 10);

        Assert.True(result.IsNone);
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_ElementPresent_ReturnsTrue()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        Assert.Contains(2, list);
    }

    [Fact]
    public void Contains_ElementAbsent_ReturnsFalse()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        Assert.DoesNotContain(99, list);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameElements_ReturnsTrue()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 3);

        Assert.True(list1.Equals(list2));
        Assert.True(list1 == list2);
    }

    [Fact]
    public void Equals_DifferentElements_ReturnsFalse()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 4);

        Assert.False(list1.Equals(list2));
        Assert.True(list1 != list2);
    }

    [Fact]
    public void Equals_DifferentLength_ReturnsFalse()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2);

        Assert.False(list1.Equals(list2));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);

        Assert.False(list.Equals(null));
    }

    [Fact]
    public void Equals_Object_Works()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        object list2 = NonEmptyList<int>.Of(1, 2, 3);
        object notList = "not a list";

        Assert.True(list1.Equals(list2));
        Assert.False(list1.Equals(notList));
    }

    [Fact]
    public void GetHashCode_SameForEqualLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<int>.Of(1, 2, 3);

        Assert.Equal(list1.GetHashCode(), list2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ContainsElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var str = list.ToString();

        Assert.Contains("1", str);
        Assert.Contains("2", str);
        Assert.Contains("3", str);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_ReturnsCorrectElement()
    {
        var list = NonEmptyList<int>.Of(10, 20, 30);

        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var list = NonEmptyList<int>.Of(10, 20, 30);

        Assert.Throws<IndexOutOfRangeException>(() => list[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => list[3]);
    }

    #endregion

    #region Reverse Tests

    [Fact]
    public void Reverse_ReversesElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.Reverse();

        Assert.Equal(new[] { 3, 2, 1 }, result.ToArray());
    }

    #endregion

    #region Sort Tests

    [Fact]
    public void Sort_SortsElements()
    {
        var list = NonEmptyList<int>.Of(3, 1, 2);
        var result = list.Sort();

        Assert.Equal(new[] { 1, 2, 3 }, result.ToArray());
    }

    [Fact]
    public void SortBy_SortsByKey()
    {
        var list = NonEmptyList<string>.Of("ccc", "a", "bb");
        var result = list.SortBy(s => s.Length);

        Assert.Equal(new[] { "a", "bb", "ccc" }, result.ToArray());
    }

    #endregion

    #region TakeFirst Tests

    [Fact]
    public void TakeFirst_WithinBounds_ReturnsSome()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var result = list.TakeFirst(3);

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue().ToArray());
    }

    [Fact]
    public void TakeFirst_Zero_ReturnsNone()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.TakeFirst(0);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void TakeFirst_MoreThanCount_ReturnsAll()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.TakeFirst(10);

        Assert.True(result.IsSome);
        Assert.Equal(list, result.GetValue());
    }

    #endregion

    #region ToList/ToArray Tests

    [Fact]
    public void ToList_ReturnsAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.ToList();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void ToArray_ReturnsAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var result = list.ToArray();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    #endregion

    #region Deconstruct Tests

    [Fact]
    public void Deconstruct_Works()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var (head, tail) = list;

        Assert.Equal(1, head);
        Assert.Equal(new[] { 2, 3 }, tail.ToArray());
    }

    #endregion

    #region Extension Tests

    [Fact]
    public void Zip_CombinesTwoLists()
    {
        var list1 = NonEmptyList<int>.Of(1, 2, 3);
        var list2 = NonEmptyList<string>.Of("a", "b", "c");
        var result = list1.Zip(list2);

        Assert.Equal(new[] { (1, "a"), (2, "b"), (3, "c") }, result.ToArray());
    }

    [Fact]
    public void GroupBy_GroupsElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5, 6);
        var result = list.GroupBy(x => x % 2);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Max_ReturnsMaxElement()
    {
        var list = NonEmptyList<int>.Of(3, 1, 4, 1, 5, 9);
        var result = list.Max();

        Assert.Equal(9, result);
    }

    [Fact]
    public void Min_ReturnsMinElement()
    {
        var list = NonEmptyList<int>.Of(3, 1, 4, 1, 5, 9);
        var result = list.Min();

        Assert.Equal(1, result);
    }

    #endregion

    #region Enumerator Tests

    [Fact]
    public void GetEnumerator_EnumeratesAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var elements = new List<int>();

        foreach (var item in list)
        {
            elements.Add(item);
        }

        Assert.Equal(new[] { 1, 2, 3 }, elements);
    }

    #endregion
}
