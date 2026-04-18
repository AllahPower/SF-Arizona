using SFSharp.Abstractions.Interop.RakNet;
using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Network.RakNet.Arizona;

internal static class AZVoiceTransportParsing
{
    public static IncomingSubPacket<TPayload> ParseIncomingControl<TPayload>(IncomingArizonaPacketArgs args, EAZVoice subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingSubPacket<TPayload>(EPacketId.AZVoice, (int)subId, packetName, parser(ref reader));
    }
}
