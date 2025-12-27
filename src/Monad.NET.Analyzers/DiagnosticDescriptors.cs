using Microsoft.CodeAnalysis;

namespace Monad.NET.Analyzers;

public static class DiagnosticDescriptors
{
    private const string Category = "Monad.NET";

    public static readonly DiagnosticDescriptor UncheckedUnwrap = new(
        id: "MNT001",
        title: "Unchecked Unwrap call",
        messageFormat: "{0} should be checked with IsSuccess/IsSome before calling Unwrap()",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling Unwrap() without first checking if the monad contains a value can throw an exception at runtime.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt001");

    public static readonly DiagnosticDescriptor RedundantMapChain = new(
        id: "MNT002",
        title: "Redundant Map chain",
        messageFormat: "Consecutive Map calls can be combined into a single Map call",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Multiple consecutive Map calls can be combined into a single Map call by composing the functions.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt002");

    public static readonly DiagnosticDescriptor DiscardedMonad = new(
        id: "MNT005",
        title: "Discarded monad value",
        messageFormat: "{0} value is discarded; errors/missing values will be silently ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Discarding a Result or Option value means that any errors or missing values will be silently ignored.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt005");
}
