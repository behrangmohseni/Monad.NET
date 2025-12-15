using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Extension methods for converting Result to IActionResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IActionResult.
    /// Ok values return 200 OK, Err values return the specified error status code.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="errorStatusCode">The HTTP status code for errors (default: 400 Bad Request).</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T, TErr>(
        this Result<T, TErr> result,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            okFunc: value => new OkObjectResult(value),
            errFunc: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts a Result to an IActionResult with custom mapping.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="onOk">Function to create IActionResult from success value.</param>
    /// <param name="onErr">Function to create IActionResult from error.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T, TErr>(
        this Result<T, TErr> result,
        Func<T, IActionResult> onOk,
        Func<TErr, IActionResult> onErr)
    {
        return result.Match(onOk, onErr);
    }

    /// <summary>
    /// Converts a Result to an IActionResult, returning 404 Not Found for errors.
    /// </summary>
    public static IActionResult ToActionResultOrNotFound<T, TErr>(this Result<T, TErr> result)
    {
        return result.Match<IActionResult>(
            okFunc: value => new OkObjectResult(value),
            errFunc: error => new NotFoundObjectResult(error)
        );
    }

    /// <summary>
    /// Converts a Result to an IActionResult, returning 201 Created for success.
    /// </summary>
    public static IActionResult ToCreatedResult<T, TErr>(
        this Result<T, TErr> result,
        string location,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            okFunc: value => new CreatedResult(location, value),
            errFunc: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts a Result to an IActionResult, returning 204 No Content for success.
    /// </summary>
    public static IActionResult ToNoContentResult<T, TErr>(
        this Result<T, TErr> result,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            okFunc: _ => new NoContentResult(),
            errFunc: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts a Result to an IActionResult with a problem details response for errors.
    /// </summary>
    public static IActionResult ToActionResultWithProblemDetails<T, TErr>(
        this Result<T, TErr> result,
        string? title = null,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            okFunc: value => new OkObjectResult(value),
            errFunc: error => new ObjectResult(new ProblemDetails
            {
                Title = title ?? "An error occurred",
                Status = errorStatusCode,
                Detail = error?.ToString()
            })
            { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Asynchronously converts a Task of Result to an IActionResult.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.ToActionResult(errorStatusCode);
    }

    /// <summary>
    /// Asynchronously converts a Task of Result to an IActionResult with custom mapping.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Func<T, IActionResult> onOk,
        Func<TErr, IActionResult> onErr)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.ToActionResult(onOk, onErr);
    }
}
