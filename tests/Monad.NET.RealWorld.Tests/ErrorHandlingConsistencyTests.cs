using Xunit;

namespace Monad.NET.RealWorld.Tests;

/// <summary>
/// These tests demonstrate how Monad.NET provides consistent error handling
/// across different scenarios and team members.
/// 
/// Consistency benefits:
/// 1. All errors are handled the same way
/// 2. No forgotten error cases
/// 3. Easy to audit error handling
/// 4. Predictable behavior across the codebase
/// </summary>
public class ErrorHandlingConsistencyTests
{
    #region Test: Type System Prevents Forgotten Error Handling

    /// <summary>
    /// With Result<T,E>, you CANNOT ignore errors - the type system forces handling.
    /// This is enforced at compile time, not runtime.
    /// </summary>
    [Fact]
    public void Result_ForcesErrorHandling_ThroughTypeSystem()
    {
        var service = new PaymentService();

        // You can't just "use" the result - you must handle both cases
        Result<PaymentConfirmation, PaymentError> result = service.ProcessPayment(100m);

        // Option 1: Pattern match (exhaustive)
        var message = result.Match(
            okFunc: confirmation => $"Payment successful: {confirmation.TransactionId}",
            errFunc: error => $"Payment failed: {error.Code}"
        );

        // Option 2: Check and extract
        if (result.IsOk)
        {
            var confirmation = result.GetValue();
            Assert.NotNull(confirmation.TransactionId);
        }
        else
        {
            var error = result.GetError();
            Assert.NotNull(error.Code);
        }

        // You CANNOT do this (would not compile with strict null checks):
        // PaymentConfirmation confirmation = result; // Error: can't convert Result to PaymentConfirmation

        Assert.True(result.IsOk);
    }

    /// <summary>
    /// Demonstrates that Option<T> prevents null reference exceptions
    /// through type-safe handling.
    /// </summary>
    [Fact]
    public void Option_PreventsNullReferenceExceptions_ThroughTypeSystem()
    {
        var repository = new UserRepository();

        // FindById returns Option<User>, not User?
        Option<User> userOption = repository.FindById(999);

        // You can't just call methods on Option - you must handle the None case
        // userOption.Name; // Won't compile - Option<User> doesn't have .Name

        // Safe access requires explicit handling
        var name = userOption.Match(
            someFunc: user => user.Name,
            noneFunc: () => "Unknown"
        );

        Assert.Equal("Unknown", name);

        // Or use Map for transformation
        var upperName = userOption
            .Map(user => user.Name.ToUpper())
            .GetValueOr("UNKNOWN");

        Assert.Equal("UNKNOWN", upperName);
    }

    #endregion

    #region Test: Consistent Error Propagation

    /// <summary>
    /// Errors propagate consistently through the entire call chain.
    /// No need to check at each step - errors automatically short-circuit.
    /// </summary>
    [Fact]
    public void ErrorPropagation_IsConsistent_ThroughEntireCallChain()
    {
        var orchestrator = new OrderOrchestrator();
        var callLog = new List<string>();

        // When validation fails early, later steps are NOT called
        var result = orchestrator.ProcessOrderWithLogging(
            customerId: -1, // Invalid - should fail validation
            productId: "product-1",
            quantity: 5,
            callLog);

        Assert.True(result.IsError);
        Assert.Single(callLog); // Only "ValidateCustomer" was called
        Assert.Equal("ValidateCustomer", callLog[0]);

        // When all steps succeed, all are called
        callLog.Clear();
        var successResult = orchestrator.ProcessOrderWithLogging(1, "product-1", 5, callLog);

        Assert.True(successResult.IsOk);
        Assert.Equal(4, callLog.Count); // All four steps called
    }

