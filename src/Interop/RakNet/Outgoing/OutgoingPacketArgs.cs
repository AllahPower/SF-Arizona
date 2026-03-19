namespace SFSharp;

public readonly record struct OutgoingPacketArgs(int PacketId, nint DataPtr, int DataBitLength)
{
    public int DataByteLength => (DataBitLength + 7) / 8;

    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, 0, DataBitLength);
    }
}
