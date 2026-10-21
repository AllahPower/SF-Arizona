using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Players;

[StructLayout(LayoutKind.Explicit, Size = 12094, Pack = 1)]
internal unsafe struct PlayerPoolR5
{
    [FieldOffset(0)] internal int Score;
    [FieldOffset(4)] internal ushort Id;
    [FieldOffset(0x22)] internal int Ping;
    [FieldOffset(0x26)] internal CLocalPlayer* Player;
}

[StructLayout(LayoutKind.Explicit, Size = 48, Pack = 1)]
internal unsafe struct PlayerInfoR5
{
    [FieldOffset(4)] internal int Score;
    [FieldOffset(8)] internal int NpcFlag;
    [FieldOffset(12)] internal int Ping;
    [FieldOffset(16)] internal CRemotePlayer* Player;
    [FieldOffset(24)] internal StdString Name;
}
