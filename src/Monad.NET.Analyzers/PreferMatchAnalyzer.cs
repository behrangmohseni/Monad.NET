using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Analyzer that suggests using Match instead of manual IsSome/IsOk checks followed by Unwrap.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferMatchAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableDictionary<string, string> StateCheckProperties = new Dictionary<string, string>
    {
        { "IsSome", "Option" },
        { "IsNone", "Option" },
        { "IsOk", "Result" },
        { "IsErr", "Result" },
        { "IsSuccess", "Try" },
        { "IsFailure", "Try" },
        { "IsValid", "Validation" },
        { "IsInvalid", "Validation" },
        { "IsRight", "Either" },
        { "IsLeft", "Either" }
    }.ToImmutableDictionary();

    private static readonly ImmutableHashSet<string> UnwrapMethods = ImmutableHashSet.Create(
        "Unwrap", "Get", "UnwrapErr", "UnwrapErrors", "GetException",
        "UnwrapLeft", "UnwrapRight");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.PreferMatch);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        // Check if the condition is a state check property (e.g., option.IsSome)
        var stateCheckInfo = GetStateCheckInfo(ifStatement.Condition, context.SemanticModel, context.CancellationToken);
        if (stateCheckInfo == null)
            return;

        var (variableName, propertyName) = stateCheckInfo.Value;

        // Check if the if body contains an Unwrap call on the same variable
        if (!ContainsUnwrapOnVariable(ifStatement.Statement, variableName, context.SemanticModel, context.CancellationToken))
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.PreferMatch,
            ifStatement.Condition.GetLocation(),
            propertyName);

        context.ReportDiagnostic(diagnostic);
    }

    private static (string variableName, string propertyName)? GetStateCheckInfo(
        ExpressionSyntax condition,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Handle direct property access: option.IsSome
        if (condition is MemberAccessExpressionSyntax memberAccess)
        {
            var propertyName = memberAccess.Name.Identifier.Text;
            if (StateCheckProperties.ContainsKey(propertyName))
            {
                var variableName = GetVariableName(memberAccess.Expression);
                if (variableName != null)
                    return (variableName, propertyName);
            }
        }

        // Handle negation: !option.IsNone
        if (condition is PrefixUnaryExpressionSyntax { OperatorToken.RawKind: (int)SyntaxKind.ExclamationToken } unary)
        {
            if (unary.Operand is MemberAccessExpressionSyntax negatedMemberAccess)
            {
                var propertyName = negatedMemberAccess.Name.Identifier.Text;
                if (StateCheckProperties.ContainsKey(propertyName))
                {
                    var variableName = GetVariableName(negatedMemberAccess.Expression);
                    if (variableName != null)
                        return (variableName, "!" + propertyName);
                }
            }
        }

        return null;
    }

    private static string? GetVariableName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.ToString(),
            _ => null
        };
    }

    private static bool ContainsUnwrapOnVariable(
        StatementSyntax statement,
        string variableName,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var methodName = memberAccess.Name.Identifier.Text;
                if (!UnwrapMethods.Contains(methodName))
                    continue;

                var targetVariable = GetVariableName(memberAccess.Expression);
                if (targetVariable == variableName)
                    return true;
            }
        }

        return false;
    }
}

