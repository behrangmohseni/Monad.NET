using Xunit;

namespace Monad.NET.Tests;

public class TapTests
{
    #region Option.Tap Tests

    [Fact]
    public void Option_Tap_Some_ExecutesAction()
    {
        var option = Option<int>.Some(42);
        var executed = false;
        var capturedValue = 0;

        var result = option.Tap(x =>
        {
            executed = true;
            capturedValue = x;
        });

        Assert.True(executed);
        Assert.Equal(42, capturedValue);
        Assert.Equal(option, result);
    }

    [Fact]
    public void Option_Tap_None_DoesNotExecuteAction()
    {
        var option = Option<int>.None();
        var executed = false;

        var result = option.Tap(_ => executed = true);

        Assert.False(executed);
        Assert.Equal(option, result);
    }

    [Fact]
    public void Option_TapNone_None_ExecutesAction()
    {
        var option = Option<int>.None();
        var executed = false;

        var result = option.TapNone(() => executed = true);

        Assert.True(executed);
        Assert.Equal(option, result);
    }

    [Fact]
    public void Option_TapNone_Some_DoesNotExecuteAction()
    {
        var option = Option<int>.Some(42);
        var executed = false;

        var result = option.TapNone(() => executed = true);

        Assert.False(executed);
        Assert.Equal(option, result);
    }

    [Fact]
    public void Option_Tap_ChainsCorrectly()
    {
        var log = new List<string>();

        var result = Option<int>.Some(10)
            .Tap(x => log.Add($"Start: {x}"))
            .Map(x => x * 2)
            .Tap(x => log.Add($"After map: {x}"))
            .Filter(x => x > 10)
            .Tap(x => log.Add($"After filter: {x}"));

        Assert.Equal(3, log.Count);
        Assert.Equal("Start: 10", log[0]);
        Assert.Equal("After map: 20", log[1]);
        Assert.Equal("After filter: 20", log[2]);
    }

    #endregion

    #region State.Tap Tests

