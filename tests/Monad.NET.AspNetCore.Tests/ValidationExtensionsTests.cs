using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monad.NET.AspNetCore;
using Xunit;

namespace Monad.NET.AspNetCore.Tests;

public class ValidationExtensionsTests
{
    [Fact]
    public void ToActionResult_Valid_ReturnsOkObjectResult()
    {
        var validation = Validation<int, string>.Ok(42);

        var result = validation.ToActionResult();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public void ToActionResult_Invalid_ReturnsUnprocessableEntity()
    {
        var validation = Validation<int, string>.Error(new[] { "Error 1", "Error 2" });

        var result = validation.ToActionResult();

        var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.NotNull(unprocessableResult.Value);
    }

    [Fact]
    public void ToActionResult_WithCustomMapping_Valid_UsesOnValid()
    {
        var validation = Validation<int, string>.Ok(42);

        var result = validation.ToActionResult(
            onValid: v => new CreatedResult("/test", v),
            onInvalid: e => new BadRequestObjectResult(e)
        );

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(42, createdResult.Value);
    }

    [Fact]
    public void ToActionResult_WithCustomMapping_Invalid_UsesOnInvalid()
    {
        var validation = Validation<int, string>.Error(new[] { "Error" });

        var result = validation.ToActionResult(
            onValid: v => new OkObjectResult(v),
            onInvalid: e => new BadRequestObjectResult(e)
        );

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IReadOnlyList<string>>(badRequest.Value);
        Assert.Single(errors);
    }

    [Fact]
    public void ToValidationProblemResult_Valid_ReturnsOk()
    {
        var validation = Validation<int, string>.Ok(42);

        var result = validation.ToValidationProblemResult();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public void ToValidationProblemResult_Invalid_ReturnsValidationProblemDetails()
    {
        var validation = Validation<int, string>.Error(new[] { "Name is required", "Email is invalid" });

        var result = validation.ToValidationProblemResult("form");

        var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(unprocessableResult.Value);
        Assert.Equal("Validation failed", problemDetails.Title);
        Assert.True(problemDetails.Errors.ContainsKey("form"));
        Assert.Equal(2, problemDetails.Errors["form"].Length);
    }

    [Fact]
    public void ToValidationProblemResult_KeyedErrors_GroupsByKey()
    {
        var validation = Validation<int, KeyValuePair<string, string>>.Error(new[]
        {
            KeyValuePair.Create("name", "Name is required"),
            KeyValuePair.Create("email", "Email is invalid"),
            KeyValuePair.Create("name", "Name must be at least 2 characters")
        });

        var result = validation.ToValidationProblemResult();

        var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(unprocessableResult.Value);
        Assert.Equal(2, problemDetails.Errors["name"].Length);
        Assert.Single(problemDetails.Errors["email"]);
    }

    [Fact]
    public void ToCreatedResult_Valid_ReturnsCreated()
    {
        var validation = Validation<int, string>.Ok(42);

        var result = validation.ToCreatedResult("/api/items/42");

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/items/42", createdResult.Location);
        Assert.Equal(42, createdResult.Value);
    }

    [Fact]
    public async Task ToActionResultAsync_Valid_ReturnsOk()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Ok(42));

        var result = await validationTask.ToActionResultAsync();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public async Task ToActionResultAsync_Invalid_ReturnsUnprocessableEntity()
    {
        var validationTask = Task.FromResult(Validation<int, string>.Error("Error"));

        var result = await validationTask.ToActionResultAsync();

        Assert.IsType<UnprocessableEntityObjectResult>(result);
    }
}

