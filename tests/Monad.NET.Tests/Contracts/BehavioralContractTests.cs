namespace Monad.NET.Tests;

/// <summary>
/// Behavioral Contract Tests - These tests ensure that the semantic behavior
/// of monadic operations remains consistent across versions.
/// 
/// These tests verify:
/// - Monad laws (Left Identity, Right Identity, Associativity)
/// - Expected behavior of factory methods
/// - Expected behavior of core operations
/// - Implicit conversion semantics
/// </summary>
public class BehavioralContractTests
{
    #region Option<T> Monad Laws

    [Fact]
    public void Option_LeftIdentity_Law()
    {
        // Left Identity: return a >>= f ≡ f a
        // Some(a).Bind(f) should equal f(a)
        var value = 42;
        Func<int, Option<string>> f = x => Option<string>.Some(x.ToString());

        var left = Option<int>.Some(value).Bind(f);
        var right = f(value);

        Assert.Equal(left, right);
    }

    [Fact]
    public void Option_RightIdentity_Law()
    {
        // Right Identity: m >>= return ≡ m
        // option.Bind(Some) should equal option
        var option = Option<int>.Some(42);

        var result = option.Bind(x => Option<int>.Some(x));

        Assert.Equal(option, result);
    }

    [Fact]
    public void Option_Associativity_Law()
    {
        // Associativity: (m >>= f) >>= g ≡ m >>= (λx → f x >>= g)
        var option = Option<int>.Some(5);
        Func<int, Option<int>> f = x => Option<int>.Some(x * 2);
        Func<int, Option<string>> g = x => Option<string>.Some(x.ToString());

        var left = option.Bind(f).Bind(g);
        var right = option.Bind(x => f(x).Bind(g));

        Assert.Equal(left, right);
    }

    #endregion

    #region Result<T, E> Monad Laws

    [Fact]
    public void Result_LeftIdentity_Law()
    {
        var value = 42;
        Func<int, Result<string, string>> f = x => Result<string, string>.Ok(x.ToString());

        var left = Result<int, string>.Ok(value).Bind(f);
        var right = f(value);

        Assert.Equal(left, right);
    }

    [Fact]
    public void Result_RightIdentity_Law()
    {
        var result = Result<int, string>.Ok(42);

        var applied = result.Bind(x => Result<int, string>.Ok(x));

        Assert.Equal(result, applied);
    }

    [Fact]
    public void Result_Associativity_Law()
    {
        var result = Result<int, string>.Ok(5);
        Func<int, Result<int, string>> f = x => Result<int, string>.Ok(x * 2);
        Func<int, Result<string, string>> g = x => Result<string, string>.Ok(x.ToString());

        var left = result.Bind(f).Bind(g);
        var right = result.Bind(x => f(x).Bind(g));

        Assert.Equal(left, right);
    }

    #endregion

    #region Option<T> Factory Method Contracts

    [Fact]
    public void Option_Some_ShouldCreateSomeValue()
    {
        var option = Option<int>.Some(42);

        Assert.True(option.IsSome);
        Assert.False(option.IsNone);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void Option_None_ShouldCreateNoneValue()
    {
        var option = Option<int>.None();

        Assert.False(option.IsSome);
        Assert.True(option.IsNone);
    }

    [Fact]
    public void Option_Some_WithNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void Option_ImplicitConversion_FromValue_ShouldCreateSome()
    {
        Option<int> option = 42;

        Assert.True(option.IsSome);
        Assert.Equal(42, option.GetValue());
    }

    [Fact]
    public void Option_ImplicitConversion_FromNull_ShouldCreateNone()
    {
        string? nullValue = null;
        Option<string> option = nullValue!;

        Assert.True(option.IsNone);
    }

    #endregion

    #region Result<T, E> Factory Method Contracts

    [Fact]
    public void Result_Ok_ShouldCreateOkValue()
    {
        var result = Result<int, string>.Ok(42);

        Assert.True(result.IsOk);
        Assert.False(result.IsErr);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Result_Err_ShouldCreateErrValue()
    {
        var result = Result<int, string>.Err("error");

        Assert.False(result.IsOk);
        Assert.True(result.IsErr);
        Assert.Equal("error", result.GetError());
    }

    [Fact]
    public void Result_Ok_WithNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string, string>.Ok(null!));
    }

