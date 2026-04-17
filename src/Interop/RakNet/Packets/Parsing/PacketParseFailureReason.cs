namespace SFSharp.Runtime.Networking;

public enum PacketParseFailureReason
{
    None = 0,
    Unsupported,
    TooShort,
    SizeMismatch,
    InvalidCast,
    Exception,
}
