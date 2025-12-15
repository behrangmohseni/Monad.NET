using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Middleware that catches exceptions and returns Result-style responses.
/// </summary>
public class ResultExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResultExceptionMiddleware> _logger;
    private readonly ResultExceptionMiddlewareOptions _options;

    /// <summary>
    /// Creates a new instance of the ResultExceptionMiddleware.
    /// </summary>
    public ResultExceptionMiddleware(
        RequestDelegate next,
        ILogger<ResultExceptionMiddleware> logger,
        ResultExceptionMiddlewareOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new ResultExceptionMiddlewareOptions();
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = _options.ExceptionMapper?.Invoke(exception)
            ?? GetDefaultResponse(exception);

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, _options.JsonSerializerOptions);
        await context.Response.WriteAsync(json);
    }

    private (int statusCode, object response) GetDefaultResponse(Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new ErrorResponse
        {
            IsOk = false,
            Error = _options.IncludeExceptionDetails
                ? new ErrorDetails
                {
                    Type = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = _options.IncludeStackTrace ? exception.StackTrace : null
                }
                : new ErrorDetails
                {
                    Type = "Error",
                    Message = "An error occurred while processing your request."
                }
        };

        return (statusCode, response);
    }
}

/// <summary>
/// Options for the ResultExceptionMiddleware.
/// </summary>
public class ResultExceptionMiddlewareOptions
{
    /// <summary>
    /// Gets or sets whether to include exception details in the response.
    /// Should be false in production.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// Gets or sets whether to include stack traces in the response.
    /// Should be false in production.
    /// </summary>
    public bool IncludeStackTrace { get; set; }

    /// <summary>
    /// Gets or sets a custom exception mapper function.
    /// Returns status code and response object.
    /// </summary>
    public Func<Exception, (int statusCode, object response)>? ExceptionMapper { get; set; }

    /// <summary>
    /// Gets or sets the JSON serializer options.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}

/// <summary>
/// Standard error response format.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Always false for error responses.
    /// </summary>
    public bool IsOk { get; set; }

    /// <summary>
    /// The error details.
    /// </summary>
    public ErrorDetails? Error { get; set; }
}

/// <summary>
/// Error details.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// The type of error.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The stack trace (if enabled).
    /// </summary>
    public string? StackTrace { get; set; }
}

