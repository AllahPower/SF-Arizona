using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CRakNetPacket
{
    public ushort PlayerIndex;
    public CRakNetPlayerId PlayerId;
    public int Length;
    public int BitSize;
    public byte* Data;
    public byte DeleteData;

    public readonly byte PacketId => Data == null || BitSize < 8 ? (byte)0 : Data[0];
    public readonly int ByteLength => (BitSize + 7) / 8;
    public readonly bool OwnsData => DeleteData != 0;

    public readonly ReadOnlySpan<byte> AsSpan()
    {
        return Data == null || ByteLength <= 0 ? ReadOnlySpan<byte>.Empty : new ReadOnlySpan<byte>(Data, ByteLength);
    }
}
