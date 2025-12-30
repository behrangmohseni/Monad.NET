using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that detects async monad operations that should use ConfigureAwait(false) in library code.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingConfigureAwaitAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.MissingConfigureAwait);

    private static readonly HashSet<string> MonadAsyncMethods = new(StringComparer.Ordinal)
    {
        "MapAsync",
        "AndThenAsync",
        "FilterAsync",
        "MatchAsync",
        "TapAsync",
        "TapErrAsync",
        "TapNoneAsync",
        "OrElseAsync",
        "UnwrapOrElseAsync",
        "OkOrElseAsync",
        "ZipAsync",
        "ZipWithAsync",
        "FirstSomeAsync",
        "OfAsync",
        "FlatMapAsync",
        "RecoverAsync",
        "TapExceptionAsync",
        "ApplyAsync",
        "CombineAsync",
        "TapErrorsAsync"
    };

    private static readonly HashSet<string> MonadTypes = new(StringComparer.Ordinal)
    {
        "Option",
        "Result",
        "Either",
        "Try",
        "Validation",
        "RemoteData"
    };

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);

    private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context)
    {
        var awaitExpression = (AwaitExpressionSyntax)context.Node;

        // Get the expression being awaited
        var expression = awaitExpression.Expression;

        // Skip if already has ConfigureAwait
        if (expression is InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name.Identifier.Text == "ConfigureAwait")
                {
                    return;
                }
            }
        }

        // Check if this is a monad async method
        if (!IsMonadAsyncMethod(expression, context.SemanticModel))
        {
            return;
        }

        // Get the monad type name for the message
        var monadTypeName = GetMonadTypeName(expression, context.SemanticModel) ?? "monad";

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.MissingConfigureAwait,
            awaitExpression.GetLocation(),
            monadTypeName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsMonadAsyncMethod(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Check for method invocations like .MapAsync(), .AndThenAsync(), etc.
        if (expression is InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var methodName = memberAccess.Name.Identifier.Text;
                if (MonadAsyncMethods.Contains(methodName))
                {
                    // Verify it's actually a monad type
                    var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
                    if (typeInfo.Type != null)
                    {
                        var typeName = typeInfo.Type.Name;
                        if (MonadTypes.Any(m => typeName.StartsWith(m, StringComparison.Ordinal)))
                        {
                            return true;
                        }

                        // Also check for Task<Option<T>>, Task<Result<T, E>>, etc.
                        if (typeName == "Task" && typeInfo.Type is INamedTypeSymbol namedType)
                        {
                            if (namedType.TypeArguments.Length == 1)
                            {
                                var innerTypeName = namedType.TypeArguments[0].Name;
                                if (MonadTypes.Any(m => innerTypeName.StartsWith(m, StringComparison.Ordinal)))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private static string? GetMonadTypeName(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression is InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
                if (typeInfo.Type != null)
                {
                    var typeName = typeInfo.Type.Name;

                    // Check for direct monad types
                    foreach (var monadType in MonadTypes)
                    {
                        if (typeName.StartsWith(monadType, StringComparison.Ordinal))
                        {
                            return monadType;
                        }
                    }

                    // Check for Task<MonadType>
                    if (typeName == "Task" && typeInfo.Type is INamedTypeSymbol namedType)
                    {
                        if (namedType.TypeArguments.Length == 1)
                        {
                            var innerTypeName = namedType.TypeArguments[0].Name;
                            foreach (var monadType in MonadTypes)
                            {
                                if (innerTypeName.StartsWith(monadType, StringComparison.Ordinal))
                                {
                                    return $"Task<{monadType}>";
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }
}

