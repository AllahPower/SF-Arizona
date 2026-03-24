using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

internal static class AZVoiceTransport
{
    public const int ControlPayloadBitOffset = 16;
    private const int MinimumVoiceFrameBits = 56;
    private const int MaximumRoundedPayloadBits = 0x1007;

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
        catch
        {
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
        catch
        {
            return false;
        }
    }

}
