namespace SFSharp.Protocol.Packets.Parsing;

public sealed class PacketParserRegistry
{
    private readonly Dictionary<int, IIncomingPacketParser> _incoming = new();
    private readonly Dictionary<int, IOutgoingPacketParser> _outgoing = new();
    private readonly Dictionary<Type, List<IncomingRoute>> _incomingRoutes = new();
    private readonly Dictionary<Type, List<OutgoingRoute>> _outgoingRoutes = new();

    public void Register(IIncomingPacketParser parser)
    {
        _incoming[(int)parser.RakNetPacketId] = parser;
        if (!_incomingRoutes.TryGetValue(parser.ParsedType, out var routes)) _incomingRoutes.Add(parser.ParsedType, routes = []);
        routes.RemoveAll(route => route.RakNetPacketId == parser.RakNetPacketId);
        routes.Add(new(parser.RakNetPacketId, parser));
    }

    public void Register(IOutgoingPacketParser parser)
    {
        _outgoing[(int)parser.RakNetPacketId] = parser;
        if (!_outgoingRoutes.TryGetValue(parser.ParsedType, out var routes)) _outgoingRoutes.Add(parser.ParsedType, routes = []);
        routes.RemoveAll(route => route.RakNetPacketId == parser.RakNetPacketId);
        routes.Add(new(parser.RakNetPacketId, parser));
    }

    public bool TryParseIncoming(IncomingPacketArgs args, out PacketParseResult result)
    {
        if (_incoming.TryGetValue(args.RakNetPacketId, out var parser)) return parser.TryParse(args, out result);
        result = PacketParseResult.Unsupported((RakNetPacketId)args.RakNetPacketId);
        return false;
    }

    public bool TryParseOutgoing(OutgoingPacketArgs args, out PacketParseResult result)
    {
        if (_outgoing.TryGetValue(args.RakNetPacketId, out var parser)) return parser.TryParse(args, out result);
        result = PacketParseResult.Unsupported((RakNetPacketId)args.RakNetPacketId);
        return false;
    }

    public IReadOnlyList<IncomingRoute> GetIncomingRoutes<TPacket>() => _incomingRoutes.TryGetValue(typeof(TPacket), out var routes) ? routes : [];

    public IReadOnlyList<OutgoingRoute> GetOutgoingRoutes<TPacket>() => _outgoingRoutes.TryGetValue(typeof(TPacket), out var routes) ? routes : [];

    public sealed record IncomingRoute(RakNetPacketId RakNetPacketId, IIncomingPacketParser Parser);
    public sealed record OutgoingRoute(RakNetPacketId RakNetPacketId, IOutgoingPacketParser Parser);
}
