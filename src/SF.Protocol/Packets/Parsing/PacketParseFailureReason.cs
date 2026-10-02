namespace SFSharp.Protocol.Packets.Parsing;

public enum PacketParseFailureReason
{
    None = 0,
    Unsupported,
    TooShort,
    SizeMismatch,
    InvalidCast,
    Exception,
}
