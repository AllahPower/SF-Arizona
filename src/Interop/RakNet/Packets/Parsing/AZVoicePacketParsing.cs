namespace SFSharp;

internal static class AZVoicePacketParsing
{
    public static OutgoingAZVoiceDataPacket ParseOutgoingVoiceDataPacket(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1); // skip packet ID (0xFC)
        AzvOutgoingVoiceData data = AZVoiceParsers.ParseOutgoingVoiceData(ref reader);
        return new OutgoingAZVoiceDataPacket(data);
    }
}
