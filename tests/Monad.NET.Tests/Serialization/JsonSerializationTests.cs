using System.Text.Json;
using Monad.NET;
using Monad.NET.Json;
using Xunit;

namespace Monad.NET.Tests;

public class JsonSerializationTests
{
    private readonly JsonSerializerOptions _options = MonadJsonExtensions.CreateMonadSerializerOptions();

    #region Option<T> Tests

    [Fact]
    public void Option_Some_SerializesAsValue()
    {
        var option = Option<int>.Some(42);
        var json = JsonSerializer.Serialize(option, _options);
        Assert.Equal("42", json);
    }

    [Fact]
    public void Option_None_SerializesAsNull()
    {
        var option = Option<int>.None();
        var json = JsonSerializer.Serialize(option, _options);
        Assert.Equal("null", json);
    }

    [Fact]
    public void Option_Some_Roundtrip()
    {
        var original = Option<string>.Some("hello");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Option<string>>(json, _options);
        Assert.True(deserialized.IsSome);
        Assert.Equal("hello", deserialized.GetValue());
    }

    [Fact]
    public void Option_None_Roundtrip()
    {
        var original = Option<string>.None();
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Option<string>>(json, _options);
        Assert.False(deserialized.IsSome);
    }

    [Fact]
    public void Option_ComplexType_Roundtrip()
    {
        var person = new Person { Name = "John", Age = 30 };
        var original = Option<Person>.Some(person);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Option<Person>>(json, _options);
        Assert.True(deserialized.IsSome);
        Assert.Equal("John", deserialized.GetValue().Name);
        Assert.Equal(30, deserialized.GetValue().Age);
    }

    #endregion

    #region Result<T, E> Tests

