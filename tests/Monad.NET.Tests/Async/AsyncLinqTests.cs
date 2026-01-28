namespace Monad.NET.Tests.Async;

public class AsyncLinqTests
{
    #region Option Async LINQ

    [Fact]
    public async Task Option_AsyncLinq_SelectMany_BothSome_ReturnsResult()
    {
        // Arrange
        var userTask = Task.FromResult(Option<int>.Some(1));

        // Act - Using LINQ query syntax
        var result = await (
            from userId in userTask
            from details in GetUserDetailsAsync(userId)
            select new { userId, details }
        );

        // Assert
        Assert.True(result.IsSome);
        var value = result.GetValue();
        Assert.Equal(1, value.userId);
        Assert.Equal("User 1", value.details);
    }

    [Fact]
    public async Task Option_AsyncLinq_SelectMany_FirstNone_ReturnsNone()
    {
        // Arrange
        var userTask = Task.FromResult(Option<int>.None());

        // Act
        var result = await (
            from userId in userTask
            from details in GetUserDetailsAsync(userId)
            select new { userId, details }
        );

        // Assert
        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task Option_AsyncLinq_SelectMany_SecondNone_ReturnsNone()
    {
        // Arrange
        var userTask = Task.FromResult(Option<int>.Some(999)); // Will return None from GetUserDetailsAsync

        // Act
        var result = await (
            from userId in userTask
            from details in GetUserDetailsAsync(userId) // Returns None for 999
            select new { userId, details }
        );

        // Assert
        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task Option_AsyncLinq_Select_TransformsValue()
    {
        // Arrange
        var task = Task.FromResult(Option<int>.Some(5));

        // Act
        var result = await (
            from x in task
            select x * 2
        );

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal(10, result.GetValue());
    }

    [Fact]
    public async Task Option_AsyncLinq_Where_FiltersValue()
    {
        // Arrange
        var task = Task.FromResult(Option<int>.Some(5));

        // Act
        var result = await task.Where(x => x > 3);

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal(5, result.GetValue());
    }

    [Fact]
    public async Task Option_AsyncLinq_Where_FiltersFalse_ReturnsNone()
    {
        // Arrange
        var task = Task.FromResult(Option<int>.Some(2));

        // Act
        var result = await task.Where(x => x > 3);

        // Assert
        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task Option_AsyncLinq_ChainedOperations()
    {
        // Arrange
        var configTask = Task.FromResult(Option<string>.Some("config"));

        // Act - Chaining multiple from clauses
        var result = await (
            from config in configTask
            from connection in GetConnectionAsync(config)
            from data in GetDataAsync(connection)
            select data.ToUpper()
        );

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal("DATA FOR CONNECTION_CONFIG", result.GetValue());
    }

    #endregion

    #region Result Async LINQ

    [Fact]
    public async Task Result_AsyncLinq_SelectMany_BothOk_ReturnsResult()
    {
        // Arrange
        var userTask = Task.FromResult(Result<int, string>.Ok(1));

        // Act
        var result = await (
            from userId in userTask
            from orders in GetOrdersAsync(userId)
            select new { userId, orders }
        );

        // Assert
        Assert.True(result.IsOk);
        var value = result.GetValue();
        Assert.Equal(1, value.userId);
        Assert.Equal(2, value.orders.Count);
    }

    [Fact]
    public async Task Result_AsyncLinq_SelectMany_FirstErr_ReturnsErr()
    {
        // Arrange
        var userTask = Task.FromResult(Result<int, string>.Err("User not found"));

        // Act
        var result = await (
            from userId in userTask
            from orders in GetOrdersAsync(userId)
            select new { userId, orders }
        );

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("User not found", result.GetError());
    }

    [Fact]
    public async Task Result_AsyncLinq_SelectMany_SecondErr_ReturnsErr()
    {
        // Arrange
        var userTask = Task.FromResult(Result<int, string>.Ok(999));

        // Act
        var result = await (
            from userId in userTask
            from orders in GetOrdersAsync(userId) // Returns Err for 999
            select new { userId, orders }
        );

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("No orders found", result.GetError());
    }

    [Fact]
    public async Task Result_AsyncLinq_Select_TransformsValue()
    {
        // Arrange
        var task = Task.FromResult(Result<int, string>.Ok(5));

        // Act
        var result = await (
            from x in task
            select x * 2
        );

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal(10, result.GetValue());
    }

    #endregion

    #region Try Async LINQ

    [Fact]
    public async Task Try_AsyncLinq_SelectMany_BothSuccess_ReturnsResult()
    {
        // Arrange
        var parseTask = Task.FromResult(Try<int>.Success(42));

        // Act
        var result = await (
            from number in parseTask
            from doubled in TryDoubleAsync(number)
            select doubled
        );

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public async Task Try_AsyncLinq_SelectMany_FirstFailure_ReturnsFailure()
    {
        // Arrange
        var parseTask = Task.FromResult(Try<int>.Failure(new InvalidOperationException("Parse failed")));

        // Act
        var result = await (
            from number in parseTask
            from doubled in TryDoubleAsync(number)
            select doubled
        );

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public async Task Try_AsyncLinq_Select_TransformsValue()
    {
        // Arrange
        var task = Task.FromResult(Try<int>.Success(5));

        // Act
        var result = await (
            from x in task
            select x * 2
        );

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal(10, result.GetValue());
    }

    #endregion

    #region Validation Async LINQ

    [Fact]
    public async Task Validation_AsyncLinq_SelectMany_BothValid_ReturnsResult()
    {
        // Arrange
        var nameTask = Task.FromResult(Validation<string, string>.Valid("John"));

        // Act
        var result = await (
            from name in nameTask
            from age in ValidateAgeAsync(25)
            select new { name, age }
        );

        // Assert
        Assert.True(result.IsOk);
        var value = result.GetValue();
        Assert.Equal("John", value.name);
        Assert.Equal(25, value.age);
    }

    [Fact]
    public async Task Validation_AsyncLinq_SelectMany_FirstInvalid_ReturnsErrors()
    {
        // Arrange
        var nameTask = Task.FromResult(Validation<string, string>.Invalid("Name required"));

        // Act
        var result = await (
            from name in nameTask
            from age in ValidateAgeAsync(25)
            select new { name, age }
        );

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("Name required", result.GetErrors());
    }

    [Fact]
    public async Task Validation_AsyncLinq_Select_TransformsValue()
    {
        // Arrange
        var task = Task.FromResult(Validation<int, string>.Valid(5));

        // Act
        var result = await (
            from x in task
            select x * 2
        );

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal(10, result.GetValue());
    }

    #endregion

    #region ValueTask LINQ

    [Fact]
    public async Task ValueTask_Option_SelectMany_Works()
    {
        // Arrange
        var source = new ValueTask<Option<int>>(Option<int>.Some(5));

        // Act
        var result = await (
            from x in source
            from y in GetValueTaskOptionAsync(x)
            select x + y
        );

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal(15, result.GetValue()); // 5 + 10
    }

    [Fact]
    public async Task ValueTask_Option_Select_Works()
    {
        // Arrange
        var source = new ValueTask<Option<int>>(Option<int>.Some(5));

        // Act
        var result = await source.Select(x => x * 2);

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal(10, result.GetValue());
    }

    [Fact]
    public async Task ValueTask_Option_Where_Works()
    {
        // Arrange
        var source = new ValueTask<Option<int>>(Option<int>.Some(5));

        // Act
        var result = await source.Where(x => x > 3);

        // Assert
        Assert.True(result.IsSome);
        Assert.Equal(5, result.GetValue());
    }

    [Fact]
    public async Task ValueTask_Result_SelectMany_Works()
    {
        // Arrange
        var source = new ValueTask<Result<int, string>>(Result<int, string>.Ok(5));

        // Act
        var result = await (
            from x in source
            from y in GetValueTaskResultAsync(x)
            select x + y
        );

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal(15, result.GetValue());
    }

    #endregion

    #region Helper Methods

    private static Task<Option<string>> GetUserDetailsAsync(int userId)
    {
        return Task.FromResult(
            userId == 999
                ? Option<string>.None()
                : Option<string>.Some($"User {userId}")
        );
    }

    private static Task<Option<string>> GetConnectionAsync(string config)
    {
        return Task.FromResult(Option<string>.Some($"Connection_{config}"));
    }

    private static Task<Option<string>> GetDataAsync(string connection)
    {
        return Task.FromResult(Option<string>.Some($"Data for {connection}"));
    }

    private static Task<Result<List<string>, string>> GetOrdersAsync(int userId)
    {
        return Task.FromResult(
            userId == 999
                ? Result<List<string>, string>.Err("No orders found")
                : Result<List<string>, string>.Ok(new List<string> { "Order1", "Order2" })
        );
    }

    private static Task<Try<int>> TryDoubleAsync(int value)
    {
        return Task.FromResult(Try<int>.Success(value * 2));
    }

    private static Task<Validation<int, string>> ValidateAgeAsync(int age)
    {
        return Task.FromResult(
            age < 0 || age > 150
                ? Validation<int, string>.Invalid("Invalid age")
                : Validation<int, string>.Valid(age)
        );
    }

    private static ValueTask<Option<int>> GetValueTaskOptionAsync(int value)
    {
        return new ValueTask<Option<int>>(Option<int>.Some(value * 2));
    }

    private static ValueTask<Result<int, string>> GetValueTaskResultAsync(int value)
    {
        return new ValueTask<Result<int, string>>(Result<int, string>.Ok(value * 2));
    }

    #endregion
}
