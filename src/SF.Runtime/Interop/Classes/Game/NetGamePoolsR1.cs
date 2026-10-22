using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Game;

// Packed R1 pool order, confirmed by CNetGame accessors in the R1 IDB.
[StructLayout(LayoutKind.Explicit, Size = 36, Pack = 1)]
internal unsafe struct NetGamePoolsR1
{
    [FieldOffset(0x00)] internal CActorPool* Actor;
    [FieldOffset(0x04)] internal CObjectPool* Object;
    [FieldOffset(0x08)] internal CGangZonePool* GangZone;
    [FieldOffset(0x0C)] internal CLabelPool* Label;
    [FieldOffset(0x10)] internal CTextDrawPool* TextDraw;
    [FieldOffset(0x14)] internal CMenuPool* Menu;
    [FieldOffset(0x18)] internal CPlayerPool* Player;
    [FieldOffset(0x1C)] internal CVehiclePool* Vehicle;
    [FieldOffset(0x20)] internal CPickupPool* Pickup;
}
