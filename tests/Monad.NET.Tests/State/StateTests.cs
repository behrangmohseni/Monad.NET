using Xunit;

namespace Monad.NET.Tests;

public class StateTests
{
    #region StateResult Tests

    [Fact]
    public void StateResult_Equals_SameValueAndState_ReturnsTrue()
    {
        var result1 = new StateResult<int, string>("hello", 42);
        var result2 = new StateResult<int, string>("hello", 42);

        Assert.True(result1.Equals(result2));
        Assert.True(result1 == result2);
        Assert.False(result1 != result2);
    }

    [Fact]
    public void StateResult_Equals_DifferentValue_ReturnsFalse()
    {
        var result1 = new StateResult<int, string>("hello", 42);
        var result2 = new StateResult<int, string>("world", 42);

        Assert.False(result1.Equals(result2));
        Assert.False(result1 == result2);
        Assert.True(result1 != result2);
    }

    [Fact]
    public void StateResult_Equals_DifferentState_ReturnsFalse()
    {
        var result1 = new StateResult<int, string>("hello", 42);
        var result2 = new StateResult<int, string>("hello", 100);

        Assert.False(result1.Equals(result2));
        Assert.False(result1 == result2);
        Assert.True(result1 != result2);
    }

    [Fact]
    public void StateResult_Equals_Object_ReturnsCorrectly()
    {
        var result = new StateResult<int, string>("hello", 42);

        Assert.True(result.Equals((object)new StateResult<int, string>("hello", 42)));
        Assert.False(result.Equals((object)"not a state result"));
        Assert.False(result.Equals(null));
    }

