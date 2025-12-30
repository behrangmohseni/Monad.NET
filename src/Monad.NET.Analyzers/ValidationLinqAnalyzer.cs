using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that warns when LINQ query syntax is used with Validation types,
/// as this will short-circuit on the first error instead of accumulating all errors.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidationLinqAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ValidationLinqShortCircuits);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeQueryExpression, SyntaxKind.QueryExpression);

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
        // which is where the short-circuiting behavior becomes problematic
        var hasMultipleFromClauses = HasMultipleFromClauses(queryExpression);

        if (!hasMultipleFromClauses)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ValidationLinqShortCircuits,
            queryExpression.GetLocation());

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

