using System.Runtime.InteropServices;

namespace SFSharp;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CRakNetNetworkID
{
    public byte PeerToPeer;
    public CRakNetPlayerId PlayerId;
    public ushort LocalSystemId;
}
