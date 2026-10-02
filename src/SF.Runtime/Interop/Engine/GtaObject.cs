using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CObject</c>. Pool slots are 0x19C bytes to fit subclasses, so never index objects by this size.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CObject.Size, Pack = 1)]
public struct GtaObject
{
    [FieldOffset(0)]
    public GtaPhysical Physical;

    [FieldOffset(GtaOffsets.CObject.ObjectType)]
    public byte ObjectType;

    [FieldOffset(GtaOffsets.CObject.Health)]
    public float Health;
}
