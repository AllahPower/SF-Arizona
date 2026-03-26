using System.Numerics;

namespace SFSharp;

// ---- AZVoice sub-packet payload types ----

public readonly record struct AzvPluginInitStream(ushort ChannelId, string Name, bool HasUrl);
public readonly record struct AzvPluginInit(uint Version, AzvPluginInitStream[] Streams);

public readonly record struct AzvCreateStaticAudioStream(byte ChannelKey, string Name);

public readonly record struct AzvDeleteStream(byte ChannelKey);

public readonly record struct AzvResetStreams;

public readonly record struct AzvSetStreamParameter(uint Value);

public enum EAzvStreamType : byte
{
    Global = 1,
    ChannelSound = 2,
    ChannelVoice = 3,
    Point3D = 4,
}

public readonly record struct AzvCreateFullStreamPlaybackEntry(
    bool HasInlineSource,
    string? SourceName,
    ushort? DirectSourceId,
    uint BaseTimestampOrZero,
    uint? DelaySeconds,
    float Volume,
    bool AllowOverlap,
    bool Looping,
    uint PlaybackPositionMs);

public enum EAzvCreateFullStreamActionType : byte
{
    EchoEffect = 1,
    BqfPeakingEqChain = 2,
}

/// <summary>
/// Trailing action block processed by ARZ::StopStreamPlaybackImpl.
/// Type 1 creates AudioEffectEcho and carries four floats.
/// Type 2 applies a built-in AudioEffectBQF + AudioEffectPeakingEq chain and has no extra payload.
/// </summary>
public readonly record struct AzvCreateFullStreamAction(
    EAzvCreateFullStreamActionType ActionType,
    Vector4? Float4Parameters);

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

public readonly record struct AzvDeleteStreamByChannel(ushort StreamId);

public readonly record struct AzvSetStreamChannel(ushort StreamId, ushort ChannelId);

public readonly record struct AzvResumeStream(ushort StreamId);

public readonly record struct AzvSetStreamPlaybackPosition(ushort StreamId, uint Timestamp);

public readonly record struct AzvSetStreamPlaybackPosition2(ushort StreamId, uint Timestamp);

public readonly record struct AzvPauseStream(ushort StreamId);

public readonly record struct AzvUpdateStreamEffect(
    ushort StreamId,
    bool HasInlineSource,
    string? SourceName,
    ushort? DirectSourceId,
    uint BaseTimestampOrZero,
    uint? DelaySeconds,
    float Volume,
    bool AllowOverlap);

public readonly record struct AzvStopStreamPlayback(ushort StreamId);

public readonly record struct AzvSetStreamTransient(ushort TargetId, bool IsTransient);

public readonly record struct AzvUpdateStreamSource(ushort ChannelId, string Name, bool HasUrl);

public readonly record struct AzvDestroyStreamObject(ushort StreamId);

public readonly record struct AzvDisconnect;

public readonly record struct AzvSetReadyFlag;

// ---- Voice data packets (raw packet 252, no sub-ID dispatch) ----

/// <summary>Incoming voice data (server -> client): carries decoded opus frames for one or more streams.</summary>
public readonly record struct AzvVoiceData(ushort SenderId, ushort PacketNumber, ushort[] StreamIds, byte[] OpusData);

/// <summary>Outgoing voice data (client -> server): single-stream opus frame from microphone capture.</summary>
public readonly record struct AzvOutgoingVoiceData(ushort PacketNumber, byte StreamId, byte[] OpusData);
