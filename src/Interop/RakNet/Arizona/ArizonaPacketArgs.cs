namespace SFSharp;

public readonly record struct IncomingArizonaPacketArgs(int PacketId, int SubId, nint DataPtr, int PayloadBitOffset, int PayloadBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, PayloadBitOffset, PayloadBitLength);
    }
}

public readonly record struct OutgoingArizonaPacketArgs(int PacketId, int SubId, nint DataPtr, int PayloadBitOffset, int PayloadBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, PayloadBitOffset, PayloadBitLength);
    }
}
