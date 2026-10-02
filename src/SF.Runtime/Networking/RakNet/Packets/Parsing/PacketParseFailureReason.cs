namespace SFSharp.Runtime.Networking.RakNet.Packets.Parsing;

public enum PacketParseFailureReason
{
    None = 0,
    Unsupported,
    TooShort,
    SizeMismatch,
    InvalidCast,
    Exception,
}
