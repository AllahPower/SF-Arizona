using SFSharp.Abstractions.Interop.RakNet;
using SFSharp.Abstractions.Interop.RakNet;
using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Network;

public sealed class SFArizonaPackets : ISFArizonaPackets
{
    private const int Packet220PayloadBitOffset = 16;
    private const int Packet221PayloadBitOffset = 24;

    public NetworkSubscription SubscribeIncoming(EArizona subId, Action<IncomingArizonaPacketArgs> handler)
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

    public IDisposable SubscribeIncoming(int subId, Action<IncomingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncoming((EArizona)subId, args =>
        {
            IncomingArizonaPacketPayload payload = IncomingArizonaPacketPayload.From(args);
            handler(new IncomingArizonaPacketFrame(args.EPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeIncomingEx(EArizonaEx subId, Action<IncomingArizonaPacketArgs> handler)
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

    public IDisposable SubscribeIncomingEx(int subId, Action<IncomingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncomingEx((EArizonaEx)subId, args =>
        {
            IncomingArizonaPacketPayload payload = IncomingArizonaPacketPayload.From(args);
            handler(new IncomingArizonaPacketFrame(args.EPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeOutgoing(EArizona subId, Action<OutgoingArizonaPacketArgs> handler)
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

    public IDisposable SubscribeOutgoing(int subId, Action<OutgoingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoing((EArizona)subId, args =>
        {
            OutgoingArizonaPacketPayload payload = OutgoingArizonaPacketPayload.From(args);
            handler(new OutgoingArizonaPacketFrame(args.EPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeOutgoingEx(EArizonaEx subId, Action<OutgoingArizonaPacketArgs> handler)
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

    public IDisposable SubscribeOutgoingEx(int subId, Action<OutgoingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoingEx((EArizonaEx)subId, args =>
        {
            OutgoingArizonaPacketPayload payload = OutgoingArizonaPacketPayload.From(args);
            handler(new OutgoingArizonaPacketFrame(args.EPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeIncomingAZVoice(EAZVoice subId, Action<IncomingArizonaPacketArgs> handler)
    {
        return SFBootstrap.IncomingAZVoiceControlHandlers.Subscribe((int)subId, handler);
    }

    public IDisposable SubscribeIncomingAZVoice(int subId, Action<IncomingArizonaPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncomingAZVoice((EAZVoice)subId, args =>
        {
            IncomingArizonaPacketPayload payload = IncomingArizonaPacketPayload.From(args);
            handler(new IncomingArizonaPacketFrame(args.EPacketId, args.SubId, payload.Data, args.PayloadBitOffset, args.PayloadBitLength));
        });
    }

    public NetworkSubscription SubscribeIncomingAZVoiceData(Action<IncomingPacketArgs> handler)
    {
        return SFBootstrap.IncomingAZVoiceDataHandlers.Subscribe(handler);
    }

    public IDisposable SubscribeIncomingAZVoiceData(Action<IncomingPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncomingAZVoiceData(args => handler(new IncomingPacketFrame(args.EPacketId, IncomingPacketPayload.From(args).Data, args.DataBitLength)));
    }

    public NetworkSubscription SubscribeOutgoingAZVoiceData(Action<OutgoingPacketArgs> handler)
    {
        return SF.Packets.SubscribeOutgoing(EPacketId.AZVoice, handler);
    }

    public IDisposable SubscribeOutgoingAZVoiceData(Action<OutgoingPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoingAZVoiceData(args => handler(new OutgoingPacketFrame(args.EPacketId, OutgoingPacketPayload.From(args).Data, args.DataBitLength)));
    }

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncoming(EArizona subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingEx(EArizonaEx subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoing(EArizona subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<OutgoingArizonaPacketPayload> StreamOutgoingEx(EArizonaEx subId, [EnumeratorCancellation] CancellationToken token = default)
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

    public async IAsyncEnumerable<IncomingArizonaPacketPayload> StreamIncomingAZVoice(EAZVoice subId, [EnumeratorCancellation] CancellationToken token = default)
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


    public async IAsyncEnumerable<TPayload> StreamIncoming<TPayload>(EArizona subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncoming(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingEx<TPayload>(EArizonaEx subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(EArizona subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoing(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoingEx<TPayload>(EArizonaEx subId, Func<OutgoingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoingEx(subId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<TPayload> StreamIncomingAZVoice<TPayload>(EAZVoice subId, Func<IncomingArizonaPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
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
        await foreach (IncomingArizonaPacketPayload payload in StreamIncoming((EArizona)subId, token))
        {
            yield return new IncomingArizonaPacketFrame((int)payload.EPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncomingEx(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingEx((EArizonaEx)subId, token))
        {
            yield return new IncomingArizonaPacketFrame((int)payload.EPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketFrame> StreamOutgoing(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoing((EArizona)subId, token))
        {
            yield return new OutgoingArizonaPacketFrame((int)payload.EPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<OutgoingArizonaPacketFrame> StreamOutgoingEx(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingArizonaPacketPayload payload in StreamOutgoingEx((EArizonaEx)subId, token))
        {
            yield return new OutgoingArizonaPacketFrame((int)payload.EPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    public async IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncomingAZVoice(int subId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingArizonaPacketPayload payload in StreamIncomingAZVoice((EAZVoice)subId, token))
        {
            yield return new IncomingArizonaPacketFrame((int)payload.EPacketId, payload.SubId, payload.Data, payload.PayloadBitOffset, payload.PayloadBitLength);
        }
    }

    async IAsyncEnumerable<IncomingPacketFrame> ISFArizonaPackets.StreamIncomingAZVoiceData([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (IncomingPacketPayload payload in StreamIncomingAZVoiceData(token))
        {
            yield return new IncomingPacketFrame((int)payload.EPacketId, payload.Data, payload.DataBitLength);
        }
    }

    async IAsyncEnumerable<OutgoingPacketFrame> ISFArizonaPackets.StreamOutgoingAZVoiceData([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoingAZVoiceData(token))
        {
            yield return new OutgoingPacketFrame((int)payload.EPacketId, payload.Data, payload.DataBitLength);
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
}
