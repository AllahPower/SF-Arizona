using SFSharp;
using System.Runtime.InteropServices;

using unsafe GetPlayerPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CPlayerPool*>;
using unsafe UpdatePlayersDelegate = delegate* unmanaged[Thiscall]<CNetGame*, void>;

[StructLayout(LayoutKind.Explicit, Size = 1006, Pack = 1)]
public unsafe ref struct CNetGame
{
    private static readonly CNetGame* _instance = *(CNetGame**)ModuleResolver.GetProcAddress("samp.dll", 0x26E8DC);
    public static ref readonly CNetGame Instance => ref *_instance;

    private static readonly GetPlayerPoolDelegate _getPlayerPool = (GetPlayerPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", 0x1160);
    public CPlayerPool* GetPlayerPool()
    {
        return _getPlayerPool(_instance);
    }

    private static readonly UpdatePlayersDelegate _updatePlayers = (UpdatePlayersDelegate)ModuleResolver.GetProcAddress("samp.dll", 0x8BA0);
    public void UpdatePlayers()
    {
        _updatePlayers(_instance);
    }
}

