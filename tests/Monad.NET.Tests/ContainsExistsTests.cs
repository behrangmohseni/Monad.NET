using Xunit;

namespace Monad.NET.Tests;

public class ContainsExistsTests
{
    #region Option<T> Contains/Exists

    [Fact]
    public void Option_Contains_ReturnsTrueForMatchingValue()
    {
        var option = Option<int>.Some(42);
        Assert.True(option.Contains(42));
    }

    [Fact]
    public void Option_Contains_ReturnsFalseForNonMatchingValue()
    {
        var option = Option<int>.Some(42);
        Assert.False(option.Contains(0));
    }

    [Fact]
    public void Option_Contains_ReturnsFalseForNone()
    {
        var option = Option<int>.None();
        Assert.False(option.Contains(42));
    }

    [Fact]
    public void Option_Contains_WorksWithStrings()
    {
        var option = Option<string>.Some("hello");
        Assert.True(option.Contains("hello"));
        Assert.False(option.Contains("world"));
    }

    [Fact]
    public void Option_Contains_WorksWithReferenceEquality()
    {
        var obj = new object();
        var option = Option<object>.Some(obj);
        Assert.True(option.Contains(obj));
        Assert.False(option.Contains(new object()));
    }

    [Fact]
    public void Option_Exists_ReturnsTrueWhenPredicateMatches()
    {
        var option = Option<int>.Some(42);
        Assert.True(option.Exists(x => x > 40));
        Assert.True(option.Exists(x => x == 42));
        Assert.True(option.Exists(x => x % 2 == 0));
    }

