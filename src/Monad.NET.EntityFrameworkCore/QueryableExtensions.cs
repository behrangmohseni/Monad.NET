using System.Linq.Expressions;

namespace Monad.NET.EntityFrameworkCore;

/// <summary>
/// Extension methods for querying with Option types in EF Core.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Filters a sequence to include only elements where the specified Option property has a value.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The type contained in the Option property.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="propertySelector">A selector for the Option property.</param>
    /// <returns>A filtered queryable containing only elements where the Option property has a value.</returns>
    public static IQueryable<TSource> WhereSome<TSource, TProperty>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, Option<TProperty>>> propertySelector)
    {
        var parameter = propertySelector.Parameters[0];
        var optionAccess = propertySelector.Body;

        // Access the IsSome property
        var isSomeProperty = typeof(Option<TProperty>).GetProperty(nameof(Option<TProperty>.IsSome))!;
        var isSomeAccess = Expression.Property(optionAccess, isSomeProperty);

        var lambda = Expression.Lambda<Func<TSource, bool>>(isSomeAccess, parameter);
        return source.Where(lambda);
    }

    /// <summary>
    /// Filters a sequence to include only elements where the specified Option property has no value.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The type contained in the Option property.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="propertySelector">A selector for the Option property.</param>
    /// <returns>A filtered queryable containing only elements where the Option property has no value.</returns>
    public static IQueryable<TSource> WhereNone<TSource, TProperty>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, Option<TProperty>>> propertySelector)
    {
        var parameter = propertySelector.Parameters[0];
        var optionAccess = propertySelector.Body;

        // Access the IsNone property
        var isNoneProperty = typeof(Option<TProperty>).GetProperty(nameof(Option<TProperty>.IsNone))!;
        var isNoneAccess = Expression.Property(optionAccess, isNoneProperty);

        var lambda = Expression.Lambda<Func<TSource, bool>>(isNoneAccess, parameter);
        return source.Where(lambda);
    }

    /// <summary>
    /// Projects each element's Option property to its contained value, filtering out None values.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The type contained in the Option property.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="propertySelector">A selector for the Option property.</param>
    /// <returns>A queryable of the unwrapped values from Some options.</returns>
    public static IQueryable<TProperty> SelectSome<TSource, TProperty>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, Option<TProperty>>> propertySelector)
    {
        var parameter = propertySelector.Parameters[0];
        var optionAccess = propertySelector.Body;

        // First filter to only Some values
        var isSomeProperty = typeof(Option<TProperty>).GetProperty(nameof(Option<TProperty>.IsSome))!;
        var isSomeAccess = Expression.Property(optionAccess, isSomeProperty);
        var filterLambda = Expression.Lambda<Func<TSource, bool>>(isSomeAccess, parameter);

        // Then get the value
        var unwrapMethod = typeof(Option<TProperty>).GetMethod(nameof(Option<TProperty>.GetValue))!;
        var unwrapCall = Expression.Call(optionAccess, unwrapMethod);
        var selectLambda = Expression.Lambda<Func<TSource, TProperty>>(unwrapCall, parameter);

        return source.Where(filterLambda).Select(selectLambda);
    }

    /// <summary>
    /// Gets the first element from a sequence, wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <returns>Some containing the first element, or None if empty.</returns>
    public static Option<TSource> FirstOrNone<TSource>(this IQueryable<TSource> source)
    {
        var result = source.Take(1).ToList();
        return result.Count > 0 ? Option<TSource>.Some(result[0]) : Option<TSource>.None();
    }

    /// <summary>
    /// Gets the first element from a sequence that satisfies a condition, wrapped in an Option.
    /// Returns None if no element satisfies the condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Some containing the first matching element, or None if none match.</returns>
    public static Option<TSource> FirstOrNone<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate)
    {
        var result = source.Where(predicate).Take(1).ToList();
        return result.Count > 0 ? Option<TSource>.Some(result[0]) : Option<TSource>.None();
    }

    /// <summary>
    /// Gets the single element from a sequence, wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// Throws if there is more than one element.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <returns>Some containing the single element, or None if empty.</returns>
    public static Option<TSource> SingleOrNone<TSource>(this IQueryable<TSource> source)
    {
        var result = source.Take(2).ToList();

        return result.Count switch
        {
            0 => Option<TSource>.None(),
            1 => Option<TSource>.Some(result[0]),
            _ => throw new InvalidOperationException("Sequence contains more than one element")
        };
    }

    /// <summary>
    /// Gets the single element from a sequence that satisfies a condition, wrapped in an Option.
    /// Returns None if no element satisfies the condition.
    /// Throws if more than one element satisfies the condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Some containing the single matching element, or None if none match.</returns>
    public static Option<TSource> SingleOrNone<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate)
    {
        var result = source.Where(predicate).Take(2).ToList();

        return result.Count switch
        {
            0 => Option<TSource>.None(),
            1 => Option<TSource>.Some(result[0]),
            _ => throw new InvalidOperationException("Sequence contains more than one matching element")
        };
    }

    /// <summary>
    /// Gets the element at the specified index from a sequence, wrapped in an Option.
    /// Returns None if the index is out of range.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>Some containing the element at the index, or None if out of range.</returns>
    public static Option<TSource> ElementAtOrNone<TSource>(this IQueryable<TSource> source, int index)
    {
        if (index < 0)
            return Option<TSource>.None();

        var result = source.Skip(index).Take(1).ToList();
        return result.Count > 0 ? Option<TSource>.Some(result[0]) : Option<TSource>.None();
    }

    /// <summary>
    /// Gets the last element from a sequence, wrapped in an Option.
    /// Returns None if the sequence is empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <returns>Some containing the last element, or None if empty.</returns>
    public static Option<TSource> LastOrNone<TSource>(this IQueryable<TSource> source)
    {
        // For queryables, we need to reverse and take first
        // This may not work for all providers, so we materialize
        var result = source.ToList();
        return result.Count > 0 ? Option<TSource>.Some(result[^1]) : Option<TSource>.None();
    }

    /// <summary>
    /// Gets the last element from a sequence that satisfies a condition, wrapped in an Option.
    /// Returns None if no element satisfies the condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Some containing the last matching element, or None if none match.</returns>
    public static Option<TSource> LastOrNone<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate)
    {
        var result = source.Where(predicate).ToList();
        return result.Count > 0 ? Option<TSource>.Some(result[^1]) : Option<TSource>.None();
    }
}

