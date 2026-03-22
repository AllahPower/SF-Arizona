using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public sealed class PacketParserRegistry
{
    private readonly Dictionary<int, IIncomingPacketParser> _incoming = new();
    private readonly Dictionary<int, IOutgoingPacketParser> _outgoing = new();
    private readonly Dictionary<int, IIncomingArizonaPacketParser> _incomingArizona220 = new();
    private readonly Dictionary<int, IIncomingArizonaPacketParser> _incomingArizona221 = new();
    private readonly Dictionary<int, IOutgoingArizonaPacketParser> _outgoingArizona220 = new();
    private readonly Dictionary<int, IOutgoingArizonaPacketParser> _outgoingArizona221 = new();
    private readonly Dictionary<Type, List<IncomingRoute>> _incomingRoutesByType = new();
    private readonly Dictionary<Type, List<OutgoingRoute>> _outgoingRoutesByType = new();

    public void Register(IIncomingPacketParser parser)
    {
        _incoming[(int)parser.EPacketId] = parser;
        AddIncomingRoute(parser.ParsedType, new IncomingRoute(parser.EPacketId, null, false, parser));
    }

    public void Register(IOutgoingPacketParser parser)
    {
        _outgoing[(int)parser.EPacketId] = parser;
        AddOutgoingRoute(parser.ParsedType, new OutgoingRoute(parser.EPacketId, null, false, parser));
    }

    public void Register(IIncomingArizonaPacketParser parser)
    {
        GetIncomingArizonaMap(parser.EPacketId)[parser.SubId] = parser;
        AddIncomingRoute(parser.ParsedType, new IncomingRoute(parser.EPacketId, parser.SubId, parser.EPacketId == EPacketId.ArizonaCefEx, parser));
    }

    public void Register(IOutgoingArizonaPacketParser parser)
    {
        GetOutgoingArizonaMap(parser.EPacketId)[parser.SubId] = parser;
        AddOutgoingRoute(parser.ParsedType, new OutgoingRoute(parser.EPacketId, parser.SubId, parser.EPacketId == EPacketId.ArizonaCefEx, parser));
    }

    public bool TryParseIncoming(IncomingPacketArgs args, out PacketParseResult result)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId == EPacketId.ArizonaCef || packetId == EPacketId.ArizonaCefEx)
        {
            return TryParseIncomingArizona(args, packetId, out result);
        }

        if (_incoming.TryGetValue(args.EPacketId, out IIncomingPacketParser? parser))
        {
            return parser.TryParse(args, out result);
        }

        result = PacketParseResult.Unsupported(packetId);
        return false;
    }

    public bool TryParseOutgoing(OutgoingPacketArgs args, out PacketParseResult result)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId == EPacketId.ArizonaCef || packetId == EPacketId.ArizonaCefEx)
        {
            return TryParseOutgoingArizona(args, packetId, out result);
        }

        if (_outgoing.TryGetValue(args.EPacketId, out IOutgoingPacketParser? parser))
        {
            return parser.TryParse(args, out result);
        }

        result = PacketParseResult.Unsupported(packetId);
        return false;
    }

    public IReadOnlyList<IncomingRoute> GetIncomingRoutes<TPacket>() where TPacket : class, IParsedIncomingPacket
    {
        if (_incomingRoutesByType.TryGetValue(typeof(TPacket), out List<IncomingRoute>? routes))
        {
            return routes;
        }

        return Array.Empty<IncomingRoute>();
    }

    public IReadOnlyList<OutgoingRoute> GetOutgoingRoutes<TPacket>() where TPacket : class, IParsedOutgoingPacket
    {
        if (_outgoingRoutesByType.TryGetValue(typeof(TPacket), out List<OutgoingRoute>? routes))
        {
            return routes;
        }

        return Array.Empty<OutgoingRoute>();
    }

    private bool TryParseIncomingArizona(IncomingPacketArgs args, EPacketId packetId, out PacketParseResult result)
    {
        if (!TryCreateIncomingArizonaArgs(args, packetId, out IncomingArizonaPacketArgs packetArgs))
        {
            result = PacketParseResult.TooShort(packetId.ToString());
            return false;
        }

        Dictionary<int, IIncomingArizonaPacketParser> map = GetIncomingArizonaMap(packetId);
        if (map.TryGetValue(packetArgs.SubId, out IIncomingArizonaPacketParser? parser))
        {
            return parser.TryParse(packetArgs, out result);
        }

        result = new PacketParseResult(
            true,
            new IncomingUnknownArizonaPacket(packetId, packetArgs.SubId, packetArgs.PayloadBitLength, packetId == EPacketId.ArizonaCef ? "ArizonaCef" : "ArizonaCefEx"),
            packetId.ToString(),
            PacketParseFailureReason.None);
        return true;
    }

    private bool TryParseOutgoingArizona(OutgoingPacketArgs args, EPacketId packetId, out PacketParseResult result)
    {
        if (!TryCreateOutgoingArizonaArgs(args, packetId, out OutgoingArizonaPacketArgs packetArgs))
        {
            result = PacketParseResult.TooShort(packetId.ToString());
            return false;
        }

        Dictionary<int, IOutgoingArizonaPacketParser> map = GetOutgoingArizonaMap(packetId);
        if (map.TryGetValue(packetArgs.SubId, out IOutgoingArizonaPacketParser? parser))
        {
            return parser.TryParse(packetArgs, out result);
        }

        result = new PacketParseResult(
            true,
            new OutgoingUnknownArizonaPacket(packetId, packetArgs.SubId, packetArgs.PayloadBitLength, packetId == EPacketId.ArizonaCef ? "ArizonaCef" : "ArizonaCefEx"),
            packetId.ToString(),
            PacketParseFailureReason.None);
        return true;
    }

    private static bool TryCreateIncomingArizonaArgs(IncomingPacketArgs args, EPacketId packetId, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        int payloadBitOffset = packetId == EPacketId.ArizonaCef ? 16 : 24;
        if (args.DataBitLength < payloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = packetId == EPacketId.ArizonaCef ? ArizonaPacket.ReadSubId220(ref reader) : ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, payloadBitOffset, args.DataBitLength - payloadBitOffset);
            return true;
        }
    }
    
    private static bool TryCreateOutgoingArizonaArgs(OutgoingPacketArgs args, EPacketId packetId, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        int payloadBitOffset = packetId == EPacketId.ArizonaCef ? 16 : 24;
        if (args.DataBitLength < payloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = packetId == EPacketId.ArizonaCef ? ArizonaPacket.ReadSubId220(ref reader) : ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, payloadBitOffset, args.DataBitLength - payloadBitOffset);
            return true;
        }
    }

    private Dictionary<int, IIncomingArizonaPacketParser> GetIncomingArizonaMap(EPacketId packetId)
    {
        return packetId == EPacketId.ArizonaCefEx ? _incomingArizona221 : _incomingArizona220;
    }

    private Dictionary<int, IOutgoingArizonaPacketParser> GetOutgoingArizonaMap(EPacketId packetId)
    {
        return packetId == EPacketId.ArizonaCefEx ? _outgoingArizona221 : _outgoingArizona220;
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

    public sealed record IncomingRoute(EPacketId EPacketId, int? SubId, bool IsEx, object Parser);
    public sealed record OutgoingRoute(EPacketId EPacketId, int? SubId, bool IsEx, object Parser);
}
