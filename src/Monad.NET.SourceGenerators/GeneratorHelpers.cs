using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.SourceGenerators;

/// <summary>
/// Shared helper methods for Union and ErrorUnion source generators.
/// </summary>
internal static class GeneratorHelpers
{
    /// <summary>
    /// Validates that the target is a valid union type (abstract, partial class/record).
    /// </summary>
    public static bool TryGetValidUnionType(
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

    /// <summary>
    /// Converts a PascalCase name to camelCase.
    /// </summary>
    public static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);

    /// <summary>
    /// Appends Func parameters for Match&lt;TResult&gt; method generation.
    /// </summary>
    public static void AppendFuncParameters<TCase>(
        StringBuilder sb,
        IReadOnlyList<TCase> cases,
        Func<TCase, string> getFullTypeName,
        Func<TCase, string> getParameterName,
        string resultType = "TResult")
    {
        for (var i = 0; i < cases.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var c = cases[i];
            sb.Append("global::System.Func<")
              .Append(getFullTypeName(c))
              .Append(", ")
              .Append(resultType)
              .Append("> ")
              .Append(getParameterName(c));
        }
    }

    /// <summary>
    /// Appends Action parameters for Match (void) method generation.
    /// </summary>
    public static void AppendActionParameters<TCase>(
        StringBuilder sb,
        IReadOnlyList<TCase> cases,
        Func<TCase, string> getFullTypeName,
        Func<TCase, string> getParameterName)
    {
        for (var i = 0; i < cases.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var c = cases[i];
            sb.Append("global::System.Action<")
              .Append(getFullTypeName(c))
              .Append("> ")
              .Append(getParameterName(c));
        }
    }

    /// <summary>
    /// Appends switch expression arms for case matching.
    /// </summary>
    public static void AppendSwitchExpressionArms<TCase>(
        StringBuilder sb,
        IReadOnlyList<TCase> cases,
        Func<TCase, string> getFullTypeName,
        Func<TCase, string> getParameterName,
        string caseVarName = "__case__")
    {
        foreach (var c in cases)
        {
            sb.Append("            ")
              .Append(getFullTypeName(c))
              .Append(' ')
              .Append(caseVarName)
              .Append(" => ")
              .Append(getParameterName(c))
              .Append('(')
              .Append(caseVarName)
              .Append("),\n");
        }
    }

    /// <summary>
    /// Appends switch statement cases for void matching.
    /// </summary>
    public static void AppendSwitchStatementCases<TCase>(
        StringBuilder sb,
        IReadOnlyList<TCase> cases,
        Func<TCase, string> getFullTypeName,
        Func<TCase, string> getParameterName,
        string caseVarName = "__case__")
    {
        foreach (var c in cases)
        {
            sb.Append("            case ")
              .Append(getFullTypeName(c))
              .Append(' ')
              .Append(caseVarName)
              .Append(":\n")
              .Append("                ")
              .Append(getParameterName(c))
              .Append('(')
              .Append(caseVarName)
              .Append(");\n")
              .Append("                break;\n");
        }
    }

    /// <summary>
    /// Generates a Match&lt;TResult&gt; method that returns a value.
    /// </summary>
    public static void GenerateMatchMethod<TCase>(
        StringBuilder sb,
        IReadOnlyList<TCase> cases,
        Func<TCase, string> getFullTypeName,
        Func<TCase, string> getParameterName,
        string caseTypeName)
    {
        sb.Append("    #region Match Methods\n\n")
          .Append("    /// <summary>\n")
          .Append("    /// Pattern matches on all ").Append(caseTypeName).Append("s, returning a result.\n")
          .Append("    /// All cases must be handled - this provides compile-time exhaustiveness checking.\n")
          .Append("    /// </summary>\n")
          .Append("    public TResult Match<TResult>(");

        AppendFuncParameters(sb, cases, getFullTypeName, getParameterName);

        sb.Append(")\n")
          .Append("    {\n")
          .Append("        return this switch\n")
          .Append("        {\n");

        AppendSwitchExpressionArms(sb, cases, getFullTypeName, getParameterName);

        sb.Append("            _ => throw new global::System.InvalidOperationException($\"Unknown ").Append(caseTypeName).Append(": {GetType().Name}\")\n")
          .Append("        };\n")
          .Append("    }\n\n");
    }

    /// <summary>
    /// Generates a Match void method that executes an action.
    /// </summary>
    public static void GenerateMatchVoidMethod<TCase>(
        StringBuilder sb,
        IReadOnlyList<TCase> cases,
        Func<TCase, string> getFullTypeName,
        Func<TCase, string> getParameterName,
        string caseTypeName)
    {
        sb.Append("    /// <summary>\n")
          .Append("    /// Pattern matches on all ").Append(caseTypeName).Append("s, executing an action.\n")
          .Append("    /// All cases must be handled - this provides compile-time exhaustiveness checking.\n")
          .Append("    /// </summary>\n")
          .Append("    public void Match(");

        AppendActionParameters(sb, cases, getFullTypeName, getParameterName);

        sb.Append(")\n")
          .Append("    {\n")
          .Append("        switch (this)\n")
          .Append("        {\n");

        AppendSwitchStatementCases(sb, cases, getFullTypeName, getParameterName);

        sb.Append("            default:\n")
          .Append("                throw new global::System.InvalidOperationException($\"Unknown ").Append(caseTypeName).Append(": {GetType().Name}\");\n")
          .Append("        }\n")
          .Append("    }\n\n")
          .Append("    #endregion\n\n");
    }
}
