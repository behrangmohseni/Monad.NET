using System.Collections.Immutable;
using Xunit;

namespace Monad.NET.Tests;

public class EqualityConsistencyTests
{
    #region default(T) equality — all types must handle this without crashing

    [Fact]
    public void Option_Default_EqualsDefault()
    {
        var a = default(Option<int>);
        var b = default(Option<int>);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Option_Default_EqualsNone()
    {
        var def = default(Option<int>);
        var none = Option<int>.None();

        Assert.True(def.Equals(none));
        Assert.True(def == none);
        Assert.Equal(def.GetHashCode(), none.GetHashCode());
    }

    [Fact]
    public void Option_Default_NotEqualsSome()
    {
        var def = default(Option<int>);
        var some = Option<int>.Some(42);

        Assert.False(def.Equals(some));
        Assert.True(def != some);
    }

    [Fact]
    public void Result_Default_EqualsDefault()
    {
        var a = default(Result<int, string>);
        var b = default(Result<int, string>);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Result_Default_NotEqualsOk()
    {
        var def = default(Result<int, string>);
        var ok = Result<int, string>.Ok(42);

        Assert.False(def.Equals(ok));
        Assert.True(def != ok);
    }

    [Fact]
    public void Try_Default_EqualsDefault()
    {
        var a = default(Try<int>);
        var b = default(Try<int>);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Try_Default_NotEqualsSuccess()
    {
        var def = default(Try<int>);
        var ok = Try<int>.Ok(42);

        Assert.False(def.Equals(ok));
        Assert.True(def != ok);
    }

    [Fact]
    public void Try_Default_NotEqualsConstructedError()
    {
        var def = default(Try<int>);
        var err = Try<int>.Error(new Exception("fail"));

        Assert.False(def.Equals(err));
        Assert.True(def != err);
    }

    [Fact]
    public void Validation_Default_EqualsDefault()
    {
        var a = default(Validation<int, string>);
        var b = default(Validation<int, string>);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Validation_Default_NotEqualsOk()
    {
        var def = default(Validation<int, string>);
        var ok = Validation<int, string>.Ok(42);

        Assert.False(def.Equals(ok));
        Assert.True(def != ok);
    }

    #endregion

    #region Try<T> equality with exceptions (previously crashed)

    [Fact]
    public void Try_TwoSameExceptions_AreEqual()
    {
        var a = Try<int>.Error(new InvalidOperationException("test"));
        var b = Try<int>.Error(new InvalidOperationException("test"));

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Try_DifferentExceptionTypes_NotEqual()
    {
        var a = Try<int>.Error(new InvalidOperationException("test"));
        var b = Try<int>.Error(new ArgumentException("test"));

        Assert.False(a.Equals(b));
        Assert.True(a != b);
    }

    [Fact]
    public void Try_DifferentExceptionMessages_NotEqual()
    {
        var a = Try<int>.Error(new Exception("one"));
        var b = Try<int>.Error(new Exception("two"));

        Assert.False(a.Equals(b));
        Assert.True(a != b);
    }

    [Fact]
    public void Try_TwoSuccesses_AreEqual()
    {
        var a = Try<int>.Ok(42);
        var b = Try<int>.Ok(42);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    #endregion

    #region ToString on default-constructed instances (previously crashed for Try)

    [Fact]
    public void Option_Default_ToStringDoesNotCrash()
    {
        var def = default(Option<int>);
        Assert.Equal("None", def.ToString());
    }

    [Fact]
    public void Result_Default_ToStringDoesNotCrash()
    {
        var def = default(Result<int, string>);
        var str = def.ToString();
        Assert.NotNull(str);
    }

    [Fact]
    public void Try_Default_ToStringDoesNotCrash()
    {
        var def = default(Try<int>);
        var str = def.ToString();
        Assert.NotNull(str);
        Assert.Contains("Failure", str);
    }

    [Fact]
    public void Validation_Default_ToStringDoesNotCrash()
    {
        var def = default(Validation<int, string>);
        var str = def.ToString();
        Assert.NotNull(str);
    }

    #endregion

    #region GetHashCode consistency with Equals (contract: equal objects have equal hashes)

    [Fact]
    public void Option_EqualValues_HaveEqualHashCodes()
    {
        var a = Option<string>.Some("hello");
        var b = Option<string>.Some("hello");

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Result_EqualOkValues_HaveEqualHashCodes()
    {
        var a = Result<int, string>.Ok(42);
        var b = Result<int, string>.Ok(42);

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Result_EqualErrors_HaveEqualHashCodes()
    {
        var a = Result<int, string>.Error("err");
        var b = Result<int, string>.Error("err");

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Try_EqualErrors_HaveEqualHashCodes()
    {
        var a = Try<int>.Error(new Exception("test"));
        var b = Try<int>.Error(new Exception("test"));

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Validation_EqualErrors_HaveEqualHashCodes()
    {
        var a = Validation<int, string>.Error("err");
        var b = Validation<int, string>.Error("err");

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    #endregion

    #region CompareTo on default-constructed instances

    [Fact]
    public void Option_Default_CompareToDefault_IsZero()
    {
        var a = default(Option<int>);
        var b = default(Option<int>);
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void Try_Default_CompareToDefault_IsZero()
    {
        var a = default(Try<int>);
        var b = default(Try<int>);
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void Result_Default_CompareToDefault_IsZero()
    {
        var a = default(Result<int, string>);
        var b = default(Result<int, string>);
        Assert.Equal(0, a.CompareTo(b));
    }

    #endregion

    #region Collection usage (HashSet, Dictionary) with default-constructed instances

    [Fact]
    public void Try_Default_WorksInHashSet()
    {
        var set = new HashSet<Try<int>>();
        var def1 = default(Try<int>);
        var def2 = default(Try<int>);

        set.Add(def1);
        Assert.Contains(def2, set);
        Assert.Single(set);
    }

    [Fact]
    public void Option_Default_WorksInHashSet()
    {
        var set = new HashSet<Option<int>>();
        var def1 = default(Option<int>);
        var none = Option<int>.None();

        set.Add(def1);
        Assert.Contains(none, set);
        Assert.Single(set);
    }

    [Fact]
    public void Result_Default_WorksInHashSet()
    {
        var set = new HashSet<Result<int, string>>();
        var def1 = default(Result<int, string>);
        var def2 = default(Result<int, string>);

        set.Add(def1);
        Assert.Contains(def2, set);
        Assert.Single(set);
    }

    #endregion
}
