using System.Numerics;

namespace SFSharp;

// ---- AZVoice sub-packet payload types ----

public readonly record struct AzvPluginInitStream(ushort ChannelId, string Name, bool HasUrl);
public readonly record struct AzvPluginInit(uint Version, AzvPluginInitStream[] Streams);

public readonly record struct AzvCreateStaticAudioStream(byte StreamType, string Name);

public readonly record struct AzvDeleteStream(byte StreamId);

public readonly record struct AzvResetStreams;

public readonly record struct AzvSetStreamParameter(uint Parameter);

public enum EAzvStreamType : byte
{
    Global = 1,
    ChannelSound = 2,
    ChannelVoice = 3,
    Point3D = 4,
}

public readonly record struct AzvCreateFullStreamPlaybackEntry(
    bool HasInlineSource,
    string? InlineSourceName,
    ushort? DirectSourceId,
    uint BaseTimestamp,
    uint? DelaySeconds,
    float Volume,
    bool Flag0,
    bool Flag1,
    uint TimeValue);

public enum EAzvCreateFullStreamActionType : byte
{
    Unknown1 = 1,
    Unknown2 = 2,
}

/// <summary>
/// Trailing action block processed by ARZ::StopStreamPlaybackImpl.
/// Type 1 carries four floats, type 2 has no extra payload.
/// </summary>
public readonly record struct AzvCreateFullStreamAction(
    EAzvCreateFullStreamActionType ActionType,
    Vector4? Parameters);

public readonly record struct AzvCreateFullStream(
    ushort StreamId,
    string Name,
    uint SessionId,
    EAzvStreamType StreamType,
    ushort? ChannelId,
    float? Distance,
    Vector3? Position,
    Vector3? Direction,
    Vector3? Velocity,
    AzvCreateFullStreamPlaybackEntry[] PlaybackEntries,
    AzvCreateFullStreamAction[] Actions);

public readonly record struct AzvDeleteStreamByChannel(ushort ChannelId);

public readonly record struct AzvSetStreamChannel(ushort StreamId, ushort ChannelId);

public readonly record struct AzvResumeStream(ushort StreamId);

public readonly record struct AzvSetStreamPlaybackPosition(ushort StreamId, uint Timestamp);

public readonly record struct AzvSetStreamPlaybackPosition2(ushort StreamId, uint Timestamp);

public readonly record struct AzvPauseStream(ushort StreamId);

public readonly record struct AzvUpdateStreamEffect(ushort StreamId, byte[] Data);

public readonly record struct AzvStopStreamPlayback(ushort StreamId);

public readonly record struct AzvSetStreamTransient(ushort ChannelId, bool Flag);

public readonly record struct AzvUpdateStreamSource(ushort ChannelId, string Name, bool HasUrl);

public readonly record struct AzvDestroyStreamObject(ushort StreamId);

public readonly record struct AzvDisconnect;

public readonly record struct AzvSetReadyFlag;

// ---- Voice data packets (raw packet 252, no sub-ID dispatch) ----

/// <summary>Incoming voice data (server -> client): carries decoded opus frames for one or more streams.</summary>
public readonly record struct AzvVoiceData(ushort SenderId, ushort PacketNumber, ushort[] StreamIds, byte[] OpusData);

/// <summary>Outgoing voice data (client -> server): single-stream opus frame from microphone capture.</summary>
public readonly record struct AzvOutgoingVoiceData(ushort PacketNumber, byte StreamId, byte[] OpusData);
