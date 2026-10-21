using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using SFSharp.Runtime.Events;

namespace SF.Network.Tests;

public sealed class EventChannelTests
{
    [Fact]
    public void WarmPublicationDoesNotAllocateSnapshots()
    {
        Action<int> publish = null!;
        var channel = new SFEventChannel<int>(callback => { publish = callback; return new Resource(); });
        using var subscription = channel.Subscribe(_ => { });
        for (int i = 0; i < 1000; i++) publish(i);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++) publish(i);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void DisposedSubscriptionDoesNotRetainHandlerOwner()
    {
        var channel = new SFEventChannel<int>(_ => new Resource());
        var (registration, owner) = Subscribe(channel);
        registration.Dispose();
        Collect(owner);

        Assert.False(owner.IsAlive);
        GC.KeepAlive(registration);
        GC.KeepAlive(channel);
    }

    [Fact]
    public void CachedPluginEventTypeDoesNotRetainCollectibleContext()
    {
        var events = new SFEvents(new SFEventFactory(null!, null!, null!));
        WeakReference context = CachePluginTypes(events);

        Collect(context);

        Assert.False(context.IsAlive);
        GC.KeepAlive(events);
    }

    [Fact]
    public void ConcurrentSubscribePublishAndDisposeAreSafe()
    {
        Action<int> publish = null!;
        var channel = new SFEventChannel<int>(callback => { publish = callback; return new Resource(); });
        using var keeper = channel.Subscribe(_ => { });

        Parallel.For(0, 2000, i =>
        {
            using var registration = channel.Subscribe(_ => { });
            publish(i);
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CachePluginTypes(SFEvents events)
    {
        var context = new AssemblyLoadContext("event-cache-test", isCollectible: true);
        var assembly = context.LoadFromAssemblyPath(typeof(EventChannelTests).Assembly.Location);
        Type type = assembly.GetType(typeof(EventChannelTests).FullName!, throwOnError: true)!;
        string[] methods = ["StreamIncomingPacket", "StreamOutgoingPacket", "StreamIncomingRpc", "StreamOutgoingRpc"];
        foreach (string method in methods)
            typeof(SFEvents).GetMethod(method)!.MakeGenericMethod(type).Invoke(events, [CancellationToken.None]);
        context.Unload();
        return new(context);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IDisposable, WeakReference) Subscribe(SFEventChannel<int> channel)
    {
        var owner = new Owner();
        return (channel.Subscribe(owner.Call), new(owner));
    }

    private static void Collect(WeakReference target)
    {
        for (int i = 0; i < 10 && target.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    }

    private sealed class Owner { public void Call(int value) { } }
    private sealed class Resource : IDisposable { public void Dispose() { } }
}
