using SFSharp.Abstractions.Interop.RakNet;
using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Networking;

public sealed unsafe class SFPackets : ISFPackets
{
    public IDisposable RegisterIncomingFilter(int packetId, SFPacketFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.IncomingPacketFilters.Add(packetId, (dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(packetId, span, bitLength);
        });
    }

    public IDisposable RegisterIncomingFilter(SFPacketFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.IncomingPacketFilters.Add((id, dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(id, span, bitLength);
        });
    }

    public IDisposable RegisterOutgoingFilter(int packetId, SFPacketFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.OutgoingPacketFilters.Add(packetId, (dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(packetId, span, bitLength);
        });
    }

    public IDisposable RegisterOutgoingFilter(SFPacketFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.OutgoingPacketFilters.Add((id, dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(id, span, bitLength);
        });
    }

    public IncomingPacketManager IncomingHandlers => SFBootstrap.IncomingPacketHandlers;
    public OutgoingPacketManager OutgoingHandlers => SFBootstrap.OutgoingPacketHandlers;

    // - Incoming packets (server -> client) -

    public NetworkSubscription SubscribeIncoming(EPacketId packetId, Action<IncomingPacketArgs> handler)
    {
        return IncomingHandlers.Subscribe(packetId, handler);
    }

    public IDisposable SubscribeIncoming(int packetId, Action<IncomingPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeIncoming((EPacketId)packetId, args => handler(new IncomingPacketFrame(args.EPacketId, IncomingPacketPayload.From(args).Data, args.DataBitLength)));
    }

    public async IAsyncEnumerable<IncomingPacketPayload> StreamIncoming(EPacketId packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<IncomingPacketPayload>();
        using NetworkSubscription subscription = SubscribeIncoming(packetId, args => channel.Writer.TryWrite(IncomingPacketPayload.From(args)));

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

    public IDisposable SubscribeOutgoing(int packetId, Action<OutgoingPacketFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoing((EPacketId)packetId, args => handler(new OutgoingPacketFrame(args.EPacketId, OutgoingPacketPayload.From(args).Data, args.DataBitLength)));
    }

    public async IAsyncEnumerable<OutgoingPacketPayload> StreamOutgoing(EPacketId packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<OutgoingPacketPayload>();
        using NetworkSubscription subscription = SubscribeOutgoing(packetId, args => channel.Writer.TryWrite(OutgoingPacketPayload.From(args)));

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


    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(EPacketId packetId, Func<OutgoingPacketArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoing(packetId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<IncomingPacketFrame> StreamIncoming(int packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingPacketPayload payload in StreamIncoming((EPacketId)packetId, token))
        {
            yield return new IncomingPacketFrame((int)payload.EPacketId, payload.Data, payload.DataBitLength);
        }
    }

    public async IAsyncEnumerable<OutgoingPacketFrame> StreamOutgoing(int packetId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingPacketPayload payload in StreamOutgoing((EPacketId)packetId, token))
        {
            yield return new OutgoingPacketFrame((int)payload.EPacketId, payload.Data, payload.DataBitLength);
        }
    }

    // - Packet filters (synchronous, run on hook thread) -

    public IDisposable RegisterOutgoingFilter(EPacketId packetId, Func<nint, int, bool> filter)
    {
        return SFBootstrap.OutgoingPacketFilters.Add((int)packetId, filter);
    }

    public IDisposable RegisterOutgoingFilter(Func<int, nint, int, bool> filter)
    {
        return SFBootstrap.OutgoingPacketFilters.Add(filter);
    }

    /// <remarks>
    /// Arizona Packet 220 traffic cannot be cancelled via incoming filters because vorbisFile.dll
    /// replaces the entire RakClientInterface vtable - those packets are intercepted and processed
    /// before reaching SAMP's RakClient Receive, so our hook never sees them.
    /// </remarks>
    public IDisposable RegisterIncomingFilter(EPacketId packetId, Func<nint, int, bool> filter)
    {
        return SFBootstrap.IncomingPacketFilters.Add((int)packetId, filter);
    }

    public IDisposable RegisterIncomingFilter(Func<int, nint, int, bool> filter)
    {
        return SFBootstrap.IncomingPacketFilters.Add(filter);
    }
}
