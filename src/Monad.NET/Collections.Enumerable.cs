namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(action);

        var index = 0;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item, index++).ConfigureAwait(false);
        }
    }

    #endregion
}

