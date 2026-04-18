using System;
using System.Runtime.InteropServices;

using unsafe GetCountDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, int, int>;
using unsafe GetLocalPlayerDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, SFSharp.Runtime.Interop.Classes.CLocalPlayer*>;
using unsafe GetLocalPlayerNameDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, byte*>;
using unsafe GetLocalPlayerPingDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, int>;
using unsafe GetNameDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, ushort, byte*>;
using unsafe GetPingDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, ushort, int>;
using unsafe GetPlayerDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, ushort, SFSharp.Runtime.Interop.Classes.Players.CRemotePlayer*>;
using unsafe GetScoreDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, ushort, int>;
using unsafe IsConnectedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, ushort, int>;

namespace SFSharp.Runtime.Interop.Classes.Pools;

[StructLayout(LayoutKind.Explicit, Size = 12094, Pack = 1)]
public unsafe ref struct CPlayerPool
{
    private static CPlayerPool* CurrentInstance => CNetGame.Instance.GetPlayerPool();
    public static ref readonly CPlayerPool Instance => ref *RequireInstance();

    [FieldOffset(SampOffsets.CPlayerPool.LocalPlayerPing)]
    private readonly int _localPlayerPing;

    [FieldOffset(SampOffsets.CPlayerPool.LocalPlayerScore)]
    private readonly int _localPlayerScore;

    [FieldOffset(SampOffsets.CPlayerPool.LocalPlayerId)]
    private readonly ushort _localPlayerId;

    [FieldOffset(SampOffsets.CPlayerPool.LocalPlayerPointer)]
    private readonly CLocalPlayer* _localPlayer;

    public int LocalPlayerPing => RequireInstance()->_localPlayerPing;
    public int LocalPlayerScore => RequireInstance()->_localPlayerScore;
    public ushort LocalPlayerId => RequireInstance()->_localPlayerId;

    private static readonly IsConnectedDelegate _isConnected = (IsConnectedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.IsConnected);
    private static readonly GetPlayerDelegate _getPlayer = (GetPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetPlayer);
    private static readonly GetLocalPlayerDelegate _getLocalPlayer = (GetLocalPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetLocalPlayer);
    private static readonly GetCountDelegate _getCount = (GetCountDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetCount);
    private static readonly GetLocalPlayerNameDelegate _getLocalPlayerName = (GetLocalPlayerNameDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetLocalPlayerName);
    private static readonly GetNameDelegate _getName = (GetNameDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetName);
    private static readonly GetScoreDelegate _getScore = (GetScoreDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetScore);
    private static readonly GetPingDelegate _getPing = (GetPingDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetPing);
    private static readonly GetLocalPlayerPingDelegate _getLocalPlayerPing = (GetLocalPlayerPingDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetLocalPlayerPing);

    public bool IsValidPlayerId(ushort playerId)
    {
        return playerId < SampOffsets.CPlayerPool.MaxPlayers;
    }

    public bool IsConnected(ushort playerId)
    {
        CPlayerPool* instance = RequireInstance();
        return IsValidPlayerId(playerId) && _isConnected(instance, playerId) != 0;
    }

    public CRemotePlayer* GetPlayer(ushort playerId)
    {
        CPlayerPool* instance = RequireInstance();
        return _getPlayer(instance, playerId);
    }

    public bool TryGetPlayer(ushort playerId, out CRemotePlayer* player)
    {
        CPlayerPool* instance = RequireInstance();
        player = null;
        if (!IsValidPlayerId(playerId) || _isConnected(instance, playerId) == 0)
        {
            return false;
        }

        player = _getPlayer(instance, playerId);
        return player != null;
    }

    public bool TryGetConnectedPlayer(ushort playerId, out CRemotePlayer* player)
    {
        return TryGetPlayer(playerId, out player);
    }

    public CPlayerInfo* GetPlayerInfo(ushort playerId)
    {
        CPlayerPool* instance = RequireInstance();
        if (!IsValidPlayerId(playerId))
        {
            return null;
        }

        return *(CPlayerInfo**)((byte*)instance + SampOffsets.CPlayerPool.RemotePlayerInfoArray + (playerId * sizeof(uint)));
    }

    public bool TryGetPlayerInfo(ushort playerId, out CPlayerInfo* playerInfo)
    {
        playerInfo = GetPlayerInfo(playerId);
        return playerInfo != null;
    }

    public CLocalPlayer* GetLocalPlayer()
    {
        return _getLocalPlayer(RequireInstance());
    }

    public bool TryGetLocalPlayer(out CLocalPlayer* localPlayer)
    {
        localPlayer = GetLocalPlayer();
        return localPlayer != null;
    }

    public int GetCount(bool includeNpcPlayers = true)
    {
        return _getCount(RequireInstance(), includeNpcPlayers ? 1 : 0);
    }

    public string? GetLocalPlayerName()
    {
        return AnsiString.Decode(_getLocalPlayerName(RequireInstance()));
    }

    public string? GetName(ushort playerId)
    {
        return AnsiString.Decode(_getName(RequireInstance(), playerId));
    }

    public int GetScore(ushort playerId)
    {
        return _getScore(RequireInstance(), playerId);
    }

    public int GetPing(ushort playerId)
    {
        return _getPing(RequireInstance(), playerId);
    }

    public int GetLocalPlayerPing()
    {
        return _getLocalPlayerPing(RequireInstance());
    }

    public int GetLocalPlayerScore()
    {
        return RequireInstance()->_localPlayerScore;
    }

    public bool IsNpc(ushort playerId)
    {
        CPlayerInfo* playerInfo = GetPlayerInfo(playerId);
        return playerInfo != null && playerInfo->NpcFlag != 0;
    }

    private static CPlayerPool* RequireInstance()
    {
        CPlayerPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CPlayerPool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 44, Pack = 1)]
public unsafe struct CPlayerInfo
{
    [FieldOffset(0x00)]
    public CRemotePlayer* Player;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoPing)]
    public int Ping;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoName)]
    public StdString Name;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoScore)]
    public int Score;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoNpcFlag)]
    public int NpcFlag;
}

[StructLayout(LayoutKind.Explicit, Size = 24, Pack = 1)]
public unsafe struct StdString;
