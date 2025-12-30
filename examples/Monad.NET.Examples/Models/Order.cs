namespace Monad.NET.Examples.Models;

/// <summary>
/// Order model for demonstrating nested option access.
/// </summary>
public record Order(Customer? Customer);

/// <summary>
/// Customer model with optional address.
/// </summary>
public record Customer(Address? Address);

/// <summary>
/// Address model.
/// </summary>
public record Address(string City, string Country);

/// <summary>
/// Order data transfer object.
/// </summary>
public record OrderDto(
    Guid Id,
    string CustomerId,
    string ProductId,
    int Quantity,
    decimal Total);

/// <summary>
/// Order form input model.
/// </summary>
public record OrderForm(string Name, string Email, int Quantity);

