using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that replaces Option.Some(x) with x.ToOption() for nullable values.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseToOptionCodeFixProvider)), Shared]
public sealed class UseToOptionCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            DiagnosticDescriptors.NullableToSome.Id,
            DiagnosticDescriptors.SomeWithPotentialNull.Id);

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
                title: "Use ToOption() instead",
                createChangedDocument: c => UseToOptionAsync(context.Document, invocation, c),
                equivalenceKey: "UseToOption"),
            diagnostic);
    }

    private static async Task<Document> UseToOptionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Get the argument passed to Some()
        if (invocation.ArgumentList.Arguments.Count != 1)
            return document;

        var argument = invocation.ArgumentList.Arguments[0].Expression;

        // Create: argument.ToOption()
        var toOptionInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ParenthesizeIfNeeded(argument),
                SyntaxFactory.IdentifierName("ToOption")),
            SyntaxFactory.ArgumentList());

        var newRoot = root.ReplaceNode(invocation, toOptionInvocation.WithTriviaFrom(invocation));
        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax ParenthesizeIfNeeded(ExpressionSyntax expression)
    {
        // Parenthesize complex expressions to maintain correct precedence
        return expression switch
        {
            ConditionalExpressionSyntax or
            BinaryExpressionSyntax or
            CastExpressionSyntax or
            AssignmentExpressionSyntax =>
                SyntaxFactory.ParenthesizedExpression(expression),
            _ => expression
        };
    }
}
