using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that removes Map(x => x) identity calls.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveMapIdentityCodeFixProvider)), Shared]
public sealed class RemoveMapIdentityCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.MapIdentity.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (invocation is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove identity Map",
                createChangedDocument: c => RemoveIdentityMapAsync(context.Document, invocation, c),
                equivalenceKey: "RemoveIdentityMap"),
            diagnostic);
    }

    private static async Task<Document> RemoveIdentityMapAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Get the base expression (before .Map)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return document;

        var baseExpression = memberAccess.Expression;

        // Replace the entire Map invocation with just the base expression
        var newRoot = root.ReplaceNode(invocation, baseExpression.WithTriviaFrom(invocation));
        return document.WithSyntaxRoot(newRoot);
    }
}
