using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Extension methods for working with IAsyncEnumerable and monad types.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AsyncEnumerableExtensions
{
    #region Option Extensions

    /// <summary>
    /// Filters an async sequence to only Some values and unwraps them.
    /// </summary>
    /// <typeparam name="T">The type contained in the Option.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of unwrapped Some values.</returns>
    public static async IAsyncEnumerable<T> ChooseAsync<T>(
        this IAsyncEnumerable<Option<T>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        await foreach (var option in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (option.IsSome)
            {
                yield return option.GetValue();
            }
        }
    }

    /// <summary>
    /// Maps each element of an async sequence using a function that returns an Option,
    /// filtering out None results and unwrapping Some values.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="U">The result element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="selector">A function that returns an Option.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of unwrapped Some values.</returns>
    public static async IAsyncEnumerable<U> ChooseAsync<T, U>(
        this IAsyncEnumerable<T> source,
        Func<T, Option<U>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var result = selector(item);
            if (result.IsSome)
            {
                yield return result.GetValue();
            }
        }
    }

    /// <summary>
    /// Maps each element of an async sequence using an async function that returns an Option,
    /// filtering out None results and unwrapping Some values.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="U">The result element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="selector">An async function that returns an Option.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of unwrapped Some values.</returns>
    public static async IAsyncEnumerable<U> ChooseAsync<T, U>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<Option<U>>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var result = await selector(item).ConfigureAwait(false);
            if (result.IsSome)
            {
                yield return result.GetValue();
            }
        }
    }

    /// <summary>
    /// Gets the first element of an async sequence wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the first element, or None if empty.</returns>
    public static async Task<Option<T>> FirstOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return Option<T>.Some(item);
        }
        return Option<T>.None();
    }

    /// <summary>
    /// Gets the first element of an async sequence that satisfies a predicate, wrapped in an Option.
    /// Returns None if no element satisfies the predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the first matching element, or None if none match.</returns>
    public static async Task<Option<T>> FirstOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                return Option<T>.Some(item);
            }
        }
        return Option<T>.None();
    }

    /// <summary>
    /// Gets the last element of an async sequence wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the last element, or None if empty.</returns>
    public static async Task<Option<T>> LastOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        var hasValue = false;
        T? last = default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            hasValue = true;
            last = item;
        }

        return hasValue ? Option<T>.Some(last!) : Option<T>.None();
    }

    /// <summary>
    /// Converts an async sequence of Options to a sequence of unwrapped values,
    /// stopping at the first None encountered.
    /// </summary>
    /// <typeparam name="T">The type contained in the Option.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An Option containing a list of values if all are Some, or None if any is None.</returns>
    public static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        this IAsyncEnumerable<Option<T>> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        var results = new List<T>();

        await foreach (var option in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (option.IsNone)
            {
                return Option<IReadOnlyList<T>>.None();
            }
            results.Add(option.GetValue());
        }

        return Option<IReadOnlyList<T>>.Some(results);
    }

    #endregion

    #region Result Extensions

    /// <summary>
    /// Filters an async sequence to only Ok values and unwraps them.
    /// </summary>
    /// <typeparam name="T">The Ok value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of unwrapped Ok values.</returns>
    public static async IAsyncEnumerable<T> CollectOkAsync<T, E>(
        this IAsyncEnumerable<Result<T, E>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result.IsOk)
            {
                yield return result.GetValue();
            }
        }
    }

    /// <summary>
    /// Filters an async sequence to only Err values and unwraps them.
    /// </summary>
    /// <typeparam name="T">The Ok value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of unwrapped Err values.</returns>
    public static async IAsyncEnumerable<E> CollectErrAsync<T, E>(
        this IAsyncEnumerable<Result<T, E>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result.IsErr)
            {
                yield return result.GetError();
            }
        }
    }

    /// <summary>
    /// Partitions an async sequence of Results into Ok and Err values.
    /// </summary>
    /// <typeparam name="T">The Ok value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A tuple containing lists of Ok values and Err values.</returns>
    public static async Task<(IReadOnlyList<T> Oks, IReadOnlyList<E> Errs)> PartitionAsync<T, E>(
        this IAsyncEnumerable<Result<T, E>> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        var oks = new List<T>();
        var errs = new List<E>();

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result.IsOk)
            {
                oks.Add(result.GetValue());
            }
            else
            {
                errs.Add(result.GetError());
            }
        }

        return (oks, errs);
    }

    /// <summary>
    /// Converts an async sequence of Results to a single Result containing a list of values.
    /// Returns the first error encountered if any result is Err.
    /// </summary>
    /// <typeparam name="T">The Ok value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Ok containing a list of values, or the first Err encountered.</returns>
    public static async Task<Result<IReadOnlyList<T>, E>> SequenceAsync<T, E>(
        this IAsyncEnumerable<Result<T, E>> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        var results = new List<T>();

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result.IsErr)
            {
                return Result<IReadOnlyList<T>, E>.Err(result.GetError());
            }
            results.Add(result.GetValue());
        }

        return Result<IReadOnlyList<T>, E>.Ok(results);
    }

    #endregion

    #region Try Extensions

    /// <summary>
    /// Filters an async sequence to only Success values and unwraps them.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of unwrapped Success values.</returns>
    public static async IAsyncEnumerable<T> CollectSuccessAsync<T>(
        this IAsyncEnumerable<Try<T>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result.IsSuccess)
            {
                yield return result.GetValue();
            }
        }
    }

    /// <summary>
    /// Filters an async sequence to only Failure values and unwraps them.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of exceptions from Failure values.</returns>
    public static async IAsyncEnumerable<Exception> CollectFailureAsync<T>(
        this IAsyncEnumerable<Try<T>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result.IsFailure)
            {
                yield return result.GetException();
            }
        }
    }

    #endregion

    #region General Extensions

    /// <summary>
    /// Applies an async transformation to each element of a sequence.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="U">The result element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="selector">An async transformation function.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of transformed elements.</returns>
    public static async IAsyncEnumerable<U> SelectAsync<T, U>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<U>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await selector(item).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Filters an async sequence based on an async predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="predicate">An async predicate function.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of elements that satisfy the predicate.</returns>
    public static async IAsyncEnumerable<T> WhereAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<bool>> predicate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (await predicate(item).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Executes an async action for each element in the sequence (side effect).
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="action">An async action to execute for each element.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable that yields elements after executing the action.</returns>
    public static async IAsyncEnumerable<T> TapAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, Task> action,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await action(item).ConfigureAwait(false);
            yield return item;
        }
    }

    /// <summary>
    /// Converts an async enumerable to a list.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list containing all elements.</returns>
    public static async Task<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Counts the elements in an async sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The count of elements.</returns>
    public static async Task<int> CountAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);

        var count = 0;
        await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Checks if any element in an async sequence satisfies a predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if any element satisfies the predicate.</returns>
    public static async Task<bool> AnyAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if all elements in an async sequence satisfy a predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if all elements satisfy the predicate.</returns>
    public static async Task<bool> AllAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (!predicate(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Aggregates an async sequence using a reducer function.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <typeparam name="TAccumulate">The accumulator type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="accumulator">An accumulator function.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The final accumulated value.</returns>
    public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(
        this IAsyncEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> accumulator,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(accumulator);

        var result = seed;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result = accumulator(result, item);
        }
        return result;
    }

    #endregion
}
