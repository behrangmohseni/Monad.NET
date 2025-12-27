using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowInMatchAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowInMatch);

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

        if (memberAccess.Name.Identifier.Text != "Match")
            return;

        // Check each argument for throw expressions
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            var branchName = GetBranchName(argument);
            if (branchName is null)
                continue;

            if (ContainsThrow(argument.Expression))
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ThrowInMatch, argument.GetLocation(), branchName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static string? GetBranchName(ArgumentSyntax argument)
    {
        // Check named argument
        if (argument.NameColon is not null)
        {
            var name = argument.NameColon.Name.Identifier.Text;
            return name switch
            {
                "none" or "noneFunc" or "noneAction" => "None",
                "err" or "errFunc" or "errAction" => "Err",
                "left" or "leftFunc" or "leftAction" => "Left",
                "failure" or "failureFunc" or "failureAction" => "Failure",
                _ => null
            };
        }
        return null;
    }

    private static bool ContainsThrow(ExpressionSyntax expression)
    {
        // Lambda that throws
        ExpressionSyntax? body = expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple.ExpressionBody ?? GetBlockBody(simple.Block),
            ParenthesizedLambdaExpressionSyntax paren => paren.ExpressionBody ?? GetBlockBody(paren.Block),
            _ => null
        };

        if (body is ThrowExpressionSyntax)
            return true;

        // Check for throw statement in block
        if (expression is SimpleLambdaExpressionSyntax { Block: not null } simpleLambda)
            return ContainsThrowStatement(simpleLambda.Block);
            
        if (expression is ParenthesizedLambdaExpressionSyntax { Block: not null } parenLambda)
            return ContainsThrowStatement(parenLambda.Block);

        return false;
    }

    private static ExpressionSyntax? GetBlockBody(BlockSyntax? block)
    {
        if (block is null || block.Statements.Count != 1)
            return null;

        return block.Statements[0] switch
        {
            ThrowStatementSyntax => null, // Will be caught by ContainsThrowStatement
            ReturnStatementSyntax ret => ret.Expression,
            ExpressionStatementSyntax expr => expr.Expression,
            _ => null
        };
    }

    private static bool ContainsThrowStatement(BlockSyntax block)
    {
        return block.Statements.Any(s => s is ThrowStatementSyntax);
    }
}
