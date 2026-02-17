using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Extension methods for Validation&lt;T, E&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class ValidationExtensions
{
    /// <summary>
    /// Combines multiple validations into one, accumulating all errors.
    /// Returns Valid only if ALL validations are valid.
    /// </summary>
    public static Validation<T, TError> Combine<T, TError>(
        this IEnumerable<Validation<T, TError>> validations)
    {
        ThrowHelper.ThrowIfNull(validations);

        var validationList = validations.ToList();
        if (validationList.Count == 0)
            ThrowHelper.ThrowArgument(nameof(validations), "Must provide at least one validation.");

        var errorBuilder = ImmutableArray.CreateBuilder<TError>();
        T? lastValue = default;

        foreach (var validation in validationList)
        {
            if (validation.IsOk)
                lastValue = validation.GetValue();
            else
                errorBuilder.AddRange(validation.GetErrors());
        }

        return errorBuilder.Count == 0
            ? Validation<T, TError>.Ok(lastValue!)
            : Validation<T, TError>.Error(errorBuilder.ToImmutable());
    }

    /// <summary>
    /// Executes an action if the validation is valid, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Tap<T, TError>(
        this Validation<T, TError> validation,
        Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (validation.IsOk)
            action(validation.GetValue());

        return validation;
    }

    /// <summary>
    /// Executes an action if the validation is invalid, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> TapErrors<T, TError>(
        this Validation<T, TError> validation,
        Action<ImmutableArray<TError>> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (validation.IsError)
            action(validation.GetErrors());

        return validation;
    }

    /// <summary>
    /// Executes an action if the validation is invalid, allowing method chaining.
    /// This is equivalent to <see cref="TapErrors{T, TError}(Validation{T, TError}, Action{ImmutableArray{TError}})"/>
    /// and provides a consistent naming convention with Result&lt;T, TError&gt;.TapError().
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> TapError<T, TError>(
        this Validation<T, TError> validation,
        Action<ImmutableArray<TError>> action)
    {
        return TapErrors(validation, action);
    }

    /// <summary>
    /// Converts a Result to a Validation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> ToValidation<T, TError>(this Result<T, TError> result)
    {
        return result.Match(
            okFunc: static value => Validation<T, TError>.Ok(value),
            errFunc: static err => Validation<T, TError>.Error(err)
        );
    }

    /// <summary>
    /// Flattens a nested Validation into a single Validation.
    /// If the outer validation is invalid, returns those errors.
    /// If the outer is valid and inner is invalid, returns the inner's errors.
    /// If both are valid, returns the inner's value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="nested">The nested validation to flatten.</param>
    /// <returns>The flattened validation.</returns>
    /// <example>
    /// <code>
    /// var nested = Validation&lt;Validation&lt;int, string&gt;, string&gt;.Ok(
    ///     Validation&lt;int, string&gt;.Ok(42));
    /// var flattened = nested.Flatten(); // Valid(42)
    /// 
    /// var nestedInvalid = Validation&lt;Validation&lt;int, string&gt;, string&gt;.Ok(
    ///     Validation&lt;int, string&gt;.Error("inner error"));
    /// var flattenedInvalid = nestedInvalid.Flatten(); // Invalid(["inner error"])
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<T, TError> Flatten<T, TError>(this Validation<Validation<T, TError>, TError> nested)
    {
        return nested.Bind(static inner => inner);
    }
}
