using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monad.NET;

public static partial class OptionExtensions
{
    #region Async Operations

    /// <summary>
    /// Maps the value with an async function.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="U">The target type.</typeparam>
    /// <param name="option">The option to map.</param>
    /// <param name="mapper">An async function to apply to the value if Some.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some with the mapped value, or None.</returns>
    public static async Task<Option<U>> MapAsync<T, U>(
        this Option<T> option,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(option.GetValue()).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Maps the value with an async function that takes a cancellation token.
    /// </summary>
    public static async Task<Option<U>> MapAsync<T, U>(
        this Option<T> option,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(option.GetValue(), cancellationToken).ConfigureAwait(false);
        return Option<U>.Some(result);
    }

    /// <summary>
    /// Chains an async operation.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="U">The target type.</typeparam>
    /// <param name="option">The option to chain.</param>
    /// <param name="binder">An async function that returns a new Option based on the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the result of the binder if Some, otherwise None.</returns>
    public static async Task<Option<U>> BindAsync<T, U>(
        this Option<T> option,
        Func<T, Task<Option<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(option.GetValue()).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains an async operation that takes a cancellation token.
    /// </summary>
    public static async Task<Option<U>> BindAsync<T, U>(
        this Option<T> option,
        Func<T, CancellationToken, Task<Option<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);

        if (!option.IsSome)
            return Option<U>.None();

        cancellationToken.ThrowIfCancellationRequested();
        return await binder(option.GetValue(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters the value with an async predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">An async predicate to test the value.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing Some if the value passes the predicate, otherwise None.</returns>
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);

        if (!option.IsSome)
            return Option<T>.None();

        cancellationToken.ThrowIfCancellationRequested();
        return await predicate(option.GetValue()).ConfigureAwait(false)
            ? option
            : Option<T>.None();
    }

    /// <summary>
    /// Filters the value with an async predicate that takes a cancellation token.
    /// </summary>
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);

        if (!option.IsSome)
            return Option<T>.None();

        cancellationToken.ThrowIfCancellationRequested();
        return await predicate(option.GetValue(), cancellationToken).ConfigureAwait(false)
            ? option
            : Option<T>.None();
    }

    /// <summary>
    /// Executes an async action if the option is Some.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The option to tap.</param>
    /// <param name="action">An async action to execute if Some.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original option.</returns>
    public static async Task<Option<T>> TapAsync<T>(
        this Option<T> option,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);

        if (option.IsSome)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(option.GetValue()).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Executes an async action if the option is Some, with cancellation support.
    /// </summary>
    public static async Task<Option<T>> TapAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);

        if (option.IsSome)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(option.GetValue(), cancellationToken).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Executes an async action if the option is None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The option to tap.</param>
    /// <param name="action">An async action to execute if None.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task containing the original option.</returns>
    public static async Task<Option<T>> TapNoneAsync<T>(
        this Option<T> option,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);

        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action().ConfigureAwait(false);
        }

        return option;
    }

    // ============================================================================
    // Task<Option<T>> Extensions - for chaining async operations
    // ============================================================================

    /// <summary>
    /// Maps the value of a Task&lt;Option&lt;T&gt;&gt; with a synchronous function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.Map(mapper);
    }

    /// <summary>
    /// Maps the value of a Task&lt;Option&lt;T&gt;&gt; with an async function.
    /// </summary>
    public static async Task<Option<U>> MapAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return await option.MapAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains a synchronous binder on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Option<U>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.Bind(binder);
    }

    /// <summary>
    /// Chains an async binder on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Option<U>> BindAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<Option<U>>> binder,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(binder);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return await option.BindAsync(binder, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; with a synchronous predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.Filter(predicate);
    }

    /// <summary>
    /// Filters a Task&lt;Option&lt;T&gt;&gt; with an async predicate.
    /// </summary>
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return await option.FilterAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Taps a Task&lt;Option&lt;T&gt;&gt; with a synchronous action.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> TapAsync<T>(
        this Task<Option<T>> optionTask,
        Action<T> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.Tap(action);
    }

    /// <summary>
    /// Taps a Task&lt;Option&lt;T&gt;&gt; with an async action.
    /// </summary>
    public static async Task<Option<T>> TapAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return await option.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the value or a default from a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrAsync<T>(
        this Task<Option<T>> optionTask,
        T defaultValue,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.GetValueOr(defaultValue);
    }

    /// <summary>
    /// Gets the value or evaluates a default factory from a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> GetValueOrAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T> defaultFactory,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(defaultFactory);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.IsSome ? option.GetValue() : defaultFactory();
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, U> some,
        Func<U> none,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(some);
        ThrowHelper.ThrowIfNull(none);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.Match(some, none);
    }

    /// <summary>
    /// Pattern matches on a Task&lt;Option&lt;T&gt;&gt; with async handlers.
    /// </summary>
    public static async Task<U> MatchAsync<T, U>(
        this Task<Option<T>> optionTask,
        Func<T, Task<U>> some,
        Func<Task<U>> none,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(some);
        ThrowHelper.ThrowIfNull(none);
        cancellationToken.ThrowIfCancellationRequested();
        var option = await optionTask.ConfigureAwait(false);
        return option.IsSome
            ? await some(option.GetValue()).ConfigureAwait(false)
            : await none().ConfigureAwait(false);
    }

    #endregion

