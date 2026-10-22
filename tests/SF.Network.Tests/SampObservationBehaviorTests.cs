using SFSharp.Abstractions.Samp;
using SFSharp.Abstractions.Samp.Events;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Gta;
using SFSharp.Runtime.Interop.Hooking;
using SFSharp.Runtime.Interop.Hooking.Hooks;
using SFSharp.Runtime.Samp.Events;

namespace SF.Network.Tests;

public sealed class SampObservationBehaviorTests
{
    [Fact]
    public void FailedGroupInstallationReleasesResourcesInReverseOrderWithoutMaskingTheFailure()
    {
        var order = new List<int>();
        var failure = new InvalidOperationException("installation failed");

        Exception actual = Assert.Throws<InvalidOperationException>(() => SampObservationHooks.Create(hooks =>
        {
            hooks.Own(new Cleanup(() => order.Add(1)));
            hooks.Own(new Cleanup(() => { order.Add(2); throw new InvalidOperationException("cleanup failed"); }));
            throw failure;
        }));

        Assert.Same(failure, actual);
        Assert.Equal([2, 1], order);
    }

    [Fact]
    public void SessionStateSamplingPublishesInitialAndChangesWithoutDuplicates()
    {
        var main = new MainThreadDispatcher(new SFSynchronizationContext(_ => { }));
        var clock = new SFGameClock { IsAvailable = true };
        SFSampGameState state = SFSampGameState.WaitingToConnect;
        var events = new SampSessionEvents(main, clock, () => state);
        var received = new List<SFSampSessionStateEvent>();
        IDisposable subscription = events.OnStateChanged(received.Add);

        clock.Publish(1, 100, 1, false);
        clock.Publish(2, 120, 1, false);
        state = SFSampGameState.Connected;
        clock.Publish(3, 140, 1, false);
        subscription.Dispose();
        state = SFSampGameState.Restarting;
        clock.Publish(4, 160, 1, false);

        Assert.Equal(new SFSampSessionStateEvent(null, SFSampGameState.WaitingToConnect), received[0]);
        Assert.Equal(new SFSampSessionStateEvent(SFSampGameState.WaitingToConnect, SFSampGameState.Connected), received[1]);
        Assert.Equal(2, received.Count);
    }

    [Fact]
    public void RemoteStateTrackingHandlesNestedTransitionsIdReuseAndExplicitRemoval()
    {
        var tracker = new SampRemoteStateTracker();

        Assert.Equal(new SFSampRemoteStateEvent(73, null, 17), tracker.Observe(0x1234, 73, 17));
        Assert.Null(tracker.Observe(0x1234, 73, 17));
        Assert.Equal(new SFSampRemoteStateEvent(73, 17, 19), tracker.Observe(0x1234, 73, 19));
        Assert.Equal(new SFSampRemoteStateEvent(73, null, 18), tracker.Observe(0x5678, 73, 18));
        tracker.Forget(73);
        Assert.Equal(new SFSampRemoteStateEvent(73, null, 17), tracker.Observe(0x5678, 73, 17));
        Assert.Null(tracker.Observe(0, 73, 17));
        Assert.Null(tracker.Observe(0x1234, ushort.MaxValue, 17));
    }

    [Fact]
    public void SnapshotCaptureAndCompletionSurroundExactlyOneOriginalCall()
    {
        var order = new List<string>();
        var hook = new TestHook(() => order.Add("native"));
        hook.AddSubHook(new SampObservation<int>(_ => { order.Add("before"); return 42; }, (_, capture) =>
        {
            Assert.Equal(42, capture);
            order.Add("after");
        }));

        hook.Call();

        Assert.Equal(["before", "native", "after"], order);
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void FailedCaptureStillCallsOriginalOnceWithoutCompletion()
    {
        var hook = new TestHook(() => { });
        bool completed = false;
        hook.AddSubHook(new SampObservation<int>(_ => throw new InvalidOperationException(), (_, _) => completed = true));

        hook.Call();

        Assert.Equal(1, hook.Calls);
        Assert.False(completed);
    }

    [Fact]
    public void DisabledObservationSkipsSnapshotWorkButNotOriginal()
    {
        var hook = new TestHook(() => { });
        int captured = 0;
        hook.AddSubHook(new SampObservation<int>(_ => ++captured, (_, _) => { }, () => false));

        hook.Call();

        Assert.Equal(1, hook.Calls);
        Assert.Equal(0, captured);
    }

    [Fact]
    public void WarmObservationAndStateTrackingAllocateNothing()
    {
        var hook = new TestHook(() => { });
        var tracker = new SampRemoteStateTracker();
        hook.AddSubHook(new SampObservation<int>(_ => 42, (_, _) => tracker.Observe(0x1234, 73, 17)));
        for (int i = 0; i < 1_000; i++) hook.Call();
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 10_000; i++) hook.Call();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
        Assert.Equal(11_000, hook.Calls);
    }

    private sealed class TestHook(Action original) : HookBase<SampObservationArgs, NoRetValue>
    {
        public int Calls;
        public void Call() => Process(default);
        protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
        { Calls++; original(); return default; }
    }

    private sealed class Cleanup(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
