namespace SFSharp.Runtime.Diagnostics.DebugWeb;

public partial class DebugModule
{
    private static (string? Name, string? Detail, string? Parsed) DecodeIncomingRpc(IncomingRpcArgs args)
    {
        if (SF.RpcParsers.TryParseIncoming(args, out RpcParseResult result) && result.Rpc is IParsedIncomingRpc rpc)
        {
            return (rpc.Name, $"rpcId={args.SampRpcId} {rpc.Detail}", rpc.Detail);
        }

        string? name = Enum.IsDefined((SampRpcId)args.SampRpcId) ? ((SampRpcId)args.SampRpcId).ToString() : null;
        return (name, $"rpcId={args.SampRpcId}", null);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeOutgoingRpc(OutgoingRpcArgs args)
    {
        if (SF.RpcParsers.TryParseOutgoing(args, out RpcParseResult result) && result.Rpc is IParsedOutgoingRpc rpc)
        {
            return (rpc.Name, $"rpcId={args.SampRpcId} {rpc.Detail}", rpc.Detail);
        }

        string? name = Enum.IsDefined((SampRpcId)args.SampRpcId) ? ((SampRpcId)args.SampRpcId).ToString() : null;
        return (name, $"rpcId={args.SampRpcId}", null);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeIncomingPacket(IncomingPacketArgs args)
    {
        if (SF.PacketParsers.TryParseIncoming(args, out PacketParseResult result) && result.Packet is IParsedIncomingPacket packet)
        {
            (string? name, string? detail) = FormatParsedPacket(packet, args.RakNetPacketId);
            return (name, detail, packet.Detail);
        }

        (string? fn, string? fd) = FormatPacketParseFailure(args.RakNetPacketId, result, TryReadArizonaSubId(args));
        return (fn, fd, null);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeIncomingAZVoiceControl(IncomingArizonaPacketArgs args)
    {
        if (SF.PacketParsers.Registry.TryGetIncomingTransportParser(RakNetPacketId.AZVoice, args.SubId, out IIncomingArizonaPacketParser? parser)
            && parser is not null
            && parser.TryParse(args, out PacketParseResult result)
            && result.Packet is IParsedIncomingPacket packet)
        {
            (string? name, string? detail) = FormatParsedPacket(packet, args.RakNetPacketId);
            return (name, detail, packet.Detail);
        }

        string? fallbackName = Enum.IsDefined((AZVoiceMessageId)args.SubId) ? ((AZVoiceMessageId)args.SubId).ToString() : null;
        string hex = HexDump(args.DataPtr, args.PayloadBitOffset + args.PayloadBitLength);
        return ($"AZVoice:{fallbackName}", $"subId={args.SubId} hex={hex}", hex);
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeOutgoingAZVoiceControl(OutgoingArizonaPacketArgs args)
    {
        string? fallbackName = Enum.IsDefined((AZVoiceMessageId)args.SubId) ? ((AZVoiceMessageId)args.SubId).ToString() : null;
        string hex = HexDump(args.DataPtr, args.PayloadBitOffset + args.PayloadBitLength);
        return ($"AZVoice:{fallbackName}", $"subId={args.SubId} hex={hex}", hex);
    }

    private static unsafe string HexDump(nint dataPtr, int totalBits)
    {
        int length = (totalBits + 7) / 8;
        if (dataPtr == 0 || length <= 0)
        {
            return string.Empty;
        }

        return Convert.ToHexString(new ReadOnlySpan<byte>((void*)dataPtr, length));
    }

    private static (string? Name, string? Detail, string? Parsed) DecodeOutgoingPacket(OutgoingPacketArgs args)
    {
        if (SF.PacketParsers.TryParseOutgoing(args, out PacketParseResult result) && result.Packet is IParsedOutgoingPacket packet)
        {
            (string? name, string? detail) = FormatParsedPacket(packet, args.RakNetPacketId);
            return (name, detail, packet.Detail);
        }

        (string? fn, string? fd) = FormatPacketParseFailure(args.RakNetPacketId, result, TryReadArizonaSubId(args));
        return (fn, fd, null);
    }

    private static (string? Name, string? Detail) FormatParsedPacket(IParsedPacket packet, int rawPacketId)
    {
        if (packet is IParsedArizonaPacket arizonaPacket)
        {
            RakNetPacketId packetId = (RakNetPacketId)rawPacketId;
            string transport = packetId switch
            {
                RakNetPacketId.ArizonaCefEx => "Arizona221",
                RakNetPacketId.AZVoice => "AZVoice",
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
        string? fallbackName = Enum.IsDefined((RakNetPacketId)rawPacketId) ? ((RakNetPacketId)rawPacketId).ToString() : null;
        if (arizonaSubId is int subId)
        {
            RakNetPacketId packetId = (RakNetPacketId)rawPacketId;
            string transport = packetId switch
            {
                RakNetPacketId.ArizonaCefEx => "Arizona221",
                RakNetPacketId.AZVoice => "AZVoice",
                _ => "Arizona220",
            };
            return ($"{transport}:{fallbackName}", $"subId={subId} {baseDetail}");
        }
        return (fallbackName, $"packetId={rawPacketId} {baseDetail}");
    }

    private static int? TryReadArizonaSubId(IncomingPacketArgs args)
    {
        RakNetPacketId packetId = (RakNetPacketId)args.RakNetPacketId;
        if (packetId is not (RakNetPacketId.ArizonaCef or RakNetPacketId.ArizonaCefEx or RakNetPacketId.AZVoice)) return null;
        try
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            if (packetId == RakNetPacketId.AZVoice)
            {
                return AZVoiceTransport.TryReadIncomingControlId(args, out byte rpcId) ? rpcId : null;
            }
            return packetId == RakNetPacketId.ArizonaCefEx
                ? ArizonaPacket.ReadSubId221(ref reader)
                : ArizonaPacket.ReadSubId220(ref reader);
        }
        catch { return null; }
    }

    private static int? TryReadArizonaSubId(OutgoingPacketArgs args)
    {
        RakNetPacketId packetId = (RakNetPacketId)args.RakNetPacketId;
        if (packetId is not (RakNetPacketId.ArizonaCef or RakNetPacketId.ArizonaCefEx or RakNetPacketId.AZVoice)) return null;
        try
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            if (packetId == RakNetPacketId.AZVoice)
                return null;
            return packetId == RakNetPacketId.ArizonaCefEx
                ? ArizonaPacket.ReadSubId221(ref reader)
                : ArizonaPacket.ReadSubId220(ref reader);
        }
        catch { return null; }
    }
}
