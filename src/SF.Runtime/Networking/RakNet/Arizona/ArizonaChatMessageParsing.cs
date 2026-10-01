using System.Text;

namespace SFSharp.Runtime.Network.RakNet.Arizona;

public abstract record ArzChatMessageSegment;
public sealed record ArzChatTextSegment(string Text, byte[] RawText) : ArzChatMessageSegment;
/// <summary>The receiver skips these 24 bits; the field layout matches the outbound slot writer.</summary>
public sealed record ArzChatSlotSegment(byte Slot, ushort ItemId) : ArzChatMessageSegment;
public sealed record ArzChatItemSegment(ushort ItemId, ArzChatItemField[] Fields) : ArzChatMessageSegment;
public readonly record struct ArzChatItemField(byte Id, uint? Value, string? Text);

public abstract record ArzLinkedChatSegment;
public sealed record ArzLinkedChatTextSegment(string Text) : ArzLinkedChatSegment;
public sealed record ArzLinkedChatSlotSegment(byte Slot, ushort ItemId) : ArzLinkedChatSegment;
public sealed record ArzLinkedChatItemSegment(ushort ItemId) : ArzLinkedChatSegment;
public sealed record ArzLinkedChatCommandSegment(string Command) : ArzLinkedChatSegment;
public readonly record struct ArzLinkedChatSend(byte Reserved, ArzLinkedChatSegment[] Segments);

internal static class ArizonaChatMessageParsing
{
    public static ArzChatMessageSegment[] ParseIncoming(ref BitStreamReader reader, Encoding encoding, out byte? unsupportedKind)
    {
        unsupportedKind = null;
        int count = reader.ReadUInt8();
        ArzChatMessageSegment[] segments = new ArzChatMessageSegment[count];
        for (int i = 0; i < count; i++)
        {
            byte kind = reader.ReadUInt8();
            if (kind > 2)
            {
                unsupportedKind = kind;
                return segments[..i];
            }
            segments[i] = kind switch
            {
                0 => ReadText(ref reader, encoding),
                1 => new ArzChatSlotSegment(reader.ReadUInt8(), reader.ReadUInt16()),
                _ => ReadItem(ref reader, encoding)
            };
        }
        return segments;
    }

    public static ArzLinkedChatSend ParseOutgoing(ref BitStreamReader reader, Encoding encoding)
    {
        byte reserved = reader.ReadUInt8();
        int count = reader.ReadUInt8();
        ArzLinkedChatSegment[] segments = new ArzLinkedChatSegment[count];
        for (int i = 0; i < count; i++)
        {
            byte kind = reader.ReadUInt8();
            segments[i] = kind switch
            {
                0 => new ArzLinkedChatTextSegment(ReadString16(ref reader, encoding)),
                1 => new ArzLinkedChatSlotSegment(reader.ReadUInt8(), reader.ReadUInt16()),
                2 => new ArzLinkedChatItemSegment(reader.ReadUInt16()),
                3 => new ArzLinkedChatCommandSegment(ReadString16(ref reader, encoding)),
                _ => throw new InvalidDataException($"Unknown outbound Arizona chat segment kind {kind}.")
            };
        }
        return new ArzLinkedChatSend(reserved, segments);
    }

    private static ArzChatTextSegment ReadText(ref BitStreamReader reader, Encoding encoding)
    {
        byte[] bytes = reader.ReadBytes(reader.ReadUInt16()).ToArray();
        return new ArzChatTextSegment(encoding.GetString(bytes), bytes);
    }

    private static ArzChatItemSegment ReadItem(ref BitStreamReader reader, Encoding encoding)
    {
        ushort itemId = reader.ReadUInt16();
        int count = reader.ReadUInt8();
        ArzChatItemField[] fields = new ArzChatItemField[count];
        for (int i = 0; i < count; i++)
        {
            byte id = reader.ReadUInt8();
            fields[i] = id == 14
                ? new ArzChatItemField(id, null, encoding.GetString(reader.ReadBytes(reader.ReadUInt8())))
                : new ArzChatItemField(id, reader.ReadUInt32(), null);
        }
        return new ArzChatItemSegment(itemId, fields);
    }

    private static string ReadString16(ref BitStreamReader reader, Encoding encoding) =>
        encoding.GetString(reader.ReadBytes(reader.ReadUInt16()));
}
