using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// A list that always contains at least one element.
/// Provides type-level guarantees that eliminate null and empty checks.
/// Useful for operations that require at least one item (aggregations, head/tail, etc.).
/// </summary>
/// <typeparam name="T">The type of elements in the list</typeparam>
/// <remarks>
/// <para>
/// This is a <c>readonly struct</c> for performance and consistency with other Monad.NET types.
/// The default value (<c>default(NonEmptyList&lt;T&gt;)</c>) represents an uninitialized state
/// and will throw when accessed, similar to how <see cref="Option{T}.GetValue"/> throws on None.
/// </para>
/// <para>
/// Always use factory methods like <see cref="Of(T)"/> or <see cref="FromEnumerable"/> to create instances.
/// </para>
/// </remarks>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(NonEmptyListDebugView<>))]
public readonly struct NonEmptyList<T> : IEnumerable<T>, IEquatable<NonEmptyList<T>>
{
    private readonly T _head;
    private readonly ImmutableArray<T> _tail;
    private readonly bool _isInitialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isInitialized 
        ? $"NonEmptyList[{Count}] {{ {_head}, ... }}" 
        : "NonEmptyList (uninitialized)";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NonEmptyList(T head, ImmutableArray<T> tail)
    {
        _head = head;
        _tail = tail.IsDefault ? ImmutableArray<T>.Empty : tail;
        _isInitialized = true;
    }

    /// <summary>
    /// Returns true if this NonEmptyList is properly initialized.
    /// A default-constructed NonEmptyList is not initialized.
    /// </summary>
    public bool IsInitialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isInitialized;
    }

    /// <summary>
    /// Gets the first element. Always exists for initialized lists.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized (default value).</exception>
    public T Head
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!_isInitialized)
                ThrowHelper.ThrowNonEmptyListNotInitialized();
            return _head;
        }
    }

    /// <summary>
    /// Gets the remaining elements after the head.
    /// Returns an empty array if this is a single-element list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized (default value).</exception>
    public IReadOnlyList<T> Tail
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!_isInitialized)
                ThrowHelper.ThrowNonEmptyListNotInitialized();
            return _tail;
        }
    }

    /// <summary>
    /// Gets the number of elements in the list. Always >= 1 for initialized lists.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized (default value).</exception>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!_isInitialized)
                ThrowHelper.ThrowNonEmptyListNotInitialized();
            return 1 + _tail.Length;
        }
    }

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized (default value).</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!_isInitialized)
                ThrowHelper.ThrowNonEmptyListNotInitialized();

            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range for NonEmptyList with {Count} elements");

            return index == 0 ? _head : _tail[index - 1];
        }
    }

    /// <summary>
    /// Creates a NonEmptyList with a single element.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if head is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NonEmptyList<T> Of(T head)
    {
        if (head is null)
            ThrowHelper.ThrowArgumentNull(nameof(head), "Cannot create NonEmptyList with null head.");

        return new NonEmptyList<T>(head, ImmutableArray<T>.Empty);
    }

    /// <summary>
    /// Creates a NonEmptyList with multiple elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if head is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NonEmptyList<T> Of(T head, params T[] tail)
    {
        if (head is null)
            ThrowHelper.ThrowArgumentNull(nameof(head), "Cannot create NonEmptyList with null head.");

        return new NonEmptyList<T>(head, tail is null ? ImmutableArray<T>.Empty : ImmutableArray.Create(tail));
    }

    /// <summary>
    /// Creates a NonEmptyList from an enumerable.
    /// Returns None if the enumerable is empty.
    /// </summary>
    public static Option<NonEmptyList<T>> FromEnumerable(IEnumerable<T> items)
    {
        ThrowHelper.ThrowIfNull(items);

        var array = items.ToImmutableArray();
        if (array.Length == 0)
            return Option<NonEmptyList<T>>.None();

        return Option<NonEmptyList<T>>.Some(
            new NonEmptyList<T>(array[0], array.RemoveAt(0)));
    }

    /// <summary>
    /// Attempts to create a NonEmptyList from an enumerable.
    /// Returns Result with error if empty.
    /// </summary>
    public static Result<NonEmptyList<T>, TErr> FromEnumerable<TErr>(IEnumerable<T> items, TErr errorIfEmpty)
    {
        ThrowHelper.ThrowIfNull(items);

        var array = items.ToImmutableArray();
        if (array.Length == 0)
            return Result<NonEmptyList<T>, TErr>.Err(errorIfEmpty);

        return Result<NonEmptyList<T>, TErr>.Ok(
            new NonEmptyList<T>(array[0], array.RemoveAt(0)));
    }

    /// <summary>
    /// Gets the last element in the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Last()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        return _tail.Length > 0 ? _tail[_tail.Length - 1] : _head;
    }

    /// <summary>
    /// Maps each element to a new value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<U> Map<U>(Func<T, U> mapper)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(mapper);

        var newHead = mapper(_head);
        var newTail = _tail.Select(mapper).ToImmutableArray();
        return new NonEmptyList<U>(newHead, newTail);
    }

    /// <summary>
    /// Maps each element with its index.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<U> MapIndexed<U>(Func<T, int, U> mapper)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(mapper);

        var newHead = mapper(_head, 0);
        var newTail = _tail.Select((item, index) => mapper(item, index + 1)).ToImmutableArray();
        return new NonEmptyList<U>(newHead, newTail);
    }

    /// <summary>
    /// Executes an action for each element in the list, allowing method chaining.
    /// </summary>
    /// <param name="action">The action to execute for each element.</param>
    /// <returns>The original NonEmptyList unchanged.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    /// <example>
    /// <code>
    /// list.Tap(x => Console.WriteLine(x))
    ///     .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Tap(Action<T> action)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(action);

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
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    /// <example>
    /// <code>
    /// list.TapIndexed((x, i) => Console.WriteLine($"{i}: {x}"))
    ///     .Map(x => x * 2);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> TapIndexed(Action<T, int> action)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(action);

        action(_head, 0);
        var index = 1;
        foreach (var item in _tail)
            action(item, index++);
        return this;
    }

    /// <summary>
    /// Applies a function that returns a NonEmptyList to each element and flattens the result.
    /// This is the monadic bind operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    public NonEmptyList<U> Bind<U>(Func<T, NonEmptyList<U>> binder)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(binder);

        var firstList = binder(_head);
        var builder = ImmutableArray.CreateBuilder<U>();
        builder.Add(firstList._head);
        builder.AddRange(firstList._tail);

        foreach (var item in _tail)
        {
            var nextList = binder(item);
            builder.Add(nextList._head);
            builder.AddRange(nextList._tail);
        }

        return new NonEmptyList<U>(builder[0], builder.ToImmutable().RemoveAt(0));
    }

    /// <summary>
    /// Filters elements. Returns Option because result might be empty.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    public Option<NonEmptyList<T>> Filter(Func<T, bool> predicate)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(predicate);

        var filtered = this.Where(predicate).ToImmutableArray();
        if (filtered.Length == 0)
            return Option<NonEmptyList<T>>.None();

        return Option<NonEmptyList<T>>.Some(
            new NonEmptyList<T>(filtered[0], filtered.RemoveAt(0)));
    }

    /// <summary>
    /// Adds an element to the end of the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Append(T item)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(item);

        return new NonEmptyList<T>(_head, _tail.Add(item));
    }

    /// <summary>
    /// Adds an element to the beginning of the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Prepend(T item)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(item);

        return new NonEmptyList<T>(item, _tail.Insert(0, _head));
    }

    /// <summary>
    /// Concatenates two NonEmptyLists.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if either list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NonEmptyList<T> Concat(NonEmptyList<T> other)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        if (!other._isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        var newTail = _tail.Add(other._head).AddRange(other._tail);
        return new NonEmptyList<T>(_head, newTail);
    }

    /// <summary>
    /// Reduces the list to a single value using the provided function.
    /// Since the list is non-empty, no initial value is needed!
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Reduce(Func<T, T, T> reducer)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(reducer);

        var result = _head;
        foreach (var item in _tail)
            result = reducer(result, item);

        return result;
    }

    /// <summary>
    /// Folds the list from the left with an initial value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Fold<U>(U initial, Func<U, T, U> folder)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(folder);

        var result = folder(initial, _head);
        foreach (var item in _tail)
            result = folder(result, item);

        return result;
    }

    /// <summary>
    /// Reverses the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    public NonEmptyList<T> Reverse()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        var all = _tail.Insert(0, _head);
        var reversed = all.Reverse().ToImmutableArray();
        return new NonEmptyList<T>(reversed[0], reversed.RemoveAt(0));
    }

    /// <summary>
    /// Sorts the list using the default comparer.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    /// <exception cref="NotSupportedException">Thrown if T doesn't implement IComparable&lt;T&gt;.</exception>
    public NonEmptyList<T> Sort()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");

        var all = _tail.Insert(0, _head).Sort();
        return new NonEmptyList<T>(all[0], all.RemoveAt(0));
    }

    /// <summary>
    /// Sorts the list using a custom key selector.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    public NonEmptyList<T> SortBy<TKey>(Func<T, TKey> keySelector)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(keySelector);

        var all = _tail.Insert(0, _head);
        var sorted = all.OrderBy(keySelector).ToImmutableArray();
        return new NonEmptyList<T>(sorted[0], sorted.RemoveAt(0));
    }

    /// <summary>
    /// Takes the first n elements. Returns Option because result might be empty.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    public Option<NonEmptyList<T>> TakeFirst(int count)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        if (count <= 0)
            return Option<NonEmptyList<T>>.None();

        if (count >= Count)
            return Option<NonEmptyList<T>>.Some(this);

        var taken = this.Take(count).ToImmutableArray();
        return Option<NonEmptyList<T>>.Some(
            new NonEmptyList<T>(taken[0], taken.RemoveAt(0)));
    }

    /// <summary>
    /// Converts to a regular list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> ToList()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        var list = new List<T>(Count) { _head };
        list.AddRange(_tail);
        return list;
    }

    /// <summary>
    /// Converts to an array.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        var array = new T[Count];
        array[0] = _head;
        for (var i = 0; i < _tail.Length; i++)
            array[i + 1] = _tail[i];
        return array;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        yield return _head;
        foreach (var item in _tail)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(NonEmptyList<T> other)
    {
        // Both uninitialized are equal
        if (!_isInitialized && !other._isInitialized)
            return true;

        // One initialized, one not - not equal
        if (_isInitialized != other._isInitialized)
            return false;

        // Both initialized - compare contents
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
        if (!_isInitialized)
            return 0;

        var hash = new HashCode();
        foreach (var item in this)
            hash.Add(item);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!_isInitialized)
            return "NonEmptyList (uninitialized)";

        return $"NonEmptyList[{string.Join(", ", this)}]";
    }

    /// <summary>
    /// Determines whether two NonEmptyList instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NonEmptyList<T> left, NonEmptyList<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two NonEmptyList instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NonEmptyList<T> left, NonEmptyList<T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the NonEmptyList into its head and tail for pattern matching.
    /// </summary>
    /// <param name="head">The first element of the list.</param>
    /// <param name="tail">The remaining elements (may be empty).</param>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    /// <example>
    /// <code>
    /// var list = NonEmptyList&lt;int&gt;.Of(1, 2, 3);
    /// var (head, tail) = list;
    /// Console.WriteLine($"Head: {head}, Tail count: {tail.Count}");
    /// // Output: Head: 1, Tail count: 2
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T head, out IReadOnlyList<T> tail)
    {
        if (!_isInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        head = _head;
        tail = _tail;
    }

}

