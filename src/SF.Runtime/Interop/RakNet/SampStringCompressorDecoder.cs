using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet;

/// <summary><see cref="ISampStringDecoder"/> over samp.dll's StringCompressor instance.</summary>
internal sealed unsafe class SampStringCompressorDecoder : ISampStringDecoder
{
    private readonly delegate* unmanaged[Stdcall]<nint> _getInstance = (delegate* unmanaged[Stdcall]<nint>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.SampStringCompressor.Instance);
    private readonly delegate* unmanaged[Thiscall]<nint, byte*, int, SampBitStream*, ushort, byte> _decodeString = (delegate* unmanaged[Thiscall]<nint, byte*, int, SampBitStream*, ushort, byte>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.SampStringCompressor.DecodeString);

    public bool TryDecode(byte* data, int bitLength, ref int bitOffset, Span<byte> output, out int length)
    {
        length = 0;
        nint instance = _getInstance();
        if (instance == 0)
        {
            return false;
        }

        SampBitStream bitStream = new()
        {
            NumberOfBitsAllocated = bitLength,
            NumberOfBitsUsed = bitLength,
            ReadOffset = bitOffset,
            Data = data,
        };

        fixed (byte* outputPtr = output)
        {
            if (_decodeString(instance, outputPtr, output.Length, &bitStream, 0) == 0)
            {
                return false;
            }
        }

        bitOffset = bitStream.ReadOffset;
        int terminator = output.IndexOf((byte)0);
        length = terminator < 0 ? output.Length : terminator;
        return true;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SampBitStream
    {
        public int NumberOfBitsAllocated;
        public int NumberOfBitsUsed;
        public int ReadOffset;
        public byte* Data;
    }
}
