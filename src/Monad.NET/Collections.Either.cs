using Monad.NET.Internal;

namespace Monad.NET;

public static partial class MonadCollectionExtensions
{
    #region Either Collections

    /// <summary>
    /// Collects all Right values from a sequence of Eithers.
    /// </summary>
    /// <typeparam name="TLeft">The type of Left values in the eithers.</typeparam>
    /// <typeparam name="TRight">The type of Right values in the eithers.</typeparam>
    /// <param name="eithers">The sequence of eithers to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Right values.</returns>
    public static IEnumerable<TRight> CollectRights<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ThrowHelper.ThrowIfNull(eithers);

        foreach (var either in eithers)
        {
            if (either.IsRight)
                yield return either.GetRight();
        }
    }

    /// <summary>
    /// Collects all Left values from a sequence of Eithers.
    /// </summary>
    /// <typeparam name="TLeft">The type of Left values in the eithers.</typeparam>
    /// <typeparam name="TRight">The type of Right values in the eithers.</typeparam>
    /// <param name="eithers">The sequence of eithers to collect from.</param>
    /// <returns>An enumerable containing only the unwrapped Left values.</returns>
    public static IEnumerable<TLeft> CollectLefts<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ThrowHelper.ThrowIfNull(eithers);

        foreach (var either in eithers)
        {
            if (either.IsLeft)
                yield return either.GetLeft();
        }
    }

    /// <summary>
    /// Partitions a sequence of Eithers into Left and Right values.
    /// </summary>
    /// <typeparam name="TLeft">The type of Left values in the eithers.</typeparam>
    /// <typeparam name="TRight">The type of Right values in the eithers.</typeparam>
    /// <param name="eithers">The sequence of eithers to partition.</param>
    /// <returns>A tuple containing a list of Left values and a list of Right values.</returns>
    public static (IReadOnlyList<TLeft> Lefts, IReadOnlyList<TRight> Rights) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        ThrowHelper.ThrowIfNull(eithers);

        // Try to get initial capacity for better allocation
        CollectionHelper.TryGetNonEnumeratedCount(eithers, out var count);
        var halfCapacity = count > 0 ? count / 2 : 4;
        var lefts = new List<TLeft>(halfCapacity);
        var rights = new List<TRight>(halfCapacity);

        foreach (var either in eithers)
        {
            if (either.IsLeft)
                lefts.Add(either.GetLeft());
            else
                rights.Add(either.GetRight());
        }

        return (lefts, rights);
    }

    #endregion
}

