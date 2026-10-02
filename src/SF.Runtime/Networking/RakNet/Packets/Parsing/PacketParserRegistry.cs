namespace SFSharp.Runtime.Networking.RakNet.Packets.Parsing;

public sealed class PacketParserRegistry
{
    private readonly Dictionary<int, IIncomingPacketParser> _incoming = new();
    private readonly Dictionary<int, IOutgoingPacketParser> _outgoing = new();
    private readonly Dictionary<int, IIncomingArizonaPacketParser> _incomingTransportParsers220 = new();
    private readonly Dictionary<int, IIncomingArizonaPacketParser> _incomingTransportParsers221 = new();
    private readonly Dictionary<int, IIncomingArizonaPacketParser> _incomingTransportParsersAZVoice = new();
    private readonly Dictionary<int, IOutgoingArizonaPacketParser> _outgoingTransportParsers220 = new();
    private readonly Dictionary<int, IOutgoingArizonaPacketParser> _outgoingTransportParsers221 = new();
    private readonly Dictionary<int, IIncomingPacketTransportRouter> _incomingTransportRouters = new();
    private readonly Dictionary<int, IOutgoingPacketTransportRouter> _outgoingTransportRouters = new();
    private readonly Dictionary<Type, List<IncomingRoute>> _incomingRoutesByType = new();
    private readonly Dictionary<Type, List<OutgoingRoute>> _outgoingRoutesByType = new();

    public void Register(IIncomingPacketParser parser)
    {
        _incoming[(int)parser.RakNetPacketId] = parser;
        IncomingRoute route = new(parser.RakNetPacketId, null, false, parser);
        AddIncomingRoute(parser.ParsedType, route);
    }

    public void Register(IOutgoingPacketParser parser)
    {
        _outgoing[(int)parser.RakNetPacketId] = parser;
        OutgoingRoute route = new(parser.RakNetPacketId, null, false, parser);
        AddOutgoingRoute(parser.ParsedType, route);
    }

    public void Register(IIncomingArizonaPacketParser parser)
    {
        GetIncomingTransportMap(parser.RakNetPacketId)[parser.SubId] = parser;
        IncomingRoute route = new(parser.RakNetPacketId, parser.SubId, parser.RakNetPacketId == RakNetPacketId.ArizonaCefEx, parser);
        AddIncomingRoute(parser.ParsedType, route);
        if (TryGetWrappedPayloadType(parser.ParsedType, typeof(IncomingSubPacket<>), out Type payloadType))
        {
            AddIncomingRoute(payloadType, route);
        }
    }

    public void Register(IOutgoingArizonaPacketParser parser)
    {
        GetOutgoingTransportMap(parser.RakNetPacketId)[parser.SubId] = parser;
        OutgoingRoute route = new(parser.RakNetPacketId, parser.SubId, parser.RakNetPacketId == RakNetPacketId.ArizonaCefEx, parser);
        AddOutgoingRoute(parser.ParsedType, route);
        if (TryGetWrappedPayloadType(parser.ParsedType, typeof(OutgoingSubPacket<>), out Type payloadType))
        {
            AddOutgoingRoute(payloadType, route);
        }
    }

    internal void Register(IIncomingPacketTransportRouter router)
    {
        _incomingTransportRouters[(int)router.PacketId] = router;
    }

    internal void Register(IOutgoingPacketTransportRouter router)
    {
        _outgoingTransportRouters[(int)router.PacketId] = router;
    }

    public bool TryParseIncoming(IncomingPacketArgs args, out PacketParseResult result)
    {
        if (_incomingTransportRouters.TryGetValue(args.RakNetPacketId, out IIncomingPacketTransportRouter? transportRouter))
        {
            return transportRouter.TryParse(args, this, out result);
        }

        if (_incoming.TryGetValue(args.RakNetPacketId, out IIncomingPacketParser? parser))
        {
            return parser.TryParse(args, out result);
        }

        result = PacketParseResult.Unsupported((RakNetPacketId)args.RakNetPacketId);
        return false;
    }

    public bool TryParseOutgoing(OutgoingPacketArgs args, out PacketParseResult result)
    {
        if (_outgoingTransportRouters.TryGetValue(args.RakNetPacketId, out IOutgoingPacketTransportRouter? transportRouter))
        {
            return transportRouter.TryParse(args, this, out result);
        }

        if (_outgoing.TryGetValue(args.RakNetPacketId, out IOutgoingPacketParser? parser))
        {
            return parser.TryParse(args, out result);
        }

        result = PacketParseResult.Unsupported((RakNetPacketId)args.RakNetPacketId);
        return false;
    }

    internal bool TryGetIncomingTransportParser(RakNetPacketId packetId, int subId, out IIncomingArizonaPacketParser? parser)
    {
        return GetIncomingTransportMap(packetId).TryGetValue(subId, out parser);
    }

    internal bool TryGetOutgoingTransportParser(RakNetPacketId packetId, int subId, out IOutgoingArizonaPacketParser? parser)
    {
        return GetOutgoingTransportMap(packetId).TryGetValue(subId, out parser);
    }

    public IReadOnlyList<IncomingRoute> GetIncomingRoutes<TPacket>()
    {
        if (_incomingRoutesByType.TryGetValue(typeof(TPacket), out List<IncomingRoute>? routes))
        {
            return routes;
        }

        return Array.Empty<IncomingRoute>();
    }

    public IReadOnlyList<OutgoingRoute> GetOutgoingRoutes<TPacket>()
    {
        if (_outgoingRoutesByType.TryGetValue(typeof(TPacket), out List<OutgoingRoute>? routes))
        {
            return routes;
        }

        return Array.Empty<OutgoingRoute>();
    }

    private Dictionary<int, IIncomingArizonaPacketParser> GetIncomingTransportMap(RakNetPacketId packetId)
    {
        return packetId switch
        {
            RakNetPacketId.ArizonaCefEx => _incomingTransportParsers221,
            RakNetPacketId.AZVoice => _incomingTransportParsersAZVoice,
            _ => _incomingTransportParsers220,
        };
    }

    private Dictionary<int, IOutgoingArizonaPacketParser> GetOutgoingTransportMap(RakNetPacketId packetId)
    {
        return packetId switch
        {
            RakNetPacketId.ArizonaCefEx => _outgoingTransportParsers221,
            _ => _outgoingTransportParsers220,
        };
    }

    private void AddIncomingRoute(Type type, IncomingRoute route)
    {
        if (!_incomingRoutesByType.TryGetValue(type, out List<IncomingRoute>? routes))
        {
            routes = new List<IncomingRoute>();
            _incomingRoutesByType[type] = routes;
        }

        routes.Add(route);
    }

    private void AddOutgoingRoute(Type type, OutgoingRoute route)
    {
        if (!_outgoingRoutesByType.TryGetValue(type, out List<OutgoingRoute>? routes))
        {
            routes = new List<OutgoingRoute>();
            _outgoingRoutesByType[type] = routes;
        }

        routes.Add(route);
    }

    private static bool TryGetWrappedPayloadType(Type parsedType, Type wrapperGenericType, out Type payloadType)
    {
        payloadType = null!;
        if (!parsedType.IsGenericType || parsedType.GetGenericTypeDefinition() != wrapperGenericType)
        {
            return false;
        }

        payloadType = parsedType.GetGenericArguments()[0];
        return true;
    }

    public sealed record IncomingRoute(RakNetPacketId RakNetPacketId, int? SubId, bool IsEx, object Parser);
    public sealed record OutgoingRoute(RakNetPacketId RakNetPacketId, int? SubId, bool IsEx, object Parser);
}
