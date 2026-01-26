using Xunit;

namespace Monad.NET.Tests;

public class OfTypeTests
{
    #region Test Classes for Inheritance

    private class Animal
    {
        public string Name { get; set; } = "";
    }

    private class Dog : Animal
    {
        public string Breed { get; set; } = "";
    }

    private class Cat : Animal
    {
        public bool IsIndoor { get; set; }
    }

    #endregion

    #region OfType<TSource, TTarget> (reference types)

    [Fact]
    public void OfType_WithMatchingType_ReturnsSome()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfType<object, string>();

        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void OfType_WithNonMatchingType_ReturnsNone()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfType<object, List<int>>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfType_WithNone_ReturnsNone()
    {
        var option = Option<object>.None();
        var result = option.OfType<object, string>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfType_WithDerivedType_ReturnsSome()
    {
        var dog = new Dog { Name = "Buddy", Breed = "Golden Retriever" };
        var option = Option<Animal>.Some(dog);
        var result = option.OfType<Animal, Dog>();

        Assert.True(result.IsSome);
        Assert.Equal("Buddy", result.GetValue().Name);
        Assert.Equal("Golden Retriever", result.GetValue().Breed);
    }

    [Fact]
    public void OfType_WithWrongDerivedType_ReturnsNone()
    {
        var dog = new Dog { Name = "Buddy" };
        var option = Option<Animal>.Some(dog);
        var result = option.OfType<Animal, Cat>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfType_WithBaseType_ReturnsSome()
    {
        var dog = new Dog { Name = "Buddy" };
        var option = Option<Dog>.Some(dog);
        // Dog is an Animal, so this should work
        var animalOption = option.Map(d => (Animal)d);
        var result = animalOption.OfType<Animal, Dog>();

        Assert.True(result.IsSome);
    }

    #endregion

    #region OfTypeValue<TSource, TTarget> (value types)

    [Fact]
    public void OfTypeValue_WithMatchingType_ReturnsSome()
    {
        var option = Option<object>.Some(42);
        var result = option.OfTypeValue<object, int>();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void OfTypeValue_WithNonMatchingType_ReturnsNone()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfTypeValue<object, int>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfTypeValue_WithNone_ReturnsNone()
    {
        var option = Option<object>.None();
        var result = option.OfTypeValue<object, int>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfTypeValue_WithDouble_ReturnsSome()
    {
        var option = Option<object>.Some(3.14);
        var result = option.OfTypeValue<object, double>();

        Assert.True(result.IsSome);
        Assert.Equal(3.14, result.GetValue());
    }

    [Fact]
    public void OfTypeValue_WithWrongNumericType_ReturnsNone()
    {
        // int boxed as object is NOT a double
        var option = Option<object>.Some(42);
        var result = option.OfTypeValue<object, double>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfTypeValue_WithGuid_ReturnsSome()
    {
        var guid = Guid.NewGuid();
        var option = Option<object>.Some(guid);
        var result = option.OfTypeValue<object, Guid>();

        Assert.True(result.IsSome);
        Assert.Equal(guid, result.GetValue());
    }

    [Fact]
    public void OfTypeValue_WithDateTime_ReturnsSome()
    {
        var date = DateTime.Now;
        var option = Option<object>.Some(date);
        var result = option.OfTypeValue<object, DateTime>();

        Assert.True(result.IsSome);
        Assert.Equal(date, result.GetValue());
    }

    #endregion

    #region OfType<TTarget> (simplified for Option<object>)

    [Fact]
    public void OfType_Simplified_WithString_ReturnsSome()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfType<string>();

        Assert.True(result.IsSome);
        Assert.Equal("hello", result.GetValue());
    }

    [Fact]
    public void OfType_Simplified_WithInt_ReturnsSome()
    {
        var option = Option<object>.Some(42);
        var result = option.OfType<int>();

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void OfType_Simplified_WithNonMatchingType_ReturnsNone()
    {
        var option = Option<object>.Some("hello");
        var result = option.OfType<int>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfType_Simplified_WithNone_ReturnsNone()
    {
        var option = Option<object>.None();
        var result = option.OfType<string>();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void OfType_Simplified_WithDerivedClass_ReturnsSome()
    {
        var dog = new Dog { Name = "Buddy" };
        var option = Option<object>.Some(dog);
        var result = option.OfType<Dog>();

        Assert.True(result.IsSome);
        Assert.Equal("Buddy", result.GetValue().Name);
    }

    [Fact]
    public void OfType_Simplified_WithBaseClass_ReturnsSome()
    {
        var dog = new Dog { Name = "Buddy" };
        var option = Option<object>.Some(dog);
        var result = option.OfType<Animal>();

        Assert.True(result.IsSome);
        Assert.Equal("Buddy", result.GetValue().Name);
    }

    #endregion

    #region Chaining and Practical Use Cases

    [Fact]
    public void OfType_ChainedWithMap_Works()
    {
        var option = Option<object>.Some("hello world");
        var result = option
            .OfType<object, string>()
            .Map(s => s.ToUpper());

        Assert.True(result.IsSome);
        Assert.Equal("HELLO WORLD", result.GetValue());
    }

    [Fact]
    public void OfType_ChainedWithFilter_Works()
    {
        var dog = new Dog { Name = "Buddy", Breed = "Golden Retriever" };
        var option = Option<Animal>.Some(dog);
        var result = option
            .OfType<Animal, Dog>()
            .Filter(d => d.Breed.Contains("Golden"));

        Assert.True(result.IsSome);
    }

    [Fact]
    public void OfType_UsedInMatch_Works()
    {
        var option = Option<object>.Some("test");
        var message = option
            .OfType<object, string>()
            .Match(
                someFunc: s => $"Got string: {s}",
                noneFunc: () => "Not a string"
            );

        Assert.Equal("Got string: test", message);
    }

    [Fact]
    public void OfType_WithNull_InsideBoxedObject_ReturnsNone()
    {
        // Edge case: What if the boxed object is null?
        // Option.Some doesn't allow null, so this shouldn't happen
        // But OfType should handle it gracefully anyway
        var option = Option<object>.None();
        var result = option.OfType<object, string>();

        Assert.True(result.IsNone);
    }

    #endregion
}

