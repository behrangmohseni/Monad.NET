using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monad.NET.AspNetCore;
using Xunit;

namespace Monad.NET.RealWorld.Tests;

/// <summary>
/// These tests demonstrate real-world ASP.NET Core scenarios where
/// Monad.NET provides tangible benefits over traditional approaches.
/// 
/// Each test shows:
/// 1. A realistic scenario from web API development
/// 2. How Result/Option/Validation map to HTTP responses
/// 3. Benefits in terms of code clarity and consistency
/// </summary>
public class AspNetCoreScenarioTests
{
    #region Scenario: CRUD Operations with Proper HTTP Status Codes

    /// <summary>
    /// GET /users/{id} - Returns 200 OK or 404 Not Found
    /// Result<T,E> naturally maps to these status codes.
    /// </summary>
    [Fact]
    public void GetById_ReturnsOkOrNotFound_BasedOnResult()
    {
        var controller = new UsersController(new UserRepository());

        // User exists -> 200 OK
        var existingResult = controller.GetUser(1);
        var okResult = Assert.IsType<OkObjectResult>(existingResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        // User doesn't exist -> 404 Not Found
        var notFoundResult = controller.GetUser(999);
        var notFound = Assert.IsType<NotFoundObjectResult>(notFoundResult);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    /// <summary>
    /// POST /users - Returns 201 Created or 400 Bad Request with validation errors
    /// Validation<T,E> accumulates all errors for the response.
    /// </summary>
    [Fact]
    public void CreateUser_ReturnsCreatedOrValidationErrors()
    {
        var controller = new UsersController(new UserRepository());

        // Valid request -> 201 Created
        var validRequest = new CreateUserRequest("John Doe", "john@example.com", 25);
        var createdResult = controller.CreateUser(validRequest);
        var created = Assert.IsType<CreatedAtActionResult>(createdResult);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);

        // Invalid request -> 400 Bad Request with ALL errors
        var invalidRequest = new CreateUserRequest("", "invalid", -5);
        var badResult = controller.CreateUser(invalidRequest);
        var badRequest = Assert.IsType<BadRequestObjectResult>(badResult);

        // Check that all validation errors are returned
        var errors = Assert.IsType<List<string>>(badRequest.Value);
        Assert.Equal(3, errors.Count);
    }

    /// <summary>
    /// PUT /users/{id} - Returns 200 OK, 404 Not Found, or 400 Bad Request
    /// Demonstrates handling multiple error types.
    /// </summary>
    [Fact]
    public void UpdateUser_HandlesMultipleErrorScenarios()
    {
        var controller = new UsersController(new UserRepository());

        // Success -> 200 OK
        var successResult = controller.UpdateUser(1, new UpdateUserRequest("Updated Name"));
        Assert.IsType<OkObjectResult>(successResult);

        // User not found -> 404
        var notFoundResult = controller.UpdateUser(999, new UpdateUserRequest("Name"));
        Assert.IsType<NotFoundObjectResult>(notFoundResult);

        // Validation error -> 400
        var validationResult = controller.UpdateUser(1, new UpdateUserRequest(""));
        Assert.IsType<BadRequestObjectResult>(validationResult);
    }

    #endregion

    #region Scenario: Complex Business Operations

    /// <summary>
    /// POST /orders - Complex order processing with multiple failure points
    /// Result chains naturally produce appropriate HTTP responses.
    /// </summary>
    [Fact]
    public void ProcessOrder_HandlesPipelineErrors_WithAppropriateStatusCodes()
    {
        var controller = new OrdersController();

        // Successful order -> 201 Created
        var successResult = controller.CreateOrder(new CreateOrderRequest(1, "product-1", 5));
        var created = Assert.IsType<CreatedAtActionResult>(successResult);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);

        // Invalid customer -> 400 Bad Request
        var invalidCustomer = controller.CreateOrder(new CreateOrderRequest(-1, "product-1", 5));
        var badRequest = Assert.IsType<BadRequestObjectResult>(invalidCustomer);
        Assert.Contains("Invalid customer", badRequest.Value?.ToString());

        // Out of stock -> 409 Conflict (business rule failure)
        var outOfStock = controller.CreateOrder(new CreateOrderRequest(1, "out-of-stock", 100));
        var conflict = Assert.IsType<ConflictObjectResult>(outOfStock);
        Assert.Contains("stock", conflict.Value?.ToString()?.ToLower());

        // Payment failed -> 402 Payment Required
        var paymentFailed = controller.CreateOrder(new CreateOrderRequest(2, "product-1", 5)); // User 2 has no funds
        var paymentRequired = Assert.IsType<ObjectResult>(paymentFailed);
        Assert.Equal(StatusCodes.Status402PaymentRequired, paymentRequired.StatusCode);
    }

