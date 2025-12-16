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
        var symbol = context.TargetSymbol as INamedTypeSymbol;
        if (symbol is null)
            return null;

        // Must be abstract and partial
        if (!symbol.IsAbstract)
            return null;

        var syntax = context.TargetNode as TypeDeclarationSyntax;
        if (syntax is null)
            return null;

        if (!syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            return null;

        // Get attribute data
        var attributeData = context.Attributes.FirstOrDefault();
        var generateFactoryMethods = true;
        var generateAsOptionMethods = true;

        if (attributeData is not null)
        {
            foreach (var namedArg in attributeData.NamedArguments)
            {
                if (namedArg.Key == "GenerateFactoryMethods" && namedArg.Value.Value is bool factoryValue)
                    generateFactoryMethods = factoryValue;
                if (namedArg.Key == "GenerateAsOptionMethods" && namedArg.Value.Value is bool optionValue)
                    generateAsOptionMethods = optionValue;
            }
        }

        // Find nested types that inherit from this type
        var cases = new List<UnionCase>();
        foreach (var member in symbol.GetTypeMembers())
        {
            if (member.BaseType?.Equals(symbol, SymbolEqualityComparer.Default) == true)
            {
                var caseName = member.Name;
                var camelCaseName = char.ToLowerInvariant(caseName[0]) + caseName.Substring(1);
                
                // Get constructor parameters for factory method generation
                var primaryCtor = member.Constructors
                    .Where(c => !c.IsImplicitlyDeclared && c.Parameters.Length > 0)
                    .OrderByDescending(c => c.Parameters.Length)
                    .FirstOrDefault();
                
                var parameters = primaryCtor?.Parameters
                    .Select(p => new UnionCaseParameter(
                        p.Name,
                        p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))
                    .ToImmutableArray() ?? ImmutableArray<UnionCaseParameter>.Empty;

                cases.Add(new UnionCase(
                    caseName, 
                    member.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), 
                    camelCaseName,
                    parameters));
            }
        }

        if (cases.Count == 0)
            return null;

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        var isRecord = syntax is RecordDeclarationSyntax;

        // Check if Monad.NET Option is available
        var hasMonadOption = context.SemanticModel.Compilation.GetTypeByMetadataName("Monad.NET.Option`1") is not null;

        return new UnionInfo(
            symbol.Name,
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ns,
            cases.ToImmutableArray(),
            isRecord,
            generateFactoryMethods,
            generateAsOptionMethods && hasMonadOption);
    }

    private static void GenerateSource(SourceProductionContext context, UnionInfo info)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (info.Namespace is not null)
        {
            sb.AppendLine($"namespace {info.Namespace};");
            sb.AppendLine();
        }

        var typeKeyword = info.IsRecord ? "record" : "class";
        sb.AppendLine($"partial {typeKeyword} {info.Name}");
        sb.AppendLine("{");

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

        sb.AppendLine("}");

        context.AddSource($"{info.Name}.Union.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateIsCaseProperties(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    #region Is{Case} Properties");
        sb.AppendLine();

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Returns true if this is the {unionCase.Name} case.");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public bool Is{unionCase.Name} => this is {unionCase.FullTypeName};");
            sb.AppendLine();
        }

        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static void GenerateAsCaseMethods(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    #region As{Case}() Methods");
        sb.AppendLine();

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Attempts to cast this to the {unionCase.Name} case.");
            sb.AppendLine($"    /// Returns Some({unionCase.Name}) if successful, None otherwise.");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public global::Monad.NET.Option<{unionCase.FullTypeName}> As{unionCase.Name}()");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        return this is {unionCase.FullTypeName} __case__");
            sb.AppendLine($"            ? global::Monad.NET.Option<{unionCase.FullTypeName}>.Some(__case__)");
            sb.AppendLine($"            : global::Monad.NET.Option<{unionCase.FullTypeName}>.None();");
            sb.AppendLine($"    }}");
            sb.AppendLine();
        }

        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static void GenerateMatchMethod(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    #region Match Methods");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Pattern matches on all cases of this union, returning a result.");
        sb.AppendLine("    /// All cases must be handled - this provides compile-time exhaustiveness checking.");
        sb.AppendLine("    /// </summary>");

        // Method signature
        sb.Append("    public TResult Match<TResult>(");

        var parameters = info.Cases
            .Select(c => $"global::System.Func<{c.FullTypeName}, TResult> {c.ParameterName}")
            .ToList();

        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine("    {");

        // Switch expression
        sb.AppendLine("        return this switch");
        sb.AppendLine("        {");

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"            {unionCase.FullTypeName} __case__ => {unionCase.ParameterName}(__case__),");
        }

        sb.AppendLine($"            _ => throw new global::System.InvalidOperationException($\"Unknown case: {{GetType().Name}}\")");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateMatchVoidMethod(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Pattern matches on all cases of this union, executing an action.");
        sb.AppendLine("    /// All cases must be handled - this provides compile-time exhaustiveness checking.");
        sb.AppendLine("    /// </summary>");

        // Method signature
        sb.Append("    public void Match(");

        var parameters = info.Cases
            .Select(c => $"global::System.Action<{c.FullTypeName}> {c.ParameterName}")
            .ToList();

        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine("    {");

        // Switch statement
        sb.AppendLine("        switch (this)");
        sb.AppendLine("        {");

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"            case {unionCase.FullTypeName} __case__:");
            sb.AppendLine($"                {unionCase.ParameterName}(__case__);");
            sb.AppendLine("                break;");
        }

        sb.AppendLine("            default:");
        sb.AppendLine($"                throw new global::System.InvalidOperationException($\"Unknown case: {{GetType().Name}}\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static void GenerateMapMethod(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    #region Map Method");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Maps each case to a new union value using the provided transformation functions.");
        sb.AppendLine("    /// </summary>");

        // Method signature
        sb.Append($"    public {info.FullTypeName} Map(");

        var parameters = info.Cases
            .Select(c => $"global::System.Func<{c.FullTypeName}, {info.FullTypeName}> {c.ParameterName}")
            .ToList();

        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine("    {");

        // Switch expression
        sb.AppendLine("        return this switch");
        sb.AppendLine("        {");

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"            {unionCase.FullTypeName} __case__ => {unionCase.ParameterName}(__case__),");
        }

        sb.AppendLine($"            _ => throw new global::System.InvalidOperationException($\"Unknown case: {{GetType().Name}}\")");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static void GenerateTapMethod(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    #region Tap Method");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Executes an action on the current case without modifying the value.");
        sb.AppendLine("    /// Useful for side effects like logging.");
        sb.AppendLine("    /// </summary>");

        // Method signature
        sb.Append($"    public {info.FullTypeName} Tap(");

        var parameters = info.Cases
            .Select(c => $"global::System.Action<{c.FullTypeName}>? {c.ParameterName} = null")
            .ToList();

        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine("    {");

        // Switch statement
        sb.AppendLine("        switch (this)");
        sb.AppendLine("        {");

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"            case {unionCase.FullTypeName} __case__:");
            sb.AppendLine($"                {unionCase.ParameterName}?.Invoke(__case__);");
            sb.AppendLine("                break;");
        }

        sb.AppendLine("        }");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static void GenerateFactoryMethods(StringBuilder sb, UnionInfo info)
    {
        sb.AppendLine("    #region Factory Methods");
        sb.AppendLine();

        foreach (var unionCase in info.Cases)
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Creates a new {unionCase.Name} case.");
            sb.AppendLine($"    /// </summary>");

            if (unionCase.Parameters.Length > 0)
            {
                // Factory method with parameters - use "New" prefix to avoid conflicts with nested types
                var paramList = string.Join(", ", unionCase.Parameters
                    .Select(p => $"{p.FullTypeName} {p.Name}"));
                var argList = string.Join(", ", unionCase.Parameters.Select(p => p.Name));

                sb.AppendLine($"    public static {unionCase.FullTypeName} New{unionCase.Name}({paramList})");
                sb.AppendLine($"        => new {unionCase.FullTypeName}({argList});");
            }
            else
            {
                // Factory method without parameters - use "New" prefix
                sb.AppendLine($"    public static {unionCase.FullTypeName} New{unionCase.Name}()");
                sb.AppendLine($"        => new {unionCase.FullTypeName}();");
            }
            sb.AppendLine();
        }

        sb.AppendLine("    #endregion");
    }
}

