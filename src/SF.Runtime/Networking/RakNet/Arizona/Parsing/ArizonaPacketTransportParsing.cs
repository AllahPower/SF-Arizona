namespace SFSharp.Runtime.Networking.RakNet.Arizona.Parsing;

internal delegate TPayload ArizonaReaderParser<TPayload>(ref SampBitStreamReader reader);

internal static class ArizonaPacketTransportParsing
{
    public static IncomingSubPacket<TPayload> ParseIncoming220<TPayload>(IncomingArizonaPacketArgs args, ArizonaPacket220Id subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        SampBitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(RakNetPacketId.ArizonaCef, (int)subId, packetName, parser(ref reader));
    }

    public static OutgoingSubPacket<TPayload> ParseOutgoing220<TPayload>(OutgoingArizonaPacketArgs args, ArizonaPacket220Id subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        SampBitStreamReader reader = args.CreateReader();
        return new OutgoingSubPacket<TPayload>(RakNetPacketId.ArizonaCef, (int)subId, packetName, parser(ref reader));
    }

    public static IncomingSubPacket<TPayload> ParseIncoming221<TPayload>(IncomingArizonaPacketArgs args, ArizonaPacket221Id subId, ArizonaReaderParser<TPayload> parser)
    {
        SampBitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(RakNetPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }

    public static OutgoingSubPacket<TPayload> ParseOutgoing221<TPayload>(OutgoingArizonaPacketArgs args, ArizonaPacket221Id subId, ArizonaReaderParser<TPayload> parser)
    {
        SampBitStreamReader reader = args.CreateReader();
        return new OutgoingSubPacket<TPayload>(RakNetPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }
}
