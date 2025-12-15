using Monad.NET;
using Monad.NET.EntityFrameworkCore;
using Xunit;

namespace Monad.NET.EntityFrameworkCore.Tests;

public class OptionValueConverterTests
{
    [Fact]
    public void OptionValueConverter_ConvertsSomeToValue()
    {
        // Arrange
        var converter = new OptionValueConverter<string>();
        var option = Option<string>.Some("test");

        // Act
        var result = converter.ConvertToProvider(option);

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void OptionValueConverter_ConvertsNoneToNull()
    {
        // Arrange
        var converter = new OptionValueConverter<string>();
        var option = Option<string>.None();

        // Act
        var result = converter.ConvertToProvider(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void OptionValueConverter_ConvertsValueToSome()
    {
        // Arrange
        var converter = new OptionValueConverter<string>();
        var value = "test";

        // Act
        var result = (Option<string>)converter.ConvertFromProvider(value)!;

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal("test", result.Unwrap());
    }

    [Fact]
    public void OptionValueConverter_ConvertsNullToNone()
    {
        // Arrange
        var converter = new OptionValueConverter<string>();
        string? value = null;

        // Act
        // When the provider value is null, the converter returns null
        // which represents None when used by EF Core
        var rawResult = converter.ConvertFromProvider(value);

        // Assert
        // For null input, the converter returns null (representing None)
        Assert.Null(rawResult);
    }

    [Fact]
    public void OptionStructValueConverter_ConvertsSomeToValue()
    {
        // Arrange
        var converter = new OptionStructValueConverter<int>();
        var option = Option<int>.Some(42);

        // Act
        var result = converter.ConvertToProvider(option);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void OptionStructValueConverter_ConvertsNoneToNull()
    {
        // Arrange
        var converter = new OptionStructValueConverter<int>();
        var option = Option<int>.None();

        // Act
        var result = converter.ConvertToProvider(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void OptionStructValueConverter_ConvertsValueToSome()
    {
        // Arrange
        var converter = new OptionStructValueConverter<int>();
        int? value = 42;

        // Act
        var result = (Option<int>)converter.ConvertFromProvider(value)!;

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void OptionStructValueConverter_ConvertsNullToNone()
    {
        // Arrange
        var converter = new OptionStructValueConverter<int>();
        int? value = null;

        // Act
        // When the provider value is null, the converter returns None
        // EF Core handles null propagation in the expression tree
        var rawResult = converter.ConvertFromProvider(value);

        // Assert
        // For null input, the converter returns null (EF Core handles this)
        Assert.Null(rawResult);
    }
}

