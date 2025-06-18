using SFSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe GetLocalPlayerDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, CLocalPlayer*>;
using unsafe GetLocalPlayerNameDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, byte*>;
using unsafe GetNameDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, ushort, byte*>;
using unsafe GetScoreDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, ushort, int>;

[StructLayout(LayoutKind.Explicit, Size = 16126, Pack = 1)]
public unsafe struct CPlayerPool
{
    private static readonly CPlayerPool* _instance = CNetGame.Instance.GetPlayerPool();
    public static ref CPlayerPool Instance => ref *_instance;

    [FieldOffset(0x00)]
    public LocalPlayerInfo LocalPlayerInfo;

    private static readonly GetLocalPlayerDelegate _getLocalPlayer = (GetLocalPlayerDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x1A40);
    public CLocalPlayer* GetLocalPlayer()
    {
        return _getLocalPlayer(_instance);
    }

    private static readonly GetLocalPlayerNameDelegate _getLocalPlayerName = (GetLocalPlayerNameDelegate)HookHelper.GetFunctionPtr("samp.dll", 0xA4E0);
    public string? GetLocalPlayerName()
    {
        return AnsiString.Decode(_getLocalPlayerName(_instance));
    }

    private static readonly GetNameDelegate _getName = (GetNameDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x175C0);
    public string? GetName(ushort playerId)
    {
        return AnsiString.Decode(_getName(_instance, playerId));
    }

    private static readonly GetScoreDelegate _getScore = (GetScoreDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x6E850);
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