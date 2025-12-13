using System.Collections;

namespace Monad.NET;

/// <summary>
/// A list that always contains at least one element.
/// Provides type-level guarantees that eliminate null and empty checks.
/// Useful for operations that require at least one item (aggregations, head/tail, etc.).
/// </summary>
/// <typeparam name="T">The type of elements in the list</typeparam>
public sealed class NonEmptyList<T> : IEnumerable<T>, IEquatable<NonEmptyList<T>>
{
    private readonly T _head;
    private readonly IReadOnlyList<T> _tail;

    private NonEmptyList(T head, IReadOnlyList<T> tail)
    {
        _head = head ?? throw new ArgumentNullException(nameof(head));
        _tail = tail ?? Array.Empty<T>();
    }

    /// <summary>
    /// Gets the first element. Always exists!
    /// </summary>
    public T Head => _head;

    /// <summary>
    /// Gets the remaining elements after the head.
    /// Returns an empty list if this is a single-element list.
    /// </summary>
    public IReadOnlyList<T> Tail => _tail;

    /// <summary>
    /// Gets the number of elements in the list. Always >= 1.
    /// </summary>
    public int Count => 1 + _tail.Count;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range for NonEmptyList with {Count} elements");
            
            return index == 0 ? _head : _tail[index - 1];
        }
    }

    /// <summary>
    /// Creates a NonEmptyList with a single element.
    /// </summary>
    public static NonEmptyList<T> Of(T head)
    {
        if (head is null)
            throw new ArgumentNullException(nameof(head), "Cannot create NonEmptyList with null head");
        
        return new NonEmptyList<T>(head, Array.Empty<T>());
    }

    /// <summary>
    /// Creates a NonEmptyList with multiple elements.
    /// </summary>
    public static NonEmptyList<T> Of(T head, params T[] tail)
    {
        if (head is null)
            throw new ArgumentNullException(nameof(head), "Cannot create NonEmptyList with null head");
        
        return new NonEmptyList<T>(head, tail ?? Array.Empty<T>());
    }

    /// <summary>
    /// Creates a NonEmptyList from an enumerable.
    /// Returns None if the enumerable is empty.
    /// </summary>
    public static Option<NonEmptyList<T>> FromEnumerable(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        var list = items.ToList();
        return list.Count == 0
            ? Option<NonEmptyList<T>>.None()
            : Option<NonEmptyList<T>>.Some(new NonEmptyList<T>(list[0], list.Skip(1).ToList()));
    }

    /// <summary>
    /// Attempts to create a NonEmptyList from an enumerable.
    /// Returns Result with error if empty.
    /// </summary>
    public static Result<NonEmptyList<T>, TErr> FromEnumerable<TErr>(IEnumerable<T> items, TErr errorIfEmpty)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        var list = items.ToList();
        return list.Count == 0
            ? Result<NonEmptyList<T>, TErr>.Err(errorIfEmpty)
            : Result<NonEmptyList<T>, TErr>.Ok(new NonEmptyList<T>(list[0], list.Skip(1).ToList()));
    }

    /// <summary>
    /// Gets the last element in the list.
    /// </summary>
    public T Last()
    {
        return _tail.Count > 0 ? _tail[_tail.Count - 1] : _head;
    }

    /// <summary>
    /// Maps each element to a new value.
    /// </summary>
    public NonEmptyList<U> Map<U>(Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var newHead = mapper(_head);
        var newTail = _tail.Select(mapper).ToList();
        return new NonEmptyList<U>(newHead, newTail);
    }

    /// <summary>
    /// Maps each element with its index.
    /// </summary>
    public NonEmptyList<U> MapIndexed<U>(Func<T, int, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var newHead = mapper(_head, 0);
        var newTail = _tail.Select((item, index) => mapper(item, index + 1)).ToList();
        return new NonEmptyList<U>(newHead, newTail);
    }

    /// <summary>
    /// Applies a function that returns a NonEmptyList to each element and flattens the result.
    /// </summary>
    public NonEmptyList<U> FlatMap<U>(Func<T, NonEmptyList<U>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        var firstList = binder(_head);
        var allItems = new List<U> { firstList.Head };
        allItems.AddRange(firstList.Tail);

        foreach (var item in _tail)
        {
            var nextList = binder(item);
            allItems.Add(nextList.Head);
            allItems.AddRange(nextList.Tail);
        }

        return new NonEmptyList<U>(allItems[0], allItems.Skip(1).ToList());
    }

    /// <summary>
    /// Filters elements. Returns Option because result might be empty.
    /// </summary>
    public Option<NonEmptyList<T>> Filter(Func<T, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var filtered = this.Where(predicate).ToList();
        return filtered.Count == 0
            ? Option<NonEmptyList<T>>.None()
            : Option<NonEmptyList<T>>.Some(new NonEmptyList<T>(filtered[0], filtered.Skip(1).ToList()));
    }

    /// <summary>
    /// Adds an element to the end of the list.
    /// </summary>
    public NonEmptyList<T> Append(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var newTail = _tail.Append(item).ToList();
        return new NonEmptyList<T>(_head, newTail);
    }

    /// <summary>
    /// Adds an element to the beginning of the list.
    /// </summary>
    public NonEmptyList<T> Prepend(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var newTail = new List<T> { _head };
        newTail.AddRange(_tail);
        return new NonEmptyList<T>(item, newTail);
    }

    /// <summary>
    /// Concatenates two NonEmptyLists.
    /// </summary>
    public NonEmptyList<T> Concat(NonEmptyList<T> other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        var newTail = new List<T>(_tail);
        newTail.Add(other.Head);
        newTail.AddRange(other.Tail);
        return new NonEmptyList<T>(_head, newTail);
    }

    /// <summary>
    /// Reduces the list to a single value using the provided function.
    /// Since the list is non-empty, no initial value is needed!
    /// </summary>
    public T Reduce(Func<T, T, T> reducer)
    {
        if (reducer is null)
            throw new ArgumentNullException(nameof(reducer));

        var result = _head;
        foreach (var item in _tail)
            result = reducer(result, item);
        
        return result;
    }

    /// <summary>
    /// Folds the list from the left with an initial value.
    /// </summary>
    public U Fold<U>(U initial, Func<U, T, U> folder)
    {
        if (folder is null)
            throw new ArgumentNullException(nameof(folder));

        var result = folder(initial, _head);
        foreach (var item in _tail)
            result = folder(result, item);
        
        return result;
    }

    /// <summary>
    /// Reverses the list.
    /// </summary>
    public NonEmptyList<T> Reverse()
    {
        var allItems = new List<T> { _head };
        allItems.AddRange(_tail);
        allItems.Reverse();
        return new NonEmptyList<T>(allItems[0], allItems.Skip(1).ToList());
    }

    /// <summary>
    /// Sorts the list using the default comparer.
    /// </summary>
    public NonEmptyList<T> Sort()
    {
        if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        {
            var allItems = new List<T> { _head };
            allItems.AddRange(_tail);
            allItems.Sort();
            return new NonEmptyList<T>(allItems[0], allItems.Skip(1).ToList());
        }
        throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");
    }

    /// <summary>
    /// Sorts the list using a custom comparer.
    /// </summary>
    public NonEmptyList<T> SortBy<TKey>(Func<T, TKey> keySelector)
    {
        if (keySelector is null)
            throw new ArgumentNullException(nameof(keySelector));

        var allItems = new List<T> { _head };
        allItems.AddRange(_tail);
        allItems = allItems.OrderBy(keySelector).ToList();
        return new NonEmptyList<T>(allItems[0], allItems.Skip(1).ToList());
    }

    /// <summary>
    /// Takes the first n elements. Returns Option because result might be empty.
    /// </summary>
    public Option<NonEmptyList<T>> TakeFirst(int count)
    {
        if (count <= 0)
            return Option<NonEmptyList<T>>.None();

        if (count >= Count)
            return Option<NonEmptyList<T>>.Some(this);

        var taken = ((IEnumerable<T>)this).Take(count).ToList();
        return taken.Count > 0
            ? Option<NonEmptyList<T>>.Some(new NonEmptyList<T>(taken[0], taken.Skip(1).ToList()))
            : Option<NonEmptyList<T>>.None();
    }

    /// <summary>
    /// Converts to a regular list.
    /// </summary>
    public List<T> ToList()
    {
        var list = new List<T> { _head };
        list.AddRange(_tail);
        return list;
    }

    /// <summary>
    /// Converts to an array.
    /// </summary>
    public T[] ToArray()
    {
        return ToList().ToArray();
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        yield return _head;
        foreach (var item in _tail)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public bool Equals(NonEmptyList<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Count != other.Count) return false;

        return this.SequenceEqual(other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is NonEmptyList<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in this)
            hash.Add(item);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"NonEmptyList[{string.Join(", ", this)}]";
    }

    /// <summary>
    /// Determines whether two NonEmptyList instances are equal.
    /// </summary>
    public static bool operator ==(NonEmptyList<T>? left, NonEmptyList<T>? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two NonEmptyList instances are not equal.
    /// </summary>
    public static bool operator !=(NonEmptyList<T>? left, NonEmptyList<T>? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Extension methods for NonEmptyList&lt;T&gt;.
/// </summary>
public static class NonEmptyListExtensions
{
    /// <summary>
    /// Zips two NonEmptyLists together.
    /// Result length is the minimum of the two lists.
    /// </summary>
    public static NonEmptyList<(T1, T2)> Zip<T1, T2>(
        this NonEmptyList<T1> first,
        NonEmptyList<T2> second)
    {
        if (first is null)
            throw new ArgumentNullException(nameof(first));
        if (second is null)
            throw new ArgumentNullException(nameof(second));

        var zipped = first.Zip(second, (a, b) => (a, b)).ToList();
        return NonEmptyList<(T1, T2)>.Of(zipped[0], zipped.Skip(1).ToArray());
    }

    /// <summary>
    /// Groups elements by a key. Each group is guaranteed non-empty.
    /// </summary>
    public static NonEmptyList<(TKey Key, NonEmptyList<T> Values)> GroupBy<T, TKey>(
        this NonEmptyList<T> list,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        if (list is null)
            throw new ArgumentNullException(nameof(list));
        if (keySelector is null)
            throw new ArgumentNullException(nameof(keySelector));

        var groupedList = list.ToList().GroupBy(keySelector);
        var groupsList = new List<(TKey, NonEmptyList<T>)>();
        
        foreach (var group in groupedList)
        {
            var groupValues = group.ToList();
            var nonEmptyGroup = NonEmptyList<T>.Of(groupValues[0], groupValues.Skip(1).ToArray());
            groupsList.Add((group.Key, nonEmptyGroup));
        }

        return NonEmptyList<(TKey, NonEmptyList<T>)>.Of(groupsList[0], groupsList.Skip(1).ToArray());
    }

    /// <summary>
    /// Finds the maximum element. Always succeeds because list is non-empty!
    /// </summary>
    public static T Max<T>(this NonEmptyList<T> list)
    {
        if (list is null)
            throw new ArgumentNullException(nameof(list));

        if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        {
            return list.Reduce((a, b) => 
                Comparer<T>.Default.Compare(a, b) > 0 ? a : b);
        }
        throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");
    }

    /// <summary>
    /// Finds the minimum element. Always succeeds because list is non-empty!
    /// </summary>
    public static T Min<T>(this NonEmptyList<T> list)
    {
        if (list is null)
            throw new ArgumentNullException(nameof(list));

        if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        {
            return list.Reduce((a, b) => 
                Comparer<T>.Default.Compare(a, b) < 0 ? a : b);
        }
        throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");
    }
}
