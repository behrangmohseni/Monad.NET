using Monad.NET.Examples.Models;

namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating ErrorUnion source generator.
/// The [ErrorUnion] attribute generates Match, Is{Case}, As{Case}, and Result helpers.
/// </summary>
public static class ErrorUnionExamples
{
    public static void Run()
    {
        Console.WriteLine("ErrorUnion generates typed error matching for Result<T, TError>.\n");
        
        // Creating Results with typed errors
        Console.WriteLine("1. Creating Results with Typed Errors:");
        var success = GetUser(Guid.NewGuid());
        var notFound = GetUser(Guid.Empty);
        var invalidEmail = ValidateEmail("bad-email");
        var unauthorized = CheckPermissions(false);
        
        Console.WriteLine($"   Success:      {success}");
        Console.WriteLine($"   NotFound:     {notFound}");
        Console.WriteLine($"   InvalidEmail: {invalidEmail}");
        Console.WriteLine($"   Unauthorized: {unauthorized}");

        // Pattern matching on error type
        Console.WriteLine("\n2. Match on Error Types:");
        var message = notFound.MatchError(
            ok: user => $"Found: {user.Email}",
            notFound: e => $"User {e.Id} not found",
            invalidEmail: e => $"Invalid email: {e.Email}",
            unauthorized: _ => "Access denied",
            validationFailed: e => $"Validation: {e.Message}");
        Console.WriteLine($"   Message: {message}");

        // Error type checking
        Console.WriteLine("\n3. Error Type Checking:");
        var error = notFound.UnwrapErr();
        Console.WriteLine($"   error.IsNotFound:        {error.IsNotFound}");
        Console.WriteLine($"   error.IsInvalidEmail:    {error.IsInvalidEmail}");
        Console.WriteLine($"   error.IsUnauthorized:    {error.IsUnauthorized}");
        Console.WriteLine($"   error.IsValidationFailed: {error.IsValidationFailed}");

        // Direct error matching
        Console.WriteLine("\n4. Match on Error Directly:");
        error.Match(
            notFound: e => Console.WriteLine($"   Not found: {e.Id}"),
            invalidEmail: e => Console.WriteLine($"   Invalid email: {e.Email}"),
            unauthorized: _ => Console.WriteLine("   Unauthorized"),
            validationFailed: e => Console.WriteLine($"   Validation: {e.Message}"));

        // Safe casting with As{Case}()
        Console.WriteLine("\n5. Safe Casting:");
        var asNotFound = error.AsNotFound();
        var asInvalidEmail = error.AsInvalidEmail();
        Console.WriteLine($"   AsNotFound():    {asNotFound}");
        Console.WriteLine($"   AsInvalidEmail(): {asInvalidEmail}");

        // MapError for transformations
        Console.WriteLine("\n6. MapError (to HTTP codes):");
        var httpCode = invalidEmail.MatchError(
            ok: _ => 200,
            notFound: _ => 404,
            invalidEmail: _ => 400,
            unauthorized: _ => 401,
            validationFailed: _ => 422);
        Console.WriteLine($"   HTTP Status: {httpCode}");

        // Recover from specific errors
        Console.WriteLine("\n7. Recover from Specific Errors:");
        var recovered = notFound.Recover(
            notFound: e => Result<User, UserError>.Ok(
                new User("default@example.com", "Default", "User", 0)));
        Console.WriteLine($"   Recovered: {recovered}");

        // ToResult helper
        Console.WriteLine("\n8. ToResult Helper:");
        var fromError = UserError.NewUnauthorized().ToResult<User>();
        Console.WriteLine($"   From error: {fromError}");

        // Real-world: API error handling
        Console.WriteLine("\n9. Real-World: API Response:");
        var apiResults = new[]
        {
            GetUser(Guid.NewGuid()),
            GetUser(Guid.Empty),
            ValidateEmail("test"),
            CheckPermissions(false)
        };

        foreach (var result in apiResults)
        {
            var response = FormatApiResponse(result);
            Console.WriteLine($"   {response}");
        }

        // Chaining with typed errors
        Console.WriteLine("\n10. Chaining Operations:");
        var chainResult = GetUser(Guid.NewGuid())
            .AndThen(user => ValidateUserEmail(user))
            .AndThen(user => CheckUserPermissions(user));
        Console.WriteLine($"   Chain result: {chainResult}");
    }

    private static Result<User, UserError> GetUser(Guid id)
    {
        if (id == Guid.Empty)
            return UserError.NewNotFound(id).ToResult<User>();
        
        return Result<User, UserError>.Ok(
            new User($"user-{id:N}@example.com", "John", "Doe", 25));
    }

    private static Result<User, UserError> ValidateEmail(string email)
    {
        if (!email.Contains('@'))
            return UserError.NewInvalidEmail(email).ToResult<User>();
        
        return Result<User, UserError>.Ok(
            new User(email, "Valid", "User", 30));
    }

    private static Result<User, UserError> CheckPermissions(bool hasPermission)
    {
        if (!hasPermission)
            return UserError.NewUnauthorized().ToResult<User>();
        
        return Result<User, UserError>.Ok(
            new User("admin@example.com", "Admin", "User", 35));
    }

    private static Result<User, UserError> ValidateUserEmail(User user)
    {
        return user.Email.Contains('@')
            ? Result<User, UserError>.Ok(user)
            : UserError.NewInvalidEmail(user.Email).ToResult<User>();
    }

    private static Result<User, UserError> CheckUserPermissions(User user)
    {
        return user.Age >= 18
            ? Result<User, UserError>.Ok(user)
            : UserError.NewUnauthorized().ToResult<User>();
    }

    private static string FormatApiResponse(Result<User, UserError> result)
    {
        return result.MatchError(
            ok: user => $"200 OK: {user.Email}",
            notFound: e => $"404 Not Found: {e.Id}",
            invalidEmail: e => $"400 Bad Request: {e.Email}",
            unauthorized: _ => "401 Unauthorized",
            validationFailed: e => $"422 Unprocessable: {e.Message}");
    }
}

