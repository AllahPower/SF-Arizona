using System.Runtime.CompilerServices;
using SFSharp.Protocol.Transport;
using SFSharp.Runtime.Networking.RakNet;
using SFSharp.Runtime.Networking.RakNet.Incoming;

namespace SF.Network.Tests;

[CollectionDefinition("Packet memory", DisableParallelization = true)]
public sealed class PacketMemoryCollection;

[Collection("Packet memory")]
public sealed class PacketMemoryTests
{
    private readonly ITestOutputHelper _output;

    public PacketMemoryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TimestampDecodingAndFrameViewsAllocateNoManagedMemory()
    {
        IncomingPacketPayload payload = new(RakNetPacketId.Timestamp, [40, 1, 2, 3, 4, 207, 0xAA, 0xBB], 61);
        ReadViews(payload, 10_000);
        long before = GC.GetAllocatedBytesForCurrentThread();
        long checksum = ReadViews(payload, 1_000_000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        _output.WriteLine($"1,000,000 frame/view reads: {allocated} allocated bytes.");
        Assert.True(checksum > 0);
        Assert.Equal(0, allocated);
        GC.KeepAlive(payload);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long ReadViews(IncomingPacketPayload payload, int count)
    {
        long checksum = 0;
        for (int i = 0; i < count; i++)
        {
            IncomingPacketFrame frame = payload.ToFrame();
            if (frame.TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope))
            {
                checksum += envelope.Timestamp + envelope.PacketId;
            }
            checksum += frame.RakNetTimestamp.GetValueOrDefault() + frame.EffectivePacketId
                + frame.PacketDataBitOffset + frame.PacketDataBitLength + frame.PacketData.Span[0];
        }
        return checksum;
    }

    [Fact]
    public unsafe void CopyAndTimestampMetadataAllocateOnlyTheExistingByteArray()
    {
        byte[] source = [40, 1, 2, 3, 4, 207, 0xAA, 0xBB];
        fixed (byte* data = source)
        {
            IncomingPacketArgs args = new(40, (nint)data, 61);
            MeasureCopies(args, 1_000, true);
            MeasureCopies(args, 1_000, false);
            long baseline = MeasureCopies(args, 10_000, false);
            long actual = MeasureCopies(args, 10_000, true);
            _output.WriteLine($"10,000 copies: {actual} bytes; byte-array baseline: {baseline} bytes.");
            Assert.True(baseline > 0);
            Assert.Equal(baseline, actual);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureCopies(IncomingPacketArgs args, int count, bool copyPayload)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < count; i++)
        {
            if (copyPayload)
            {
                IncomingPacketPayload payload = IncomingPacketPayload.From(args);
                IncomingPacketFrame frame = payload.ToFrame();
                _ = frame.TryGetTimestampEnvelope(out _);
                GC.KeepAlive(payload.Data);
            }
            else
            {
                GC.KeepAlive(new byte[args.DataByteLength]);
            }
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void CompletedDispatchDoesNotRetainSourceBuffers()
    {
        using IncomingPacketManager manager = new();
        using NetworkSubscription subscription = manager.Subscribe(40, ReadPacket);
        WeakReference[] buffers = DispatchBatch(manager, 10_000);
        Collect();
        Assert.All(buffers, buffer => Assert.False(buffer.IsAlive));
        Assert.True(manager.HasSubscribers(40));
        GC.KeepAlive(manager);
        GC.KeepAlive(subscription);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] DispatchBatch(IncomingPacketManager manager, int count)
    {
        WeakReference[] buffers = new WeakReference[64];
        for (int i = 0; i < count; i++)
        {
            byte[] source = [40, 1, 2, 3, 4, 207, 0xAA, 0xBB];
            manager.Dispatch(40, source, 61);
            if (i < buffers.Length)
            {
                buffers[i] = new WeakReference(source);
            }
        }
        return buffers;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ReadPacket(IncomingPacketArgs args)
    {
        IncomingPacketPayload payload = IncomingPacketPayload.From(args);
        Assert.Equal(0x04030201u, payload.ToFrame().RakNetTimestamp);
    }

    [Fact]
    public void DispatchCopiesBecomeCollectibleWhileSubscriberIsStillRegistered()
    {
        using IncomingPacketManager manager = new();
        CopyObserver observer = new();
        using NetworkSubscription subscription = manager.Subscribe(40, observer.Receive);
        DispatchBatch(manager, 10_000);
        Collect();
        Assert.Equal(64, observer.Buffers.Count);
        Assert.All(observer.Buffers, buffer => Assert.False(buffer.IsAlive));
        GC.KeepAlive(observer);
        GC.KeepAlive(manager);
        GC.KeepAlive(subscription);
    }

    private sealed class CopyObserver
    {
        public List<WeakReference> Buffers { get; } = new();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Receive(IncomingPacketArgs args)
        {
            IncomingPacketPayload payload = IncomingPacketPayload.From(args);
            _ = payload.ToFrame().RakNetTimestamp;
            if (Buffers.Count < 64)
            {
                Buffers.Add(new WeakReference(payload.Data));
            }
        }
    }

    [Fact]
    public void DisposedSubscriptionDoesNotRetainCallbackTarget()
    {
        using IncomingPacketManager manager = new();
        (NetworkSubscription subscription, WeakReference target) = CreateDisposedSubscription(manager);
        Collect();
        Assert.False(manager.HasAnySubscribers());
        Assert.False(target.IsAlive);
        subscription.Dispose();
        GC.KeepAlive(subscription);
        GC.KeepAlive(manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (NetworkSubscription, WeakReference) CreateDisposedSubscription(IncomingPacketManager manager)
    {
        CopyObserver target = new();
        NetworkSubscription subscription = manager.Subscribe(40, target.Receive);
        subscription.Dispose();
        return (subscription, new WeakReference(target));
    }

    [Fact]
    public void SubscriptionDisposesCallbackExactlyOnceUnderContention()
    {
        int calls = 0;
        NetworkSubscription subscription = new(() => Interlocked.Increment(ref calls));
        Parallel.For(0, 1_000, _ => subscription.Dispose());
        Assert.Equal(1, calls);
    }

    [Fact]
    public void DisposePropagatesCallbackFailureWithoutInvokingItAgain()
    {
        int calls = 0;
        NetworkSubscription subscription = new(() =>
        {
            calls++;
            throw new InvalidOperationException("Unsubscribe failed.");
        });
        Assert.Throws<InvalidOperationException>(() => subscription.Dispose());
        subscription.Dispose();
        Assert.Equal(1, calls);
    }

    [Fact]
    public void DispatchWithTimestampCopyAllocatesOnlySnapshotAndPayloadArrays()
    {
        using IncomingPacketManager manager = new();
        using NetworkSubscription subscription = manager.Subscribe(40, ReadMetadata);
        byte[] source = [40, 1, 2, 3, 4, 207, 0xAA, 0xBB];
        MeasureDispatch(manager, source, 1_000, true);
        MeasureDispatch(manager, source, 1_000, false);
        long expected = MeasureDispatch(manager, source, 10_000, false);
        long actual = MeasureDispatch(manager, source, 10_000, true);
        _output.WriteLine($"10,000 dispatches: {actual} bytes; snapshot + payload baseline: {expected} bytes.");
        Assert.Equal(expected, actual);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ReadMetadata(IncomingPacketArgs args)
    {
        IncomingPacketPayload payload = IncomingPacketPayload.From(args);
        IncomingPacketFrame frame = payload.ToFrame();
        _ = frame.TryGetTimestampEnvelope(out _);
        GC.KeepAlive(payload.Data);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureDispatch(IncomingPacketManager manager, byte[] source, int count, bool dispatch)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < count; i++)
        {
            if (dispatch)
            {
                manager.Dispatch(40, source, 61);
            }
            else
            {
                GC.KeepAlive(new Action<IncomingPacketArgs>[1]);
                GC.KeepAlive(new byte[source.Length]);
            }
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void UnobservedDispatchAllocatesNoManagedMemory()
    {
        using IncomingPacketManager manager = new();
        byte[] source = [40, 1, 2, 3, 4, 207];
        MeasureDispatch(manager, source, 1_000, true);
        Assert.Equal(0, MeasureDispatch(manager, source, 10_000, true));
    }

    private static void Collect()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }
}
