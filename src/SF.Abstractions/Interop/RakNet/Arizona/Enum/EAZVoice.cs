namespace SFSharp.Abstractions.Interop.RakNet;

/// <summary>
/// AZVoice sub-packet identifiers carried inside packet 252.
/// </summary>
/// <remarks>
/// Assert: the first byte of the AZVoice payload is the sub-ID from this enum.
/// Assert: the remaining bytes are a sub-packet-specific bitstream payload.
/// </remarks>
public enum EAZVoice : byte
{
    #region incoming (server -> client)

    PluginInit = 3,
    CreateStaticAudioStream = 6,
    DeleteStream = 7,
    ResetStreams = 8,
    SetStreamParameter = 9,
    CreateFullStream = 10,
    DeleteStreamByChannel = 11,
    SetStreamChannel = 12,
    ResumeStream = 13,
    SetStreamPlaybackPosition = 14,
    SetStreamPlaybackPosition2 = 15,
    PauseStream = 16,
    UpdateStreamEffect = 17,
    StopStreamPlayback = 18,
    SetStreamTransient = 19,
    UpdateStreamSource = 20,
    DestroyStreamObject = 21,
    Disconnect = 22,
    SetReadyFlag = 23,

    #endregion
}
