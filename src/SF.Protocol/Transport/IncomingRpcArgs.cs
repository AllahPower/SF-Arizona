namespace SFSharp.Protocol.Transport;

public readonly record struct IncomingRpcArgs(int SampRpcId, nint DataPtr, int DataBitOffset, int DataBitLength)
{
    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, DataBitOffset, DataBitLength);
    }
}
