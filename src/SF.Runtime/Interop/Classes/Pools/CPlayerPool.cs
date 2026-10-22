using System.Runtime.InteropServices;

using unsafe FindDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, nint, ushort>;
using unsafe GetCountDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, int, int>;
using unsafe GetLocalPlayerDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*, SFSharp.Runtime.Interop.Classes.CLocalPlayer*>;
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
    [FieldOffset(0)]
    private readonly PlayerPoolR5 _r5;

    [FieldOffset(0)]
    private readonly PlayerPoolR1 _r1;
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

    public int LocalPlayerPing => SampEnvironment.IsR1 ? RequireInstance()->_r1.Ping : SampEnvironment.IsR5 ? RequireInstance()->_r5.Ping : RequireInstance()->_localPlayerPing;
    public int LocalPlayerScore => SampEnvironment.IsR1 ? RequireInstance()->_r1.Score : SampEnvironment.IsR5 ? RequireInstance()->_r5.Score : RequireInstance()->_localPlayerScore;
    public ushort LocalPlayerId => SampEnvironment.IsR1 ? RequireInstance()->_r1.Id : SampEnvironment.IsR5 ? RequireInstance()->_r5.Id : RequireInstance()->_localPlayerId;

    private static readonly IsConnectedDelegate _isConnected = (IsConnectedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.IsConnected);
    private static readonly GetPlayerDelegate _getPlayer = (GetPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetPlayer);
    private static readonly GetLocalPlayerDelegate _getLocalPlayer = (GetLocalPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetLocalPlayer);
    private static readonly GetCountDelegate _getCount = (GetCountDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.GetCount);
    private static readonly FindDelegate _find = (FindDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.Find);
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

    public ushort Find(nint gamePed)
    {
        return _find(RequireInstance(), gamePed);
    }

    public int GetCount(bool includeNpcPlayers = true)
    {
        return _getCount(RequireInstance(), includeNpcPlayers ? 1 : 0);
    }

    /// <summary>Resolves the local name through the same ID-aware getter used for remote players.</summary>
    /// <remarks>R1 RVA 0x13CD0 reads CPlayerInfo's string, not CPlayerPool's local string.
    /// GetName handles the packed local layout explicitly when the supplied ID is local.</remarks>
    public string? GetLocalPlayerName() => GetName(LocalPlayerId);

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
        return LocalPlayerScore;
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

[StructLayout(LayoutKind.Explicit, Size = 48, Pack = 1)]
public unsafe struct CPlayerInfo
{
    [FieldOffset(0)]
    private readonly PlayerInfoR5 _r5;

    [FieldOffset(0)]
    private readonly PlayerInfoR1 _r1;

    [FieldOffset(0x00)]
    private readonly CRemotePlayer* _player;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoPing)]
    private readonly int _ping;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoName)]
    private readonly StdString _name;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoScore)]
    private readonly int _score;

    [FieldOffset(SampOffsets.CPlayerPool.PlayerInfoNpcFlag)]
    private readonly int _npcFlag;

    public CRemotePlayer* Player => SampEnvironment.IsR5 ? _r5.Player : _player;
    public int Ping => SampEnvironment.IsR1 ? _r1.Ping : SampEnvironment.IsR5 ? _r5.Ping : _ping;
    public StdString Name => SampEnvironment.IsR5 ? _r5.Name : _name;
    public int Score => SampEnvironment.IsR5 ? _r5.Score : _score;
    public int NpcFlag => SampEnvironment.IsR1 ? _r1.NpcFlag : SampEnvironment.IsR5 ? _r5.NpcFlag : _npcFlag;
}

[StructLayout(LayoutKind.Explicit, Size = 24, Pack = 1)]
public unsafe struct StdString;
