using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
    #region Try Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Try&lt;T&gt;&gt; to Try&lt;IReadOnlyList&lt;T&gt;&gt;.
    /// Returns Success with all values if all tries are Success, otherwise returns the first Failure.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of success values in the tries.</typeparam>
    /// <param name="tries">The sequence of tries to transpose.</param>
    /// <returns>Success containing all values if all tries are Success; otherwise the first Failure encountered.</returns>
    /// <example>
    /// <code>
    /// var tries = new[]
    /// {
    ///     Try&lt;int&gt;.Success(1),
    ///     Try&lt;int&gt;.Success(2),
    ///     Try&lt;int&gt;.Failure(new Exception("error"))
    /// };
    /// 
    /// var result = tries.Sequence();
    /// // Failure with the exception
    /// </code>
    /// </example>
    public static Try<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Try<T>> tries)
    {
        ThrowHelper.ThrowIfNull(tries);

        var values = CollectionHelper.CreateListWithCapacity<Try<T>, T>(tries);

        foreach (var @try in tries)
        {
            if (@try.IsError)
                return Try<IReadOnlyList<T>>.Failure(@try.GetException());

            values.Add(@try.GetValue());
        }

        return Try<IReadOnlyList<T>>.Success(values);
    }

    /// <summary>
    /// Maps each element to a Try and sequences the results.
    /// Returns Success of all values if all mappings succeed, otherwise returns the first failure.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting tries.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">A function that maps each element to a Try.</param>
    /// <returns>Success containing all mapped values if all mappings return Success; otherwise the first Failure encountered.</returns>
    /// <example>
    /// <code>
    /// var items = new[] { "1", "2", "abc" };
    /// 
    /// var result = items.Traverse(s => Try&lt;int&gt;.Run(() => int.Parse(s)));
    /// // Failure with FormatException for "abc"
    /// </code>
    /// </example>
    public static Try<IReadOnlyList<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, Try<U>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        var values = CollectionHelper.CreateListWithCapacity<T, U>(source);

        foreach (var item in source)
        {
            var result = selector(item);
            if (result.IsError)
                return Try<IReadOnlyList<U>>.Failure(result.GetException());

            values.Add(result.GetValue());
        }

        return Try<IReadOnlyList<U>>.Success(values);
    }

    /// <summary>
    /// Collects all Success values from a sequence of Tries.
    /// Discards all Failure values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the tries.</typeparam>
    /// <param name="tries">The sequence of tries to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Success values.</returns>
    public static IEnumerable<T> CollectSuccess<T>(this IEnumerable<Try<T>> tries)
    {
        ThrowHelper.ThrowIfNull(tries);

        foreach (var @try in tries)
        {
            if (@try.IsOk)
                yield return @try.GetValue();
        }
    }

    /// <summary>
    /// Collects all Exceptions from Failure tries in a sequence.
    /// Discards all Success values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the tries.</typeparam>
    /// <param name="tries">The sequence of tries to collect from.</param>
    /// <returns>An enumerable containing all exceptions from Failure tries.</returns>
    public static IEnumerable<Exception> CollectFailures<T>(this IEnumerable<Try<T>> tries)
    {
        ThrowHelper.ThrowIfNull(tries);

        foreach (var @try in tries)
        {
            if (@try.IsError)
                yield return @try.GetException();
        }
    }

    /// <summary>
    /// Partitions a sequence of Tries into Success values and Failure exceptions.
    /// </summary>
    /// <typeparam name="T">The type of success values in the tries.</typeparam>
    /// <param name="tries">The sequence of tries to partition.</param>
    /// <returns>A tuple containing a list of Success values and a list of Failure exceptions.</returns>
    public static (IReadOnlyList<T> Successes, IReadOnlyList<Exception> Failures) Partition<T>(
        this IEnumerable<Try<T>> tries)
    {
        ThrowHelper.ThrowIfNull(tries);

        CollectionHelper.TryGetNonEnumeratedCount(tries, out var count);
        var successes = new List<T>(count > 0 ? count : 4);
        var failures = new List<Exception>(count > 0 ? count / 4 : 4);

        foreach (var @try in tries)
        {
            if (@try.IsOk)
                successes.Add(@try.GetValue());
            else
                failures.Add(@try.GetException());
        }

        return (successes, failures);
    }

    /// <summary>
    /// Returns the first Success value in the sequence, or the last Failure if all are Failure.
    /// </summary>
    /// <typeparam name="T">The type of success values in the tries.</typeparam>
    /// <param name="tries">The sequence of tries to search.</param>
    /// <returns>The first Success try found, or the last Failure try if no Success is found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sequence is empty.</exception>
    public static Try<T> FirstSuccess<T>(this IEnumerable<Try<T>> tries)
    {
        ThrowHelper.ThrowIfNull(tries);

        Try<T>? lastFailure = null;

        foreach (var @try in tries)
        {
            if (@try.IsOk)
                return @try;

            lastFailure = @try;
        }

        if (lastFailure is null)
            ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");

        return lastFailure.Value;
    }

    #endregion
}
