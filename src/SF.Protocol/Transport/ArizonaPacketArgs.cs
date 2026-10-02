namespace SFSharp.Protocol.Transport;

public readonly record struct IncomingArizonaPacketArgs(int RakNetPacketId, int SubId, nint DataPtr, int PayloadBitOffset, int PayloadBitLength)
{
    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, PayloadBitOffset, PayloadBitLength);
    }
}

public readonly record struct OutgoingArizonaPacketArgs(int RakNetPacketId, int SubId, nint DataPtr, int PayloadBitOffset, int PayloadBitLength)
{
    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, PayloadBitOffset, PayloadBitLength);
    }
}