    /// <summary>
    /// Shows that error context is preserved through the chain.
    /// You always know WHERE the error originated.
    /// </summary>
    [Fact]
    public void ErrorContext_IsPreserved_ThroughCallChain()
    {
        var orchestrator = new OrderOrchestrator();

        // Error at validation step
        var validationError = orchestrator.ProcessOrder(-1, "product-1", 5);
        Assert.True(validationError.IsError);
        Assert.IsType<OrderError.ValidationError>(validationError.GetError());

        // Error at inventory step (product ID must start with "product-" to pass validation)
        var inventoryError = orchestrator.ProcessOrder(1, "product-out-of-stock", 5);
        Assert.True(inventoryError.IsError);
        Assert.IsType<OrderError.InventoryError>(inventoryError.GetError());

        // Error at payment step
        var paymentError = orchestrator.ProcessOrder(999, "product-1", 5); // Customer 999 has no funds
        Assert.True(paymentError.IsError);
        Assert.IsType<OrderError.PaymentError>(paymentError.GetError());
    }

    #endregion

    #region Test: Standardized Error Types

    /// <summary>
    /// Using discriminated unions for errors ensures all error types are known
    /// and handled consistently.
    /// </summary>
    [Fact]
    public void ErrorTypes_AreStandardized_AcrossApplication()
    {
        var errors = new List<ApiError>
        {
            new ApiError.NotFound("User", 123),
            new ApiError.ValidationFailed(new[] { "Name is required", "Email is invalid" }),
            new ApiError.Unauthorized("Invalid token"),
            new ApiError.BusinessRuleViolation("Cannot delete active subscription")
        };

        // All error types can be handled uniformly
        foreach (var error in errors)
        {
            var (statusCode, message) = error switch
            {
                ApiError.NotFound e => (404, $"{e.ResourceType} {e.ResourceId} not found"),
                ApiError.ValidationFailed e => (400, $"Validation errors: {string.Join(", ", e.Errors)}"),
                ApiError.Unauthorized e => (401, e.Reason),
                ApiError.BusinessRuleViolation e => (422, e.Message),
                _ => (500, "Unknown error")
            };

            Assert.True(statusCode >= 400);
            Assert.NotEmpty(message);
        }
    }

    /// <summary>
    /// Demonstrates consistent error transformation across layers.
    /// </summary>
    [Fact]
    public void ErrorTransformation_IsConsistent_AcrossLayers()
    {
        var repository = new ProductRepository();
        var service = new ProductService(repository);
        var controller = new ProductController(service);

        // Repository returns Option
        Option<Product> repoResult = repository.FindById(999);
        Assert.True(repoResult.IsNone);

        // Service converts to Result with business error
        Result<Product, ServiceError> serviceResult = service.GetProduct(999);
        Assert.True(serviceResult.IsError);
        Assert.IsType<ServiceError.NotFound>(serviceResult.GetError());

        // Controller converts to HTTP response
        var httpResponse = controller.GetProduct(999);
        Assert.Equal(404, httpResponse.StatusCode);
    }

    #endregion

    #region Test: Validation Consistency

    /// <summary>
    /// Validation is consistent regardless of who writes the code.
    /// The Validation<T,E> type enforces the pattern.
    /// </summary>
    [Fact]
    public void Validation_IsConsistent_AcrossTeamMembers()
    {
        // Developer A's validation
        var validatorA = new UserValidatorA();
        var resultA = validatorA.Validate("", "invalid", -5);

        // Developer B's validation (same pattern, same behavior)
        var validatorB = new UserValidatorB();
        var resultB = validatorB.Validate("", "invalid", -5);

        // Both accumulate all errors
        Assert.True(resultA.IsError);
        Assert.True(resultB.IsError);

        var errorsA = resultA.Match(validFunc: _ => 0, invalidFunc: errs => errs.Count());
        var errorsB = resultB.Match(validFunc: _ => 0, invalidFunc: errs => errs.Count());

        Assert.Equal(3, errorsA);
        Assert.Equal(3, errorsB);
    }

