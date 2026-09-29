namespace SFSharp.Runtime.Network.RakNet.Arizona;

internal delegate TPayload ArizonaReaderParser<TPayload>(ref BitStreamReader reader);

internal static class ArizonaPacketTransportParsing
{
    public static IncomingSubPacket<TPayload> ParseIncoming220<TPayload>(IncomingArizonaPacketArgs args, EArizona subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(EPacketId.ArizonaCef, (int)subId, packetName, parser(ref reader));
    }

    public static OutgoingSubPacket<TPayload> ParseOutgoing220<TPayload>(OutgoingArizonaPacketArgs args, EArizona subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingSubPacket<TPayload>(EPacketId.ArizonaCef, (int)subId, packetName, parser(ref reader));
    }

    public static IncomingSubPacket<TPayload> ParseIncoming221<TPayload>(IncomingArizonaPacketArgs args, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(EPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }

    public static OutgoingSubPacket<TPayload> ParseOutgoing221<TPayload>(OutgoingArizonaPacketArgs args, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingSubPacket<TPayload>(EPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }
}
