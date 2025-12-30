using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that warns when a Match call has an empty branch,
/// suggesting to use Tap/TapErr/TapNone instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EmptyMatchBranchAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> MonadTypes = ImmutableHashSet.Create(
        "Option", "Result", "Either", "Try", "Validation", "RemoteData");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.EmptyMatchBranch);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.Text != "Match")
            return;

        // Match typically has 2 arguments (the two branches)
        if (invocation.ArgumentList.Arguments.Count != 2)
            return;

        // Verify this is on a monad type
        var expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (expressionType is null)
            return;

        var typeName = GetBaseTypeName(expressionType);
        if (!MonadTypes.Contains(typeName))
            return;

        var arg0 = invocation.ArgumentList.Arguments[0].Expression;
        var arg1 = invocation.ArgumentList.Arguments[1].Expression;

        var (firstBranchEmpty, firstBranchName) = IsEmptyBranch(arg0, typeName, true);
        var (secondBranchEmpty, secondBranchName) = IsEmptyBranch(arg1, typeName, false);

        if (firstBranchEmpty)
        {
            var suggestedMethod = GetSuggestedTapMethod(typeName, false);
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EmptyMatchBranch,
                invocation.GetLocation(),
                firstBranchName,
                suggestedMethod);
            context.ReportDiagnostic(diagnostic);
        }
        else if (secondBranchEmpty)
        {
            var suggestedMethod = GetSuggestedTapMethod(typeName, true);
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EmptyMatchBranch,
                invocation.GetLocation(),
                secondBranchName,
                suggestedMethod);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static (bool isEmpty, string branchName) IsEmptyBranch(ExpressionSyntax expression, string typeName, bool isFirstBranch)
    {
        var branchName = GetBranchName(typeName, isFirstBranch);

        // Check for _ => { } pattern (empty block lambda)
        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
        {
            if (IsEmptyBody(simpleLambda.ExpressionBody, simpleLambda.Block))
                return (true, branchName);
        }

        // Check for (_) => { } pattern
        if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            if (IsEmptyBody(parenLambda.ExpressionBody, parenLambda.Body as BlockSyntax))
                return (true, branchName);
        }

        // Check for () => { } pattern (Action delegate with no params)
        if (expression is ParenthesizedLambdaExpressionSyntax actionLambda &&
            actionLambda.ParameterList.Parameters.Count == 0)
        {
            if (IsEmptyBody(actionLambda.ExpressionBody, actionLambda.Body as BlockSyntax))
                return (true, branchName);
        }

        return (false, branchName);
    }

    private static bool IsEmptyBody(ExpressionSyntax? expressionBody, BlockSyntax? blockBody)
    {
        // Empty block: { }
        if (blockBody is { Statements.Count: 0 })
            return true;

        // Default literal: () => default or _ => default
        if (expressionBody is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression })
            return true;

        // Default expression: () => default(T)
        if (expressionBody is DefaultExpressionSyntax)
            return true;

        // Unit.Value: _ => Unit.Value (common pattern for void-like operations)
        if (expressionBody is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "Value" &&
            memberAccess.Expression is IdentifierNameSyntax identifier &&
            identifier.Identifier.Text == "Unit")
            return true;

        return false;
    }

    private static string GetBranchName(string typeName, bool isFirstBranch)
    {
        return typeName switch
        {
            "Option" => isFirstBranch ? "some" : "none",
            "Result" => isFirstBranch ? "ok" : "err",
            "Try" => isFirstBranch ? "success" : "failure",
            "Validation" => isFirstBranch ? "valid" : "invalid",
            "Either" => isFirstBranch ? "left" : "right",
            "RemoteData" => isFirstBranch ? "success" : "other",
            _ => isFirstBranch ? "first" : "second"
        };
    }

    private static string GetSuggestedTapMethod(string typeName, bool isFirstBranch)
    {
        return typeName switch
        {
            "Option" => isFirstBranch ? "" : "None",
            "Result" => isFirstBranch ? "" : "Err",
            "Try" => isFirstBranch ? "" : "Failure",
            "Validation" => isFirstBranch ? "" : "Errors",
            _ => ""
        };
    }

    private static string GetBaseTypeName(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } namedType ? namedType.Name : type.Name;
}

