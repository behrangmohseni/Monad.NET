using Monad.NET;

namespace Monad.NET.Tests;

/// <summary>
/// Extended tests for State<TState, T> to improve code coverage.
/// </summary>
public class StateExtendedTests
{
    #region Factory Tests

    [Fact]
    public void Pure_ReturnsValueWithoutModifyingState()
    {
        var state = State<int, string>.Pure("hello");
        var result = state.Run(42);

        Assert.Equal("hello", result.Value);
        Assert.Equal(42, result.State);
    }

    [Fact]
    public void Return_ReturnsValueWithoutModifyingState()
    {
        var state = State<int, string>.Return("hello");
        var result = state.Run(42);

        Assert.Equal("hello", result.Value);
        Assert.Equal(42, result.State);
    }

    [Fact]
    public void Get_ReturnsCurrentStateAsValue()
    {
        var state = State<int, int>.Get();
        var result = state.Run(42);

        Assert.Equal(42, result.Value);
        Assert.Equal(42, result.State);
    }

    [Fact]
    public void Gets_ExtractsValueFromState()
    {
        var state = State<string, int>.Gets(s => s.Length);
        var result = state.Run("hello");

        Assert.Equal(5, result.Value);
        Assert.Equal("hello", result.State);
    }

    [Fact]
    public void Of_WithStateResult_CreatesState()
    {
        var state = State<int, string>.Of(s => new StateResult<int, string>($"Value is {s}", s + 1));
        var result = state.Run(10);

        Assert.Equal("Value is 10", result.Value);
        Assert.Equal(11, result.State);
    }

    [Fact]
    public void Of_WithTuple_CreatesState()
    {
        var state = State<int, string>.Of(s => ($"Value is {s}", s + 1));
        var result = state.Run(10);

        Assert.Equal("Value is 10", result.Value);
        Assert.Equal(11, result.State);
    }

    #endregion

    #region Eval and Exec Tests

    [Fact]
    public void Eval_ReturnsOnlyValue()
    {
        var state = State<int, string>.Pure("hello");
        var value = state.Eval(42);

        Assert.Equal("hello", value);
    }

