using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Extension methods for converting Validation to IActionResult.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Converts a Validation to an IActionResult.
    /// Valid values return 200 OK, Invalid returns 422 Unprocessable Entity with validation errors.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T, TErr>(this Validation<T, TErr> validation)
    {
        return validation.Match<IActionResult>(
            validFunc: value => new OkObjectResult(value),
            invalidFunc: errors => new UnprocessableEntityObjectResult(new
            {
                Errors = errors
            })
        );
    }

    /// <summary>
    /// Converts a Validation to an IActionResult with custom mapping.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <param name="onValid">Function to create IActionResult from valid value.</param>
    /// <param name="onInvalid">Function to create IActionResult from errors.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToActionResult<T, TErr>(
        this Validation<T, TErr> validation,
        Func<T, IActionResult> onValid,
        Func<IReadOnlyList<TErr>, IActionResult> onInvalid)
    {
        return validation.Match(onValid, onInvalid);
    }

    /// <summary>
    /// Converts a Validation to an IActionResult with a ValidationProblemDetails response.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <param name="fieldName">The field name for the validation errors.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToValidationProblemResult<T>(
        this Validation<T, string> validation,
        string fieldName = "")
    {
        return validation.Match<IActionResult>(
            validFunc: value => new OkObjectResult(value),
            invalidFunc: errors =>
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                problemDetails.Errors[fieldName] = errors.ToArray();

                return new UnprocessableEntityObjectResult(problemDetails);
            }
        );
    }

    /// <summary>
    /// Converts a Validation with keyed errors to a ValidationProblemDetails response.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <returns>An IActionResult.</returns>
    public static IActionResult ToValidationProblemResult<T>(
        this Validation<T, KeyValuePair<string, string>> validation)
    {
        return validation.Match<IActionResult>(
            validFunc: value => new OkObjectResult(value),
            invalidFunc: errors =>
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                foreach (var group in errors.GroupBy(e => e.Key))
                {
                    problemDetails.Errors[group.Key] = group.Select(e => e.Value).ToArray();
                }

                return new UnprocessableEntityObjectResult(problemDetails);
            }
        );
    }

    /// <summary>
    /// Converts a Validation to an IActionResult, returning 201 Created for valid values.
    /// </summary>
    public static IActionResult ToCreatedResult<T, TErr>(
        this Validation<T, TErr> validation,
        string location)
    {
        return validation.Match<IActionResult>(
            validFunc: value => new CreatedResult(location, value),
            invalidFunc: errors => new UnprocessableEntityObjectResult(new { Errors = errors })
        );
    }

    /// <summary>
    /// Asynchronously converts a Task of Validation to an IActionResult.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T, TErr>(
        this Task<Validation<T, TErr>> validationTask)
    {
        var validation = await validationTask.ConfigureAwait(false);
        return validation.ToActionResult();
    }

    /// <summary>
    /// Asynchronously converts a Task of Validation to an IActionResult with custom mapping.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T, TErr>(
        this Task<Validation<T, TErr>> validationTask,
        Func<T, IActionResult> onValid,
        Func<IReadOnlyList<TErr>, IActionResult> onInvalid)
    {
        var validation = await validationTask.ConfigureAwait(false);
        return validation.ToActionResult(onValid, onInvalid);
    }
}