    #endregion

    #region Scenario: Query Operations with Filtering

    /// <summary>
    /// GET /products?category=X&minPrice=Y
    /// Option<T> for optional query parameters, Result for the operation.
    /// </summary>
    [Fact]
    public void SearchProducts_HandlesOptionalFilters_Gracefully()
    {
        var controller = new ProductsController();

        // No filters -> returns all products
        var allProducts = controller.SearchProducts(null, null, null);
        var okResult = Assert.IsType<OkObjectResult>(allProducts);
        var products = Assert.IsType<List<Product>>(okResult.Value);
        Assert.True(products.Count > 0);

        // With category filter
        var filtered = controller.SearchProducts("Electronics", null, null);
        var filteredOk = Assert.IsType<OkObjectResult>(filtered);
        var filteredProducts = Assert.IsType<List<Product>>(filteredOk.Value);
        Assert.All(filteredProducts, p => Assert.Equal("Electronics", p.Category));

        // With price filter
        var priceFiltered = controller.SearchProducts(null, 100m, null);
        var priceOk = Assert.IsType<OkObjectResult>(priceFiltered);
        var priceProducts = Assert.IsType<List<Product>>(priceOk.Value);
        Assert.All(priceProducts, p => Assert.True(p.Price >= 100m));
    }

    #endregion

    #region Scenario: Async Operations

    /// <summary>
    /// Demonstrates async Result handling in controllers.
    /// </summary>
    [Fact]
    public async Task AsyncOperation_ReturnsAppropriateStatusCodes()
    {
        var controller = new AsyncController();

        // Successful async operation
        var successResult = await controller.ProcessAsync(new ProcessRequest("valid-data"));
        Assert.IsType<OkObjectResult>(successResult);

        // Failed async operation - ToActionResult returns ObjectResult with status code
        var failResult = await controller.ProcessAsync(new ProcessRequest("invalid-data"));
        var objectResult = Assert.IsType<ObjectResult>(failResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    #endregion

    #region Scenario: Consistent Error Response Format

    /// <summary>
    /// All endpoints return consistent error format using ProblemDetails.
    /// This is a key benefit - standardized error responses across the API.
    /// </summary>
    [Fact]
    public void ErrorResponses_UseConsistentProblemDetailsFormat()
    {
        var controller = new ConsistentErrorController();

        // Validation error
        var validationResult = controller.ValidateEndpoint(new ValidateRequest(""));
        var validationProblem = Assert.IsType<ObjectResult>(validationResult);
        var validationDetails = Assert.IsType<ProblemDetails>(validationProblem.Value);
        Assert.Equal("Validation Error", validationDetails.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, validationDetails.Status);

        // Not found error
        var notFoundResult = controller.FindEndpoint(999);
        var notFoundProblem = Assert.IsType<ObjectResult>(notFoundResult);
        var notFoundDetails = Assert.IsType<ProblemDetails>(notFoundProblem.Value);
        Assert.Equal("Not Found", notFoundDetails.Title);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundDetails.Status);

        // Business rule error
        var businessResult = controller.BusinessEndpoint();
        var businessProblem = Assert.IsType<ObjectResult>(businessResult);
        var businessDetails = Assert.IsType<ProblemDetails>(businessProblem.Value);
        Assert.Equal("Business Rule Violation", businessDetails.Title);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, businessDetails.Status);
    }

    #endregion

    #region Controller Implementations

    private class UsersController
    {
        private readonly UserRepository _repository;

        public UsersController(UserRepository repository) => _repository = repository;

        public IActionResult GetUser(int id)
        {
            return _repository.FindById(id)
                .ToActionResult(
                    onOk: user => new OkObjectResult(user),
                    onErr: error => new NotFoundObjectResult(error));
        }

        public IActionResult CreateUser(CreateUserRequest request)
        {
            return ValidateCreateRequest(request)
                .Match<IActionResult>(
                    validFunc: user =>
                    {
                        var created = _repository.Create(user);
                        return new CreatedAtActionResult(
                            nameof(GetUser),
                            "Users",
                            new { id = created.Id },
                            created);
                    },
                    invalidFunc: errors => new BadRequestObjectResult(errors.ToList()));
        }

