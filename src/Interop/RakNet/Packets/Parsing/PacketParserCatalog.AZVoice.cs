using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public static partial class PacketParserCatalog
{
    private static void RegisterAZVoice(PacketParserRegistry registry)
    {
        // Incoming (server -> client) — AZVoice control sub-packets (sub-ID 3-23)
        RegisterAZVoiceIncoming(registry, EAZVoice.PluginInit, AZVoiceRpc.ParsePluginInit);
        RegisterAZVoiceIncoming(registry, EAZVoice.CreateStaticAudioStream, AZVoiceRpc.ParseCreateStaticAudioStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.DeleteStream, AZVoiceRpc.ParseDeleteStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.ResetStreams, AZVoiceRpc.ParseResetStreams);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamParameter, AZVoiceRpc.ParseSetStreamParameter);
        RegisterAZVoiceIncoming(registry, EAZVoice.CreateFullStream, AZVoiceRpc.ParseCreateFullStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.DeleteStreamByChannel, AZVoiceRpc.ParseDeleteStreamByChannel);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamChannel, AZVoiceRpc.ParseSetStreamChannel);
        RegisterAZVoiceIncoming(registry, EAZVoice.ResumeStream, AZVoiceRpc.ParseResumeStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamPlaybackPosition, AZVoiceRpc.ParseSetStreamPlaybackPosition);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamPlaybackPosition2, AZVoiceRpc.ParseSetStreamPlaybackPosition2);
        RegisterAZVoiceIncoming(registry, EAZVoice.PauseStream, AZVoiceRpc.ParsePauseStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.UpdateStreamEffect, AZVoiceRpc.ParseUpdateStreamEffect);
        RegisterAZVoiceIncoming(registry, EAZVoice.StopStreamPlayback, AZVoiceRpc.ParseStopStreamPlayback);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamTransient, AZVoiceRpc.ParseSetStreamTransient);
        RegisterAZVoiceIncoming(registry, EAZVoice.UpdateStreamSource, AZVoiceRpc.ParseUpdateStreamSource);
        RegisterAZVoiceIncoming(registry, EAZVoice.DestroyStreamObject, AZVoiceRpc.ParseDestroyStreamObject);
        RegisterAZVoiceIncoming(registry, EAZVoice.Disconnect, AZVoiceRpc.ParseDisconnect);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetReadyFlag, AZVoiceRpc.ParseSetReadyFlag);

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
