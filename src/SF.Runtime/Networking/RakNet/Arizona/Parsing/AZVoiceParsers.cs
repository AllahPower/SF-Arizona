using System.Numerics;

namespace SFSharp.Runtime.Networking.RakNet.Arizona.Parsing;

public static class AZVoiceParsers
{
    private const int MaxVoicePayloadRoundedBits = 0x1007;

    private static void AlignToByte(ref SampBitStreamReader r)
    {
        int rem = r.OffsetBits % 8;
        if (rem != 0)
            r.SkipBits(8 - rem);
    }

    private static string ReadAzvString16(ref SampBitStreamReader r)
    {
        ushort length = r.ReadUInt16();
        return length == 0
            ? string.Empty
            : r.ReadFixedString(length);
    }

    private static float ReadNormFloat16(ref SampBitStreamReader r)
    {
        ushort raw = r.ReadUInt16();
        return raw / 32767.5f - 1.0f;
    }

    private static Vector3 ReadScaledVector3(ref SampBitStreamReader r)
    {
        float scale = r.ReadFloat();
        if (scale == 0.0f)
            return Vector3.Zero;
        float x = ReadNormFloat16(ref r) * scale;
        float y = ReadNormFloat16(ref r) * scale;
        float z = ReadNormFloat16(ref r) * scale;
        return new Vector3(x, y, z);
    }

    private static AzvCreateFullStreamPlaybackEntry ReadCreateFullStreamPlaybackEntry(ref SampBitStreamReader r)
    {
        bool hasInlineSource = r.ReadBitBool();
        string? sourceName = null;
        ushort? directSourceId = null;
        if (hasInlineSource)
        {
            sourceName = ReadAzvString16(ref r);
        }
        else
        {
            directSourceId = r.ReadUInt16();
        }

        uint baseTimestampOrZero = r.ReadUInt32();
        float volume = r.ReadFloat();
        bool allowOverlap = r.ReadBitBool();
        uint? delaySeconds = null;
        bool looping;
        uint playbackPositionMs;

        if (baseTimestampOrZero != 0)
        {
            looping = r.ReadBitBool();
            playbackPositionMs = r.ReadUInt32();
        }
        else
        {
            delaySeconds = r.ReadUInt32();
            looping = r.ReadBitBool();
            playbackPositionMs = r.ReadUInt32();
        }

        return new AzvCreateFullStreamPlaybackEntry(
            hasInlineSource,
            sourceName,
            directSourceId,
            baseTimestampOrZero,
            delaySeconds,
            volume,
            allowOverlap,
            looping,
            playbackPositionMs);
    }

    private static AzvCreateFullStreamAction ReadCreateFullStreamAction(ref SampBitStreamReader r)
    {
        var actionType = (EAzvCreateFullStreamActionType)r.ReadUInt8();
        Vector4? parameters = actionType switch
        {
            EAzvCreateFullStreamActionType.EchoEffect => new Vector4(
                r.ReadFloat(),
                r.ReadFloat(),
                r.ReadFloat(),
                r.ReadFloat()),
            EAzvCreateFullStreamActionType.BqfPeakingEqChain => null,
            _ => throw new InvalidOperationException($"Unknown AZVoice CreateFullStream action type: {(byte)actionType}"),
        };

        return new AzvCreateFullStreamAction(actionType, parameters);
    }

    #region incoming (server -> client)

    public static AzvPluginInit ParsePluginInit(ref SampBitStreamReader r)
    {
        uint version = r.ReadUInt32();
        ushort streamCount = r.ReadUInt16();
        var streams = new AzvPluginInitStream[streamCount];
        for (int i = 0; i < streamCount; i++)
        {
            ushort channelId = r.ReadUInt16();
            string name = ReadAzvString16(ref r);
            bool hasUrl = r.ReadBitBool();
            AlignToByte(ref r);
            streams[i] = new AzvPluginInitStream(channelId, name, hasUrl);
        }
        return new AzvPluginInit(version, streams);
    }

    public static AzvCreateStaticAudioStream ParseCreateStaticAudioStream(ref SampBitStreamReader r)
    {
        byte channelKey = r.ReadUInt8();
        string name = ReadAzvString16(ref r);
        return new AzvCreateStaticAudioStream(channelKey, name);
    }

    public static AzvDeleteStream ParseDeleteStream(ref SampBitStreamReader r)
    {
        return new AzvDeleteStream(r.ReadUInt8());
    }

    public static AzvResetStreams ParseResetStreams(ref SampBitStreamReader r)
    {
        return new AzvResetStreams();
    }

    public static AzvSetStreamParameter ParseSetStreamParameter(ref SampBitStreamReader r)
    {
        return new AzvSetStreamParameter(r.ReadUInt32());
    }

    public static AzvCreateFullStream ParseCreateFullStream(ref SampBitStreamReader r)
    {
        ushort streamId = r.ReadUInt16();
        string name = ReadAzvString16(ref r);
        uint sessionId = r.ReadUInt32();
        var streamType = (EAzvStreamType)r.ReadUInt8();

        ushort? channelId = null;
        float? distance = null;
        Vector3? position = null;
        Vector3? direction = null;
        Vector3? velocity = null;

        switch (streamType)
        {
            case EAzvStreamType.Global:
                break;
            case EAzvStreamType.ChannelSound:
            case EAzvStreamType.ChannelVoice:
                channelId = r.ReadUInt16();
                break;
            case EAzvStreamType.Point3D:
                distance = r.ReadFloat();
                position = ReadScaledVector3(ref r);
                direction = ReadScaledVector3(ref r);
                velocity = ReadScaledVector3(ref r);
                break;
        }

        ushort playbackEntryCount = r.ReadUInt16();
        var playbackEntries = new AzvCreateFullStreamPlaybackEntry[playbackEntryCount];
        for (int i = 0; i < playbackEntryCount; i++)
        {
            playbackEntries[i] = ReadCreateFullStreamPlaybackEntry(ref r);
        }

        ushort actionCount = r.ReadUInt16();
        var actions = new AzvCreateFullStreamAction[actionCount];
        for (int i = 0; i < actionCount; i++)
        {
            actions[i] = ReadCreateFullStreamAction(ref r);
        }

        return new AzvCreateFullStream(
            streamId,
            name,
            sessionId,
            streamType,
            channelId,
            distance,
            position,
            direction,
            velocity,
            playbackEntries,
            actions);
    }

