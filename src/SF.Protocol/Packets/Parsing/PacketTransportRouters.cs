namespace SFSharp.Protocol.Packets.Parsing;

internal interface IIncomingPacketTransportRouter
{
    RakNetPacketId PacketId { get; }

    bool TryParse(IncomingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result);
}

internal interface IOutgoingPacketTransportRouter
{
    RakNetPacketId PacketId { get; }

    bool TryParse(OutgoingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result);
}

internal sealed class Arizona220PacketTransportRouter : IIncomingPacketTransportRouter, IOutgoingPacketTransportRouter
{
    private const int PayloadBitOffset = 16;

    public RakNetPacketId PacketId => RakNetPacketId.ArizonaCef;

    public bool TryParse(IncomingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result)
    {
        if (!TryCreateIncomingArgs(args, out IncomingArizonaPacketArgs packetArgs))
        {
            result = PacketParseResult.TooShort(PacketId.ToString());
            return false;
        }

        if (registry.TryGetIncomingTransportParser(PacketId, packetArgs.SubId, out IIncomingArizonaPacketParser? parser))
        {
            return parser!.TryParse(packetArgs, out result);
        }

        result = new PacketParseResult(
            true,
            new IncomingUnknownArizonaPacket(PacketId, packetArgs.SubId, packetArgs.PayloadBitLength, "ArizonaCef"),
            PacketId.ToString(),
            PacketParseFailureReason.None);
        return true;
    }

    public bool TryParse(OutgoingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result)
    {
        if (!TryCreateOutgoingArgs(args, out OutgoingArizonaPacketArgs packetArgs))
        {
            result = PacketParseResult.TooShort(PacketId.ToString());
            return false;
        }

        if (registry.TryGetOutgoingTransportParser(PacketId, packetArgs.SubId, out IOutgoingArizonaPacketParser? parser))
        {
            return parser!.TryParse(packetArgs, out result);
        }

        result = new PacketParseResult(
            true,
            new OutgoingUnknownArizonaPacket(PacketId, packetArgs.SubId, packetArgs.PayloadBitLength, "ArizonaCef"),
            PacketId.ToString(),
            PacketParseFailureReason.None);
        return true;
    }

    private static bool TryCreateIncomingArgs(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.DataBitLength < PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoingArgs(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.DataBitLength < PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
            return true;
        }
    }
}

internal sealed class Arizona221PacketTransportRouter : IIncomingPacketTransportRouter, IOutgoingPacketTransportRouter
{
    private const int PayloadBitOffset = 24;

    public RakNetPacketId PacketId => RakNetPacketId.ArizonaCefEx;

    public bool TryParse(IncomingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result)
    {
        if (!TryCreateIncomingArgs(args, out IncomingArizonaPacketArgs packetArgs))
        {
            result = PacketParseResult.TooShort(PacketId.ToString());
            return false;
        }

        if (registry.TryGetIncomingTransportParser(PacketId, packetArgs.SubId, out IIncomingArizonaPacketParser? parser))
        {
            return parser!.TryParse(packetArgs, out result);
        }

        result = new PacketParseResult(
            true,
            new IncomingUnknownArizonaPacket(PacketId, packetArgs.SubId, packetArgs.PayloadBitLength, "ArizonaCefEx"),
            PacketId.ToString(),
            PacketParseFailureReason.None);
        return true;
    }

    public bool TryParse(OutgoingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result)
    {
        if (!TryCreateOutgoingArgs(args, out OutgoingArizonaPacketArgs packetArgs))
        {
            result = PacketParseResult.TooShort(PacketId.ToString());
            return false;
        }

        if (registry.TryGetOutgoingTransportParser(PacketId, packetArgs.SubId, out IOutgoingArizonaPacketParser? parser))
        {
            return parser!.TryParse(packetArgs, out result);
        }

        result = new PacketParseResult(
            true,
            new OutgoingUnknownArizonaPacket(PacketId, packetArgs.SubId, packetArgs.PayloadBitLength, "ArizonaCefEx"),
            PacketId.ToString(),
            PacketParseFailureReason.None);
        return true;
    }

    private static bool TryCreateIncomingArgs(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.DataBitLength < PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
            return true;
        }
    }

    private static bool TryCreateOutgoingArgs(OutgoingPacketArgs args, out OutgoingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (args.DataBitLength < PayloadBitOffset)
        {
            return false;
        }

        unsafe
        {
            SampBitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.RakNetPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
            return true;
        }
    }
}

internal sealed class AZVoiceIncomingPacketTransportRouter : IIncomingPacketTransportRouter
{
    public RakNetPacketId PacketId => RakNetPacketId.AZVoice;

    public bool TryParse(IncomingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result)
    {
        if (AZVoiceTransport.TryClassifyIncomingPacket(args, out AZVoiceTransport.IncomingPacketClassification classification))
        {
            if (classification.Kind == AZVoiceTransport.IncomingKind.VoiceData)
            {
                IncomingAZVoiceDataPacket packet = new(classification.VoiceData);
                result = new PacketParseResult(true, packet, packet.Name, PacketParseFailureReason.None);
                return true;
            }

            if (registry.TryGetIncomingTransportParser(PacketId, classification.ControlArgs.SubId, out IIncomingArizonaPacketParser? parser))
            {
                return parser!.TryParse(classification.ControlArgs, out result);
            }

            result = new PacketParseResult(
                true,
                new IncomingUnknownArizonaPacket(PacketId, classification.ControlArgs.SubId, classification.ControlArgs.PayloadBitLength, "AZVoice"),
                PacketId.ToString(),
                PacketParseFailureReason.None);
            return true;
        }

        ProtocolDiagnostics.Warn($"AZVoice packet parse failed: packetId={RakNetPacketId.AZVoice} bits={args.DataBitLength} error=unrecognized raw 252 payload");
        result = PacketParseResult.Unsupported(RakNetPacketId.AZVoice);
        return false;
    }
}
