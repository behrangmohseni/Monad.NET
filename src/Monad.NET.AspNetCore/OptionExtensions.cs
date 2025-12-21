using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Extension methods for converting Option to IActionResult.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Converts an Option to an IActionResult.
    /// Some values return 200 OK, None returns 404 Not Found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T>(this Option<T> option)
    {
        return option.Match<IActionResult>(
            someFunc: value => new OkObjectResult(value),
            noneFunc: () => new NotFoundResult()
        );
    }

    /// <summary>
    /// Converts an Option to an IActionResult with a custom not found message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <param name="notFoundMessage">The message to return when None.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T>(this Option<T> option, string notFoundMessage)
    {
        return option.Match<IActionResult>(
            someFunc: value => new OkObjectResult(value),
            noneFunc: () => new NotFoundObjectResult(new { Message = notFoundMessage })
        );
    }

    /// <summary>
    /// Converts an Option to an IActionResult with a custom not found object.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TNotFound">The not found response type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <param name="notFoundValue">The object to return when None.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T, TNotFound>(this Option<T> option, TNotFound notFoundValue)
    {
        return option.Match<IActionResult>(
            someFunc: value => new OkObjectResult(value),
            noneFunc: () => new NotFoundObjectResult(notFoundValue)
        );
    }

    /// <summary>
    /// Converts an Option to an IActionResult with custom mapping.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <param name="onSome">Function to create IActionResult from value.</param>
    /// <param name="onNone">Function to create IActionResult for None.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T>(
        this Option<T> option,
        Func<T, IActionResult> onSome,
        Func<IActionResult> onNone)
    {
        return option.Match(onSome, onNone);
    }

    /// <summary>
    /// Converts an Option to an IActionResult with Problem Details for None.
    /// </summary>
    public static IActionResult ToActionResultWithProblemDetails<T>(
        this Option<T> option,
        string? title = null,
        string? detail = null)
    {
        return option.Match<IActionResult>(
            someFunc: value => new OkObjectResult(value),
            noneFunc: () => new ObjectResult(new ProblemDetails
            {
                Title = title ?? "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = detail ?? "The requested resource was not found."
            })
            { StatusCode = StatusCodes.Status404NotFound }
        );
    }

    /// <summary>
    /// Asynchronously converts a Task of Option to an IActionResult.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(this Task<Option<T>> optionTask)
    {
        var option = await optionTask.ConfigureAwait(false);
        return option.ToActionResult();
    }

    /// <summary>
    /// Asynchronously converts a Task of Option to an IActionResult with a custom not found message.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Option<T>> optionTask,
        string notFoundMessage)
    {
        var option = await optionTask.ConfigureAwait(false);
        return option.ToActionResult(notFoundMessage);
    }

    /// <summary>
    /// Asynchronously converts a Task of Option to an IActionResult with custom mapping.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, IActionResult> onSome,
        Func<IActionResult> onNone)
    {
        var option = await optionTask.ConfigureAwait(false);
        return option.ToActionResult(onSome, onNone);
    }
}

