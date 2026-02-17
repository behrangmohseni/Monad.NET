using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Detects default construction of Monad.NET types (e.g., default(Option&lt;T&gt;), Option&lt;T&gt; x = default).
/// Suggests using factory methods instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultMonadConstructionAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableDictionary<string, string> MonadFactorySuggestions =
        ImmutableDictionary.CreateRange(new[]
        {
            new KeyValuePair<string, string>("Option", "Option<T>.None()"),
            new KeyValuePair<string, string>("Result", "Result<T,E>.Ok(value) or Result<T,E>.Error(error)"),
            new KeyValuePair<string, string>("Try", "Try<T>.Ok(value) or Try<T>.Error(exception)"),
            new KeyValuePair<string, string>("Validation", "Validation<T,E>.Ok(value) or Validation<T,E>.Error(error)"),
            new KeyValuePair<string, string>("RemoteData", "RemoteData<T,E>.NotAsked()"),
            new KeyValuePair<string, string>("NonEmptyList", "NonEmptyList<T>.Of(value)"),
            new KeyValuePair<string, string>("Reader", "Reader<R,A>.From(func)"),
        });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.DefaultMonadConstruction);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);
        context.RegisterSyntaxNodeAction(AnalyzeDefaultLiteral, SyntaxKind.DefaultLiteralExpression);
    }

    private static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
    {
        var defaultExpression = (DefaultExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(defaultExpression, context.CancellationToken);
        CheckAndReport(context, typeInfo.Type, defaultExpression.GetLocation());
    }

    private static void AnalyzeDefaultLiteral(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(literal, context.CancellationToken);
        CheckAndReport(context, typeInfo.ConvertedType, literal.GetLocation());
    }

    private static void CheckAndReport(SyntaxNodeAnalysisContext context, ITypeSymbol? type, Location location)
    {
        if (type is not INamedTypeSymbol { IsGenericType: true } namedType)
            return;

        if (namedType.ContainingNamespace?.ToDisplayString() != "Monad.NET")
            return;

        var typeName = namedType.Name;
        if (!MonadFactorySuggestions.TryGetValue(typeName, out var suggestion))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.DefaultMonadConstruction,
            location,
            namedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            suggestion);

        context.ReportDiagnostic(diagnostic);
    }
}
