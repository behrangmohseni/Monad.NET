using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
    #region RemoteData Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;RemoteData&lt;T, E&gt;&gt; to RemoteData&lt;IReadOnlyList&lt;T&gt;, E&gt;.
    /// Returns Success with all values if all are Success, otherwise returns the first non-Success state.
    /// Priority order: Failure > Loading > NotAsked > Success
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="remoteDataItems">The sequence of RemoteData to transpose.</param>
    /// <returns>Success containing all values if all are Success; otherwise the first non-Success state.</returns>
    /// <example>
    /// <code>
    /// var items = new[]
    /// {
    ///     RemoteData&lt;int, string&gt;.Success(1),
    ///     RemoteData&lt;int, string&gt;.Loading(),
    ///     RemoteData&lt;int, string&gt;.Success(3)
    /// };
    /// 
    /// var result = items.Sequence();
    /// // Loading (returns first non-Success state)
    /// </code>
    /// </example>
    public static RemoteData<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        this IEnumerable<RemoteData<T, TErr>> remoteDataItems)
    {
        ThrowHelper.ThrowIfNull(remoteDataItems);

        var values = CollectionHelper.CreateListWithCapacity<RemoteData<T, TErr>, T>(remoteDataItems);
        RemoteData<IReadOnlyList<T>, TErr>? firstNonSuccess = null;

        foreach (var item in remoteDataItems)
        {
            if (item.IsSuccess)
            {
                values.Add(item.GetValue());
            }
            else if (firstNonSuccess is null)
            {
                // Capture the first non-Success state with priority: Failure > Loading > NotAsked
                if (item.IsFailure)
                {
                    firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Failure(item.GetError());
                }
                else if (item.IsLoading)
                {
                    firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Loading();
                }
                else if (item.IsNotAsked)
                {
                    firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.NotAsked();
                }
            }
            else if (item.IsFailure && !firstNonSuccess.Value.IsFailure)
            {
                // Failure takes priority over Loading/NotAsked
                firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Failure(item.GetError());
            }
            else if (item.IsLoading && firstNonSuccess.Value.IsNotAsked)
            {
                // Loading takes priority over NotAsked
                firstNonSuccess = RemoteData<IReadOnlyList<T>, TErr>.Loading();
            }
        }

        return firstNonSuccess ?? RemoteData<IReadOnlyList<T>, TErr>.Success(values);
    }

    /// <summary>
    /// Maps each element to a RemoteData and sequences the results.
    /// Returns Success of all values if all mappings succeed, otherwise returns the first non-Success state.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting RemoteData.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">A function that maps each element to a RemoteData.</param>
    /// <returns>Success containing all mapped values if all mappings return Success; otherwise the first non-Success state.</returns>
    public static RemoteData<IReadOnlyList<U>, TErr> Traverse<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, RemoteData<U, TErr>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        var values = CollectionHelper.CreateListWithCapacity<T, U>(source);
        RemoteData<IReadOnlyList<U>, TErr>? firstNonSuccess = null;

        foreach (var item in source)
        {
            var result = selector(item);

            if (result.IsSuccess)
            {
                values.Add(result.GetValue());
            }
            else if (firstNonSuccess is null)
            {
                if (result.IsFailure)
                {
                    firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Failure(result.GetError());
                }
                else if (result.IsLoading)
                {
                    firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Loading();
                }
                else if (result.IsNotAsked)
                {
                    firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.NotAsked();
                }
            }
            else if (result.IsFailure && !firstNonSuccess.Value.IsFailure)
            {
                firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Failure(result.GetError());
            }
            else if (result.IsLoading && firstNonSuccess.Value.IsNotAsked)
            {
                firstNonSuccess = RemoteData<IReadOnlyList<U>, TErr>.Loading();
            }
        }

        return firstNonSuccess ?? RemoteData<IReadOnlyList<U>, TErr>.Success(values);
    }

    /// <summary>
    /// Collects all Success values from a sequence of RemoteData.
    /// Discards all non-Success values.
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="remoteDataItems">The sequence of RemoteData to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Success values.</returns>
    public static IEnumerable<T> CollectSuccess<T, TErr>(this IEnumerable<RemoteData<T, TErr>> remoteDataItems)
    {
        ThrowHelper.ThrowIfNull(remoteDataItems);

        foreach (var item in remoteDataItems)
        {
            if (item.IsSuccess)
                yield return item.GetValue();
        }
    }

    /// <summary>
    /// Collects all Failure errors from a sequence of RemoteData.
    /// Discards all non-Failure values.
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="remoteDataItems">The sequence of RemoteData to collect from.</param>
    /// <returns>An enumerable containing all error values from Failure items.</returns>
    public static IEnumerable<TErr> CollectFailures<T, TErr>(this IEnumerable<RemoteData<T, TErr>> remoteDataItems)
    {
        ThrowHelper.ThrowIfNull(remoteDataItems);

        foreach (var item in remoteDataItems)
        {
            if (item.IsFailure)
                yield return item.GetError();
        }
    }

    /// <summary>
    /// Partitions a sequence of RemoteData by their state.
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="remoteDataItems">The sequence of RemoteData to partition.</param>
    /// <returns>A tuple containing lists of Success values, Failure errors, and counts of Loading and NotAsked.</returns>
    public static (IReadOnlyList<T> Successes, IReadOnlyList<TErr> Failures, int LoadingCount, int NotAskedCount) Partition<T, TErr>(
        this IEnumerable<RemoteData<T, TErr>> remoteDataItems)
    {
        ThrowHelper.ThrowIfNull(remoteDataItems);

        CollectionHelper.TryGetNonEnumeratedCount(remoteDataItems, out var count);
        var successes = new List<T>(count > 0 ? count : 4);
        var failures = new List<TErr>(count > 0 ? count / 4 : 4);
        var loadingCount = 0;
        var notAskedCount = 0;

        foreach (var item in remoteDataItems)
        {
            if (item.IsSuccess)
                successes.Add(item.GetValue());
            else if (item.IsFailure)
                failures.Add(item.GetError());
            else if (item.IsLoading)
                loadingCount++;
            else if (item.IsNotAsked)
                notAskedCount++;
        }

        return (successes, failures, loadingCount, notAskedCount);
    }

    /// <summary>
    /// Returns the first Success value in the sequence, or the first non-Success state if none are Success.
    /// </summary>
    /// <typeparam name="T">The type of success values.</typeparam>
    /// <typeparam name="TErr">The type of error values.</typeparam>
    /// <param name="remoteDataItems">The sequence of RemoteData to search.</param>
    /// <returns>The first Success RemoteData found, or the first non-Success state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sequence is empty.</exception>
    public static RemoteData<T, TErr> FirstSuccess<T, TErr>(this IEnumerable<RemoteData<T, TErr>> remoteDataItems)
    {
        ThrowHelper.ThrowIfNull(remoteDataItems);

        RemoteData<T, TErr>? firstNonSuccess = null;

        foreach (var item in remoteDataItems)
        {
            if (item.IsSuccess)
                return item;

            firstNonSuccess ??= item;
        }

        if (firstNonSuccess is null)
            ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");

        return firstNonSuccess.Value;
    }

    #endregion
}