        public IActionResult UpdateUser(int id, UpdateUserRequest request)
        {
            // First validate the request
            if (string.IsNullOrWhiteSpace(request.Name))
                return new BadRequestObjectResult("Name is required");

            // Then check if user exists
            return _repository.Update(id, request.Name)
                .ToActionResult(
                    onOk: user => new OkObjectResult(user),
                    onErr: error => new NotFoundObjectResult(error));
        }

        private static Validation<UserData, string> ValidateCreateRequest(CreateUserRequest request)
        {
            var nameValidation = string.IsNullOrWhiteSpace(request.Name)
                ? Validation<string, string>.Error("Name is required")
                : Validation<string, string>.Ok(request.Name);

            var emailValidation = !request.Email.Contains('@')
                ? Validation<string, string>.Error("Invalid email format")
                : Validation<string, string>.Ok(request.Email);

            var ageValidation = request.Age < 0
                ? Validation<int, string>.Error("Age must be non-negative")
                : Validation<int, string>.Ok(request.Age);

            return nameValidation
                .Apply(emailValidation, (n, e) => (n, e))
                .Apply(ageValidation, (tuple, a) => new UserData(0, tuple.n, tuple.e, a));
        }
    }

    private class OrdersController
    {
        public IActionResult CreateOrder(CreateOrderRequest request)
        {
            return ProcessOrderPipeline(request)
                .Match<IActionResult>(
                    okFunc: order => new CreatedAtActionResult("GetOrder", "Orders", new { id = order.Id }, order),
                    errFunc: error => error switch
                    {
                        OrderError.InvalidCustomer e => new BadRequestObjectResult($"Invalid customer: {e.Message}"),
                        OrderError.InvalidProduct e => new BadRequestObjectResult($"Invalid product: {e.Message}"),
                        OrderError.InsufficientStock e => new ConflictObjectResult($"Insufficient stock: need {e.Required}, have {e.Available}"),
                        OrderError.PaymentFailed e => new ObjectResult($"Payment failed: {e.Message}") { StatusCode = StatusCodes.Status402PaymentRequired },
                        _ => new BadRequestObjectResult("Unknown error")
                    });
        }

        private Result<Order, OrderError> ProcessOrderPipeline(CreateOrderRequest request)
        {
            return ValidateCustomer(request.CustomerId)
                .Bind(_ => ValidateProduct(request.ProductId))
                .Bind(product => CheckStock(product, request.Quantity))
                .Bind(stock => ProcessPayment(request.CustomerId, stock.Price * request.Quantity))
                .Map(paymentId => new Order(Guid.NewGuid(), request.CustomerId, request.ProductId, request.Quantity, paymentId));
        }

        private Result<Customer, OrderError> ValidateCustomer(int customerId) =>
            customerId > 0
                ? Result<Customer, OrderError>.Ok(new Customer(customerId, $"Customer {customerId}"))
                : Result<Customer, OrderError>.Error(new OrderError.InvalidCustomer("Customer ID must be positive"));

        private Result<ProductInfo, OrderError> ValidateProduct(string productId) =>
            productId.Contains("invalid")
                ? Result<ProductInfo, OrderError>.Error(new OrderError.InvalidProduct($"Product {productId} not found"))
                : Result<ProductInfo, OrderError>.Ok(new ProductInfo(productId, 10m));

        private Result<StockInfo, OrderError> CheckStock(ProductInfo product, int quantity) =>
            product.Id.Contains("out-of-stock")
                ? Result<StockInfo, OrderError>.Error(new OrderError.InsufficientStock(quantity, 0))
                : Result<StockInfo, OrderError>.Ok(new StockInfo(100, product.Price));

        private Result<string, OrderError> ProcessPayment(int customerId, decimal amount) =>
            customerId == 2 // User 2 has no funds
                ? Result<string, OrderError>.Error(new OrderError.PaymentFailed("Insufficient funds"))
                : Result<string, OrderError>.Ok($"payment-{Guid.NewGuid():N}");
    }

    private class ProductsController
    {
        private readonly List<Product> _products = new()
        {
            new Product(1, "Laptop", "Electronics", 999.99m),
            new Product(2, "Phone", "Electronics", 499.99m),
            new Product(3, "Desk", "Furniture", 199.99m),
            new Product(4, "Chair", "Furniture", 89.99m)
        };