    [Fact]
    public void Option_Exists_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var option = Option<int>.Some(42);
        Assert.False(option.Exists(x => x > 50));
        Assert.False(option.Exists(x => x < 0));
    }

    [Fact]
    public void Option_Exists_ReturnsFalseForNone()
    {
        var option = Option<int>.None();
        Assert.False(option.Exists(x => true));
    }

    [Fact]
    public void Option_Exists_ThrowsForNullPredicate()
    {
        var option = Option<int>.Some(42);
        Assert.Throws<ArgumentNullException>(() => option.Exists(null!));
    }

    #endregion

    #region Result<T, TErr> Contains/Exists

    [Fact]
    public void Result_Contains_ReturnsTrueForMatchingValue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.True(result.Contains(42));
    }

    [Fact]
    public void Result_Contains_ReturnsFalseForNonMatchingValue()
    {
        var result = Result<int, string>.Ok(42);
        Assert.False(result.Contains(0));
    }

    [Fact]
    public void Result_Contains_ReturnsFalseForErr()
    {
        var result = Result<int, string>.Err("error");
        Assert.False(result.Contains(42));
    }

    [Fact]
    public void Result_ContainsError_ReturnsTrueForMatchingError()
    {
        var result = Result<int, string>.Err("not found");
        Assert.True(result.ContainsError("not found"));
    }

    [Fact]
    public void Result_ContainsError_ReturnsFalseForNonMatchingError()
    {
        var result = Result<int, string>.Err("not found");
        Assert.False(result.ContainsError("other error"));
    }

    [Fact]
    public void Result_ContainsError_ReturnsFalseForOk()
    {
        var result = Result<int, string>.Ok(42);
        Assert.False(result.ContainsError("error"));
    }

    [Fact]
    public void Result_Exists_ReturnsTrueWhenPredicateMatches()
    {
        var result = Result<int, string>.Ok(42);
        Assert.True(result.Exists(x => x > 40));
        Assert.True(result.Exists(x => x == 42));
    }

    [Fact]
    public void Result_Exists_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var result = Result<int, string>.Ok(42);
        Assert.False(result.Exists(x => x > 50));
    }

    [Fact]
    public void Result_Exists_ReturnsFalseForErr()
    {
        var result = Result<int, string>.Err("error");
        Assert.False(result.Exists(x => true));
    }

    [Fact]
    public void Result_Exists_ThrowsForNullPredicate()
    {
        var result = Result<int, string>.Ok(42);
        Assert.Throws<ArgumentNullException>(() => result.Exists(null!));
    }

    [Fact]
    public void Result_ExistsError_ReturnsTrueWhenPredicateMatches()
    {
        var result = Result<int, string>.Err("not found");
        Assert.True(result.ExistsError(e => e.Contains("not")));
        Assert.True(result.ExistsError(e => e.Length > 0));
    }

    [Fact]
    public void Result_ExistsError_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var result = Result<int, string>.Err("not found");
        Assert.False(result.ExistsError(e => e.Contains("xyz")));
    }

    [Fact]
    public void Result_ExistsError_ReturnsFalseForOk()
    {
        var result = Result<int, string>.Ok(42);
        Assert.False(result.ExistsError(e => true));
    }

    [Fact]
    public void Result_ExistsError_ThrowsForNullPredicate()
    {
        var result = Result<int, string>.Err("error");
        Assert.Throws<ArgumentNullException>(() => result.ExistsError(null!));
    }

    #endregion

    #region Either<TLeft, TRight> Contains/Exists

    [Fact]
    public void Either_ContainsRight_ReturnsTrueForMatchingValue()
    {
        var either = Either<string, int>.Right(42);
        Assert.True(either.ContainsRight(42));
    }

    [Fact]
    public void Either_ContainsRight_ReturnsFalseForNonMatchingValue()
    {
        var either = Either<string, int>.Right(42);
        Assert.False(either.ContainsRight(0));
    }

    [Fact]
    public void Either_ContainsRight_ReturnsFalseForLeft()
    {
        var either = Either<string, int>.Left("error");
        Assert.False(either.ContainsRight(42));
    }

    [Fact]
    public void Either_ContainsLeft_ReturnsTrueForMatchingValue()
    {
        var either = Either<string, int>.Left("error");
        Assert.True(either.ContainsLeft("error"));
    }

    [Fact]
    public void Either_ContainsLeft_ReturnsFalseForNonMatchingValue()
    {
        var either = Either<string, int>.Left("error");
        Assert.False(either.ContainsLeft("other"));
    }

    [Fact]
    public void Either_ContainsLeft_ReturnsFalseForRight()
    {
        var either = Either<string, int>.Right(42);
        Assert.False(either.ContainsLeft("error"));
    }

    [Fact]
    public void Either_ExistsRight_ReturnsTrueWhenPredicateMatches()
    {
        var either = Either<string, int>.Right(42);
        Assert.True(either.ExistsRight(x => x > 40));
    }

    [Fact]
    public void Either_ExistsRight_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var either = Either<string, int>.Right(42);
        Assert.False(either.ExistsRight(x => x > 50));
    }

    [Fact]
    public void Either_ExistsRight_ReturnsFalseForLeft()
    {
        var either = Either<string, int>.Left("error");
        Assert.False(either.ExistsRight(x => true));
    }

    [Fact]
    public void Either_ExistsRight_ThrowsForNullPredicate()
    {
        var either = Either<string, int>.Right(42);
        Assert.Throws<ArgumentNullException>(() => either.ExistsRight(null!));
    }

    [Fact]
    public void Either_ExistsLeft_ReturnsTrueWhenPredicateMatches()
    {
        var either = Either<string, int>.Left("error");
        Assert.True(either.ExistsLeft(e => e.Contains("err")));
    }

    [Fact]
    public void Either_ExistsLeft_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var either = Either<string, int>.Left("error");
        Assert.False(either.ExistsLeft(e => e.Contains("xyz")));
    }

    [Fact]
    public void Either_ExistsLeft_ReturnsFalseForRight()
    {
        var either = Either<string, int>.Right(42);
        Assert.False(either.ExistsLeft(e => true));
    }

    [Fact]
    public void Either_ExistsLeft_ThrowsForNullPredicate()
    {
        var either = Either<string, int>.Left("error");
        Assert.Throws<ArgumentNullException>(() => either.ExistsLeft(null!));
    }

    #endregion

    #region Try<T> Contains/Exists

    [Fact]
    public void Try_Contains_ReturnsTrueForMatchingValue()
    {
        var result = Try<int>.Success(42);
        Assert.True(result.Contains(42));
    }

    [Fact]
    public void Try_Contains_ReturnsFalseForNonMatchingValue()
    {
        var result = Try<int>.Success(42);
        Assert.False(result.Contains(0));
    }

    [Fact]
    public void Try_Contains_ReturnsFalseForFailure()
    {
        var result = Try<int>.Failure(new Exception("error"));
        Assert.False(result.Contains(42));
    }

    [Fact]
    public void Try_Exists_ReturnsTrueWhenPredicateMatches()
    {
        var result = Try<int>.Success(42);
        Assert.True(result.Exists(x => x > 40));
    }

    [Fact]
    public void Try_Exists_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var result = Try<int>.Success(42);
        Assert.False(result.Exists(x => x > 50));
    }

    [Fact]
    public void Try_Exists_ReturnsFalseForFailure()
    {
        var result = Try<int>.Failure(new Exception("error"));
        Assert.False(result.Exists(x => true));
    }

    [Fact]
    public void Try_Exists_ThrowsForNullPredicate()
    {
        var result = Try<int>.Success(42);
        Assert.Throws<ArgumentNullException>(() => result.Exists(null!));
    }

    #endregion

    #region Validation<T, TErr> Contains/Exists

    [Fact]
    public void Validation_Contains_ReturnsTrueForMatchingValue()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.True(validation.Contains(42));
    }

    [Fact]
    public void Validation_Contains_ReturnsFalseForNonMatchingValue()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.False(validation.Contains(0));
    }

    [Fact]
    public void Validation_Contains_ReturnsFalseForInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        Assert.False(validation.Contains(42));
    }

    [Fact]
    public void Validation_Exists_ReturnsTrueWhenPredicateMatches()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.True(validation.Exists(x => x > 40));
    }

    [Fact]
    public void Validation_Exists_ReturnsFalseWhenPredicateDoesNotMatch()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.False(validation.Exists(x => x > 50));
    }

    [Fact]
    public void Validation_Exists_ReturnsFalseForInvalid()
    {
        var validation = Validation<int, string>.Invalid("error");
        Assert.False(validation.Exists(x => true));
    }

    [Fact]
    public void Validation_Exists_ThrowsForNullPredicate()
    {
        var validation = Validation<int, string>.Valid(42);
        Assert.Throws<ArgumentNullException>(() => validation.Exists(null!));
    }

    #endregion
}

