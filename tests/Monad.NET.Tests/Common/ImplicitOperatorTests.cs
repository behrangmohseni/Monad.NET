using Xunit;

namespace Monad.NET.Tests;

public class ImplicitOperatorTests
{
    #region Option Implicit Operators (already existed)

    [Fact]
    public void Option_ImplicitFromValue_CreatesSome()
    {
        Option<int> option = 42;

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void Option_ImplicitFromNull_CreatesNone()
    {
        Option<string> option = null!;

        Assert.True(option.IsNone);
    }

    [Fact]
    public void Option_ImplicitInMethodCall()
    {
        static string Process(Option<int> opt) => opt.Match(v => $"Got {v}", () => "None");

        Assert.Equal("Got 42", Process(42));
    }

    #endregion

    #region Result Implicit Operators

    [Fact]
    public void Result_ImplicitFromValue_CreatesOk()
    {
        Result<int, string> result = 42;

        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Result_ImplicitInMethodCall()
    {
        static string Process(Result<int, string> result) =>
            result.Match(v => $"Ok: {v}", e => $"Err: {e}");

        Assert.Equal("Ok: 42", Process(42));
    }

    [Fact]
    public void Result_ImplicitFromValueInReturn()
    {
        static Result<int, string> GetValue() => 42;

        var result = GetValue();
        Assert.True(result.IsOk);
        Assert.Equal(42, result.GetValue());
    }

    #endregion

    #region Either Implicit Operators

    [Fact]
    public void Either_ImplicitFromRightValue_CreatesRight()
    {
        Either<string, int> either = 42;

        Assert.True(either.IsRight);
        Assert.Equal(42, either.GetRight());
    }

    [Fact]
    public void Either_ImplicitInMethodCall()
    {
        static string Process(Either<string, int> either) =>
            either.Match(l => $"Left: {l}", r => $"Right: {r}");

        Assert.Equal("Right: 42", Process(42));
    }

    [Fact]
    public void Either_ImplicitFromRightInReturn()
    {
        static Either<string, int> GetValue() => 100;

        var either = GetValue();
        Assert.True(either.IsRight);
        Assert.Equal(100, either.GetRight());
    }

    #endregion

    #region Try Implicit Operators

    [Fact]
    public void Try_ImplicitFromValue_CreatesSuccess()
    {
        Try<int> tryResult = 42;

        Assert.True(tryResult.IsSuccess);
        Assert.Equal(42, tryResult.GetValue());
    }

    [Fact]
    public void Try_ImplicitFromException_CreatesFailure()
    {
        Try<int> tryResult = new InvalidOperationException("test error");

        Assert.True(tryResult.IsFailure);
        Assert.IsType<InvalidOperationException>(tryResult.GetException());
        Assert.Equal("test error", tryResult.GetException().Message);
    }

    [Fact]
    public void Try_ImplicitInMethodCall()
    {
        static string Process(Try<int> result) =>
            result.Match(v => $"Success: {v}", ex => $"Failure: {ex.Message}");

        Assert.Equal("Success: 42", Process(42));
        Assert.Equal("Failure: error", Process(new Exception("error")));
    }

    [Fact]
    public void Try_ImplicitFromValueInReturn()
    {
        static Try<int> GetValue() => 42;
        static Try<int> GetError() => new Exception("oops");

        Assert.True(GetValue().IsSuccess);
        Assert.True(GetError().IsFailure);
    }

    #endregion

    #region Validation Implicit Operators

    [Fact]
    public void Validation_ImplicitFromValue_CreatesValid()
    {
        Validation<int, string> validation = 42;

        Assert.True(validation.IsValid);
        Assert.Equal(42, validation.GetValue());
    }

    [Fact]
    public void Validation_ImplicitInMethodCall()
    {
        static string Process(Validation<int, string> v) =>
            v.Match(val => $"Valid: {val}", errors => $"Errors: {errors.Count}");

        Assert.Equal("Valid: 42", Process(42));
    }

    [Fact]
    public void Validation_ImplicitFromValueInReturn()
    {
        static Validation<int, string> GetValue() => 42;

        var validation = GetValue();
        Assert.True(validation.IsValid);
        Assert.Equal(42, validation.GetValue());
    }

    #endregion

    #region NonEmptyList Implicit Operators

    [Fact]
    public void NonEmptyList_ImplicitFromValue_CreatesSingleElementList()
    {
        NonEmptyList<int> list = 42;

        Assert.Equal(1, list.Count);
        Assert.Equal(42, list.Head);
    }

    [Fact]
    public void NonEmptyList_ImplicitInMethodCall()
    {
        static int GetHead(NonEmptyList<int> list) => list.Head;

        Assert.Equal(42, GetHead(42));
    }

    [Fact]
    public void NonEmptyList_ImplicitFromValueInReturn()
    {
        static NonEmptyList<int> GetList() => 42;

        var list = GetList();
        Assert.Single(list);
        Assert.Equal(42, list.Head);
    }

    #endregion

    #region RemoteData Implicit Operators

    [Fact]
    public void RemoteData_ImplicitFromValue_CreatesSuccess()
    {
        RemoteData<int, string> data = 42;

        Assert.True(data.IsSuccess);
        Assert.Equal(42, data.GetValue());
    }

    [Fact]
    public void RemoteData_ImplicitInMethodCall()
    {
        static string Process(RemoteData<int, string> data) =>
            data.Match(
                notAskedFunc: () => "NotAsked",
                loadingFunc: () => "Loading",
                successFunc: v => $"Success: {v}",
                failureFunc: e => $"Failure: {e}"
            );

        Assert.Equal("Success: 42", Process(42));
    }

    [Fact]
    public void RemoteData_ImplicitFromValueInReturn()
    {
        static RemoteData<int, string> GetData() => 42;

        var data = GetData();
        Assert.True(data.IsSuccess);
        Assert.Equal(42, data.GetValue());
    }

    #endregion

    #region Real-World Usage Patterns

    [Fact]
    public void Chaining_WithImplicitConversions()
    {
        // Start with an implicit value
        Option<int> option = 10;

        var result = option
            .Map(x => x * 2)
            .Filter(x => x > 15)
            .Map(x => $"Result: {x}");

        Assert.True(result.IsSome);
        Assert.Equal("Result: 20", result.GetValue());
    }

    [Fact]
    public void ReturnImplicitResult_FromValidation()
    {
        static Result<int, string> ValidatePositive(int value)
        {
            if (value <= 0)
                return Result<int, string>.Err("Must be positive");
            return value; // Implicit conversion!
        }

        Assert.True(ValidatePositive(42).IsOk);
        Assert.True(ValidatePositive(-1).IsErr);
    }

    [Fact]
    public void ReturnImplicitTry_FromComputation()
    {
        static Try<int> SafeDivide(int a, int b)
        {
            if (b == 0)
                return new DivideByZeroException();
            return a / b; // Implicit conversion!
        }

        Assert.True(SafeDivide(10, 2).IsSuccess);
        Assert.Equal(5, SafeDivide(10, 2).GetValue());
        Assert.True(SafeDivide(10, 0).IsFailure);
    }

    #endregion
}

