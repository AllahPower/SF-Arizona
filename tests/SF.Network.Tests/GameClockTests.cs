using System.Runtime.CompilerServices;
using SFSharp.Abstractions.Gta;
using SFSharp.Runtime.Gta;

namespace SF.Network.Tests;

public sealed class GameClockTests
{
    [Fact]
    public void DeduplicatesFramesAndHandlesPauseAndTimerWrap()
    {
        SFGameClock clock = new() { IsAvailable = true };
        int calls = 0;
        using IDisposable subscription = clock.Subscribe(_ => calls++);

        clock.Publish(1, uint.MaxValue - 9, 1, false);
        clock.Publish(1, uint.MaxValue - 9, 1, false);
        clock.Publish(2, 10, 1, false);
        Assert.Equal(TimeSpan.FromMilliseconds(20), clock.Current.Delta);
        clock.Publish(3, 40, 0, true);

        Assert.Equal(3, calls);
        Assert.Equal(TimeSpan.Zero, clock.Current.Delta);
        clock.Publish(4, 0, 1, false);
        Assert.Equal(TimeSpan.Zero, clock.Current.Delta);
    }

    [Fact]
    public void SteadyTickDoesNotAllocateWithOrWithoutSubscriber()
    {
        SFGameClock clock = new() { IsAvailable = true };
        for (uint i = 0; i < 1000; i++) clock.Publish(i, i * 20, 1, false);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (uint i = 1000; i < 11000; i++) clock.Publish(i, i * 20, 1, false);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);

        using IDisposable subscription = clock.Subscribe(static _ => { });
        before = GC.GetAllocatedBytesForCurrentThread();
        for (uint i = 11000; i < 21000; i++) clock.Publish(i, i * 20, 1, false);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void DisposedSubscriptionDoesNotRetainOwner()
    {
        SFGameClock clock = new() { IsAvailable = true };
        (WeakReference owner, IDisposable subscription) = Subscribe(clock);
        subscription.Dispose();
        for (int i = 0; i < 5 && owner.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }

        Assert.False(owner.IsAlive);
        GC.KeepAlive(clock);
        GC.KeepAlive(subscription);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, IDisposable) Subscribe(SFGameClock clock)
    {
        Listener listener = new();
        return (new(listener), clock.Subscribe(listener.Tick));
    }

    private sealed class Listener { public void Tick(SFGameTick value) { } }
}
