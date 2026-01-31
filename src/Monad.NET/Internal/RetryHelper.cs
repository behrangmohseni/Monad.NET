using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Internal helper class for retry operations.
/// Used by IO and IOAsync for retry logic.
/// </summary>
internal static class RetryHelper
{
    /// <summary>
    /// Validates the retries parameter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ValidateRetries(int retries, string paramName)
    {
        if (retries < 0)
            ThrowHelper.ThrowArgumentOutOfRange(paramName, "Retries must be non-negative.");
    }

    /// <summary>
    /// Executes a synchronous effect with retry logic.
    /// </summary>
    internal static T ExecuteWithRetry<T>(Func<T> effect, int retries)
    {
        Exception? lastException = null;
        for (var i = 0; i <= retries; i++)
        {
            try
            {
                return effect();
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }
        throw lastException!;
    }

    /// <summary>
    /// Executes an async effect with retry logic.
    /// </summary>
    internal static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> effect, int retries)
    {
        Exception? lastException = null;
        for (var i = 0; i <= retries; i++)
        {
            try
            {
                return await effect().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }
        throw lastException!;
    }

    /// <summary>
    /// Executes an async effect with retry logic and a fixed delay between attempts.
    /// </summary>
    internal static async Task<T> ExecuteWithRetryAndDelayAsync<T>(Func<Task<T>> effect, int retries, TimeSpan delay)
    {
        Exception? lastException = null;
        for (var i = 0; i <= retries; i++)
        {
            try
            {
                return await effect().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (i < retries)
                {
                    await Task.Delay(delay).ConfigureAwait(false);
                }
            }
        }
        throw lastException!;
    }

    /// <summary>
    /// Executes an async effect with retry logic and exponential backoff.
    /// </summary>
    internal static async Task<T> ExecuteWithExponentialBackoffAsync<T>(
        Func<Task<T>> effect, int retries, TimeSpan initialDelay, TimeSpan? maxDelay)
    {
        Exception? lastException = null;
        var currentDelay = initialDelay;
        for (var i = 0; i <= retries; i++)
        {
            try
            {
                return await effect().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (i < retries)
                {
                    await Task.Delay(currentDelay).ConfigureAwait(false);
                    currentDelay = TimeSpan.FromTicks(currentDelay.Ticks * 2);
                    if (maxDelay.HasValue && currentDelay > maxDelay.Value)
                    {
                        currentDelay = maxDelay.Value;
                    }
                }
            }
        }
        throw lastException!;
    }
}
