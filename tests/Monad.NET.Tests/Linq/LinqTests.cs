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
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Option_SelectMany_ChainsOperations()
    {
        var result = from x in Option<int>.Some(10)
                     from y in Option<int>.Some(20)
                     select x + y;

        Assert.True(result.IsSome);
        Assert.Equal(30, result.GetValue());
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
        Assert.Equal(42, result.GetValue());
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
        Assert.Equal(200, result.GetValue());
    }

    [Fact]
    public void Option_QueryWithLet_WorksCorrectly()
    {
        var result = from x in Option<int>.Some(5)
                     let doubled = x * 2
                     from y in Option<int>.Some(3)
                     select doubled + y;

        Assert.True(result.IsSome);
        Assert.Equal(13, result.GetValue());
    }

    #endregion

    #region Result LINQ Tests

    [Fact]
    public void Result_Select_TransformsOkValue()
    {
        var result = from x in Result<int, string>.Ok(42)
                     select x * 2;

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Result_Select_OnErr_ReturnsErr()
    {
        var result = from x in Result<int, string>.Error("error")
                     select x * 2;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void Result_SelectMany_ChainsOperations()
    {
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Result_SelectMany_WithErr_ReturnsErr()
    {
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Error("error")
                     select x + y;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void Result_Where_FiltersValues()
    {
        var okResult = Result<int, string>.Ok(42);
        var result = okResult.Where(x => x > 40, "too small");

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Result_Where_WithFailingPredicate_ReturnsErr()
    {
        var okResult = Result<int, string>.Ok(42);
        var result = okResult.Where(x => x < 40, "too large");

        Assert.True(result.IsError);
        Assert.Equal("too large", result.GetError());
    }

    [Fact]
    public void Result_Where_WithErrorFactory_WorksCorrectly()
    {
        var okResult = Result<int, string>.Ok(30);
        var result = okResult.Where(
            x => x >= 40,
            x => $"Value {x} is too small"
        );

        Assert.True(result.IsError);
        Assert.Equal("Value 30 is too small", result.GetError());
    }

    [Fact]
    public void Result_ComplexQuery_WorksCorrectly()
    {
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Ok(20)
                     from z in Result<int, string>.Ok(30)
                     select x + y + z;

        Assert.True(result.IsOk);
        Assert.Equal(60, result.GetValue());
    }

    [Fact]
    public void Result_QueryWithMultipleSteps_ShortCircuitsOnError()
    {
        var executedThird = false;

        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Error("error")
                     from z in GetResultWithSideEffect(ref executedThird)
                     select x + y + z;

        Assert.True(result.IsError);
        Assert.Equal("error", result.GetError());
        Assert.False(executedThird); // Should not execute due to short-circuiting
    }

    private Result<int, string> GetResultWithSideEffect(ref bool executed)
    {
        executed = true;
        return Result<int, string>.Ok(30);
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
        Assert.Equal("10+20", result.GetValue());
    }

    [Fact]
    public void Result_ValidationChain_UsingLinq()
    {
        Result<int, string> ValidatePositive(int x) =>
            x > 0
                ? Result<int, string>.Ok(x)
                : Result<int, string>.Error("Must be positive");

        Result<int, string> ValidateLessThan100(int x) =>
            x < 100
                ? Result<int, string>.Ok(x)
                : Result<int, string>.Error("Must be less than 100");

        var result = from x in Result<int, string>.Ok(50)
                     from validated1 in ValidatePositive(x)
                     from validated2 in ValidateLessThan100(validated1)
                     select validated2 * 2;

        Assert.True(result.IsOk);
        Assert.Equal(100, result.GetValue());
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
        Assert.Equal("John lives in NYC", result.GetValue());
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

    #region Try LINQ Tests

    [Fact]
    public void Try_Select_TransformsValue()
    {
        var result = from x in Try<int>.Ok(42)
                     select x * 2;

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Try_SelectMany_ChainsOperations()
    {
        var result = from x in Try<int>.Ok(10)
                     from y in Try<int>.Ok(20)
                     select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Try_SelectMany_WithFailure_ReturnsFailure()
    {
        var result = from x in Try<int>.Ok(10)
                     from y in Try<int>.Error(new InvalidOperationException("Failed"))
                     select x + y;

        Assert.True(result.IsError);
    }

    [Fact]
    public void Try_Where_FiltersValues()
    {
        var result = from x in Try<int>.Ok(42)
                     where x > 40
                     select x;

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Try_Where_FailingPredicate_ReturnsFailure()
    {
        var result = from x in Try<int>.Ok(42)
                     where x > 50
                     select x;

        Assert.True(result.IsError);
    }

    [Fact]
    public void Try_ComplexQuery_ChainsMultipleOperations()
    {
        static Try<int> ParseInt(string s) =>
            Try<int>.Of(() => int.Parse(s));

        var result = from x in ParseInt("10")
                     from y in ParseInt("20")
                     from z in ParseInt("30")
                     select x + y + z;

        Assert.True(result.IsOk);
        Assert.Equal(60, result.GetValue());
    }

    #endregion

    #region Validation LINQ Tests

    [Fact]
    public void Validation_Select_TransformsValue()
    {
        var result = from x in Validation<int, string>.Ok(42)
                     select x * 2;

        Assert.True(result.IsOk);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Validation_SelectMany_ChainsOperations()
    {
        var result = from x in Validation<int, string>.Ok(10)
                     from y in Validation<int, string>.Ok(20)
                     select x + y;

        Assert.True(result.IsOk);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Validation_SelectMany_WithInvalid_ReturnsInvalid()
    {
        var result = from x in Validation<int, string>.Ok(10)
                     from y in Validation<int, string>.Error("Error")
                     select x + y;

        Assert.True(result.IsError);
    }

    [Fact]
    public void Validation_ComplexQuery_ChainsValidations()
    {
        static Validation<string, string> ValidateName(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Validation<string, string>.Error("Name is required")
                : Validation<string, string>.Ok(name);

        static Validation<int, string> ValidateAge(int age) =>
            age < 0
                ? Validation<int, string>.Error("Age must be positive")
                : Validation<int, string>.Ok(age);

        var result = from name in ValidateName("John")
                     from age in ValidateAge(30)
                     select $"{name} is {age} years old";

        Assert.True(result.IsOk);
        Assert.Equal("John is 30 years old", result.GetValue());
    }

    #endregion

    #region Writer LINQ Tests

    [Fact]
    public void Writer_Select_TransformsValue()
    {
        var writer = Writer<string, int>.Tell(42, "Initial value\n");
        var result = from x in writer
                     select x * 2;

        Assert.Equal(84, result.Value);
        Assert.Equal("Initial value\n", result.Log);
    }

    [Fact]
    public void Writer_SelectMany_ChainsOperationsWithLogs()
    {
        var result = from x in Writer<string, int>.Tell(10, "Started\n")
                     from y in Writer<string, int>.Tell(20, "Added 20\n")
                     select x + y;

        Assert.Equal(30, result.Value);
        Assert.Equal("Started\nAdded 20\n", result.Log);
    }

    [Fact]
    public void Writer_SelectMany_MultipleChains()
    {
        var result = from a in Writer<string, int>.Tell(1, "a=1\n")
                     from b in Writer<string, int>.Tell(2, "b=2\n")
                     from c in Writer<string, int>.Tell(3, "c=3\n")
                     select a + b + c;

        Assert.Equal(6, result.Value);
        Assert.Equal("a=1\nb=2\nc=3\n", result.Log);
    }

    [Fact]
    public void Writer_ListLog_SelectMany_ChainsWithListConcatenation()
    {
        var result = from x in Writer<List<string>, int>.Tell(10, ["Step 1"])
                     from y in Writer<List<string>, int>.Tell(20, ["Step 2"])
                     select x + y;

        Assert.Equal(30, result.Value);
        Assert.Equal(2, result.Log.Count);
        Assert.Equal("Step 1", result.Log[0]);
        Assert.Equal("Step 2", result.Log[1]);
    }

    #endregion

    #region NonEmptyList Deconstruct Tests

    [Fact]
    public void NonEmptyList_Deconstruct_SingleElement()
    {
        var list = NonEmptyList<int>.Of(42);
        var (head, tail) = list;

        Assert.Equal(42, head);
        Assert.Empty(tail);
    }

    [Fact]
    public void NonEmptyList_Deconstruct_MultipleElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var (head, tail) = list;

        Assert.Equal(1, head);
        Assert.Equal(4, tail.Count);
        Assert.Equal([2, 3, 4, 5], tail);
    }

    [Fact]
    public void NonEmptyList_Deconstruct_PatternMatching()
    {
        var list = NonEmptyList<string>.Of("first", "second", "third");
        var (head, tail) = list;

        Assert.Equal("first", head);
        Assert.Equal(2, tail.Count);
    }

    #endregion
}