    [Fact]
    public void StateResult_GetHashCode_SameForEqualValues()
    {
        var result1 = new StateResult<int, string>("hello", 42);
        var result2 = new StateResult<int, string>("hello", 42);

        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    [Fact]
    public void StateResult_ToString_FormatsCorrectly()
    {
        var result = new StateResult<int, string>("hello", 42);

        var str = result.ToString();

        Assert.Contains("hello", str);
        Assert.Contains("42", str);
        Assert.Contains("StateResult", str);
    }

    [Fact]
    public void StateResult_CanBeUsedInDictionary()
    {
        var result = new StateResult<int, string>("hello", 42);
        var dict = new Dictionary<StateResult<int, string>, string>
        {
            [result] = "test"
        };

        Assert.Equal("test", dict[new StateResult<int, string>("hello", 42)]);
    }

    [Fact]
    public void StateResult_CanBeUsedInHashSet()
    {
        var set = new HashSet<StateResult<int, string>>
        {
            new("hello", 42),
            new("hello", 42), // duplicate
            new("world", 100)
        };

        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void StateResult_Deconstruct_ExtractsComponents()
    {
        var result = new StateResult<int, string>("hello", 42);

        var (value, state) = result;

        Assert.Equal("hello", value);
        Assert.Equal(42, state);
    }

    #endregion

    #region Creation Tests

    [Fact]
    public void Pure_ReturnsValueWithoutModifyingState()
    {
        var state = State<int, string>.Return("hello");
        var result = state.Run(42);

        Assert.Equal("hello", result.Value);
        Assert.Equal(42, result.State);
    }

    [Fact]
    public void Return_ReturnsValueWithoutModifyingState()
    {
        var state = State<int, string>.Return("world");
        var result = state.Run(100);

        Assert.Equal("world", result.Value);
        Assert.Equal(100, result.State);
    }

    [Fact]
    public void Get_ReturnsCurrentState()
    {
        var state = State<int, int>.Get();
        var result = state.Run(42);

        Assert.Equal(42, result.Value);
        Assert.Equal(42, result.State);
    }

    [Fact]
    public void Put_ReplacesState()
    {
        var state = State<int, Unit>.Put(100);
        var result = state.Run(42);

        Assert.Equal(Unit.Default, result.Value);
        Assert.Equal(100, result.State);
    }

    [Fact]
    public void Modify_TransformsState()
    {
        var state = State<int, Unit>.Modify(s => s * 2);
        var result = state.Run(21);

        Assert.Equal(Unit.Default, result.Value);
        Assert.Equal(42, result.State);
    }

    [Fact]
    public void Gets_ExtractsValueFromState()
    {
        var state = State<string, int>.Gets<int>(s => s.Length);
        var result = state.Run("hello");

        Assert.Equal(5, result.Value);
        Assert.Equal("hello", result.State);
    }

    [Fact]
    public void Of_CreatesStateFromFunction()
    {
        var state = State<int, string>.Of(s => new StateResult<int, string>($"Value: {s}", s + 1));
        var result = state.Run(10);

        Assert.Equal("Value: 10", result.Value);
        Assert.Equal(11, result.State);
    }

    [Fact]
    public void Of_WithTuple_CreatesStateFromFunction()
    {
        var state = State<int, string>.Of(s => ($"Value: {s}", s + 1));
        var result = state.Run(10);

        Assert.Equal("Value: 10", result.Value);
        Assert.Equal(11, result.State);
    }

    #endregion

    #region Run Tests

    [Fact]
    public void Eval_ReturnsOnlyValue()
    {
        var state = State<int, string>.Of(s => ("result", s + 100));
        var value = state.Eval(0);

        Assert.Equal("result", value);
    }

    [Fact]
    public void Exec_ReturnsOnlyState()
    {
        var state = State<int, string>.Of(s => ("result", s + 100));
        var finalState = state.Exec(0);

        Assert.Equal(100, finalState);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_TransformsValue()
    {
        var state = State<int, int>.Return(10).Map(x => x * 2);
        var result = state.Run(0);

        Assert.Equal(20, result.Value);
        Assert.Equal(0, result.State);
    }

    [Fact]
    public void Map_PreservesState()
    {
        var state = State<int, int>.Of(s => (s, s + 1)).Map(x => x * 2);
        var result = state.Run(5);

        Assert.Equal(10, result.Value);
        Assert.Equal(6, result.State);
    }

    #endregion

    #region AndThen/FlatMap Tests

    [Fact]
    public void AndThen_ChainsComputations()
    {
        var increment = State<int, Unit>.Modify(s => s + 1);
        var getState = State<int, int>.Get();

        var computation = increment.Bind(_ => getState);
        var result = computation.Run(0);

        Assert.Equal(1, result.Value);
        Assert.Equal(1, result.State);
    }

    [Fact]
    public void FlatMap_ChainsComputations()
    {
        var state = State<int, int>.Return(10)
            .Bind(x => State<int, int>.Of(s => (x + s, s + 1)));

        var result = state.Run(5);

        Assert.Equal(15, result.Value);
        Assert.Equal(6, result.State);
    }

    [Fact]
    public void Bind_ChainsComputations()
    {
        var state = State<int, int>.Return(10)
            .Bind(x => State<int, int>.Return(x * 2));

        var result = state.Run(0);

        Assert.Equal(20, result.Value);
        Assert.Equal(0, result.State);
    }

    [Fact]
    public void AndThen_ThreadsStateThroughChain()
    {
        var increment = State<int, int>.Modify(s => s + 1).Map(_ => 0);

        var computation = increment
            .Bind(_ => increment)
            .Bind(_ => increment)
            .Bind(_ => State<int, int>.Get());

        var result = computation.Run(0);

        Assert.Equal(3, result.Value);
        Assert.Equal(3, result.State);
    }

    #endregion

    #region Apply Tests

    [Fact]
    public void Apply_AppliesWrappedFunction()
    {
        var stateValue = State<int, int>.Return(10);
        var stateFunc = State<int, Func<int, int>>.Return(x => x * 2);

        var result = stateValue.Apply(stateFunc).Run(0);

        Assert.Equal(20, result.Value);
        Assert.Equal(0, result.State);
    }

    #endregion

    #region Zip Tests

    [Fact]
    public void Zip_CombinesValues()
    {
        var state1 = State<int, string>.Of(s => ("a", s + 1));
        var state2 = State<int, string>.Of(s => ("b", s + 1));

        var result = state1.Zip(state2).Run(0);

        Assert.Equal(("a", "b"), result.Value);
        Assert.Equal(2, result.State);
    }

    [Fact]
    public void ZipWith_CombinesValuesWithFunction()
    {
        var state1 = State<int, int>.Return(10);
        var state2 = State<int, int>.Return(20);

        var result = state1.ZipWith(state2, (a, b) => a + b).Run(0);

        Assert.Equal(30, result.Value);
    }

    #endregion

    #region Extension Methods Tests

    [Fact]
    public void Flatten_UnwrapsNestedState()
    {
        var nested = State<int, State<int, string>>.Return(State<int, string>.Return("inner"));
        var flattened = nested.Flatten();

        var result = flattened.Run(0);
        Assert.Equal("inner", result.Value);
    }

    [Fact]
    public void Sequence_ExecutesStatesInOrder()
    {
        var states = new[]
        {
            State<int, int>.Of(s => (s, s + 1)),
            State<int, int>.Of(s => (s, s + 1)),
            State<int, int>.Of(s => (s, s + 1))
        };

        var result = states.Sequence().Run(0);

        Assert.Equal(new[] { 0, 1, 2 }, result.Value);
        Assert.Equal(3, result.State);
    }

    [Fact]
    public void Traverse_AppliesFunctionAndSequences()
    {
        var items = new[] { 1, 2, 3 };

        var result = items.Traverse(x => State<int, int>.Of(s => (x + s, s + 1))).Run(0);

        Assert.Equal(new[] { 1, 3, 5 }, result.Value);
        Assert.Equal(3, result.State);
    }

    [Fact]
    public void Replicate_RepeatsComputation()
    {
        var increment = State<int, int>.Of(s => (s, s + 1));
        var result = increment.Replicate(3).Run(0);

        Assert.Equal(new[] { 0, 1, 2 }, result.Value);
        Assert.Equal(3, result.State);
    }

    [Fact]
    public void WhileM_RepeatsWhileConditionHolds()
    {
        var increment = State<int, int>.Of(s => (s, s + 1));
        var result = increment.WhileM(s => s < 3).Run(0);

        Assert.Equal(new[] { 0, 1, 2 }, result.Value);
        Assert.Equal(3, result.State);
    }

    #endregion

    #region LINQ Tests

    [Fact]
    public void Select_WorksWithLinqSyntax()
    {
        var state = from x in State<int, int>.Return(10)
                    select x * 2;

        var result = state.Run(0);
        Assert.Equal(20, result.Value);
    }

    [Fact]
    public void SelectMany_WorksWithLinqSyntax()
    {
        var state = from x in State<int, int>.Return(10)
                    from y in State<int, int>.Return(20)
                    select x + y;

        var result = state.Run(0);
        Assert.Equal(30, result.Value);
    }

    [Fact]
    public void ComplexLinqExpression_WorksCorrectly()
    {
        var computation =
            from _ in State<int, Unit>.Modify(s => s + 1)
            from a in State<int, int>.Get()
            from __ in State<int, Unit>.Modify(s => s * 2)
            from b in State<int, int>.Get()
            select (a, b);

        var result = computation.Run(5);

        Assert.Equal((6, 12), result.Value);
        Assert.Equal(12, result.State);
    }

    #endregion

    #region Real-World Example Tests

    [Fact]
    public void Counter_Example()
    {
        var increment = State<int, Unit>.Modify(s => s + 1);
        var decrement = State<int, Unit>.Modify(s => s - 1);
        var getCount = State<int, int>.Get();

        var computation =
            from _ in increment
            from __ in increment
            from ___ in increment
            from ____ in decrement
            from count in getCount
            select count;

        var result = computation.Run(0);

        Assert.Equal(2, result.Value);
        Assert.Equal(2, result.State);
    }

    [Fact]
    public void Stack_Example()
    {
        State<List<int>, Unit> Push(int value) =>
            State<List<int>, Unit>.Modify(stack =>
            {
                var newStack = new List<int>(stack) { value };
                return newStack;
            });

        State<List<int>, Option<int>> Pop() =>
            State<List<int>, Option<int>>.Of(stack =>
            {
                if (stack.Count == 0)
                    return (Option<int>.None(), stack);

                var value = stack[^1];
                var newStack = new List<int>(stack);
                newStack.RemoveAt(newStack.Count - 1);
                return (Option<int>.Some(value), newStack);
            });

        var computation =
            from _ in Push(1)
            from __ in Push(2)
            from ___ in Push(3)
            from a in Pop()
            from b in Pop()
            select (a, b);

        var result = computation.Run(new List<int>());

        Assert.True(result.Value.a.IsSome);
        Assert.Equal(3, result.Value.a.GetValue());
        Assert.True(result.Value.b.IsSome);
        Assert.Equal(2, result.Value.b.GetValue());
        Assert.Single(result.State);
        Assert.Equal(1, result.State[0]);
    }

    [Fact]
    public void RandomNumberGenerator_Example()
    {
        // Simple linear congruential generator
        State<int, int> NextRandom() =>
            State<int, int>.Of(seed =>
            {
                var next = ((seed * 1103515245) + 12345) & 0x7FFFFFFF;
                return (next % 100, next); // Return value 0-99
            });

        var computation =
            from a in NextRandom()
            from b in NextRandom()
            from c in NextRandom()
            select new[] { a, b, c };

        var result1 = computation.Run(42);
        var result2 = computation.Run(42);

        // Same seed should produce same results
        Assert.Equal(result1.Value, result2.Value);

        // Different seed should produce different results
        var result3 = computation.Run(123);
        Assert.NotEqual(result1.Value, result3.Value);
    }

    #endregion

    #region As and Void Tests

    [Fact]
    public void As_ReplacesValue()
    {
        var state = State<int, int>.Of(s => (s, s + 1)).As("replaced");
        var result = state.Run(10);

        Assert.Equal("replaced", result.Value);
        Assert.Equal(11, result.State);
    }

    [Fact]
    public void Void_ReplacesValueWithUnit()
    {
        var state = State<int, int>.Of(s => (s, s + 1)).Void();
        var result = state.Run(10);

        Assert.Equal(Unit.Default, result.Value);
        Assert.Equal(11, result.State);
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

    #endregion
}

