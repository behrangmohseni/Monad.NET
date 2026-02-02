using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that converts Map followed by GetOrElse to Match.
/// Example: option.Map(x => x + 1).GetOrElse(0) becomes option.Match(x => x + 1, () => 0)
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MapGetOrElseToMatchCodeFixProvider)), Shared]
public sealed class MapGetOrElseToMatchCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.MapGetOrElseToMatch.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var getOrElseInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (getOrElseInvocation is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to Match",
                createChangedDocument: c => ConvertToMatchAsync(context.Document, getOrElseInvocation, c),
                equivalenceKey: "MapGetOrElseToMatch"),
            diagnostic);
    }

    private static async Task<Document> ConvertToMatchAsync(
        Document document,
        InvocationExpressionSyntax getOrElseInvocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Get GetOrElse's member access: something.GetOrElse(...)
        if (getOrElseInvocation.Expression is not MemberAccessExpressionSyntax getOrElseMemberAccess)
            return document;

        // The "something" should be the Map invocation: option.Map(f)
        if (getOrElseMemberAccess.Expression is not InvocationExpressionSyntax mapInvocation)
            return document;

        // Get Map's member access: option.Map
        if (mapInvocation.Expression is not MemberAccessExpressionSyntax mapMemberAccess)
            return document;

        // Get the base option expression
        var optionExpr = mapMemberAccess.Expression;

        // Get the Map function
        if (mapInvocation.ArgumentList.Arguments.Count != 1)
            return document;
        var mapFunction = mapInvocation.ArgumentList.Arguments[0].Expression;

        // Get the GetOrElse default value
        if (getOrElseInvocation.ArgumentList.Arguments.Count != 1)
            return document;
        var defaultValue = getOrElseInvocation.ArgumentList.Arguments[0].Expression;

        // Convert default value to a lambda if it's not already
        var noneFunction = ConvertToLambda(defaultValue);

        // Create: option.Match(mapFunction, noneFunction)
        var matchInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                optionExpr,
                SyntaxFactory.IdentifierName("Match")),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(new[]
                {
                    SyntaxFactory.Argument(mapFunction),
                    SyntaxFactory.Argument(noneFunction)
                })));

        var newRoot = root.ReplaceNode(getOrElseInvocation, matchInvocation.WithTriviaFrom(getOrElseInvocation));
        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax ConvertToLambda(ExpressionSyntax expression)
    {
        // If it's already a lambda, return as-is
        if (expression is LambdaExpressionSyntax)
            return expression;

        // Convert value to () => value
        return SyntaxFactory.ParenthesizedLambdaExpression(
            SyntaxFactory.ParameterList(),
            expression);
    }
}
