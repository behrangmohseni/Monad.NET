using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Monad.NET.EntityFrameworkCore;

/// <summary>
/// Value converter that converts between Option&lt;T&gt; and nullable T for EF Core.
/// </summary>
/// <typeparam name="T">The type contained in the Option.</typeparam>
public class OptionValueConverter<T> : ValueConverter<Option<T>, T?>
    where T : class
{
    /// <summary>
    /// Creates a new OptionValueConverter for reference types.
    /// </summary>
    public OptionValueConverter()
        : base(
            option => option.Match(v => v, () => default!),
            value => value.ToOption())
    {
    }
}

/// <summary>
/// Value converter that converts between Option&lt;T&gt; and nullable T for value types.
/// </summary>
/// <typeparam name="T">The value type contained in the Option.</typeparam>
public class OptionStructValueConverter<T> : ValueConverter<Option<T>, T?>
    where T : struct
{
    /// <summary>
    /// Creates a new OptionStructValueConverter for value types.
    /// </summary>
    public OptionStructValueConverter()
        : base(
            option => option.Match(v => (T?)v, () => null),
            value => value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None())
    {
    }
}

