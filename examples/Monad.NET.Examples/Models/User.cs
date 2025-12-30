namespace Monad.NET.Examples.Models;

/// <summary>
/// Sample user model for demonstrating validation and data operations.
/// </summary>
public record User(
    string Email,
    string FirstName,
    string LastName,
    int Age)
{
    public string? MiddleName { get; init; }

    public string FullName => MiddleName is not null
        ? $"{FirstName} {MiddleName} {LastName}"
        : $"{FirstName} {LastName}";
}

/// <summary>
/// Data transfer object for user creation.
/// </summary>
public record UserDto(string Name, string Email, int Age);

/// <summary>
/// User form input model.
/// </summary>
public record UserForm(string Email, int Age);

