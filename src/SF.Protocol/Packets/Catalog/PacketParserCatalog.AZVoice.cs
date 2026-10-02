namespace SFSharp.Protocol.Packets.Catalog;

public static partial class PacketParserCatalog
{
    private static void RegisterAZVoice(PacketParserRegistry registry)
    {
        #region incoming (server -> client)
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.PluginInit, AZVoiceParsers.ParsePluginInit);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.CreateStaticAudioStream, AZVoiceParsers.ParseCreateStaticAudioStream);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.DeleteStream, AZVoiceParsers.ParseDeleteStream);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.ResetStreams, AZVoiceParsers.ParseResetStreams);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.SetStreamParameter, AZVoiceParsers.ParseSetStreamParameter);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.CreateFullStream, AZVoiceParsers.ParseCreateFullStream);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.DeleteStreamByChannel, AZVoiceParsers.ParseDeleteStreamByChannel);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.SetStreamChannel, AZVoiceParsers.ParseSetStreamChannel);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.ResumeStream, AZVoiceParsers.ParseResumeStream);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.SetStreamPlaybackPosition, AZVoiceParsers.ParseSetStreamPlaybackPosition);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.SetStreamPlaybackPosition2, AZVoiceParsers.ParseSetStreamPlaybackPosition2);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.PauseStream, AZVoiceParsers.ParsePauseStream);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.UpdateStreamEffect, AZVoiceParsers.ParseUpdateStreamEffect);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.StopStreamPlayback, AZVoiceParsers.ParseStopStreamPlayback);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.SetStreamTransient, AZVoiceParsers.ParseSetStreamTransient);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.UpdateStreamSource, AZVoiceParsers.ParseUpdateStreamSource);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.DestroyStreamObject, AZVoiceParsers.ParseDestroyStreamObject);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.Disconnect, AZVoiceParsers.ParseDisconnect);
        RegisterAZVoiceIncoming(registry, AZVoiceMessageId.SetReadyFlag, AZVoiceParsers.ParseSetReadyFlag);

        #endregion

        #region outgoing (client -> server)

        registry.Register(new DelegateOutgoingPacketParser<OutgoingAZVoiceDataPacket>(
            RakNetPacketId.AZVoice, AZVoicePacketParsing.ParseOutgoingVoiceDataPacket, name: "AZVoice:VoiceData", minimumBitLength: 32));

        #endregion
    }
}
