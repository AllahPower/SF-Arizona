using SFSharp;
using System.Runtime.InteropServices;

using unsafe GetPlayerPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CPlayerPool*>;
using unsafe UpdatePlayersDelegate = delegate* unmanaged[Thiscall]<CNetGame*, void>;

[StructLayout(LayoutKind.Explicit, Size = 1006, Pack = 1)]
public unsafe ref struct CNetGame
{
    private static readonly CNetGame* _instance = *(CNetGame**)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.Instance);
    public static ref readonly CNetGame Instance => ref *_instance;

    [FieldOffset(SampOffsets.CNetGame.RakClient)]
    private readonly nint _rakClient;

    public static nint GetRakClientPtr()
    {
        return GetRakClientPtr((nint)_instance);
    }

    public static nint GetRakClientPtr(nint cnetGamePtr)
    {
        if (cnetGamePtr == 0)
        {
            return 0;
        }

        return *(nint*)(cnetGamePtr + SampOffsets.CNetGame.RakClient);
    }

    private static readonly GetPlayerPoolDelegate _getPlayerPool = (GetPlayerPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetPlayerPool);
    public CPlayerPool* GetPlayerPool()
    {
        return _getPlayerPool(_instance);
    }

    private static readonly UpdatePlayersDelegate _updatePlayers = (UpdatePlayersDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.UpdatePlayers);
    public void UpdatePlayers()
    {
        _updatePlayers(_instance);
    }
}