    [Fact]
    public void Result_Err_WithNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string, string>.Err(null!));
    }

    #endregion

    #region Option<T> Operation Contracts

    [Fact]
    public void Option_Map_OnSome_ShouldTransformValue()
    {
        var option = Option<int>.Some(42);

        var result = option.Map(x => x * 2);

        Assert.True(result.IsSome);
        Assert.Equal(84, result.GetValue());
    }

    [Fact]
    public void Option_Map_OnNone_ShouldReturnNone()
    {
        var option = Option<int>.None();

        var result = option.Map(x => x * 2);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_Filter_OnSome_WithPassingPredicate_ShouldReturnSome()
    {
        var option = Option<int>.Some(42);

        var result = option.Filter(x => x > 10);

        Assert.True(result.IsSome);
        Assert.Equal(42, result.GetValue());
    }

    [Fact]
    public void Option_Filter_OnSome_WithFailingPredicate_ShouldReturnNone()
    {
        var option = Option<int>.Some(42);

        var result = option.Filter(x => x > 100);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Option_AndThen_OnSome_ShouldExecuteFunction()
    {
        var option = Option<int>.Some(42);

        var result = option.Bind(x => Option<string>.Some(x.ToString()));

        Assert.True(result.IsSome);
        Assert.Equal("42", result.GetValue());
    }

    [Fact]
    public void Option_AndThen_OnNone_ShouldNotExecuteFunction()
    {
        var option = Option<int>.None();
        var executed = false;

        var result = option.Bind(x =>
        {
            executed = true;
            return Option<string>.Some(x.ToString());
        });

        Assert.True(result.IsNone);
        Assert.False(executed);
    }

    [Fact]
    public void Option_Match_OnSome_ShouldExecuteSomeBranch()
    {
        var option = Option<int>.Some(42);

        var result = option.Match(
            someFunc: x => x.ToString(),
            noneFunc: () => "none"
        );

        Assert.Equal("42", result);
    }

    [Fact]
    public void Option_Match_OnNone_ShouldExecuteNoneBranch()
    {
        var option = Option<int>.None();

        var result = option.Match(
            someFunc: x => x.ToString(),
            noneFunc: () => "none"
        );

        Assert.Equal("none", result);
    }

    [Fact]
    public void Option_UnwrapOr_OnSome_ShouldReturnValue()
    {
        var option = Option<int>.Some(42);

        var result = option.GetValueOr(0);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Option_UnwrapOr_OnNone_ShouldReturnDefault()
    {
        var option = Option<int>.None();

        var result = option.GetValueOr(0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Option_Unwrap_OnNone_ShouldThrow()
    {
        var option = Option<int>.None();

        Assert.Throws<InvalidOperationException>(() => option.GetValue());
    }

    #endregion

    #region Result<T, E> Operation Contracts

    [Fact]
    public void Result_Map_OnOk_ShouldTransformValue()
    {
        var result = Result<int, string>.Ok(42);

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsOk);
        Assert.Equal(84, mapped.GetValue());
    }

    [Fact]
    public void Result_Map_OnErr_ShouldReturnErr()
    {
        var result = Result<int, string>.Err("error");

        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsErr);
        Assert.Equal("error", mapped.GetError());
    }

    [Fact]
    public void Result_MapErr_OnErr_ShouldTransformError()
    {
        var result = Result<int, string>.Err("error");

        var mapped = result.MapError(e => e.ToUpper());

        Assert.True(mapped.IsErr);
        Assert.Equal("ERROR", mapped.GetError());
    }

    [Fact]
    public void Result_MapErr_OnOk_ShouldReturnOk()
    {
        var result = Result<int, string>.Ok(42);

        var mapped = result.MapError(e => e.ToUpper());

        Assert.True(mapped.IsOk);
        Assert.Equal(42, mapped.GetValue());
    }

    [Fact]
    public void Result_AndThen_OnOk_ShouldExecuteFunction()
    {
        var result = Result<int, string>.Ok(42);

        var chained = result.Bind(x => Result<string, string>.Ok(x.ToString()));

        Assert.True(chained.IsOk);
        Assert.Equal("42", chained.GetValue());
    }

    [Fact]
    public void Result_AndThen_OnErr_ShouldNotExecuteFunction()
    {
        var result = Result<int, string>.Err("error");
        var executed = false;

        var chained = result.Bind(x =>
        {
            executed = true;
            return Result<string, string>.Ok(x.ToString());
        });

        Assert.True(chained.IsErr);
        Assert.False(executed);
    }

    [Fact]
    public void Result_Match_OnOk_ShouldExecuteOkBranch()
    {
        var result = Result<int, string>.Ok(42);

        var matched = result.Match(
            okFunc: x => x.ToString(),
            errFunc: e => e
        );

        Assert.Equal("42", matched);
    }

    [Fact]
    public void Result_Match_OnErr_ShouldExecuteErrBranch()
    {
        var result = Result<int, string>.Err("error");

        var matched = result.Match(
            okFunc: x => x.ToString(),
            errFunc: e => e
        );

        Assert.Equal("error", matched);
    }

    [Fact]
    public void Result_Unwrap_OnErr_ShouldThrow()
    {
        var result = Result<int, string>.Err("error");

        Assert.Throws<InvalidOperationException>(() => result.GetValue());
    }

    [Fact]
    public void Result_UnwrapErr_OnOk_ShouldThrow()
    {
        var result = Result<int, string>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => result.GetError());
    }

    #endregion

    #region Validation<T, E> Operation Contracts

    [Fact]
    public void Validation_Valid_ShouldCreateValidValue()
    {
        var validation = Validation<int, string>.Valid(42);

        Assert.True(validation.IsValid);
        Assert.False(validation.IsInvalid);
        Assert.Equal(42, validation.GetValue());
    }

    [Fact]
    public void Validation_Invalid_ShouldCreateInvalidValue()
    {
        var validation = Validation<int, string>.Invalid("error");

        Assert.False(validation.IsValid);
        Assert.True(validation.IsInvalid);
    }

    [Fact]
    public void Validation_Apply_BothValid_ShouldCombine()
    {
        var v1 = Validation<int, string>.Valid(10);
        var v2 = Validation<int, string>.Valid(20);

        var result = v1.Apply(v2, (a, b) => a + b);

        Assert.True(result.IsValid);
        Assert.Equal(30, result.GetValue());
    }

    [Fact]
    public void Validation_Apply_BothInvalid_ShouldAccumulateErrors()
    {
        var v1 = Validation<int, string>.Invalid("error1");
        var v2 = Validation<int, string>.Invalid("error2");

        var result = v1.Apply(v2, (a, b) => a + b);

        Assert.True(result.IsInvalid);
        var errors = result.GetErrors();
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    [Fact]
    public void Validation_Apply_FirstInvalid_ShouldReturnErrors()
    {
        var v1 = Validation<int, string>.Invalid("error1");
        var v2 = Validation<int, string>.Valid(20);

        var result = v1.Apply(v2, (a, b) => a + b);

        Assert.True(result.IsInvalid);
        Assert.Contains("error1", result.GetErrors());
    }

    #endregion

    #region Either<L, R> Operation Contracts

    [Fact]
    public void Either_Left_ShouldCreateLeftValue()
    {
        var either = Either<string, int>.Left("error");

        Assert.True(either.IsLeft);
        Assert.False(either.IsRight);
        Assert.Equal("error", either.GetLeft());
    }

    [Fact]
    public void Either_Right_ShouldCreateRightValue()
    {
        var either = Either<string, int>.Right(42);

        Assert.False(either.IsLeft);
        Assert.True(either.IsRight);
        Assert.Equal(42, either.GetRight());
    }

    [Fact]
    public void Either_MapRight_OnRight_ShouldTransformValue()
    {
        var either = Either<string, int>.Right(42);

        var result = either.MapRight(x => x * 2);

        Assert.True(result.IsRight);
        Assert.Equal(84, result.GetRight());
    }

    [Fact]
    public void Either_MapLeft_OnLeft_ShouldTransformValue()
    {
        var either = Either<string, int>.Left("error");

        var result = either.MapLeft(e => e.ToUpper());

        Assert.True(result.IsLeft);
        Assert.Equal("ERROR", result.GetLeft());
    }

    [Fact]
    public void Either_Swap_ShouldSwapLeftAndRight()
    {
        var either = Either<string, int>.Right(42);

        var swapped = either.Swap();

        Assert.True(swapped.IsLeft);
        Assert.Equal(42, swapped.GetLeft());
    }

    #endregion

    #region Try<T> Operation Contracts

    [Fact]
    public void Try_Success_ShouldCreateSuccessValue()
    {
        var tryValue = Try<int>.Success(42);

        Assert.True(tryValue.IsSuccess);
        Assert.False(tryValue.IsFailure);
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void Try_Failure_ShouldCreateFailureValue()
    {
        var exception = new InvalidOperationException("error");
        var tryValue = Try<int>.Failure(exception);

        Assert.False(tryValue.IsSuccess);
        Assert.True(tryValue.IsFailure);
        Assert.Equal(exception, tryValue.GetException());
    }

    [Fact]
    public void Try_Of_WithSuccessfulAction_ShouldCreateSuccess()
    {
        var tryValue = Try<int>.Of(() => 42);

        Assert.True(tryValue.IsSuccess);
        Assert.Equal(42, tryValue.GetValue());
    }

    [Fact]
    public void Try_Of_WithThrowingAction_ShouldCreateFailure()
    {
        var tryValue = Try<int>.Of(() => throw new InvalidOperationException("error"));

        Assert.True(tryValue.IsFailure);
    }

    [Fact]
    public void Try_Recover_OnFailure_ShouldRecoverValue()
    {
        var tryValue = Try<int>.Failure(new InvalidOperationException("error"));

        var result = tryValue.Recover(_ => 0);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.GetValue());
    }

    [Fact]
    public void Try_Recover_OnSuccess_ShouldKeepValue()
    {
        var tryValue = Try<int>.Success(42);

        var result = tryValue.Recover(_ => 0);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.GetValue());
    }

    #endregion

    #region Unit Operation Contracts

    [Fact]
    public void Unit_AllInstances_ShouldBeEqual()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Default;
        var unit3 = new Unit();

        Assert.Equal(unit1, unit2);
        Assert.Equal(unit2, unit3);
        Assert.True(unit1 == unit2);
        Assert.True(unit2 == unit3);
    }

    [Fact]
    public void Unit_From_ShouldExecuteAction()
    {
        var executed = false;

        var result = Unit.From(() => { executed = true; });

        Assert.True(executed);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public void Unit_ToString_ShouldReturnParentheses()
    {
        var unit = Unit.Value;

        Assert.Equal("()", unit.ToString());
    }

    #endregion

    #region NonEmptyList<T> Contracts

    [Fact]
    public void NonEmptyList_Of_ShouldHaveHead()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);

        Assert.Equal(1, list.Head);
    }

    [Fact]
    public void NonEmptyList_Of_SingleElement_ShouldWork()
    {
        var list = NonEmptyList<int>.Of(42);

        Assert.Equal(42, list.Head);
        Assert.Single(list);
    }

    [Fact]
    public void NonEmptyList_Map_ShouldTransformAllElements()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);

        var result = list.Map(x => x * 2);

        Assert.Equal(new[] { 2, 4, 6 }, result.ToArray());
    }

    #endregion

    #region Collection Extension Contracts

    [Fact]
    public void Sequence_AllSome_ShouldReturnSome()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.Some(2),
            Option<int>.Some(3)
        };

        var result = options.Sequence();

        Assert.True(result.IsSome);
        Assert.Equal(new[] { 1, 2, 3 }, result.GetValue());
    }

    [Fact]
    public void Sequence_WithNone_ShouldReturnNone()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3)
        };

        var result = options.Sequence();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void Choose_ShouldFilterAndUnwrap()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3)
        };

        var result = options.Choose().ToList();

        Assert.Equal(new[] { 1, 3 }, result);
    }

    [Fact]
    public void Partition_ShouldSeparateOksAndErrs()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2")
        };

        var (oks, errors) = results.Partition();

        Assert.Equal(new[] { 1, 3 }, oks);
        Assert.Equal(new[] { "error1", "error2" }, errors);
    }

    #endregion

    #region Equality and Comparison Contracts

    [Fact]
    public void Option_SomeWithSameValue_ShouldBeEqual()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(42);

        Assert.Equal(option1, option2);
        Assert.True(option1 == option2);
    }

    [Fact]
    public void Option_SomeWithDifferentValue_ShouldNotBeEqual()
    {
        var option1 = Option<int>.Some(42);
        var option2 = Option<int>.Some(43);

        Assert.NotEqual(option1, option2);
        Assert.True(option1 != option2);
    }

    [Fact]
    public void Option_NoneInstances_ShouldBeEqual()
    {
        var option1 = Option<int>.None();
        var option2 = Option<int>.None();

        Assert.Equal(option1, option2);
        Assert.True(option1 == option2);
    }

    [Fact]
    public void Option_SomeAndNone_ShouldNotBeEqual()
    {
        var some = Option<int>.Some(42);
        var none = Option<int>.None();

        Assert.NotEqual(some, none);
        Assert.True(some != none);
    }

    [Fact]
    public void Result_OkWithSameValue_ShouldBeEqual()
    {
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(42);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Result_ErrWithSameError_ShouldBeEqual()
    {
        var result1 = Result<int, string>.Err("error");
        var result2 = Result<int, string>.Err("error");

        Assert.Equal(result1, result2);
    }

    #endregion
}

