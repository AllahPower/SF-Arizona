using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CPool&lt;T&gt;</c> header. A byte-map entry holds the slot generation; bit 0x80 marks a free slot.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CPool.Size, Pack = 1)]
public unsafe struct GtaPool
{
    [FieldOffset(GtaOffsets.CPool.Objects)]
    public byte* Objects;

    [FieldOffset(GtaOffsets.CPool.ByteMap)]
    public byte* ByteMap;

    [FieldOffset(GtaOffsets.CPool.Capacity)]
    public int Capacity;
}
