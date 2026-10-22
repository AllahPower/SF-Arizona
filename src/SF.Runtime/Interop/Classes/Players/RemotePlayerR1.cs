using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Players;

// Only fields that differ from the R3 view. Common tail fields stay in CRemotePlayer.
[StructLayout(LayoutKind.Explicit, Size = 509, Pack = 1)]
internal struct RemotePlayerR1
{
    [FieldOffset(0x08)] internal byte team;
    [FieldOffset(0x09)] internal byte state;
    [FieldOffset(0x0A)] internal byte seatId;
    [FieldOffset(0x0F)] internal int passengerDriveBy;
    [FieldOffset(0x53)] internal Vector3 positionDifference;
    [FieldOffset(0x5F)] internal CRemotePlayerIncarTargetRotation incarTargetRotation;
    [FieldOffset(0x7B)] internal Vector3 onfootTargetPosition;
    [FieldOffset(0x87)] internal Vector3 onfootTargetSpeed;
    [FieldOffset(0x93)] internal Vector3 incarTargetPosition;
    [FieldOffset(0x9F)] internal Vector3 incarTargetSpeed;
    [FieldOffset(0xAB)] internal ushort id;
    [FieldOffset(0xAD)] internal ushort vehicleId;
    [FieldOffset(0xB3)] internal int drawLabels;
    [FieldOffset(0xB7)] internal int hasJetpack;
    [FieldOffset(0xBB)] internal byte specialAction;
    [FieldOffset(0xC8)] internal SampOnfootData onfootData;
    [FieldOffset(0x10C)] internal SampIncarData incarData;
    [FieldOffset(0x14B)] internal SampTrailerData trailerData;
    [FieldOffset(0x181)] internal SampPassengerData passengerData;
    [FieldOffset(0x199)] internal SampAimData aimData;
    [FieldOffset(0x1B8)] internal float reportedArmour;
    [FieldOffset(0x1BC)] internal float reportedHealth;
}
