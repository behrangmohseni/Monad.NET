using Xunit;

namespace Monad.NET.RealWorld.Tests;

/// <summary>
/// These tests demonstrate the practical differences between Result-based
/// error handling and exception-based error handling in real-world scenarios.
/// 
/// Key metrics we're comparing:
/// 1. Error visibility in method signatures
/// 2. Composability of error handling
/// 3. Behavior consistency
/// 4. Testing ease
/// </summary>
public class ResultVsExceptionsTests
{
    #region Scenario: Order Processing Pipeline

    /// <summary>
    /// EXCEPTION-BASED APPROACH
    /// 
    /// Problems demonstrated:
    /// - Caller doesn't know what exceptions to catch (hidden contract)
    /// - Multiple catch blocks needed
    /// - Easy to forget error handling
    /// - Control flow via exceptions is expensive
    /// </summary>
    [Fact]
    public void ExceptionBased_OrderProcessing_RequiresMultipleCatchBlocks()
    {
        var service = new ExceptionBasedOrderService();
        var errors = new List<string>();

        // Caller must GUESS what exceptions might be thrown
        try
        {
            var order = service.ProcessOrder("invalid-customer", "product-1", 5);
        }
        catch (CustomerNotFoundException ex)
        {
            errors.Add(ex.Message);
        }
        catch (ProductNotFoundException ex)
        {
            errors.Add(ex.Message);
        }
        catch (InsufficientInventoryException ex)
        {
            errors.Add(ex.Message);
        }
        catch (PaymentFailedException ex)
        {
            errors.Add(ex.Message);
        }
        // What if there are other exceptions? They propagate silently!

        Assert.Single(errors);
        Assert.Contains("Customer not found", errors[0]);
    }

    /// <summary>
    /// RESULT-BASED APPROACH
    /// 
    /// Benefits demonstrated:
    /// - Error types are explicit in the signature
    /// - No try/catch needed
    /// - Compiler ensures all errors are handled
    /// - Easy to compose with other operations
    /// </summary>
    [Fact]
    public void ResultBased_OrderProcessing_ErrorsAreExplicitInSignature()
    {
        var service = new ResultBasedOrderService();

        // The return type TELLS us exactly what can go wrong
        Result<Order, OrderError> result = service.ProcessOrder("invalid-customer", "product-1", 5);

        // Pattern matching ensures we handle both cases
        var message = result.Match(
            okFunc: order => $"Order {order.Id} created",
            errFunc: error => error switch
            {
                OrderError.CustomerNotFound e => $"Customer error: {e.CustomerId}",
                OrderError.ProductNotFound e => $"Product error: {e.ProductId}",
                OrderError.InsufficientInventory e => $"Inventory error: need {e.Required}, have {e.Available}",
                OrderError.PaymentFailed e => $"Payment error: {e.Reason}",
                _ => "Unknown error"
            }
        );

        Assert.Contains("Customer error", message);
    }

    /// <summary>
    /// Demonstrates that Result-based code composes naturally with LINQ-like operations.
    /// Exception-based code cannot be composed this way without try/catch everywhere.
    /// </summary>
    [Fact]
    public void ResultBased_Composability_ChainsNaturally()
    {
        var service = new ResultBasedOrderService();

        // Compose a complex pipeline - errors short-circuit automatically
        var result = service.ValidateCustomer("customer-123")
            .Bind(customer => service.ValidateProduct("product-456")
                .Map(product => (customer, product)))
            .Bind(tuple => service.CheckInventory(tuple.product, 5)
                .Map(inventory => (tuple.customer, tuple.product, inventory)))
            .Bind(tuple => service.ProcessPayment(tuple.customer, 99.99m)
                .Map(payment => new Order(Guid.NewGuid(), tuple.customer, tuple.product, 5, 99.99m)));

        Assert.True(result.IsOk);
    }

    /// <summary>
    /// Exception-based equivalent requires nested try/catch - much harder to read and maintain.
    /// </summary>
    [Fact]
    public void ExceptionBased_Composability_RequiresNestedTryCatch()
    {
        var service = new ExceptionBasedOrderService();
        Order? order = null;
        string? error = null;

        // This is what composed exception handling looks like - ugly!
        try
        {
            var customer = service.ValidateCustomer("customer-123");
            try
            {
                var product = service.ValidateProduct("product-456");
                try
                {
                    var inventory = service.CheckInventory(product, 5);
                    try
                    {
                        var payment = service.ProcessPayment(customer, 99.99m);
                        order = new Order(Guid.NewGuid(), customer, product, 5, 99.99m);
                    }
                    catch (PaymentFailedException ex) { error = ex.Message; }
                }
                catch (InsufficientInventoryException ex) { error = ex.Message; }
            }
            catch (ProductNotFoundException ex) { error = ex.Message; }
        }
        catch (CustomerNotFoundException ex) { error = ex.Message; }

        Assert.NotNull(order);
        Assert.Null(error);
    }

    #endregion

    #region Scenario: Validation with Multiple Errors

