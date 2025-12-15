using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Monad.NET.AspNetCore;

/// <summary>
/// Extension methods for configuring Monad.NET with ASP.NET Core.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Monad.NET services to the service collection.
    /// Currently a no-op but reserved for future service registrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMonadNet(this IServiceCollection services)
    {
        // Reserved for future service registrations (e.g., custom serializers)
        return services;
    }

    /// <summary>
    /// Adds the Result exception handling middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ResultExceptionMiddleware>();
    }

    /// <summary>
    /// Adds the Result exception handling middleware to the pipeline with options.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="options">The middleware options.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultExceptionHandler(
        this IApplicationBuilder app,
        ResultExceptionMiddlewareOptions options)
    {
        return app.UseMiddleware<ResultExceptionMiddleware>(options);
    }

    /// <summary>
    /// Adds the Result exception handling middleware to the pipeline with configuration.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configure">Action to configure the middleware options.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultExceptionHandler(
        this IApplicationBuilder app,
        Action<ResultExceptionMiddlewareOptions> configure)
    {
        var options = new ResultExceptionMiddlewareOptions();
        configure(options);
        return app.UseMiddleware<ResultExceptionMiddleware>(options);
    }
}

