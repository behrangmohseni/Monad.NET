using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Async extensions for Option&lt;T&gt; to work seamlessly with Task-based asynchronous code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(mapper);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.GetValue()).ConfigureAwait(false);
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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(mapper);

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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(predicate);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<T>.None();

        var value = option.GetValue();
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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(predicate);

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
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<Option<U>>> binder)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(binder);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.GetValue()).ConfigureAwait(false);
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
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Option<U>> binder)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(binder);

        var option = await optionTask.ConfigureAwait(false);
        return option.Bind(binder);
    }

    /// <summary>
    /// Returns the Option value or a default computed asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option.</param>
    /// <param name="defaultFunc">An async function to compute the default value if None.</param>
    /// <returns>A task containing the value if Some, or the computed default if None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<T>> defaultFunc)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(defaultFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option.GetValue();

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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return await someFunc(option.GetValue()).ConfigureAwait(false);

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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);

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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(action);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            await action(option.GetValue()).ConfigureAwait(false);

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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(errFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return Result<T, TErr>.Ok(option.GetValue());

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
        ThrowHelper.ThrowIfNull(mapper);

        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.GetValue()).ConfigureAwait(false);
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
    public static async Task<Option<U>> BindAsync<T, U>(
        this Option<T> option,
        Func<T, Task<Option<U>>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.GetValue()).ConfigureAwait(false);
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
        ThrowHelper.ThrowIfNull(predicate);

        if (!option.IsSome)
            return Option<T>.None();

        var value = option.GetValue();
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
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(action);

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
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.IsSome && result2.IsSome
            ? Option<(T, U)>.Some((result1.GetValue(), result2.GetValue()))
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
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);

        var result1 = await firstTask.ConfigureAwait(false);
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.IsSome && result2.IsSome
            ? Option<V>.Some(combiner(result1.GetValue(), result2.GetValue()))
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
        ThrowHelper.ThrowIfNull(optionTasks);

        foreach (var task in optionTasks)
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                return option;
        }
        return Option<T>.None();
    }

    /// <summary>
    /// Returns the Option if Some, otherwise returns the alternative computed asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="alternativeAsync">An async function to compute the alternative if None.</param>
    /// <returns>A task containing the original Some or the computed alternative.</returns>
    /// <example>
    /// <code>
    /// var result = await option.OrElseAsync(async () => await FetchDefaultAsync());
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrElseAsync<T>(
        this Option<T> option,
        Func<Task<Option<T>>> alternativeAsync)
    {
        ThrowHelper.ThrowIfNull(alternativeAsync);

        if (option.IsSome)
            return option;

        return await alternativeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Option if Some, otherwise returns the alternative computed asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to check.</param>
    /// <param name="alternativeAsync">An async function to compute the alternative if None.</param>
    /// <returns>A task containing the original Some or the computed alternative.</returns>
    /// <example>
    /// <code>
    /// var result = await optionTask.OrElseAsync(async () => await FetchDefaultAsync());
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<Option<T>>> alternativeAsync)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(alternativeAsync);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option;

        return await alternativeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Option if Some, otherwise returns the alternative.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to check.</param>
    /// <param name="alternative">The alternative to return if None.</param>
    /// <returns>A task containing the original Some or the alternative.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrAsync<T>(
        this Task<Option<T>> optionTask,
        Option<T> alternative)
    {
        ThrowHelper.ThrowIfNull(optionTask);

        var option = await optionTask.ConfigureAwait(false);
        return option.IsSome ? option : alternative;
    }

    #region ValueTask Overloads
    // ValueTask overloads provide better performance when the result is often 
    // available synchronously or when frequent allocations need to be avoided.

    /// <summary>
    /// Maps the value inside a ValueTask&lt;Option&lt;T&gt;&gt; using a synchronous function.
    /// Optimized for scenarios where the option is frequently None or already completed.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The value task containing the option to map.</param>
    /// <param name="mapper">A function to apply to the value if Some.</param>
    /// <returns>A value task containing Some with the mapped value, or None if the original was None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<U>> MapAsync<T, U>(
        this ValueTask<Option<T>> optionTask,
        Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (optionTask.IsCompletedSuccessfully)
        {
            var option = optionTask.Result;
            return new ValueTask<Option<U>>(option.Map(mapper));
        }

        return MapAsyncCore(optionTask, mapper);

        static async ValueTask<Option<U>> MapAsyncCore(ValueTask<Option<T>> task, Func<T, U> m)
        {
            var option = await task.ConfigureAwait(false);
            return option.Map(m);
        }
    }

    /// <summary>
    /// Maps the value inside a ValueTask&lt;Option&lt;T&gt;&gt; using an async function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The value task containing the option to map.</param>
    /// <param name="mapper">An async function to apply to the value if Some.</param>
    /// <returns>A value task containing Some with the mapped value, or None if the original was None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Option<U>> MapAsync<T, U>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.GetValue()).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Chains a synchronous operation on a ValueTask&lt;Option&lt;T&gt;&gt;.
    /// Optimized for scenarios where the option is frequently None or already completed.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The value task containing the option to chain.</param>
    /// <param name="binder">A function that returns a new option based on the value.</param>
    /// <returns>A value task containing the result of the binder if Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<U>> BindAsync<T, U>(
        this ValueTask<Option<T>> optionTask,
        Func<T, Option<U>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (optionTask.IsCompletedSuccessfully)
        {
            var option = optionTask.Result;
            return new ValueTask<Option<U>>(option.Bind(binder));
        }

        return BindAsyncCore(optionTask, binder);

        static async ValueTask<Option<U>> BindAsyncCore(ValueTask<Option<T>> task, Func<T, Option<U>> b)
        {
            var option = await task.ConfigureAwait(false);
            return option.Bind(b);
        }
    }

    /// <summary>
    /// Chains an async operation on a ValueTask&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The value task containing the option to chain.</param>
    /// <param name="binder">An async function that returns a new option based on the value.</param>
    /// <returns>A value task containing the result of the binder if Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Option<U>> BindAsync<T, U>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask<Option<U>>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters a ValueTask&lt;Option&lt;T&gt;&gt; using a synchronous predicate.
    /// Optimized for scenarios where the option is frequently None or already completed.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The value task containing the option to filter.</param>
    /// <param name="predicate">A predicate to test the value.</param>
    /// <returns>A value task containing the original Some if the predicate passes, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<T>> FilterAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);

        if (optionTask.IsCompletedSuccessfully)
        {
            var option = optionTask.Result;
            return new ValueTask<Option<T>>(option.Filter(predicate));
        }

        return FilterAsyncCore(optionTask, predicate);

        static async ValueTask<Option<T>> FilterAsyncCore(ValueTask<Option<T>> task, Func<T, bool> p)
        {
            var option = await task.ConfigureAwait(false);
            return option.Filter(p);
        }
    }

    /// <summary>
    /// Pattern matches on a ValueTask&lt;Option&lt;T&gt;&gt; with synchronous handlers.
    /// Optimized for scenarios where the option is frequently None or already completed.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="U">The type of the result.</typeparam>
    /// <param name="optionTask">The value task containing the option to match.</param>
    /// <param name="someFunc">A function to call if Some.</param>
    /// <param name="noneFunc">A function to call if None.</param>
    /// <returns>A value task containing the result of the matched handler.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, U>(
        this ValueTask<Option<T>> optionTask,
        Func<T, U> someFunc,
        Func<U> noneFunc)
    {
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);

        if (optionTask.IsCompletedSuccessfully)
        {
            var option = optionTask.Result;
            return new ValueTask<U>(option.Match(someFunc, noneFunc));
        }

        return MatchAsyncCore(optionTask, someFunc, noneFunc);

        static async ValueTask<U> MatchAsyncCore(ValueTask<Option<T>> task, Func<T, U> some, Func<U> none)
        {
            var option = await task.ConfigureAwait(false);
            return option.Match(some, none);
        }
    }

    /// <summary>
    /// Wraps an Option&lt;T&gt; in a completed ValueTask&lt;Option&lt;T&gt;&gt;.
    /// More efficient than Task.FromResult for frequently-called paths.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to wrap.</param>
    /// <returns>A completed value task containing the option.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<T>> AsValueTask<T>(this Option<T> option)
    {
        return new ValueTask<Option<T>>(option);
    }

    /// <summary>
    /// Returns the value or a default from a ValueTask&lt;Option&lt;T&gt;&gt;.
    /// Optimized for scenarios where the option is frequently Some or already completed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> GetValueOrElseAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T> defaultFunc)
    {
        ThrowHelper.ThrowIfNull(defaultFunc);

        if (optionTask.IsCompletedSuccessfully)
        {
            var option = optionTask.Result;
            return new ValueTask<T>(option.GetValueOrElse(defaultFunc));
        }

        return GetValueOrElseAsyncCore(optionTask, defaultFunc);

        static async ValueTask<T> GetValueOrElseAsyncCore(ValueTask<Option<T>> task, Func<T> d)
        {
            var option = await task.ConfigureAwait(false);
            return option.GetValueOrElse(d);
        }
    }

    /// <summary>
    /// Returns the value or a default asynchronously from a ValueTask&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> GetValueOrElseAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<ValueTask<T>> defaultFunc)
    {
        ThrowHelper.ThrowIfNull(defaultFunc);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option.GetValue();

        return await defaultFunc().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a synchronous action if the option is Some from a ValueTask&lt;Option&lt;T&gt;&gt;.
    /// Optimized for scenarios where the option is frequently None or already completed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<T>> TapAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (optionTask.IsCompletedSuccessfully)
        {
            var option = optionTask.Result;
            if (option.IsSome)
                action(option.GetValue());
            return new ValueTask<Option<T>>(option);
        }

        return TapAsyncCore(optionTask, action);

        static async ValueTask<Option<T>> TapAsyncCore(ValueTask<Option<T>> task, Action<T> a)
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                a(option.GetValue());
            return option;
        }
    }

    /// <summary>
    /// Executes an async action if the option is Some from a ValueTask&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Option<T>> TapAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask> action)
    {
        ThrowHelper.ThrowIfNull(action);

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            await action(option.GetValue()).ConfigureAwait(false);

        return option;
    }

    #endregion

    #region CancellationToken Overloads

    /// <summary>
    /// Maps the value inside a Task&lt;Option&lt;T&gt;&gt; using an async function with cancellation support.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to map.</param>
    /// <param name="mapper">An async function to apply to the value if Some.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(mapper);

        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(option.GetValue(), cancellationToken).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Option&lt;T&gt;&gt; with cancellation support.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to chain.</param>
    /// <param name="binder">An async function that returns a new option based on the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, CancellationToken, Task<Option<U>>> binder,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(binder);

        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(option.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt; with async handlers and cancellation support.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="U">The type of the result.</typeparam>
    /// <param name="optionTask">The task containing the option to match.</param>
    /// <param name="someFunc">An async function to call if Some.</param>
    /// <param name="noneFunc">An async function to call if None.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the matched handler.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, CancellationToken, Task<U>> someFunc,
        Func<CancellationToken, Task<U>> noneFunc,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);

        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        if (option.IsSome)
            return await someFunc(option.GetValue(), cancellationToken).ConfigureAwait(false);

        return await noneFunc(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using an async predicate with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, CancellationToken, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<T>.None();

        cancellationToken.ThrowIfCancellationRequested();
        var value = option.GetValue();
        var passes = await predicate(value, cancellationToken).ConfigureAwait(false);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Returns the value or computes a default asynchronously with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<CancellationToken, Task<T>> defaultFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(defaultFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option.GetValue();

        cancellationToken.ThrowIfCancellationRequested();
        return await defaultFunc(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async action if the option is Some with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(option.GetValue(), cancellationToken).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Converts a Task&lt;Option&lt;T&gt;&gt; to a Task&lt;Result&lt;T, E&gt;&gt; with async error function and cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> OkOrElseAsync<T, TErr>(
        this Task<Option<T>> optionTask,
        Func<CancellationToken, Task<TErr>> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(errFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return Result<T, TErr>.Ok(option.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        return Result<T, TErr>.Err(await errFunc(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Executes an async action if the option is None with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapNoneAsync<T>(
        this Task<Option<T>> optionTask,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(cancellationToken).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Asynchronously zips two Option tasks with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<(T, U)>> ZipAsync<T, U>(
        Task<Option<T>> firstTask,
        Task<Option<U>> secondTask,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);

        return result1.IsSome && result2.IsSome
            ? Option<(T, U)>.Some((result1.GetValue(), result2.GetValue()))
            : Option<(T, U)>.None();
    }

    /// <summary>
    /// Asynchronously zips two Option tasks using a combiner function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<V>> ZipWithAsync<T, U, V>(
        Task<Option<T>> firstTask,
        Task<Option<U>> secondTask,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);

        return result1.IsSome && result2.IsSome
            ? Option<V>.Some(combiner(result1.GetValue(), result2.GetValue()))
            : Option<V>.None();
    }

    /// <summary>
    /// Returns the first Some option with cancellation support.
    /// </summary>
    public static async Task<Option<T>> FirstSomeAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(optionTasks);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var task in optionTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                return option;
        }
        return Option<T>.None();
    }

    /// <summary>
    /// Returns the Option if Some, otherwise returns the alternative computed asynchronously with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrElseAsync<T>(
        this Option<T> option,
        Func<CancellationToken, Task<Option<T>>> alternativeAsync,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(alternativeAsync);
        cancellationToken.ThrowIfCancellationRequested();

        if (option.IsSome)
            return option;

        return await alternativeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Option if Some, otherwise returns the alternative computed asynchronously with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<CancellationToken, Task<Option<T>>> alternativeAsync,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(alternativeAsync);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option;

        cancellationToken.ThrowIfCancellationRequested();
        return await alternativeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters an Option&lt;T&gt; using an async predicate with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

        if (!option.IsSome)
            return Option<T>.None();

        var value = option.GetValue();
        var passes = await predicate(value, cancellationToken).ConfigureAwait(false);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; using an async function with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Option<T> option,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.GetValue(), cancellationToken).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Chains an async operation on an Option&lt;T&gt; with cancellation support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> BindAsync<T, U>(
        this Option<T> option,
        Func<T, CancellationToken, Task<Option<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    #endregion
}
