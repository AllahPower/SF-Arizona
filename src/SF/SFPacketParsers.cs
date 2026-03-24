using SFSharp.Interop.RakNet.Arizona.Enum;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFPacketParsers
{
    private static readonly Lazy<PacketParserRegistry> _registry = new(PacketParserCatalog.CreateDefaultRegistry);

    public PacketParserRegistry Registry => _registry.Value;

    public bool TryParseIncoming(IncomingPacketArgs args, out PacketParseResult result)
    {
        return Registry.TryParseIncoming(args, out result);
    }

    public bool TryParseOutgoing(OutgoingPacketArgs args, out PacketParseResult result)
    {
        return Registry.TryParseOutgoing(args, out result);
    }

    public IDisposable BindIncoming<TPacket>(Action<TPacket> handler, CancellationToken token = default)
        where TPacket : class, IParsedIncomingPacket
    {
        IReadOnlyList<PacketParserRegistry.IncomingRoute> routes = Registry.GetIncomingRoutes<TPacket>();
        if (routes.Count == 0)
        {
            throw new InvalidOperationException($"Incoming packet parser for {typeof(TPacket).FullName} is not registered.");
        }

        SubscriptionGroup group = new();
        foreach (PacketParserRegistry.IncomingRoute route in routes)
        {
            if (route.SubId is null)
            {
                IIncomingPacketParser parser = (IIncomingPacketParser)route.Parser;
                group.Add(SF.Packets.SubscribeIncoming(route.EPacketId, args =>
                {
                    if (parser.TryParse(args, out PacketParseResult result) && result.Packet is TPacket packet)
                    {
                        handler(packet);
                    }
                }));
                continue;
            }

            IIncomingArizonaPacketParser parserArizona = (IIncomingArizonaPacketParser)route.Parser;
            if (route.IsEx)
            {
                group.Add(SF.Arizona.SubscribeIncomingEx((EArizonaPacketIdEx)route.SubId.Value, args =>
                {
                    if (parserArizona.TryParse(args, out PacketParseResult result) && result.Packet is TPacket packet)
                    {
                        handler(packet);
                    }
                }));
            }
            else
            {
                group.Add(SF.Arizona.SubscribeIncoming((EArizonaPacketId)route.SubId.Value, args =>
                {
                    if (parserArizona.TryParse(args, out PacketParseResult result) && result.Packet is TPacket packet)
                    {
                        handler(packet);
                    }
                }));
            }
        }

        group.Link(token);
        return group;
    }

    public IDisposable BindOutgoing<TPacket>(Action<TPacket> handler, CancellationToken token = default)
        where TPacket : class, IParsedOutgoingPacket
    {
        IReadOnlyList<PacketParserRegistry.OutgoingRoute> routes = Registry.GetOutgoingRoutes<TPacket>();
        if (routes.Count == 0)
        {
            throw new InvalidOperationException($"Outgoing packet parser for {typeof(TPacket).FullName} is not registered.");
        }

        SubscriptionGroup group = new();
        foreach (PacketParserRegistry.OutgoingRoute route in routes)
        {
            if (route.SubId is null)
            {
                IOutgoingPacketParser parser = (IOutgoingPacketParser)route.Parser;
                group.Add(SF.Packets.SubscribeOutgoing(route.EPacketId, args =>
                {
                    if (parser.TryParse(args, out PacketParseResult result) && result.Packet is TPacket packet)
                    {
                        handler(packet);
                    }
                }));
                continue;
            }

            IOutgoingArizonaPacketParser parserArizona = (IOutgoingArizonaPacketParser)route.Parser;
            if (route.IsEx)
            {
                group.Add(SF.Arizona.SubscribeOutgoingEx((EArizonaPacketIdEx)route.SubId.Value, args =>
                {
                    if (parserArizona.TryParse(args, out PacketParseResult result) && result.Packet is TPacket packet)
                    {
                        handler(packet);
                    }
                }));
            }
            else
            {
                group.Add(SF.Arizona.SubscribeOutgoing((EArizonaPacketId)route.SubId.Value, args =>
                {
                    if (parserArizona.TryParse(args, out PacketParseResult result) && result.Packet is TPacket packet)
                    {
                        handler(packet);
                    }
                }));
            }
        }

        group.Link(token);
        return group;
    }

    public async IAsyncEnumerable<TPacket> StreamIncoming<TPacket>([EnumeratorCancellation] CancellationToken token = default)
        where TPacket : class, IParsedIncomingPacket
    {
        ConcurrentQueue<TPacket> queue = new();
        using IDisposable binding = BindIncoming<TPacket>(packet => queue.Enqueue(packet), token);

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out TPacket? packet))
            {
                yield return packet;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPacket> StreamOutgoing<TPacket>([EnumeratorCancellation] CancellationToken token = default)
        where TPacket : class, IParsedOutgoingPacket
    {
        ConcurrentQueue<TPacket> queue = new();
        using IDisposable binding = BindOutgoing<TPacket>(packet => queue.Enqueue(packet), token);

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out TPacket? packet))
            {
                yield return packet;
            }

            await Task.Yield();
        }
    }

    private sealed class SubscriptionGroup : IDisposable
    {
        private readonly List<IDisposable> _items = new();
        private CancellationTokenRegistration _tokenRegistration;
        private bool _disposed;

        public void Add(IDisposable subscription)
        {
            _items.Add(subscription);
        }

        public void Link(CancellationToken token)
        {
            if (token.CanBeCanceled)
            {
                _tokenRegistration = token.Register(Dispose);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _tokenRegistration.Dispose();
            foreach (IDisposable item in _items)
            {
                item.Dispose();
            }

            _items.Clear();
        }
    }
}
