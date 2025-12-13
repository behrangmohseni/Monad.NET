namespace Monad.NET;

/// <summary>
/// Async extensions for Option&lt;T&gt; to work seamlessly with Task-based asynchronous code.
/// </summary>
public static class OptionAsyncExtensions
{
    /// <summary>
    /// Maps the value inside a Task&lt;Option&lt;T&gt;&gt; using an async function.
    /// </summary>
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var option = await optionTask;
        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.Unwrap());
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Maps the value inside a Task&lt;Option&lt;T&gt;&gt; using a synchronous function.
    /// </summary>
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var option = await optionTask;
        return option.Map(mapper);
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using an async predicate.
    /// </summary>
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var option = await optionTask;
        if (!option.IsSome)
            return Option<T>.None();

        var value = option.Unwrap();
        var passes = await predicate(value);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using a synchronous predicate.
    /// </summary>
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var option = await optionTask;
        return option.Filter(predicate);
    }

    /// <summary>
    /// Chains an async operation on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Option<U>> AndThenAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<Option<U>>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        var option = await optionTask;
        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.Unwrap());
    }

    /// <summary>
    /// Chains a synchronous operation on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Option<U>> AndThenAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Option<U>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        var option = await optionTask;
        return option.AndThen(binder);
    }

    /// <summary>
    /// Returns the Option value or a default computed asynchronously.
    /// </summary>
    public static async Task<T> UnwrapOrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<T>> defaultFunc)
    {
        if (defaultFunc is null)
            throw new ArgumentNullException(nameof(defaultFunc));

        var option = await optionTask;
        if (option.IsSome)
            return option.Unwrap();

        return await defaultFunc();
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt; with async handlers.
    /// </summary>
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> someFunc,
        Func<Task<U>> noneFunc)
    {
        if (someFunc is null)
            throw new ArgumentNullException(nameof(someFunc));
        if (noneFunc is null)
            throw new ArgumentNullException(nameof(noneFunc));

        var option = await optionTask;
        if (option.IsSome)
            return await someFunc(option.Unwrap());

        return await noneFunc();
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt; with synchronous handlers.
    /// </summary>
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> someFunc,
        Func<U> noneFunc)
    {
        if (someFunc is null)
            throw new ArgumentNullException(nameof(someFunc));
        if (noneFunc is null)
            throw new ArgumentNullException(nameof(noneFunc));

        var option = await optionTask;
        return option.Match(someFunc, noneFunc);
    }

    /// <summary>
    /// Executes an async action if the option is Some, allowing method chaining.
    /// </summary>
    public static async Task<Option<T>> TapAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var option = await optionTask;
        if (option.IsSome)
            await action(option.Unwrap());

        return option;
    }

    /// <summary>
    /// Converts a Task&lt;Option&lt;T&gt;&gt; to a Task&lt;Result&lt;T, E&gt;&gt; with async error function.
    /// </summary>
    public static async Task<Result<T, TErr>> OkOrElseAsync<T, TErr>(
        this Task<Option<T>> optionTask,
        Func<Task<TErr>> errFunc)
    {
        if (errFunc is null)
            throw new ArgumentNullException(nameof(errFunc));

        var option = await optionTask;
        if (option.IsSome)
            return Result<T, TErr>.Ok(option.Unwrap());

        return Result<T, TErr>.Err(await errFunc());
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; to Task&lt;Option&lt;U&gt;&gt; by applying an async function.
    /// </summary>
    public static async Task<Option<U>> MapAsync<T, U>(
        this Option<T> option,
        Func<T, Task<U>> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        if (!option.IsSome)
            return Option<U>.None();

        var result = await mapper(option.Unwrap());
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Chains an async operation on an Option&lt;T&gt;.
    /// </summary>
    public static async Task<Option<U>> AndThenAsync<T, U>(
        this Option<T> option,
        Func<T, Task<Option<U>>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));

        if (!option.IsSome)
            return Option<U>.None();

        return await binder(option.Unwrap());
    }

    /// <summary>
    /// Filters an Option&lt;T&gt; using an async predicate.
    /// </summary>
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (!option.IsSome)
            return Option<T>.None();

        var value = option.Unwrap();
        var passes = await predicate(value);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Wraps a value in a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    public static Task<Option<T>> AsTask<T>(this Option<T> option)
    {
        return Task.FromResult(option);
    }
}

