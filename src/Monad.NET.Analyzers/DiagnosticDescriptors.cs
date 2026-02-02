using Microsoft.CodeAnalysis;

namespace Monad.NET.Analyzers;

/// <summary>
/// Contains all diagnostic descriptors for Monad.NET analyzers.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Monad.NET";

    // ============================================================================
    // Core diagnostics (MNT001-MNT005)
    // ============================================================================

    /// <summary>
    /// MNT001: Result/Option not checked before Unwrap.
    /// </summary>
    public static readonly DiagnosticDescriptor UncheckedUnwrap = new(
        id: "MNT001",
        title: "Unchecked Unwrap call",
        messageFormat: "{0} should be checked with IsOk/IsSome before calling Unwrap()",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling Unwrap() without first checking if the monad contains a value can throw an exception at runtime. Consider using Match, Map, or checking IsOk/IsSome first.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt001");

    /// <summary>
    /// MNT002: Consecutive Map calls can be combined.
    /// </summary>
    public static readonly DiagnosticDescriptor RedundantMapChain = new(
        id: "MNT002",
        title: "Redundant Map chain",
        messageFormat: "Consecutive Map calls can be combined into a single Map call",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Multiple consecutive Map calls can be combined into a single Map call by composing the functions. This improves readability and may slightly improve performance.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt002");

    /// <summary>
    /// MNT003: Map followed by GetOrElse can be simplified to Match.
    /// </summary>
    public static readonly DiagnosticDescriptor MapGetOrElseToMatch = new(
        id: "MNT003",
        title: "Map followed by GetOrElse can be simplified",
        messageFormat: "Map followed by GetOrElse can be simplified to Match",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using Map followed by GetOrElse is equivalent to Match, which is more idiomatic and expressive.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt003");

    /// <summary>
    /// MNT004: Bind returning Some/Ok can be simplified to Map.
    /// </summary>
    public static readonly DiagnosticDescriptor BindToMap = new(
        id: "MNT004",
        title: "Bind can be simplified to Map",
        messageFormat: "Bind returning {0} can be simplified to Map",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When Bind's lambda always wraps the result in Some/Ok, it can be simplified to a Map call.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt004");

    /// <summary>
    /// MNT005: Avoid discarding Result/Option values.
    /// </summary>
    public static readonly DiagnosticDescriptor DiscardedMonad = new(
        id: "MNT005",
        title: "Discarded monad value",
        messageFormat: "{0} value is discarded; errors/missing values will be silently ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Discarding a Result or Option value means that any errors or missing values will be silently ignored, which can hide bugs.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt005");

    // ============================================================================
    // Pattern-specific diagnostics (MNT006-MNT012)
    // ============================================================================

    /// <summary>
    /// MNT006: Throwing exception in Match branch - consider using Result/Try instead.
    /// </summary>
    public static readonly DiagnosticDescriptor ThrowInMatch = new(
        id: "MNT006",
        title: "Throwing in Match branch",
        messageFormat: "Throwing an exception in a Match branch defeats the purpose of monadic error handling",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "If you need to throw on a specific case, consider using Result<T,E> or Try<T> to propagate errors functionally instead of throwing exceptions.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt006");

    /// <summary>
    /// MNT007: Nullable value passed to Some - use ToOption() instead.
    /// </summary>
    public static readonly DiagnosticDescriptor NullableToSome = new(
        id: "MNT007",
        title: "Nullable value passed to Some",
        messageFormat: "Passing a nullable value to Option.Some() may throw; use ToOption() instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option.Some(value) throws if value is null. For nullable values, use value.ToOption() which safely converts null to None.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt007");

    /// <summary>
    /// MNT008: Filter with constant predicate detected.
    /// </summary>
    public static readonly DiagnosticDescriptor FilterConstant = new(
        id: "MNT008",
        title: "Filter with constant predicate",
        messageFormat: "Filter with constant predicate '{0}' - this will always {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Filter with a constant true/false predicate is redundant. Filter(_ => true) always returns the same value, and Filter(_ => false) always returns None.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt008");

    /// <summary>
    /// MNT009: Map with identity function detected.
    /// </summary>
    public static readonly DiagnosticDescriptor MapIdentity = new(
        id: "MNT009",
        title: "Map with identity function",
        messageFormat: "Map with identity function (x => x) has no effect and can be removed",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Mapping with an identity function (x => x) returns the same value and can be safely removed.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt009");

    /// <summary>
    /// MNT010: Option compared to null - use IsSome/IsNone instead.
    /// </summary>
    public static readonly DiagnosticDescriptor OptionNullComparison = new(
        id: "MNT010",
        title: "Option compared to null",
        messageFormat: "Comparing Option to null is incorrect; use IsSome or IsNone instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option<T> is a struct and will never be null. Use IsSome or IsNone to check the state of an Option.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt010");

    /// <summary>
    /// MNT011: Reserved for future use.
    /// </summary>

    /// <summary>
    /// MNT012: Double negation pattern detected (e.g., !option.IsNone can be simplified to option.IsSome).
    /// </summary>
    public static readonly DiagnosticDescriptor DoubleNegation = new(
        id: "MNT012",
        title: "Double negation detected",
        messageFormat: "Double negation '{0}' can be simplified to '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Patterns like !option.IsNone or !result.IsErr can be simplified to option.IsSome or result.IsOk for better readability.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt012");

    // ============================================================================
    // Additional diagnostics (MNT013+) - Added for future analyzers
    // ============================================================================

    /// <summary>
    /// MNT014: Prefer Match over manual IsSome/IsOk checks.
    /// </summary>
    public static readonly DiagnosticDescriptor PreferMatch = new(
        id: "MNT014",
        title: "Prefer Match over manual state checks",
        messageFormat: "Consider using Match() instead of checking {0} and then accessing the value",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Match() is more idiomatic and ensures exhaustive handling of all cases. It also prevents accidental Unwrap() calls without prior checks.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt014");

    /// <summary>
    /// MNT015: Async operation missing ConfigureAwait(false).
    /// </summary>
    public static readonly DiagnosticDescriptor MissingConfigureAwait = new(
        id: "MNT015",
        title: "Async monad operation missing ConfigureAwait(false)",
        messageFormat: "Async operation on {0} should use ConfigureAwait(false) in library code",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: false, // Disabled by default as it's opinionated
        description: "In library code, async operations should generally use ConfigureAwait(false) to avoid deadlocks and improve performance. Enable this rule for library projects.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt015");

    /// <summary>
    /// MNT016: Option.Some(null) will throw; use ToOption() for nullable values.
    /// </summary>
    public static readonly DiagnosticDescriptor SomeWithPotentialNull = new(
        id: "MNT016",
        title: "Option.Some() may receive null",
        messageFormat: "Option.Some() will throw if passed null; consider using ToOption() for potentially null values",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option.Some(value) throws ArgumentNullException if value is null. If the value may be null, use value.ToOption() which safely converts null to None.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt016");

    /// <summary>
    /// MNT017: Empty Match branch - consider using Tap instead.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyMatchBranch = new(
        id: "MNT017",
        title: "Empty Match branch",
        messageFormat: "Match has an empty {0} branch; if you only need to handle one case, consider using Tap{1}() instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "If one branch of a Match call is empty (does nothing), consider using Tap() or TapErr()/TapNone() which are designed for handling only one case.",
        helpLinkUri: "https://github.com/behrangmohseni/Monad.NET/blob/main/docs/Guides/Analyzers.md#mnt017");
}
