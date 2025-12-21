using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Extension methods for converting Either to IActionResult.
/// </summary>
public static class EitherExtensions
{
    /// <summary>
    /// Converts an Either to an IActionResult.
    /// Right values return 200 OK, Left values return the specified error status code.
    /// </summary>
    /// <typeparam name="TLeft">The left (error) type.</typeparam>
    /// <typeparam name="TRight">The right (success) type.</typeparam>
    /// <param name="either">The either to convert.</param>
    /// <param name="leftStatusCode">The HTTP status code for Left values (default: 400 Bad Request).</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        int leftStatusCode = StatusCodes.Status400BadRequest)
    {
        return either.Match<IActionResult>(
            leftFunc: left => new ObjectResult(left) { StatusCode = leftStatusCode },
            rightFunc: right => new OkObjectResult(right)
        );
    }

    /// <summary>
    /// Converts an Either to an IActionResult with custom mapping.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="either">The either to convert.</param>
    /// <param name="onLeft">Function to create IActionResult from Left.</param>
    /// <param name="onRight">Function to create IActionResult from Right.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Func<TLeft, IActionResult> onLeft,
        Func<TRight, IActionResult> onRight)
    {
        return either.Match(onLeft, onRight);
    }

    /// <summary>
    /// Asynchronously converts a Task of Either to an IActionResult.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<TLeft, TRight>(
        this Task<Either<TLeft, TRight>> eitherTask,
        int leftStatusCode = StatusCodes.Status400BadRequest)
    {
        var either = await eitherTask.ConfigureAwait(false);
        return either.ToActionResult(leftStatusCode);
    }

    /// <summary>
    /// Asynchronously converts a Task of Either to an IActionResult with custom mapping.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<TLeft, TRight>(
        this Task<Either<TLeft, TRight>> eitherTask,
        Func<TLeft, IActionResult> onLeft,
        Func<TRight, IActionResult> onRight)
    {
        var either = await eitherTask.ConfigureAwait(false);
        return either.ToActionResult(onLeft, onRight);
    }
}
