using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FilterConstantAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Try", "Validation", "RemoteData");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.FilterConstant);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.Text != "Filter" && memberAccess.Name.Identifier.Text != "Where")
            return;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = invocation.ArgumentList.Arguments[0].Expression;
        var constantValue = GetConstantBoolValue(argument);

        if (constantValue is null)
            return;

        // Verify this is on a monad type
        var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (expressionType is null)
            return;

        var typeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(typeName))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.FilterConstant, invocation.GetLocation(), constantValue.Value ? "true" : "false");
        context.ReportDiagnostic(diagnostic);
    }

    private static bool? GetConstantBoolValue(ExpressionSyntax expression)
    {
        // Check for x => true or x => false
        ExpressionSyntax? body = expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple.ExpressionBody,
            ParenthesizedLambdaExpressionSyntax paren => paren.ExpressionBody,
            _ => null
        };

        if (body is LiteralExpressionSyntax literal)
        {
            if (literal.IsKind(SyntaxKind.TrueLiteralExpression))
                return true;
            if (literal.IsKind(SyntaxKind.FalseLiteralExpression))
                return false;
        }

        return null;
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;
}
