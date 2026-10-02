using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Networking;

public sealed class SFArizonaPackets : ISFArizonaPackets
{
    private const int Packet220PayloadBitOffset = 16;
    private const int Packet221PayloadBitOffset = 24;

    public NetworkSubscription SubscribeIncoming(ArizonaPacket220Id subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(RakNetPacketId.ArizonaCef, args =>
        {
            if (!TryCreateIncoming220(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public IDisposable SubscribeIncoming(int subId, Action<IncomingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncoming((ArizonaPacket220Id)subId, args =>
        {
            IncomingArizonaPacketPayload payload = IncomingArizonaPacketPayload.From(args);
            handler(new IncomingArizonaPacketFrame(args.RakNetPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeIncomingEx(ArizonaPacket221Id subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeIncoming(RakNetPacketId.ArizonaCefEx, args =>
        {
            if (!TryCreateIncoming221(args, out IncomingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public IDisposable SubscribeIncomingEx(int subId, Action<IncomingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncomingEx((ArizonaPacket221Id)subId, args =>
        {
            IncomingArizonaPacketPayload payload = IncomingArizonaPacketPayload.From(args);
            handler(new IncomingArizonaPacketFrame(args.RakNetPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeOutgoing(ArizonaPacket220Id subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(RakNetPacketId.ArizonaCef, args =>
        {
            if (!TryCreateOutgoing220(args, out OutgoingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public IDisposable SubscribeOutgoing(int subId, Action<OutgoingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoing((ArizonaPacket220Id)subId, args =>
        {
            OutgoingArizonaPacketPayload payload = OutgoingArizonaPacketPayload.From(args);
            handler(new OutgoingArizonaPacketFrame(args.RakNetPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeOutgoingEx(ArizonaPacket221Id subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(RakNetPacketId.ArizonaCefEx, args =>
        {
            if (!TryCreateOutgoing221(args, out OutgoingArizonaPacketArgs packetArgs) || packetArgs.SubId != (int)subId)
            {
                return;
            }

            handler(packetArgs);
        });
    }

    public IDisposable SubscribeOutgoingEx(int subId, Action<OutgoingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoingEx((ArizonaPacket221Id)subId, args =>
        {
            OutgoingArizonaPacketPayload payload = OutgoingArizonaPacketPayload.From(args);
            handler(new OutgoingArizonaPacketFrame(args.RakNetPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeIncomingAZVoice(AZVoiceMessageId subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SFBootstrap.IncomingAZVoiceControlHandlers.Subscribe((int)subId, handler);
    }

    public IDisposable SubscribeIncomingAZVoice(int subId, Action<IncomingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncomingAZVoice((AZVoiceMessageId)subId, args =>
        {
            IncomingArizonaPacketPayload payload = IncomingArizonaPacketPayload.From(args);
            handler(new IncomingArizonaPacketFrame(args.RakNetPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeOutgoingAZVoice(AZVoiceMessageId subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        return SFBootstrap.OutgoingAZVoiceControlHandlers.Subscribe((int)subId, handler);
    }

    public IDisposable SubscribeOutgoingAZVoice(int subId, Action<OutgoingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoingAZVoice((AZVoiceMessageId)subId, args =>
        {
            OutgoingArizonaPacketPayload payload = OutgoingArizonaPacketPayload.From(args);
            handler(new OutgoingArizonaPacketFrame(args.RakNetPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeIncomingAZVoiceData(Action<IncomingPacketArgs> handler)
    {
        return SFBootstrap.IncomingAZVoiceDataHandlers.Subscribe(handler);
    }

    public IDisposable SubscribeIncomingAZVoiceData(Action<IncomingPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncomingAZVoiceData(args => handler(IncomingPacketPayload.From(args).ToFrame()));
    }

    public NetworkSubscription SubscribeOutgoingAZVoiceData(Action<OutgoingPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(RakNetPacketId.AZVoice, handler);
    }

    public IDisposable SubscribeOutgoingAZVoiceData(Action<OutgoingPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoingAZVoiceData(args => handler(new OutgoingPacketFrame(args.RakNetPacketId, OutgoingPacketPayload.From(args).Data, args.DataBitLength)));
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncoming(ArizonaPacket220Id subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<IncomingArizonaPacketPayload>();
        using NetworkSubscription subscription = SubscribeIncoming(subId, args => channel.Writer.TryWrite(IncomingArizonaPacketPayload.From(args)));

        try
        {
            await foreach (IncomingArizonaPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingEx(ArizonaPacket221Id subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<IncomingArizonaPacketPayload>();
        using NetworkSubscription subscription = SubscribeIncomingEx(subId, args => channel.Writer.TryWrite(IncomingArizonaPacketPayload.From(args)));

        try
        {
            await foreach (IncomingArizonaPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoing(ArizonaPacket220Id subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<OutgoingArizonaPacketPayload>();
        using NetworkSubscription subscription = SubscribeOutgoing(subId, args => channel.Writer.TryWrite(OutgoingArizonaPacketPayload.From(args)));

        try
        {
            await foreach (OutgoingArizonaPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoingEx(ArizonaPacket221Id subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<OutgoingArizonaPacketPayload>();
        using NetworkSubscription subscription = SubscribeOutgoingEx(subId, args => channel.Writer.TryWrite(OutgoingArizonaPacketPayload.From(args)));

        try
        {
            await foreach (OutgoingArizonaPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingAZVoice(AZVoiceMessageId subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<IncomingArizonaPacketPayload>();
        using NetworkSubscription subscription = SubscribeIncomingAZVoice(subId, args => channel.Writer.TryWrite(IncomingArizonaPacketPayload.From(args)));

        try
        {
            await foreach (IncomingArizonaPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<IncomingPacketPayload> StreamIncomingAZVoiceData([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<IncomingPacketPayload>();
        using NetworkSubscription subscription = SubscribeIncomingAZVoiceData(args => channel.Writer.TryWrite(IncomingPacketPayload.From(args)));

        try
        {
            await foreach (IncomingPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<OutgoingPacketPayload> StreamOutgoingAZVoiceData([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<OutgoingPacketPayload>();
        using NetworkSubscription subscription = SubscribeOutgoingAZVoiceData(args => channel.Writer.TryWrite(OutgoingPacketPayload.From(args)));

        try
        {
            await foreach (OutgoingPacketPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }


    public async IAsyncEnumerable<TPayload> StreamIncoming<TPayload>(ArizonaPacket220Id subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncoming(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingEx<TPayload>(ArizonaPacket221Id subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(ArizonaPacket220Id subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoing(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoingEx<TPayload>(ArizonaPacket221Id subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingAZVoice<TPayload>(AZVoiceMessageId subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncoming(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncoming((ArizonaPacket220Id)subId, token))
        {
            yield return new IncomingArizonaPacketFrame((int)payload.RakNetPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncomingEx(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingEx((ArizonaPacket221Id)subId, token))
        {
            yield return new IncomingArizonaPacketFrame((int)payload.RakNetPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketFrame> StreamOutgoing(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoing((ArizonaPacket220Id)subId, token))
        {
            yield return new OutgoingArizonaPacketFrame((int)payload.RakNetPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketFrame> StreamOutgoingEx(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoingEx((ArizonaPacket221Id)subId, token))
        {
            yield return new OutgoingArizonaPacketFrame((int)payload.RakNetPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncomingAZVoice(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingAZVoice((AZVoiceMessageId)subId, token))
        {
            yield return new IncomingArizonaPacketFrame((int)payload.RakNetPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    async IAsyncEnumerable<IncomingPacketFrame> ISFArizonaPackets.StreamIncomingAZVoiceData([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (IncomingPacketPayload payload in StreamIncomingAZVoiceData(token))
        {
            yield return payload.ToFrame();
        }
    }

    async IAsyncEnumerable<OutgoingPacketFrame> ISFArizonaPackets.StreamOutgoingAZVoiceData([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoingAZVoiceData(token))
        {
            yield return new OutgoingPacketFrame((int)payload.RakNetPacketId, payload.Data, payload.DataBitLength);
        }
    }

    private static bool TryCreateIncoming220(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.RakNetPacketId != (int)RakNetPacketId.ArizonaCef || args.DataBitLength < Packet220PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, Packet220PayloadBitOffset, args.DataBitLength - Packet220PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateIncoming221(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.RakNetPacketId != (int)RakNetPacketId.ArizonaCefEx || args.DataBitLength < Packet221PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            ushort subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, Packet221PayloadBitOffset, args.DataBitLength - Packet221PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoing220(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.RakNetPacketId != (int)RakNetPacketId.ArizonaCef || args.DataBitLength < Packet220PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, Packet220PayloadBitOffset, args.DataBitLength - Packet220PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoing221(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.RakNetPacketId != (int)RakNetPacketId.ArizonaCefEx || args.DataBitLength < Packet221PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            ushort subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, Packet221PayloadBitOffset, args.DataBitLength - Packet221PayloadBitOffset);
            return true;
        }
    }
}
