using Xunit;

namespace Monad.NET.RealWorld.Tests;

/// <summary>
/// These tests demonstrate the composability benefits of monadic types.
/// 
/// Composability means:
/// 1. Operations can be chained without intermediate variables
/// 2. Error handling is automatic through the chain
/// 3. Code reads like a specification of what should happen
/// 4. Individual steps can be tested in isolation
/// 5. Steps can be reordered or modified without breaking the chain
/// </summary>
public class ComposabilityTests
{
    #region Test: Pipeline Composition

    /// <summary>
    /// Demonstrates a realistic data transformation pipeline.
    /// Each step can fail, and failures automatically short-circuit.
    /// </summary>
    [Fact]
    public void Pipeline_ComposesMultipleSteps_WithAutomaticErrorPropagation()
    {
        // This pipeline:
        // 1. Parses raw input
        // 2. Validates the parsed data
        // 3. Transforms to domain model
        // 4. Enriches with external data
        // 5. Applies business rules

        var pipeline = new DataPipeline();

        // Success case - all steps pass
        var validResult = pipeline.Process(@"{""userId"": 123, ""amount"": 99.99}");
        Assert.True(validResult.IsOk);
        var enrichedOrder = validResult.GetValue();
        Assert.Equal(123, enrichedOrder.UserId);
        Assert.Equal("user-123@example.com", enrichedOrder.UserEmail);

        // Error case - validation fails mid-pipeline
        var invalidResult = pipeline.Process(@"{""userId"": 123, ""amount"": -50}");
        Assert.True(invalidResult.IsError);
        Assert.Equal("Amount must be positive", invalidResult.GetError());
    }

    /// <summary>
    /// Shows that individual pipeline steps are independently testable.
    /// This is a key maintainability benefit.
    /// </summary>
    [Fact]
    public void Pipeline_IndividualSteps_AreIndependentlyTestable()
    {
        var pipeline = new DataPipeline();

        // Test parsing step in isolation
        var parseResult = pipeline.Parse(@"{""userId"": 42, ""amount"": 10}");
        Assert.True(parseResult.IsOk);

        // Test validation step in isolation
        var validateResult = pipeline.Validate(new RawOrder(42, -10));
        Assert.True(validateResult.IsError);

        // Test transformation step in isolation
        var transformResult = pipeline.Transform(new RawOrder(42, 100));
        Assert.True(transformResult.IsOk);
    }

    #endregion

    #region Test: Optional Value Chains

    /// <summary>
    /// Demonstrates chaining through potentially missing values.
    /// This is the classic "null chain" problem solved elegantly.
    /// </summary>
    [Fact]
    public void OptionChain_NavigatesThroughOptionalValues_Safely()
    {
        var repository = new UserRepository();

        // Chain: User -> Settings -> NotificationPreferences -> EmailEnabled
        // Any step could return None

        var result = repository.FindUser(1)
            .Bind(user => user.Settings)
            .Bind(settings => settings.NotificationPreferences)
            .Map(prefs => prefs.EmailEnabled);

        Assert.True(result.IsSome);
        Assert.True(result.GetValue());
    }

    /// <summary>
    /// Shows safe navigation when intermediate values are missing.
    /// </summary>
    [Fact]
    public void OptionChain_ReturnsNone_WhenIntermediateValueMissing()
    {
        var repository = new UserRepository();

        // User 2 has no settings configured
        var result = repository.FindUser(2)
            .Bind(user => user.Settings)
            .Bind(settings => settings.NotificationPreferences)
            .Map(prefs => prefs.EmailEnabled);

        Assert.True(result.IsNone);
    }

    /// <summary>
    /// Demonstrates providing defaults at any point in the chain.
    /// </summary>
    [Fact]
    public void OptionChain_ProvidesDefaults_AtAnyPoint()
    {
        var repository = new UserRepository();

        // User doesn't exist, but we get a sensible default
        var emailEnabled = repository.FindUser(999)
            .Bind(user => user.Settings)
            .Bind(settings => settings.NotificationPreferences)
            .Map(prefs => prefs.EmailEnabled)
            .GetValueOr(false); // Default when user not found

        Assert.False(emailEnabled);
    }

    #endregion

    #region Test: Combining Multiple Independent Operations

