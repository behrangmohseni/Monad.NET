# Monad.NET.AspNetCore

ASP.NET Core integration for [Monad.NET](https://www.nuget.org/packages/Monad.NET/).

## Installation

```bash
dotnet add package Monad.NET.AspNetCore
```

## Features

- **IActionResult Extensions** - Convert monad types directly to HTTP responses
- **Exception Middleware** - Catch exceptions and return Result-style responses
- **ValidationProblemDetails Support** - Automatic conversion to RFC 7807 format

## Quick Start

### IActionResult Extensions

Convert monad types directly to HTTP responses in your controllers:

```csharp
using Monad.NET;
using Monad.NET.AspNetCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    // Option → 200 OK or 404 Not Found
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        return _userService.FindUser(id)
            .ToActionResult("User not found");
    }

    // Result → 200 OK or error status
    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        return _userService.CreateUser(request)
            .ToActionResult(StatusCodes.Status400BadRequest);
    }

    // Validation → 200 OK or 422 with validation errors
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UpdateUserRequest request)
    {
        return ValidateRequest(request)
            .ToValidationProblemResult();
    }

    // Async support
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetUserProfile(int id)
    {
        return await _userService.GetUserProfileAsync(id)
            .ToActionResultAsync();
    }
}
```

### Custom Response Mapping

```csharp
// Custom mapping for Result
return result.ToActionResult(
    onOk: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
    onErr: error => error switch
    {
        UserError.NotFound => NotFound(),
        UserError.Duplicate => Conflict(error),
        _ => BadRequest(error)
    }
);

// Custom mapping for Option
return option.ToActionResult(
    onSome: user => Ok(new { User = user, Timestamp = DateTime.UtcNow }),
    onNone: () => NotFound(new { Message = "User not found" })
);
```

### Exception Handling Middleware

Catch unhandled exceptions and return consistent Result-style responses:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Add early in the pipeline
app.UseResultExceptionHandler(options =>
{
    options.IncludeExceptionDetails = app.Environment.IsDevelopment();
    options.IncludeStackTrace = app.Environment.IsDevelopment();
});

app.MapControllers();
app.Run();
```

Response format:
```json
{
  "isOk": false,
  "error": {
    "type": "InvalidOperationException",
    "message": "Something went wrong"
  }
}
```

### ValidationProblemDetails Support

Convert validation errors to RFC 7807 format:

```csharp
// Simple string errors
var validation = Validation<User, string>.Invalid(
    new[] { "Name is required", "Email is invalid" }
);
return validation.ToValidationProblemResult("user");

// Keyed errors (field-specific)
var validation = Validation<User, KeyValuePair<string, string>>.Invalid(
    new[]
    {
        KeyValuePair.Create("name", "Name is required"),
        KeyValuePair.Create("email", "Email is invalid")
    }
);
return validation.ToValidationProblemResult();
```

Response:
```json
{
  "title": "Validation failed",
  "status": 422,
  "errors": {
    "name": ["Name is required"],
    "email": ["Email is invalid"]
  }
}
```

## Available Extensions

### Option Extensions

| Method | Description |
|--------|-------------|
| `ToActionResult()` | Some → 200 OK, None → 404 |
| `ToActionResult(message)` | None → 404 with message |
| `ToActionResult(onSome, onNone)` | Custom mapping |
| `ToActionResultWithProblemDetails()` | None → ProblemDetails |
| `ToActionResultAsync()` | Async version |

### Result Extensions

| Method | Description |
|--------|-------------|
| `ToActionResult(errorCode)` | Ok → 200, Err → specified code |
| `ToActionResult(onOk, onErr)` | Custom mapping |
| `ToActionResultOrNotFound()` | Err → 404 |
| `ToCreatedResult(location)` | Ok → 201 Created |
| `ToNoContentResult()` | Ok → 204 No Content |
| `ToActionResultWithProblemDetails()` | Err → ProblemDetails |
| `ToActionResultAsync()` | Async versions |

### Validation Extensions

| Method | Description |
|--------|-------------|
| `ToActionResult()` | Valid → 200, Invalid → 422 |
| `ToActionResult(onValid, onInvalid)` | Custom mapping |
| `ToValidationProblemResult()` | Invalid → ValidationProblemDetails |
| `ToCreatedResult(location)` | Valid → 201 |
| `ToActionResultAsync()` | Async versions |

### Try Extensions

| Method | Description |
|--------|-------------|
| `ToActionResult(includeDetails)` | Success → 200, Failure → 500 |
| `ToActionResult(onSuccess, onFailure)` | Custom mapping |
| `ToActionResultWithProblemDetails()` | Failure → ProblemDetails |
| `ToActionResultAsync()` | Async versions |

## Requirements

- .NET 6.0 or later
- [Monad.NET](https://www.nuget.org/packages/Monad.NET/) (automatically referenced)

## License

MIT — Free for commercial and personal use.

**Author:** [Behrang Mohseni](https://www.linkedin.com/in/behrangmohseni/)