    [Fact]
    public void Result_Ok_Serializes()
    {
        var result = Result<int, string>.Ok(42);
        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"isOk\":true", json);
        Assert.Contains("\"value\":42", json);
    }

    [Fact]
    public void Result_Err_Serializes()
    {
        var result = Result<int, string>.Err("error message");
        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"isOk\":false", json);
        Assert.Contains("\"error\":\"error message\"", json);
    }

    [Fact]
    public void Result_Ok_Roundtrip()
    {
        var original = Result<string, string>.Ok("success");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string, string>>(json, _options);
        Assert.True(deserialized.IsOk);
        Assert.Equal("success", deserialized.GetValue());
    }

    [Fact]
    public void Result_Err_Roundtrip()
    {
        var original = Result<string, string>.Err("failure");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Result<string, string>>(json, _options);
        Assert.True(deserialized.IsErr);
        Assert.Equal("failure", deserialized.GetError());
    }

    #endregion

    #region Either<L, R> Tests

    [Fact]
    public void Either_Right_Serializes()
    {
        var either = Either<string, int>.Right(42);
        var json = JsonSerializer.Serialize(either, _options);
        Assert.Contains("\"isRight\":true", json);
        Assert.Contains("\"right\":42", json);
    }

    [Fact]
    public void Either_Left_Serializes()
    {
        var either = Either<string, int>.Left("left value");
        var json = JsonSerializer.Serialize(either, _options);
        Assert.Contains("\"isRight\":false", json);
        Assert.Contains("\"left\":\"left value\"", json);
    }

    [Fact]
    public void Either_Right_Roundtrip()
    {
        var original = Either<string, int>.Right(100);
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Either<string, int>>(json, _options);
        Assert.True(deserialized.IsRight);
        Assert.Equal(100, deserialized.Match(_ => 0, r => r));
    }

    [Fact]
    public void Either_Left_Roundtrip()
    {
        var original = Either<string, int>.Left("error");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Either<string, int>>(json, _options);
        Assert.True(deserialized.IsLeft);
        Assert.Equal("error", deserialized.Match(l => l, _ => ""));
    }

    #endregion

    #region Try<T> Tests

    [Fact]
    public void Try_Success_Serializes()
    {
        var tryResult = Try<int>.Success(42);
        var json = JsonSerializer.Serialize(tryResult, _options);
        Assert.Contains("\"isSuccess\":true", json);
        Assert.Contains("\"value\":42", json);
    }

    [Fact]
    public void Try_Failure_Serializes()
    {
        var tryResult = Try<int>.Failure(new InvalidOperationException("Something went wrong"));
        var json = JsonSerializer.Serialize(tryResult, _options);
        Assert.Contains("\"isSuccess\":false", json);
        Assert.Contains("\"error\":\"Something went wrong\"", json);
    }

    [Fact]
    public void Try_Success_Roundtrip()
    {
        var original = Try<string>.Success("hello");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Try<string>>(json, _options);
        Assert.True(deserialized.IsSuccess);
        Assert.Equal("hello", deserialized.GetValue());
    }

    [Fact]
    public void Try_Failure_Roundtrip()
    {
        var original = Try<string>.Failure(new Exception("test error"));
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Try<string>>(json, _options);
        Assert.False(deserialized.IsSuccess);
    }

    #endregion

    #region Validation<T, E> Tests

    [Fact]
    public void Validation_Valid_Serializes()
    {
        var validation = Validation<int, string>.Valid(42);
        var json = JsonSerializer.Serialize(validation, _options);
        Assert.Contains("\"isValid\":true", json);
        Assert.Contains("\"value\":42", json);
    }

    [Fact]
    public void Validation_Invalid_Serializes()
    {
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var json = JsonSerializer.Serialize(validation, _options);
        Assert.Contains("\"isValid\":false", json);
        Assert.Contains("\"errors\":", json);
    }

    [Fact]
    public void Validation_Valid_Roundtrip()
    {
        var original = Validation<string, string>.Valid("success");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Validation<string, string>>(json, _options);
        Assert.True(deserialized.IsValid);
        Assert.Equal("success", deserialized.Match(v => v, _ => ""));
    }

    [Fact]
    public void Validation_Invalid_Roundtrip()
    {
        var original = Validation<string, string>.Invalid(new[] { "error1", "error2" });
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Validation<string, string>>(json, _options);
        Assert.False(deserialized.IsValid);
    }

    #endregion

    #region NonEmptyList<T> Tests

    [Fact]
    public void NonEmptyList_Serializes()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3);
        var json = JsonSerializer.Serialize(list, _options);
        Assert.Equal("[1,2,3]", json);
    }

    [Fact]
    public void NonEmptyList_Roundtrip()
    {
        var original = NonEmptyList<string>.Of("a", "b", "c");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<NonEmptyList<string>>(json, _options);
        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.Count);
        Assert.Equal("a", deserialized.Head);
    }

    [Fact]
    public void NonEmptyList_EmptyArray_ThrowsJsonException()
    {
        var json = "[]";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<NonEmptyList<int>>(json, _options));
    }

    #endregion

    #region RemoteData<T, E> Tests

    [Fact]
    public void RemoteData_NotAsked_Serializes()
    {
        var data = RemoteData<int, string>.NotAsked();
        var json = JsonSerializer.Serialize(data, _options);
        Assert.Contains("\"state\":\"NotAsked\"", json);
    }

    [Fact]
    public void RemoteData_Loading_Serializes()
    {
        var data = RemoteData<int, string>.Loading();
        var json = JsonSerializer.Serialize(data, _options);
        Assert.Contains("\"state\":\"Loading\"", json);
    }

    [Fact]
    public void RemoteData_Success_Serializes()
    {
        var data = RemoteData<int, string>.Success(42);
        var json = JsonSerializer.Serialize(data, _options);
        Assert.Contains("\"state\":\"Success\"", json);
        Assert.Contains("\"data\":42", json);
    }

    [Fact]
    public void RemoteData_Failure_Serializes()
    {
        var data = RemoteData<int, string>.Failure("error");
        var json = JsonSerializer.Serialize(data, _options);
        Assert.Contains("\"state\":\"Failure\"", json);
        Assert.Contains("\"error\":\"error\"", json);
    }

    [Fact]
    public void RemoteData_Success_Roundtrip()
    {
        var original = RemoteData<string, string>.Success("data");
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<RemoteData<string, string>>(json, _options);
        Assert.True(deserialized.IsSuccess);
    }

    [Fact]
    public void RemoteData_NotAsked_Roundtrip()
    {
        var original = RemoteData<string, string>.NotAsked();
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<RemoteData<string, string>>(json, _options);
        Assert.True(deserialized.IsNotAsked);
    }

    #endregion

    #region Helper Types

    private record Person
    {
        public string Name { get; init; } = "";
        public int Age { get; init; }
    }

    #endregion
}

