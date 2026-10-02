namespace SFSharp.Runtime.Interop.RakNet.Incoming;

public readonly record struct IncomingPacketArgs(int EPacketId, nint DataPtr, int DataBitLength)
{
    public int DataByteLength => (DataBitLength + 7) / 8;

    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, 0, DataBitLength);
    }
}
