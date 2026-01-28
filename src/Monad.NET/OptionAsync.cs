using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents an asynchronous optional value. Wraps Task&lt;Option&lt;T&gt;&gt; for cleaner fluent chaining.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="OptionAsync{T}"/> when working with async operations that return optional values.
/// It provides a cleaner API than working with Task&lt;Option&lt;T&gt;&gt; directly.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Instead of:
/// Task&lt;Option&lt;User&gt;&gt; userTask = GetUserAsync(id);
/// var result = await userTask
///     .MapAsync(u => u.Name)
///     .BindAsync(name => GetOrdersAsync(name));
/// 
/// // Use:
/// var result = await OptionAsync.From(GetUserAsync(id))
///     .Map(u => u.Name)
///     .Bind(name => OptionAsync.From(GetOrdersAsync(name)));
/// </code>
/// </example>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct OptionAsync<T>
{
    private readonly Task<Option<T>> _task;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _task.Status == TaskStatus.RanToCompletion
        ? (_task.Result.IsSome ? $"SomeAsync({_task.Result.GetValue()})" : "NoneAsync")
        : "Pending...";

    /// <summary>
    /// Creates an OptionAsync from a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync(Task<Option<T>> task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
    }

    /// <summary>
    /// Gets the underlying task.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<Option<T>> AsTask() => _task;

    /// <summary>
    /// Gets an awaiter for the underlying task.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TaskAwaiter<Option<T>> GetAwaiter() => _task.GetAwaiter();

    /// <summary>
    /// Gets a configured task awaiter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ConfiguredTaskAwaitable<Option<T>> ConfigureAwait(bool continueOnCapturedContext)
        => _task.ConfigureAwait(continueOnCapturedContext);

    /// <summary>
    /// Maps the value inside the async option using a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<U> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);
        var task = _task; // Capture for local function
        return new OptionAsync<U>(MapCore());

        async Task<Option<U>> MapCore()
        {
            var option = await task.ConfigureAwait(false);
            return option.Map(mapper);
        }
    }

    /// <summary>
    /// Maps the value inside the async option using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<U> MapAsync<U>(Func<T, Task<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);
        var task = _task; // Capture for local function
        return new OptionAsync<U>(MapAsyncCore());

        async Task<Option<U>> MapAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (!option.IsSome)
                return Option<U>.None();
            var result = await mapper(option.GetValue()).ConfigureAwait(false);
            return Option<U>.Some(result);
        }
    }

    /// <summary>
    /// Chains another async option computation using a synchronous binder.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<U> Bind<U>(Func<T, Option<U>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);
        var task = _task; // Capture for local function
        return new OptionAsync<U>(BindCore());

        async Task<Option<U>> BindCore()
        {
            var option = await task.ConfigureAwait(false);
            return option.Bind(binder);
        }
    }

    /// <summary>
    /// Chains another async option computation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<U> BindAsync<U>(Func<T, Task<Option<U>>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);
        var task = _task; // Capture for local function
        return new OptionAsync<U>(BindAsyncCore());

        async Task<Option<U>> BindAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (!option.IsSome)
                return Option<U>.None();
            return await binder(option.GetValue()).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Chains another OptionAsync computation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<U> Bind<U>(Func<T, OptionAsync<U>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);
        var task = _task; // Capture for local function
        return new OptionAsync<U>(BindOptionAsyncCore());

        async Task<Option<U>> BindOptionAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (!option.IsSome)
                return Option<U>.None();
            return await binder(option.GetValue()).AsTask().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Filters the value with a synchronous predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> Filter(Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(FilterCore());

        async Task<Option<T>> FilterCore()
        {
            var option = await task.ConfigureAwait(false);
            return option.Filter(predicate);
        }
    }

    /// <summary>
    /// Filters the value with an async predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> FilterAsync(Func<T, Task<bool>> predicate)
    {
        ThrowHelper.ThrowIfNull(predicate);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(FilterAsyncCore());

        async Task<Option<T>> FilterAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (!option.IsSome)
                return option;
            var passes = await predicate(option.GetValue()).ConfigureAwait(false);
            return passes ? option : Option<T>.None();
        }
    }

    /// <summary>
    /// Returns the value if Some, or the specified default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<T> GetValueOr(T defaultValue)
    {
        var option = await _task.ConfigureAwait(false);
        return option.GetValueOr(defaultValue);
    }

    /// <summary>
    /// Returns the value if Some, or computes a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<T> GetValueOrElse(Func<T> defaultFunc)
    {
        ThrowHelper.ThrowIfNull(defaultFunc);
        var option = await _task.ConfigureAwait(false);
        return option.GetValueOrElse(defaultFunc);
    }

    /// <summary>
    /// Returns the value if Some, or computes a default value asynchronously.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<T> GetValueOrElseAsync(Func<Task<T>> defaultFunc)
    {
        ThrowHelper.ThrowIfNull(defaultFunc);
        var option = await _task.ConfigureAwait(false);
        if (option.IsSome)
            return option.GetValue();
        return await defaultFunc().ConfigureAwait(false);
    }

    /// <summary>
    /// Pattern matches on the async option with synchronous handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<U> Match<U>(Func<T, U> someFunc, Func<U> noneFunc)
    {
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);
        var option = await _task.ConfigureAwait(false);
        return option.Match(someFunc, noneFunc);
    }

    /// <summary>
    /// Pattern matches on the async option with async handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<U> MatchAsync<U>(Func<T, Task<U>> someFunc, Func<Task<U>> noneFunc)
    {
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);
        var option = await _task.ConfigureAwait(false);
        if (option.IsSome)
            return await someFunc(option.GetValue()).ConfigureAwait(false);
        return await noneFunc().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action if Some, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> Tap(Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(TapCore());

        async Task<Option<T>> TapCore()
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                action(option.GetValue());
            return option;
        }
    }

    /// <summary>
    /// Executes an async action if Some, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> TapAsync(Func<T, Task> action)
    {
        ThrowHelper.ThrowIfNull(action);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(TapAsyncCore());

        async Task<Option<T>> TapAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                await action(option.GetValue()).ConfigureAwait(false);
            return option;
        }
    }

    /// <summary>
    /// Executes an action if None, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> TapNone(Action action)
    {
        ThrowHelper.ThrowIfNull(action);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(TapNoneCore());

        async Task<Option<T>> TapNoneCore()
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsNone)
                action();
            return option;
        }
    }

    /// <summary>
    /// Executes an async action if None, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> TapNoneAsync(Func<Task> action)
    {
        ThrowHelper.ThrowIfNull(action);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(TapNoneAsyncCore());

        async Task<Option<T>> TapNoneAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsNone)
                await action().ConfigureAwait(false);
            return option;
        }
    }

    /// <summary>
    /// Returns this option if Some, otherwise returns the alternative.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> OrElse(Func<Option<T>> alternative)
    {
        ThrowHelper.ThrowIfNull(alternative);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(OrElseCore());

        async Task<Option<T>> OrElseCore()
        {
            var option = await task.ConfigureAwait(false);
            return option.IsSome ? option : alternative();
        }
    }

    /// <summary>
    /// Returns this option if Some, otherwise returns the async alternative.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> OrElseAsync(Func<Task<Option<T>>> alternative)
    {
        ThrowHelper.ThrowIfNull(alternative);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(OrElseAsyncCore());

        async Task<Option<T>> OrElseAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                return option;
            return await alternative().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns this option if Some, otherwise returns the OptionAsync alternative.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<T> OrElse(Func<OptionAsync<T>> alternative)
    {
        ThrowHelper.ThrowIfNull(alternative);
        var task = _task; // Capture for local function
        return new OptionAsync<T>(OrElseOptionAsyncCore());

        async Task<Option<T>> OrElseOptionAsyncCore()
        {
            var option = await task.ConfigureAwait(false);
            if (option.IsSome)
                return option;
            return await alternative().AsTask().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Converts to a Result, using the provided error if None.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<Result<T, TErr>> ToResult<TErr>(TErr error)
    {
        var option = await _task.ConfigureAwait(false);
        return option.OkOr(error);
    }

    /// <summary>
    /// Converts to a Result, using the factory to create the error if None.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<Result<T, TErr>> ToResult<TErr>(Func<TErr> errorFactory)
    {
        ThrowHelper.ThrowIfNull(errorFactory);
        var option = await _task.ConfigureAwait(false);
        return option.OkOrElse(errorFactory);
    }

    /// <summary>
    /// Zips with another OptionAsync into a tuple.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<(T, U)> Zip<U>(OptionAsync<U> other)
    {
        var task = _task; // Capture for local function
        return new OptionAsync<(T, U)>(ZipCore());

        async Task<Option<(T, U)>> ZipCore()
        {
            var option1 = await task.ConfigureAwait(false);
            if (!option1.IsSome)
                return Option<(T, U)>.None();
            var option2 = await other.AsTask().ConfigureAwait(false);
            if (!option2.IsSome)
                return Option<(T, U)>.None();
            return Option<(T, U)>.Some((option1.GetValue(), option2.GetValue()));
        }
    }

    /// <summary>
    /// Zips with another OptionAsync using a combiner function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OptionAsync<V> ZipWith<U, V>(OptionAsync<U> other, Func<T, U, V> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);
        var task = _task; // Capture for local function
        return new OptionAsync<V>(ZipWithCore());

        async Task<Option<V>> ZipWithCore()
        {
            var option1 = await task.ConfigureAwait(false);
            if (!option1.IsSome)
                return Option<V>.None();
            var option2 = await other.AsTask().ConfigureAwait(false);
            if (!option2.IsSome)
                return Option<V>.None();
            return Option<V>.Some(combiner(option1.GetValue(), option2.GetValue()));
        }
    }

    /// <summary>
    /// Implicit conversion from Task&lt;Option&lt;T&gt;&gt; to OptionAsync&lt;T&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OptionAsync<T>(Task<Option<T>> task)
        => new(task);

    /// <summary>
    /// Implicit conversion from Option&lt;T&gt; to OptionAsync&lt;T&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OptionAsync<T>(Option<T> option)
        => new(Task.FromResult(option));
}

/// <summary>
/// Static factory methods for creating OptionAsync instances.
/// </summary>
public static class OptionAsync
{
    /// <summary>
    /// Creates an OptionAsync containing Some value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> Some<T>(T value)
        => new(Task.FromResult(Option<T>.Some(value)));

    /// <summary>
    /// Creates an OptionAsync containing None.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> None<T>()
        => new(Task.FromResult(Option<T>.None()));

    /// <summary>
    /// Creates an OptionAsync from a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> From<T>(Task<Option<T>> task)
        => new(task);

    /// <summary>
    /// Creates an OptionAsync from an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> From<T>(Option<T> option)
        => new(Task.FromResult(option));

    /// <summary>
    /// Creates an OptionAsync from a nullable reference type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> FromNullable<T>(T? value) where T : class
        => new(Task.FromResult(value.ToOption()));

    /// <summary>
    /// Creates an OptionAsync from a nullable value type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> FromNullable<T>(T? value) where T : struct
        => new(Task.FromResult(value.ToOption()));

    /// <summary>
    /// Creates an OptionAsync from a Task returning a nullable reference type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> FromNullableAsync<T>(Task<T?> task) where T : class
    {
        ThrowHelper.ThrowIfNull(task);
        return new OptionAsync<T>(task.ContinueWith(
            t => t.Result.ToOption(),
            TaskContinuationOptions.ExecuteSynchronously));
    }

    /// <summary>
    /// Creates an OptionAsync from a Task returning a nullable value type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptionAsync<T> FromNullableAsync<T>(Task<T?> task) where T : struct
    {
        ThrowHelper.ThrowIfNull(task);
        return new OptionAsync<T>(task.ContinueWith(
            t => t.Result.ToOption(),
            TaskContinuationOptions.ExecuteSynchronously));
    }
}

/// <summary>
/// Async extensions for Option&lt;T&gt; to work seamlessly with Task-based asynchronous code.
/// All async methods support CancellationToken for proper cancellation handling.
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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        return option.Map(mapper);
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using an async predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to filter.</param>
    /// <param name="predicate">An async predicate to test the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original Some if the predicate passes, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate,
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
        var passes = await predicate(value).ConfigureAwait(false);
        return passes ? option : Option<T>.None();
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; using a synchronous predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to filter.</param>
    /// <param name="predicate">A predicate to test the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original Some if the predicate passes, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<Option<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(option.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains a synchronous operation on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="optionTask">The task containing the option to chain.</param>
    /// <param name="binder">A function that returns a new option based on the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Option<U>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        return option.Bind(binder);
    }

    /// <summary>
    /// Returns the Option value or a default computed asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option.</param>
    /// <param name="defaultFunc">An async function to compute the default value if None.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the value if Some, or the computed default if None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<T>> defaultFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(defaultFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option.GetValue();

        cancellationToken.ThrowIfCancellationRequested();
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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the matched handler.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> someFunc,
        Func<Task<U>> noneFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the matched handler.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> someFunc,
        Func<U> noneFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(someFunc);
        ThrowHelper.ThrowIfNull(noneFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        return option.Match(someFunc, noneFunc);
    }

    /// <summary>
    /// Executes an async action if the option is Some, allowing method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option.</param>
    /// <param name="action">An async action to execute if Some.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original option, unchanged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(option.GetValue()).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Converts a Task&lt;Option&lt;T&gt;&gt; to a Task&lt;Result&lt;T, E&gt;&gt; with async error function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="optionTask">The task containing the option to convert.</param>
    /// <param name="errFunc">An async function to compute the error if None.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Ok with the value if Some, or Err with the computed error if None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, TErr>> OkOrElseAsync<T, TErr>(
        this Task<Option<T>> optionTask,
        Func<Task<TErr>> errFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(errFunc);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return Result<T, TErr>.Ok(option.GetValue());

        cancellationToken.ThrowIfCancellationRequested();
        return Result<T, TErr>.Err(await errFunc().ConfigureAwait(false));
    }

    /// <summary>
    /// Maps an Option&lt;T&gt; to Task&lt;Option&lt;U&gt;&gt; by applying an async function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="U">The type of the value in the resulting option.</typeparam>
    /// <param name="option">The option to map.</param>
    /// <param name="mapper">An async function to apply to the value if Some.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with the mapped value, or None if the original was None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Option<T> option,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> BindAsync<T, U>(
        this Option<T> option,
        Func<T, Task<Option<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original Some if the predicate passes, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original option, unchanged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapNoneAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action().ConfigureAwait(false);
        }

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with a tuple of both values if both are Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<(T, U)>> ZipAsync<T, U>(
        Task<Option<T>> firstTask,
        Task<Option<U>> secondTask,
        CancellationToken cancellationToken = default)
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
    /// Asynchronously zips two Option tasks using a combiner function.
    /// If both are Some, applies the combiner. Otherwise, returns None.
    /// </summary>
    /// <typeparam name="T">The type of the first option's value.</typeparam>
    /// <typeparam name="U">The type of the second option's value.</typeparam>
    /// <typeparam name="V">The type of the combined result.</typeparam>
    /// <param name="firstTask">The first option task.</param>
    /// <param name="secondTask">The second option task.</param>
    /// <param name="combiner">A function to combine the values if both are Some.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with the combined value if both are Some, otherwise None.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<V>> ZipWithAsync<T, U, V>(
        Task<Option<T>> firstTask,
        Task<Option<U>> secondTask,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken = default)
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
    /// Returns the first Some option from a collection of Option tasks, or None if all are None.
    /// </summary>
    /// <typeparam name="T">The type of the value in the options.</typeparam>
    /// <param name="optionTasks">The collection of option tasks to search.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the first Some option found, or None if all are None.</returns>
    public static async Task<Option<T>> FirstSomeAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks,
        CancellationToken cancellationToken = default)
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
    /// Returns the Option if Some, otherwise returns the alternative computed asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="alternativeAsync">An async function to compute the alternative if None.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original Some or the computed alternative.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrElseAsync<T>(
        this Option<T> option,
        Func<Task<Option<T>>> alternativeAsync,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(alternativeAsync);
        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original Some or the computed alternative.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<Option<T>>> alternativeAsync,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        ThrowHelper.ThrowIfNull(alternativeAsync);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        if (option.IsSome)
            return option;

        cancellationToken.ThrowIfCancellationRequested();
        return await alternativeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the Option if Some, otherwise returns the alternative.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="optionTask">The task containing the option to check.</param>
    /// <param name="alternative">The alternative to return if None.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original Some or the alternative.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> OrAsync<T>(
        this Task<Option<T>> optionTask,
        Option<T> alternative,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(optionTask);
        cancellationToken.ThrowIfCancellationRequested();

        var option = await optionTask.ConfigureAwait(false);
        return option.IsSome ? option : alternative;
    }

    #region ValueTask Overloads

    /// <summary>
    /// Maps the value inside a ValueTask&lt;Option&lt;T&gt;&gt; using a synchronous function.
    /// Optimized for scenarios where the option is frequently None or already completed.
    /// </summary>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<T>> AsValueTask<T>(this Option<T> option)
    {
        return new ValueTask<Option<T>>(option);
    }

    #endregion

    #region CancellationToken with Func Overloads

    /// <summary>
    /// Maps the value inside a Task&lt;Option&lt;T&gt;&gt; using an async function with cancellation support.
    /// </summary>
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

    #endregion
}
