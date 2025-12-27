using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionNullComparisonAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> StructMonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Either", "Try", "Validation", "RemoteData");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.OptionNullComparison);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeIsPattern, SyntaxKind.IsPatternExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binary = (BinaryExpressionSyntax)context.Node;

        // Check if one side is null
        var (monadExpr, isNullOnRight) = (binary.Left, binary.Right) switch
        {
            (_, LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression }) => (binary.Left, true),
            (LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression }, _) => (binary.Right, false),
            _ => (null, false)
        };

        if (monadExpr is null)
            return;

        var type = context.SemanticModel.GetTypeInfo(monadExpr, context.CancellationToken).Type;
        if (type is null)
            return;

        var typeName = GetBaseTypeName(type);
        if (!StructMonadTypes.Contains(typeName))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.OptionNullComparison, binary.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeIsPattern(SyntaxNodeAnalysisContext context)
    {
        var isPattern = (IsPatternExpressionSyntax)context.Node;

        // Check for "option is null" or "option is not null"
        var isNullPattern = isPattern.Pattern switch
        {
            ConstantPatternSyntax { Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } } => true,
            UnaryPatternSyntax { Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } } } => true,
            _ => false
        };

        if (!isNullPattern)
            return;

        var type = context.SemanticModel.GetTypeInfo(isPattern.Expression, context.CancellationToken).Type;
        if (type is null)
            return;

        var typeName = GetBaseTypeName(type);
        if (!StructMonadTypes.Contains(typeName))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.OptionNullComparison, isPattern.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;
}
