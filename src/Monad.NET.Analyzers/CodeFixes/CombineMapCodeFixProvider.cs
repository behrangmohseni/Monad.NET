using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that combines consecutive Map calls into a single Map call.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CombineMapCodeFixProvider)), Shared]
public sealed class CombineMapCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.RedundantMapChain.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var outerMapInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (outerMapInvocation is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Combine Map calls",
                createChangedDocument: c => CombineMapCallsAsync(context.Document, outerMapInvocation, c),
                equivalenceKey: "CombineMapCalls"),
            diagnostic);
    }

    private static async Task<Document> CombineMapCallsAsync(
        Document document,
        InvocationExpressionSyntax outerMapInvocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        if (!TryExtractMapChainInfo(outerMapInvocation, out var innerExpression, out var outerLambda, out var innerLambda))
            return document;

        var combinedLambda = CombineLambdas(innerLambda, outerLambda);
        if (combinedLambda is null)
            return document;

        var newInvocation = CreateCombinedMapInvocation(innerExpression, combinedLambda);
        var newRoot = root.ReplaceNode(outerMapInvocation, newInvocation.WithTriviaFrom(outerMapInvocation));

        return document.WithSyntaxRoot(newRoot);
    }

    private static bool TryExtractMapChainInfo(
        InvocationExpressionSyntax outerMapInvocation,
        out ExpressionSyntax innerExpression,
        out LambdaExpressionSyntax outerLambda,
        out LambdaExpressionSyntax innerLambda)
    {
        innerExpression = null!;
        outerLambda = null!;
        innerLambda = null!;

        // Get the outer Map's member access: x.Map(...).Map(...)
        if (outerMapInvocation.Expression is not MemberAccessExpressionSyntax outerMemberAccess)
            return false;

        // Get the inner Map invocation: x.Map(...)
        if (outerMemberAccess.Expression is not InvocationExpressionSyntax innerMapInvocation)
            return false;

        // Get the inner Map's member access: x.Map
        if (innerMapInvocation.Expression is not MemberAccessExpressionSyntax innerMemberAccess)
            return false;

        var outer = GetLambdaExpression(outerMapInvocation);
        var inner = GetLambdaExpression(innerMapInvocation);

        if (outer is null || inner is null)
            return false;

        innerExpression = innerMemberAccess.Expression;
        outerLambda = outer;
        innerLambda = inner;
        return true;
    }

    private static InvocationExpressionSyntax CreateCombinedMapInvocation(
        ExpressionSyntax baseExpression,
        LambdaExpressionSyntax combinedLambda)
    {
        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                baseExpression,
                SyntaxFactory.IdentifierName("Map")),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(combinedLambda))));
    }

    private static LambdaExpressionSyntax? GetLambdaExpression(InvocationExpressionSyntax invocation)
    {
        if (invocation.ArgumentList.Arguments.Count != 1)
            return null;

        return invocation.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
    }

    private static LambdaExpressionSyntax? CombineLambdas(
        LambdaExpressionSyntax innerLambda,
        LambdaExpressionSyntax outerLambda)
    {
        var innerParam = GetParameterName(innerLambda);
        var innerBody = innerLambda.ExpressionBody;
        var outerParam = GetParameterName(outerLambda);
        var outerBody = outerLambda.ExpressionBody;

        if (innerParam is null || innerBody is null || outerParam is null || outerBody is null)
            return null;

        var combinedBody = ReplaceIdentifier(outerBody, outerParam, innerBody);

        return SyntaxFactory.SimpleLambdaExpression(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(innerParam)),
            combinedBody);
    }

    private static string? GetParameterName(LambdaExpressionSyntax lambda) => lambda switch
    {
        SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
        ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } paren =>
            paren.ParameterList.Parameters[0].Identifier.Text,
        _ => null
    };

    private static ExpressionSyntax ReplaceIdentifier(
        ExpressionSyntax expression,
        string identifierToReplace,
        ExpressionSyntax replacement)
    {
        return (ExpressionSyntax)new IdentifierReplacer(identifierToReplace, replacement).Visit(expression);
    }

    private sealed class IdentifierReplacer : CSharpSyntaxRewriter
    {
        private readonly string _identifierToReplace;
        private readonly ExpressionSyntax _replacement;

        public IdentifierReplacer(string identifierToReplace, ExpressionSyntax replacement)
        {
            _identifierToReplace = identifierToReplace;
            _replacement = replacement;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.Text == _identifierToReplace)
            {
                return SyntaxFactory.ParenthesizedExpression(_replacement);
            }

            return base.VisitIdentifierName(node);
        }
    }
}
