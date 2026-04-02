using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

internal static class AZVoiceTransportParsing
{
    public static IncomingSubPacket<TPayload> ParseIncomingControl<TPayload>(IncomingArizonaPacketArgs args, EAZVoice subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(EPacketId.AZVoice, (int)subId, packetName, parser(ref reader));
    }
}
