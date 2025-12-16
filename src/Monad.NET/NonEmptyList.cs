using System.Collections;
using System.Runtime.CompilerServices;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NonEmptyList(T head, IReadOnlyList<T> tail)
    {
        ArgumentNullException.ThrowIfNull(head);
        _head = head;
        _tail = tail ?? Array.Empty<T>();
    }

    /// <summary>
    /// Gets the first element. Always exists!
    /// </summary>
    public T Head
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _head;
    }

    /// <summary>
    /// Gets the remaining elements after the head.
    /// Returns an empty list if this is a single-element list.
    /// </summary>
    public IReadOnlyList<T> Tail
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _tail;
    }

    /// <summary>
    /// Gets the number of elements in the list. Always >= 1.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => 1 + _tail.Count;
    }

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NonEmptyList<T> Of(T head)
    {
        if (head is null)
            ThrowHelper.ThrowArgumentNull(nameof(head), "Cannot create NonEmptyList with null head.");

        return new NonEmptyList<T>(head, Array.Empty<T>());
    }

    /// <summary>
    /// Creates a NonEmptyList with multiple elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NonEmptyList<T> Of(T head, params T[] tail)
    {
        if (head is null)
            ThrowHelper.ThrowArgumentNull(nameof(head), "Cannot create NonEmptyList with null head.");

        return new NonEmptyList<T>(head, tail ?? Array.Empty<T>());
    }

    /// <summary>
    /// Creates a NonEmptyList from an enumerable.
    /// Returns None if the enumerable is empty.
    /// </summary>
    public static Option<NonEmptyList<T>> FromEnumerable(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

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
        ArgumentNullException.ThrowIfNull(items);

        var list = items.ToList();
        return list.Count == 0
            ? Result<NonEmptyList<T>, TErr>.Err(errorIfEmpty)
            : Result<NonEmptyList<T>, TErr>.Ok(new NonEmptyList<T>(list[0], list.Skip(1).ToList()));
    }

    /// <summary>
    /// Gets the last element in the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Last()
    {
        return _tail.Count > 0 ? _tail[_tail.Count - 1] : _head;
    }

    /// <summary>
    /// Maps each element to a new value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<U> Map<U>(Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var newHead = mapper(_head);
        var newTail = _tail.Select(mapper).ToList();
        return new NonEmptyList<U>(newHead, newTail);
    }

    /// <summary>
    /// Maps each element with its index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<U> MapIndexed<U>(Func<T, int, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var newHead = mapper(_head, 0);
        var newTail = _tail.Select((item, index) => mapper(item, index + 1)).ToList();
        return new NonEmptyList<U>(newHead, newTail);
    }

    /// <summary>
    /// Executes an action for each element in the list, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute for each element.</param>
    /// <returns>The original NonEmptyList unchanged.</returns>
    /// <example>
    /// <code>
    /// list.Tap(x => Console.WriteLine(x))
    ///     .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        action(_head);
        foreach (var item in _tail)
            action(item);
        return this;
    }

    /// <summary>
    /// Executes an action for each element with its index in the list, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute for each element with its index.</param>
    /// <returns>The original NonEmptyList unchanged.</returns>
    /// <example>
    /// <code>
    /// list.TapIndexed((x, i) => Console.WriteLine($"{i}: {x}"))
    ///     .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> TapIndexed(Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        action(_head, 0);
        var index = 1;
        foreach (var item in _tail)
            action(item, index++);
        return this;
    }

    /// <summary>
    /// Applies a function that returns a NonEmptyList to each element and flattens the result.
    /// </summary>
    public NonEmptyList<U> FlatMap<U>(Func<T, NonEmptyList<U>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

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
        ArgumentNullException.ThrowIfNull(predicate);

        var filtered = this.Where(predicate).ToList();
        return filtered.Count == 0
            ? Option<NonEmptyList<T>>.None()
            : Option<NonEmptyList<T>>.Some(new NonEmptyList<T>(filtered[0], filtered.Skip(1).ToList()));
    }

    /// <summary>
    /// Adds an element to the end of the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Append(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var newTail = _tail.Append(item).ToList();
        return new NonEmptyList<T>(_head, newTail);
    }

    /// <summary>
    /// Adds an element to the beginning of the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Prepend(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var newTail = new List<T> { _head };
        newTail.AddRange(_tail);
        return new NonEmptyList<T>(item, newTail);
    }

    /// <summary>
    /// Concatenates two NonEmptyLists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Concat(NonEmptyList<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var newTail = new List<T>(_tail);
        newTail.Add(other.Head);
        newTail.AddRange(other.Tail);
        return new NonEmptyList<T>(_head, newTail);
    }

    /// <summary>
    /// Reduces the list to a single value using the provided function.
    /// Since the list is non-empty, no initial value is needed!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Reduce(Func<T, T, T> reducer)
    {
        ArgumentNullException.ThrowIfNull(reducer);

        var result = _head;
        foreach (var item in _tail)
            result = reducer(result, item);

        return result;
    }

    /// <summary>
    /// Folds the list from the left with an initial value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Fold<U>(U initial, Func<U, T, U> folder)
    {
        ArgumentNullException.ThrowIfNull(folder);

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
        ArgumentNullException.ThrowIfNull(keySelector);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        var list = new List<T> { _head };
        list.AddRange(_tail);
        return list;
    }

    /// <summary>
    /// Converts to an array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(NonEmptyList<T>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (Count != other.Count)
            return false;

        return this.SequenceEqual(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NonEmptyList<T>? left, NonEmptyList<T>? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two NonEmptyList instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NonEmptyList<T>? left, NonEmptyList<T>? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implicit conversion from T to NonEmptyList&lt;T&gt; (single element).
    /// Allows: NonEmptyList&lt;int&gt; list = 42;
    /// </summary>
    /// <param name="value">The single element value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator NonEmptyList<T>(T value)
    {
        return Of(value);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NonEmptyList<(T1, T2)> Zip<T1, T2>(
        this NonEmptyList<T1> first,
        NonEmptyList<T2> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var zipped = first.Zip(second, static (a, b) => (a, b)).ToList();
        return NonEmptyList<(T1, T2)>.Of(zipped[0], zipped.Skip(1).ToArray());
    }

    /// <summary>
    /// Groups elements by a key. Each group is guaranteed non-empty.
    /// </summary>
    public static NonEmptyList<(TKey Key, NonEmptyList<T> Values)> GroupBy<T, TKey>(
        this NonEmptyList<T> list,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(keySelector);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this NonEmptyList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this NonEmptyList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        {
            return list.Reduce((a, b) =>
                Comparer<T>.Default.Compare(a, b) < 0 ? a : b);
        }
        throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");
    }
}