    [Fact]
    public void State_Tap_ExecutesAction()
    {
        var state = State<int, string>.Return("hello");
        var executed = false;
        var capturedValue = "";

        var result = state.Tap(x =>
        {
            executed = true;
            capturedValue = x;
        }).Run(0);

        Assert.True(executed);
        Assert.Equal("hello", capturedValue);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void State_TapState_ExecutesAction()
    {
        var state = State<int, string>.Return("hello");
        var capturedState = 0;

        var result = state.TapState(s => capturedState = s).Run(42);

        Assert.Equal(42, capturedState);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void State_Tap_ChainsCorrectly()
    {
        var log = new List<string>();

        var result = State<int, int>.Get()
            .Tap(x => log.Add($"Initial: {x}"))
            .Map(x => x * 2)
            .Tap(x => log.Add($"Doubled: {x}"))
            .Run(5);

        Assert.Equal(2, log.Count);
        Assert.Equal("Initial: 5", log[0]);
        Assert.Equal("Doubled: 10", log[1]);
        Assert.Equal(10, result.Value);
    }

    #endregion

    #region Reader.Tap Tests

    [Fact]
    public void Reader_Tap_ExecutesAction()
    {
        var reader = Reader<int, string>.Ask().Map(x => $"Value: {x}");
        var executed = false;
        var capturedValue = "";

        var result = reader.Tap(x =>
        {
            executed = true;
            capturedValue = x;
        }).Run(42);

        Assert.True(executed);
        Assert.Equal("Value: 42", capturedValue);
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Reader_TapEnv_ExecutesAction()
    {
        var reader = Reader<int, string>.Ask().Map(x => $"Value: {x}");
        var capturedEnv = 0;

        var result = reader.TapEnv(env => capturedEnv = env).Run(42);

        Assert.Equal(42, capturedEnv);
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Reader_Tap_ChainsCorrectly()
    {
        var log = new List<string>();

        var result = Reader<int, int>.Ask()
            .Tap(x => log.Add($"Env: {x}"))
            .Map(x => x * 2)
            .Tap(x => log.Add($"Doubled: {x}"))
            .Run(5);

        Assert.Equal(2, log.Count);
        Assert.Equal("Env: 5", log[0]);
        Assert.Equal("Doubled: 10", log[1]);
        Assert.Equal(10, result);
    }

    #endregion

    #region Writer.Tap Tests

    [Fact]
    public void Writer_Tap_ExecutesAction()
    {
        var writer = Writer<string, int>.Of(42, "initial");
        var executed = false;
        var capturedValue = 0;

        var result = writer.Tap(x =>
        {
            executed = true;
            capturedValue = x;
        });

        Assert.True(executed);
        Assert.Equal(42, capturedValue);
        Assert.Equal(42, result.Value);
        Assert.Equal("initial", result.Log);
    }

    [Fact]
    public void Writer_TapLog_ExecutesAction()
    {
        var writer = Writer<string, int>.Of(42, "test log");
        var capturedLog = "";

        var result = writer.TapLog(log => capturedLog = log);

        Assert.Equal("test log", capturedLog);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Writer_Tap_ChainsCorrectly()
    {
        var log = new List<string>();

        var result = Writer<string, int>.Of(10, "start")
            .Tap(x => log.Add($"Value: {x}"))
            .Map(x => x * 2)
            .Tap(x => log.Add($"Doubled: {x}"));

        Assert.Equal(2, log.Count);
        Assert.Equal("Value: 10", log[0]);
        Assert.Equal("Doubled: 20", log[1]);
        Assert.Equal(20, result.Value);
    }

    #endregion

    #region NonEmptyList.Tap Tests

    [Fact]
    public void NonEmptyList_Tap_ExecutesForEachElement()
    {
        var list = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        var collected = new List<int>();

        var result = list.Tap(x => collected.Add(x));

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, collected);
        Assert.Equal(list, result);
    }

    [Fact]
    public void NonEmptyList_TapIndexed_ExecutesWithIndices()
    {
        var list = NonEmptyList<string>.Of("a", "b", "c");
        var collected = new List<string>();

        var result = list.TapIndexed((x, i) => collected.Add($"{i}:{x}"));

        Assert.Equal(new[] { "0:a", "1:b", "2:c" }, collected);
        Assert.Equal(list, result);
    }

    [Fact]
    public void NonEmptyList_Tap_ChainsCorrectly()
    {
        var log = new List<string>();

        var result = NonEmptyList<int>.Of(1, 2, 3)
            .Tap(x => log.Add($"Before: {x}"))
            .Map(x => x * 10)
            .Tap(x => log.Add($"After: {x}"));

        Assert.Equal(6, log.Count); // 3 before + 3 after
        Assert.Contains("Before: 1", log);
        Assert.Contains("After: 10", log);
        Assert.Equal(10, result.Head);
    }

    [Fact]
    public void NonEmptyList_Tap_SingleElement()
    {
        var list = NonEmptyList<int>.Of(42);
        var collected = new List<int>();

        list.Tap(x => collected.Add(x));

        Assert.Single(collected);
        Assert.Equal(42, collected[0]);
    }

    #endregion

    #region Existing Tap Methods Still Work

    [Fact]
    public void Result_Tap_StillWorks()
    {
        var result = Result<int, string>.Ok(42);
        var executed = false;

        result.Tap(x => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void Result_TapErr_StillWorks()
    {
        var result = Result<int, string>.Error("error");
        var executed = false;

        result.TapError(e => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void Try_Tap_StillWorks()
    {
        var tryResult = Try<int>.Ok(42);
        var executed = false;

        tryResult.Tap(x => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void Validation_Tap_StillWorks()
    {
        var validation = Validation<int, string>.Ok(42);
        var executed = false;

        validation.Tap(x => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void RemoteData_Tap_StillWorks()
    {
        var remoteData = RemoteData<int, string>.Ok(42);
        var executed = false;

        remoteData.Tap(x => executed = true);

        Assert.True(executed);
    }

    #endregion
}

