namespace SFSharp.Runtime.Interop.RakNet;

/// <summary>
/// Decodes a RakNet StringCompressor (Huffman) string from a bitstream. The host plugs in samp.dll's own
/// StringCompressor; <see cref="SampBitStreamReader"/> falls back to its managed decoder without one.
/// </summary>
public unsafe interface ISampStringDecoder
{
    /// <summary>
    /// Decodes the string starting at <paramref name="bitOffset"/> into <paramref name="output"/> and advances
    /// <paramref name="bitOffset"/> past it. <paramref name="length"/> is the byte count before the terminator.
    /// </summary>
    bool TryDecode(byte* data, int bitLength, ref int bitOffset, Span<byte> output, out int length);
}
