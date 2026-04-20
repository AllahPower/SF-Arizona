namespace SFSharp.Runtime.Modules;

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

    private static (string? Name, string? Detail, string? Parsed) DecodeIncomingAZVoiceControl(IncomingArizonaPacketArgs args)
    {
        if (SF.PacketParsers.Registry.TryGetIncomingTransportParser(EPacketId.AZVoice, args.SubId, out IIncomingArizonaPacketParser? parser)
            && parser is not null
            && parser.TryParse(args, out PacketParseResult result)
            && result.Packet is IParsedIncomingPacket packet)
        {
            (string? name, string? detail) = FormatParsedPacket(packet, args.EPacketId);
            return (name, detail, packet.Detail);
        }

        string? fallbackName = Enum.IsDefined(typeof(EAZVoice), args.SubId) ? ((EAZVoice)args.SubId).ToString() : null;
        return ($"AZVoice:{fallbackName}", $"subId={args.SubId}", null);
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
            string transport = packetId switch
            {
                EPacketId.ArizonaCefEx => "Arizona221",
                EPacketId.AZVoice => "AZVoice",
                _ => "Arizona220",
            };
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
        string baseDetail = result.ErrorMessage is { Length: > 0 } errorMessage
            ? $"error={errorMessage}"
            : $"reason={result.FailureReason}";
        string? fallbackName = Enum.IsDefined((EPacketId)rawPacketId) ? ((EPacketId)rawPacketId).ToString() : null;
        if (arizonaSubId is int subId)
        {
            EPacketId packetId = (EPacketId)rawPacketId;
            string transport = packetId switch
            {
                EPacketId.ArizonaCefEx => "Arizona221",
                EPacketId.AZVoice => "AZVoice",
                _ => "Arizona220",
            };
            return ($"{transport}:{fallbackName}", $"subId={subId} {baseDetail}");
        }
        return (fallbackName, $"packetId={rawPacketId} {baseDetail}");
    }

    private static int? TryReadArizonaSubId(IncomingPacketArgs args)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId is not (EPacketId.ArizonaCef or EPacketId.ArizonaCefEx or EPacketId.AZVoice)) return null;
        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            if (packetId == EPacketId.AZVoice)
            {
                return AZVoiceTransport.TryReadIncomingControlId(args, out byte rpcId) ? rpcId : null;
            }
            return packetId == EPacketId.ArizonaCefEx
                ? ArizonaPacket.ReadSubId221(ref reader)
                : ArizonaPacket.ReadSubId220(ref reader);
        }
        catch { return null; }
    }

    private static int? TryReadArizonaSubId(OutgoingPacketArgs args)
    {
        EPacketId packetId = (EPacketId)args.EPacketId;
        if (packetId is not (EPacketId.ArizonaCef or EPacketId.ArizonaCefEx or EPacketId.AZVoice)) return null;
        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            if (packetId == EPacketId.AZVoice)
                return null;
            return packetId == EPacketId.ArizonaCefEx
                ? ArizonaPacket.ReadSubId221(ref reader)
                : ArizonaPacket.ReadSubId220(ref reader);
        }
        catch { return null; }
    }
}
