using SFSharp.Interop.RakNet.Packets.Enum;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFPackets
{
    public IncomingPacketManager IncomingHandlers => SFBootstrap.IncomingPacketHandlers;
    public OutgoingPacketManager OutgoingHandlers => SFBootstrap.OutgoingPacketHandlers;

    // - Incoming packets (server -> client) -

    public NetworkSubscription SubscribeIncoming(EPacketId packetId, Action<IncomingPacketArgs> handler)
    {
        return IncomingHandlers.Subscribe(packetId, handler);
    }

    public async IAsyncEnumerable<IncomingPacketPayload> StreamIncoming(EPacketId packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingPacketPayload> queue = new();
        using NetworkSubscription subscription = SubscribeIncoming(packetId, args => queue.Enqueue(IncomingPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncoming<TPayload>(EPacketId packetId, Func<IncomingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingPacketPayload payload in StreamIncoming(packetId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    // - Outgoing packets (client -> server) -

    public NetworkSubscription SubscribeOutgoing(EPacketId packetId, Action<OutgoingPacketArgs> handler)
    {
        return OutgoingHandlers.Subscribe(packetId, handler);
    }

    public async IAsyncEnumerable<OutgoingPacketPayload> StreamOutgoing(EPacketId packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<OutgoingPacketPayload> queue = new();
        using NetworkSubscription subscription = SubscribeOutgoing(packetId, args => queue.Enqueue(OutgoingPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out OutgoingPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(EPacketId packetId, Func<OutgoingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoing(packetId, token))
        {
            yield return payload.Parse(parser);
        }
    }
}
