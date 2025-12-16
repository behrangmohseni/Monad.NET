using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Async extensions for Option&lt;T&gt; to work seamlessly with Task-based asynchronous code.
/// </summary>
public static class OptionAsyncExtensions
{
    /// <summary>
    /// Maps the value inside a Task&lt;Option&lt;T&gt;&gt; using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.Unwrap()).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Maps the value inside a Task&lt;Option&lt;T&gt;&gt; using a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var option = await optionTask.ConfigureAwait(false);
        return option.Map(mapper);
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using an async predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(predicate);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<T>.None();

        var value = option.Unwrap();
        var passes = await predicate(value).ConfigureAwait(false);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using a synchronous predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(predicate);

        var option = await optionTask.ConfigureAwait(false);
        return option.Filter(predicate);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> AndThenAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<Option<U>>> binder)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(binder);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.Unwrap()).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains a synchronous operation on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> AndThenAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Option<U>> binder)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(binder);

        var option = await optionTask.ConfigureAwait(false);
        return option.AndThen(binder);
    }

    /// <summary>
    /// Returns the Option value or a default computed asynchronously.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> UnwrapOrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<T>> defaultFunc)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(defaultFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option.Unwrap();

        return await defaultFunc().ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt; with async handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> someFunc,
        Func<Task<U>> noneFunc)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(someFunc);
        ArgumentNullException.ThrowIfNull(noneFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return await someFunc(option.Unwrap()).ConfigureAwait(false);

        return await noneFunc().ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt; with synchronous handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> someFunc,
        Func<U> noneFunc)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(someFunc);
        ArgumentNullException.ThrowIfNull(noneFunc);

        var option = await optionTask.ConfigureAwait(false);
        return option.Match(someFunc, noneFunc);
    }

    /// <summary>
    /// Executes an async action if the option is Some, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(action);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            await action(option.Unwrap()).ConfigureAwait(false);

        return option;
    }

    /// <summary>
    /// Converts a Task&lt;Option&lt;T&gt;&gt; to a Task&lt;Result&lt;T, E&gt;&gt; with async error function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> OkOrElseAsync<T, TErr>(
        this Task<Option<T>> optionTask,
        Func<Task<TErr>> errFunc)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(errFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return Result<T, TErr>.Ok(option.Unwrap());

        return Result<T, TErr>.Err(await errFunc().ConfigureAwait(false));
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; to Task&lt;Option&lt;U&gt;&gt; by applying an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Option<T> option,
        Func<T, Task<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.Unwrap()).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Chains an async operation on an Option&lt;T&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> AndThenAsync<T, U>(
        this Option<T> option,
        Func<T, Task<Option<U>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.Unwrap()).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters an Option&lt;T&gt; using an async predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (!option.IsSome)
            return Option<T>.None();

        var value = option.Unwrap();
        var passes = await predicate(value).ConfigureAwait(false);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Wraps a value in a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Option<T>> AsTask<T>(this Option<T> option)
    {
        return Task.FromResult(option);
    }

    /// <summary>
    /// Executes an async action if the option is None, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapNoneAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(optionTask);
        ArgumentNullException.ThrowIfNull(action);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsNone)
            await action().ConfigureAwait(false);

        return option;
    }

    /// <summary>
    /// Asynchronously zips two Option tasks into a single Option containing a tuple.
    /// If both are Some, returns Some((T, U)). Otherwise, returns None.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<(T, U)>> ZipAsync<T, U>(
        Task<Option<T>> firstTask,
        Task<Option<U>> secondTask)
    {
        ArgumentNullException.ThrowIfNull(firstTask);
        ArgumentNullException.ThrowIfNull(secondTask);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.IsSome && result2.IsSome
            ? Option<(T, U)>.Some((result1.Unwrap(), result2.Unwrap()))
            : Option<(T, U)>.None();
    }

    /// <summary>
    /// Asynchronously zips two Option tasks using a combiner function.
    /// If both are Some, applies the combiner. Otherwise, returns None.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<V>> ZipWithAsync<T, U, V>(
        Task<Option<T>> firstTask,
        Task<Option<U>> secondTask,
        Func<T, U, V> combiner)
    {
        ArgumentNullException.ThrowIfNull(firstTask);
        ArgumentNullException.ThrowIfNull(secondTask);
        ArgumentNullException.ThrowIfNull(combiner);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.IsSome && result2.IsSome
            ? Option<V>.Some(combiner(result1.Unwrap(), result2.Unwrap()))
            : Option<V>.None();
    }

    /// <summary>
    /// Returns the first Some option from a collection of Option tasks, or None if all are None.
    /// </summary>
    public static async Task<Option<T>> FirstSomeAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks)
    {
        ArgumentNullException.ThrowIfNull(optionTasks);

        foreach (var task in optionTasks)
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                return option;
        }
        return Option<T>.None();
    }
}
