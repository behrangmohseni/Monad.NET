using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
    #region Validation Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Validation&lt;T, E&gt;&gt; to Validation&lt;IReadOnlyList&lt;T&gt;, E&gt;.
    /// Returns Valid with all values if all validations are Valid, otherwise Invalid with ALL accumulated errors.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of valid values in the validations.</typeparam>
    /// <typeparam name="TErr">The type of error values in the validations.</typeparam>
    /// <param name="validations">The sequence of validations to transpose.</param>
    /// <returns>Valid containing all values if all validations are Valid; otherwise Invalid with all accumulated errors.</returns>
    /// <example>
    /// <code>
    /// var validations = new[]
    /// {
    ///     Validation&lt;int, string&gt;.Valid(1),
    ///     Validation&lt;int, string&gt;.Invalid("error1"),
    ///     Validation&lt;int, string&gt;.Invalid("error2")
    /// };
    /// 
    /// var result = validations.Sequence();
    /// // Invalid with errors: ["error1", "error2"]
    /// </code>
    /// </example>
    public static Validation<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        this IEnumerable<Validation<T, TErr>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

        var values = CollectionHelper.CreateListWithCapacity<Validation<T, TErr>, T>(validations);
        var errors = new List<TErr>();

        foreach (var validation in validations)
        {
            if (validation.IsValid)
            {
                values.Add(validation.GetValue());
            }
            else
            {
                errors.AddRange(validation.GetErrors());
            }
        }

        return errors.Count > 0
            ? Validation<IReadOnlyList<T>, TErr>.Invalid(errors)
            : Validation<IReadOnlyList<T>, TErr>.Valid(values);
    }

    /// <summary>
    /// Maps each element to a Validation and sequences the results.
    /// Returns Valid of all values if all mappings succeed, otherwise Invalid with ALL accumulated errors.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="U">The type of valid values in the resulting validations.</typeparam>
    /// <typeparam name="TErr">The type of error values in the resulting validations.</typeparam>
    /// <param name="source">The source sequence to traverse.</param>
    /// <param name="selector">A function that maps each element to a Validation.</param>
    /// <returns>Valid containing all mapped values if all mappings return Valid; otherwise Invalid with all accumulated errors.</returns>
    /// <example>
    /// <code>
    /// var items = new[] { "1", "abc", "2", "xyz" };
    /// 
    /// var result = items.Traverse(s =>
    ///     int.TryParse(s, out var n)
    ///         ? Validation&lt;int, string&gt;.Valid(n)
    ///         : Validation&lt;int, string&gt;.Invalid($"'{s}' is not a number"));
    /// 
    /// // Invalid with errors: ["'abc' is not a number", "'xyz' is not a number"]
    /// </code>
    /// </example>
    public static Validation<IReadOnlyList<U>, TErr> Traverse<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Validation<U, TErr>> selector)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(selector);

        var values = CollectionHelper.CreateListWithCapacity<T, U>(source);
        var errors = new List<TErr>();

        foreach (var item in source)
        {
            var validation = selector(item);
            if (validation.IsValid)
            {
                values.Add(validation.GetValue());
            }
            else
            {
                errors.AddRange(validation.GetErrors());
            }
        }

        return errors.Count > 0
            ? Validation<IReadOnlyList<U>, TErr>.Invalid(errors)
            : Validation<IReadOnlyList<U>, TErr>.Valid(values);
    }

    /// <summary>
    /// Collects all Valid values from a sequence of Validations.
    /// Discards all Invalid values.
    /// </summary>
    /// <typeparam name="T">The type of valid values in the validations.</typeparam>
    /// <typeparam name="TErr">The type of error values in the validations.</typeparam>
    /// <param name="validations">The sequence of validations to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Valid values.</returns>
    public static IEnumerable<T> CollectValid<T, TErr>(this IEnumerable<Validation<T, TErr>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

        foreach (var validation in validations)
        {
            if (validation.IsValid)
                yield return validation.GetValue();
        }
    }

    /// <summary>
    /// Collects all errors from Invalid validations in a sequence.
    /// Discards all Valid values.
    /// </summary>
    /// <typeparam name="T">The type of valid values in the validations.</typeparam>
    /// <typeparam name="TErr">The type of error values in the validations.</typeparam>
    /// <param name="validations">The sequence of validations to collect from.</param>
    /// <returns>An enumerable containing all errors from Invalid validations (flattened).</returns>
    public static IEnumerable<TErr> CollectErrors<T, TErr>(this IEnumerable<Validation<T, TErr>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

        foreach (var validation in validations)
        {
            if (validation.IsInvalid)
            {
                foreach (var error in validation.GetErrors())
                    yield return error;
            }
        }
    }

    /// <summary>
    /// Partitions a sequence of Validations into Valid values and all accumulated errors.
    /// </summary>
    /// <typeparam name="T">The type of valid values in the validations.</typeparam>
    /// <typeparam name="TErr">The type of error values in the validations.</typeparam>
    /// <param name="validations">The sequence of validations to partition.</param>
    /// <returns>A tuple containing a list of Valid values and a list of all errors (flattened).</returns>
    public static (IReadOnlyList<T> Valids, IReadOnlyList<TErr> Errors) Partition<T, TErr>(
        this IEnumerable<Validation<T, TErr>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

        CollectionHelper.TryGetNonEnumeratedCount(validations, out var count);
        var valids = new List<T>(count > 0 ? count : 4);
        var errors = new List<TErr>(count > 0 ? count : 4);

        foreach (var validation in validations)
        {
            if (validation.IsValid)
            {
                valids.Add(validation.GetValue());
            }
            else
            {
                errors.AddRange(validation.GetErrors());
            }
        }

        return (valids, errors);
    }

    #endregion
}
