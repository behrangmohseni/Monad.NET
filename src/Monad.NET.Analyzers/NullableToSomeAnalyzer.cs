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

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check for Option.Some(...) or Option<T>.Some(...)
        var methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            _ => null
        };

        if (methodName != "Some")
            return;

        // Verify it's on Option type
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
            if (receiverType is not INamedTypeSymbol namedType || namedType.Name != "Option")
                return;
        }

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = invocation.ArgumentList.Arguments[0].Expression;
        var argumentType = context.SemanticModel.GetTypeInfo(argument, context.CancellationToken).Type;

        if (argumentType is null)
            return;

        // Check if the argument could be null
        var isNullable = argumentType.NullableAnnotation == NullableAnnotation.Annotated ||
                         argumentType.IsReferenceType ||
                         IsNullableValueType(argumentType);

        // Skip if it's a literal that's clearly not null
        if (argument is LiteralExpressionSyntax literal && !literal.IsKind(SyntaxKind.NullLiteralExpression))
            return;

        // Skip if it's a creation expression
        if (argument is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax)
            return;

        if (isNullable && !IsDefinitelyNotNull(argument, context))
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.NullableToSome, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullableValueType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol { IsGenericType: true } namedType &&
               namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
    }

    private static bool IsDefinitelyNotNull(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        // Check for some patterns that are definitely not null
        return expression switch
        {
            LiteralExpressionSyntax { RawKind: not (int)SyntaxKind.NullLiteralExpression } => true,
            ObjectCreationExpressionSyntax => true,
            ImplicitObjectCreationExpressionSyntax => true,
            ThisExpressionSyntax => true,
            TypeOfExpressionSyntax => true,
            DefaultExpressionSyntax def => IsValueType(context.SemanticModel.GetTypeInfo(def, context.CancellationToken).Type),
            _ => false
        };
    }

    private static bool IsValueType(ITypeSymbol? type)
    {
        return type?.IsValueType == true && !IsNullableValueType(type);
    }
}
