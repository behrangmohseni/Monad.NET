using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MapGetOrElseAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Either", "Try", "Validation", "RemoteData");

    private static readonly ImmutableHashSet<string> GetOrElseMethods = ImmutableHashSet.Create(
        "GetOrElse", "UnwrapOr", "UnwrapOrDefault", "UnwrapOrElse");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MapGetOrElseToMatch);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!TryGetMapGetOrElseChain(invocation, context, out var methodName))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.MapGetOrElseToMatch,
            invocation.GetLocation(),
            methodName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool TryGetMapGetOrElseChain(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        out string methodName)
    {
        methodName = null!;

        if (!TryGetGetOrElseMethod(invocation, out var getOrElseMemberAccess, out methodName))
            return false;

        if (!TryGetPrecedingMapCall(getOrElseMemberAccess, out var mapMemberAccess))
            return false;

        return IsMonadType(mapMemberAccess.Expression, context);
    }

    private static bool TryGetGetOrElseMethod(
        InvocationExpressionSyntax invocation,
        out MemberAccessExpressionSyntax memberAccess,
        out string methodName)
    {
        memberAccess = null!;
        methodName = null!;

        if (invocation.Expression is not MemberAccessExpressionSyntax ma)
            return false;

        var name = ma.Name.Identifier.Text;
        if (!GetOrElseMethods.Contains(name))
            return false;

        memberAccess = ma;
        methodName = name;
        return true;
    }

    private static bool TryGetPrecedingMapCall(
        MemberAccessExpressionSyntax getOrElseMemberAccess,
        out MemberAccessExpressionSyntax mapMemberAccess)
    {
        mapMemberAccess = null!;

        if (getOrElseMemberAccess.Expression is not InvocationExpressionSyntax mapInvocation)
            return false;

        if (mapInvocation.Expression is not MemberAccessExpressionSyntax ma)
            return false;

        if (ma.Name.Identifier.Text != "Map")
            return false;

        mapMemberAccess = ma;
        return true;
    }

    private static bool IsMonadType(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        var expressionType = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
        if (expressionType is null)
            return false;

        var typeName = GetBaseTypeName(expressionType);
        return MonadTypes.Contains(typeName);
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;
}
