using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Owned, inline copy of a packed SA-MP sync structure, without a packet ID or player ID prefix.</summary>
/// <remarks>This is native decoded data, not RakNet's compressed wire representation. Copies retain all bytes without heap arrays.</remarks>
public readonly struct SFSampSyncPayload
{
    [InlineArray(80)]
    private struct Buffer { private byte _first; }
    private readonly Buffer _bytes;
    public int Length { get; }

    public SFSampSyncPayload(ReadOnlySpan<byte> data)
    {
        if (data.Length > 80) throw new ArgumentOutOfRangeException(nameof(data));
        Buffer buffer = default;
        data.CopyTo(buffer);
        _bytes = buffer;
        Length = data.Length;
    }

    public byte ReadByte(int offset) => ((ReadOnlySpan<byte>)_bytes)[..Length][offset];
    public ushort ReadUInt16(int offset) => Read<ushort>(offset);
    public uint ReadUInt32(int offset) => Read<uint>(offset);
    public float ReadSingle(int offset) => Read<float>(offset);
    public Vector3 ReadVector3(int offset) => Read<Vector3>(offset);
    public void CopyTo(Span<byte> destination) => ((ReadOnlySpan<byte>)_bytes)[..Length].CopyTo(destination);
    public bool ContentEquals(SFSampSyncPayload other) => Length == other.Length
        && ((ReadOnlySpan<byte>)_bytes)[..Length].SequenceEqual(((ReadOnlySpan<byte>)other._bytes)[..other.Length]);
    private T Read<T>(int offset) where T : unmanaged => MemoryMarshal.Read<T>(((ReadOnlySpan<byte>)_bytes)[..Length][offset..]);
}