    /// <summary>
    /// Demonstrates combining results of multiple independent operations.
    /// All operations run, and we get either all successes or the first failure.
    /// </summary>
    [Fact]
    public void ResultCombination_CombinesMultipleIndependentOperations()
    {
        var userService = new UserService();
        var orderService = new OrderService();
        var inventoryService = new InventoryService();

        // All three operations are independent - they can conceptually run in parallel
        var userResult = userService.GetUser(1);
        var orderResult = orderService.GetOrder(100);
        var inventoryResult = inventoryService.GetStock("product-1");

        // Combine them into a single result
        var combined = userResult
            .Bind(user => orderResult.Map(order => (user, order)))
            .Bind(tuple => inventoryResult.Map(stock => new OrderSummary(
                tuple.user.Name,
                tuple.order.Total,
                stock)));

        Assert.True(combined.IsOk);
        var summary = combined.GetValue();
        Assert.Equal("John Doe", summary.UserName);
        Assert.Equal(149.99m, summary.OrderTotal);
        Assert.Equal(50, summary.AvailableStock);
    }

    /// <summary>
    /// Shows that Validation accumulates ALL errors from independent operations.
    /// </summary>
    [Fact]
    public void ValidationCombination_AccumulatesAllErrors()
    {
        // All three validations fail
        var nameValidation = ValidateName("");
        var emailValidation = ValidateEmail("invalid");
        var ageValidation = ValidateAge(-5);

        var combined = nameValidation
            .Apply(emailValidation, (name, email) => (name, email))
            .Apply(ageValidation, (tuple, age) => new PersonDto(tuple.name, tuple.email, age));

        Assert.True(combined.IsError);

        var errors = new List<string>();
        combined.Match(
            validAction: _ => { },
            invalidAction: errs => errors.AddRange(errs));

        // All three errors are collected
        Assert.Equal(3, errors.Count);
        Assert.Contains("Name cannot be empty", errors);
        Assert.Contains("Invalid email format", errors);
        Assert.Contains("Age must be non-negative", errors);
    }

    #endregion

    #region Test: Conditional Composition

    /// <summary>
    /// Demonstrates conditional execution within a pipeline.
    /// </summary>
    [Fact]
    public void ConditionalComposition_ExecutesStepsConditionally()
    {
        var service = new PremiumService();

        // Premium users get extra features
        var premiumResult = service.GetUserFeatures(isPremium: true);
        Assert.True(premiumResult.IsOk);
        Assert.Contains("premium-analytics", premiumResult.GetValue().Features);

        // Regular users get basic features
        var regularResult = service.GetUserFeatures(isPremium: false);
        Assert.True(regularResult.IsOk);
        Assert.DoesNotContain("premium-analytics", regularResult.GetValue().Features);
    }

    /// <summary>
    /// Shows Filter as a conditional continue/stop mechanism.
    /// </summary>
    [Fact]
    public void FilterComposition_StopsOnPredicateFailure()
    {
        var orders = new[] { 100m, 250m, 50m, 300m };

        // Only process orders over $200
        var processed = orders
            .Select(amount => Option<decimal>.Some(amount))
            .Select(opt => opt.Filter(a => a > 200))
            .Where(opt => opt.IsSome)
            .Select(opt => opt.GetValue())
            .ToList();

        Assert.Equal(2, processed.Count);
        Assert.Contains(250m, processed);
        Assert.Contains(300m, processed);
    }

    #endregion

    #region Test: Side Effects in Composition

    /// <summary>
    /// Demonstrates Tap for logging/debugging without breaking the chain.
    /// </summary>
    [Fact]
    public void TapComposition_AllowsSideEffects_WithoutBreakingChain()
    {
        var logs = new List<string>();

        var result = Result<int, string>.Ok(10)
            .Tap(x => logs.Add($"Started with: {x}"))
            .Map(x => x * 2)
            .Tap(x => logs.Add($"After doubling: {x}"))
            .Map(x => x + 5)
            .Tap(x => logs.Add($"Final value: {x}"));

        Assert.Equal(25, result.GetValue());
        Assert.Equal(3, logs.Count);
        Assert.Equal("Started with: 10", logs[0]);
        Assert.Equal("After doubling: 20", logs[1]);
        Assert.Equal("Final value: 25", logs[2]);
    }

    /// <summary>
    /// Shows TapError for error logging without changing the error.
    /// </summary>
    [Fact]
    public void TapErrorComposition_LogsErrors_WithoutChangingThem()
    {
        var errorLogs = new List<string>();

        var result = Result<int, string>.Error("Something went wrong")
            .TapError(e => errorLogs.Add($"Error occurred: {e}"))
            .Map(x => x * 2) // Never executed
            .TapError(e => errorLogs.Add($"Still failing: {e}"));

        Assert.True(result.IsError);
        Assert.Equal(2, errorLogs.Count);
    }

    #endregion

    #region Helper Classes

    private class DataPipeline
    {
        public Result<EnrichedOrder, string> Process(string rawJson)
        {
            return Parse(rawJson)
                .Bind(Validate)
                .Bind(Transform)
                .Bind(Enrich)
                .Bind(ApplyBusinessRules);
        }