    public static AzvDeleteStreamByChannel ParseDeleteStreamByChannel(ref SampBitStreamReader r)
    {
        return new AzvDeleteStreamByChannel(r.ReadUInt16());
    }

    public static AzvSetStreamChannel ParseSetStreamChannel(ref SampBitStreamReader r)
    {
        ushort streamId = r.ReadUInt16();
        ushort channelId = r.ReadUInt16();
        return new AzvSetStreamChannel(streamId, channelId);
    }

    public static AzvResumeStream ParseResumeStream(ref SampBitStreamReader r)
    {
        return new AzvResumeStream(r.ReadUInt16());
    }

    public static AzvSetStreamPlaybackPosition ParseSetStreamPlaybackPosition(ref SampBitStreamReader r)
    {
        ushort streamId = r.ReadUInt16();
        uint timestamp = r.ReadUInt32();
        return new AzvSetStreamPlaybackPosition(streamId, timestamp);
    }

    public static AzvSetStreamPlaybackPosition2 ParseSetStreamPlaybackPosition2(ref SampBitStreamReader r)
    {
        ushort streamId = r.ReadUInt16();
        uint timestamp = r.ReadUInt32();
        return new AzvSetStreamPlaybackPosition2(streamId, timestamp);
    }

    public static AzvPauseStream ParsePauseStream(ref SampBitStreamReader r)
    {
        return new AzvPauseStream(r.ReadUInt16());
    }

    public static AzvUpdateStreamEffect ParseUpdateStreamEffect(ref SampBitStreamReader r)
    {
        ushort streamId = r.ReadUInt16();
        bool hasInlineSource = r.ReadBitBool();
        string? sourceName = null;
        ushort? directSourceId = null;
        if (hasInlineSource)
        {
            sourceName = ReadAzvString16(ref r);
        }
        else
        {
            directSourceId = r.ReadUInt16();
        }

        uint baseTimestampOrZero = r.ReadUInt32();
        float volume = r.ReadFloat();
        bool allowOverlap = r.ReadBitBool();
        uint? delaySeconds = null;
        if (baseTimestampOrZero == 0)
        {
            delaySeconds = r.ReadUInt32();
        }

        return new AzvUpdateStreamEffect(
            streamId,
            hasInlineSource,
            sourceName,
            directSourceId,
            baseTimestampOrZero,
            delaySeconds,
            volume,
            allowOverlap);
    }

    public static AzvStopStreamPlayback ParseStopStreamPlayback(ref SampBitStreamReader r)
    {
        return new AzvStopStreamPlayback(r.ReadUInt16());
    }

    public static AzvSetStreamTransient ParseSetStreamTransient(ref SampBitStreamReader r)
    {
        ushort targetId = r.ReadUInt16();
        bool isTransient = r.ReadBitBool();
        return new AzvSetStreamTransient(targetId, isTransient);
    }

    public static AzvUpdateStreamSource ParseUpdateStreamSource(ref SampBitStreamReader r)
    {
        ushort channelId = r.ReadUInt16();
        string name = ReadAzvString16(ref r);
        bool hasUrl = r.ReadBitBool();
        return new AzvUpdateStreamSource(channelId, name, hasUrl);
    }

    public static AzvDestroyStreamObject ParseDestroyStreamObject(ref SampBitStreamReader r)
    {
        return new AzvDestroyStreamObject(r.ReadUInt16());
    }

    public static AzvDisconnect ParseDisconnect(ref SampBitStreamReader r)
    {
        return new AzvDisconnect();
    }

    public static AzvSetReadyFlag ParseSetReadyFlag(ref SampBitStreamReader r)
    {
        return new AzvSetReadyFlag();
    }

    #endregion

    #region multiplexed / raw AZVoice data packets

    public static AzvVoiceData ParseVoiceData(ref SampBitStreamReader r)
    {
        ushort senderId = r.ReadUInt16();
        ushort packetNumber = r.ReadUInt16();
        ushort streamCount = r.ReadUInt16();
        var streamIds = new ushort[streamCount];
        for (int i = 0; i < streamCount; i++)
            streamIds[i] = r.ReadUInt16();
        int roundedRemainingBits = r.RemainingBits + 7;
        if ((uint)roundedRemainingBits > MaxVoicePayloadRoundedBits)
            throw new InvalidOperationException($"AZVoice voice payload exceeds plugin limit: {roundedRemainingBits} bits");
        int opusBytes = roundedRemainingBits >> 3;
        if (opusBytes <= 2)
            opusBytes = 0;
        byte[] opusData = opusBytes > 0 ? r.ReadBytes(opusBytes).ToArray() : [];
        return new AzvVoiceData(senderId, packetNumber, streamIds, opusData);
    }

    public static AzvOutgoingVoiceData ParseOutgoingVoiceData(ref SampBitStreamReader r)
    {
        ushort packetNumber = r.ReadUInt16();
        byte streamId = r.ReadUInt8();
        byte[] opusData = r.RemainingBits > 0 ? r.ReadRemainingBytes() : [];
        return new AzvOutgoingVoiceData(packetNumber, streamId, opusData);
    }

    #endregion
}
