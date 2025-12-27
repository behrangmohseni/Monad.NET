using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MapIdentityAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Either", "Try", "Validation", "RemoteData");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MapIdentity);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.Text != "Map")
            return;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = invocation.ArgumentList.Arguments[0].Expression;
        if (!IsIdentityFunction(argument))
            return;

        // Verify this is on a monad type
        var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (expressionType is null)
            return;

        var typeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(typeName))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.MapIdentity, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsIdentityFunction(ExpressionSyntax expression)
    {
        // Check for x => x pattern
        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
        {
            if (simpleLambda.ExpressionBody is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.Text == simpleLambda.Parameter.Identifier.Text;
            }
        }

        // Check for (x) => x pattern
        if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            if (parenLambda.ParameterList.Parameters.Count == 1 &&
                parenLambda.ExpressionBody is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.Text == parenLambda.ParameterList.Parameters[0].Identifier.Text;
            }
        }

        return false;
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;
}
