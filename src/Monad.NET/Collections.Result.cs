using System.Runtime.CompilerServices;
using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
    #region Result Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Result&lt;T, E&gt;&gt; to Result&lt;IReadOnlyList&lt;T&gt;, E&gt;.
    /// Returns Ok with all values if all results are Ok, otherwise returns the first Err.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to transpose.</param>
    /// <returns>Ok containing all values if all results are Ok; otherwise the first Err encountered.</returns>
    public static Result<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        // Pre-allocate with capacity if we can determine the count
        var list = CollectionHelper.CreateListWithCapacity<Result<T, TErr>, T>(results);

        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.GetError());

            list.Add(result.GetValue());
        }

        return Result<IReadOnlyList<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Maps each element to a Result and sequences the results.
    /// Returns Ok of all values if all mappings succeed, otherwise returns the first error.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">A function that maps each element to a Result.</param>
    /// <returns>Ok containing all mapped values if all mappings return Ok; otherwise the first Err encountered.</returns>
    public static Result<IReadOnlyList<U>, TErr> Traverse<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Result<U, TErr>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        // Pre-allocate with capacity if we can determine the count
        var list = CollectionHelper.CreateListWithCapacity<T, U>(source);

        foreach (var item in source)
        {
            var result = selector(item);
            if (result.IsErr)
                return Result<IReadOnlyList<U>, TErr>.Err(result.GetError());

            list.Add(result.GetValue());
        }

        return Result<IReadOnlyList<U>, TErr>.Ok(list);
    }

    /// <summary>
    /// Collects all Ok values from a sequence of Results.
    /// Discards all Err values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Ok values.</returns>
    public static IEnumerable<T> CollectOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsOk)
                yield return result.GetValue();
        }
    }

    /// <summary>
    /// Collects all Err values from a sequence of Results.
    /// Discards all Ok values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Err values.</returns>
    public static IEnumerable<TErr> CollectErr<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsErr)
                yield return result.GetError();
        }
    }

    /// <summary>
    /// Partitions a sequence of Results into Ok and Err values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to partition.</param>
    /// <returns>A tuple containing a list of Ok values and a list of Err values.</returns>
    public static (IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errors) Partition<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        // Try to get initial capacity for better allocation
        CollectionHelper.TryGetNonEnumeratedCount(results, out var count);
        var oks = new List<T>(count > 0 ? count : 4);
        var errors = new List<TErr>(count > 0 ? count / 4 : 4); // Expect fewer errors

        foreach (var result in results)
        {
            if (result.IsOk)
                oks.Add(result.GetValue());
            else
                errors.Add(result.GetError());
        }

        return (oks, errors);
    }

    /// <summary>
    /// Returns the first Ok value in the sequence, or the last Err if all are Err.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to search.</param>
    /// <returns>The first Ok result found, or the last Err result if no Ok is found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sequence is empty.</exception>
    public static Result<T, TErr> FirstOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        Result<T, TErr>? lastErr = null;

        foreach (var result in results)
        {
            if (result.IsOk)
                return result;

            lastErr = result;
        }

        if (lastErr is null)
            ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");

        return lastErr.Value;
    }

    /// <summary>
    /// Returns the first Ok value in the sequence, or the last Err if all are Err.
    /// Returns an Err with <paramref name="defaultError"/> if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="TErr">The type of error values in the results.</typeparam>
    /// <param name="results">The sequence of results to search.</param>
    /// <param name="defaultError">The error to return if the sequence is empty.</param>
    /// <returns>The first Ok result found, the last Err result, or an Err with the default error if empty.</returns>
    public static Result<T, TErr> FirstOkOrDefault<T, TErr>(
        this IEnumerable<Result<T, TErr>> results,
        TErr defaultError)
    {
        ThrowHelper.ThrowIfNull(results);

        Result<T, TErr>? lastErr = null;

        foreach (var result in results)
        {
            if (result.IsOk)
                return result;

            lastErr = result;
        }

        return lastErr ?? Result<T, TErr>.Err(defaultError);
    }

    #endregion
}