    /// <summary>
    /// Exception-based validation stops at first error.
    /// Users hate this - they want to see ALL errors at once.
    /// </summary>
    [Fact]
    public void ExceptionBased_Validation_StopsAtFirstError()
    {
        var validator = new ExceptionBasedValidator();
        var errors = new List<string>();

        // Empty name, invalid email, underage - but we only see ONE error
        try
        {
            validator.ValidateUser("", "not-an-email", 15);
        }
        catch (ValidationException ex)
        {
            errors.Add(ex.Message);
        }

        // User only sees "Name is required" - frustrating!
        Assert.Single(errors);
        Assert.Equal("Name is required", errors[0]);
    }

    /// <summary>
    /// Validation<T,E> accumulates ALL errors - much better UX.
    /// </summary>
    [Fact]
    public void ValidationMonad_AccumulatesAllErrors()
    {
        var validator = new MonadicValidator();

        var result = validator.ValidateUser("", "not-an-email", 15);

        // User sees ALL three errors at once!
        Assert.True(result.IsError);
        var errors = result.Match(
            validFunc: _ => Array.Empty<string>(),
            invalidFunc: errs => errs.ToArray()
        );

        Assert.Equal(3, errors.Length);
        Assert.Contains("Name is required", errors);
        Assert.Contains("Invalid email format", errors);
        Assert.Contains("Must be at least 18 years old", errors);
    }

    #endregion

    #region Scenario: Error Recovery

    /// <summary>
    /// Result enables elegant fallback chains without try/catch nesting.
    /// </summary>
    [Fact]
    public void ResultBased_ErrorRecovery_ChainsElegantly()
    {
        var dataService = new ResultBasedDataService();

        // Try primary, fall back to secondary, fall back to cache
        var result = dataService.FetchFromPrimary()
            .OrElse(_ => dataService.FetchFromSecondary())
            .OrElse(_ => dataService.FetchFromCache());

        Assert.True(result.IsOk);
        Assert.Equal("cached-data", result.GetValue());
    }

    /// <summary>
    /// Exception-based fallback requires nested try/catch.
    /// </summary>
    [Fact]
    public void ExceptionBased_ErrorRecovery_RequiresNestedTryCatch()
    {
        var dataService = new ExceptionBasedDataService();
        string? data = null;

        try
        {
            data = dataService.FetchFromPrimary();
        }
        catch
        {
            try
            {
                data = dataService.FetchFromSecondary();
            }
            catch
            {
                try
                {
                    data = dataService.FetchFromCache();
                }
                catch
                {
                    data = "default";
                }
            }
        }

        Assert.Equal("cached-data", data);
    }

    #endregion

    #region Scenario: Testing

    /// <summary>
    /// Result-based code is trivial to test - just check the result value.
    /// No need to verify exception types or messages.
    /// </summary>
    [Fact]
    public void ResultBased_Testing_IsSimple()
    {
        var service = new ResultBasedOrderService();

        // Test success case
        var successResult = service.ValidateCustomer("valid-customer");
        Assert.True(successResult.IsOk);
        Assert.Equal("valid-customer", successResult.GetValue().Id);

        // Test error case - no exception handling needed
        var errorResult = service.ValidateCustomer("invalid-customer");
        Assert.True(errorResult.IsError);
        Assert.IsType<OrderError.CustomerNotFound>(errorResult.GetError());
    }

    /// <summary>
    /// Exception-based testing requires Assert.Throws and exception type knowledge.
    /// </summary>
    [Fact]
    public void ExceptionBased_Testing_RequiresExceptionAssertions()
    {
        var service = new ExceptionBasedOrderService();

        // Test success case
        var customer = service.ValidateCustomer("valid-customer");
        Assert.Equal("valid-customer", customer.Id);

        // Test error case - must know exact exception type
        var ex = Assert.Throws<CustomerNotFoundException>(
            () => service.ValidateCustomer("invalid-customer"));
        Assert.Contains("not found", ex.Message);
    }

    #endregion

    #region Helper Classes - Exception-Based

    private class ExceptionBasedOrderService
    {
        public Order ProcessOrder(string customerId, string productId, int quantity)
        {
            var customer = ValidateCustomer(customerId);
            var product = ValidateProduct(productId);
            CheckInventory(product, quantity);
            ProcessPayment(customer, quantity * 10m);
            return new Order(Guid.NewGuid(), customer, product, quantity, quantity * 10m);
        }

        public Customer ValidateCustomer(string customerId)
        {
            if (customerId.StartsWith("invalid"))
                throw new CustomerNotFoundException($"Customer not found: {customerId}");
            return new Customer(customerId, $"Customer {customerId}");
        }

        public Product ValidateProduct(string productId)
        {
            if (productId.StartsWith("invalid"))
                throw new ProductNotFoundException($"Product not found: {productId}");
            return new Product(productId, $"Product {productId}", 10m);
        }

        public int CheckInventory(Product product, int required)
        {
            if (product.Id.Contains("out-of-stock"))
                throw new InsufficientInventoryException(required, 0);
            return 100;
        }

