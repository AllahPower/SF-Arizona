using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Players;

// SA-MP R5-1 layout from SAMP-API 6d4db99 and the R5 IDB.
// Only fields that differ from the R3 view are listed here.
[StructLayout(LayoutKind.Explicit, Size = 804, Pack = 1)]
internal unsafe struct LocalPlayerR5
{
    [FieldOffset(0x104)]
    internal CPed* ped;

    [FieldOffset(0x0)]
    internal SampIncarData incarData;

    [FieldOffset(0x3F)]
    internal SampAimData aimData;

    [FieldOffset(0x5E)]
    internal SampTrailerData trailerData;

    [FieldOffset(0x94)]
    internal SampOnfootData onfootData;

    [FieldOffset(0xD8)]
    internal SampPassengerData passengerData;

    [FieldOffset(0xF8)]
    internal ushort currentVehicleId;

    [FieldOffset(0xFA)]
    internal ushort lastVehicleId;

    [FieldOffset(0x10C)]
    internal byte team;

    [FieldOffset(0x12B)]
    internal CPlayerHeadState head;

    [FieldOffset(0x30E)]
    internal CLocalPlayerSpectatingState spectating;

    [FieldOffset(0x318)]
    internal CLocalPlayerDamageState damage;
}
