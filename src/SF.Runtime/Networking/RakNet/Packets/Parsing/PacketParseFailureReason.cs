namespace SFSharp.Runtime.Network.RakNet.Packets;

public enum PacketParseFailureReason
{
    None = 0,
    Unsupported,
    TooShort,
    SizeMismatch,
    InvalidCast,
    Exception,
}
