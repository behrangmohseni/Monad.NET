using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullableToSomeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.NullableToSome);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsOptionSomeInvocation(invocation, context))
            return;

        if (!TryGetSingleArgument(invocation, out var argument))
            return;

        if (IsDefinitelyNotNullArgument(argument, context))
            return;

        if (IsPotentiallyNullableArgument(argument, context))
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.NullableToSome, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsOptionSomeInvocation(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        if (memberAccess.Name.Identifier.Text != "Some")
            return false;

        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        return receiverType is INamedTypeSymbol { Name: "Option" };
    }

    private static bool TryGetSingleArgument(InvocationExpressionSyntax invocation, out ExpressionSyntax argument)
    {
        argument = null!;
        if (invocation.ArgumentList.Arguments.Count != 1)
            return false;

        argument = invocation.ArgumentList.Arguments[0].Expression;
        return true;
    }

    private static bool IsDefinitelyNotNullArgument(ExpressionSyntax argument, SyntaxNodeAnalysisContext context)
    {
        return argument switch
        {
            LiteralExpressionSyntax { RawKind: not (int)SyntaxKind.NullLiteralExpression } => true,
            ObjectCreationExpressionSyntax => true,
            ImplicitObjectCreationExpressionSyntax => true,
            ThisExpressionSyntax => true,
            TypeOfExpressionSyntax => true,
            DefaultExpressionSyntax def => IsNonNullableValueType(
                context.SemanticModel.GetTypeInfo(def, context.CancellationToken).Type),
            _ => false
        };
    }

    private static bool IsPotentiallyNullableArgument(ExpressionSyntax argument, SyntaxNodeAnalysisContext context)
    {
        var argumentType = context.SemanticModel.GetTypeInfo(argument, context.CancellationToken).Type;
        if (argumentType is null)
            return false;

        return argumentType.NullableAnnotation == NullableAnnotation.Annotated ||
               argumentType.IsReferenceType ||
               IsNullableValueType(argumentType);
    }

    private static bool IsNullableValueType(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType &&
        namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

    private static bool IsNonNullableValueType(ITypeSymbol? type) =>
        type?.IsValueType == true && !IsNullableValueType(type);
}
