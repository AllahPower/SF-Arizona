namespace SFSharp.Runtime.Interop.RakNet.Arizona;

public readonly record struct IncomingArizonaPacketArgs(int EPacketId, int SubId, nint DataPtr, int PayloadBitOffset, int PayloadBitLength)
{
    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, PayloadBitOffset, PayloadBitLength);
    }
}

public readonly record struct OutgoingArizonaPacketArgs(int EPacketId, int SubId, nint DataPtr, int PayloadBitOffset, int PayloadBitLength)
{
    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, PayloadBitOffset, PayloadBitLength);
    }
}
