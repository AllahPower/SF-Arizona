using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public static partial class PacketParserCatalog
{
    private static void RegisterAZVoice(PacketParserRegistry registry)
    {
        // Incoming (server -> client) — AZVoice control sub-packets (sub-ID 3-23)
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.PluginInit, AZVoiceRpc.ParsePluginInit);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.CreateStaticAudioStream, AZVoiceRpc.ParseCreateStaticAudioStream);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.DeleteStream, AZVoiceRpc.ParseDeleteStream);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.ResetStreams, AZVoiceRpc.ParseResetStreams);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.SetStreamParameter, AZVoiceRpc.ParseSetStreamParameter);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.CreateFullStream, AZVoiceRpc.ParseCreateFullStream);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.DeleteStreamByChannel, AZVoiceRpc.ParseDeleteStreamByChannel);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.SetStreamChannel, AZVoiceRpc.ParseSetStreamChannel);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.ResumeStream, AZVoiceRpc.ParseResumeStream);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.SetStreamPlaybackPosition, AZVoiceRpc.ParseSetStreamPlaybackPosition);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.SetStreamPlaybackPosition2, AZVoiceRpc.ParseSetStreamPlaybackPosition2);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.PauseStream, AZVoiceRpc.ParsePauseStream);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.UpdateStreamEffect, AZVoiceRpc.ParseUpdateStreamEffect);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.StopStreamPlayback, AZVoiceRpc.ParseStopStreamPlayback);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.SetStreamTransient, AZVoiceRpc.ParseSetStreamTransient);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.UpdateStreamSource, AZVoiceRpc.ParseUpdateStreamSource);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.DestroyStreamObject, AZVoiceRpc.ParseDestroyStreamObject);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.Disconnect, AZVoiceRpc.ParseDisconnect);
        RegisterAZVoiceIncoming(registry, EAZVoiceSubRpcId.SetReadyFlag, AZVoiceRpc.ParseSetReadyFlag);

        // Outgoing (client -> server) — raw voice data only (no sub-ID dispatch)
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAZVoiceDataPacket>(
            EPacketId.AZVoice, ParseOutgoingAZVoiceData, name: "AZVoice:VoiceData", minimumBitLength: 32));
    }

    private static OutgoingAZVoiceDataPacket ParseOutgoingAZVoiceData(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1); // skip packet ID (0xFC)
        var data = AZVoiceRpc.ParseOutgoingVoiceData(ref reader);
        return new OutgoingAZVoiceDataPacket(data);
    }
}
