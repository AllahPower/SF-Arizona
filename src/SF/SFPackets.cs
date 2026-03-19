using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFPackets
{
    public IncomingPacketManager IncomingHandlers => SFBootstrap.IncomingPacketHandlers;
    public OutgoingPacketManager OutgoingHandlers => SFBootstrap.OutgoingPacketHandlers;

    // - Incoming packets (server -> client) -

    public RpcSubscription SubscribeIncoming(PacketId packetId, Action<IncomingPacketArgs> handler)
    {
        return IncomingHandlers.Subscribe(packetId, handler);
    }

    public async IAsyncEnumerable<IncomingPacketPayload> StreamIncoming(PacketId packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeIncoming(packetId, args => queue.Enqueue(IncomingPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncoming<TPayload>(PacketId packetId, Func<IncomingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingPacketPayload payload in StreamIncoming(packetId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    // - Outgoing packets (client -> server) -

    public RpcSubscription SubscribeOutgoing(PacketId packetId, Action<OutgoingPacketArgs> handler)
    {
        return OutgoingHandlers.Subscribe(packetId, handler);
    }

    public async IAsyncEnumerable<OutgoingPacketPayload> StreamOutgoing(PacketId packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<OutgoingPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeOutgoing(packetId, args => queue.Enqueue(OutgoingPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out OutgoingPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(PacketId packetId, Func<OutgoingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoing(packetId, token))
        {
            yield return payload.Parse(parser);
        }
    }
}
