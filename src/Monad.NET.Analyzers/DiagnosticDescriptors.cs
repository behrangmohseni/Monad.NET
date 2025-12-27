using Microsoft.CodeAnalysis;

namespace Monad.NET.Analyzers;

public static class DiagnosticDescriptors
{
    private const string Category = "Monad.NET";

    // === SAFETY WARNINGS ===

    public static readonly DiagnosticDescriptor UncheckedUnwrap = new(
        id: "MNT001",
        title: "Unchecked Unwrap call",
        messageFormat: "{0} should be checked with IsSuccess/IsSome before calling Unwrap()",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling Unwrap() without first checking if the monad contains a value can throw an exception at runtime.");

    public static readonly DiagnosticDescriptor DiscardedMonad = new(
        id: "MNT005",
        title: "Discarded monad value",
        messageFormat: "{0} value is discarded; errors/missing values will be silently ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Discarding a Result or Option value means that any errors or missing values will be silently ignored.");

    public static readonly DiagnosticDescriptor ThrowInMatch = new(
        id: "MNT006",
        title: "Throwing in Match defeats its purpose",
        messageFormat: "Throwing in {0} branch of Match defeats the purpose of pattern matching; use Unwrap/Expect instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "If you're going to throw in the error/none case of Match, just use Unwrap() or Expect() instead.");

    public static readonly DiagnosticDescriptor NullableToSome = new(
        id: "MNT007",
        title: "Nullable value passed to Some",
        messageFormat: "Passing a potentially null value to Option.Some will throw at runtime; use Option.FromNullable instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option.Some throws if passed null. Use Option.FromNullable for nullable values.");

    // === STYLE SUGGESTIONS ===

    public static readonly DiagnosticDescriptor RedundantMapChain = new(
        id: "MNT002",
        title: "Redundant Map chain",
        messageFormat: "Consecutive Map calls can be combined into a single Map call",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Multiple consecutive Map calls can be combined into a single Map call by composing the functions.");

    public static readonly DiagnosticDescriptor MapGetOrElseToMatch = new(
        id: "MNT003",
        title: "Map+GetOrElse can be simplified",
        messageFormat: "Map followed by {0} can be simplified to Match or MapOr",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using Map followed by GetOrElse/UnwrapOr is equivalent to Match or MapOr.");

    public static readonly DiagnosticDescriptor FlatMapToMap = new(
        id: "MNT004",
        title: "FlatMap can be simplified to Map",
        messageFormat: "FlatMap returning {0} can be simplified to Map",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When FlatMap/AndThen always wraps the result in Some/Ok, it can be simplified to Map.");

    public static readonly DiagnosticDescriptor FilterConstant = new(
        id: "MNT008",
        title: "Filter with constant predicate",
        messageFormat: "Filter with constant {0} is redundant",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Filter with a constant true returns the same value; Filter with constant false always returns None.");

    public static readonly DiagnosticDescriptor MapIdentity = new(
        id: "MNT009",
        title: "Map with identity function",
        messageFormat: "Map with identity function (x => x) is redundant",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Mapping with an identity function has no effect and can be removed.");

    public static readonly DiagnosticDescriptor OptionNullComparison = new(
        id: "MNT010",
        title: "Null comparison on Option",
        messageFormat: "Comparing Option to null is incorrect; use IsSome/IsNone instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option<T> is a struct and is never null. Use IsSome/IsNone properties instead.");

    public static readonly DiagnosticDescriptor UnwrapInLinq = new(
        id: "MNT011",
        title: "Unwrap in LINQ query",
        messageFormat: "Using Unwrap inside a LINQ query can throw; consider using SelectMany or filtering first",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling Unwrap inside Select/Where can throw for None/Err values. Use SelectMany for safe unwrapping.");

    public static readonly DiagnosticDescriptor DoubleNegation = new(
        id: "MNT012",
        title: "Double negation in condition",
        messageFormat: "Double negation (!{0}.IsNone or !{0}.IsErr) can be simplified to {0}.IsSome or {0}.IsOk",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Use the positive form (IsSome, IsOk) instead of negating the negative form.");
}