    /// <summary>
    /// Validation errors can be combined from multiple sources consistently.
    /// </summary>
    [Fact]
    public void ValidationErrors_CombineConsistently_FromMultipleSources()
    {
        var addressValidator = new AddressValidator();
        var paymentValidator = new PaymentValidator();

        // Both fail with errors
        var addressResult = addressValidator.Validate("", "", "");
        var paymentResult = paymentValidator.Validate("", "");

        // Combine errors from both validators
        var combined = addressResult
            .Apply(paymentResult, (addr, payment) => (addr, payment));

        Assert.True(combined.IsError);

        var allErrors = combined.Match(
            validFunc: _ => Array.Empty<string>(),
            invalidFunc: errs => errs.ToArray());

        // All errors from both validators are present
        Assert.True(allErrors.Length >= 4); // At least 4 errors total
    }

    #endregion

    #region Test: Cross-Team Consistency

    /// <summary>
    /// Demonstrates that different teams using the same patterns
    /// produce consistent, interoperable code.
    /// </summary>
    [Fact]
    public void CrossTeam_CodeIsInteroperable_WithConsistentPatterns()
    {
        // Team A's service returns Result<User, UserError>
        var teamAService = new TeamAUserService();

        // Team B's service returns Result<Order, OrderError>
        var teamBService = new TeamBOrderService();

        // Both can be composed naturally
        var result = teamAService.GetUser(1)
            .MapError(e => $"User error: {e}")
            .Bind(user => teamBService.GetOrders(user.Id)
                .MapError(e => $"Order error: {e}")
                .Map(orders => new UserWithOrders(user, orders)));

        Assert.True(result.IsOk);
        var userWithOrders = result.GetValue();
        Assert.Equal("Alice", userWithOrders.User.Name);
        Assert.NotEmpty(userWithOrders.Orders);
    }

    #endregion

    #region Helper Classes

    private class PaymentService
    {
        public Result<PaymentConfirmation, PaymentError> ProcessPayment(decimal amount)
        {
            if (amount <= 0)
                return Result<PaymentConfirmation, PaymentError>.Error(
                    new PaymentError("INVALID_AMOUNT", "Amount must be positive"));

            return Result<PaymentConfirmation, PaymentError>.Ok(
                new PaymentConfirmation($"txn-{Guid.NewGuid():N}", amount));
        }
    }

    public record PaymentConfirmation(string TransactionId, decimal Amount);
    public record PaymentError(string Code, string Message);

    private class UserRepository
    {
        private readonly Dictionary<int, User> _users = new()
        {
            [1] = new User(1, "Alice"),
            [2] = new User(2, "Bob")
        };

        public Option<User> FindById(int id)
        {
            return _users.TryGetValue(id, out var user)
                ? Option<User>.Some(user)
                : Option<User>.None();
        }
    }

    public record User(int Id, string Name);

    private class OrderOrchestrator
    {
        public Result<Order, OrderError> ProcessOrder(int customerId, string productId, int quantity)
        {
            return ValidateCustomer(customerId)
                .Bind(customer => ValidateProduct(productId)
                    .Map(product => (customer, product)))
                .Bind(tuple => CheckInventory(tuple.product, quantity)
                    .Map(stock => (tuple.customer, tuple.product, stock)))
                .Bind(tuple => ProcessPayment(tuple.customer)
                    .Map(payment => new Order(Guid.NewGuid(), tuple.customer.Id, tuple.product.Id, quantity)));
        }

        public Result<Order, OrderError> ProcessOrderWithLogging(
            int customerId, string productId, int quantity, List<string> callLog)
        {
            return ValidateCustomerWithLogging(customerId, callLog)
                .Bind(customer => ValidateProductWithLogging(productId, callLog)
                    .Map(product => (customer, product)))
                .Bind(tuple => CheckInventoryWithLogging(tuple.product, quantity, callLog)
                    .Map(stock => (tuple.customer, tuple.product, stock)))
                .Bind(tuple => ProcessPaymentWithLogging(tuple.customer, callLog)
                    .Map(payment => new Order(Guid.NewGuid(), tuple.customer.Id, tuple.product.Id, quantity)));
        }

