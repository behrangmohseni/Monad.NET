using Microsoft.CodeAnalysis;

namespace Monad.NET.SourceGenerators;

/// <summary>
/// Diagnostic descriptors for the Union source generator.
/// </summary>
internal static class UnionDiagnostics
{
    private const string Category = "Monad.NET.SourceGenerators";

    /// <summary>
    /// MNG001: Type marked with [Union] must be abstract.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBeAbstract = new(
        id: "MNG001",
        title: "Union type must be abstract",
        messageFormat: "Type '{0}' is marked with [Union] but is not abstract. Union types must be abstract to prevent direct instantiation.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Types marked with [Union] must be declared as abstract. This ensures that only the nested case types can be instantiated, providing exhaustive pattern matching.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG001");

    /// <summary>
    /// MNG002: Type marked with [Union] must be partial.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBePartial = new(
        id: "MNG002",
        title: "Union type must be partial",
        messageFormat: "Type '{0}' is marked with [Union] but is not partial. Add the 'partial' keyword to enable source generation.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Types marked with [Union] must be declared as partial so that the source generator can add Match, Is{Case}, and As{Case} methods.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG002");

    /// <summary>
    /// MNG003: Union type has no cases.
    /// </summary>
    public static readonly DiagnosticDescriptor NoCasesFound = new(
        id: "MNG003",
        title: "Union type has no cases",
        messageFormat: "Type '{0}' is marked with [Union] but has no nested types that inherit from it. Add at least one case type (e.g., public sealed record Case1 : {0}).",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Union types should have at least one nested case type that inherits from the union. Without cases, the union serves no purpose.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG003");

    /// <summary>
    /// MNG004: Case type should be sealed.
    /// </summary>
    public static readonly DiagnosticDescriptor CaseShouldBeSealed = new(
        id: "MNG004",
        title: "Union case should be sealed",
        messageFormat: "Case type '{0}' in union '{1}' is not sealed. Consider making case types sealed to prevent further inheritance.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Union case types should typically be sealed to prevent creating sub-cases that could break exhaustive pattern matching.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG004");

    /// <summary>
    /// MNG005: Duplicate case name.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateCaseName = new(
        id: "MNG005",
        title: "Duplicate union case name",
        messageFormat: "Duplicate case name '{0}' in union '{1}'. Each case must have a unique name.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each case in a union must have a unique name to enable unambiguous pattern matching and Is{Case} property generation.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG005");

    /// <summary>
    /// MNG006: Case type is not nested.
    /// </summary>
    public static readonly DiagnosticDescriptor CaseNotNested = new(
        id: "MNG006",
        title: "Union case is not nested",
        messageFormat: "Type '{0}' inherits from union '{1}' but is not nested inside it. Move the case type inside the union type.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Union case types should be nested inside the union type for proper encapsulation and to enable factory method generation.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG006");

    /// <summary>
    /// MNG007: Union type should not have instance fields.
    /// </summary>
    public static readonly DiagnosticDescriptor UnionHasInstanceFields = new(
        id: "MNG007",
        title: "Union type has instance fields",
        messageFormat: "Union type '{0}' has instance fields. Consider moving fields to case types instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Union types typically should not have instance fields. Each case should define its own properties. Shared behavior can use abstract members instead.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG007");

    /// <summary>
    /// MNG008: Prefer record for union types.
    /// </summary>
    public static readonly DiagnosticDescriptor PreferRecord = new(
        id: "MNG008",
        title: "Consider using record for union",
        messageFormat: "Union type '{0}' is a class. Consider using 'abstract partial record' for automatic equality, deconstruction, and with-expressions.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Records provide built-in value equality, deconstruction, and with-expressions which are typically desirable for discriminated unions.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/UnionAttribute.md#MNG008");

    /// <summary>
    /// Creates a diagnostic for a type that must be abstract.
    /// </summary>
    public static Diagnostic CreateTypeMustBeAbstract(Location location, string typeName)
        => Diagnostic.Create(TypeMustBeAbstract, location, typeName);

    /// <summary>
    /// Creates a diagnostic for a type that must be partial.
    /// </summary>
    public static Diagnostic CreateTypeMustBePartial(Location location, string typeName)
        => Diagnostic.Create(TypeMustBePartial, location, typeName);

    /// <summary>
    /// Creates a diagnostic for a union with no cases.
    /// </summary>
    public static Diagnostic CreateNoCasesFound(Location location, string typeName)
        => Diagnostic.Create(NoCasesFound, location, typeName);

    /// <summary>
    /// Creates a diagnostic for a case that should be sealed.
    /// </summary>
    public static Diagnostic CreateCaseShouldBeSealed(Location location, string caseName, string unionName)
        => Diagnostic.Create(CaseShouldBeSealed, location, caseName, unionName);

    /// <summary>
    /// Creates a diagnostic for duplicate case names.
    /// </summary>
    public static Diagnostic CreateDuplicateCaseName(Location location, string caseName, string unionName)
        => Diagnostic.Create(DuplicateCaseName, location, caseName, unionName);

    /// <summary>
    /// Creates a diagnostic for a union with instance fields.
    /// </summary>
    public static Diagnostic CreateUnionHasInstanceFields(Location location, string typeName)
        => Diagnostic.Create(UnionHasInstanceFields, location, typeName);

    /// <summary>
    /// Creates a diagnostic suggesting record over class.
    /// </summary>
    public static Diagnostic CreatePreferRecord(Location location, string typeName)
        => Diagnostic.Create(PreferRecord, location, typeName);
}

