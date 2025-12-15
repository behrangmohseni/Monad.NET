using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monad.NET.AspNetCore;
using Xunit;

namespace Monad.NET.AspNetCore.Tests;

public class OptionExtensionsTests
{
    [Fact]
    public void ToActionResult_Some_ReturnsOkObjectResult()
    {
        var option = Option<int>.Some(42);

        var result = option.ToActionResult();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public void ToActionResult_None_ReturnsNotFoundResult()
    {
        var option = Option<int>.None();

        var result = option.ToActionResult();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ToActionResult_None_WithMessage_ReturnsNotFoundObjectResult()
    {
        var option = Option<int>.None();

        var result = option.ToActionResult("Item not found");

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public void ToActionResult_WithCustomMapping_Some_UsesOnSome()
    {
        var option = Option<int>.Some(42);

        var result = option.ToActionResult(
            onSome: v => new CreatedResult("/test", v),
            onNone: () => new BadRequestResult()
        );

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(42, createdResult.Value);
    }

    [Fact]
    public void ToActionResult_WithCustomMapping_None_UsesOnNone()
    {
        var option = Option<int>.None();

        var result = option.ToActionResult(
            onSome: v => new OkObjectResult(v),
            onNone: () => new BadRequestResult()
        );

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void ToActionResultWithProblemDetails_None_ReturnsProblemDetails()
    {
        var option = Option<int>.None();

        var result = option.ToActionResultWithProblemDetails("Not Found", "The item was not found");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public async Task ToActionResultAsync_Some_ReturnsOkObjectResult()
    {
        var optionTask = Task.FromResult(Option<int>.Some(42));

        var result = await optionTask.ToActionResultAsync();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public async Task ToActionResultAsync_None_ReturnsNotFoundResult()
    {
        var optionTask = Task.FromResult(Option<int>.None());

        var result = await optionTask.ToActionResultAsync();

        Assert.IsType<NotFoundResult>(result);
    }
}

