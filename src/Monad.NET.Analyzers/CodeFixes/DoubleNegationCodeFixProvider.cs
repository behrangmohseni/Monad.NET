using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that simplifies double negation patterns.
/// Example: !option.IsNone becomes option.IsSome
/// Example: !result.IsError becomes result.IsOk
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DoubleNegationCodeFixProvider)), Shared]
public sealed class DoubleNegationCodeFixProvider : CodeFixProvider
{
    private static readonly Dictionary<string, string> NegationMap = new()
    {
        ["IsNone"] = "IsSome",
        ["IsSome"] = "IsNone",
        ["IsError"] = "IsOk",
        ["IsOk"] = "IsError",
        ["IsInvalid"] = "IsValid",
        ["IsValid"] = "IsInvalid"
    };

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.DoubleNegation.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var prefixUnary = node.FirstAncestorOrSelf<PrefixUnaryExpressionSyntax>();

        if (prefixUnary is null || !prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
            return;

        var memberAccess = prefixUnary.Operand as MemberAccessExpressionSyntax;
        if (memberAccess is null)
            return;

        var propertyName = memberAccess.Name.Identifier.Text;
        if (!NegationMap.TryGetValue(propertyName, out var simplifiedProperty))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Simplify to {simplifiedProperty}",
                createChangedDocument: c => SimplifyDoubleNegationAsync(context.Document, prefixUnary, memberAccess, simplifiedProperty, c),
                equivalenceKey: $"SimplifyTo{simplifiedProperty}"),
            diagnostic);
    }

    private static async Task<Document> SimplifyDoubleNegationAsync(
        Document document,
        PrefixUnaryExpressionSyntax prefixUnary,
        MemberAccessExpressionSyntax memberAccess,
        string simplifiedProperty,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Create: monad.SimplifiedProperty
        var newMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            memberAccess.Expression,
            SyntaxFactory.IdentifierName(simplifiedProperty));

        var newRoot = root.ReplaceNode(prefixUnary, newMemberAccess.WithTriviaFrom(prefixUnary));
        return document.WithSyntaxRoot(newRoot);
    }
}
