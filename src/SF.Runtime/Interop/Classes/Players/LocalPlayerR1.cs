using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Players;

// SAMP-API 6d4db99 and R1 IDB. This is a borrowed view, never a native allocation.
[StructLayout(LayoutKind.Explicit, Size = 804, Pack = 1)]
internal unsafe struct LocalPlayerR1
{
    [FieldOffset(0x14)] internal ushort currentVehicleId;
    [FieldOffset(0x16)] internal ushort lastVehicleId;
    [FieldOffset(0x18)] internal SampOnfootData onfootData;
    [FieldOffset(0x5C)] internal SampPassengerData passengerData;
    [FieldOffset(0x74)] internal SampTrailerData trailerData;
    [FieldOffset(0xAA)] internal SampIncarData incarData;
    [FieldOffset(0xE9)] internal SampAimData aimData;
    [FieldOffset(0x108)] internal CLocalPlayerSpawnInfo spawnInfo;
    [FieldOffset(0x136)] internal int hasSpawnInfo;
    [FieldOffset(0x14A)] internal byte team;
    [FieldOffset(0x161)] internal CLocalPlayerWeaponsData weaponsData;
    [FieldOffset(0x1A7)] internal int passengerDriveBy;
    [FieldOffset(0x1AB)] internal byte currentInterior;
    [FieldOffset(0x1AC)] internal int inRcMode;
    [FieldOffset(0x1B0)] internal CLocalPlayerCameraTarget cameraTarget;
    [FieldOffset(0x1B8)] internal CPlayerHeadState head;
    [FieldOffset(0x2FA)] internal CLocalPlayerClassSelectionState classSelection;
    [FieldOffset(0x30E)] internal CLocalPlayerSpectatingState spectating;
    [FieldOffset(0x318)] internal CLocalPlayerDamageState damage;
}
