using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddGuardCodeFixProvider)), Shared]
public sealed class AddGuardCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.UncheckedUnwrap.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use GetOrElse instead of Unwrap",
                createChangedDocument: c => UseGetOrElseAsync(context.Document, invocation, c),
                equivalenceKey: "UseGetOrElse"),
            diagnostic);
    }

    private static async Task<Document> UseGetOrElseAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return document;

        var getOrElseInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, memberAccess.Expression, SyntaxFactory.IdentifierName("GetOrElse")),
            SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)))));

        var newRoot = root.ReplaceNode(invocation, getOrElseInvocation.WithTriviaFrom(invocation));
        return document.WithSyntaxRoot(newRoot);
    }
}