        public string ProcessPayment(Customer customer, decimal amount)
        {
            if (customer.Id.Contains("no-funds"))
                throw new PaymentFailedException("Insufficient funds");
            return $"payment-{Guid.NewGuid():N}";
        }
    }

    private class ExceptionBasedValidator
    {
        public void ValidateUser(string name, string email, int age)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Name is required");
            if (!email.Contains('@'))
                throw new ValidationException("Invalid email format");
            if (age < 18)
                throw new ValidationException("Must be at least 18 years old");
        }
    }

    private class ExceptionBasedDataService
    {
        public string FetchFromPrimary() => throw new DataSourceException("Primary unavailable");
        public string FetchFromSecondary() => throw new DataSourceException("Secondary unavailable");
        public string FetchFromCache() => "cached-data";
    }

    #endregion

    #region Helper Classes - Result-Based

    private class ResultBasedOrderService
    {
        public Result<Order, OrderError> ProcessOrder(string customerId, string productId, int quantity)
        {
            return ValidateCustomer(customerId)
                .Bind(customer => ValidateProduct(productId)
                    .Map(product => (customer, product)))
                .Bind(tuple => CheckInventory(tuple.product, quantity)
                    .Map(inv => (tuple.customer, tuple.product, inv)))
                .Bind(tuple => ProcessPayment(tuple.customer, quantity * 10m)
                    .Map(payment => new Order(Guid.NewGuid(), tuple.customer, tuple.product, quantity, quantity * 10m)));
        }

        public Result<Customer, OrderError> ValidateCustomer(string customerId)
        {
            if (customerId.StartsWith("invalid"))
                return Result<Customer, OrderError>.Error(new OrderError.CustomerNotFound(customerId));
            return Result<Customer, OrderError>.Ok(new Customer(customerId, $"Customer {customerId}"));
        }

        public Result<Product, OrderError> ValidateProduct(string productId)
        {
            if (productId.StartsWith("invalid"))
                return Result<Product, OrderError>.Error(new OrderError.ProductNotFound(productId));
            return Result<Product, OrderError>.Ok(new Product(productId, $"Product {productId}", 10m));
        }

        public Result<int, OrderError> CheckInventory(Product product, int required)
        {
            if (product.Id.Contains("out-of-stock"))
                return Result<int, OrderError>.Error(new OrderError.InsufficientInventory(required, 0));
            return Result<int, OrderError>.Ok(100);
        }

        public Result<string, OrderError> ProcessPayment(Customer customer, decimal amount)
        {
            if (customer.Id.Contains("no-funds"))
                return Result<string, OrderError>.Error(new OrderError.PaymentFailed("Insufficient funds"));
            return Result<string, OrderError>.Ok($"payment-{Guid.NewGuid():N}");
        }
    }

    private class MonadicValidator
    {
        public Validation<ValidatedUser, string> ValidateUser(string name, string email, int age)
        {
            var nameValidation = string.IsNullOrWhiteSpace(name)
                ? Validation<string, string>.Error("Name is required")
                : Validation<string, string>.Ok(name);

            var emailValidation = !email.Contains('@')
                ? Validation<string, string>.Error("Invalid email format")
                : Validation<string, string>.Ok(email);

            var ageValidation = age < 18
                ? Validation<int, string>.Error("Must be at least 18 years old")
                : Validation<int, string>.Ok(age);

            return nameValidation
                .Apply(emailValidation, (n, e) => (n, e))
                .Apply(ageValidation, (tuple, a) => new ValidatedUser(tuple.n, tuple.e, a));
        }
    }

    private class ResultBasedDataService
    {
        public Result<string, string> FetchFromPrimary() =>
            Result<string, string>.Error("Primary unavailable");

        public Result<string, string> FetchFromSecondary() =>
            Result<string, string>.Error("Secondary unavailable");

        public Result<string, string> FetchFromCache() =>
            Result<string, string>.Ok("cached-data");
    }

    #endregion

    #region Domain Models

    public record Customer(string Id, string Name);
    public record Product(string Id, string Name, decimal Price);
    public record Order(Guid Id, Customer Customer, Product Product, int Quantity, decimal Total);
    public record ValidatedUser(string Name, string Email, int Age);

    public abstract record OrderError
    {
        public record CustomerNotFound(string CustomerId) : OrderError;
        public record ProductNotFound(string ProductId) : OrderError;
        public record InsufficientInventory(int Required, int Available) : OrderError;
        public record PaymentFailed(string Reason) : OrderError;
    }

    #endregion

    #region Exception Types

    public class CustomerNotFoundException(string message) : Exception(message);
    public class ProductNotFoundException(string message) : Exception(message);
    public class InsufficientInventoryException(int required, int available)
        : Exception($"Need {required}, have {available}");
    public class PaymentFailedException(string message) : Exception(message);
    public class ValidationException(string message) : Exception(message);
    public class DataSourceException(string message) : Exception(message);

    #endregion
}
