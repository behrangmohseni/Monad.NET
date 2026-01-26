using Xunit;

namespace Monad.NET.Tests;

public class ResultCombineTests
{
    #region Combine Two Results

    [Fact]
    public void Combine_TwoOk_ReturnsTuple()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);

        var combined = ResultExtensions.Combine(r1, r2);

        Assert.True(combined.IsOk);
        Assert.Equal((1, 2), combined.GetValue());
    }

    [Fact]
    public void Combine_FirstErr_ReturnsFirstError()
    {
        var r1 = Result<int, string>.Err("error1");
        var r2 = Result<int, string>.Ok(2);

        var combined = ResultExtensions.Combine(r1, r2);

        Assert.True(combined.IsErr);
        Assert.Equal("error1", combined.GetError());
    }

    [Fact]
    public void Combine_SecondErr_ReturnsSecondError()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Err("error2");

        var combined = ResultExtensions.Combine(r1, r2);

        Assert.True(combined.IsErr);
        Assert.Equal("error2", combined.GetError());
    }

    [Fact]
    public void Combine_BothErr_ReturnsFirstError()
    {
        var r1 = Result<int, string>.Err("error1");
        var r2 = Result<int, string>.Err("error2");

        var combined = ResultExtensions.Combine(r1, r2);

        Assert.True(combined.IsErr);
        Assert.Equal("error1", combined.GetError());
    }

    [Fact]
    public void Combine_TwoOk_WithCombiner_ReturnsResult()
    {
        var r1 = Result<int, string>.Ok(10);
        var r2 = Result<int, string>.Ok(20);

        var combined = ResultExtensions.Combine(r1, r2, (a, b) => a + b);

        Assert.True(combined.IsOk);
        Assert.Equal(30, combined.GetValue());
    }

    [Fact]
    public void Combine_WithCombiner_FirstErr_ReturnsError()
    {
        var r1 = Result<int, string>.Err("error");
        var r2 = Result<int, string>.Ok(20);

        var combined = ResultExtensions.Combine(r1, r2, (a, b) => a + b);

        Assert.True(combined.IsErr);
        Assert.Equal("error", combined.GetError());
    }

    #endregion

    #region Combine Three Results

    [Fact]
    public void Combine_ThreeOk_ReturnsTuple()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<string, string>.Ok("hello");
        var r3 = Result<double, string>.Ok(3.14);

        var combined = ResultExtensions.Combine(r1, r2, r3);

        Assert.True(combined.IsOk);
        Assert.Equal((1, "hello", 3.14), combined.GetValue());
    }

    [Fact]
    public void Combine_ThirdErr_ReturnsThirdError()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<string, string>.Ok("hello");
        var r3 = Result<double, string>.Err("error3");

        var combined = ResultExtensions.Combine(r1, r2, r3);

        Assert.True(combined.IsErr);
        Assert.Equal("error3", combined.GetError());
    }

    [Fact]
    public void Combine_ThreeOk_WithCombiner_ReturnsResult()
    {
        var r1 = Result<string, string>.Ok("Hello");
        var r2 = Result<string, string>.Ok(" ");
        var r3 = Result<string, string>.Ok("World");

        var combined = ResultExtensions.Combine(r1, r2, r3, (a, b, c) => a + b + c);

        Assert.True(combined.IsOk);
        Assert.Equal("Hello World", combined.GetValue());
    }

    #endregion

    #region Combine Four Results

    [Fact]
    public void Combine_FourOk_ReturnsTuple()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Ok(3);
        var r4 = Result<int, string>.Ok(4);

        var combined = ResultExtensions.Combine(r1, r2, r3, r4);

        Assert.True(combined.IsOk);
        Assert.Equal((1, 2, 3, 4), combined.GetValue());
    }

    [Fact]
    public void Combine_FourthErr_ReturnsFourthError()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Ok(3);
        var r4 = Result<int, string>.Err("error4");

        var combined = ResultExtensions.Combine(r1, r2, r3, r4);

        Assert.True(combined.IsErr);
        Assert.Equal("error4", combined.GetError());
    }

    [Fact]
    public void Combine_FourOk_WithCombiner_ReturnsResult()
    {
        var r1 = Result<int, string>.Ok(1);
        var r2 = Result<int, string>.Ok(2);
        var r3 = Result<int, string>.Ok(3);
        var r4 = Result<int, string>.Ok(4);

        var combined = ResultExtensions.Combine(r1, r2, r3, r4, (a, b, c, d) => a + b + c + d);

        Assert.True(combined.IsOk);
        Assert.Equal(10, combined.GetValue());
    }

    #endregion

    #region Combine Collection

    [Fact]
    public void Combine_Collection_AllOk_ReturnsList()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.Combine(results);

        Assert.True(combined.IsOk);
        Assert.Equal(new[] { 1, 2, 3 }, combined.GetValue());
    }

    [Fact]
    public void Combine_Collection_OneErr_ReturnsError()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.Combine(results);

        Assert.True(combined.IsErr);
        Assert.Equal("error", combined.GetError());
    }

    [Fact]
    public void Combine_Collection_Empty_ReturnsEmptyList()
    {
        var results = Array.Empty<Result<int, string>>();

        var combined = ResultExtensions.Combine(results);

        Assert.True(combined.IsOk);
        Assert.Empty(combined.GetValue());
    }

    [Fact]
    public void Combine_Collection_FirstErr_ReturnsFirstError()
    {
        var results = new[]
        {
            Result<int, string>.Err("first"),
            Result<int, string>.Err("second"),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.Combine(results);

        Assert.True(combined.IsErr);
        Assert.Equal("first", combined.GetError());
    }

    #endregion

    #region CombineAll

    [Fact]
    public void CombineAll_AllOk_ReturnsUnit()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.CombineAll(results);

        Assert.True(combined.IsOk);
        Assert.Equal(Unit.Value, combined.GetValue());
    }

    [Fact]
    public void CombineAll_OneErr_ReturnsError()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("validation failed"),
            Result<int, string>.Ok(3)
        };

        var combined = ResultExtensions.CombineAll(results);

        Assert.True(combined.IsErr);
        Assert.Equal("validation failed", combined.GetError());
    }

    [Fact]
    public void CombineAll_Empty_ReturnsUnit()
    {
        var results = Array.Empty<Result<int, string>>();

        var combined = ResultExtensions.CombineAll(results);

        Assert.True(combined.IsOk);
    }

    #endregion

    #region Unit Type

    [Fact]
    public void Unit_Equality()
    {
        var u1 = Unit.Value;
        var u2 = Unit.Value;

        Assert.True(u1.Equals(u2));
        Assert.True(u1 == u2);
        Assert.False(u1 != u2);
        Assert.Equal(u1.GetHashCode(), u2.GetHashCode());
    }

    [Fact]
    public void Unit_ToString_ReturnsParens()
    {
        Assert.Equal("()", Unit.Value.ToString());
    }

    [Fact]
    public void Unit_Equals_Object()
    {
        object obj = Unit.Value;
        Assert.True(Unit.Value.Equals(obj));
        Assert.False(Unit.Value.Equals("not unit"));
        Assert.False(Unit.Value.Equals(null));
    }

    #endregion

    #region Real-World Examples

    [Fact]
    public void RealWorld_CombineUserAndOrder()
    {
        // Simulate fetching user and order
        Result<string, string> GetUser(int id) =>
            id > 0 ? Result<string, string>.Ok($"User{id}") : Result<string, string>.Err("User not found");

        Result<string, string> GetOrder(int id) =>
            id > 0 ? Result<string, string>.Ok($"Order{id}") : Result<string, string>.Err("Order not found");

        // Combine both
        var result = ResultExtensions.Combine(
            GetUser(1),
            GetOrder(100),
            (user, order) => $"{user} placed {order}"
        );

        Assert.True(result.IsOk);
        Assert.Equal("User1 placed Order100", result.GetValue());
    }

    [Fact]
    public void RealWorld_ValidateMultipleFields()
    {
        Result<string, string> ValidateName(string name) =>
            string.IsNullOrEmpty(name) ? Result<string, string>.Err("Name required") : Result<string, string>.Ok(name);

        Result<int, string> ValidateAge(int age) =>
            age < 0 ? Result<int, string>.Err("Age invalid") : Result<int, string>.Ok(age);

        Result<string, string> ValidateEmail(string email) =>
            email.Contains('@') ? Result<string, string>.Ok(email) : Result<string, string>.Err("Invalid email");

        // All valid
        var valid = ResultExtensions.Combine(
            ValidateName("John"),
            ValidateAge(25),
            ValidateEmail("john@example.com")
        );
        Assert.True(valid.IsOk);
        Assert.Equal(("John", 25, "john@example.com"), valid.GetValue());

        // One invalid
        var invalid = ResultExtensions.Combine(
            ValidateName("John"),
            ValidateAge(-5),
            ValidateEmail("john@example.com")
        );
        Assert.True(invalid.IsErr);
        Assert.Equal("Age invalid", invalid.GetError());
    }

    [Fact]
    public void RealWorld_BatchOperation()
    {
        Result<int, string> Process(int x) =>
            x >= 0 ? Result<int, string>.Ok(x * 2) : Result<int, string>.Err($"Cannot process {x}");

        var inputs = new[] { 1, 2, 3, 4, 5 };
        var results = ResultExtensions.Combine(inputs.Select(Process));

        Assert.True(results.IsOk);
        Assert.Equal(new[] { 2, 4, 6, 8, 10 }, results.GetValue());
    }

    #endregion
}

