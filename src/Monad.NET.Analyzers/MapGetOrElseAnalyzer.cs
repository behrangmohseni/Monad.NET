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
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name.Identifier.Text;
        if (!GetOrElseMethods.Contains(methodName))
            return;

        // Check if the receiver is a Map call
        if (memberAccess.Expression is not InvocationExpressionSyntax mapInvocation)
            return;

        if (mapInvocation.Expression is not MemberAccessExpressionSyntax mapMemberAccess)
            return;

        if (mapMemberAccess.Name.Identifier.Text != "Map")
            return;

        // Verify this is on a monad type
        var expressionType = context.SemanticModel.GetTypeInfo(mapMemberAccess.Expression, context.CancellationToken).Type;
        if (expressionType is null)
            return;

        var typeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(typeName))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.MapGetOrElseToMatch, invocation.GetLocation(), methodName);
        context.ReportDiagnostic(diagnostic);
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;
}
