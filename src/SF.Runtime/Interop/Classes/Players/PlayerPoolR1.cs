using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Players;

[StructLayout(LayoutKind.Explicit, Size = 12094, Pack = 1)]
internal struct PlayerPoolR1
{
    [FieldOffset(0x04)] internal ushort Id;
    [FieldOffset(0x26)] internal int Ping;
    [FieldOffset(0x2A)] internal int Score;
}

[StructLayout(LayoutKind.Explicit, Size = 44, Pack = 1)]
internal struct PlayerInfoR1
{
    [FieldOffset(0x04)] internal int NpcFlag;
    [FieldOffset(0x28)] internal int Ping;
}