internal sealed class UnionInfo
{
    public string Name { get; }
    public string FullTypeName { get; }
    public string? Namespace { get; }
    public ImmutableArray<UnionCase> Cases { get; }
    public bool IsRecord { get; }
    public bool GenerateFactoryMethods { get; }
    public bool GenerateAsOptionMethods { get; }

    public UnionInfo(
        string name, 
        string fullTypeName, 
        string? ns, 
        ImmutableArray<UnionCase> cases, 
        bool isRecord,
        bool generateFactoryMethods,
        bool generateAsOptionMethods)
    {
        Name = name;
        FullTypeName = fullTypeName;
        Namespace = ns;
        Cases = cases;
        IsRecord = isRecord;
        GenerateFactoryMethods = generateFactoryMethods;
        GenerateAsOptionMethods = generateAsOptionMethods;
    }
}

internal sealed class UnionCase
{
    public string Name { get; }
    public string FullTypeName { get; }
    public string ParameterName { get; }
    public ImmutableArray<UnionCaseParameter> Parameters { get; }

    public UnionCase(string name, string fullTypeName, string parameterName, ImmutableArray<UnionCaseParameter> parameters)
    {
        Name = name;
        FullTypeName = fullTypeName;
        ParameterName = parameterName;
        Parameters = parameters;
    }
}

internal sealed class UnionCaseParameter
{
    public string Name { get; }
    public string FullTypeName { get; }
    public string ShortTypeName { get; }

    public UnionCaseParameter(string name, string fullTypeName, string shortTypeName)
    {
        Name = name;
        FullTypeName = fullTypeName;
        ShortTypeName = shortTypeName;
    }
}
