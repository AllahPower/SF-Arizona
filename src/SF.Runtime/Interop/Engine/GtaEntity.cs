using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CEntity</c>, the base of every world entity.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CEntity.Size, Pack = 1)]
public unsafe struct GtaEntity
{
    [FieldOffset(0)]
    public GtaPlaceable Placeable;

    [FieldOffset(GtaOffsets.CEntity.RwObject)]
    public nint RwObject;

    [FieldOffset(GtaOffsets.CEntity.Flags)]
    public uint Flags;

    [FieldOffset(GtaOffsets.CEntity.ModelIndex)]
    public short ModelIndex;

    [FieldOffset(GtaOffsets.CEntity.Interior)]
    public byte Interior;

    [FieldOffset(GtaOffsets.CEntity.TypeStatus)]
    public byte TypeStatus;

    public readonly GtaEntityType Type => (GtaEntityType)(TypeStatus & 7);

    /// <summary>Calls the virtual <c>Teleport(CVector, bool)</c>; peds and vehicles override it.</summary>
    public void Teleport(Vector3 position, bool resetRotation)
    {
        fixed (GtaEntity* self = &this)
        {
            var teleport = (delegate* unmanaged[Thiscall]<GtaEntity*, Vector3, byte, void>)Placeable.VTable[GtaOffsets.CEntity.TeleportVTableIndex];
            teleport(self, position, resetRotation ? (byte)1 : (byte)0);
        }
    }
}
