using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Extension methods for Result&lt;T, TError&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class ResultExtensions
{
    /// <summary>
    /// Flattens a nested Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Flatten<T, TError>(this Result<Result<T, TError>, TError> result)
    {
        return result.Bind(static inner => inner);
    }

    /// <summary>
    /// Transposes a Result of an Option into an Option of a Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Result<T, TError>> Transpose<T, TError>(this Result<Option<T>, TError> result)
    {
        return result.Match(
            okFunc: static option => option.Match(
                someFunc: static value => Option<Result<T, TError>>.Some(Result<T, TError>.Ok(value)),
                noneFunc: static () => Option<Result<T, TError>>.None()
            ),
            errFunc: static err => Option<Result<T, TError>>.Some(Result<T, TError>.Error(err))
        );
    }

    /// <summary>
    /// Executes an action if the result is Ok, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Tap<T, TError>(this Result<T, TError> result, Action<T> action)
    {
        if (result.IsOk)
            action(result.GetValue());

        return result;
    }

    /// <summary>
    /// Executes an action if the result is Error, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> TapError<T, TError>(this Result<T, TError> result, Action<TError> action)
    {
        if (result.IsError)
            action(result.GetError());

        return result;
    }

    /// <summary>
    /// Returns the contained Ok value if successful, otherwise throws the specified exception.
    /// This is an alternative to Expect that allows throwing specific exception types.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The source Result.</param>
    /// <param name="exception">The exception to throw if Error.</param>
    /// <returns>The contained Ok value if successful.</returns>
    /// <exception cref="Exception">Throws the specified exception if Error.</exception>
    /// <example>
    /// <code>
    /// var ok = Result&lt;User, string&gt;.Ok(user);
    /// var value = ok.ThrowIfError(new UserNotFoundException()); // returns user
    /// 
    /// var err = Result&lt;User, string&gt;.Error("not found");
    /// err.ThrowIfError(new UserNotFoundException()); // throws UserNotFoundException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfError<T, TError>(this Result<T, TError> result, Exception exception)
    {
        ThrowHelper.ThrowIfNull(exception);

        if (result.IsError)
            throw exception;

        return result.GetValue();
    }

    /// <summary>
    /// Returns the contained Ok value if successful, otherwise throws an exception created by the factory.
    /// The factory receives the error value and is only called if the Result is Error.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The source Result.</param>
    /// <param name="exceptionFactory">The factory function to create the exception from the error.</param>
    /// <returns>The contained Ok value if successful.</returns>
    /// <exception cref="Exception">Throws the exception from the factory if Error.</exception>
    /// <example>
    /// <code>
    /// var result = GetUser(id).ThrowIfError(err => new UserNotFoundException($"User not found: {err}"));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfError<T, TError>(this Result<T, TError> result, Func<TError, Exception> exceptionFactory)
    {
        ThrowHelper.ThrowIfNull(exceptionFactory);

        if (result.IsError)
            throw exceptionFactory(result.GetError());

        return result.GetValue();
    }

    /// <summary>
    /// Wraps a function that may throw an exception into a Result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, Exception> Try<T>(Func<T> func)
    {
        try
        {
            return Result<T, Exception>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Error(ex);
        }
    }

    /// <summary>
    /// Combines two Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = Result.Combine(
    ///     GetUser(id),
    ///     GetOrder(orderId)
    /// ); // Result&lt;(User, Order), Error&gt;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2), TError> Combine<T1, T2, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second)
    {
        if (first.IsError)
            return Result<(T1, T2), TError>.Error(first.GetError());
        if (second.IsError)
            return Result<(T1, T2), TError>.Error(second.GetError());
        return Result<(T1, T2), TError>.Ok((first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines two Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var combined = Result.Combine(
    ///     GetUser(id),
    ///     GetOrder(orderId),
    ///     (user, order) => new UserOrder(user, order)
    /// );
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Combine<T1, T2, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Func<T1, T2, TResult> combiner)
    {
        if (first.IsError)
            return Result<TResult, TError>.Error(first.GetError());
        if (second.IsError)
            return Result<TResult, TError>.Error(second.GetError());
        return Result<TResult, TError>.Ok(combiner(first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines three Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3), TError> Combine<T1, T2, T3, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third)
    {
        if (first.IsError)
            return Result<(T1, T2, T3), TError>.Error(first.GetError());
        if (second.IsError)
            return Result<(T1, T2, T3), TError>.Error(second.GetError());
        if (third.IsError)
            return Result<(T1, T2, T3), TError>.Error(third.GetError());
        return Result<(T1, T2, T3), TError>.Ok((first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines three Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Combine<T1, T2, T3, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        if (first.IsError)
            return Result<TResult, TError>.Error(first.GetError());
        if (second.IsError)
            return Result<TResult, TError>.Error(second.GetError());
        if (third.IsError)
            return Result<TResult, TError>.Error(third.GetError());
        return Result<TResult, TError>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines four Results into a single Result containing a tuple.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3, T4), TError> Combine<T1, T2, T3, T4, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Result<T4, TError> fourth)
    {
        if (first.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(first.GetError());
        if (second.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(second.GetError());
        if (third.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(third.GetError());
        if (fourth.IsError)
            return Result<(T1, T2, T3, T4), TError>.Error(fourth.GetError());
        return Result<(T1, T2, T3, T4), TError>.Ok((first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines four Results using a combiner function.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Combine<T1, T2, T3, T4, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Result<T4, TError> fourth,
        Func<T1, T2, T3, T4, TResult> combiner)
    {
        if (first.IsError)
            return Result<TResult, TError>.Error(first.GetError());
        if (second.IsError)
            return Result<TResult, TError>.Error(second.GetError());
        if (third.IsError)
            return Result<TResult, TError>.Error(third.GetError());
        if (fourth.IsError)
            return Result<TResult, TError>.Error(fourth.GetError());
        return Result<TResult, TError>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines a collection of Results into a single Result containing a list.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var userIds = new[] { 1, 2, 3 };
    /// var usersResult = Result.Combine(userIds.Select(GetUser));
    /// // Result&lt;IReadOnlyList&lt;User&gt;, Error&gt;
    /// </code>
    /// </example>
    public static Result<IReadOnlyList<T>, TError> Combine<T, TError>(IEnumerable<Result<T, TError>> results)
    {
        var list = new List<T>();
        foreach (var result in results)
        {
            if (result.IsError)
                return Result<IReadOnlyList<T>, TError>.Error(result.GetError());
            list.Add(result.GetValue());
        }
        return Result<IReadOnlyList<T>, TError>.Ok(list);
    }

    /// <summary>
    /// Combines a collection of Results into a single Result, ignoring the values.
    /// Useful when you only care about success/failure, not the values.
    /// Returns the first error encountered if any Result is Err.
    /// </summary>
    /// <example>
    /// <code>
    /// var validations = new[] { ValidateA(), ValidateB(), ValidateC() };
    /// var allValid = Result.CombineAll(validations);
    /// // Result&lt;Unit, Error&gt;
    /// </code>
    /// </example>
    public static Result<Unit, TError> CombineAll<T, TError>(IEnumerable<Result<T, TError>> results)
    {
        foreach (var result in results)
        {
            if (result.IsError)
                return Result<Unit, TError>.Error(result.GetError());
        }
        return Result<Unit, TError>.Ok(Unit.Value);
    }

    #region Error Aggregation (CombineErrors)

    /// <summary>
    /// Combines two Results, accumulating ALL errors from both if either/both fail.
    /// Unlike <see cref="Combine{T1,T2,TError}"/> which returns the first error,
    /// this method collects all errors like Validation does.
    /// </summary>
    /// <example>
    /// <code>
    /// var name = ValidateName(input);  // Result&lt;string, Error&gt;
    /// var age = ValidateAge(input);    // Result&lt;int, Error&gt;
    /// var combined = ResultExtensions.CombineErrors(name, age);
    /// // Result&lt;(string, int), IReadOnlyList&lt;Error&gt;&gt; - contains ALL errors if any failed
    /// </code>
    /// </example>
    public static Result<(T1, T2), IReadOnlyList<TError>> CombineErrors<T1, T2, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second)
    {
        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());

        if (errors.Count > 0)
            return Result<(T1, T2), IReadOnlyList<TError>>.Error(errors);

        return Result<(T1, T2), IReadOnlyList<TError>>.Ok((first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines two Results with a combiner function, accumulating ALL errors from both if either/both fail.
    /// </summary>
    public static Result<TResult, IReadOnlyList<TError>> CombineErrors<T1, T2, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Func<T1, T2, TResult> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());

        if (errors.Count > 0)
            return Result<TResult, IReadOnlyList<TError>>.Error(errors);

        return Result<TResult, IReadOnlyList<TError>>.Ok(combiner(first.GetValue(), second.GetValue()));
    }

    /// <summary>
    /// Combines three Results, accumulating ALL errors from all if any fail.
    /// </summary>
    public static Result<(T1, T2, T3), IReadOnlyList<TError>> CombineErrors<T1, T2, T3, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third)
    {
        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());
        if (third.IsError)
            errors.Add(third.GetError());

        if (errors.Count > 0)
            return Result<(T1, T2, T3), IReadOnlyList<TError>>.Error(errors);

        return Result<(T1, T2, T3), IReadOnlyList<TError>>.Ok((first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines three Results with a combiner function, accumulating ALL errors from all if any fail.
    /// </summary>
    public static Result<TResult, IReadOnlyList<TError>> CombineErrors<T1, T2, T3, TError, TResult>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Func<T1, T2, T3, TResult> combiner)
    {
        ThrowHelper.ThrowIfNull(combiner);

        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());
        if (third.IsError)
            errors.Add(third.GetError());

        if (errors.Count > 0)
            return Result<TResult, IReadOnlyList<TError>>.Error(errors);

        return Result<TResult, IReadOnlyList<TError>>.Ok(combiner(first.GetValue(), second.GetValue(), third.GetValue()));
    }

    /// <summary>
    /// Combines four Results, accumulating ALL errors from all if any fail.
    /// </summary>
    public static Result<(T1, T2, T3, T4), IReadOnlyList<TError>> CombineErrors<T1, T2, T3, T4, TError>(
        Result<T1, TError> first,
        Result<T2, TError> second,
        Result<T3, TError> third,
        Result<T4, TError> fourth)
    {
        var errors = new List<TError>();

        if (first.IsError)
            errors.Add(first.GetError());
        if (second.IsError)
            errors.Add(second.GetError());
        if (third.IsError)
            errors.Add(third.GetError());
        if (fourth.IsError)
            errors.Add(fourth.GetError());

        if (errors.Count > 0)
            return Result<(T1, T2, T3, T4), IReadOnlyList<TError>>.Error(errors);

        return Result<(T1, T2, T3, T4), IReadOnlyList<TError>>.Ok((
            first.GetValue(), second.GetValue(), third.GetValue(), fourth.GetValue()));
    }

    /// <summary>
    /// Combines a collection of Results, accumulating ALL errors from all if any fail.
    /// </summary>
    /// <example>
    /// <code>
    /// var validations = items.Select(ValidateItem);
    /// var combined = ResultExtensions.CombineErrors(validations);
    /// // Result&lt;IReadOnlyList&lt;T&gt;, IReadOnlyList&lt;Error&gt;&gt;
    /// </code>
    /// </example>
    public static Result<IReadOnlyList<T>, IReadOnlyList<TError>> CombineErrors<T, TError>(
        IEnumerable<Result<T, TError>> results)
    {
        ThrowHelper.ThrowIfNull(results);

        var values = new List<T>();
        var errors = new List<TError>();

        foreach (var result in results)
        {
            if (result.IsOk)
                values.Add(result.GetValue());
            else
                errors.Add(result.GetError());
        }

        if (errors.Count > 0)
            return Result<IReadOnlyList<T>, IReadOnlyList<TError>>.Error(errors);

        return Result<IReadOnlyList<T>, IReadOnlyList<TError>>.Ok(values);
    }

    #endregion

#if NET6_0_OR_GREATER
    #region Span Operations

    /// <summary>
    /// Filters a span of results, keeping only Ok values and unwrapping them.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to filter.</param>
    /// <returns>An array containing only the Ok values.</returns>
    public static T[] CollectOkFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        var count = 0;
        foreach (var res in results)
            if (res.IsOk) count++;

        if (count == 0)
            return Array.Empty<T>();

        var result = new T[count];
        var index = 0;
        foreach (var res in results)
        {
            if (res.IsOk)
                result[index++] = res.GetValue();
        }

        return result;
    }

    /// <summary>
    /// Filters a span of results, keeping only Error values and unwrapping them.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to filter.</param>
    /// <returns>An array containing only the Error values.</returns>
    public static TError[] CollectErrorsFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        var count = 0;
        foreach (var res in results)
            if (res.IsError) count++;

        if (count == 0)
            return Array.Empty<TError>();

        var result = new TError[count];
        var index = 0;
        foreach (var res in results)
        {
            if (res.IsError)
                result[index++] = res.GetError();
        }

        return result;
    }

    /// <summary>
    /// Partitions a span of results into Ok and Error arrays.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to partition.</param>
    /// <returns>A tuple of (Ok values, Error values).</returns>
    public static (T[] Oks, TError[] Errors) PartitionFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        var okCount = 0;
        var errCount = 0;
        foreach (var res in results)
        {
            if (res.IsOk) okCount++;
            else errCount++;
        }

        var oks = okCount > 0 ? new T[okCount] : Array.Empty<T>();
        var errs = errCount > 0 ? new TError[errCount] : Array.Empty<TError>();

        var okIndex = 0;
        var errIndex = 0;
        foreach (var res in results)
        {
            if (res.IsOk)
                oks[okIndex++] = res.GetValue();
            else
                errs[errIndex++] = res.GetError();
        }

        return (oks, errs);
    }

    /// <summary>
    /// Returns the first Ok value from a span of results, or the last Error if all are errors.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to search.</param>
    /// <returns>The first Ok result, or the last Error.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the span is empty.</exception>
    public static Result<T, TError> FirstOkFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        if (results.IsEmpty)
            ThrowHelper.ThrowInvalidOperation("Span is empty.");

        Result<T, TError> lastError = default;
        var hasError = false;

        foreach (var res in results)
        {
            if (res.IsOk)
                return res;
            lastError = res;
            hasError = true;
        }

        return hasError ? lastError : throw new InvalidOperationException("Span is empty.");
    }

    /// <summary>
    /// Sequences a span of results into a result of array.
    /// Returns Error with the first error if any result is Error.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to sequence.</param>
    /// <returns>Ok containing an array of all values if all are Ok, otherwise the first Error.</returns>
    public static Result<T[], TError> SequenceFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        var result = new T[results.Length];
        for (var i = 0; i < results.Length; i++)
        {
            if (results[i].IsError)
                return Result<T[], TError>.Error(results[i].GetError());
            result[i] = results[i].GetValue();
        }

        return Result<T[], TError>.Ok(result);
    }

    /// <summary>
    /// Checks if all results in a span are Ok.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to check.</param>
    /// <returns>True if all results are Ok, false otherwise.</returns>
    public static bool AllOkFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        foreach (var res in results)
            if (res.IsError) return false;

        return true;
    }

    /// <summary>
    /// Checks if any result in a span is Ok.
    /// </summary>
    /// <typeparam name="T">The type of the Ok values.</typeparam>
    /// <typeparam name="TError">The type of the Error values.</typeparam>
    /// <param name="results">The span of results to check.</param>
    /// <returns>True if any result is Ok, false otherwise.</returns>
    public static bool AnyOkFromSpan<T, TError>(ReadOnlySpan<Result<T, TError>> results)
    {
        foreach (var res in results)
            if (res.IsOk) return true;

        return false;
    }

    #endregion
#endif
}
