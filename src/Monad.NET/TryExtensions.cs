using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Extension methods for Try&lt;T&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class TryExtensions
{
    /// <summary>
    /// Executes an action on success, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Tap<T>(this Try<T> @try, Action<T> action)
    {
        if (@try.IsOk)
        {
            try
            {
                action(@try.GetValue());
            }
            catch (Exception ex)
            {
                return Try<T>.Error(ex);
            }
        }

        return @try;
    }

    /// <summary>
    /// Executes an action on error, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> TapError<T>(this Try<T> @try, Action<Exception> action)
    {
        if (@try.IsError)
            action(@try.GetException());

        return @try;
    }

    /// <summary>
    /// Flattens a nested Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> Flatten<T>(this Try<Try<T>> @try)
    {
        return @try.Bind(static inner => inner);
    }

    /// <summary>
    /// Converts a Result to a Try.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Try<T> ToTry<T>(this Result<T, Exception> result)
    {
        return result.Match(
            okFunc: static value => Try<T>.Ok(value),
            errFunc: static ex => Try<T>.Error(ex)
        );
    }
}
