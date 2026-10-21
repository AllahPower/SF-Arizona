using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Networking;

public sealed partial class SFPacketParsers(PacketParserRegistry registry, SFPackets packets) : ISFPacketParsers
{
    public PacketParserRegistry Registry => registry;

    public bool TryParseIncoming(IncomingPacketArgs args, out PacketParseResult result)
    {
        return Registry.TryParseIncoming(args, out result);
    }

    public bool TryParseOutgoing(OutgoingPacketArgs args, out PacketParseResult result)
    {
        return Registry.TryParseOutgoing(args, out result);
    }

    public IDisposable BindIncoming<TPacket>(Action<TPacket> handler, CancellationToken token = default)
    {
        var routes = Registry.GetIncomingRoutes<TPacket>();
        if (routes.Count == 0) throw new InvalidOperationException($"Incoming parser for {typeof(TPacket).FullName} is not registered.");
        SubscriptionGroup group = new();
        foreach (var route in routes)
            group.Add(packets.SubscribeIncoming(route.RakNetPacketId, args =>
            {
                if (route.Parser.TryParse(args, out var result) && result.Packet is TPacket packet) handler(packet);
            }));
        group.Link(token);
        return group;
    }

    public IDisposable BindOutgoing<TPacket>(Action<TPacket> handler, CancellationToken token = default)
    {
        var routes = Registry.GetOutgoingRoutes<TPacket>();
        if (routes.Count == 0) throw new InvalidOperationException($"Outgoing parser for {typeof(TPacket).FullName} is not registered.");
        SubscriptionGroup group = new();
        foreach (var route in routes)
            group.Add(packets.SubscribeOutgoing(route.RakNetPacketId, args =>
            {
                if (route.Parser.TryParse(args, out var result) && result.Packet is TPacket packet) handler(packet);
            }));
        group.Link(token);
        return group;
    }

    public async IAsyncEnumerable<TPacket> StreamIncoming<TPacket>([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<TPacket>();
        using IDisposable binding = BindIncoming<TPacket>(packet => channel.Writer.TryWrite(packet), token);

        try
        {
            await foreach (TPacket packet in channel.Reader.ReadAllAsync(token))
            {
                yield return packet;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<TPacket> StreamOutgoing<TPacket>([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<TPacket>();
        using IDisposable binding = BindOutgoing<TPacket>(packet => channel.Writer.TryWrite(packet), token);

        try
        {
            await foreach (TPacket packet in channel.Reader.ReadAllAsync(token))
            {
                yield return packet;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }


}
