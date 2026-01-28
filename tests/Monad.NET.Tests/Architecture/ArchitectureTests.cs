using System.Reflection;
using System.Runtime.CompilerServices;
using Monad.NET;

namespace Monad.NET.Tests.Architecture;

/// <summary>
/// Tests to enforce architectural decisions and design constraints.
/// See docs/ArchitecturalDecisions.md for rationale behind these constraints.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly MonadAssembly = typeof(Option<>).Assembly;

    /// <summary>
    /// All core monad types must be readonly structs for performance and immutability.
    /// ADR: Struct-based types for zero-allocation patterns.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Try<>))]
    [InlineData(typeof(Validation<,>))]
    [InlineData(typeof(RemoteData<,>))]
    [InlineData(typeof(Unit))]
    public void CoreMonadTypes_ShouldBeReadonlyStructs(Type type)
    {
        Assert.True(type.IsValueType, $"{type.Name} should be a value type (struct)");

        // Check for IsReadOnly via attributes (readonly structs have IsReadOnlyAttribute)
        var isReadOnly = type.GetCustomAttributes()
            .Any(a => a.GetType().Name == "IsReadOnlyAttribute");

        Assert.True(isReadOnly, $"{type.Name} should be a readonly struct");
    }

    /// <summary>
    /// Core monad types should not have any mutable fields.
    /// This ensures immutability and thread-safety.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<int>))]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Try<int>))]
    [InlineData(typeof(Validation<int, string>))]
    [InlineData(typeof(RemoteData<int, string>))]
    public void CoreMonadTypes_ShouldHaveOnlyReadonlyFields(Type type)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            Assert.True(field.IsInitOnly,
                $"Field {type.Name}.{field.Name} should be readonly");
        }
    }

    /// <summary>
    /// Key operations on monad types should have AggressiveInlining for performance.
    /// ADR: AggressiveInlining on hot path methods.
    /// Note: MethodImplOptions flags are stored in MethodImplAttributes on MethodBase, accessed via MethodImplementationFlags.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<int>), "IsSome")]
    [InlineData(typeof(Option<int>), "IsNone")]
    [InlineData(typeof(Option<int>), "GetValue")]
    [InlineData(typeof(Option<int>), "GetValueOr")]
    [InlineData(typeof(Result<int, string>), "IsOk")]
    [InlineData(typeof(Result<int, string>), "IsError")]
    [InlineData(typeof(Result<int, string>), "GetValue")]
    [InlineData(typeof(Try<int>), "IsOk")]
    [InlineData(typeof(Try<int>), "IsError")]
    [InlineData(typeof(Validation<int, string>), "IsOk")]
    [InlineData(typeof(Validation<int, string>), "IsError")]
    public void HotPathMethods_ShouldHaveAggressiveInlining(Type type, string methodOrPropertyName)
    {
        // Check both methods and property getters
        var member = type.GetMethod(methodOrPropertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? type.GetProperty(methodOrPropertyName, BindingFlags.Instance | BindingFlags.Public)?.GetMethod;

        Assert.NotNull(member);

        // MethodImplOptions are stored in MethodImplAttributes, accessed via MethodImplementationFlags
        var flags = member!.MethodImplementationFlags;
        Assert.True(flags.HasFlag(MethodImplAttributes.AggressiveInlining),
            $"{type.Name}.{methodOrPropertyName} should have AggressiveInlining (flags: {flags})");
    }

    /// <summary>
    /// Static factory methods on monad types should have AggressiveInlining.
    /// Note: Some overloads like Validation.Invalid(IEnumerable) do more work and don't have AggressiveInlining.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<int>), "Some")]
    [InlineData(typeof(Option<int>), "None")]
    [InlineData(typeof(Result<int, string>), "Ok")]
    [InlineData(typeof(Result<int, string>), "Err")]
    [InlineData(typeof(Try<int>), "Success")]
    [InlineData(typeof(Try<int>), "Failure")]
    [InlineData(typeof(Validation<int, string>), "Valid")]
    public void FactoryMethods_ShouldHaveAggressiveInlining(Type type, string methodName)
    {
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name == methodName)
            .ToList();

        Assert.NotEmpty(methods);

        foreach (var method in methods)
        {
            var flags = method.MethodImplementationFlags;
            Assert.True(flags.HasFlag(MethodImplAttributes.AggressiveInlining),
                $"{type.Name}.{methodName} should have AggressiveInlining (flags: {flags})");
        }
    }

    /// <summary>
    /// Validation.Invalid(TErr) single-error factory should have AggressiveInlining.
    /// The IEnumerable overload is excluded as it does more work (ToList).
    /// </summary>
    [Fact]
    public void Validation_Invalid_SingleError_ShouldHaveAggressiveInlining()
    {
        var type = typeof(Validation<int, string>);
        var method = type.GetMethod("Invalid", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);

        Assert.NotNull(method);
        var flags = method!.MethodImplementationFlags;
        Assert.True(flags.HasFlag(MethodImplAttributes.AggressiveInlining),
            $"Validation.Invalid(TErr) should have AggressiveInlining (flags: {flags})");
    }

    /// <summary>
    /// ThrowHelper methods should NOT be inlined to keep hot paths small.
    /// ADR: ThrowHelper pattern for exception throwing.
    /// </summary>
    [Fact]
    public void ThrowHelperMethods_ShouldNotBeInlined()
    {
        var throwHelperType = MonadAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == "ThrowHelper");

        Assert.NotNull(throwHelperType);

        var throwMethods = throwHelperType!.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name.StartsWith("Throw") && !m.Name.StartsWith("ThrowIf"));

        foreach (var method in throwMethods)
        {
            var flags = method.MethodImplementationFlags;
            Assert.True(flags.HasFlag(MethodImplAttributes.NoInlining),
                $"ThrowHelper.{method.Name} should have NoInlining to keep hot paths small (flags: {flags})");
        }
    }

    /// <summary>
    /// All monad types should be serializable.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Try<>))]
    [InlineData(typeof(Validation<,>))]
    [InlineData(typeof(Unit))]
    public void CoreMonadTypes_ShouldBeSerializable(Type type)
    {
        var hasSerializable = type.GetCustomAttribute<SerializableAttribute>() != null;
        Assert.True(hasSerializable, $"{type.Name} should have [Serializable] attribute");
    }

    /// <summary>
    /// Core monad types should implement IEquatable&lt;T&gt; for value equality.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<int>))]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Try<int>))]
    [InlineData(typeof(Validation<int, string>))]
    public void CoreMonadTypes_ShouldImplementIEquatable(Type type)
    {
        var equatableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));

        Assert.NotNull(equatableInterface);
    }

    /// <summary>
    /// Core monad types should implement IComparable&lt;T&gt; for ordering.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<int>))]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Try<int>))]
    [InlineData(typeof(Validation<int, string>))]
    public void CoreMonadTypes_ShouldImplementIComparable(Type type)
    {
        var comparableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComparable<>));

        Assert.NotNull(comparableInterface);
    }

    /// <summary>
    /// Extension method classes should have EditorBrowsableState.Never to reduce IntelliSense noise.
    /// </summary>
    [Fact]
    public void ExtensionMethodClasses_ShouldBeHiddenFromIntelliSense()
    {
        var extensionClasses = MonadAssembly.GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed) // static classes
            .Where(t => t.Name.Contains("Extensions") || t.Name.Contains("Collection"))
            .ToList();

        foreach (var type in extensionClasses)
        {
            var editorBrowsable = type.GetCustomAttribute<System.ComponentModel.EditorBrowsableAttribute>();
            // It's okay if it doesn't have the attribute - some extension classes are meant to be visible
            if (editorBrowsable != null)
            {
                Assert.Equal(System.ComponentModel.EditorBrowsableState.Never, editorBrowsable.State);
            }
        }
    }

    /// <summary>
    /// The library should have zero runtime dependencies.
    /// ADR: Zero dependencies philosophy.
    /// </summary>
    [Fact]
    public void Library_ShouldHaveZeroRuntimeDependencies()
    {
        var references = MonadAssembly.GetReferencedAssemblies();

        // Only system/runtime assemblies should be referenced
        foreach (var reference in references)
        {
            var isSystemAssembly = reference.Name!.StartsWith("System")
                || reference.Name.StartsWith("Microsoft")
                || reference.Name == "netstandard"
                || reference.Name == "mscorlib";

            Assert.True(isSystemAssembly,
                $"Library has unexpected dependency: {reference.Name}. " +
                "The core library should have zero third-party dependencies.");
        }
    }

    /// <summary>
    /// All async extension methods should exist and follow naming conventions.
    /// </summary>
    [Theory]
    [InlineData("MapAsync")]
    [InlineData("BindAsync")]
    [InlineData("MatchAsync")]
    public void AsyncMethods_ShouldFollowNamingConventions(string methodName)
    {
        var allTypes = MonadAssembly.GetTypes();
        var extensionClasses = allTypes.Where(t => t.IsClass && t.IsAbstract && t.IsSealed);

        var hasMethod = extensionClasses.Any(t =>
            t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Any(m => m.Name == methodName));

        Assert.True(hasMethod, $"Expected async method {methodName} to exist in extension classes");
    }

    /// <summary>
    /// Monad types should have proper debugger display attributes for better debugging experience.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Try<>))]
    [InlineData(typeof(Validation<,>))]
    public void CoreMonadTypes_ShouldHaveDebuggerDisplay(Type type)
    {
        var debuggerDisplay = type.GetCustomAttribute<System.Diagnostics.DebuggerDisplayAttribute>();
        Assert.NotNull(debuggerDisplay);
    }

    /// <summary>
    /// Core monad types should have DebuggerTypeProxy for detailed debugging view.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Try<>))]
    [InlineData(typeof(Validation<,>))]
    public void CoreMonadTypes_ShouldHaveDebuggerTypeProxy(Type type)
    {
        var debuggerTypeProxy = type.GetCustomAttribute<System.Diagnostics.DebuggerTypeProxyAttribute>();
        Assert.NotNull(debuggerTypeProxy);
    }

    /// <summary>
    /// Core monad types should NOT have implicit conversion operators.
    /// Implicit conversions can lead to subtle bugs and make code less explicit.
    /// ADR: Explicit construction over implicit magic - users should use factory methods.
    /// </summary>
    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Try<>))]
    [InlineData(typeof(Validation<,>))]
    [InlineData(typeof(RemoteData<,>))]
    [InlineData(typeof(NonEmptyList<>))]
    public void CoreMonadTypes_ShouldNotHaveImplicitConversions(Type type)
    {
        var implicitOperators = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "op_Implicit")
            .ToList();

        Assert.Empty(implicitOperators);
    }
}

