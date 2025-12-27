using System.Runtime.CompilerServices;

namespace Monad.NET.Internal;

/// <summary>
/// Helper methods for efficient collection building.
/// </summary>
internal static class CollectionHelper
{
    /// <summary>
    /// Attempts to get the count of an enumerable without enumerating it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNonEnumeratedCount<T>(IEnumerable<T> source, out int count)
    {
#if NET6_0_OR_GREATER
        return source.TryGetNonEnumeratedCount(out count);
#else
        if (source is ICollection<T> collection)
        {
            count = collection.Count;
            return true;
        }
        if (source is IReadOnlyCollection<T> readOnlyCollection)
        {
            count = readOnlyCollection.Count;
            return true;
        }
        count = 0;
        return false;
#endif
    }

    /// <summary>
    /// Creates a list with optimal initial capacity if the count can be determined.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<TResult> CreateListWithCapacity<TSource, TResult>(IEnumerable<TSource> source, int minimumCapacity = 4)
    {
        if (TryGetNonEnumeratedCount(source, out var count))
        {
            return new List<TResult>(Math.Max(count, minimumCapacity));
        }
        return new List<TResult>(minimumCapacity);
    }

    /// <summary>
    /// Materializes the source to a list, reusing if it's already a list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IList<T> MaterializeToList<T>(IEnumerable<T> source)
    {
        if (source is IList<T> list)
        {
            return list;
        }
        if (source is IReadOnlyList<T> readOnlyList)
        {
            // Wrap in a simple adapter to avoid ToList() allocation
            return new ReadOnlyListAdapter<T>(readOnlyList);
        }
        return source.ToList();
    }

    /// <summary>
    /// Adapter that wraps IReadOnlyList as IList for read operations.
    /// </summary>
    private sealed class ReadOnlyListAdapter<T> : IList<T>
    {
        private readonly IReadOnlyList<T> _source;

        public ReadOnlyListAdapter(IReadOnlyList<T> source) => _source = source;

        public T this[int index]
        {
            get => _source[index];
            set => throw new NotSupportedException();
        }

        public int Count => _source.Count;
        public bool IsReadOnly => true;

        public void Add(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(T item) => _source.Contains(item);
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < _source.Count; i++)
            {
                array[arrayIndex + i] = _source[i];
            }
        }
        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
        public int IndexOf(T item)
        {
            for (var i = 0; i < _source.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_source[i], item))
                    return i;
            }
            return -1;
        }
        public void Insert(int index, T item) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

