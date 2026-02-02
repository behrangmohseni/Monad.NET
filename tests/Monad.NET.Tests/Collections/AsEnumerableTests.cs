using Xunit;

namespace Monad.NET.Tests;

public class AsEnumerableTests
{
    #region Option.AsEnumerable Tests

    [Fact]
    public void Option_AsEnumerable_Some_ReturnsSingleElement()
    {
        var option = Option<int>.Some(42);

        var result = option.AsEnumerable().ToList();

        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public void Option_AsEnumerable_None_ReturnsEmpty()
    {
        var option = Option<int>.None();

        var result = option.AsEnumerable().ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Option_AsEnumerable_CanBeUsedWithLinq()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3),
            Option<int>.None(),
            Option<int>.Some(5)
        };

        var values = options.SelectMany(o => o.AsEnumerable()).ToList();

        Assert.Equal(new[] { 1, 3, 5 }, values);
    }

    [Fact]
    public void Option_AsEnumerable_CanBeForeach()
    {
        var option = Option<string>.Some("hello");
        var collected = new List<string>();

        foreach (var value in option.AsEnumerable())
        {
            collected.Add(value);
        }

        Assert.Single(collected);
        Assert.Equal("hello", collected[0]);
    }

    [Fact]
    public void Option_ToArray_Some_ReturnsSingleElementArray()
    {
        var option = Option<int>.Some(42);

        var result = option.ToArray();

        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public void Option_ToArray_None_ReturnsEmptyArray()
    {
        var option = Option<int>.None();

        var result = option.ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void Option_ToList_Some_ReturnsSingleElementList()
    {
        var option = Option<int>.Some(42);

        var result = option.ToList();

        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public void Option_ToList_None_ReturnsEmptyList()
    {
        var option = Option<int>.None();

        var result = option.ToList();

        Assert.Empty(result);
    }

    #endregion

    #region Result.AsEnumerable Tests

    [Fact]
    public void Result_AsEnumerable_Ok_ReturnsSingleElement()
    {
        var result = Result<int, string>.Ok(42);

        var enumerable = result.AsEnumerable().ToList();

        Assert.Single(enumerable);
        Assert.Equal(42, enumerable[0]);
    }

    [Fact]
    public void Result_AsEnumerable_Err_ReturnsEmpty()
    {
        var result = Result<int, string>.Error("error");

        var enumerable = result.AsEnumerable().ToList();

        Assert.Empty(enumerable);
    }

    [Fact]
    public void Result_AsEnumerable_CanBeUsedWithLinq()
    {
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Error("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Error("error2"),
            Result<int, string>.Ok(5)
        };

        var values = results.SelectMany(r => r.AsEnumerable()).ToList();

        Assert.Equal(new[] { 1, 3, 5 }, values);
    }

    [Fact]
    public void Result_AsEnumerable_CanBeForeach()
    {
        var result = Result<string, int>.Ok("hello");
        var collected = new List<string>();

        foreach (var value in result.AsEnumerable())
        {
            collected.Add(value);
        }

        Assert.Single(collected);
        Assert.Equal("hello", collected[0]);
    }

    [Fact]
    public void Result_ToArray_Ok_ReturnsSingleElementArray()
    {
        var result = Result<int, string>.Ok(42);

        var array = result.ToArray();

        Assert.Single(array);
        Assert.Equal(42, array[0]);
    }

    [Fact]
    public void Result_ToArray_Err_ReturnsEmptyArray()
    {
        var result = Result<int, string>.Error("error");

        var array = result.ToArray();

        Assert.Empty(array);
    }

    [Fact]
    public void Result_ToList_Ok_ReturnsSingleElementList()
    {
        var result = Result<int, string>.Ok(42);

        var list = result.ToList();

        Assert.Single(list);
        Assert.Equal(42, list[0]);
    }

    [Fact]
    public void Result_ToList_Err_ReturnsEmptyList()
    {
        var result = Result<int, string>.Error("error");

        var list = result.ToList();

        Assert.Empty(list);
    }

    #endregion

    #region Practical Use Cases

    [Fact]
    public void Option_AsEnumerable_UsefulForConcat()
    {
        var definiteValues = new[] { 1, 2 };
        var maybeValue = Option<int>.Some(3);

        var all = definiteValues.Concat(maybeValue.AsEnumerable()).ToList();

        Assert.Equal(new[] { 1, 2, 3 }, all);
    }

    [Fact]
    public void Result_AsEnumerable_UsefulForConcat()
    {
        var definiteValues = new[] { 1, 2 };
        var maybeValue = Result<int, string>.Ok(3);

        var all = definiteValues.Concat(maybeValue.AsEnumerable()).ToList();

        Assert.Equal(new[] { 1, 2, 3 }, all);
    }

    [Fact]
    public void Option_AsEnumerable_UsefulWithAny()
    {
        var someOption = Option<int>.Some(42);
        var noneOption = Option<int>.None();

        Assert.True(someOption.AsEnumerable().Any());
        Assert.False(noneOption.AsEnumerable().Any());
    }

    [Fact]
    public void Result_AsEnumerable_UsefulWithAny()
    {
        var okResult = Result<int, string>.Ok(42);
        var errResult = Result<int, string>.Error("error");

        Assert.True(okResult.AsEnumerable().Any());
        Assert.False(errResult.AsEnumerable().Any());
    }

    [Fact]
    public void Option_AsEnumerable_UsefulWithFirstOrDefault()
    {
        var someOption = Option<int>.Some(42);
        var noneOption = Option<int>.None();

        Assert.Equal(42, someOption.AsEnumerable().FirstOrDefault());
        Assert.Equal(0, noneOption.AsEnumerable().FirstOrDefault());
    }

    [Fact]
    public void Result_AsEnumerable_UsefulWithFirstOrDefault()
    {
        var okResult = Result<int, string>.Ok(42);
        var errResult = Result<int, string>.Error("error");

        Assert.Equal(42, okResult.AsEnumerable().FirstOrDefault());
        Assert.Equal(0, errResult.AsEnumerable().FirstOrDefault());
    }

    #endregion
}

