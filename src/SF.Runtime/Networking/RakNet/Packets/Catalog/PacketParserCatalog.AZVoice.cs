namespace SFSharp.Runtime.Network.RakNet.Packets;

public static partial class PacketParserCatalog
{
    private static void RegisterAZVoice(PacketParserRegistry registry)
    {
        #region incoming (server -> client)
        RegisterAZVoiceIncoming(registry, EAZVoice.PluginInit, AZVoiceParsers.ParsePluginInit);
        RegisterAZVoiceIncoming(registry, EAZVoice.CreateStaticAudioStream, AZVoiceParsers.ParseCreateStaticAudioStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.DeleteStream, AZVoiceParsers.ParseDeleteStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.ResetStreams, AZVoiceParsers.ParseResetStreams);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamParameter, AZVoiceParsers.ParseSetStreamParameter);
        RegisterAZVoiceIncoming(registry, EAZVoice.CreateFullStream, AZVoiceParsers.ParseCreateFullStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.DeleteStreamByChannel, AZVoiceParsers.ParseDeleteStreamByChannel);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamChannel, AZVoiceParsers.ParseSetStreamChannel);
        RegisterAZVoiceIncoming(registry, EAZVoice.ResumeStream, AZVoiceParsers.ParseResumeStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamPlaybackPosition, AZVoiceParsers.ParseSetStreamPlaybackPosition);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamPlaybackPosition2, AZVoiceParsers.ParseSetStreamPlaybackPosition2);
        RegisterAZVoiceIncoming(registry, EAZVoice.PauseStream, AZVoiceParsers.ParsePauseStream);
        RegisterAZVoiceIncoming(registry, EAZVoice.UpdateStreamEffect, AZVoiceParsers.ParseUpdateStreamEffect);
        RegisterAZVoiceIncoming(registry, EAZVoice.StopStreamPlayback, AZVoiceParsers.ParseStopStreamPlayback);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetStreamTransient, AZVoiceParsers.ParseSetStreamTransient);
        RegisterAZVoiceIncoming(registry, EAZVoice.UpdateStreamSource, AZVoiceParsers.ParseUpdateStreamSource);
        RegisterAZVoiceIncoming(registry, EAZVoice.DestroyStreamObject, AZVoiceParsers.ParseDestroyStreamObject);
        RegisterAZVoiceIncoming(registry, EAZVoice.Disconnect, AZVoiceParsers.ParseDisconnect);
        RegisterAZVoiceIncoming(registry, EAZVoice.SetReadyFlag, AZVoiceParsers.ParseSetReadyFlag);

        #endregion

        #region outgoing (client -> server)

        registry.Register(new DelegateOutgoingPacketParser<OutgoingAZVoiceDataPacket>(
            EPacketId.AZVoice, AZVoicePacketParsing.ParseOutgoingVoiceDataPacket, name: "AZVoice:VoiceData", minimumBitLength: 32));

        #endregion
    }
}