/// <summary>
/// Debug view for NonEmptyList.
/// </summary>
internal sealed class NonEmptyListDebugView<T>
{
    private readonly NonEmptyList<T> _list;

    public NonEmptyListDebugView(NonEmptyList<T> list) => _list = list;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _list.IsInitialized ? _list.ToArray() : Array.Empty<T>();
}

/// <summary>
/// Extension methods for NonEmptyList&lt;T&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class NonEmptyListExtensions
{
    /// <summary>
    /// Zips two NonEmptyLists together.
    /// Result length is the minimum of the two lists.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if either list is not initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NonEmptyList<(T1, T2)> Zip<T1, T2>(
        this NonEmptyList<T1> first,
        NonEmptyList<T2> second)
    {
        if (!first.IsInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        if (!second.IsInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        var zipped = first.Zip(second, static (a, b) => (a, b)).ToImmutableArray();
        return NonEmptyList<(T1, T2)>.Of(zipped[0], zipped.RemoveAt(0).ToArray());
    }

    /// <summary>
    /// Groups elements by a key. Each group is guaranteed non-empty.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    public static NonEmptyList<(TKey Key, NonEmptyList<T> Values)> GroupBy<T, TKey>(
        this NonEmptyList<T> list,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        if (!list.IsInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();
        ThrowHelper.ThrowIfNull(keySelector);

        var grouped = list.AsEnumerable().GroupBy(keySelector);
        var result = new List<(TKey, NonEmptyList<T>)>();

        foreach (var group in grouped)
        {
            var groupArray = group.ToImmutableArray();
            var nonEmptyGroup = NonEmptyList<T>.Of(groupArray[0], groupArray.RemoveAt(0).ToArray());
            result.Add((group.Key, nonEmptyGroup));
        }

        return NonEmptyList<(TKey, NonEmptyList<T>)>.Of(result[0], result.Skip(1).ToArray());
    }

    /// <summary>
    /// Finds the maximum element. Always succeeds because list is non-empty!
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    /// <exception cref="NotSupportedException">Thrown if T doesn't implement IComparable&lt;T&gt;.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(this NonEmptyList<T> list)
    {
        if (!list.IsInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");

        return list.Reduce((a, b) =>
            Comparer<T>.Default.Compare(a, b) > 0 ? a : b);
    }

    /// <summary>
    /// Finds the minimum element. Always succeeds because list is non-empty!
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the list is not initialized.</exception>
    /// <exception cref="NotSupportedException">Thrown if T doesn't implement IComparable&lt;T&gt;.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(this NonEmptyList<T> list)
    {
        if (!list.IsInitialized)
            ThrowHelper.ThrowNonEmptyListNotInitialized();

        if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            throw new NotSupportedException($"Type {typeof(T).Name} does not implement IComparable<T>");

        return list.Reduce((a, b) =>
            Comparer<T>.Default.Compare(a, b) < 0 ? a : b);
    }
}