    [Fact]
    public void Exec_ReturnsOnlyFinalState()
    {
        var state = State<int, Unit>.Modify(s => s * 2);
        var finalState = state.Exec(21);

        Assert.Equal(42, finalState);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_TransformsValue()
    {
        var state = State<int, int>.Pure(10).Map(x => x * 2);
        var result = state.Run(0);

        Assert.Equal(20, result.Value);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_ExecutesActionWithValue()
    {
        var capturedValue = 0;
        var state = State<int, int>.Pure(42).Tap(v => capturedValue = v);
        var result = state.Run(0);

        Assert.Equal(42, capturedValue);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void TapState_ExecutesActionWithState()
    {
        var capturedState = 0;
        var state = State<int, string>.Pure("hello").TapState(s => capturedState = s);
        var result = state.Run(42);

        Assert.Equal(42, capturedState);
        Assert.Equal("hello", result.Value);
    }

    #endregion

    #region AndThen/FlatMap/Bind Tests

    [Fact]
    public void AndThen_ChainsComputations()
    {
        var state = State<int, int>.Get()
            .AndThen(x => State<int, string>.Pure($"Value: {x}"));
        var result = state.Run(42);

        Assert.Equal("Value: 42", result.Value);
    }

    [Fact]
    public void FlatMap_ChainsComputations()
    {
        var state = State<int, int>.Get()
            .FlatMap(x => State<int, string>.Pure($"Value: {x}"));
        var result = state.Run(42);

        Assert.Equal("Value: 42", result.Value);
    }

    [Fact]
    public void Bind_ChainsComputations()
    {
        var state = State<int, int>.Get()
            .Bind(x => State<int, string>.Pure($"Value: {x}"));
        var result = state.Run(42);

        Assert.Equal("Value: 42", result.Value);
    }

    #endregion

    #region Apply Tests

    [Fact]
    public void Apply_AppliesWrappedFunction()
    {
        var stateFunc = State<int, Func<int, int>>.Pure(x => x * 2);
        var stateValue = State<int, int>.Pure(21);
        var result = stateValue.Apply(stateFunc).Run(0);

        Assert.Equal(42, result.Value);
    }

    #endregion

    #region Zip Tests

    [Fact]
    public void Zip_CombinesTwoStates()
    {
        var state1 = State<int, string>.Pure("hello");
        var state2 = State<int, int>.Pure(42);
        var result = state1.Zip(state2).Run(0);

        Assert.Equal(("hello", 42), result.Value);
    }

    [Fact]
    public void ZipWith_CombinesWithFunction()
    {
        var state1 = State<int, int>.Pure(10);
        var state2 = State<int, int>.Pure(32);
        var result = state1.ZipWith(state2, (a, b) => a + b).Run(0);

        Assert.Equal(42, result.Value);
    }

    #endregion

    #region As and Void Tests

    [Fact]
    public void As_ReplacesValue()
    {
        var state = State<int, int>.Pure(10).As("replaced");
        var result = state.Run(0);

        Assert.Equal("replaced", result.Value);
    }

    [Fact]
    public void Void_ReplacesValueWithUnit()
    {
        var state = State<int, int>.Pure(42).Void();
        var result = state.Run(0);

        Assert.Equal(Unit.Default, result.Value);
    }

    #endregion

    #region StateResult Tests

    [Fact]
    public void StateResult_Deconstruct_Works()
    {
        var stateResult = new StateResult<int, string>("value", 42);
        var (value, state) = stateResult;

        Assert.Equal("value", value);
        Assert.Equal(42, state);
    }

    [Fact]
    public void StateResult_Equals_SameValues_ReturnsTrue()
    {
        var sr1 = new StateResult<int, string>("value", 42);
        var sr2 = new StateResult<int, string>("value", 42);

        Assert.True(sr1.Equals(sr2));
        Assert.True(sr1 == sr2);
    }

    [Fact]
    public void StateResult_Equals_DifferentValues_ReturnsFalse()
    {
        var sr1 = new StateResult<int, string>("value1", 42);
        var sr2 = new StateResult<int, string>("value2", 42);

        Assert.False(sr1.Equals(sr2));
        Assert.True(sr1 != sr2);
    }

    [Fact]
    public void StateResult_Equals_Object_Works()
    {
        var sr1 = new StateResult<int, string>("value", 42);
        object sr2 = new StateResult<int, string>("value", 42);
        object notStateResult = "not a state result";

        Assert.True(sr1.Equals(sr2));
        Assert.False(sr1.Equals(notStateResult));
    }

    [Fact]
    public void StateResult_GetHashCode_SameForEqual()
    {
        var sr1 = new StateResult<int, string>("value", 42);
        var sr2 = new StateResult<int, string>("value", 42);

        Assert.Equal(sr1.GetHashCode(), sr2.GetHashCode());
    }

    [Fact]
    public void StateResult_ToString_ContainsInfo()
    {
        var sr = new StateResult<int, string>("value", 42);
        var str = sr.ToString();

        Assert.Contains("value", str);
        Assert.Contains("42", str);
    }

    #endregion

    #region Extension Tests

    [Fact]
    public void Flatten_UnwrapsNestedState()
    {
        var nested = State<int, State<int, string>>.Pure(State<int, string>.Pure("inner"));
        var flattened = nested.Flatten();
        var result = flattened.Run(0);

        Assert.Equal("inner", result.Value);
    }

    [Fact]
    public void Sequence_CombinesMultipleStates()
    {
        var states = new[]
        {
            State<int, int>.Modify(s => s + 1).Map(_ => 1),
            State<int, int>.Modify(s => s + 1).Map(_ => 2),
            State<int, int>.Modify(s => s + 1).Map(_ => 3)
        };

        var sequenced = states.Sequence();
        var result = sequenced.Run(0);

        Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        Assert.Equal(3, result.State);
    }

    [Fact]
    public void Traverse_AppliesFunctionToEach()
    {
        var source = new[] { 1, 2, 3 };
        var traversed = source.Traverse(x =>
            State<int, int>.Modify(s => s + x).Map(_ => x * 10));
        var result = traversed.Run(0);

        Assert.Equal(new[] { 10, 20, 30 }, result.Value);
        Assert.Equal(6, result.State);
    }

    [Fact]
    public void Replicate_RepeatsComputation()
    {
        var counter = State<int, int>.Modify(s => s + 1).AndThen(_ => State<int, int>.Get());
        var replicated = counter.Replicate(3);
        var result = replicated.Run(0);

        Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        Assert.Equal(3, result.State);
    }

    [Fact]
    public void WhileM_RunsWhileConditionHolds()
    {
        var increment = State<int, int>.Modify(s => s + 1).AndThen(_ => State<int, int>.Get());
        var result = increment.WhileM(s => s < 5).Run(0);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Value);
        Assert.Equal(5, result.State);
    }

    [Fact]
    public void Select_LinqSupport()
    {
        var state = from x in State<int, int>.Get()
                    select x * 2;
        var result = state.Run(21);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void SelectMany_LinqSupport()
    {
        var state = from x in State<int, int>.Get()
                    from _ in State<int, Unit>.Modify(s => s * 2)
                    from y in State<int, int>.Get()
                    select (x, y);
        var result = state.Run(10);

        Assert.Equal((10, 20), result.Value);
        Assert.Equal(20, result.State);
    }

    #endregion
}

