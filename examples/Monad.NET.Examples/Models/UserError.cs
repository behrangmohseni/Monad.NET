namespace Monad.NET.Examples.Models;

/// <summary>
/// Typed error union for user operations.
/// The [ErrorUnion] attribute generates Match, Is{Case}, and As{Case} methods.
/// </summary>
[ErrorUnion]
public abstract partial record UserError
{
    /// <summary>User not found by ID.</summary>
    public sealed partial record NotFound(Guid Id) : UserError;

    /// <summary>Invalid email format.</summary>
    public sealed partial record InvalidEmail(string Email) : UserError;

    /// <summary>User is not authorized.</summary>
    public sealed partial record Unauthorized : UserError;

    /// <summary>Validation failed with message.</summary>
    public sealed partial record ValidationFailed(string Message) : UserError;
}

