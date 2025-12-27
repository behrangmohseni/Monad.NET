using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantMapChainAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Either", "Try", "Validation", "RemoteData");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RedundantMapChain);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (!IsMapCall(invocation, out var innerMemberAccess))
            return;

        if (innerMemberAccess?.Expression is not InvocationExpressionSyntax innerInvocation)
            return;

        if (!IsMapCall(innerInvocation, out _))
            return;

        var expressionType = GetRootExpressionType(innerInvocation, context);
        if (expressionType is null)
            return;

        var typeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(typeName))
            return;

        if (!AreMapsCombinable(invocation, innerInvocation))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.RedundantMapChain, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsMapCall(InvocationExpressionSyntax invocation, out MemberAccessExpressionSyntax? memberAccess)
    {
        memberAccess = null;
        if (invocation.Expression is not MemberAccessExpressionSyntax access || access.Name.Identifier.Text != "Map")
            return false;
        memberAccess = access;
        return true;
    }

    private static ITypeSymbol? GetRootExpressionType(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
    {
        ExpressionSyntax? current = invocation;
        while (current is InvocationExpressionSyntax inv && inv.Expression is MemberAccessExpressionSyntax memberAccess)
            current = memberAccess.Expression;
        return context.SemanticModel.GetTypeInfo(current, context.CancellationToken).Type;
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;

    private static bool AreMapsCombinable(InvocationExpressionSyntax outerMap, InvocationExpressionSyntax innerMap)
    {
        if (outerMap.ArgumentList.Arguments.Count != 1 || innerMap.ArgumentList.Arguments.Count != 1)
            return false;
        var outerArg = outerMap.ArgumentList.Arguments[0].Expression;
        var innerArg = innerMap.ArgumentList.Arguments[0].Expression;
        return IsSimpleLambdaOrMethodGroup(outerArg) && IsSimpleLambdaOrMethodGroup(innerArg);
    }

    private static bool IsSimpleLambdaOrMethodGroup(ExpressionSyntax expression) => expression switch
    {
        SimpleLambdaExpressionSyntax simple => simple.ExpressionBody is not null,
        ParenthesizedLambdaExpressionSyntax paren => paren.ExpressionBody is not null,
        IdentifierNameSyntax or MemberAccessExpressionSyntax => true,
        _ => false
    };
}
