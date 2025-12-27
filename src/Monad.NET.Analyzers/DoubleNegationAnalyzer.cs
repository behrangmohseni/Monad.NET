using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoubleNegationAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableDictionary<string, string> NegativeToPositive = new Dictionary<string, string>
    {
        ["IsNone"] = "IsSome",
        ["IsErr"] = "IsOk",
        ["IsError"] = "IsSuccess",
        ["IsLeft"] = "IsRight",
        ["IsInvalid"] = "IsValid",
        ["IsFailure"] = "IsSuccess"
    }.ToImmutableDictionary();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.DoubleNegation);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeLogicalNot, SyntaxKind.LogicalNotExpression);
    }

    private static void AnalyzeLogicalNot(SyntaxNodeAnalysisContext context)
    {
        var notExpression = (PrefixUnaryExpressionSyntax)context.Node;

        // Check for !option.IsNone pattern
        if (notExpression.Operand is not MemberAccessExpressionSyntax memberAccess)
            return;

        var propertyName = memberAccess.Name.Identifier.Text;
        if (!NegativeToPositive.ContainsKey(propertyName))
            return;

        var monadName = memberAccess.Expression.ToString();
        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoubleNegation, notExpression.GetLocation(), monadName, monadName);
        context.ReportDiagnostic(diagnostic);
    }
}
