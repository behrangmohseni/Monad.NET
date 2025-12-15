using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Extension methods for converting Try to IActionResult.
/// </summary>
public static class TryExtensions
{
    /// <summary>
    /// Converts a Try to an IActionResult.
    /// Success values return 200 OK, Failure returns 500 Internal Server Error.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="tryResult">The try result to convert.</param>
    /// <param name="includeExceptionDetails">Whether to include exception details in the response.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T>(
        this Try<T> tryResult,
        bool includeExceptionDetails = false)
    {
        return tryResult.Match<IActionResult>(
            successFunc: value => new OkObjectResult(value),
            failureFunc: ex => new ObjectResult(new
            {
                Error = includeExceptionDetails ? ex.Message : "An error occurred",
                Type = includeExceptionDetails ? ex.GetType().Name : null
            })
            { StatusCode = StatusCodes.Status500InternalServerError }
        );
    }

    /// <summary>
    /// Converts a Try to an IActionResult with custom mapping.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="tryResult">The try result to convert.</param>
    /// <param name="onSuccess">Function to create IActionResult from success value.</param>
    /// <param name="onFailure">Function to create IActionResult from exception.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T>(
        this Try<T> tryResult,
        Func<T, IActionResult> onSuccess,
        Func<Exception, IActionResult> onFailure)
    {
        return tryResult.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Converts a Try to an IActionResult with ProblemDetails for failures.
    /// </summary>
    public static IActionResult ToActionResultWithProblemDetails<T>(
        this Try<T> tryResult,
        bool includeExceptionDetails = false)
    {
        return tryResult.Match<IActionResult>(
            successFunc: value => new OkObjectResult(value),
            failureFunc: ex => new ObjectResult(new ProblemDetails
            {
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = includeExceptionDetails ? ex.Message : "An internal error occurred while processing your request."
            })
            { StatusCode = StatusCodes.Status500InternalServerError }
        );
    }

    /// <summary>
    /// Asynchronously converts a Task of Try to an IActionResult.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Try<T>> tryTask,
        bool includeExceptionDetails = false)
    {
        var tryResult = await tryTask.ConfigureAwait(false);
        return tryResult.ToActionResult(includeExceptionDetails);
    }

    /// <summary>
    /// Asynchronously converts a Task of Try to an IActionResult with custom mapping.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Try<T>> tryTask,
        Func<T, IActionResult> onSuccess,
        Func<Exception, IActionResult> onFailure)
    {
        var tryResult = await tryTask.ConfigureAwait(false);
        return tryResult.ToActionResult(onSuccess, onFailure);
    }
}
