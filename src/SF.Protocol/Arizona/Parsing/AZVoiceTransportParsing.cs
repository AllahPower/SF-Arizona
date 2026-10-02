namespace SFSharp.Protocol.Arizona.Parsing;

internal static class AZVoiceTransportParsing
{
    public static IncomingSubPacket<TPayload> ParseIncomingControl<TPayload>(IncomingArizonaPacketArgs args, AZVoiceMessageId subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        SampBitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(RakNetPacketId.AZVoice, (int)subId, packetName, parser(ref reader));
    }
}
