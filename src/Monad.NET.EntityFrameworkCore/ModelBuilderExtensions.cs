using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Monad.NET.EntityFrameworkCore;

/// <summary>
/// Extension methods for configuring Option&lt;T&gt; properties in EF Core models.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures all Option&lt;T&gt; properties in the model to use the appropriate value converters.
    /// Call this in OnModelCreating after all entity configurations.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The model builder for chaining.</returns>
    public static ModelBuilder UseOptionTypes(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            foreach (var property in clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsOptionType(property.PropertyType, out var innerType))
                    continue;

                var propertyBuilder = modelBuilder.Entity(clrType).Property(property.Name);

                var converter = CreateOptionConverter(innerType!);
                if (converter is not null)
                {
                    propertyBuilder.HasConversion(converter);
                }
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures a specific Option&lt;T&gt; property to use the appropriate value converter.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="T">The type contained in the Option (must be a reference type).</typeparam>
    /// <param name="propertyBuilder">The property builder.</param>
    /// <returns>The property builder for chaining.</returns>
    public static PropertyBuilder<Option<T>> HasOptionConversion<TEntity, T>(
        this PropertyBuilder<Option<T>> propertyBuilder)
        where TEntity : class
        where T : class
    {
        return propertyBuilder.HasConversion(new OptionValueConverter<T>());
    }

    /// <summary>
    /// Configures a specific Option&lt;T&gt; property for value types to use the appropriate value converter.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="T">The value type contained in the Option.</typeparam>
    /// <param name="propertyBuilder">The property builder.</param>
    /// <returns>The property builder for chaining.</returns>
    public static PropertyBuilder<Option<T>> HasOptionStructConversion<TEntity, T>(
        this PropertyBuilder<Option<T>> propertyBuilder)
        where TEntity : class
        where T : struct
    {
        return propertyBuilder.HasConversion(new OptionStructValueConverter<T>());
    }

    private static bool IsOptionType(Type type, out Type? innerType)
    {
        innerType = null;

        if (!type.IsGenericType)
            return false;

        var genericDef = type.GetGenericTypeDefinition();
        if (genericDef != typeof(Option<>))
            return false;

        innerType = type.GetGenericArguments()[0];
        return true;
    }

    private static ValueConverter? CreateOptionConverter(Type innerType)
    {
        Type converterType;

        if (innerType.IsValueType)
        {
            converterType = typeof(OptionStructValueConverter<>).MakeGenericType(innerType);
        }
        else
        {
            converterType = typeof(OptionValueConverter<>).MakeGenericType(innerType);
        }

        return (ValueConverter?)Activator.CreateInstance(converterType);
    }
}

