namespace SFSharp.Runtime.Network.RakNet.Arizona;

internal static class AZVoiceTransport
{
    public const int ControlPayloadBitOffset = 16;
    private const int MinimumVoiceFrameBits = 56;
    private const int MaximumRoundedPayloadBits = 0x1007;

    public enum IncomingKind
    {
        Control,
        VoiceData,
    }

    public readonly record struct IncomingPacketClassification(
        IncomingKind Kind,
        IncomingArizonaPacketArgs ControlArgs,
        AzvVoiceData VoiceData);

    public static bool TryClassifyIncomingPacket(IncomingPacketArgs args, out IncomingPacketClassification classification)
    {
        classification = default;

        if (TryParseIncomingVoiceData(args, out AzvVoiceData voiceData))
        {
            classification = new IncomingPacketClassification(IncomingKind.VoiceData, default, voiceData);
            return true;
        }

        if (TryCreateIncomingControlArgs(args, out IncomingArizonaPacketArgs controlArgs))
        {
            classification = new IncomingPacketClassification(IncomingKind.Control, controlArgs, default);
            return true;
        }

        return false;
    }

    public static bool TryCreateIncomingControlArgs(IncomingPacketArgs args, out IncomingArizonaPacketArgs packetArgs)
    {
        packetArgs = default;
        if (!TryReadIncomingControlId(args, out byte controlId))
        {
            return false;
        }

        packetArgs = new(args.EPacketId, controlId, args.DataPtr, ControlPayloadBitOffset, args.DataBitLength - ControlPayloadBitOffset);
        return true;
    }

    public static bool TryReadIncomingControlId(IncomingPacketArgs args, out byte controlId)
    {
        controlId = default;
        if (args.EPacketId != (int)EPacketId.AZVoice || args.DataBitLength < ControlPayloadBitOffset)
        {
            return false;
        }

        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);
            byte value = reader.ReadUInt8();
            if (!Enum.IsDefined(typeof(EAZVoice), value))
            {
                return false;
            }

            controlId = value;
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"Packet parse failed: parser={nameof(TryReadIncomingControlId)} bits={args.DataBitLength}: {ex.Message}");
            return false;
        }
    }

    public static bool TryParseIncomingVoiceData(IncomingPacketArgs args, out AzvVoiceData data)
    {
        data = default;
        if (args.EPacketId != (int)EPacketId.AZVoice || args.DataBitLength < MinimumVoiceFrameBits)
        {
            return false;
        }

        try
        {
            BitStreamReader reader = args.CreateReader();
            reader.SkipBytes(1);

            ushort senderId = reader.ReadUInt16();
            ushort packetNumber = reader.ReadUInt16();
            ushort streamCount = reader.ReadUInt16();
            int streamIdsBits = streamCount * 16;
            if (reader.RemainingBits < streamIdsBits)
            {
                return false;
            }

            ushort[] streamIds = new ushort[streamCount];
            for (int i = 0; i < streamCount; i++)
            {
                streamIds[i] = reader.ReadUInt16();
            }

            int roundedRemainingBits = reader.RemainingBits + 7;
            if ((uint)roundedRemainingBits > MaximumRoundedPayloadBits)
            {
                return false;
            }

            int opusBytes = roundedRemainingBits >> 3;
            if (opusBytes <= 2)
            {
                opusBytes = 0;
            }

            byte[] opusData = opusBytes > 0 ? reader.ReadBytes(opusBytes).ToArray() : [];
            data = new AzvVoiceData(senderId, packetNumber, streamIds, opusData);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"Packet parse failed: parser={nameof(TryParseIncomingVoiceData)} bits={args.DataBitLength}: {ex.Message}");
            return false;
        }
    }

}
