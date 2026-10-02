namespace SFSharp.Protocol.Rpc.Parsing;

public sealed class RpcParserRegistry
{
    private readonly Dictionary<int, IIncomingRpcParser> _incoming = new();
    private readonly Dictionary<int, IOutgoingRpcParser> _outgoing = new();
    private readonly Dictionary<Type, List<IncomingRpcRoute>> _incomingRoutesByType = new();
    private readonly Dictionary<Type, List<OutgoingRpcRoute>> _outgoingRoutesByType = new();

    public void Register(IIncomingRpcParser parser)
    {
        _incoming[(int)parser.SampRpcId] = parser;
        IncomingRpcRoute route = new(parser.SampRpcId, parser);
        AddIncomingRoute(parser.ParsedType, route);
        if (TryGetWrappedPayloadType(parser.ParsedType, typeof(IncomingRpc<>), out Type payloadType))
        {
            AddIncomingRoute(payloadType, route);
        }
    }

    public void Register(IOutgoingRpcParser parser)
    {
        _outgoing[(int)parser.SampRpcId] = parser;
        OutgoingRpcRoute route = new(parser.SampRpcId, parser);
        AddOutgoingRoute(parser.ParsedType, route);
        if (TryGetWrappedPayloadType(parser.ParsedType, typeof(OutgoingRpc<>), out Type payloadType))
        {
            AddOutgoingRoute(payloadType, route);
        }
    }

    public bool TryParseIncoming(IncomingRpcArgs args, out RpcParseResult result)
    {
        if (_incoming.TryGetValue(args.SampRpcId, out IIncomingRpcParser? parser))
        {
            return parser.TryParse(args, out result);
        }

        result = new RpcParseResult(
            true,
            new IncomingUnknownRpc((SampRpcId)args.SampRpcId, args.DataBitLength),
            "Unknown",
            PacketParseFailureReason.None);
        return true;
    }

    public bool TryParseOutgoing(OutgoingRpcArgs args, out RpcParseResult result)
    {
        if (_outgoing.TryGetValue(args.SampRpcId, out IOutgoingRpcParser? parser))
        {
            return parser.TryParse(args, out result);
        }

        result = new RpcParseResult(
            true,
            new OutgoingUnknownRpc((SampRpcId)args.SampRpcId, args.DataBitLength),
            "Unknown",
            PacketParseFailureReason.None);
        return true;
    }

    public IReadOnlyList<IncomingRpcRoute> GetIncomingRoutes<TRpc>()
    {
        if (_incomingRoutesByType.TryGetValue(typeof(TRpc), out List<IncomingRpcRoute>? routes))
        {
            return routes;
        }

        return Array.Empty<IncomingRpcRoute>();
    }

    public IReadOnlyList<OutgoingRpcRoute> GetOutgoingRoutes<TRpc>()
    {
        if (_outgoingRoutesByType.TryGetValue(typeof(TRpc), out List<OutgoingRpcRoute>? routes))
        {
            return routes;
        }

        return Array.Empty<OutgoingRpcRoute>();
    }

    private void AddIncomingRoute(Type type, IncomingRpcRoute route)
    {
        if (!_incomingRoutesByType.TryGetValue(type, out List<IncomingRpcRoute>? routes))
        {
            routes = new List<IncomingRpcRoute>();
            _incomingRoutesByType[type] = routes;
        }

        routes.Add(route);
    }

    private void AddOutgoingRoute(Type type, OutgoingRpcRoute route)
    {
        if (!_outgoingRoutesByType.TryGetValue(type, out List<OutgoingRpcRoute>? routes))
        {
            routes = new List<OutgoingRpcRoute>();
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

    public sealed record IncomingRpcRoute(SampRpcId SampRpcId, object Parser);
    public sealed record OutgoingRpcRoute(SampRpcId SampRpcId, object Parser);
}
