using System.Numerics;
using System.Text;

namespace SFSharp;

public static partial class ArizonaPacket
{
    // ---- Internal Arizona runtime custom packet helpers/parsers ----

    private static string ReadPacketNumericString(ref BitStreamReader reader)
    {
        uint value = reader.ReadUInt32();
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string ReadPacketString(ref BitStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        byte encodedFlag = reader.ReadUInt8();

        if (length == 1)
        {
            return reader.ReadUInt32().ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (encodedFlag != 0)
        {
            return reader.ReadEncodedString(length + encodedFlag);
        }

        if (length <= 1)
        {
            return string.Empty;
        }

        return reader.ReadFixedString(length);
    }

    private static string ReadPacketMaybeEncodedString(ref BitStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        byte encodedFlag = reader.ReadUInt8();

        if (length <= 1)
        {
            return string.Empty;
        }

        if (encodedFlag != 0)
        {
            return reader.ReadEncodedString(length + encodedFlag);
        }

        return reader.ReadFixedString(length);
    }

    public static ArzSimpleCreate ParseSimpleCreate(ref BitStreamReader reader)
    {
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        uint x = reader.ReadUInt32();
        uint y = reader.ReadUInt32();
        string primaryText = ReadPacketMaybeEncodedString(ref reader);
        string secondaryText = ReadPacketString(ref reader);
        uint? extraInt = reader.RemainingBits >= 32 ? reader.ReadUInt32() : null;
        float? extraFloat = reader.RemainingBits >= 32 ? reader.ReadFloat() : null;
        return new(width, height, x, y, primaryText, secondaryText, extraInt, extraFloat);
    }

    public static ArzCreateScaled ParseCreateScaled(ref BitStreamReader reader)
    {
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        uint x = reader.ReadUInt32();
        uint y = reader.ReadUInt32();
        string primaryText = ReadPacketMaybeEncodedString(ref reader);
        string secondaryText = ReadPacketString(ref reader);
        float scale = reader.ReadFloat();
        uint? extraInt = reader.RemainingBits >= 32 ? reader.ReadUInt32() : null;
        float? extraFloat = reader.RemainingBits >= 32 ? reader.ReadFloat() : null;
        return new(width, height, x, y, primaryText, secondaryText, scale, extraInt, extraFloat);
    }

    public static ArzObjectCreate ParseObjectCreate(ref BitStreamReader reader)
    {
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        uint x = reader.ReadUInt32();
        uint y = reader.ReadUInt32();
        string primaryText = ReadPacketMaybeEncodedString(ref reader);
        string secondaryText = ReadPacketString(ref reader);
        ushort short0 = reader.ReadUInt16();
        ushort short1 = reader.ReadUInt16();
        float floatValue = reader.ReadFloat();
        uint? extraInt = reader.RemainingBits >= 32 ? reader.ReadUInt32() : null;
        float? extraFloat = reader.RemainingBits >= 32 ? reader.ReadFloat() : null;
        return new(width, height, x, y, primaryText, secondaryText, short0, short1, floatValue, extraInt, extraFloat);
    }

    public static ArzInsideObjectCreate ParseInsideObjectCreate(ref BitStreamReader reader)
    {
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        uint x = reader.ReadUInt32();
        uint y = reader.ReadUInt32();
        string primaryText = ReadPacketMaybeEncodedString(ref reader);
        string secondaryText = ReadPacketString(ref reader);
        ushort short0 = reader.ReadUInt16();
        ushort short1 = reader.ReadUInt16();
        float floatValue = reader.ReadFloat();
        uint value4 = reader.ReadUInt32();
        uint value5 = reader.ReadUInt32();
        uint? extraInt = reader.RemainingBits >= 32 ? reader.ReadUInt32() : null;
        float? extraFloat = reader.RemainingBits >= 32 ? reader.ReadFloat() : null;
        return new(width, height, x, y, primaryText, secondaryText, short0, short1, floatValue, value4, value5, extraInt, extraFloat);
    }

    public static ArzClose ParseClose(ref BitStreamReader reader)
    {
        return new(ReadPacketNumericString(ref reader));
    }

    public static ArzMove ParseMove(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(browserId, value0, value1);
    }

    public static ArzChangeUrl ParseChangeUrl(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        string url = ReadPacketMaybeEncodedString(ref reader);
        return new(browserId, url);
    }

    public static ArzInjectCode ParseInjectCode(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        string code = ReadPacketMaybeEncodedString(ref reader);
        uint requestId = reader.ReadUInt32();
        return new(browserId, code, requestId);
    }

    public static ArzSendMessage ParseSendMessage(ref BitStreamReader reader)
    {
        string text = ReadPacketMaybeEncodedString(ref reader);
        uint value = reader.ReadUInt32();
        return new(text, value);
    }

    public static ArzToggleScreen ParseToggleScreen(ref BitStreamReader reader)
    {
        return new(ReadPacketNumericString(ref reader));
    }

    public static ArzRequestClientViewport ParseRequestClientViewport(ref BitStreamReader reader)
    {
        return new();
    }

    public static ArzStatePair ParseStatePair(ref BitStreamReader reader)
    {
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(value0, value1);
    }

    public static ArzModuleReadRequest ParseModuleReadRequest(ref BitStreamReader reader)
    {
        uint moduleOffset = reader.ReadUInt32();
        byte moduleNameLength = reader.ReadUInt8();
        string moduleName = moduleNameLength > 0 ? reader.ReadFixedString(moduleNameLength) : string.Empty;
        uint size = reader.ReadUInt32();
        return new(moduleOffset, moduleName, size);
    }

    public static ArzToggleShow ParseToggleShow(ref BitStreamReader reader)
    {
        return new(ReadPacketNumericString(ref reader));
    }

    public static ArzBrowserClick ParseBrowserClick(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        byte value2 = reader.ReadUInt8();
        return new(browserId, value0, value1, value2);
    }

    public static ArzGetBrowserControlState ParseGetBrowserControlState(ref BitStreamReader reader)
    {
        return new(ReadPacketNumericString(ref reader));
    }

    public static ArzSetBrowserControlState ParseSetBrowserControlState(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        bool state = reader.ReadBitBool();
        return new(browserId, state);
    }

    public static ArzResize ParseResize(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        return new(browserId, width, height);
    }

    public static ArzAddObject ParseAddObject(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(browserId, value0, value1);
    }

    public static ArzRemoveObject ParseRemoveObject(ref BitStreamReader reader)
    {
        string browserId = ReadPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(browserId, value0, value1);
    }

    public static ArzSetPlayerAnimGroups ParseSetPlayerAnimGroups(ref BitStreamReader r)
    {
        ushort playerId = r.ReadUInt16();
        List<ArzPlayerAnimGroupBatch> batches = [];

        while (r.RemainingBits >= 8)
        {
            byte opcode = r.ReadUInt8();
            if (opcode != 0)
            {
                batches.Add(new(opcode, []));
                continue;
            }

            if (r.RemainingBits < 8)
            {
                batches.Add(new(opcode, []));
                break;
            }

            byte count = r.ReadUInt8();
            ArzPlayerAnimGroupEntry[] entries = new ArzPlayerAnimGroupEntry[count];
            for (int i = 0; i < count; i++)
            {
                ushort groupNameLength = r.ReadUInt16();
                string groupName = groupNameLength > 0 ? r.ReadFixedString(groupNameLength) : string.Empty;
                uint packedValue = r.ReadUInt32();
                ushort animationNameLength = r.ReadUInt16();
                string animationName = animationNameLength > 0 ? r.ReadFixedString(animationNameLength) : string.Empty;
                byte selector = r.ReadUInt8();
                entries[i] = new(groupName, packedValue, animationName, selector);
            }

            batches.Add(new(opcode, entries));
        }

        return new(playerId, [.. batches]);
    }

    public static ArzScaleRadarMapIcon ParseBlipIconRaw(ref BitStreamReader reader)
    {
        byte radarIconId = reader.ReadUInt8();
        float scaleX = reader.RemainingBits >= 32 ? reader.ReadFloat() : 1.0f;
        float scaleY = reader.RemainingBits >= 32 ? reader.ReadFloat() : 1.0f;
        return new(radarIconId, scaleX, scaleY);
    }

    public static ArzGangZonePoly ParseMarkerIconBatchRaw(ref BitStreamReader reader)
    {
        byte zoneId = reader.ReadUInt8();
        uint pointWordCount = reader.ReadUInt32();
        uint[] packedPolygonPoints = new uint[pointWordCount];
        for (int i = 0; i < packedPolygonPoints.Length; i++)
        {
            packedPolygonPoints[i] = reader.ReadUInt32();
        }

        byte colorR = reader.ReadUInt8();
        byte colorG = reader.ReadUInt8();
        byte colorB = reader.ReadUInt8();
        byte colorA = reader.ReadUInt8();
        byte style = reader.ReadUInt8();
        bool enabled = reader.ReadBitBool();
        return new(zoneId, packedPolygonPoints, colorR, colorG, colorB, colorA, style, enabled);
    }

    // ============================================================================
    // Dispatcher helpers.
    // Call these from packet hooks to extract sub-id and create a reader
    // positioned at the payload start.
    //
    // Usage from IncomingPacketArgs:
    //   var reader = args.CreateReader();
    //   reader.SkipBytes(1); // skip packet id byte (220/221)
    //   byte subId = reader.ReadUInt8(); // or ReadUInt16 for 221
    //   // now call the appropriate Parse* method with ref reader
    // ============================================================================

    // extract sub-id from a Packet 220 bitstream (after skipping the packet id byte)
    public static byte ReadSubId220(ref BitStreamReader reader)
    {
        return reader.ReadUInt8();
    }

    // extract sub-id from a Packet 221 bitstream (after skipping the packet id byte)
    public static ushort ReadSubId221(ref BitStreamReader reader)
    {
        return reader.ReadUInt16();
    }
}

