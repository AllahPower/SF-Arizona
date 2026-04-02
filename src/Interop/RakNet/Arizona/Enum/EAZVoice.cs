namespace SFSharp;

/// <summary>
/// AZVoice plugin sub-RPC IDs dispatched within RPC 252.
/// The first byte of the RPC payload is the sub-RPC ID,
/// followed by a sub-RPC-specific bitstream payload.
/// </summary>
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
