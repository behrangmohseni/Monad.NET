using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monad.NET.AspNetCore;
using Xunit;

namespace Monad.NET.AspNetCore.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void ToActionResult_Ok_ReturnsOkObjectResult()
    {
        var result = Result<int, string>.Ok(42);

        var actionResult = result.ToActionResult();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public void ToActionResult_Err_ReturnsObjectResultWithErrorCode()
    {
        var result = Result<int, string>.Error("Error occurred");

        var actionResult = result.ToActionResult(StatusCodes.Status400BadRequest);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("Error occurred", objectResult.Value);
    }

    [Fact]
    public void ToActionResult_Err_DefaultsTo400()
    {
        var result = Result<int, string>.Error("Error");

        var actionResult = result.ToActionResult();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_WithCustomMapping_Ok_UsesOnOk()
    {
        var result = Result<int, string>.Ok(42);

        var actionResult = result.ToActionResult(
            onOk: v => new CreatedResult("/test", v),
            onErr: e => new BadRequestObjectResult(e)
        );

        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(42, createdResult.Value);
    }

    [Fact]
    public void ToActionResult_WithCustomMapping_Err_UsesOnErr()
    {
        var result = Result<int, string>.Error("Error");

        var actionResult = result.ToActionResult(
            onOk: v => new OkObjectResult(v),
            onErr: e => new NotFoundObjectResult(e)
        );

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        Assert.Equal("Error", notFoundResult.Value);
    }

    [Fact]
    public void ToActionResultOrNotFound_Err_ReturnsNotFound()
    {
        var result = Result<int, string>.Error("Not found");

        var actionResult = result.ToActionResultOrNotFound();

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        Assert.Equal("Not found", notFoundResult.Value);
    }

    [Fact]
    public void ToCreatedResult_Ok_ReturnsCreatedResult()
    {
        var result = Result<int, string>.Ok(42);

        var actionResult = result.ToCreatedResult("/api/items/42");

        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("/api/items/42", createdResult.Location);
        Assert.Equal(42, createdResult.Value);
    }

    [Fact]
    public void ToNoContentResult_Ok_ReturnsNoContent()
    {
        var result = Result<int, string>.Ok(42);

        var actionResult = result.ToNoContentResult();

        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_Err_ReturnsProblemDetails()
    {
        var result = Result<int, string>.Error("Something went wrong");

        var actionResult = result.ToActionResultWithProblemDetails("Error", StatusCodes.Status500InternalServerError);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Error", problemDetails.Title);
        Assert.Equal("Something went wrong", problemDetails.Detail);
    }

    [Fact]
    public async Task ToActionResultAsync_Ok_ReturnsOkObjectResult()
    {
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        var actionResult = await resultTask.ToActionResultAsync();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public async Task ToActionResultAsync_Err_ReturnsObjectResult()
    {
        var resultTask = Task.FromResult(Result<int, string>.Error("Error"));

        var actionResult = await resultTask.ToActionResultAsync(StatusCodes.Status422UnprocessableEntity);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);
    }
}

