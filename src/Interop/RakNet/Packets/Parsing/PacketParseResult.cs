using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public readonly record struct PacketParseResult(
    bool Success,
    IParsedPacket? Packet,
    string ParserName,
    PacketParseFailureReason FailureReason,
    string? Error = null)
{
    public static PacketParseResult Unsupported(EPacketId packetId)
    {
        return new PacketParseResult(false, null, packetId.ToString(), PacketParseFailureReason.Unsupported);
    }

    public static PacketParseResult TooShort(string parserName)
    {
        return new PacketParseResult(false, null, parserName, PacketParseFailureReason.TooShort);
    }

    public static PacketParseResult SizeMismatch(string parserName)
    {
        return new PacketParseResult(false, null, parserName, PacketParseFailureReason.SizeMismatch);
    }

    public static PacketParseResult InvalidCast(string parserName, Type expectedType)
    {
        return new PacketParseResult(false, null, parserName, PacketParseFailureReason.InvalidCast, expectedType.FullName);
    }

    public static PacketParseResult FromException(string parserName, Exception ex)
    {
        return new PacketParseResult(false, null, parserName, PacketParseFailureReason.Exception, ex.Message);
    }

    public bool TryGet<TPacket>(out TPacket packet) where TPacket : class, IParsedPacket
    {
        if (Packet is TPacket typed)
        {
            packet = typed;
            return true;
        }

        packet = null!;
        return false;
    }
}
