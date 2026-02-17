using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

public static partial class ValidationExtensions
{
    #region Async Operations

    /// <summary>
    /// Asynchronously combines two Validation tasks using applicative functor semantics.
    /// If both are valid, applies the combiner function. If either/both are invalid, accumulates ALL errors.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = await ValidationExtensions.ApplyAsync(
    ///     ValidateUserAsync(user),
    ///     ValidateAddressAsync(address),
    ///     (u, a) => new ValidatedUserWithAddress(u, a)
    /// );
    /// </code>
    /// </example>
    public static async Task<Validation<U, TError>> ApplyAsync<T, TIntermediate, U, TError>(
        Task<Validation<T, TError>> firstTask,
        Task<Validation<TIntermediate, TError>> secondTask,
        Func<T, TIntermediate, U> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.Apply(result2, combiner);
    }

    /// <summary>
    /// Asynchronously zips two Validation tasks into a single Validation containing a tuple.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = await ValidationExtensions.ZipAsync(
    ///     ValidateNameAsync(name),
    ///     ValidateAgeAsync(age)
    /// ); // Task&lt;Validation&lt;(string, int), Error&gt;&gt;
    /// </code>
    /// </example>
    public static async Task<Validation<(T, U), TError>> ZipAsync<T, U, TError>(
        Task<Validation<T, TError>> firstTask,
        Task<Validation<U, TError>> secondTask,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(firstTask);
        ThrowHelper.ThrowIfNull(secondTask);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.Zip(result2);
    }

    /// <summary>
    /// Asynchronously zips two Validation tasks using a combiner function.
    /// Accumulates ALL errors from both if either/both are invalid.
    /// </summary>
    /// <example>
    /// <code>
    /// var person = await ValidationExtensions.ZipWithAsync(
    ///     ValidateNameAsync(name),
    ///     ValidateAgeAsync(age),
    ///     (n, a) => new Person(n, a)
    /// );
    /// </code>
    /// </example>
    public static async Task<Validation<V, TError>> ZipWithAsync<T, U, V, TError>(
        Task<Validation<T, TError>> firstTask,
        Task<Validation<U, TError>> secondTask,
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
        return result1.ZipWith(result2, combiner);
    }

    /// <summary>
    /// Asynchronously zips three Validation tasks into a single Validation containing a tuple.
    /// Accumulates ALL errors from all if any are invalid.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3), TError>> ZipAsync<T1, T2, T3, TError>(
        Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(third);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await second.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result3 = await third.ConfigureAwait(false);

        // Accumulate all errors using ImmutableArray.Builder
        var errorBuilder = ImmutableArray.CreateBuilder<TError>();
        if (result1.IsError)
            errorBuilder.AddRange(result1.GetErrors());
        if (result2.IsError)
            errorBuilder.AddRange(result2.GetErrors());
        if (result3.IsError)
            errorBuilder.AddRange(result3.GetErrors());

        if (errorBuilder.Count > 0)
            return Validation<(T1, T2, T3), TError>.Error(errorBuilder.ToImmutable());

