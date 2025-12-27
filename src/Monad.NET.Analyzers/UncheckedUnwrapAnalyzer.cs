using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UncheckedUnwrapAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Either", "Try", "Validation");

    private static readonly ImmutableHashSet<string> UnwrapMethods = ImmutableHashSet.Create(
        "Unwrap", "UnwrapOr", "UnwrapOrDefault", "UnwrapOrThrow");

    private static readonly ImmutableHashSet<string> CheckProperties = ImmutableHashSet.Create(
        "IsSome", "IsNone", "IsSuccess", "IsOk", "IsErr", "IsError",
        "IsLeft", "IsRight", "IsValid", "IsInvalid", "IsFailure");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.UncheckedUnwrap);

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
        if (!UnwrapMethods.Contains(methodName))
            return;

        var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (expressionType is null)
            return;

        var typeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(typeName))
            return;

        if (IsGuardedByCheck(invocation, memberAccess.Expression, context))
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.UncheckedUnwrap, invocation.GetLocation(), typeName);
        context.ReportDiagnostic(diagnostic);
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;

    private static bool IsGuardedByCheck(InvocationExpressionSyntax unwrapCall, ExpressionSyntax monadExpression, SyntaxNodeAnalysisContext context)
    {
        var containingStatement = unwrapCall.FirstAncestorOrSelf<StatementSyntax>();
        if (containingStatement is null)
            return false;

        var ifStatement = containingStatement.FirstAncestorOrSelf<IfStatementSyntax>();
        if (ifStatement is not null && IsConditionCheckingMonad(ifStatement.Condition, monadExpression, context))
            return true;

        var conditional = unwrapCall.FirstAncestorOrSelf<ConditionalExpressionSyntax>();
        if (conditional is not null && IsConditionCheckingMonad(conditional.Condition, monadExpression, context))
            return true;

        var binaryExpr = unwrapCall.FirstAncestorOrSelf<BinaryExpressionSyntax>();
        while (binaryExpr is not null)
        {
            if (binaryExpr.IsKind(SyntaxKind.LogicalAndExpression) && 
                IsConditionCheckingMonad(binaryExpr.Left, monadExpression, context))
                return true;
            binaryExpr = binaryExpr.Parent as BinaryExpressionSyntax;
        }

        var block = containingStatement.FirstAncestorOrSelf<BlockSyntax>();
        if (block is not null)
        {
            var statementIndex = block.Statements.IndexOf(containingStatement);
            for (var i = 0; i < statementIndex; i++)
            {
                if (block.Statements[i] is IfStatementSyntax priorIf &&
                    IsConditionCheckingMonad(priorIf.Condition, monadExpression, context) &&
                    HasEarlyReturn(priorIf.Statement))
                    return true;
            }
        }

        return false;
    }

    private static bool IsConditionCheckingMonad(ExpressionSyntax condition, ExpressionSyntax monadExpression, SyntaxNodeAnalysisContext context)
    {
        var monadText = monadExpression.ToString();
        if (condition is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } prefixUnary)
            condition = prefixUnary.Operand;

        if (condition is MemberAccessExpressionSyntax memberAccess)
        {
            var propertyName = memberAccess.Name.Identifier.Text;
            if (CheckProperties.Contains(propertyName) && memberAccess.Expression.ToString() == monadText)
                return true;
        }

        if (condition is BinaryExpressionSyntax binary)
            return IsConditionCheckingMonad(binary.Left, monadExpression, context) ||
                   IsConditionCheckingMonad(binary.Right, monadExpression, context);

        if (condition is IsPatternExpressionSyntax isPattern && isPattern.Expression.ToString() == monadText)
            return true;

        return false;
    }

    private static bool HasEarlyReturn(StatementSyntax statement)
    {
        if (statement is BlockSyntax block)
            return block.Statements.Any(HasEarlyReturn);

        return statement is ReturnStatementSyntax or ThrowStatementSyntax or BreakStatementSyntax or ContinueStatementSyntax;
    }
}
