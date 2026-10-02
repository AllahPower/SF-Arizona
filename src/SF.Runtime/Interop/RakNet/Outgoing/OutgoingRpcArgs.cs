namespace SFSharp.Runtime.Interop.RakNet.Outgoing;

public readonly record struct OutgoingRpcArgs(int SampRpcId, nint DataPtr, int DataBitLength)
{
    public unsafe SampBitStreamReader CreateReader()
    {
        return new SampBitStreamReader((byte*)DataPtr, 0, DataBitLength);
    }
}