#if NET6_0_OR_GREATER
    #region Span Operations

    /// <summary>
    /// Filters a span of values, keeping only Some values and unwrapping them.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="options">The span of options to filter.</param>
    /// <returns>An array containing only the Some values.</returns>
    /// <remarks>
    /// This is an efficient alternative to Choose() for span-based data,
    /// avoiding IEnumerable overhead.
    /// </remarks>
    public static T[] ChooseFromSpan<T>(ReadOnlySpan<Option<T>> options)
    {
        var count = 0;
        foreach (var opt in options)
            if (opt.IsSome) count++;

        if (count == 0)
            return Array.Empty<T>();

        var result = new T[count];
        var index = 0;
        foreach (var opt in options)
        {
            if (opt.IsSome)
                result[index++] = opt.GetValue();
        }

        return result;
    }

    /// <summary>
    /// Returns the first Some value from a span of options, or None if all are None.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="options">The span of options to search.</param>
    /// <returns>The first Some value, or None.</returns>
    public static Option<T> FirstSomeFromSpan<T>(ReadOnlySpan<Option<T>> options)
    {
        foreach (var opt in options)
            if (opt.IsSome) return opt;

        return Option<T>.None();
    }

    /// <summary>
    /// Checks if all options in a span are Some.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="options">The span of options to check.</param>
    /// <returns>True if all options are Some, false otherwise.</returns>
    public static bool AllSomeFromSpan<T>(ReadOnlySpan<Option<T>> options)
    {
        foreach (var opt in options)
            if (opt.IsNone) return false;

        return true;
    }

    /// <summary>
    /// Checks if any option in a span is Some.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="options">The span of options to check.</param>
    /// <returns>True if any option is Some, false otherwise.</returns>
    public static bool AnySomeFromSpan<T>(ReadOnlySpan<Option<T>> options)
    {
        foreach (var opt in options)
            if (opt.IsSome) return true;

        return false;
    }

    /// <summary>
    /// Sequences a span of options into an option of array.
    /// Returns None if any option is None.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="options">The span of options to sequence.</param>
    /// <returns>Some containing an array of all values if all are Some, otherwise None.</returns>
    public static Option<T[]> SequenceFromSpan<T>(ReadOnlySpan<Option<T>> options)
    {
        var result = new T[options.Length];
        for (var i = 0; i < options.Length; i++)
        {
            if (options[i].IsNone)
                return Option<T[]>.None();
            result[i] = options[i].GetValue();
        }

        return Option<T[]>.Some(result);
    }

    #endregion
#endif
}
