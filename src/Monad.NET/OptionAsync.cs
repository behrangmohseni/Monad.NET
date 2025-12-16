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
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to map.</param>
    /// <param name="mapper">An async function to apply to the value if Some.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
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
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to map.</param>
    /// <param name="mapper">A function to apply to the value if Some.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to filter.</param>
    /// <param name="predicate">An async predicate to test the value.</param>
    /// <returns>A task containing the original Some if the predicate passes, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to filter.</param>
    /// <param name="predicate">A predicate to test the value.</param>
    /// <returns>A task containing the original Some if the predicate passes, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to chain.</param>
    /// <param name="binder">An async function that returns a new option based on the value.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to chain.</param>
    /// <param name="binder">A function that returns a new option based on the value.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option.</param>
    /// <param name="defaultFunc">An async function to compute the default value if None.</param>
    /// <returns>A task containing the value if Some, or the computed default if None.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="U">The type of the result.</typeparam>
    /// <param name="optionTask">The task containing the option to match.</param>
    /// <param name="someFunc">An async function to call if Some.</param>
    /// <param name="noneFunc">An async function to call if None.</param>
    /// <returns>A task containing the result of the matched handler.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="U">The type of the result.</typeparam>
    /// <param name="optionTask">The task containing the option to match.</param>
    /// <param name="someFunc">A function to call if Some.</param>
    /// <param name="noneFunc">A function to call if None.</param>
    /// <returns>A task containing the result of the matched handler.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option.</param>
    /// <param name="action">An async action to execute if Some.</param>
    /// <returns>A task containing the original option, unchanged.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="optionTask">The task containing the option to convert.</param>
    /// <param name="errFunc">An async function to compute the error if None.</param>
    /// <returns>A task containing Ok with the value if Some, or Err with the computed error if None.</returns>
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
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="option">The option to map.</param>
    /// <param name="mapper">An async function to apply to the value if Some.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
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
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="option">The option to chain.</param>
    /// <param name="binder">An async function that returns a new option based on the value.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">An async predicate to test the value.</param>
    /// <returns>A task containing the original Some if the predicate passes, otherwise None.</returns>
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
    /// Wraps an Option&lt;T&gt; in a completed Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to wrap.</param>
    /// <returns>A completed task containing the option.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Option<T>> AsTask<T>(this Option<T> option)
    {
        return Task.FromResult(option);
    }

    /// <summary>
    /// Executes an async action if the option is None, allowing method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option.</param>
    /// <param name="action">An async action to execute if None.</param>
    /// <returns>A task containing the original option, unchanged.</returns>
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
    /// <typeparam name="T">The type of the first option's value.</typeparam>
    /// <typeparam name="U">The type of the second option's value.</typeparam>
    /// <param name="firstTask">The first option task.</param>
    /// <param name="secondTask">The second option task.</param>
    /// <returns>A task containing Some with a tuple of both values if both are Some, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the first option's value.</typeparam>
    /// <typeparam name="U">The type of the second option's value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="firstTask">The first option task.</param>
    /// <param name="secondTask">The second option task.</param>
    /// <param name="combiner">A function to combine the values if both are Some.</param>
    /// <returns>A task containing Some with the combined value if both are Some, otherwise None.</returns>
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
    /// <typeparam name="T">The type of the value in the options.</typeparam>
    /// <param name="optionTasks">The collection of option tasks to search.</param>
    /// <returns>A task containing the first Some option found, or None if all are None.</returns>
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
