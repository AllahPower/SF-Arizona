using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Game;

[StructLayout(LayoutKind.Explicit, Size = 36, Pack = 1)]
internal unsafe struct NetGamePoolsR5
{
    [FieldOffset(0x20)] internal CMenuPool* Menu;
    [FieldOffset(0x10)] internal CActorPool* Actor;
    [FieldOffset(0x4)] internal CPlayerPool* Player;
    [FieldOffset(0x0)] internal CVehiclePool* Vehicle;
    [FieldOffset(0x8)] internal CPickupPool* Pickup;
    [FieldOffset(0xC)] internal CObjectPool* Object;
    [FieldOffset(0x14)] internal CGangZonePool* GangZone;
    [FieldOffset(0x18)] internal CLabelPool* Label;
    [FieldOffset(0x1C)] internal CTextDrawPool* TextDraw;
}
