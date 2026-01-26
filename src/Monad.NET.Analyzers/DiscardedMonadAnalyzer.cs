using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiscardedMonadAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Try", "Validation");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.DiscardedMonad);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);

    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        var expressionStatement = (ExpressionStatementSyntax)context.Node;
        var expression = expressionStatement.Expression;

        var typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);
        var type = typeInfo.Type;
        if (type is null)
            return;

        var typeName = GetBaseTypeName(type);
        if (!MonadTypes.Contains(typeName))
            return;

        if (expression is InvocationExpressionSyntax invocation)
        {
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
            if (methodSymbol?.ReturnsVoid == true)
                return;

            if (IsSideEffectMethod(invocation))
                return;
        }

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DiscardedMonad, expression.GetLocation(), typeName);
        context.ReportDiagnostic(diagnostic);
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;

    private static bool IsSideEffectMethod(InvocationExpressionSyntax invocation)
    {
        var methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => null
        };
        return methodName is "Match" or "MatchAsync" or "Do" or "DoAsync" or "Tap" or "TapAsync" or "Iter" or "IterAsync";
    }
}
