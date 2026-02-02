using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monad.NET.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider that replaces Bind returning Some/Ok with Map.
/// Example: .Bind(x => Option.Some(x + 1)) becomes .Map(x => x + 1)
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BindToMapCodeFixProvider)), Shared]
public sealed class BindToMapCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.BindToMap.Id);

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
                title: "Convert Bind to Map",
                createChangedDocument: c => ConvertBindToMapAsync(context.Document, invocation, c),
                equivalenceKey: "BindToMap"),
            diagnostic);
    }

    private static async Task<Document> ConvertBindToMapAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Get the Bind's member access expression
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return document;

        // Get the lambda argument
        if (invocation.ArgumentList.Arguments.Count != 1)
            return document;

        var lambdaArg = invocation.ArgumentList.Arguments[0].Expression;
        if (lambdaArg is not LambdaExpressionSyntax lambda)
            return document;

        // Extract the inner expression from Some(...) or Ok(...)
        var innerExpression = ExtractInnerExpression(lambda);
        if (innerExpression is null)
            return document;

        // Create the new lambda with the inner expression
        var newLambda = CreateSimplifiedLambda(lambda, innerExpression);
        if (newLambda is null)
            return document;

        // Create the new Map invocation
        var newInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccess.Expression,
                SyntaxFactory.IdentifierName("Map")),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(newLambda))));

        var newRoot = root.ReplaceNode(invocation, newInvocation.WithTriviaFrom(invocation));
        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax? ExtractInnerExpression(LambdaExpressionSyntax lambda)
    {
        var body = lambda.ExpressionBody;
        if (body is null)
            return null;

        // Handle Option.Some(expr) or Result.Ok(expr)
        if (body is InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var methodName = memberAccess.Name.Identifier.Text;
                if ((methodName == "Some" || methodName == "Ok") &&
                    invocation.ArgumentList.Arguments.Count == 1)
                {
                    return invocation.ArgumentList.Arguments[0].Expression;
                }
            }
        }

        return null;
    }

    private static LambdaExpressionSyntax? CreateSimplifiedLambda(
        LambdaExpressionSyntax originalLambda,
        ExpressionSyntax newBody)
    {
        return originalLambda switch
        {
            SimpleLambdaExpressionSyntax simple =>
                SyntaxFactory.SimpleLambdaExpression(simple.Parameter, newBody),
            ParenthesizedLambdaExpressionSyntax paren =>
                SyntaxFactory.ParenthesizedLambdaExpression(paren.ParameterList, newBody),
            _ => null
        };
    }
}