        private Result<Customer, OrderError> ValidateCustomer(int customerId) =>
            customerId > 0
                ? Result<Customer, OrderError>.Ok(new Customer(customerId, $"Customer {customerId}"))
                : Result<Customer, OrderError>.Error(new OrderError.ValidationError("Invalid customer ID"));

        private Result<Customer, OrderError> ValidateCustomerWithLogging(int customerId, List<string> log)
        {
            log.Add("ValidateCustomer");
            return ValidateCustomer(customerId);
        }

        private Result<Product, OrderError> ValidateProduct(string productId) =>
            productId.StartsWith("product-")
                ? Result<Product, OrderError>.Ok(new Product(productId, $"Product {productId}", 10m))
                : Result<Product, OrderError>.Error(new OrderError.ValidationError("Invalid product ID"));

        private Result<Product, OrderError> ValidateProductWithLogging(string productId, List<string> log)
        {
            log.Add("ValidateProduct");
            return ValidateProduct(productId);
        }

        private Result<int, OrderError> CheckInventory(Product product, int quantity) =>
            product.Id.Contains("out-of-stock")
                ? Result<int, OrderError>.Error(new OrderError.InventoryError(quantity, 0))
                : Result<int, OrderError>.Ok(100);

        private Result<int, OrderError> CheckInventoryWithLogging(Product product, int quantity, List<string> log)
        {
            log.Add("CheckInventory");
            return CheckInventory(product, quantity);
        }

        private Result<string, OrderError> ProcessPayment(Customer customer) =>
            customer.Id == 999
                ? Result<string, OrderError>.Error(new OrderError.PaymentError("Insufficient funds"))
                : Result<string, OrderError>.Ok($"payment-{Guid.NewGuid():N}");

        private Result<string, OrderError> ProcessPaymentWithLogging(Customer customer, List<string> log)
        {
            log.Add("ProcessPayment");
            return ProcessPayment(customer);
        }
    }

    public record Customer(int Id, string Name);
    public record Product(string Id, string Name, decimal Price);
    public record Order(Guid Id, int CustomerId, string ProductId, int Quantity);

    public abstract record OrderError
    {
        public record ValidationError(string Message) : OrderError;
        public record InventoryError(int Requested, int Available) : OrderError;
        public record PaymentError(string Message) : OrderError;
    }

    public abstract record ApiError
    {
        public record NotFound(string ResourceType, int ResourceId) : ApiError;
        public record ValidationFailed(IEnumerable<string> Errors) : ApiError;
        public record Unauthorized(string Reason) : ApiError;
        public record BusinessRuleViolation(string Message) : ApiError;
    }

    private class ProductRepository
    {
        public Option<Product> FindById(int id) =>
            id > 0 && id < 100
                ? Option<Product>.Some(new Product($"product-{id}", $"Product {id}", id * 10m))
                : Option<Product>.None();
    }

    public abstract record ServiceError
    {
        public record NotFound(string Message) : ServiceError;
        public record ValidationFailed(string Message) : ServiceError;
    }

    private class ProductService
    {
        private readonly ProductRepository _repository;

        public ProductService(ProductRepository repository) => _repository = repository;

        public Result<Product, ServiceError> GetProduct(int id)
        {
            return _repository.FindById(id)
                .Match(
                    someFunc: product => Result<Product, ServiceError>.Ok(product),
                    noneFunc: () => Result<Product, ServiceError>.Error(
                        new ServiceError.NotFound($"Product {id} not found")));
        }
    }

    private class ProductController
    {
        private readonly ProductService _service;

        public ProductController(ProductService service) => _service = service;

        public HttpResponse GetProduct(int id)
        {
            return _service.GetProduct(id)
                .Match(
                    okFunc: product => new HttpResponse(200, product),
                    errFunc: error => error switch
                    {
                        ServiceError.NotFound => new HttpResponse(404, error),
                        _ => new HttpResponse(500, error)
                    });
        }
    }

