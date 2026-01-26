using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that detects Unwrap() calls on monads without prior IsSuccess/IsSome checks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UncheckedUnwrapAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option",
        "Result",
        "Try",
        "Validation"
    );

    private static readonly ImmutableHashSet<string> UnwrapMethods = ImmutableHashSet.Create(
        "Unwrap",
        "UnwrapOr",
        "UnwrapOrDefault",
        "UnwrapOrThrow"
    );

    private static readonly ImmutableHashSet<string> CheckProperties = ImmutableHashSet.Create(
        "IsSome",
        "IsNone",
        "IsSuccess",
        "IsOk",
        "IsErr",
        "IsError",
        "IsLeft",
        "IsRight",
        "IsValid",
        "IsInvalid",
        "IsFailure"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.UncheckedUnwrap);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!TryGetUnwrapMethodInfo(invocation, context, out var memberAccess, out var typeName))
            return;

        if (IsGuardedByCheck(invocation, memberAccess.Expression, context))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.UncheckedUnwrap,
            invocation.GetLocation(),
            typeName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool TryGetUnwrapMethodInfo(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        out MemberAccessExpressionSyntax memberAccess,
        out string typeName)
    {
        memberAccess = null!;
        typeName = null!;

        if (invocation.Expression is not MemberAccessExpressionSyntax ma)
            return false;

        var methodName = ma.Name.Identifier.Text;
        if (!UnwrapMethods.Contains(methodName))
            return false;

        var expressionType = context.SemanticModel.GetTypeInfo(ma.Expression, context.CancellationToken).Type;
        if (expressionType is null)
            return false;

        var baseTypeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(baseTypeName))
            return false;

        memberAccess = ma;
        typeName = baseTypeName;
        return true;
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;

    private static bool IsGuardedByCheck(
        InvocationExpressionSyntax unwrapCall,
        ExpressionSyntax monadExpression,
        SyntaxNodeAnalysisContext context)
    {
        return IsGuardedByIfStatement(unwrapCall, monadExpression, context) ||
               IsGuardedByTernary(unwrapCall, monadExpression, context) ||
               IsGuardedBySwitchExpression(unwrapCall) ||
               IsGuardedByLogicalAnd(unwrapCall, monadExpression, context) ||
               IsGuardedByPriorCheck(unwrapCall, monadExpression, context);
    }

    private static bool IsGuardedByIfStatement(
        InvocationExpressionSyntax unwrapCall,
        ExpressionSyntax monadExpression,
        SyntaxNodeAnalysisContext context)
    {
        var containingStatement = unwrapCall.FirstAncestorOrSelf<StatementSyntax>();
        if (containingStatement is null)
            return false;

        var ifStatement = containingStatement.FirstAncestorOrSelf<IfStatementSyntax>();
        return ifStatement is not null &&
               IsConditionCheckingMonad(ifStatement.Condition, monadExpression, context);
    }

    private static bool IsGuardedByTernary(
        InvocationExpressionSyntax unwrapCall,
        ExpressionSyntax monadExpression,
        SyntaxNodeAnalysisContext context)
    {
        var conditional = unwrapCall.FirstAncestorOrSelf<ConditionalExpressionSyntax>();
        return conditional is not null &&
               IsConditionCheckingMonad(conditional.Condition, monadExpression, context);
    }

    private static bool IsGuardedBySwitchExpression(InvocationExpressionSyntax unwrapCall)
    {
        // Switch expressions typically pattern match, which is a valid guard
        return unwrapCall.FirstAncestorOrSelf<SwitchExpressionSyntax>() is not null;
    }

    private static bool IsGuardedByLogicalAnd(
        InvocationExpressionSyntax unwrapCall,
        ExpressionSyntax monadExpression,
        SyntaxNodeAnalysisContext context)
    {
        var binaryExpr = unwrapCall.FirstAncestorOrSelf<BinaryExpressionSyntax>();
        while (binaryExpr is not null)
        {
            if (binaryExpr.IsKind(SyntaxKind.LogicalAndExpression) &&
                IsConditionCheckingMonad(binaryExpr.Left, monadExpression, context))
            {
                return true;
            }
            binaryExpr = binaryExpr.Parent as BinaryExpressionSyntax;
        }
        return false;
    }

    private static bool IsGuardedByPriorCheck(
        InvocationExpressionSyntax unwrapCall,
        ExpressionSyntax monadExpression,
        SyntaxNodeAnalysisContext context)
    {
        var containingStatement = unwrapCall.FirstAncestorOrSelf<StatementSyntax>();
        if (containingStatement is null)
            return false;

        var block = containingStatement.FirstAncestorOrSelf<BlockSyntax>();
        if (block is null)
            return false;

        var statementIndex = block.Statements.IndexOf(containingStatement);
        for (var i = 0; i < statementIndex; i++)
        {
            if (block.Statements[i] is IfStatementSyntax priorIf &&
                IsConditionCheckingMonad(priorIf.Condition, monadExpression, context) &&
                HasEarlyReturn(priorIf.Statement))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsConditionCheckingMonad(
        ExpressionSyntax condition,
        ExpressionSyntax monadExpression,
        SyntaxNodeAnalysisContext context)
    {
        var monadText = monadExpression.ToString();

        // Handle negation
        if (condition is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } prefixUnary)
        {
            condition = prefixUnary.Operand;
        }

        return condition switch
        {
            MemberAccessExpressionSyntax memberAccess =>
                CheckProperties.Contains(memberAccess.Name.Identifier.Text) &&
                memberAccess.Expression.ToString() == monadText,

            BinaryExpressionSyntax binary =>
                IsConditionCheckingMonad(binary.Left, monadExpression, context) ||
                IsConditionCheckingMonad(binary.Right, monadExpression, context),

            IsPatternExpressionSyntax isPattern =>
                isPattern.Expression.ToString() == monadText,

            _ => false
        };
    }

    private static bool HasEarlyReturn(StatementSyntax statement)
    {
        if (statement is BlockSyntax block)
        {
            foreach (var stmt in block.Statements)
            {
                if (HasEarlyReturn(stmt))
                    return true;
            }
        }

        return statement is ReturnStatementSyntax or
               ThrowStatementSyntax or
               BreakStatementSyntax or
               ContinueStatementSyntax;
    }
}
