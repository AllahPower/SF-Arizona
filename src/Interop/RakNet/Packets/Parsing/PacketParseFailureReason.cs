namespace SFSharp;

public enum PacketParseFailureReason
{
    None = 0,
    Unsupported,
    TooShort,
    SizeMismatch,
    InvalidCast,
    Exception,
}
