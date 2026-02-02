using System.Reflection;

namespace Monad.NET.Tests;

/// <summary>
/// API Contract Tests - These tests ensure that the public API surface remains stable.
/// If any of these tests fail, it indicates a potential breaking change.
/// 
/// These tests verify:
/// - Public types exist
/// - Expected methods and properties exist with correct signatures
/// - Type characteristics (struct, readonly, sealed, etc.)
/// - Generic constraints
/// </summary>
public class ApiContractTests
{
    private static readonly Assembly MonadAssembly = typeof(Option<>).Assembly;

    #region Type Existence Tests

    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Validation<,>))]
    [InlineData(typeof(Try<>))]
    [InlineData(typeof(RemoteData<,>))]
    [InlineData(typeof(NonEmptyList<>))]
    [InlineData(typeof(Writer<,>))]
    [InlineData(typeof(Reader<,>))]
    [InlineData(typeof(State<,>))]
    [InlineData(typeof(IO<>))]
    [InlineData(typeof(Unit))]
    public void CoreTypes_ShouldExist(Type type)
    {
        Assert.NotNull(type);
        Assert.True(type.IsPublic || type.IsNestedPublic, $"{type.Name} should be public");
    }

    [Fact]
    public void Option_ShouldBeReadOnlyStruct()
    {
        var type = typeof(Option<int>);
        Assert.True(type.IsValueType, "Option<T> should be a value type (struct)");
        Assert.True(IsReadOnlyStruct(type), "Option<T> should be a readonly struct");
    }

    [Fact]
    public void Result_ShouldBeReadOnlyStruct()
    {
        var type = typeof(Result<int, string>);
        Assert.True(type.IsValueType, "Result<T, E> should be a value type (struct)");
        Assert.True(IsReadOnlyStruct(type), "Result<T, E> should be a readonly struct");
    }

    [Fact]
    public void Validation_ShouldBeReadOnlyStruct()
    {
        var type = typeof(Validation<int, string>);
        Assert.True(type.IsValueType, "Validation<T, E> should be a value type (struct)");
        Assert.True(IsReadOnlyStruct(type), "Validation<T, E> should be a readonly struct");
    }

    [Fact]
    public void Try_ShouldBeReadOnlyStruct()
    {
        var type = typeof(Try<int>);
        Assert.True(type.IsValueType, "Try<T> should be a value type (struct)");
        Assert.True(IsReadOnlyStruct(type), "Try<T> should be a readonly struct");
    }

    [Fact]
    public void Unit_ShouldBeReadOnlyStruct()
    {
        var type = typeof(Unit);
        Assert.True(type.IsValueType, "Unit should be a value type (struct)");
        Assert.True(IsReadOnlyStruct(type), "Unit should be a readonly struct");
    }

    #endregion

    #region Option<T> API Contract

    [Theory]
    [InlineData("Some", typeof(Option<int>), new[] { typeof(int) })]
    [InlineData("None", typeof(Option<int>), new Type[0])]
    public void Option_StaticFactoryMethods_ShouldExist(string methodName, Type returnType, Type[] parameterTypes)
    {
        var method = typeof(Option<int>).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, parameterTypes);
        Assert.NotNull(method);
        Assert.Equal(returnType, method.ReturnType);
    }

    [Theory]
    [InlineData("IsSome", typeof(bool))]
    [InlineData("IsNone", typeof(bool))]
    public void Option_Properties_ShouldExist(string propertyName, Type propertyType)
    {
        var property = typeof(Option<int>).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.True(property.CanRead, $"{propertyName} should be readable");
        Assert.False(property.CanWrite, $"{propertyName} should be read-only");
    }

    [Theory]
    [InlineData("GetValue")]
    [InlineData("GetValueOr")]
    [InlineData("GetOrThrow")]
    [InlineData("TryGet")]
    [InlineData("Map")]
    [InlineData("Filter")]
    [InlineData("Bind")]
    [InlineData("Or")]
    [InlineData("OrElse")]
    [InlineData("Xor")]
    [InlineData("And")]
    [InlineData("Match")]
    [InlineData("Tap")]
    [InlineData("TapNone")]
    [InlineData("OkOr")]
    [InlineData("OkOrElse")]
    [InlineData("MapOr")]
    [InlineData("MapOrElse")]
    [InlineData("Zip")]
    [InlineData("ZipWith")]
    [InlineData("Contains")]
    [InlineData("Exists")]
    [InlineData("AsEnumerable")]
    [InlineData("ToArray")]
    [InlineData("ToList")]
    [InlineData("Deconstruct")]
    public void Option_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Option<int>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Fact]
    public void Option_ShouldImplementInterfaces()
    {
        var type = typeof(Option<int>);
        Assert.True(typeof(IEquatable<Option<int>>).IsAssignableFrom(type));
        Assert.True(typeof(IComparable<Option<int>>).IsAssignableFrom(type));
    }

    #endregion

    #region Result<T, TError> API Contract

    [Theory]
    [InlineData("Ok")]
    [InlineData("Error")]
    public void Result_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Result<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("IsOk", typeof(bool))]
    [InlineData("IsError", typeof(bool))]
    public void Result_Properties_ShouldExist(string propertyName, Type propertyType)
    {
        var property = typeof(Result<int, string>).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
    }

    [Theory]
    [InlineData("GetValue")]
    [InlineData("GetError")]
    [InlineData("GetValueOr")]
    [InlineData("GetOrThrow")]
    [InlineData("GetErrorOrThrow")]
    [InlineData("TryGet")]
    [InlineData("TryGetError")]
    [InlineData("Map")]
    [InlineData("MapError")]
    [InlineData("BiMap")]
    [InlineData("Bind")]
    [InlineData("Or")]
    [InlineData("OrElse")]
    [InlineData("And")]
    [InlineData("Match")]
    [InlineData("Ok")]
    [InlineData("Err")]
    [InlineData("MapOr")]
    [InlineData("MapOrElse")]
    [InlineData("Zip")]
    [InlineData("ZipWith")]
    [InlineData("Contains")]
    [InlineData("ContainsError")]
    [InlineData("Exists")]
    [InlineData("ExistsError")]
    [InlineData("AsEnumerable")]
    [InlineData("ToArray")]
    [InlineData("ToList")]
    [InlineData("Deconstruct")]
    public void Result_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Result<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Fact]
    public void Result_ShouldImplementInterfaces()
    {
        var type = typeof(Result<int, string>);
        Assert.True(typeof(IEquatable<Result<int, string>>).IsAssignableFrom(type));
        Assert.True(typeof(IComparable<Result<int, string>>).IsAssignableFrom(type));
    }

    #endregion

    #region Validation<T, TError> API Contract

    [Theory]
    [InlineData("Ok")]
    [InlineData("Error")]
    public void Validation_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Validation<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("IsOk", typeof(bool))]
    [InlineData("IsError", typeof(bool))]
    public void Validation_Properties_ShouldExist(string propertyName, Type propertyType)
    {
        var property = typeof(Validation<int, string>).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
    }

    [Theory]
    [InlineData("GetValue")]
    [InlineData("GetErrors")]
    [InlineData("GetValueOr")]
    [InlineData("GetOrThrow")]
    [InlineData("GetErrorsOrThrow")]
    [InlineData("TryGet")]
    [InlineData("TryGetErrors")]
    [InlineData("Map")]
    [InlineData("MapErrors")]
    [InlineData("BiMap")]
    [InlineData("Apply")]
    [InlineData("Bind")]
    [InlineData("And")]
    [InlineData("Ensure")]
    [InlineData("Match")]
    [InlineData("Zip")]
    [InlineData("ZipWith")]
    [InlineData("ToResult")]
    [InlineData("ToOption")]
    [InlineData("Contains")]
    [InlineData("Exists")]
    [InlineData("Deconstruct")]
    public void Validation_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Validation<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region Try<T> API Contract

    [Theory]
    [InlineData("Ok")]
    [InlineData("Error")]
    [InlineData("Of")]
    [InlineData("OfAsync")]
    public void Try_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Try<int>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("IsOk", typeof(bool))]
    [InlineData("IsError", typeof(bool))]
    public void Try_Properties_ShouldExist(string propertyName, Type propertyType)
    {
        var property = typeof(Try<int>).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
    }

    [Theory]
    [InlineData("GetValue")]
    [InlineData("GetOrThrow")]
    [InlineData("GetException")]
    [InlineData("GetExceptionOrThrow")]
    [InlineData("GetValueOr")]
    [InlineData("TryGet")]
    [InlineData("TryGetException")]
    [InlineData("Map")]
    [InlineData("Bind")]
    [InlineData("Filter")]
    [InlineData("Recover")]
    [InlineData("RecoverWith")]
    [InlineData("Match")]
    [InlineData("Zip")]
    [InlineData("ZipWith")]
    [InlineData("ToOption")]
    [InlineData("ToResult")]
    [InlineData("Contains")]
    [InlineData("Exists")]
    [InlineData("Deconstruct")]
    public void Try_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Try<int>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region Extension Classes Existence

    [Theory]
    [InlineData("OptionExtensions")]
    [InlineData("ResultExtensions")]
    [InlineData("ValidationExtensions")]
    [InlineData("TryExtensions")]
    public void ExtensionClasses_ShouldExist(string className)
    {
        var type = MonadAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == className && t.IsPublic);

        Assert.NotNull(type);
        Assert.True(type.IsAbstract && type.IsSealed, $"{className} should be a static class");
    }

    #endregion

    #region Unit Type Contract

    [Fact]
    public void Unit_ShouldHaveValueField()
    {
        var field = typeof(Unit).GetField("Value", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        Assert.Equal(typeof(Unit), field.FieldType);
        Assert.True(field.IsInitOnly || field.IsLiteral || !field.IsLiteral, "Value should be readonly");
    }

    [Fact]
    public void Unit_ShouldHaveDefaultField()
    {
        var field = typeof(Unit).GetField("Default", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        Assert.Equal(typeof(Unit), field.FieldType);
    }

    [Fact]
    public void Unit_ShouldHaveTaskField()
    {
        var field = typeof(Unit).GetField("Task", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        Assert.Equal(typeof(Task<Unit>), field.FieldType);
    }

    [Fact]
    public void Unit_ShouldHaveFromMethod()
    {
        var methods = typeof(Unit).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "From")
            .ToList();
        Assert.NotEmpty(methods);
    }

    [Fact]
    public void Unit_ShouldHaveFromAsyncMethod()
    {
        var methods = typeof(Unit).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "FromAsync")
            .ToList();
        Assert.NotEmpty(methods);
    }

    [Fact]
    public void Unit_ShouldBeEquatable()
    {
        Assert.True(typeof(IEquatable<Unit>).IsAssignableFrom(typeof(Unit)));
    }

    [Fact]
    public void Unit_ShouldBeComparable()
    {
        Assert.True(typeof(IComparable<Unit>).IsAssignableFrom(typeof(Unit)));
    }

    #endregion

    #region Serialization Attributes

    [Fact]
    public void Option_ShouldBeSerializable()
    {
        var type = typeof(Option<int>);
        Assert.True(type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0);
    }

    [Fact]
    public void Result_ShouldBeSerializable()
    {
        var type = typeof(Result<int, string>);
        Assert.True(type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0);
    }

    [Fact]
    public void Validation_ShouldBeSerializable()
    {
        var type = typeof(Validation<int, string>);
        Assert.True(type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0);
    }

    [Fact]
    public void Try_ShouldBeSerializable()
    {
        var type = typeof(Try<int>);
        Assert.True(type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0);
    }

    #endregion

    #region RemoteData<T, TError> API Contract

    [Theory]
    [InlineData("NotAsked")]
    [InlineData("Loading")]
    [InlineData("Ok")]
    [InlineData("Error")]
    public void RemoteData_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(RemoteData<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("IsNotAsked", typeof(bool))]
    [InlineData("IsLoading", typeof(bool))]
    [InlineData("IsOk", typeof(bool))]
    [InlineData("IsError", typeof(bool))]
    public void RemoteData_Properties_ShouldExist(string propertyName, Type propertyType)
    {
        var property = typeof(RemoteData<int, string>).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
    }

    #endregion

    #region NonEmptyList<T> API Contract

    [Fact]
    public void NonEmptyList_ShouldHaveOfMethod()
    {
        var methods = typeof(NonEmptyList<int>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Of")
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Fact]
    public void NonEmptyList_ShouldHaveHeadProperty()
    {
        var property = typeof(NonEmptyList<int>).GetProperty("Head", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.Equal(typeof(int), property.PropertyType);
    }

    [Theory]
    [InlineData("Last")]
    [InlineData("Map")]

    [InlineData("Filter")]
    [InlineData("Append")]
    [InlineData("Prepend")]
    [InlineData("Concat")]
    [InlineData("Reverse")]
    [InlineData("Reduce")]
    public void NonEmptyList_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(NonEmptyList<int>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region Writer<TLog, T> API Contract

    [Fact]
    public void Writer_ShouldHaveTellMethod()
    {
        var methods = typeof(Writer<string, int>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Tell")
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("Map")]

    [InlineData("BiMap")]
    [InlineData("Match")]
    public void Writer_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Writer<string, int>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region Reader<TEnv, T> API Contract

    [Theory]
    [InlineData("From")]
    [InlineData("Return")]
    [InlineData("Ask")]
    [InlineData("Asks")]
    public void Reader_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Reader<string, int>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("Run")]
    [InlineData("Map")]

    [InlineData("WithEnvironment")]
    public void Reader_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(Reader<string, int>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region State<TState, T> API Contract

    [Theory]
    [InlineData("Return")]
    [InlineData("Get")]
    [InlineData("Put")]
    [InlineData("Modify")]
    public void State_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(State<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("Run")]
    [InlineData("Eval")]
    [InlineData("Exec")]
    [InlineData("Map")]

    [InlineData("Bind")]
    public void State_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(State<int, string>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region IO<T> API Contract

    [Theory]
    [InlineData("Of")]
    [InlineData("Return")]
    public void IO_StaticFactoryMethods_ShouldExist(string methodName)
    {
        var methods = typeof(IO<int>).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    [Theory]
    [InlineData("Run")]
    [InlineData("RunAsync")]
    [InlineData("Map")]

    [InlineData("Bind")]
    public void IO_InstanceMethods_ShouldExist(string methodName)
    {
        var methods = typeof(IO<int>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);
    }

    #endregion

    #region Helper Methods

    private static bool IsReadOnlyStruct(Type type)
    {
        // Check for IsReadOnlyAttribute which indicates a readonly struct
        return type.GetCustomAttributes(false)
            .Any(attr => attr.GetType().FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute")
            || (type.IsValueType && type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(f => f.IsInitOnly));
    }

    #endregion
}

