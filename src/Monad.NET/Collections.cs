using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Collection extensions for working with sequences of Option&lt;T&gt;, Result&lt;T, E&gt;, and Either&lt;L, R&gt;.
/// </summary>
public static class MonadCollectionExtensions
{
    #region Option Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Option&lt;T&gt;&gt; to Option&lt;IReadOnlyList&lt;T&gt;&gt;.
    /// Returns Some if all options are Some, otherwise None.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    public static Option<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var result = new List<T>();

        foreach (var option in options)
        {
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<T>>.Some(result);
    }

    /// <summary>
    /// Maps each element to an Option and sequences the results.
    /// Returns Some of all values if all mappings succeed, otherwise None.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    public static Option<IReadOnlyList<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, Option<U>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var result = new List<U>();

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsNone)
                return Option<IReadOnlyList<U>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<U>>.Some(result);
    }

    /// <summary>
    /// Filters and unwraps Some values from a sequence of Options.
    /// Similar to Rust's filter_map.
    /// </summary>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var option in options)
        {
            if (option.IsSome)
                yield return option.Unwrap();
        }
    }

    /// <summary>
    /// Maps and filters in one operation, keeping only Some results.
    /// </summary>
    public static IEnumerable<U> Choose<T, U>(
        this IEnumerable<T> source,
        Func<T, Option<U>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsSome)
                yield return option.Unwrap();
        }
    }

    /// <summary>
    /// Returns the first Some value in the sequence, or None if all are None.
    /// </summary>
    public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var option in options)
        {
            if (option.IsSome)
                return option;
        }

        return Option<T>.None();
    }

    #endregion

    #region Result Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Result&lt;T, E&gt;&gt; to Result&lt;IReadOnlyList&lt;T&gt;, E&gt;.
    /// Returns Ok with all values if all results are Ok, otherwise returns the first Err.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    public static Result<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var list = new List<T>();

        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Maps each element to a Result and sequences the results.
    /// Returns Ok of all values if all mappings succeed, otherwise returns the first error.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    public static Result<IReadOnlyList<U>, TErr> Traverse<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Result<U, TErr>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var list = new List<U>();

        foreach (var item in source)
        {
            var result = selector(item);
            if (result.IsErr)
                return Result<IReadOnlyList<U>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<U>, TErr>.Ok(list);
    }

    /// <summary>
    /// Collects all Ok values from a sequence of Results.
    /// Discards all Err values.
    /// </summary>
    public static IEnumerable<T> CollectOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsOk)
                yield return result.Unwrap();
        }
    }

    /// <summary>
    /// Collects all Err values from a sequence of Results.
    /// Discards all Ok values.
    /// </summary>
    public static IEnumerable<TErr> CollectErr<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsErr)
                yield return result.UnwrapErr();
        }
    }

    /// <summary>
    /// Partitions a sequence of Results into Ok and Err values.
    /// Returns a tuple of (oks, errors).
    /// </summary>
    public static (IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errors) Partition<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var oks = new List<T>();
        var errors = new List<TErr>();

        foreach (var result in results)
        {
            if (result.IsOk)
                oks.Add(result.Unwrap());
            else
                errors.Add(result.UnwrapErr());
        }

        return (oks, errors);
    }

    /// <summary>
    /// Returns the first Ok value in the sequence, or the last Err if all are Err.
    /// Throws <see cref="InvalidOperationException"/> if the sequence is empty.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the sequence is empty.</exception>
    public static Result<T, TErr> FirstOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

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
    /// Returns <paramref name="defaultError"/> if the sequence is empty.
    /// </summary>
    /// <param name="results">The sequence of results.</param>
    /// <param name="defaultError">The error to return if the sequence is empty.</param>
    public static Result<T, TErr> FirstOkOrDefault<T, TErr>(
        this IEnumerable<Result<T, TErr>> results,
        TErr defaultError)
    {
        ArgumentNullException.ThrowIfNull(results);

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

    #region Either Collections

    /// <summary>
    /// Collects all Right values from a sequence of Eithers.
    /// </summary>
    public static IEnumerable<TRight> CollectRights<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ArgumentNullException.ThrowIfNull(eithers);

        foreach (var either in eithers)
        {
            if (either.IsRight)
                yield return either.UnwrapRight();
        }
    }

    /// <summary>
    /// Collects all Left values from a sequence of Eithers.
    /// </summary>
    public static IEnumerable<TLeft> CollectLefts<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ArgumentNullException.ThrowIfNull(eithers);

        foreach (var either in eithers)
        {
            if (either.IsLeft)
                yield return either.UnwrapLeft();
        }
    }

    /// <summary>
    /// Partitions a sequence of Eithers into Left and Right values.
    /// Returns a tuple of (lefts, rights).
    /// </summary>
    public static (IReadOnlyList<TLeft> Lefts, IReadOnlyList<TRight> Rights) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ArgumentNullException.ThrowIfNull(eithers);

        var lefts = new List<TLeft>();
        var rights = new List<TRight>();

        foreach (var either in eithers)
        {
            if (either.IsLeft)
                lefts.Add(either.UnwrapLeft());
            else
                rights.Add(either.UnwrapRight());
        }

        return (lefts, rights);
    }

    #endregion

    #region Async Collection Extensions

    /// <summary>
    /// Async version of Sequence for Options.
    /// </summary>
    public static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks)
    {
        ArgumentNullException.ThrowIfNull(optionTasks);

        var result = new List<T>();

        foreach (var task in optionTasks)
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<T>>.Some(result);
    }

    /// <summary>
    /// Async version of Traverse for Options.
    /// </summary>
    public static async Task<Option<IReadOnlyList<U>>> TraverseAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var result = new List<U>();

        foreach (var item in source)
        {
            var option = await selector(item).ConfigureAwait(false);
            if (option.IsNone)
                return Option<IReadOnlyList<U>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IReadOnlyList<U>>.Some(result);
    }

    /// <summary>
    /// Async version of Sequence for Results.
    /// </summary>
    public static async Task<Result<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        this IEnumerable<Task<Result<T, TErr>>> resultTasks)
    {
        ArgumentNullException.ThrowIfNull(resultTasks);

        var list = new List<T>();

        foreach (var task in resultTasks)
        {
            var result = await task.ConfigureAwait(false);
            if (result.IsErr)
                return Result<IReadOnlyList<T>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Async version of Traverse for Results.
    /// </summary>
    public static async Task<Result<IReadOnlyList<U>, TErr>> TraverseAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, TErr>>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var list = new List<U>();

        foreach (var item in source)
        {
            var result = await selector(item).ConfigureAwait(false);
            if (result.IsErr)
                return Result<IReadOnlyList<U>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IReadOnlyList<U>, TErr>.Ok(list);
    }

    #endregion

    #region General Enumerable Extensions

    /// <summary>
    /// Executes an action for each element in the sequence and returns the original sequence.
    /// Useful for side effects in a functional pipeline without breaking the chain.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <returns>The original sequence, allowing method chaining.</returns>
    /// <remarks>
    /// Unlike ForEach, this method returns the sequence for continued chaining.
    /// The action is executed lazily when the sequence is enumerated.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = items
    ///     .Where(x => x.IsValid)
    ///     .Do(x => Console.WriteLine($"Processing: {x}"))
    ///     .Select(x => x.Transform())
    ///     .ToList();
    /// </code>
    /// </example>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Executes an action for each element in the sequence with the element's index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element and its index.</param>
    /// <returns>The original sequence, allowing method chaining.</returns>
    /// <example>
    /// <code>
    /// var results = items
    ///     .Do((x, i) => Console.WriteLine($"[{i}] {x}"))
    ///     .ToList();
    /// </code>
    /// </example>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            action(item, index++);
            yield return item;
        }
    }

    /// <summary>
    /// Executes an action for each element in the sequence.
    /// This is an eager operation that immediately iterates the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <remarks>
    /// Unlike Do, this method does not return the sequence.
    /// It immediately executes the action for all elements.
    /// </remarks>
    /// <example>
    /// <code>
    /// items.ForEach(x => Console.WriteLine(x));
    /// </code>
    /// </example>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an action for each element in the sequence with the element's index.
    /// This is an eager operation that immediately iterates the sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute for each element and its index.</param>
    /// <example>
    /// <code>
    /// items.ForEach((x, i) => Console.WriteLine($"[{i}] {x}"));
    /// </code>
    /// </example>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Executes an async action for each element in the sequence sequentially.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The async action to execute for each element.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <code>
    /// await items.ForEachAsync(async x => await ProcessAsync(x));
    /// </code>
    /// </example>
    public static async Task ForEachAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes an async action for each element in the sequence with the element's index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The async action to execute for each element and its index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <code>
    /// await items.ForEachAsync(async (x, i) => await ProcessAsync(x, i));
    /// </code>
    /// </example>
    public static async Task ForEachAsync<T>(
        this IEnumerable<T> source,
        Func<T, int, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item, index++).ConfigureAwait(false);
        }
    }

    #endregion
}
