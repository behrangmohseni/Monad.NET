using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that replaces Option null comparisons with IsSome/IsNone.
/// Example: option == null becomes option.IsNone
/// Example: option != null becomes option.IsSome
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionNullComparisonCodeFixProvider)), Shared]
public sealed class OptionNullComparisonCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.OptionNullComparison.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var binaryExpr = node.FirstAncestorOrSelf<BinaryExpressionSyntax>();

        if (binaryExpr is null)
            return;

        var (optionExpr, propertyName) = GetFixInfo(binaryExpr);
        if (optionExpr is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use {propertyName} instead",
                createChangedDocument: c => UseIsPropertyAsync(context.Document, binaryExpr, optionExpr, propertyName, c),
                equivalenceKey: $"UseIs{propertyName}"),
            diagnostic);
    }

    private static (ExpressionSyntax? optionExpr, string propertyName) GetFixInfo(BinaryExpressionSyntax binaryExpr)
    {
        var isEquality = binaryExpr.IsKind(SyntaxKind.EqualsExpression);
        var isInequality = binaryExpr.IsKind(SyntaxKind.NotEqualsExpression);

        if (!isEquality && !isInequality)
            return (null, string.Empty);

        // Find which side is the Option and which is null
        ExpressionSyntax? optionExpr = null;
        if (IsNullLiteral(binaryExpr.Right))
            optionExpr = binaryExpr.Left;
        else if (IsNullLiteral(binaryExpr.Left))
            optionExpr = binaryExpr.Right;

        if (optionExpr is null)
            return (null, string.Empty);

        // == null means IsNone, != null means IsSome
        var propertyName = isEquality ? "IsNone" : "IsSome";

        return (optionExpr, propertyName);
    }

    private static bool IsNullLiteral(ExpressionSyntax expr)
    {
        return expr.IsKind(SyntaxKind.NullLiteralExpression);
    }

    private static async Task<Document> UseIsPropertyAsync(
        Document document,
        BinaryExpressionSyntax binaryExpr,
        ExpressionSyntax optionExpr,
        string propertyName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Create: option.IsSome or option.IsNone
        var memberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            optionExpr,
            SyntaxFactory.IdentifierName(propertyName));

        var newRoot = root.ReplaceNode(binaryExpr, memberAccess.WithTriviaFrom(binaryExpr));
        return document.WithSyntaxRoot(newRoot);
    }
}
