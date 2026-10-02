namespace SFSharp.Runtime.Interop.RakNet.Outgoing;

public readonly record struct OutgoingPacketArgs(int RakNetPacketId, nint DataPtr, int DataBitLength)
{
    public int DataByteLength => (DataBitLength + 7) / 8;

    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, 0, DataBitLength);
    }
}
