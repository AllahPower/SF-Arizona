using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CRakNetBitStream
{
    public int NumberOfBitsAllocated;
    public int NumberOfBitsUsed;
    public int ReadOffset;
    public byte* Data;

    public readonly int ByteLength => (NumberOfBitsUsed + 7) / 8;
    public readonly bool IsEmpty => Data == null || NumberOfBitsUsed <= 0;

    public static CRakNetBitStream Create(byte* data, int bitLength)
    {
        return new CRakNetBitStream
        {
            NumberOfBitsAllocated = ((bitLength + 7) / 8) * 8,
            NumberOfBitsUsed = bitLength,
            ReadOffset = 0,
            Data = data
        };
    }

    public readonly ReadOnlySpan<byte> AsSpan()
    {
        return Data == null || ByteLength <= 0 ? ReadOnlySpan<byte>.Empty : new ReadOnlySpan<byte>(Data, ByteLength);
    }
}
