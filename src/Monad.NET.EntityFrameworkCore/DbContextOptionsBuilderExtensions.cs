using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Monad.NET.EntityFrameworkCore;

/// <summary>
/// Extension methods for configuring DbContext options with Monad.NET support.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds Monad.NET Option type support to the DbContext.
    /// This enables automatic conversion of Option&lt;T&gt; properties.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <returns>The options builder for chaining.</returns>
    /// <remarks>
    /// After calling this, you still need to call UseOptionTypes() in OnModelCreating
    /// to configure the value converters for your Option properties.
    /// </remarks>
    public static DbContextOptionsBuilder UseMonadOptions(this DbContextOptionsBuilder optionsBuilder)
    {
        // Register extension info for tracking
        var extension = optionsBuilder.Options.FindExtension<MonadOptionsExtension>()
            ?? new MonadOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}

/// <summary>
/// DbContext extension for Monad.NET integration tracking.
/// </summary>
public class MonadOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info => _info ??= new MonadOptionsExtensionInfo(this);

    /// <inheritdoc />
    public void ApplyServices(IServiceCollection services)
    {
        // No additional services needed
    }

    /// <inheritdoc />
    public void Validate(IDbContextOptions options)
    {
        // No validation needed
    }

    private sealed class MonadOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public MonadOptionsExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using Monad.NET ";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is MonadOptionsExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Monad.NET:Enabled"] = "true";
        }
    }
}