        public Result<RawOrder, string> Parse(string json)
        {
            try
            {
                // Simplified parsing
                if (json.Contains("userId") && json.Contains("amount"))
                {
                    var userId = int.Parse(json.Split("userId")[1].Split(':')[1].Split(',')[0].Trim().Trim('"'));
                    var amount = decimal.Parse(json.Split("amount")[1].Split(':')[1].Split('}')[0].Trim().Trim('"'));
                    return Result<RawOrder, string>.Ok(new RawOrder(userId, amount));
                }
                return Result<RawOrder, string>.Error("Invalid JSON format");
            }
            catch
            {
                return Result<RawOrder, string>.Error("Failed to parse JSON");
            }
        }

        public Result<RawOrder, string> Validate(RawOrder order)
        {
            if (order.UserId <= 0)
                return Result<RawOrder, string>.Error("Invalid user ID");
            if (order.Amount <= 0)
                return Result<RawOrder, string>.Error("Amount must be positive");
            return Result<RawOrder, string>.Ok(order);
        }

        public Result<DomainOrder, string> Transform(RawOrder raw)
        {
            return Result<DomainOrder, string>.Ok(
                new DomainOrder(raw.UserId, raw.Amount, DateTime.UtcNow));
        }

        public Result<EnrichedOrder, string> Enrich(DomainOrder order)
        {
            // Simulate enrichment with user email
            var email = $"user-{order.UserId}@example.com";
            return Result<EnrichedOrder, string>.Ok(
                new EnrichedOrder(order.UserId, email, order.Amount, order.CreatedAt));
        }

        public Result<EnrichedOrder, string> ApplyBusinessRules(EnrichedOrder order)
        {
            if (order.Amount > 10000)
                return Result<EnrichedOrder, string>.Error("Order exceeds maximum allowed amount");
            return Result<EnrichedOrder, string>.Ok(order);
        }
    }

    public record RawOrder(int UserId, decimal Amount);
    public record DomainOrder(int UserId, decimal Amount, DateTime CreatedAt);
    public record EnrichedOrder(int UserId, string UserEmail, decimal Amount, DateTime CreatedAt);

    private class UserRepository
    {
        private readonly Dictionary<int, User> _users = new()
        {
            [1] = new User(1, "Alice", Option<UserSettings>.Some(
                new UserSettings(Option<NotificationPreferences>.Some(
                    new NotificationPreferences(true, false))))),
            [2] = new User(2, "Bob", Option<UserSettings>.None()) // No settings
        };

        public Option<User> FindUser(int id)
        {
            return _users.TryGetValue(id, out var user)
                ? Option<User>.Some(user)
                : Option<User>.None();
        }
    }

    public record User(int Id, string Name, Option<UserSettings> Settings);
    public record UserSettings(Option<NotificationPreferences> NotificationPreferences);
    public record NotificationPreferences(bool EmailEnabled, bool SmsEnabled);

    private class UserService
    {
        public Result<UserDto, string> GetUser(int id) =>
            Result<UserDto, string>.Ok(new UserDto(id, "John Doe"));
    }

    private class OrderService
    {
        public Result<OrderDto, string> GetOrder(int id) =>
            Result<OrderDto, string>.Ok(new OrderDto(id, 149.99m));
    }

    private class InventoryService
    {
        public Result<int, string> GetStock(string productId) =>
            Result<int, string>.Ok(50);
    }

    public record UserDto(int Id, string Name);
    public record OrderDto(int Id, decimal Total);
    public record OrderSummary(string UserName, decimal OrderTotal, int AvailableStock);

    private static Validation<string, string> ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? Validation<string, string>.Error("Name cannot be empty")
            : Validation<string, string>.Ok(name);

    private static Validation<string, string> ValidateEmail(string email) =>
        !email.Contains('@')
            ? Validation<string, string>.Error("Invalid email format")
            : Validation<string, string>.Ok(email);

    private static Validation<int, string> ValidateAge(int age) =>
        age < 0
            ? Validation<int, string>.Error("Age must be non-negative")
            : Validation<int, string>.Ok(age);

    public record PersonDto(string Name, string Email, int Age);

    private class PremiumService
    {
        public Result<FeatureSet, string> GetUserFeatures(bool isPremium)
        {
            var baseFeatures = new List<string> { "basic-dashboard", "reports" };

            return Result<List<string>, string>.Ok(baseFeatures)
                .Map(features => isPremium
                    ? features.Concat(new[] { "premium-analytics", "api-access" }).ToList()
                    : features)
                .Map(features => new FeatureSet(features));
        }
    }

    public record FeatureSet(List<string> Features);

    #endregion
}
