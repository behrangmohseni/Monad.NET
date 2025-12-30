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

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

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
        // Check for obvious null literals
        if (expression is LiteralExpressionSyntax literal)
        {
            if (literal.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return true;
            }
            // Non-null literals are safe
            return false;
        }

        // Check for default expressions - default(string), default, etc.
        if (expression is DefaultExpressionSyntax)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            if (typeInfo.Type != null && typeInfo.Type.IsReferenceType)
            {
                return true;
            }
        }

        if (expression is LiteralExpressionSyntax defaultLiteral &&
            defaultLiteral.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            if (typeInfo.Type != null && typeInfo.Type.IsReferenceType)
            {
                return true;
            }
        }

        // Check nullable annotations
        var exprTypeInfo = semanticModel.GetTypeInfo(expression);
        if (exprTypeInfo.Nullability.FlowState == NullableFlowState.MaybeNull)
        {
            return true;
        }

        // Check for nullable reference type annotations on the type itself
        if (exprTypeInfo.Type is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated })
        {
            return true;
        }

        // Check for method return values that might be null
        if (expression is InvocationExpressionSyntax invocation)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                // Check if return type is nullable
                if (methodSymbol.ReturnType.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }

                // Check for methods known to return null (like FirstOrDefault, etc.)
                var methodName = methodSymbol.Name;
                if (methodName.EndsWith("OrDefault", StringComparison.Ordinal) ||
                    methodName.EndsWith("OrNull", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        // Check for identifier references to nullable variables
        if (expression is IdentifierNameSyntax identifier)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            if (symbolInfo.Symbol is ILocalSymbol localSymbol)
            {
                if (localSymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }
            }
            else if (symbolInfo.Symbol is IParameterSymbol paramSymbol)
            {
                if (paramSymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }
            }
            else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
            {
                if (fieldSymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }
            }
            else if (symbolInfo.Symbol is IPropertySymbol propSymbol)
            {
                if (propSymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }
            }
        }

        // Check for member access that might return null
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
            if (symbolInfo.Symbol is IPropertySymbol propSymbol)
            {
                if (propSymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }
            }
            else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
            {
                if (fieldSymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return true;
                }
            }
        }

        // Check for conditional access (?.) which implies nullable
        if (expression is ConditionalAccessExpressionSyntax)
        {
            return true;
        }

        // Check for null-coalescing (??) right operand
        if (expression is BinaryExpressionSyntax binary &&
            binary.IsKind(SyntaxKind.CoalesceExpression))
        {
            // The result of ?? might still be null if the right side is null
            return MightBeNull(binary.Right, semanticModel);
        }

        // Check for as-expression (always nullable)
        if (expression is BinaryExpressionSyntax asBinary &&
            asBinary.IsKind(SyntaxKind.AsExpression))
        {
            return true;
        }

        // Check for cast to nullable type
        if (expression is CastExpressionSyntax cast)
        {
            var castTypeInfo = semanticModel.GetTypeInfo(cast.Type);
            if (castTypeInfo.Type?.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return true;
            }
        }

        return false;
    }
}

