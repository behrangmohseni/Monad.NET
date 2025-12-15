using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Monad.NET.EntityFrameworkCore;

/// <summary>
/// Async extension methods for querying with Option types in EF Core.
/// </summary>
public static class QueryableAsyncExtensions
{
    /// <summary>
    /// Asynchronously gets the first element from a sequence, wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the first element, or None if empty.</returns>
    public static async Task<Option<TSource>> FirstOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        var result = await source.Take(1).ToListAsync(cancellationToken).ConfigureAwait(false);
        return result.Count > 0 ? Option<TSource>.Some(result[0]) : Option<TSource>.None();
    }

    /// <summary>
    /// Asynchronously gets the first element from a sequence that satisfies a condition, wrapped in an Option.
    /// Returns None if no element satisfies the condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the first matching element, or None if none match.</returns>
    public static async Task<Option<TSource>> FirstOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var result = await source.Where(predicate).Take(1).ToListAsync(cancellationToken).ConfigureAwait(false);
        return result.Count > 0 ? Option<TSource>.Some(result[0]) : Option<TSource>.None();
    }

    /// <summary>
    /// Asynchronously gets the single element from a sequence, wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// Throws if there is more than one element.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the single element, or None if empty.</returns>
    public static async Task<Option<TSource>> SingleOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        var result = await source.Take(2).ToListAsync(cancellationToken).ConfigureAwait(false);

        return result.Count switch
        {
            0 => Option<TSource>.None(),
            1 => Option<TSource>.Some(result[0]),
            _ => throw new InvalidOperationException("Sequence contains more than one element")
        };
    }

    /// <summary>
    /// Asynchronously gets the single element from a sequence that satisfies a condition, wrapped in an Option.
    /// Returns None if no element satisfies the condition.
    /// Throws if more than one element satisfies the condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the single matching element, or None if none match.</returns>
    public static async Task<Option<TSource>> SingleOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var result = await source.Where(predicate).Take(2).ToListAsync(cancellationToken).ConfigureAwait(false);

        return result.Count switch
        {
            0 => Option<TSource>.None(),
            1 => Option<TSource>.Some(result[0]),
            _ => throw new InvalidOperationException("Sequence contains more than one matching element")
        };
    }

    /// <summary>
    /// Asynchronously gets the element at the specified index from a sequence, wrapped in an Option.
    /// Returns None if the index is out of range.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the element at the index, or None if out of range.</returns>
    public static async Task<Option<TSource>> ElementAtOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        int index,
        CancellationToken cancellationToken = default)
    {
        if (index < 0)
            return Option<TSource>.None();

        var result = await source.Skip(index).Take(1).ToListAsync(cancellationToken).ConfigureAwait(false);
        return result.Count > 0 ? Option<TSource>.Some(result[0]) : Option<TSource>.None();
    }

    /// <summary>
    /// Asynchronously gets the last element from a sequence, wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the last element, or None if empty.</returns>
    public static async Task<Option<TSource>> LastOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        var result = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        return result.Count > 0 ? Option<TSource>.Some(result[^1]) : Option<TSource>.None();
    }

    /// <summary>
    /// Asynchronously gets the last element from a sequence that satisfies a condition, wrapped in an Option.
    /// Returns None if no element satisfies the condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Some containing the last matching element, or None if none match.</returns>
    public static async Task<Option<TSource>> LastOrNoneAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var result = await source.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        return result.Count > 0 ? Option<TSource>.Some(result[^1]) : Option<TSource>.None();
    }
}

