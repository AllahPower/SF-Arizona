using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Pools;

// R5 uses 33-byte menu strings; every field after them moves by 27 bytes.
[StructLayout(LayoutKind.Explicit, Size = 0x3D3, Pack = 1)]
internal struct MenuR5
{
    [FieldOffset(0x37C)] internal float X;
    [FieldOffset(0x380)] internal float Y;
    [FieldOffset(0x384)] internal float FirstColumnWidth;
    [FieldOffset(0x388)] internal float SecondColumnWidth;
    [FieldOffset(0x38C)] internal byte Columns;
    [FieldOffset(0x38D)] internal CMenuInteraction Interaction;
    [FieldOffset(0x3CF)] internal int Panel;
}
