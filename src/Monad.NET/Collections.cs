namespace Monad.NET;

/// <summary>
/// Collection extensions for working with sequences of Option&lt;T&gt;, Result&lt;T, E&gt;, and Either&lt;L, R&gt;.
/// </summary>
public static class MonadCollectionExtensions
{
    #region Option Collections

    /// <summary>
    /// Transposes an IEnumerable&lt;Option&lt;T&gt;&gt; to Option&lt;IEnumerable&lt;T&gt;&gt;.
    /// Returns Some if all options are Some, otherwise None.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    public static Option<IEnumerable<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var result = new List<T>();
        
        foreach (var option in options)
        {
            if (option.IsNone)
                return Option<IEnumerable<T>>.None();
            
            result.Add(option.Unwrap());
        }

        return Option<IEnumerable<T>>.Some(result);
    }

    /// <summary>
    /// Maps each element to an Option and sequences the results.
    /// Returns Some of all values if all mappings succeed, otherwise None.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    public static Option<IEnumerable<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, Option<U>> selector)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        var result = new List<U>();

        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsNone)
                return Option<IEnumerable<U>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IEnumerable<U>>.Some(result);
    }

    /// <summary>
    /// Filters and unwraps Some values from a sequence of Options.
    /// Similar to Rust's filter_map.
    /// </summary>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

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
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

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
        if (options is null)
            throw new ArgumentNullException(nameof(options));

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
    /// Transposes an IEnumerable&lt;Result&lt;T, E&gt;&gt; to Result&lt;IEnumerable&lt;T&gt;, E&gt;.
    /// Returns Ok with all values if all results are Ok, otherwise returns the first Err.
    /// Also known as 'sequence' in Haskell.
    /// </summary>
    public static Result<IEnumerable<T>, TErr> Sequence<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var list = new List<T>();

        foreach (var result in results)
        {
            if (result.IsErr)
                return Result<IEnumerable<T>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IEnumerable<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Maps each element to a Result and sequences the results.
    /// Returns Ok of all values if all mappings succeed, otherwise returns the first error.
    /// Also known as 'traverse' in Haskell.
    /// </summary>
    public static Result<IEnumerable<U>, TErr> Traverse<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Result<U, TErr>> selector)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        var list = new List<U>();

        foreach (var item in source)
        {
            var result = selector(item);
            if (result.IsErr)
                return Result<IEnumerable<U>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IEnumerable<U>, TErr>.Ok(list);
    }

    /// <summary>
    /// Collects all Ok values from a sequence of Results.
    /// Discards all Err values.
    /// </summary>
    public static IEnumerable<T> CollectOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

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
        if (results is null)
            throw new ArgumentNullException(nameof(results));

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
    public static (IEnumerable<T> Oks, IEnumerable<TErr> Errors) Partition<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

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
    /// </summary>
    public static Result<T, TErr> FirstOk<T, TErr>(this IEnumerable<Result<T, TErr>> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        Result<T, TErr>? lastErr = null;

        foreach (var result in results)
        {
            if (result.IsOk)
                return result;
            
            lastErr = result;
        }

        return lastErr ?? Result<T, TErr>.Err(default(TErr)!);
    }

    #endregion

    #region Either Collections

    /// <summary>
    /// Collects all Right values from a sequence of Eithers.
    /// </summary>
    public static IEnumerable<TRight> CollectRights<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        if (eithers is null)
            throw new ArgumentNullException(nameof(eithers));

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
        if (eithers is null)
            throw new ArgumentNullException(nameof(eithers));

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
    public static (IEnumerable<TLeft> Lefts, IEnumerable<TRight> Rights) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        if (eithers is null)
            throw new ArgumentNullException(nameof(eithers));

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
    public static async Task<Option<IEnumerable<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks)
    {
        if (optionTasks is null)
            throw new ArgumentNullException(nameof(optionTasks));

        var result = new List<T>();

        foreach (var task in optionTasks)
        {
            var option = await task;
            if (option.IsNone)
                return Option<IEnumerable<T>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IEnumerable<T>>.Some(result);
    }

    /// <summary>
    /// Async version of Traverse for Options.
    /// </summary>
    public static async Task<Option<IEnumerable<U>>> TraverseAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> selector)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        var result = new List<U>();

        foreach (var item in source)
        {
            var option = await selector(item);
            if (option.IsNone)
                return Option<IEnumerable<U>>.None();

            result.Add(option.Unwrap());
        }

        return Option<IEnumerable<U>>.Some(result);
    }

    /// <summary>
    /// Async version of Sequence for Results.
    /// </summary>
    public static async Task<Result<IEnumerable<T>, TErr>> SequenceAsync<T, TErr>(
        this IEnumerable<Task<Result<T, TErr>>> resultTasks)
    {
        if (resultTasks is null)
            throw new ArgumentNullException(nameof(resultTasks));

        var list = new List<T>();

        foreach (var task in resultTasks)
        {
            var result = await task;
            if (result.IsErr)
                return Result<IEnumerable<T>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IEnumerable<T>, TErr>.Ok(list);
    }

    /// <summary>
    /// Async version of Traverse for Results.
    /// </summary>
    public static async Task<Result<IEnumerable<U>, TErr>> TraverseAsync<T, U, TErr>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, TErr>>> selector)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        var list = new List<U>();

        foreach (var item in source)
        {
            var result = await selector(item);
            if (result.IsErr)
                return Result<IEnumerable<U>, TErr>.Err(result.UnwrapErr());

            list.Add(result.Unwrap());
        }

        return Result<IEnumerable<U>, TErr>.Ok(list);
    }

    #endregion
}

