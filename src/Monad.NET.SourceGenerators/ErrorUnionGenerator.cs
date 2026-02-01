using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Monad.NET.SourceGenerators;

/// <summary>
/// Source generator that creates typed error hierarchies for use with Result&lt;T, TError&gt;.
/// Generates: Match methods, Is{Case} properties, As{Case}() methods, and Result extension methods.
/// </summary>
[Generator]
public class ErrorUnionGenerator : IIncrementalGenerator
{
    private const string ErrorUnionAttributeSource = """
        namespace Monad.NET;

        /// <summary>
        /// Marks a type as an error union for use with Result&lt;T, TError&gt;.
        /// Generates comprehensive pattern matching support for typed error hierarchies.
        /// </summary>
        /// <remarks>
        /// The type must be:
        /// - Abstract
        /// - Partial
        /// - Have nested types that inherit from it (the error cases)
        /// </remarks>
        /// <example>
        /// <code>
        /// [ErrorUnion]
        /// public abstract partial record UserError
        /// {
        ///     public sealed partial record NotFound(Guid Id) : UserError;
        ///     public sealed partial record InvalidEmail(string Email) : UserError;
        ///     public sealed partial record Unauthorized : UserError;
        /// }
        /// 
        /// // Usage with Result:
        /// Result&lt;User, UserError&gt; result = GetUser(id);
        /// 
        /// // Generated extension method:
        /// result.MatchError(
        ///     notFound: e => $"User {e.Id} not found",
        ///     invalidEmail: e => $"Invalid email: {e.Email}",
        ///     unauthorized: _ => "Access denied");
        /// </code>
        /// </example>
        [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        internal sealed class ErrorUnionAttribute : global::System.Attribute
        {
            /// <summary>
            /// When true, generates factory methods (e.g., UserError.NotFound(...)) on the base type.
            /// Default is true.
            /// </summary>
            public bool GenerateFactoryMethods { get; set; } = true;

            /// <summary>
            /// When true, generates Result extension methods for typed error matching.
            /// Default is true.
            /// </summary>
            public bool GenerateResultExtensions { get; set; } = true;

            public ErrorUnionAttribute() { }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ErrorUnionAttribute.g.cs", SourceText.From(ErrorUnionAttributeSource, Encoding.UTF8)));

        // Find all classes with [ErrorUnion] attribute
        var errorUnionTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Monad.NET.ErrorUnionAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => GetErrorUnionInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Generate the source
        context.RegisterSourceOutput(errorUnionTypes, static (ctx, info) => GenerateSource(ctx, info));
    }

    private static ErrorUnionInfo? GetErrorUnionInfo(GeneratorAttributeSyntaxContext context)
    {
        if (!TryGetValidErrorUnionType(context, out var symbol, out var syntax))
            return null;

        var (generateFactoryMethods, generateResultExtensions) = ExtractAttributeOptions(context.Attributes);
        var cases = ExtractErrorCases(symbol);

        if (cases.Length == 0)
            return null;

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        var hasResult = context.SemanticModel.Compilation
            .GetTypeByMetadataName("Monad.NET.Result`2") is not null;

        var hasOption = context.SemanticModel.Compilation
            .GetTypeByMetadataName("Monad.NET.Option`1") is not null;

        return new ErrorUnionInfo(
            symbol.Name,
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ns,
            cases,
            IsRecord: syntax is RecordDeclarationSyntax,
            generateFactoryMethods,
            generateResultExtensions && hasResult,
            hasOption);
    }

    private static bool TryGetValidErrorUnionType(
        GeneratorAttributeSyntaxContext context,
        out INamedTypeSymbol symbol,
        out TypeDeclarationSyntax syntax) =>
        GeneratorHelpers.TryGetValidUnionType(context, out symbol, out syntax);

    private static (bool GenerateFactoryMethods, bool GenerateResultExtensions) ExtractAttributeOptions(
        ImmutableArray<AttributeData> attributes)
    {
        var generateFactoryMethods = true;
        var generateResultExtensions = true;

        var attributeData = attributes.FirstOrDefault();
        if (attributeData is null)
            return (generateFactoryMethods, generateResultExtensions);

        foreach (var namedArg in attributeData.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "GenerateFactoryMethods" when namedArg.Value.Value is bool value:
                    generateFactoryMethods = value;
                    break;
                case "GenerateResultExtensions" when namedArg.Value.Value is bool value:
                    generateResultExtensions = value;
                    break;
            }
        }

        return (generateFactoryMethods, generateResultExtensions);
    }

    private static ImmutableArray<ErrorCase> ExtractErrorCases(INamedTypeSymbol symbol)
    {
        var cases = ImmutableArray.CreateBuilder<ErrorCase>();

        foreach (var member in symbol.GetTypeMembers())
        {
            if (member.BaseType?.Equals(symbol, SymbolEqualityComparer.Default) != true)
                continue;

            var caseName = member.Name;
            var parameters = ExtractCaseParameters(member);

            cases.Add(new ErrorCase(
                caseName,
                member.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                ParameterName: ToCamelCase(caseName),
                parameters));
        }

        return cases.ToImmutable();
    }

    private static ImmutableArray<ErrorCaseParameter> ExtractCaseParameters(INamedTypeSymbol caseType)
    {
        var primaryCtor = caseType.Constructors
            .Where(c => !c.IsImplicitlyDeclared && c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (primaryCtor is null)
            return ImmutableArray<ErrorCaseParameter>.Empty;

        return primaryCtor.Parameters
            .Select(p => new ErrorCaseParameter(
                p.Name,
                p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))
            .ToImmutableArray();
    }

    private static string ToCamelCase(string name) =>
        GeneratorHelpers.ToCamelCase(name);

    private static void GenerateSource(SourceProductionContext context, ErrorUnionInfo info)
    {
        try
        {
            // Generate the error union type itself
            GenerateErrorUnionType(context, info);

            // Generate Result extension methods
            if (info.GenerateResultExtensions)
            {
                GenerateResultExtensions(context, info);
            }
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(
                UnionDiagnostics.CreateSourceGenerationFailed(info.Name, ex.Message));
        }
    }

    private static void GenerateErrorUnionType(SourceProductionContext context, ErrorUnionInfo info)
    {
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
        if (info.HasOption)
        {
            GenerateAsCaseMethods(sb, info);
        }

        // Generate Match<TResult> method
        GenerateMatchMethod(sb, info);

        // Generate Match (void) method
        GenerateMatchVoidMethod(sb, info);

        // Generate factory methods
        if (info.GenerateFactoryMethods)
        {
            GenerateFactoryMethods(sb, info);
        }

        // Generate ToResult helper
        GenerateToResultMethod(sb, info);

        sb.Append("}\n");

        context.AddSource(
            string.Concat(info.Name, ".ErrorUnion.g.cs"),
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateIsCaseProperties(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    #region Is{Case} Properties\n\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("    /// <summary>\n")
              .Append("    /// Returns true if this error is the ").Append(errorCase.Name).Append(" case.\n")
              .Append("    /// </summary>\n")
              .Append("    public bool Is").Append(errorCase.Name).Append(" => this is ").Append(errorCase.FullTypeName).Append(";\n\n");
        }

        sb.Append("    #endregion\n\n");
    }

    private static void GenerateAsCaseMethods(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    #region As{Case}() Methods\n\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("    /// <summary>\n")
              .Append("    /// Attempts to cast this to the ").Append(errorCase.Name).Append(" case.\n")
              .Append("    /// Returns Some(").Append(errorCase.Name).Append(") if successful, None otherwise.\n")
              .Append("    /// </summary>\n")
              .Append("    public global::Monad.NET.Option<").Append(errorCase.FullTypeName).Append("> As").Append(errorCase.Name).Append("()\n")
              .Append("    {\n")
              .Append("        return this is ").Append(errorCase.FullTypeName).Append(" __case__\n")
              .Append("            ? global::Monad.NET.Option<").Append(errorCase.FullTypeName).Append(">.Some(__case__)\n")
              .Append("            : global::Monad.NET.Option<").Append(errorCase.FullTypeName).Append(">.None();\n")
              .Append("    }\n\n");
        }

        sb.Append("    #endregion\n\n");
    }

    private static void GenerateMatchMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        var cases = (IReadOnlyList<ErrorCase>)info.Cases;

        GeneratorHelpers.GenerateMatchMethod(sb, cases, c => c.FullTypeName, c => c.ParameterName, "error case");
    }

    private static void GenerateMatchVoidMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        var cases = (IReadOnlyList<ErrorCase>)info.Cases;

        GeneratorHelpers.GenerateMatchVoidMethod(sb, cases, c => c.FullTypeName, c => c.ParameterName, "error case");
    }

    private static void GenerateFactoryMethods(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    #region Factory Methods\n\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("    /// <summary>\n")
              .Append("    /// Creates a new ").Append(errorCase.Name).Append(" error.\n")
              .Append("    /// </summary>\n");

            if (errorCase.Parameters.Length > 0)
            {
                sb.Append("    public static ").Append(errorCase.FullTypeName).Append(" New").Append(errorCase.Name).Append('(');

                for (var i = 0; i < errorCase.Parameters.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    var p = errorCase.Parameters[i];
                    sb.Append(p.FullTypeName).Append(' ').Append(p.Name);
                }

                sb.Append(")\n        => new ").Append(errorCase.FullTypeName).Append('(');

                for (var i = 0; i < errorCase.Parameters.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(errorCase.Parameters[i].Name);
                }

                sb.Append(");\n\n");
            }
            else
            {
                sb.Append("    public static ").Append(errorCase.FullTypeName).Append(" New").Append(errorCase.Name).Append("()\n")
                  .Append("        => new ").Append(errorCase.FullTypeName).Append("();\n\n");
            }
        }

        sb.Append("    #endregion\n\n");
    }

    private static void GenerateToResultMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    #region Result Helpers\n\n")
          .Append("    /// <summary>\n")
          .Append("    /// Converts this error to a failed Result.\n")
          .Append("    /// </summary>\n")
          .Append("    public global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append("> ToResult<T>()\n")
          .Append("        => global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append(">.Err(this);\n\n")
          .Append("    #endregion\n");
    }

    private static void GenerateResultExtensions(SourceProductionContext context, ErrorUnionInfo info)
    {
        var sb = new StringBuilder(2048);

        sb.Append("// <auto-generated />\n#nullable enable\n\n");

        if (info.Namespace is not null)
        {
            sb.Append("namespace ").Append(info.Namespace).Append(";\n\n");
        }

        sb.Append("/// <summary>\n")
          .Append("/// Extension methods for Result&lt;T, ").Append(info.Name).Append("&gt; with typed error matching.\n")
          .Append("/// </summary>\n")
          .Append("public static class ").Append(info.Name).Append("ResultExtensions\n")
          .Append("{\n");

        // Generate MatchError<TResult> for Result
        GenerateResultMatchErrorMethod(sb, info);

        // Generate MatchError (void) for Result
        GenerateResultMatchErrorVoidMethod(sb, info);

        // Generate MapError for Result
        GenerateResultMapErrorMethod(sb, info);

        // Generate Recover method
        GenerateResultRecoverMethod(sb, info);

        sb.Append("}\n");

        context.AddSource(
            string.Concat(info.Name, "ResultExtensions.g.cs"),
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateResultMatchErrorMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    /// <summary>\n")
          .Append("    /// Pattern matches on the error if the Result is Err, otherwise returns the Ok value transformed.\n")
          .Append("    /// </summary>\n")
          .Append("    public static TResult MatchError<T, TResult>(this global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append("> result,\n")
          .Append("        global::System.Func<T, TResult> ok,\n");

        for (var i = 0; i < info.Cases.Length; i++)
        {
            var c = info.Cases[i];
            sb.Append("        global::System.Func<").Append(c.FullTypeName).Append(", TResult> ").Append(c.ParameterName);
            if (i < info.Cases.Length - 1)
                sb.Append(",\n");
            else
                sb.Append(")\n");
        }

        sb.Append("    {\n")
          .Append("        if (result.IsOk)\n")
          .Append("            return ok(result.GetValue());\n\n")
          .Append("        var error = result.GetError();\n")
          .Append("        return error switch\n")
          .Append("        {\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("            ").Append(errorCase.FullTypeName).Append(" __e__ => ").Append(errorCase.ParameterName).Append("(__e__),\n");
        }

        sb.Append("            _ => throw new global::System.InvalidOperationException($\"Unknown error case: {error.GetType().Name}\")\n")
          .Append("        };\n")
          .Append("    }\n\n");
    }

    private static void GenerateResultMatchErrorVoidMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    /// <summary>\n")
          .Append("    /// Pattern matches on the error if the Result is Err, executing the appropriate action.\n")
          .Append("    /// </summary>\n")
          .Append("    public static void MatchError<T>(this global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append("> result,\n")
          .Append("        global::System.Action<T> ok,\n");

        for (var i = 0; i < info.Cases.Length; i++)
        {
            var c = info.Cases[i];
            sb.Append("        global::System.Action<").Append(c.FullTypeName).Append("> ").Append(c.ParameterName);
            if (i < info.Cases.Length - 1)
                sb.Append(",\n");
            else
                sb.Append(")\n");
        }

        sb.Append("    {\n")
          .Append("        if (result.IsOk)\n")
          .Append("        {\n")
          .Append("            ok(result.GetValue());\n")
          .Append("            return;\n")
          .Append("        }\n\n")
          .Append("        var error = result.GetError();\n")
          .Append("        switch (error)\n")
          .Append("        {\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("            case ").Append(errorCase.FullTypeName).Append(" __e__:\n")
              .Append("                ").Append(errorCase.ParameterName).Append("(__e__);\n")
              .Append("                break;\n");
        }

        sb.Append("            default:\n")
          .Append("                throw new global::System.InvalidOperationException($\"Unknown error case: {error.GetType().Name}\");\n")
          .Append("        }\n")
          .Append("    }\n\n");
    }

    private static void GenerateResultMapErrorMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    /// <summary>\n")
          .Append("    /// Maps each error case to a new error value.\n")
          .Append("    /// </summary>\n")
          .Append("    public static global::Monad.NET.Result<T, TNewError> MapError<T, TNewError>(this global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append("> result,\n");

        for (var i = 0; i < info.Cases.Length; i++)
        {
            var c = info.Cases[i];
            sb.Append("        global::System.Func<").Append(c.FullTypeName).Append(", TNewError> ").Append(c.ParameterName);
            if (i < info.Cases.Length - 1)
                sb.Append(",\n");
            else
                sb.Append(")\n");
        }

        sb.Append("    {\n")
          .Append("        if (result.IsOk)\n")
          .Append("            return global::Monad.NET.Result<T, TNewError>.Ok(result.GetValue());\n\n")
          .Append("        var error = result.GetError();\n")
          .Append("        var newError = error switch\n")
          .Append("        {\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("            ").Append(errorCase.FullTypeName).Append(" __e__ => ").Append(errorCase.ParameterName).Append("(__e__),\n");
        }

        sb.Append("            _ => throw new global::System.InvalidOperationException($\"Unknown error case: {error.GetType().Name}\")\n")
          .Append("        };\n")
          .Append("        return global::Monad.NET.Result<T, TNewError>.Err(newError);\n")
          .Append("    }\n\n");
    }

    private static void GenerateResultRecoverMethod(StringBuilder sb, ErrorUnionInfo info)
    {
        sb.Append("    /// <summary>\n")
          .Append("    /// Attempts to recover from each error case, returning a new Result.\n")
          .Append("    /// </summary>\n")
          .Append("    public static global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append("> Recover<T>(this global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append("> result,\n");

        for (var i = 0; i < info.Cases.Length; i++)
        {
            var c = info.Cases[i];
            sb.Append("        global::System.Func<").Append(c.FullTypeName).Append(", global::Monad.NET.Result<T, ").Append(info.FullTypeName).Append(">>? ").Append(c.ParameterName).Append(" = null");
            if (i < info.Cases.Length - 1)
                sb.Append(",\n");
            else
                sb.Append(")\n");
        }

        sb.Append("    {\n")
          .Append("        if (result.IsOk)\n")
          .Append("            return result;\n\n")
          .Append("        var error = result.GetError();\n")
          .Append("        return error switch\n")
          .Append("        {\n");

        foreach (var errorCase in info.Cases)
        {
            sb.Append("            ").Append(errorCase.FullTypeName).Append(" __e__ when ").Append(errorCase.ParameterName).Append(" is not null => ").Append(errorCase.ParameterName).Append("(__e__),\n");
        }

        sb.Append("            _ => result\n")
          .Append("        };\n")
          .Append("    }\n");
    }
}

internal sealed record ErrorUnionInfo(
    string Name,
    string FullTypeName,
    string? Namespace,
    ImmutableArray<ErrorCase> Cases,
    bool IsRecord,
    bool GenerateFactoryMethods,
    bool GenerateResultExtensions,
    bool HasOption);

internal sealed record ErrorCase(
    string Name,
    string FullTypeName,
    string ParameterName,
    ImmutableArray<ErrorCaseParameter> Parameters);

internal sealed record ErrorCaseParameter(
    string Name,
    string FullTypeName,
    string ShortTypeName);

