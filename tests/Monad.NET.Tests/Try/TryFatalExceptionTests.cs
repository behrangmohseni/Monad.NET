using Xunit;

namespace Monad.NET.Tests;

public class TryFatalExceptionTests
{
    #region OperationCanceledException propagation

    [Fact]
    public void Of_RethrowsOperationCanceledException()
    {
        Assert.Throws<OperationCanceledException>(() =>
            Try<int>.Of(() => throw new OperationCanceledException()));
    }

    [Fact]
    public void Map_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OperationCanceledException>(() =>
            t.Map<int>(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public void Bind_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OperationCanceledException>(() =>
            t.Bind<int>(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public void Filter_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OperationCanceledException>(() =>
            t.Filter(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public void Filter_WithMessage_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OperationCanceledException>(() =>
            t.Filter(_ => throw new OperationCanceledException(), "msg"));
    }

    [Fact]
    public void Filter_WithFactory_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OperationCanceledException>(() =>
            t.Filter(_ => throw new OperationCanceledException(), () => new Exception()));
    }

    [Fact]
    public void Recover_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Error(new Exception("fail"));
        Assert.Throws<OperationCanceledException>(() =>
            t.Recover(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public void RecoverWith_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Error(new Exception("fail"));
        Assert.Throws<OperationCanceledException>(() =>
            t.RecoverWith(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public void ZipWith_RethrowsOperationCanceledException()
    {
        var t1 = Try<int>.Ok(1);
        var t2 = Try<int>.Ok(2);
        Assert.Throws<OperationCanceledException>(() =>
            t1.ZipWith<int, int>(t2, (_, _) => throw new OperationCanceledException()));
    }

    [Fact]
    public void Tap_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OperationCanceledException>(() =>
            t.Tap(_ => throw new OperationCanceledException()));
    }

    #endregion

    #region TaskCanceledException propagation (derives from OperationCanceledException)

    [Fact]
    public void Map_RethrowsTaskCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<TaskCanceledException>(() =>
            t.Map<int>(_ => throw new TaskCanceledException()));
    }

    [Fact]
    public void Bind_RethrowsTaskCanceledException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<TaskCanceledException>(() =>
            t.Bind<int>(_ => throw new TaskCanceledException()));
    }

    #endregion

    #region OutOfMemoryException propagation

    [Fact]
    public void Of_RethrowsOutOfMemoryException()
    {
        Assert.Throws<OutOfMemoryException>(() =>
            Try<int>.Of(() => throw new OutOfMemoryException()));
    }

    [Fact]
    public void Map_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OutOfMemoryException>(() =>
            t.Map<int>(_ => throw new OutOfMemoryException()));
    }

    [Fact]
    public void Bind_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OutOfMemoryException>(() =>
            t.Bind<int>(_ => throw new OutOfMemoryException()));
    }

    [Fact]
    public void Filter_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Ok(1);
        Assert.Throws<OutOfMemoryException>(() =>
            t.Filter(_ => throw new OutOfMemoryException()));
    }

    [Fact]
    public void Recover_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Error(new Exception("fail"));
        Assert.Throws<OutOfMemoryException>(() =>
            t.Recover(_ => throw new OutOfMemoryException()));
    }

    [Fact]
    public void RecoverWith_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Error(new Exception("fail"));
        Assert.Throws<OutOfMemoryException>(() =>
            t.RecoverWith(_ => throw new OutOfMemoryException()));
    }

    #endregion

    #region Non-fatal exceptions are still captured

    [Fact]
    public void Of_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Of(() => throw new InvalidOperationException("test"));
        Assert.True(t.IsError);
        Assert.IsType<InvalidOperationException>(t.GetException());
    }

    [Fact]
    public void Map_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Ok(1).Map<int>(_ => throw new InvalidOperationException("test"));
        Assert.True(t.IsError);
        Assert.IsType<InvalidOperationException>(t.GetException());
    }

    [Fact]
    public void Bind_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Ok(1).Bind<int>(_ => throw new InvalidOperationException("test"));
        Assert.True(t.IsError);
        Assert.IsType<InvalidOperationException>(t.GetException());
    }

    [Fact]
    public void Filter_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Ok(1).Filter(_ => throw new InvalidOperationException("test"));
        Assert.True(t.IsError);
        Assert.IsType<InvalidOperationException>(t.GetException());
    }

    [Fact]
    public void Recover_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Error(new Exception("fail"))
            .Recover(_ => throw new InvalidOperationException("recovery failed"));
        Assert.True(t.IsError);
        Assert.IsType<InvalidOperationException>(t.GetException());
    }

    [Fact]
    public void RecoverWith_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Error(new Exception("fail"))
            .RecoverWith(_ => throw new InvalidOperationException("recovery failed"));
        Assert.True(t.IsError);
        Assert.IsType<InvalidOperationException>(t.GetException());
    }

    #endregion

    #region Async methods propagate fatal exceptions

    [Fact]
    public async Task OfAsync_RethrowsOperationCanceledException()
    {
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            Try<int>.OfAsync(() => throw new OperationCanceledException()));
    }

    [Fact]
    public async Task OfAsync_RethrowsOutOfMemoryException()
    {
        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            Try<int>.OfAsync(() => throw new OutOfMemoryException()));
    }

    [Fact]
    public async Task MapAsync_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            t.MapAsync<int, int>(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public async Task MapAsync_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Ok(1);
        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            t.MapAsync<int, int>(_ => throw new OutOfMemoryException()));
    }

    [Fact]
    public async Task BindAsync_RethrowsOperationCanceledException()
    {
        var t = Try<int>.Ok(1);
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            t.BindAsync<int, int>(_ => throw new OperationCanceledException()));
    }

    [Fact]
    public async Task BindAsync_RethrowsOutOfMemoryException()
    {
        var t = Try<int>.Ok(1);
        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            t.BindAsync<int, int>(_ => throw new OutOfMemoryException()));
    }

    [Fact]
    public async Task MapAsync_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Ok(1);
        var result = await t.MapAsync<int, int>(_ => throw new InvalidOperationException("test"));
        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    [Fact]
    public async Task BindAsync_CapturesNonFatalExceptions()
    {
        var t = Try<int>.Ok(1);
        var result = await t.BindAsync<int, int>(_ => throw new InvalidOperationException("test"));
        Assert.True(result.IsError);
        Assert.IsType<InvalidOperationException>(result.GetException());
    }

    #endregion
}
