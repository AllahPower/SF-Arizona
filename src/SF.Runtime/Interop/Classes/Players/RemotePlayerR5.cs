using System.Runtime.InteropServices;
using System.Numerics;

namespace SFSharp.Runtime.Interop.Classes.Players;

// SA-MP R5-1 layout from SAMP-API 6d4db99 and the R5 IDB.
// Only fields that differ from the R3 view are listed here.
[StructLayout(LayoutKind.Explicit, Size = 509, Pack = 1)]
internal unsafe struct RemotePlayerR5
{
    [FieldOffset(0x1DD)]
    internal CPed* ped;

    [FieldOffset(0x1E1)]
    internal CVehicle* vehicle;

    [FieldOffset(0x1E5)]
    internal ushort id;

    [FieldOffset(0x1E7)]
    internal ushort vehicleId;

    [FieldOffset(0x4)]
    internal int drawLabels;

    [FieldOffset(0x8)]
    internal int hasJetpack;

    [FieldOffset(0xC)]
    internal byte specialAction;

    [FieldOffset(0x17C)]
    internal Vector3 onfootTargetPosition;

    [FieldOffset(0x188)]
    internal Vector3 onfootTargetSpeed;

    [FieldOffset(0x194)]
    internal Vector3 incarTargetPosition;

    [FieldOffset(0x1A0)]
    internal Vector3 incarTargetSpeed;

    [FieldOffset(0x160)]
    internal Vector3 positionDifference;

    [FieldOffset(0x16C)]
    internal CRemotePlayerIncarTargetRotation incarTargetRotation;

    [FieldOffset(0x1B4)]
    internal SampAnimation animation;

    [FieldOffset(0x1B8)]
    internal byte updateType;

    [FieldOffset(0x1B9)]
    internal uint lastUpdate;

    [FieldOffset(0x1BD)]
    internal uint lastTimestamp;

    [FieldOffset(0x1C1)]
    internal int performingCustomAnimationRaw;

    [FieldOffset(0x1C9)]
    internal CPlayerHeadState head;
}