    public record HttpResponse(int StatusCode, object Body);

    private class UserValidatorA
    {
        public Validation<ValidatedUser, string> Validate(string name, string email, int age)
        {
            var nameV = string.IsNullOrWhiteSpace(name)
                ? Validation<string, string>.Error("Name required")
                : Validation<string, string>.Ok(name);

            var emailV = !email.Contains('@')
                ? Validation<string, string>.Error("Invalid email")
                : Validation<string, string>.Ok(email);

            var ageV = age < 0
                ? Validation<int, string>.Error("Invalid age")
                : Validation<int, string>.Ok(age);

            return nameV
                .Apply(emailV, (n, e) => (n, e))
                .Apply(ageV, (t, a) => new ValidatedUser(t.n, t.e, a));
        }
    }

    private class UserValidatorB
    {
        public Validation<ValidatedUser, string> Validate(string name, string email, int age)
        {
            // Same pattern, different implementation - same result
            return ValidateName(name)
                .Apply(ValidateEmail(email), (n, e) => (n, e))
                .Apply(ValidateAge(age), (t, a) => new ValidatedUser(t.n, t.e, a));
        }

        private static Validation<string, string> ValidateName(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Validation<string, string>.Error("Name required")
                : Validation<string, string>.Ok(name);

        private static Validation<string, string> ValidateEmail(string email) =>
            !email.Contains('@')
                ? Validation<string, string>.Error("Invalid email")
                : Validation<string, string>.Ok(email);

        private static Validation<int, string> ValidateAge(int age) =>
            age < 0
                ? Validation<int, string>.Error("Invalid age")
                : Validation<int, string>.Ok(age);
    }

    public record ValidatedUser(string Name, string Email, int Age);

    private class AddressValidator
    {
        public Validation<ValidatedAddress, string> Validate(string street, string city, string zip)
        {
            var streetV = string.IsNullOrWhiteSpace(street)
                ? Validation<string, string>.Error("Street required")
                : Validation<string, string>.Ok(street);

            var cityV = string.IsNullOrWhiteSpace(city)
                ? Validation<string, string>.Error("City required")
                : Validation<string, string>.Ok(city);

            var zipV = string.IsNullOrWhiteSpace(zip)
                ? Validation<string, string>.Error("ZIP required")
                : Validation<string, string>.Ok(zip);

            return streetV
                .Apply(cityV, (s, c) => (s, c))
                .Apply(zipV, (t, z) => new ValidatedAddress(t.s, t.c, z));
        }
    }

    public record ValidatedAddress(string Street, string City, string Zip);

    private class PaymentValidator
    {
        public Validation<ValidatedPayment, string> Validate(string cardNumber, string expiry)
        {
            var cardV = string.IsNullOrWhiteSpace(cardNumber)
                ? Validation<string, string>.Error("Card number required")
                : Validation<string, string>.Ok(cardNumber);

            var expiryV = string.IsNullOrWhiteSpace(expiry)
                ? Validation<string, string>.Error("Expiry required")
                : Validation<string, string>.Ok(expiry);

            return cardV.Apply(expiryV, (c, e) => new ValidatedPayment(c, e));
        }
    }

    public record ValidatedPayment(string CardNumber, string Expiry);

    private class TeamAUserService
    {
        public Result<User, string> GetUser(int id) =>
            id > 0
                ? Result<User, string>.Ok(new User(id, "Alice"))
                : Result<User, string>.Error("User not found");
    }

    private class TeamBOrderService
    {
        public Result<List<TeamBOrder>, string> GetOrders(int userId) =>
            Result<List<TeamBOrder>, string>.Ok(new List<TeamBOrder>
            {
                new TeamBOrder(1, userId, 99.99m),
                new TeamBOrder(2, userId, 149.99m)
            });
    }

    public record TeamBOrder(int Id, int UserId, decimal Total);
    public record UserWithOrders(User User, List<TeamBOrder> Orders);

    #endregion
}
