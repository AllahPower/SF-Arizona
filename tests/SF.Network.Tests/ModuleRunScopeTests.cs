using SFSharp.Runtime.Modules.Hosting;

namespace SF.Network.Tests;

public sealed class ModuleRunScopeTests
{
    private static ModuleRunScope Create() => new(CancellationToken.None, (_, _) => { }, _ => { });

    [Fact]
    public async Task NormalCompletionCancelsAndDrainsOwnedWorkBeforeReverseDisposal()
    {
        var scope = Create();
        List<int> disposed = [];
        scope.Register(new Resource(() => disposed.Add(1)));
        scope.Register(new Resource(() => disposed.Add(2)));
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task task = scope.Run(async token =>
        {
            started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        }, background: true);
        await started.Task.WaitAsync(TestContext.Current.CancellationToken);

        await scope.DrainAsync();
        Assert.True(task.IsCanceled);
        Assert.Empty(disposed);
        scope.Dispose();
        scope.Dispose();
        Assert.Equal([2, 1], disposed);
    }

    [Fact]
    public async Task CompletedWorkerFaultIsStillObservedByDrain()
    {
        var scope = Create();
        var failure = new InvalidOperationException("worker failed");
        Task task = scope.Run(_ => Task.FromException(failure), background: false);
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => task));

        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => scope.DrainAsync()));
        Assert.True(scope.Token.IsCancellationRequested);
        scope.Dispose();
    }

    [Fact]
    public async Task LateResourceIsDisposedAndLateWorkDoesNotStart()
    {
        var scope = Create();
        await scope.DrainAsync();
        int releases = 0;
        bool ran = false;

        Assert.Throws<ObjectDisposedException>(() => scope.Register(new Resource(() => releases++)));
        Assert.Throws<ObjectDisposedException>(() => { _ = scope.Run(_ => { ran = true; return Task.CompletedTask; }, true); });

        Assert.Equal(1, releases);
        Assert.False(ran);
        scope.Dispose();
    }

    [Fact]
    public async Task ResourcesCannotBeReleasedWhileUncooperativeWorkIsActive()
    {
        var scope = Create();
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = scope.Run(_ => release.Task, background: false);

        Task drain = scope.DrainAsync();
        Assert.False(drain.IsCompleted);
        Assert.Throws<InvalidOperationException>(scope.Dispose);
        release.SetResult();
        await drain;
        scope.Dispose();
    }

    [Fact]
    public async Task CancellationAndResourceFailuresDoNotSkipOtherResources()
    {
        var scope = Create();
        bool released = false;
        scope.Register(new Resource(() => released = true));
        scope.Register(new Resource(() => throw new InvalidOperationException("dispose failed")));
        using var callback = scope.Token.Register(() => throw new InvalidOperationException("cancel failed"));

        await Assert.ThrowsAsync<AggregateException>(() => scope.DrainAsync());
        scope.Dispose();

        Assert.True(released);
    }

    [Fact]
    public async Task ConcurrentRegistrationAndShutdownReleaseEveryResourceExactlyOnce()
    {
        var scope = Create();
        int releases = 0;
        Parallel.For(0, 2000, index =>
        {
            if (index == 500) scope.BeginStop();
            try { scope.Register(new Resource(() => Interlocked.Increment(ref releases))); }
            catch (ObjectDisposedException) { }
        });

        await scope.DrainAsync();
        scope.Dispose();

        Assert.Equal(2000, releases);
    }

    [Fact]
    public async Task OldRunCannotRegisterResourcesIntoNewRun()
    {
        var oldRun = Create();
        await oldRun.DrainAsync();
        oldRun.Dispose();
        using var newRun = Create();
        bool released = false;

        Assert.Throws<ObjectDisposedException>(() => oldRun.Register(new Resource(() => released = true)));
        Assert.True(released);
        newRun.Register(new Resource(() => { }));
    }

    private sealed class Resource(Action release) : IDisposable
    {
        public void Dispose() => release();
    }
}
