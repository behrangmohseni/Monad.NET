using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BindToMapAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> BindMethods = ImmutableHashSet.Create(
        "Bind", "SelectMany");

    private static readonly ImmutableHashSet<string> WrapperMethods = ImmutableHashSet.Create(
        "Some", "Ok", "Right", "Success", "Valid", "Return", "Pure");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.BindToMap);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeActionWithDefaults(AnalyzeInvocation, SyntaxKind.InvocationExpression);

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name.Identifier.Text;
        if (!BindMethods.Contains(methodName))
            return;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = invocation.ArgumentList.Arguments[0].Expression;

        // Check if the lambda body always wraps in Some/Ok/etc
        var wrapperMethod = GetWrapperMethodIfAlwaysWraps(argument);
        if (wrapperMethod is null)
            return;

        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.BindToMap, invocation.GetLocation(), wrapperMethod);
        context.ReportDiagnostic(diagnostic);
    }

    private static string? GetWrapperMethodIfAlwaysWraps(ExpressionSyntax expression)
    {
        // Handle lambda expressions: x => Option.Some(f(x))
        ExpressionSyntax? body = expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple.ExpressionBody,
            ParenthesizedLambdaExpressionSyntax paren => paren.ExpressionBody,
            _ => null
        };

        if (body is null)
            return null;

        // Check if body is a call to Some/Ok/Right/etc
        if (body is InvocationExpressionSyntax invocation)
        {
            var methodName = invocation.Expression switch
            {
                MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                IdentifierNameSyntax id => id.Identifier.Text,
                _ => null
            };

            if (methodName is not null && WrapperMethods.Contains(methodName))
                return methodName;
        }

        return null;
    }
}
