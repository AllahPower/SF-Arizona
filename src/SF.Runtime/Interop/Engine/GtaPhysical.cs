using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CPhysical</c>: an entity with velocity, mass and collision state.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CPhysical.Size, Pack = 1)]
public unsafe struct GtaPhysical
{
    [FieldOffset(0)]
    public GtaEntity Entity;

    [FieldOffset(GtaOffsets.CPhysical.MoveSpeed)]
    public Vector3 MoveSpeed;

    [FieldOffset(GtaOffsets.CPhysical.TurnSpeed)]
    public Vector3 TurnSpeed;

    [FieldOffset(GtaOffsets.CPhysical.Mass)]
    public float Mass;

    [FieldOffset(GtaOffsets.CPhysical.TurnMass)]
    public float TurnMass;

    [FieldOffset(GtaOffsets.CPhysical.AttachedTo)]
    public GtaPhysical* AttachedTo;
}
