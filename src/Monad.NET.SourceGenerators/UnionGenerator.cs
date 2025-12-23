using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Monad.NET.SourceGenerators;

/// <summary>
/// Source generator that creates comprehensive discriminated union support for types marked with [Union].
/// Generates: Match methods, Is{Case} properties, As{Case}() methods, Map, Tap, and factory methods.
/// </summary>
[Generator]
public class UnionGenerator : IIncrementalGenerator
{
    private const string UnionAttributeSource = """
        namespace Monad.NET;

        /// <summary>
        /// Marks a type as a discriminated union, enabling source generation of Match methods,
        /// Is{Case} properties, As{Case}() methods, and other utility methods.
        /// </summary>
        /// <remarks>
        /// The type must be:
        /// - Abstract
        /// - Partial
        /// - Have nested types that inherit from it (the union cases)
        /// </remarks>
        /// <example>
        /// <code>
        /// [Union]
        /// public abstract partial record Shape
        /// {
        ///     public sealed record Circle(double Radius) : Shape;
        ///     public sealed record Rectangle(double Width, double Height) : Shape;
        ///     public sealed record Triangle(double Base, double Height) : Shape;
        /// }
        /// 
        /// // Generated methods:
        /// // - Match&lt;T&gt;(circle, rectangle, triangle)
        /// // - Match(circle, rectangle, triangle) (void)
        /// // - IsCircle, IsRectangle, IsTriangle properties
        /// // - AsCircle(), AsRectangle(), AsTriangle() returning Option&lt;T&gt;
        /// // - Map&lt;T&gt;(circle, rectangle, triangle)
        /// // - Tap(circle, rectangle, triangle)
        /// </code>
        /// </example>
        [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        internal sealed class UnionAttribute : global::System.Attribute
        {
            /// <summary>
            /// When true, generates factory methods (e.g., Shape.Circle(...)) on the base type.
            /// Default is true.
            /// </summary>
            public bool GenerateFactoryMethods { get; set; } = true;
            
            /// <summary>
            /// When true, generates As{Case}() methods that return Option&lt;CaseType&gt;.
            /// Requires Monad.NET to be referenced. Default is true.
            /// </summary>
            public bool GenerateAsOptionMethods { get; set; } = true;
            
            public UnionAttribute() { }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("UnionAttribute.g.cs", SourceText.From(UnionAttributeSource, Encoding.UTF8)));

        // Find all classes with [Union] attribute
        var unionTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Monad.NET.UnionAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => GetUnionInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Generate the source
        context.RegisterSourceOutput(unionTypes, static (ctx, info) => GenerateSource(ctx, info));
    }

    private static UnionInfo? GetUnionInfo(GeneratorAttributeSyntaxContext context)
    {
        if (!TryGetValidUnionType(context, out var symbol, out var syntax))
            return null;

        var (generateFactoryMethods, generateAsOptionMethods) = ExtractAttributeOptions(context.Attributes);
        var cases = ExtractUnionCases(symbol);

        if (cases.Length == 0)
            return null;

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        var hasMonadOption = context.SemanticModel.Compilation
            .GetTypeByMetadataName("Monad.NET.Option`1") is not null;

        return new UnionInfo(
            symbol.Name,
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ns,
            cases,
            IsRecord: syntax is RecordDeclarationSyntax,
            generateFactoryMethods,
            generateAsOptionMethods && hasMonadOption);
    }

    private static bool TryGetValidUnionType(
        GeneratorAttributeSyntaxContext context,
        out INamedTypeSymbol symbol,
        out TypeDeclarationSyntax syntax)
    {
        symbol = null!;
        syntax = null!;

        if (context.TargetSymbol is not INamedTypeSymbol namedSymbol || !namedSymbol.IsAbstract)
            return false;

        if (context.TargetNode is not TypeDeclarationSyntax typeSyntax)
            return false;

        if (!typeSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            return false;

        symbol = namedSymbol;
        syntax = typeSyntax;
        return true;
    }

    private static (bool GenerateFactoryMethods, bool GenerateAsOptionMethods) ExtractAttributeOptions(
        ImmutableArray<AttributeData> attributes)
    {
        var generateFactoryMethods = true;
        var generateAsOptionMethods = true;

        var attributeData = attributes.FirstOrDefault();
        if (attributeData is null)
            return (generateFactoryMethods, generateAsOptionMethods);

        foreach (var namedArg in attributeData.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "GenerateFactoryMethods" when namedArg.Value.Value is bool value:
                    generateFactoryMethods = value;
                    break;
                case "GenerateAsOptionMethods" when namedArg.Value.Value is bool value:
                    generateAsOptionMethods = value;
                    break;
            }
        }

        return (generateFactoryMethods, generateAsOptionMethods);
    }

    private static ImmutableArray<UnionCase> ExtractUnionCases(INamedTypeSymbol symbol)
    {
        var cases = ImmutableArray.CreateBuilder<UnionCase>();

        foreach (var member in symbol.GetTypeMembers())
        {
            if (member.BaseType?.Equals(symbol, SymbolEqualityComparer.Default) != true)
                continue;

            var caseName = member.Name;
            var parameters = ExtractCaseParameters(member);

            cases.Add(new UnionCase(
                caseName,
                member.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                ParameterName: ToCamelCase(caseName),
                parameters));
        }

        return cases.ToImmutable();
    }

    private static ImmutableArray<UnionCaseParameter> ExtractCaseParameters(INamedTypeSymbol caseType)
    {
        var primaryCtor = caseType.Constructors
            .Where(c => !c.IsImplicitlyDeclared && c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (primaryCtor is null)
            return ImmutableArray<UnionCaseParameter>.Empty;

        return primaryCtor.Parameters
            .Select(p => new UnionCaseParameter(
                p.Name,
                p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))
            .ToImmutableArray();
    }

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);

    private static void GenerateSource(SourceProductionContext context, UnionInfo info)
    {
        // Estimate capacity to reduce reallocations
        var estimatedCapacity = 2048 + (info.Cases.Length * 512);
        var sb = new StringBuilder(estimatedCapacity);

        sb.Append("// <auto-generated />\n#nullable enable\n\n");

        if (info.Namespace is not null)
        {
            sb.Append("namespace ").Append(info.Namespace).Append(";\n\n");
        }

        var typeKeyword = info.IsRecord ? "record" : "class";
        sb.Append("partial ").Append(typeKeyword).Append(' ').Append(info.Name).Append('\n');
        sb.Append("{\n");

        // Generate Is{Case} properties
        GenerateIsCaseProperties(sb, info);

        // Generate As{Case}() methods (if Option is available)
        if (info.GenerateAsOptionMethods)
        {
            GenerateAsCaseMethods(sb, info);
        }

        // Generate Match<TResult> method
        GenerateMatchMethod(sb, info);

        // Generate Match (void) method
        GenerateMatchVoidMethod(sb, info);

        // Generate Map method
        GenerateMapMethod(sb, info);

        // Generate Tap method
        GenerateTapMethod(sb, info);

        // Generate factory methods
        if (info.GenerateFactoryMethods)
        {
            GenerateFactoryMethods(sb, info);
        }

        sb.Append("}\n");

        context.AddSource(
            string.Concat(info.Name, ".Union.g.cs"),
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateIsCaseProperties(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    #region Is{Case} Properties\n\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("    /// <summary>\n")
              .Append("    /// Returns true if this is the ").Append(unionCase.Name).Append(" case.\n")
              .Append("    /// </summary>\n")
              .Append("    public bool Is").Append(unionCase.Name).Append(" => this is ").Append(unionCase.FullTypeName).Append(";\n\n");
        }

        sb.Append("    #endregion\n\n");
    }

    private static void GenerateAsCaseMethods(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    #region As{Case}() Methods\n\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("    /// <summary>\n")
              .Append("    /// Attempts to cast this to the ").Append(unionCase.Name).Append(" case.\n")
              .Append("    /// Returns Some(").Append(unionCase.Name).Append(") if successful, None otherwise.\n")
              .Append("    /// </summary>\n")
              .Append("    public global::Monad.NET.Option<").Append(unionCase.FullTypeName).Append("> As").Append(unionCase.Name).Append("()\n")
              .Append("    {\n")
              .Append("        return this is ").Append(unionCase.FullTypeName).Append(" __case__\n")
              .Append("            ? global::Monad.NET.Option<").Append(unionCase.FullTypeName).Append(">.Some(__case__)\n")
              .Append("            : global::Monad.NET.Option<").Append(unionCase.FullTypeName).Append(">.None();\n")
              .Append("    }\n\n");
        }

        sb.Append("    #endregion\n\n");
    }

    private static void GenerateMatchMethod(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    #region Match Methods\n\n")
          .Append("    /// <summary>\n")
          .Append("    /// Pattern matches on all cases of this union, returning a result.\n")
          .Append("    /// All cases must be handled - this provides compile-time exhaustiveness checking.\n")
          .Append("    /// </summary>\n")
          .Append("    public TResult Match<TResult>(");

        // Append parameters
        for (var i = 0; i < info.Cases.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var c = info.Cases[i];
            sb.Append("global::System.Func<").Append(c.FullTypeName).Append(", TResult> ").Append(c.ParameterName);
        }

        sb.Append(")\n")
          .Append("    {\n")
          .Append("        return this switch\n")
          .Append("        {\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("            ").Append(unionCase.FullTypeName).Append(" __case__ => ").Append(unionCase.ParameterName).Append("(__case__),\n");
        }

        sb.Append("            _ => throw new global::System.InvalidOperationException($\"Unknown case: {GetType().Name}\")\n")
          .Append("        };\n")
          .Append("    }\n\n");
    }

    private static void GenerateMatchVoidMethod(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    /// <summary>\n")
          .Append("    /// Pattern matches on all cases of this union, executing an action.\n")
          .Append("    /// All cases must be handled - this provides compile-time exhaustiveness checking.\n")
          .Append("    /// </summary>\n")
          .Append("    public void Match(");

        // Append parameters
        for (var i = 0; i < info.Cases.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var c = info.Cases[i];
            sb.Append("global::System.Action<").Append(c.FullTypeName).Append("> ").Append(c.ParameterName);
        }

        sb.Append(")\n")
          .Append("    {\n")
          .Append("        switch (this)\n")
          .Append("        {\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("            case ").Append(unionCase.FullTypeName).Append(" __case__:\n")
              .Append("                ").Append(unionCase.ParameterName).Append("(__case__);\n")
              .Append("                break;\n");
        }

        sb.Append("            default:\n")
          .Append("                throw new global::System.InvalidOperationException($\"Unknown case: {GetType().Name}\");\n")
          .Append("        }\n")
          .Append("    }\n\n")
          .Append("    #endregion\n\n");
    }

    private static void GenerateMapMethod(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    #region Map Method\n\n")
          .Append("    /// <summary>\n")
          .Append("    /// Maps each case to a new union value using the provided transformation functions.\n")
          .Append("    /// </summary>\n")
          .Append("    public ").Append(info.FullTypeName).Append(" Map(");

        // Append parameters
        for (var i = 0; i < info.Cases.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var c = info.Cases[i];
            sb.Append("global::System.Func<").Append(c.FullTypeName).Append(", ").Append(info.FullTypeName).Append("> ").Append(c.ParameterName);
        }

        sb.Append(")\n")
          .Append("    {\n")
          .Append("        return this switch\n")
          .Append("        {\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("            ").Append(unionCase.FullTypeName).Append(" __case__ => ").Append(unionCase.ParameterName).Append("(__case__),\n");
        }

        sb.Append("            _ => throw new global::System.InvalidOperationException($\"Unknown case: {GetType().Name}\")\n")
          .Append("        };\n")
          .Append("    }\n\n")
          .Append("    #endregion\n\n");
    }

    private static void GenerateTapMethod(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    #region Tap Method\n\n")
          .Append("    /// <summary>\n")
          .Append("    /// Executes an action on the current case without modifying the value.\n")
          .Append("    /// Useful for side effects like logging.\n")
          .Append("    /// </summary>\n")
          .Append("    public ").Append(info.FullTypeName).Append(" Tap(");

        // Append parameters
        for (var i = 0; i < info.Cases.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var c = info.Cases[i];
            sb.Append("global::System.Action<").Append(c.FullTypeName).Append(">? ").Append(c.ParameterName).Append(" = null");
        }

        sb.Append(")\n")
          .Append("    {\n")
          .Append("        switch (this)\n")
          .Append("        {\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("            case ").Append(unionCase.FullTypeName).Append(" __case__:\n")
              .Append("                ").Append(unionCase.ParameterName).Append("?.Invoke(__case__);\n")
              .Append("                break;\n");
        }

        sb.Append("        }\n")
          .Append("        return this;\n")
          .Append("    }\n\n")
          .Append("    #endregion\n\n");
    }

    private static void GenerateFactoryMethods(StringBuilder sb, UnionInfo info)
    {
        sb.Append("    #region Factory Methods\n\n");

        foreach (var unionCase in info.Cases)
        {
            sb.Append("    /// <summary>\n")
              .Append("    /// Creates a new ").Append(unionCase.Name).Append(" case.\n")
              .Append("    /// </summary>\n");

            if (unionCase.Parameters.Length > 0)
            {
                // Factory method with parameters
                sb.Append("    public static ").Append(unionCase.FullTypeName).Append(" New").Append(unionCase.Name).Append('(');

                for (var i = 0; i < unionCase.Parameters.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    var p = unionCase.Parameters[i];
                    sb.Append(p.FullTypeName).Append(' ').Append(p.Name);
                }

                sb.Append(")\n        => new ").Append(unionCase.FullTypeName).Append('(');

                for (var i = 0; i < unionCase.Parameters.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(unionCase.Parameters[i].Name);
                }

                sb.Append(");\n\n");
            }
            else
            {
                // Factory method without parameters
                sb.Append("    public static ").Append(unionCase.FullTypeName).Append(" New").Append(unionCase.Name).Append("()\n")
                  .Append("        => new ").Append(unionCase.FullTypeName).Append("();\n\n");
            }
        }

        sb.Append("    #endregion\n");
    }
}

internal sealed record UnionInfo(
    string Name,
    string FullTypeName,
    string? Namespace,
    ImmutableArray<UnionCase> Cases,
    bool IsRecord,
    bool GenerateFactoryMethods,
    bool GenerateAsOptionMethods);

internal sealed record UnionCase(
    string Name,
    string FullTypeName,
    string ParameterName,
    ImmutableArray<UnionCaseParameter> Parameters);

internal sealed record UnionCaseParameter(
    string Name,
    string FullTypeName,
    string ShortTypeName);

