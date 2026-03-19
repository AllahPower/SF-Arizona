using SFSharp;
using System.Runtime.InteropServices;

using unsafe GetLocalPlayerDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, CLocalPlayer*>;
using unsafe GetLocalPlayerNameDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, byte*>;
using unsafe GetNameDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, ushort, byte*>;
using unsafe GetScoreDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, ushort, int>;

[StructLayout(LayoutKind.Explicit, Size = 16126, Pack = 1)]
public unsafe ref struct CPlayerPool
{
    private static readonly CPlayerPool* _instance = CNetGame.Instance.GetPlayerPool();
    public static ref readonly CPlayerPool Instance => ref *_instance;

    [FieldOffset(0x00)]
    public LocalPlayerInfo LocalPlayerInfo;

    private static readonly GetLocalPlayerDelegate _getLocalPlayer = (GetLocalPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetLocalPlayer);
    public CLocalPlayer* GetLocalPlayer()
    {
        return _getLocalPlayer(_instance);
    }

    private static readonly GetLocalPlayerNameDelegate _getLocalPlayerName = (GetLocalPlayerNameDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetLocalPlayerName);
    public string? GetLocalPlayerName()
    {
        return AnsiString.Decode(_getLocalPlayerName(_instance));
    }

    private static readonly GetNameDelegate _getName = (GetNameDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetName);
    public string? GetName(ushort playerId)
    {
        return AnsiString.Decode(_getName(_instance, playerId));
    }

    private static readonly GetScoreDelegate _getScore = (GetScoreDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetScore);
    public int GetScore(ushort playerId)
    {
        return _getScore(_instance, playerId);
    }
}

public unsafe struct LocalPlayerInfo
{
    public int Score;
    public ushort Id;
    public StdString Name;
    public int Ping;
    public CLocalPlayer* LocalPlayer;
}

[StructLayout(LayoutKind.Explicit, Size = 40, Pack = 1)]
public unsafe struct StdString;
