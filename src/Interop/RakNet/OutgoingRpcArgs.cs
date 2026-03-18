namespace SFSharp;

public readonly record struct OutgoingRpcArgs(int RpcId, nint DataPtr, int DataBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, 0, DataBitLength);
    }
}
