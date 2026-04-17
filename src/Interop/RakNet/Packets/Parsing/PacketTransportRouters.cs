using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Networking;

internal interface IIncomingPacketTransportRouter
{
    EPacketId PacketId { get; }

    bool TryParse(IncomingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result);
}

internal interface IOutgoingPacketTransportRouter
{
    EPacketId PacketId { get; }

    bool TryParse(OutgoingPacketArgs args, PacketParserRegistry registry, out PacketParseResult result);
}

internal sealed class Arizona220PacketTransportRouter : IIncomingPacketTransportRouter, IOutgoingPacketTransportRouter
{
    private const int PayloadBitOffset = 16;

    public EPacketId PacketId => EPacketId.ArizonaCef;

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
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
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
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId220(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
            return true;
        }
    }
}

internal sealed class Arizona221PacketTransportRouter : IIncomingPacketTransportRouter, IOutgoingPacketTransportRouter
{
    private const int PayloadBitOffset = 24;

    public EPacketId PacketId => EPacketId.ArizonaCefEx;

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
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
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
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            int subId = ArizonaPacket.ReadSubId221(ref reader);
            packetArgs = new(args.EPacketId, subId, args.DataPtr, PayloadBitOffset, args.DataBitLength - PayloadBitOffset);
            return true;
        }
    }
}

internal sealed class AZVoiceIncomingPacketTransportRouter : IIncomingPacketTransportRouter
{
    public EPacketId PacketId => EPacketId.AZVoice;

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

        SFLog.Warn($"AZVoice packet parse failed: packetId={EPacketId.AZVoice} bits={args.DataBitLength} error=unrecognized raw 252 payload");
        result = PacketParseResult.Unsupported(EPacketId.AZVoice);
        return false;
    }
}
