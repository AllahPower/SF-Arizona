namespace SFSharp.Runtime.Network.RakNet.Outgoing;

public readonly record struct OutgoingRpcArgs(int ERpcId, nint DataPtr, int DataBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, 0, DataBitLength);
    }
}
