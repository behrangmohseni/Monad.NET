using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Monad.NET.Analyzers;

/// <summary>
/// Extension methods for <see cref="AnalysisContext"/> to reduce boilerplate in analyzers.
/// </summary>
internal static class AnalysisContextExtensions
{
    /// <summary>
    /// Configures the analysis context with standard settings and registers a syntax node action.
    /// </summary>
    /// <param name="context">The analysis context to configure.</param>
    /// <param name="action">The action to execute for matching syntax nodes.</param>
    /// <param name="syntaxKinds">The syntax kinds to analyze.</param>
    public static void RegisterSyntaxNodeActionWithDefaults(
        this AnalysisContext context,
        Action<SyntaxNodeAnalysisContext> action,
        params SyntaxKind[] syntaxKinds)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(action, syntaxKinds);
    }
}

