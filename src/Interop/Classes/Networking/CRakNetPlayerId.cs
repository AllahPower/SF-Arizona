using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CRakNetPlayerId
{
    public uint BinaryAddress;
    public ushort Port;
}
