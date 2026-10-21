using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using SFSharp.Runtime.Bootstrap;

namespace SF.Network.Tests;

public sealed class MainThreadExecutionTests
{
    private static MainThreadDispatcher Create() => new(new SFSynchronizationContext(ex => throw ex));

    [Fact]
    public async Task BackgroundDelegateExecutesOnlyWhenOwnerPumps()
    {
        MainThreadDispatcher dispatcher = Create();
        int owner = Environment.CurrentManagedThreadId;
        Task<int>? pending = null;
        OnWorker(() => { pending = dispatcher.InvokeAsync(() => Environment.CurrentManagedThreadId, TestContext.Current.CancellationToken); });

        Assert.False(pending!.IsCompleted);
        dispatcher.Pump();

        Assert.Equal(owner, await pending);
    }

    [Fact]
    public void CancellationPreventsExecutionWithoutPumping()
    {
        MainThreadDispatcher dispatcher = Create();
        using CancellationTokenSource stop = new();
        bool called = false;
        Task? pending = null;
        OnWorker(() => { pending = dispatcher.InvokeAsync(() => { called = true; }, stop.Token); });

        stop.Cancel();

        Assert.True(pending!.IsCanceled);
        dispatcher.Pump();
        Assert.False(called);
    }

    [Fact]
    public void CancellationReleasesCallbackWhileQueueStillContainsWork()
    {
        MainThreadDispatcher dispatcher = Create();
        using CancellationTokenSource stop = new();
        WeakReference owner = Enqueue(dispatcher, stop.Token);

        stop.Cancel();
        for (int i = 0; i < 5 && owner.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }

        Assert.False(owner.IsAlive);
        GC.KeepAlive(dispatcher);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Enqueue(MainThreadDispatcher dispatcher, CancellationToken token)
    {
        CallbackOwner owner = new();
        OnWorker(() => { _ = dispatcher.InvokeAsync(owner.Call, token); });
        return new(owner);
    }

    private sealed class CallbackOwner { public void Call() { } }

    [Fact]
    public async Task ExceptionsAreDeliveredToCallerAndWrongThreadCannotPump()
    {
        MainThreadDispatcher dispatcher = Create();
        OnWorker(() => Assert.Throws<InvalidOperationException>(dispatcher.Pump));
        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.InvokeAsync<int>(() => throw new InvalidOperationException(), TestContext.Current.CancellationToken));
    }

    private static void OnWorker(Action action)
    {
        ExceptionDispatchInfo? fault = null;
        Thread thread = new(() => { try { action(); } catch (Exception ex) { fault = ExceptionDispatchInfo.Capture(ex); } });
        thread.Start();
        thread.Join();
        fault?.Throw();
    }
}
