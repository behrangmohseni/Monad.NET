using Monad.NET;

namespace Monad.NET.Tests;

public class LinqTests
{
    #region Option LINQ Tests

    [Fact]
    public void Option_Select_TransformsValue()
    {
        var result = from x in Option<int>.Some(42)
                     select x * 2;

        Assert.True(result.IsSome);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public void Option_SelectMany_ChainsOperations()
    {
        var result = from x in Option<int>.Some(10)
                     from y in Option<int>.Some(20)
                     select x + y;

        Assert.True(result.IsSome);
        Assert.Equal(30, result.Unwrap());
    }

    [Fact]
    public void Option_SelectMany_WithNone_ReturnsNone()
    {
        var result = from x in Option<int>.Some(10)
                     from y in Option<int>.None()
                     select x + y;

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_Where_FiltersValues()
    {
        var result = from x in Option<int>.Some(42)
                     where x > 40
                     select x;

        Assert.True(result.IsSome);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Option_Where_WithFailingPredicate_ReturnsNone()
    {
        var result = from x in Option<int>.Some(42)
                     where x < 40
                     select x;

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_ComplexQuery_WorksCorrectly()
    {
        var result = from x in Option<int>.Some(10)
                     where x > 5
                     from y in Option<int>.Some(20)
                     where y > 15
                     select x * y;

        Assert.True(result.IsSome);
        Assert.Equal(200, result.Unwrap());
    }

    [Fact]
    public void Option_QueryWithLet_WorksCorrectly()
    {
        var result = from x in Option<int>.Some(5)
                     let doubled = x * 2
                     from y in Option<int>.Some(3)
                     select doubled + y;

        Assert.True(result.IsSome);
        Assert.Equal(13, result.Unwrap());
    }

    #endregion

    #region Result LINQ Tests

    [Fact]
    public void Result_Select_TransformsOkValue()
    {
        var result = from x in Result<int, string>.Ok(42)
                     select x * 2;

        Assert.True(result.IsOk);
        Assert.Equal(84, result.Unwrap());
    }

    [Fact]
    public void Result_Select_OnErr_ReturnsErr()
    {
        var result = from x in Result<int, string>.Err("error")
                     select x * 2;

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public void Result_SelectMany_ChainsOperations()
    {
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(30, result.Unwrap());
    }

    [Fact]
    public void Result_SelectMany_WithErr_ReturnsErr()
    {
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Err("error")
                     select x + y;

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
    }

    [Fact]
    public void Result_Where_FiltersValues()
    {
        var okResult = Result<int, string>.Ok(42);
        var result = okResult.Where(x => x > 40, "too small");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.Unwrap());
    }

    [Fact]
    public void Result_Where_WithFailingPredicate_ReturnsErr()
    {
        var okResult = Result<int, string>.Ok(42);
        var result = okResult.Where(x => x < 40, "too large");

        Assert.True(result.IsErr);
        Assert.Equal("too large", result.UnwrapErr());
    }

    [Fact]
    public void Result_Where_WithErrorFactory_WorksCorrectly()
    {
        var okResult = Result<int, string>.Ok(30);
        var result = okResult.Where(
            x => x >= 40,
            x => $"Value {x} is too small"
        );

        Assert.True(result.IsErr);
        Assert.Equal("Value 30 is too small", result.UnwrapErr());
    }

    [Fact]
    public void Result_ComplexQuery_WorksCorrectly()
    {
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Ok(20)
                     from z in Result<int, string>.Ok(30)
                     select x + y + z;

        Assert.True(result.IsOk);
        Assert.Equal(60, result.Unwrap());
    }

    [Fact]
    public void Result_QueryWithMultipleSteps_ShortCircuitsOnError()
    {
        var executedThird = false;

        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Err("error")
                     from z in GetResultWithSideEffect(ref executedThird)
                     select x + y + z;

        Assert.True(result.IsErr);
        Assert.Equal("error", result.UnwrapErr());
        Assert.False(executedThird); // Should not execute due to short-circuiting
    }

    private Result<int, string> GetResultWithSideEffect(ref bool executed)
    {
        executed = true;
        return Result<int, string>.Ok(30);
    }

    #endregion

    #region Either LINQ Tests

    [Fact]
    public void Either_Select_TransformsRightValue()
    {
        var result = from x in Either<string, int>.Right(42)
                     select x * 2;

        Assert.True(result.IsRight);
        Assert.Equal(84, result.UnwrapRight());
    }

    [Fact]
    public void Either_Select_OnLeft_ReturnsLeft()
    {
        var result = from x in Either<string, int>.Left("error")
                     select x * 2;

        Assert.True(result.IsLeft);
        Assert.Equal("error", result.UnwrapLeft());
    }

    [Fact]
    public void Either_SelectMany_ChainsOperations()
    {
        var result = from x in Either<string, int>.Right(10)
                     from y in Either<string, int>.Right(20)
                     select x + y;

        Assert.True(result.IsRight);
        Assert.Equal(30, result.UnwrapRight());
    }

    [Fact]
    public void Either_SelectMany_WithLeft_ReturnsLeft()
    {
        var result = from x in Either<string, int>.Right(10)
                     from y in Either<string, int>.Left("error")
                     select x + y;

        Assert.True(result.IsLeft);
        Assert.Equal("error", result.UnwrapLeft());
    }

    [Fact]
    public void Either_Where_WithPredicate_WorksCorrectly()
    {
        var either = Either<string, int>.Right(42);
        var result = either.Where(x => x > 40, "too small");

        Assert.True(result.IsRight);
        Assert.Equal(42, result.UnwrapRight());
    }

    [Fact]
    public void Either_Where_WithFailingPredicate_ReturnsLeft()
    {
        var either = Either<string, int>.Right(42);
        var result = either.Where(x => x < 40, "too large");

        Assert.True(result.IsLeft);
        Assert.Equal("too large", result.UnwrapLeft());
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void Option_ParseAndCalculate_UsingLinq()
    {
        Option<string> TryParse(string input) =>
            int.TryParse(input, out var value)
                ? Option<int>.Some(value).Map(x => x.ToString())
                : Option<string>.None();

        var result = from a in TryParse("10")
                     from b in TryParse("20")
                     select $"{a}+{b}";

        Assert.True(result.IsSome);
        Assert.Equal("10+20", result.Unwrap());
    }

    [Fact]
    public void Result_ValidationChain_UsingLinq()
    {
        Result<int, string> ValidatePositive(int x) =>
            x > 0
                ? Result<int, string>.Ok(x)
                : Result<int, string>.Err("Must be positive");

        Result<int, string> ValidateLessThan100(int x) =>
            x < 100
                ? Result<int, string>.Ok(x)
                : Result<int, string>.Err("Must be less than 100");

        var result = from x in Result<int, string>.Ok(50)
                     from validated1 in ValidatePositive(x)
                     from validated2 in ValidateLessThan100(validated1)
                     select validated2 * 2;

        Assert.True(result.IsOk);
        Assert.Equal(100, result.Unwrap());
    }

    [Fact]
    public void Option_DatabaseLookup_Simulation()
    {
        Option<User> FindUser(int id) =>
            id == 1
                ? Option<User>.Some(new User { Id = 1, Name = "John" })
                : Option<User>.None();

        Option<Address> FindAddress(int userId) =>
            userId == 1
                ? Option<Address>.Some(new Address { City = "NYC" })
                : Option<Address>.None();

        var result = from user in FindUser(1)
                     from address in FindAddress(user.Id)
                     select $"{user.Name} lives in {address.City}";

        Assert.True(result.IsSome);
        Assert.Equal("John lives in NYC", result.Unwrap());
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class Address
    {
        public string City { get; set; } = string.Empty;
    }

    #endregion
}