        public IActionResult SearchProducts(string? category, decimal? minPrice, decimal? maxPrice)
        {
            var filtered = _products.AsEnumerable();

            // Apply optional filters using Option
            var categoryOpt = category.ToOption();
            var minPriceOpt = minPrice.HasValue ? Option<decimal>.Some(minPrice.Value) : Option<decimal>.None();
            var maxPriceOpt = maxPrice.HasValue ? Option<decimal>.Some(maxPrice.Value) : Option<decimal>.None();

            categoryOpt.Tap(c => filtered = filtered.Where(p => p.Category == c));
            minPriceOpt.Tap(min => filtered = filtered.Where(p => p.Price >= min));
            maxPriceOpt.Tap(max => filtered = filtered.Where(p => p.Price <= max));

            return new OkObjectResult(filtered.ToList());
        }
    }

    private class AsyncController
    {
        public async Task<IActionResult> ProcessAsync(ProcessRequest request)
        {
            var result = await ProcessDataAsync(request.Data);
            return result.ToActionResult();
        }

        private async Task<Result<string, string>> ProcessDataAsync(string data)
        {
            await Task.Delay(1); // Simulate async work
            return data.Contains("invalid")
                ? Result<string, string>.Error("Invalid data provided")
                : Result<string, string>.Ok($"Processed: {data}");
        }
    }

    private class ConsistentErrorController
    {
        public IActionResult ValidateEndpoint(ValidateRequest request)
        {
            var result = string.IsNullOrWhiteSpace(request.Data)
                ? Result<string, string>.Error("Data is required")
                : Result<string, string>.Ok(request.Data);

            return result.ToActionResultWithProblemDetails("Validation Error", StatusCodes.Status400BadRequest);
        }

        public IActionResult FindEndpoint(int id)
        {
            var result = id > 0 && id < 100
                ? Result<string, string>.Ok($"Found item {id}")
                : Result<string, string>.Error($"Item {id} not found");

            return result.ToActionResultWithProblemDetails("Not Found", StatusCodes.Status404NotFound);
        }

        public IActionResult BusinessEndpoint()
        {
            var result = Result<string, string>.Error("Cannot process: business rule violated");
            return result.ToActionResultWithProblemDetails("Business Rule Violation", StatusCodes.Status422UnprocessableEntity);
        }
    }

    #endregion

    #region Models

    public record CreateUserRequest(string Name, string Email, int Age);
    public record UpdateUserRequest(string Name);
    public record UserData(int Id, string Name, string Email, int Age);

    public record CreateOrderRequest(int CustomerId, string ProductId, int Quantity);
    public record Order(Guid Id, int CustomerId, string ProductId, int Quantity, string PaymentId);
    public record Customer(int Id, string Name);
    public record ProductInfo(string Id, decimal Price);
    public record StockInfo(int Available, decimal Price);

    public record Product(int Id, string Name, string Category, decimal Price);

    public record ProcessRequest(string Data);
    public record ValidateRequest(string Data);

    public abstract record OrderError
    {
        public record InvalidCustomer(string Message) : OrderError;
        public record InvalidProduct(string Message) : OrderError;
        public record InsufficientStock(int Required, int Available) : OrderError;
        public record PaymentFailed(string Message) : OrderError;
    }

    #endregion

    #region Repository

    private class UserRepository
    {
        private readonly Dictionary<int, UserData> _users = new()
        {
            [1] = new UserData(1, "Alice", "alice@example.com", 30),
            [2] = new UserData(2, "Bob", "bob@example.com", 25)
        };
        private int _nextId = 3;

        public Result<UserData, string> FindById(int id)
        {
            return _users.TryGetValue(id, out var user)
                ? Result<UserData, string>.Ok(user)
                : Result<UserData, string>.Error($"User {id} not found");
        }

        public UserData Create(UserData user)
        {
            var created = user with { Id = _nextId++ };
            _users[created.Id] = created;
            return created;
        }

        public Result<UserData, string> Update(int id, string name)
        {
            if (!_users.TryGetValue(id, out var user))
                return Result<UserData, string>.Error($"User {id} not found");

            var updated = user with { Name = name };
            _users[id] = updated;
            return Result<UserData, string>.Ok(updated);
        }
    }

    #endregion
}
