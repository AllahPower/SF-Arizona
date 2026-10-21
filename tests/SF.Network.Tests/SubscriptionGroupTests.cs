using SFSharp.Runtime.Networking;

namespace SF.Network.Tests;

public sealed class SubscriptionGroupTests
{
    [Fact]
    public void PreCancelledTokenReleasesLateItems()
    {
        using CancellationTokenSource stop = new();
        stop.Cancel();
        var group = new SubscriptionGroup();
        group.Link(stop.Token);
        int releases = 0;

        group.Add(new Resource(() => releases++));
        group.Dispose();

        Assert.Equal(1, releases);
    }

    [Fact]
    public void ConcurrentCancellationAddAndDisposeReleaseEveryItemOnce()
    {
        using CancellationTokenSource stop = new();
        var group = new SubscriptionGroup();
        group.Link(stop.Token);
        int releases = 0;

        Parallel.For(0, 2000, i =>
        {
            group.Add(new Resource(() => Interlocked.Increment(ref releases)));
            if (i == 500) stop.Cancel();
            if (i == 1000) group.Dispose();
        });
        group.Dispose();

        Assert.Equal(2000, releases);
    }

    private sealed class Resource(Action release) : IDisposable { public void Dispose() => release(); }
}
