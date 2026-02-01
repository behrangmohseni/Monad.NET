using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Helper class for throwing exceptions without inlining the throw site.
/// This keeps hot paths small and improves JIT optimization.
/// </summary>
internal static class ThrowHelper
{
    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.
    /// Cross-platform polyfill for ThrowHelper.ThrowIfNull.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNull(
#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.NotNull]
#endif
        object? argument,
#if NET6_0_OR_GREATER
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(argument))]
#endif
        string? paramName = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument is null)
            ThrowArgumentNull(paramName ?? "argument");
#endif
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperation(string message)
    {
        throw new InvalidOperationException(message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentNull(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentNull(string paramName, string message)
    {
        throw new ArgumentNullException(paramName, message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentOutOfRange(string paramName, string message)
    {
        throw new ArgumentOutOfRangeException(paramName, message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgument(string paramName, string message)
    {
        throw new ArgumentException(message, paramName);
    }

    // Monad-specific throw helpers

    /// <summary>
    /// Throws when attempting to unwrap a None value.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowOptionIsNone()
    {
        throw new InvalidOperationException(
            "Cannot unwrap Option because it is None. " +
            "Use Match(), UnwrapOr(), or check IsSome before calling Unwrap().");
    }

    /// <summary>
    /// Throws when attempting to unwrap a None value with a custom message.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowOptionIsNone(string message)
    {
        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Throws when attempting to get value from an Err Result.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowResultIsErr<TErr>(TErr error)
    {
        throw new InvalidOperationException(
            $"Cannot unwrap Result because it is Err: {error}. " +
            "Use Match(), UnwrapOr(), or check IsOk before calling Unwrap().");
    }

    /// <summary>
    /// Throws when attempting to get error from an Ok Result.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowResultIsOk<T>(T value)
    {
        throw new InvalidOperationException(
            $"Cannot get error from Result because it is Ok: {value}. " +
            "Check IsErr before calling UnwrapErr().");
    }

    /// <summary>
    /// Throws when attempting to get value from an Invalid Validation.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowValidationIsInvalid<TErr>(IReadOnlyList<TErr> errors)
    {
        var errorList = string.Join(", ", errors);
        throw new InvalidOperationException(
            $"Cannot unwrap Validation because it is Invalid with errors: [{errorList}]. " +
            "Use Match(), UnwrapOr(), or check IsValid before calling Unwrap().");
    }

    /// <summary>
    /// Throws when attempting to get errors from a Valid Validation.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowValidationIsValid<T>(T value)
    {
        throw new InvalidOperationException(
            $"Cannot get errors from Validation because it is Valid: {value}. " +
            "Check IsInvalid before calling UnwrapErrors().");
    }

    /// <summary>
    /// Throws when attempting to get value from a failed Try.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowTryIsFailure(Exception exception)
    {
        throw new InvalidOperationException(
            $"Cannot get value from Try because it is Failure: {exception.Message}. " +
            "Use Match(), GetOrElse(), Recover(), or check IsSuccess before calling Get().",
            exception);
    }

    /// <summary>
    /// Throws when attempting to get exception from a successful Try.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowTryIsSuccess<T>(T value)
    {
        throw new InvalidOperationException(
            $"Cannot get exception from Try because it is Success: {value}. " +
            "Check IsFailure before calling GetException().");
    }

    /// <summary>
    /// Throws when attempting to unwrap RemoteData that is not in Success state.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowRemoteDataNotSuccess(string state)
    {
        throw new InvalidOperationException(
            $"Cannot unwrap RemoteData because it is {state}. " +
            "Use Match() or check IsSuccess before calling Unwrap().");
    }

    /// <summary>
    /// Throws when attempting to get error from RemoteData that is not in Failure state.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowRemoteDataNotFailure(string state)
    {
        throw new InvalidOperationException(
            $"Cannot get error from RemoteData because it is {state}. " +
            "Check IsFailure before calling UnwrapError().");
    }

    /// <summary>
    /// Throws when null is passed to Option.Some().
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowCannotCreateSomeWithNull()
    {
        throw new ArgumentNullException(
            "value",
            "Cannot create Some with null value. Use None() for absent values, " +
            "or use value.ToOption() to safely convert nullable values.");
    }

    /// <summary>
    /// Throws when null is passed to Result.Ok().
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowCannotCreateOkWithNull()
    {
        throw new ArgumentNullException(
            "value",
            "Cannot create Ok with null value. Ok values must be non-null.");
    }

    /// <summary>
    /// Throws when null is passed to Result.Err().
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowCannotCreateErrWithNull()
    {
        throw new ArgumentNullException(
            "error",
            "Cannot create Err with null error. Error values must be non-null.");
    }

    /// <summary>
    /// Throws when attempting to create a NonEmptyList from an empty collection.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowCollectionIsEmpty(string paramName)
    {
        throw new ArgumentException(
            "Cannot create NonEmptyList from empty collection. " +
            "Use NonEmptyList.FromEnumerable() which returns Option<NonEmptyList<T>> for safe handling.",
            paramName);
    }

    /// <summary>
    /// Throws when attempting to access an uninitialized NonEmptyList (default value).
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNonEmptyListNotInitialized()
    {
        throw new InvalidOperationException(
            "Cannot access NonEmptyList because it is not initialized. " +
            "Use NonEmptyList<T>.Of() or NonEmptyList<T>.FromEnumerable() to create a valid instance. " +
            "The default value of NonEmptyList<T> is not usable.");
    }

    /// <summary>
    /// Throws when attempting to use a default-constructed Result.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowResultIsDefault()
    {
        throw new InvalidOperationException(
            "Cannot use Result because it was not properly initialized. " +
            "Use Result<T,E>.Ok(value) or Result<T,E>.Err(error) to create a valid instance. " +
            "The default value (e.g., default(Result<T,E>)) is not a valid Result state.");
    }
}
