using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Networking;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CRakNetNetworkID
{
    public byte PeerToPeer;
    public CRakNetPlayerId PlayerId;
    public ushort LocalSystemId;
}
