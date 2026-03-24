using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFArizonaPackets
{
    private const int Packet220PayloadBitOffset = 16;
    private const int Packet221PayloadBitOffset = 24;
    private const int AZVoiceControlPayloadBitOffset = 16;

    public RpcSubscription SubscribeIncoming(EArizonaPacketId subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(EPacketId.ArizonaCef, args =>
        {
            if (!TryCreateIncoming220(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeIncomingEx(EArizonaPacketIdEx subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(EPacketId.ArizonaCefEx, args =>
        {
            if (!TryCreateIncoming221(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeOutgoing(EArizonaPacketId subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(EPacketId.ArizonaCef, args =>
        {
            if (!TryCreateOutgoing220(args, out OutgoingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeOutgoingEx(EArizonaPacketIdEx subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(EPacketId.ArizonaCefEx, args =>
        {
            if (!TryCreateOutgoing221(args, out OutgoingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeIncomingAZVoice(EAZVoiceSubRpcId subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(EPacketId.AZVoice, args =>
        {
            if (!TryCreateIncomingAZVoice(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public RpcSubscription SubscribeIncomingAZVoiceData(Action<IncomingPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(EPacketId.AZVoice, args =>
        {
            if (!TryCreateIncomingAZVoice(args, out _))
            {
                handler(args);
            }
        });
    }

    public RpcSubscription SubscribeOutgoingAZVoiceData(Action<OutgoingPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(EPacketId.AZVoice, handler);
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncoming(EArizonaPacketId subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingEx(EArizonaPacketIdEx subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoing(EArizonaPacketId subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoingEx(EArizonaPacketIdEx subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingAZVoice(EAZVoiceSubRpcId subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingArizonaPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeIncomingAZVoice(subId, args => queue.Enqueue(IncomingArizonaPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingArizonaPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<IncomingPacketPayload> StreamIncomingAZVoiceData([EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeIncomingAZVoiceData(args => queue.Enqueue(IncomingPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<OutgoingPacketPayload> StreamOutgoingAZVoiceData([EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<OutgoingPacketPayload> queue = new();
        using RpcSubscription subscription = SubscribeOutgoingAZVoiceData(args => queue.Enqueue(OutgoingPacketPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out OutgoingPacketPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncoming<TPayload>(EArizonaPacketId subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncoming(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingEx<TPayload>(EArizonaPacketIdEx subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(EArizonaPacketId subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoing(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoingEx<TPayload>(EArizonaPacketIdEx subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingAZVoice<TPayload>(EAZVoiceSubRpcId subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingAZVoice(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingAZVoiceData<TPayload>(Func<IncomingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingPacketPayload payload in StreamIncomingAZVoiceData(token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoingAZVoiceData<TPayload>(Func<OutgoingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoingAZVoiceData(token))
        {
            yield return payload.Parse(parser);
        }
    }

    private static bool TryCreateIncoming220(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.EPacketId != (int)EPacketId.ArizonaCef || args.DataBitLength < Packet220PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, Packet220PayloadBitOffset, args.DataBitLength - Packet220PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateIncoming221(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.EPacketId != (int)EPacketId.ArizonaCefEx || args.DataBitLength < Packet221PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            ushort subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, Packet221PayloadBitOffset, args.DataBitLength - Packet221PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoing220(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.EPacketId != (int)EPacketId.ArizonaCef || args.DataBitLength < Packet220PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, Packet220PayloadBitOffset, args.DataBitLength - Packet220PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoing221(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.EPacketId != (int)EPacketId.ArizonaCefEx || args.DataBitLength < Packet221PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            ushort subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, Packet221PayloadBitOffset, args.DataBitLength - Packet221PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateIncomingAZVoice(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.EPacketId != (int)EPacketId.AZVoice || args.DataBitLength < AZVoiceControlPayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = reader.ReadUInt8();
            if (!Enum.IsDefined(typeof(EAZVoiceSubRpcId), subId))
            {
                return false;
            }

            packetArgs = new(args.EPacketId, subId, args.DataPtr, AZVoiceControlPayloadBitOffset, args.DataBitLength - AZVoiceControlPayloadBitOffset);
            return true;
        }
    }
}
