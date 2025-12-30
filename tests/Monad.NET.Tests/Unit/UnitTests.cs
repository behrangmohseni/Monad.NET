using Xunit;

namespace Monad.NET.Tests;

public class UnitTests
{
    [Fact]
    public void Value_ReturnsSingletonUnit()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        Assert.Equal(unit1, unit2);
    }

    [Fact]
    public void Default_ReturnsSingletonUnit()
    {
        var unit1 = Unit.Default;
        var unit2 = Unit.Value;

        Assert.Equal(unit1, unit2);
    }

    [Fact]
    public async Task Task_ReturnsCompletedTask()
    {
        var task = Unit.Task;

        Assert.True(task.IsCompleted);
        Assert.Equal(Unit.Value, await task);
    }

    [Fact]
    public void From_ExecutesActionAndReturnsUnit()
    {
        var executed = false;

        var result = Unit.From(() => executed = true);

        Assert.True(executed);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public void From_ThrowsOnNullAction()
    {
        Assert.Throws<ArgumentNullException>(() => Unit.From(null!));
    }

    [Fact]
    public async Task FromAsync_ExecutesActionAndReturnsUnit()
    {
        var executed = false;

        var result = await Unit.FromAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task FromAsync_ThrowsOnNullAction()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Unit.FromAsync((Func<Task>)null!));
    }

    [Fact]
    public async Task FromAsync_WithCancellation_ExecutesAction()
    {
        var executed = false;
        using var cts = new CancellationTokenSource();

        var result = await Unit.FromAsync(async ct =>
        {
            await Task.Delay(1, ct);
            executed = true;
        }, cts.Token);

        Assert.True(executed);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public void Equals_TwoUnits_ReturnsTrue()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Default;

        Assert.True(unit1.Equals(unit2));
        Assert.True(unit1.Equals((object)unit2));
    }

    [Fact]
    public void Equals_NonUnitObject_ReturnsFalse()
    {
        var unit = Unit.Value;

        Assert.False(unit.Equals("not a unit"));
        Assert.False(unit.Equals(42));
        Assert.False(unit.Equals(null));
    }

    [Fact]
    public void GetHashCode_AlwaysReturnsZero()
    {
        Assert.Equal(0, Unit.Value.GetHashCode());
        Assert.Equal(0, Unit.Default.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsParentheses()
    {
        Assert.Equal("()", Unit.Value.ToString());
    }

    [Fact]
    public void CompareTo_Unit_ReturnsZero()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Default;

        Assert.Equal(0, unit1.CompareTo(unit2));
    }

    [Fact]
    public void CompareTo_Object_Unit_ReturnsZero()
    {
        var unit = Unit.Value;

        Assert.Equal(0, unit.CompareTo((object)Unit.Default));
    }

    [Fact]
    public void CompareTo_Object_Null_ReturnsPositive()
    {
        var unit = Unit.Value;

        Assert.Equal(1, unit.CompareTo(null));
    }

    [Fact]
    public void CompareTo_Object_NonUnit_ThrowsArgumentException()
    {
        var unit = Unit.Value;

        Assert.Throws<ArgumentException>(() => unit.CompareTo("not a unit"));
    }

    [Fact]
    public void EqualityOperator_ReturnsTrue()
    {
        Assert.True(Unit.Value == Unit.Default);
    }

    [Fact]
    public void InequalityOperator_ReturnsFalse()
    {
        Assert.False(Unit.Value != Unit.Default);
    }

    [Fact]
    public void LessThanOperator_ReturnsFalse()
    {
        Assert.False(Unit.Value < Unit.Default);
    }

    [Fact]
    public void LessThanOrEqualOperator_ReturnsTrue()
    {
        Assert.True(Unit.Value <= Unit.Default);
    }

    [Fact]
    public void GreaterThanOperator_ReturnsFalse()
    {
        Assert.False(Unit.Value > Unit.Default);
    }

    [Fact]
    public void GreaterThanOrEqualOperator_ReturnsTrue()
    {
        Assert.True(Unit.Value >= Unit.Default);
    }

    [Fact]
    public void Unit_CanBeUsedInDictionary()
    {
        var dict = new Dictionary<Unit, string>
        {
            [Unit.Value] = "test"
        };

        Assert.Equal("test", dict[Unit.Default]);
    }

    [Fact]
    public void Unit_CanBeUsedInHashSet()
    {
        var set = new HashSet<Unit> { Unit.Value, Unit.Default };

        Assert.Single(set); // Both are equal, so only one element
    }

    [Fact]
    public void Unit_CanBeSorted()
    {
        var list = new List<Unit> { Unit.Value, Unit.Default, Unit.Value };

        list.Sort();

        Assert.Equal(3, list.Count); // Sorting should work without issues
    }
}

