using SFSharp;
using SFSharp.Interop.RakNet.Packets.Enum;

public partial class DebugModule
{
    private static (string? Name, string? Detail, string? Parsed) DecodeIncomingRpc(IncomingRpcArgs args)
    {
        if (SF.RpcParsers.TryParseIncoming(args, out RpcParseResult result) && result.Rpc is IParsedIncomingRpc rpc)
        {
            return (rpc.Name, $"rpcId={args.ERpcId} {rpc.Detail}", rpc.Detail);
        }

        string? name = Enum.IsDefined((ERpcId)args.ERpcId) ? ((ERpcId)args.ERpcId).ToString() : null;
        return (name, $"rpcId={args.ERpcId}", null);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeOutgoingRpc(OutgoingRpcArgs args)
    {
        if (SF.RpcParsers.TryParseOutgoing(args, out RpcParseResult result) && result.Rpc is IParsedOutgoingRpc rpc)
        {
            return (rpc.Name, $"rpcId={args.ERpcId} {rpc.Detail}", rpc.Detail);
        }

        string? name = Enum.IsDefined((ERpcId)args.ERpcId) ? ((ERpcId)args.ERpcId).ToString() : null;
        return (name, $"rpcId={args.ERpcId}", null);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeIncomingPacket(IncomingPacketArgs args)
    {
        if (SF.PacketParsers.TryParseIncoming(args, out PacketParseResult result) && result.Packet is IParsedIncomingPacket packet)
        {
            (string? name, string? detail) = FormatParsedPacket(packet, args.EPacketId);
            return (name, detail, packet.Detail);
        }

        (string? fn, string? fd) = FormatPacketParseFailure(args.EPacketId, result, TryReadArizonaSubId(args));
        return (fn, fd, null);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeOutgoingPacket(OutgoingPacketArgs args)
    {
        if (SF.PacketParsers.TryParseOutgoing(args, out PacketParseResult result) && result.Packet is IParsedOutgoingPacket packet)
        {
            (string? name, string? detail) = FormatParsedPacket(packet, args.EPacketId);
            return (name, detail, packet.Detail);
        }

        (string? fn, string? fd) = FormatPacketParseFailure(args.EPacketId, result, TryReadArizonaSubId(args));
        return (fn, fd, null);
    }

    private static (string? Name, string? Detail) FormatParsedPacket(IParsedPacket packet, int rawPacketId)
    {
        if (packet is IParsedArizonaPacket arizonaPacket)
        {
            EPacketId packetId = (EPacketId)rawPacketId;
            string transport = packetId == EPacketId.ArizonaCefEx ? "Arizona221" : "Arizona220";
            string name = $"{transport}:{packet.Name}";
            string detail = packet.Detail is { Length: > 0 }
                ? $"subId={arizonaPacket.SubId} {packet.Detail}"
                : $"subId={arizonaPacket.SubId}";
            return (name, detail);
        }
        return (packet.Name, packet.Detail);
    }

    private static (string? Name, string? Detail) FormatPacketParseFailure(int rawPacketId, PacketParseResult result, int? arizonaSubId)
    {
        string? fallbackName = Enum.IsDefined((EPacketId)rawPacketId) ? ((EPacketId)rawPacketId).ToString() : null;
        if (arizonaSubId is int subId)
        {
            EPacketId packetId = (EPacketId)rawPacketId;
            string transport = packetId == EPacketId.ArizonaCefEx ? "Arizona221" : "Arizona220";
            return ($"{transport}:{fallbackName}", $"subId={subId}");
        }
        return (fallbackName, $"packetId={rawPacketId}");
    }

    private static int? TryReadArizonaSubId(IncomingPacketArgs args)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId is not (EPacketId.ArizonaCef or EPacketId.ArizonaCefEx)) return null;
        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            return packetId == EPacketId.ArizonaCef
                ? ArizonaPacket.ReadSubId220(ref reader)
                : ArizonaPacket.ReadSubId221(ref reader);
        }
        catch { return null; }
    }

    private static int? TryReadArizonaSubId(OutgoingPacketArgs args)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId is not (EPacketId.ArizonaCef or EPacketId.ArizonaCefEx)) return null;
        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            return packetId == EPacketId.ArizonaCef
                ? ArizonaPacket.ReadSubId220(ref reader)
                : ArizonaPacket.ReadSubId221(ref reader);
        }
        catch { return null; }
    }
}
