using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFArizonaPackets
{
    private const int Packet220PayloadBitOffset = 16;
    private const int Packet221PayloadBitOffset = 24;

    public RpcSubscription SubscribeIncoming(ArizonaPacketId subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(PacketId.ArizonaCef, args =>
        {
            if (!TryCreateIncoming220(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeIncomingEx(ArizonaPacketIdEx subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(PacketId.ArizonaCefEx, args =>
        {
            if (!TryCreateIncoming221(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeOutgoing(ArizonaPacketId subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(PacketId.ArizonaCef, args =>
        {
            if (!TryCreateOutgoing220(args, out OutgoingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeOutgoingEx(ArizonaPacketIdEx subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(PacketId.ArizonaCefEx, args =>
        {
            if (!TryCreateOutgoing221(args, out OutgoingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncoming(ArizonaPacketId subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingArizonaPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeIncoming(subId, args => queue.Enqueue(IncomingArizonaPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingArizonaPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingEx(ArizonaPacketIdEx subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingArizonaPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeIncomingEx(subId, args => queue.Enqueue(IncomingArizonaPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingArizonaPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoing(ArizonaPacketId subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<OutgoingArizonaPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeOutgoing(subId, args => queue.Enqueue(OutgoingArizonaPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out OutgoingArizonaPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoingEx(ArizonaPacketIdEx subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<OutgoingArizonaPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeOutgoingEx(subId, args => queue.Enqueue(OutgoingArizonaPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out OutgoingArizonaPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncoming<TPayload>(ArizonaPacketId subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncoming(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingEx<TPayload>(ArizonaPacketIdEx subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(ArizonaPacketId subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoing(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoingEx<TPayload>(ArizonaPacketIdEx subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    private static bool TryCreateIncoming220(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.PacketId != (int)PacketId.ArizonaCef || args.DataBitLength < Packet220PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.PacketId, subId, args.DataPtr, Packet220PayloadBitOffset, args.DataBitLength - Packet220PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateIncoming221(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.PacketId != (int)PacketId.ArizonaCefEx || args.DataBitLength < Packet221PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            ushort subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.PacketId, subId, args.DataPtr, Packet221PayloadBitOffset, args.DataBitLength - Packet221PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoing220(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.PacketId != (int)PacketId.ArizonaCef || args.DataBitLength < Packet220PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.PacketId, subId, args.DataPtr, Packet220PayloadBitOffset, args.DataBitLength - Packet220PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoing221(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.PacketId != (int)PacketId.ArizonaCefEx || args.DataBitLength < Packet221PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            ushort subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.PacketId, subId, args.DataPtr, Packet221PayloadBitOffset, args.DataBitLength - Packet221PayloadBitOffset);
            return true;
        }
    }
}
