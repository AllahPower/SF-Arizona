using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CPed</c>. Pool slots are <c>CPlayerPed</c>-sized (0x7C4), so never index peds by this size.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CPed.Size, Pack = 1)]
public unsafe struct GtaPed
{
    [FieldOffset(0)]
    public GtaPhysical Physical;

    [FieldOffset(GtaOffsets.CPed.Flags)]
    public uint Flags;

    [FieldOffset(GtaOffsets.CPed.PedState)]
    public int PedState;

    [FieldOffset(GtaOffsets.CPed.Health)]
    public float Health;

    [FieldOffset(GtaOffsets.CPed.MaxHealth)]
    public float MaxHealth;

    [FieldOffset(GtaOffsets.CPed.Armour)]
    public float Armour;

    [FieldOffset(GtaOffsets.CPed.CurrentRotation)]
    public float CurrentRotation;

    [FieldOffset(GtaOffsets.CPed.AimingRotation)]
    public float AimingRotation;

    [FieldOffset(GtaOffsets.CPed.Vehicle)]
    public GtaVehicle* Vehicle;

    [FieldOffset(GtaOffsets.CPed.ActiveWeaponSlot)]
    public byte ActiveWeaponSlot;

    public readonly bool IsInVehicle => (Flags & GtaOffsets.CPed.InVehicleFlag) != 0 && Vehicle is not null;
}