        return Validation<(T1, T2, T3), TError>.Ok((
            result1.GetValue(),
            result2.GetValue(),
            result3.GetValue()
        ));
    }

    /// <summary>
    /// Asynchronously combines a collection of Validation tasks into a single Validation.
    /// Accumulates ALL errors from all if any are invalid.
    /// </summary>
    /// <example>
    /// <code>
    /// var items = new[] { item1, item2, item3 };
    /// var allValidated = await items
    ///     .Select(item => ValidateItemAsync(item))
    ///     .CombineAsync();
    /// </code>
    /// </example>
    public static async Task<Validation<ImmutableArray<T>, TError>> CombineAsync<T, TError>(
        this IEnumerable<Task<Validation<T, TError>>> validationTasks,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var validations = await Task.WhenAll(validationTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var errorBuilder = ImmutableArray.CreateBuilder<TError>();
        var valueBuilder = ImmutableArray.CreateBuilder<T>();

        foreach (var validation in validations)
        {
            if (validation.IsOk)
                valueBuilder.Add(validation.GetValue());
            else
                errorBuilder.AddRange(validation.GetErrors());
        }

        return errorBuilder.Count == 0
            ? Validation<ImmutableArray<T>, TError>.Ok(valueBuilder.ToImmutable())
            : Validation<ImmutableArray<T>, TError>.Error(errorBuilder.ToImmutable());
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in a valid value.
    /// </summary>
    public static async Task<Validation<T, TError>> TapAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetValue()).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in errors.
    /// </summary>
    public static async Task<Validation<T, TError>> TapErrorsAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<ImmutableArray<TError>, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsError)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetErrors()).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously maps the valid value using an async function.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Validation<T, TError> validation,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (validation.IsError)
            return Validation<U, TError>.Error(validation.GetErrors());

        var result = await mapper(validation.GetValue()).ConfigureAwait(false);
        return Validation<U, TError>.Ok(result);
    }

    /// <summary>
    /// Asynchronously maps the valid value from a validation task using an async function.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        return await validation.MapAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region CancellationToken Overloads

    /// <summary>
    /// Asynchronously executes an action if the validation task results in a valid value, with cancellation support.
    /// </summary>
    public static async Task<Validation<T, TError>> TapAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsOk)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetValue(), cancellationToken).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously executes an action if the validation task results in errors, with cancellation support.
    /// </summary>
    public static async Task<Validation<T, TError>> TapErrorsAsync<T, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<ImmutableArray<TError>, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsError)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(validation.GetErrors(), cancellationToken).ConfigureAwait(false);
        }

        return validation;
    }

    /// <summary>
    /// Asynchronously maps the valid value using an async function with cancellation support.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Validation<T, TError> validation,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (validation.IsError)
            return Validation<U, TError>.Error(validation.GetErrors());

        var result = await mapper(validation.GetValue(), cancellationToken).ConfigureAwait(false);
        return Validation<U, TError>.Ok(result);
    }

    /// <summary>
    /// Asynchronously maps the valid value from a validation task using an async function with cancellation support.
    /// </summary>
    public static async Task<Validation<U, TError>> MapAsync<T, U, TError>(
        this Task<Validation<T, TError>> validationTask,
        Func<T, CancellationToken, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validationTask);
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        return await validation.MapAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously applies a validation with cancellation support.
    /// </summary>
    public static async Task<Validation<U, TError>> ApplyAsync<T, TIntermediate, U, TError>(
        this Task<Validation<TIntermediate, TError>> first,
        Func<TIntermediate, CancellationToken, Task<Validation<T, TError>>> second,
        Func<TIntermediate, T, U> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var firstValidation = await first.ConfigureAwait(false);

        if (firstValidation.IsError)
            return Validation<U, TError>.Error(firstValidation.GetErrors());

        cancellationToken.ThrowIfCancellationRequested();
        var secondValidation = await second(firstValidation.GetValue(), cancellationToken).ConfigureAwait(false);

        if (secondValidation.IsError)
            return Validation<U, TError>.Error(secondValidation.GetErrors());

        return Validation<U, TError>.Ok(combiner(firstValidation.GetValue(), secondValidation.GetValue()));
    }

    /// <summary>
    /// Asynchronously zips two validations with cancellation support.
    /// </summary>
    public static async Task<Validation<(T, U), TError>> ZipAsync<T, U, TError>(
        this Task<Validation<T, TError>> first,
        Func<CancellationToken, Task<Validation<U, TError>>> second,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        cancellationToken.ThrowIfCancellationRequested();

        var firstValidation = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var secondValidation = await second(cancellationToken).ConfigureAwait(false);

        return firstValidation.Zip(secondValidation);
    }

    /// <summary>
    /// Asynchronously zips two validations with a combiner function and cancellation support.
    /// </summary>
    public static async Task<Validation<V, TError>> ZipWithAsync<T, U, V, TError>(
        this Task<Validation<T, TError>> first,
        Func<CancellationToken, Task<Validation<U, TError>>> second,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(first);
        ThrowHelper.ThrowIfNull(second);
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var firstValidation = await first.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var secondValidation = await second(cancellationToken).ConfigureAwait(false);

        return firstValidation.ZipWith(secondValidation, combiner);
    }

    #endregion

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a Validation in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Validation<T, TError>> AsValueTask<T, TError>(this Validation<T, TError> validation)
        => new(validation);

    /// <summary>
    /// Maps the valid value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Validation<U, TError>> MapAsync<T, U, TError>(
        this ValueTask<Validation<T, TError>> validationTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (validationTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(validationTask.Result.Map(mapper));
        }
        return Core(validationTask, mapper, cancellationToken);

        static async ValueTask<Validation<U, TError>> Core(ValueTask<Validation<T, TError>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var v = await t.ConfigureAwait(false);
            return v.Map(m);
        }
    }

    /// <summary>
    /// Maps the valid value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Validation<U, TError>> MapAsync<T, U, TError>(
        this ValueTask<Validation<T, TError>> validationTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = await validationTask.ConfigureAwait(false);
        if (validation.IsError)
            return Validation<U, TError>.Error(validation.GetErrors());

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(validation.GetValue(), cancellationToken).ConfigureAwait(false);
        return Validation<U, TError>.Ok(result);
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, TError, U>(
        this ValueTask<Validation<T, TError>> validationTask,
        Func<T, U> validFunc,
        Func<ImmutableArray<TError>, U> invalidFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(validFunc);
        ThrowHelper.ThrowIfNull(invalidFunc);
        if (validationTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(validationTask.Result.Match(validFunc, invalidFunc));
        }
        return Core(validationTask, validFunc, invalidFunc, cancellationToken);

        static async ValueTask<U> Core(ValueTask<Validation<T, TError>> t, Func<T, U> v, Func<ImmutableArray<TError>, U> i, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var validation = await t.ConfigureAwait(false);
            return validation.Match(v, i);
        }
    }

    /// <summary>
    /// Zips two ValueTask validations into a tuple. Accumulates ALL errors.
    /// </summary>
    public static async ValueTask<Validation<(T, U), TError>> ZipAsync<T, U, TError>(
        ValueTask<Validation<T, TError>> firstTask,
        ValueTask<Validation<U, TError>> secondTask,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.Zip(result2);
    }

    /// <summary>
    /// Zips two ValueTask validations using a combiner. Accumulates ALL errors.
    /// </summary>
    public static async ValueTask<Validation<V, TError>> ZipWithAsync<T, U, V, TError>(
        ValueTask<Validation<T, TError>> firstTask,
        ValueTask<Validation<U, TError>> secondTask,
        Func<T, U, V> combiner,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(combiner);
        cancellationToken.ThrowIfCancellationRequested();

        var result1 = await firstTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result2 = await secondTask.ConfigureAwait(false);
        return result1.ZipWith(result2, combiner);
    }

    #endregion
}
