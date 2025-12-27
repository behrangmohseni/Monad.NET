using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CombineMapCodeFixProvider)), Shared]
public sealed class CombineMapCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.RedundantMapChain.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var outerMapInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (outerMapInvocation is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Combine Map calls",
                createChangedDocument: c => CombineMapCallsAsync(context.Document, outerMapInvocation, c),
                equivalenceKey: "CombineMapCalls"),
            diagnostic);
    }

    private static async Task<Document> CombineMapCallsAsync(Document document, InvocationExpressionSyntax outerMapInvocation, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        if (outerMapInvocation.Expression is not MemberAccessExpressionSyntax outerMemberAccess) return document;
        if (outerMemberAccess.Expression is not InvocationExpressionSyntax innerMapInvocation) return document;
        if (innerMapInvocation.Expression is not MemberAccessExpressionSyntax innerMemberAccess) return document;

        var outerLambda = GetLambdaExpression(outerMapInvocation);
        var innerLambda = GetLambdaExpression(innerMapInvocation);
        if (outerLambda is null || innerLambda is null) return document;

        var combinedLambda = CombineLambdas(innerLambda, outerLambda);
        if (combinedLambda is null) return document;

        var newInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, innerMemberAccess.Expression, SyntaxFactory.IdentifierName("Map")),
            SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(combinedLambda))));

        var newRoot = root.ReplaceNode(outerMapInvocation, newInvocation.WithTriviaFrom(outerMapInvocation));
        return document.WithSyntaxRoot(newRoot);
    }

    private static LambdaExpressionSyntax? GetLambdaExpression(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments.Count == 1 ? invocation.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax : null;

    private static LambdaExpressionSyntax? CombineLambdas(LambdaExpressionSyntax innerLambda, LambdaExpressionSyntax outerLambda)
    {
        var innerParam = GetParameterName(innerLambda);
        var innerBody = innerLambda.ExpressionBody;
        var outerParam = GetParameterName(outerLambda);
        var outerBody = outerLambda.ExpressionBody;

        if (innerParam is null || innerBody is null || outerParam is null || outerBody is null) return null;

        var combinedBody = ReplaceIdentifier(outerBody, outerParam, innerBody);
        return SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier(innerParam)), combinedBody);
    }

    private static string? GetParameterName(LambdaExpressionSyntax lambda) => lambda switch
    {
        SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
        ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } paren => paren.ParameterList.Parameters[0].Identifier.Text,
        _ => null
    };

    private static ExpressionSyntax ReplaceIdentifier(ExpressionSyntax expression, string identifierToReplace, ExpressionSyntax replacement)
    {
        return (ExpressionSyntax)new IdentifierReplacer(identifierToReplace, replacement).Visit(expression);
    }

    private sealed class IdentifierReplacer(string identifierToReplace, ExpressionSyntax replacement) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node) =>
            node.Identifier.Text == identifierToReplace ? SyntaxFactory.ParenthesizedExpression(replacement) : base.VisitIdentifierName(node);
    }
}
