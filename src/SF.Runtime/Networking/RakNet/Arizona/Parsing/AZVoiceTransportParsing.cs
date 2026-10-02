namespace SFSharp.Runtime.Networking.RakNet.Arizona.Parsing;

internal static class AZVoiceTransportParsing
{
    public static IncomingSubPacket<TPayload> ParseIncomingControl<TPayload>(IncomingArizonaPacketArgs args, EAZVoice subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        SampBitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(EPacketId.AZVoice, (int)subId, packetName, parser(ref reader));
    }
}
