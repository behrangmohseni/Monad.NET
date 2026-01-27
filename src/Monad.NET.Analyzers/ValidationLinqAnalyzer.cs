using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that warns when LINQ is used with Validation types.
/// While the SelectMany implementation attempts to accumulate errors,
/// it only works reliably when validations are independent.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidationLinqAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ValidationLinqShortCircuits);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeQueryExpression, SyntaxKind.QueryExpression);
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
    {
        var queryExpression = (QueryExpressionSyntax)context.Node;

        // Check if the from clause is using a Validation type
        var fromClause = queryExpression.FromClause;
        var expressionType = context.SemanticModel.GetTypeInfo(fromClause.Expression, context.CancellationToken).Type;

        if (expressionType is null)
            return;

        if (!IsValidationType(expressionType))
            return;

        // Only warn if there are multiple from clauses (SelectMany chains)
        // which is where the error accumulation behavior matters
        var hasMultipleFromClauses = HasMultipleFromClauses(queryExpression);

        if (!hasMultipleFromClauses)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ValidationLinqShortCircuits,
            queryExpression.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a SelectMany call
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name.Identifier.Text;
        if (methodName != "SelectMany")
            return;

        // Check if the receiver is a Validation type
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (receiverType is null || !IsValidationType(receiverType))
            return;

        // Verify this is actually the ValidationLinq.SelectMany method
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.ContainingType?.Name != "ValidationLinq")
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ValidationLinqShortCircuits,
            invocation.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsValidationType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            return namedType.Name == "Validation" &&
                   (namedType.ContainingNamespace?.ToString() == "Monad.NET" ||
                    namedType.ContainingNamespace?.ToString()?.StartsWith("Monad.NET") == true);
        }

        return false;
    }

    private static bool HasMultipleFromClauses(QueryExpressionSyntax query)
    {
        // Count from clauses - the initial one plus any in the body
        var fromCount = 1; // Initial from clause

        foreach (var clause in query.Body.Clauses)
        {
            if (clause is FromClauseSyntax)
            {
                fromCount++;
                if (fromCount >= 2)
                    return true;
            }
        }

        // Also check for continuation (into ... from)
        var continuation = query.Body.Continuation;
        while (continuation != null)
        {
            fromCount++;
            if (fromCount >= 2)
                return true;

            foreach (var clause in continuation.Body.Clauses)
            {
                if (clause is FromClauseSyntax)
                {
                    fromCount++;
                    if (fromCount >= 2)
                        return true;
                }
            }

            continuation = continuation.Body.Continuation;
        }

        return false;
    }
}

