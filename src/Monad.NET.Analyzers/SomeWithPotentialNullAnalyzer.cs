using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that detects Option.Some() calls that might receive null values.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SomeWithPotentialNullAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.SomeWithPotentialNull);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a call to Option.Some() or Option<T>.Some()
        if (!IsSomeCall(invocation, context.SemanticModel))
        {
            return;
        }

        // Get the argument
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        var argument = invocation.ArgumentList.Arguments[0].Expression;

        // Check if the argument might be null
        if (MightBeNull(argument, context.SemanticModel))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.SomeWithPotentialNull,
                invocation.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsSomeCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        // Match patterns like:
        // - Option<T>.Some(value)
        // - Option.Some(value)

        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Name.Identifier.Text != "Some")
            {
                return false;
            }

            // Check if the type is Option or Option<T>
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                var containingTypeName = methodSymbol.ContainingType?.Name;
                if (containingTypeName == "Option")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool MightBeNull(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        return IsNullLiteral(expression)
            || IsNullableDefault(expression, semanticModel)
            || HasMaybeNullFlowState(expression, semanticModel)
            || IsNullableReferenceType(expression, semanticModel)
            || IsNullableMethodReturn(expression, semanticModel)
            || IsNullableIdentifier(expression, semanticModel)
            || IsNullableMemberAccess(expression, semanticModel)
            || IsConditionalAccess(expression)
            || IsNullableCoalesce(expression, semanticModel)
            || IsAsExpression(expression)
            || IsNullableCast(expression, semanticModel);
    }

    private static bool IsNullLiteral(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal)
        {
            // Null literal is always potentially null
            if (literal.IsKind(SyntaxKind.NullLiteralExpression))
                return true;

            // Non-null literals are safe
            return false;
        }
        return false;
    }

    private static bool IsNullableDefault(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Check for default expressions - default(string), default, etc.
        if (expression is DefaultExpressionSyntax)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            return typeInfo.Type is { IsReferenceType: true };
        }

        if (expression is LiteralExpressionSyntax defaultLiteral &&
            defaultLiteral.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            return typeInfo.Type is { IsReferenceType: true };
        }

        return false;
    }

    private static bool HasMaybeNullFlowState(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var exprTypeInfo = semanticModel.GetTypeInfo(expression);
        return exprTypeInfo.Nullability.FlowState == NullableFlowState.MaybeNull;
    }

    private static bool IsNullableReferenceType(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var exprTypeInfo = semanticModel.GetTypeInfo(expression);
        return exprTypeInfo.Type is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated };
    }

    private static bool IsNullableMethodReturn(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression is not InvocationExpressionSyntax invocation)
            return false;

        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return false;

        // Check if return type is nullable
        if (methodSymbol.ReturnType.NullableAnnotation == NullableAnnotation.Annotated)
            return true;

        // Check for methods known to return null (like FirstOrDefault, etc.)
        var methodName = methodSymbol.Name;
        return methodName.EndsWith("OrDefault", StringComparison.Ordinal) ||
               methodName.EndsWith("OrNull", StringComparison.Ordinal);
    }

    private static bool IsNullableIdentifier(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression is not IdentifierNameSyntax identifier)
            return false;

        var symbolInfo = semanticModel.GetSymbolInfo(identifier);
        return symbolInfo.Symbol switch
        {
            ILocalSymbol local => local.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IParameterSymbol param => param.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IFieldSymbol field => field.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IPropertySymbol prop => prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
            _ => false
        };
    }

    private static bool IsNullableMemberAccess(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
        return symbolInfo.Symbol switch
        {
            IPropertySymbol prop => prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IFieldSymbol field => field.Type.NullableAnnotation == NullableAnnotation.Annotated,
            _ => false
        };
    }

    private static bool IsConditionalAccess(ExpressionSyntax expression)
    {
        // Conditional access (?.) implies nullable
        return expression is ConditionalAccessExpressionSyntax;
    }

    private static bool IsNullableCoalesce(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Check for null-coalescing (??) - the result might still be null if the right side is null
        if (expression is BinaryExpressionSyntax binary &&
            binary.IsKind(SyntaxKind.CoalesceExpression))
        {
            return MightBeNull(binary.Right, semanticModel);
        }
        return false;
    }

    private static bool IsAsExpression(ExpressionSyntax expression)
    {
        // as-expression is always nullable
        return expression is BinaryExpressionSyntax asBinary &&
               asBinary.IsKind(SyntaxKind.AsExpression);
    }

    private static bool IsNullableCast(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression is not CastExpressionSyntax cast)
            return false;

        var castTypeInfo = semanticModel.GetTypeInfo(cast.Type);
        return castTypeInfo.Type?.NullableAnnotation == NullableAnnotation.Annotated;
    }
}
