namespace SFSharp.Runtime.Game.Players;

public partial class SFPlayers : ISFPlayers
{
    private readonly ISFMainThread _mainThread;

    private ref readonly CPlayerPool Pool
    {
        get
        {
            RequireThread();
            return ref CPlayerPool.Instance;
        }
    }

    private void RequireThread()
    {
        if (!_mainThread.IsMainThread) throw new InvalidOperationException("SA-MP player queries require the game thread.");
    }

    public SFPlayers(ISFMainThread mainThread)
    {
        _mainThread = mainThread;
        Local = new SFLocalPlayer(this);
    }

    public SFLocalPlayer Local { get; }

    public ushort LocalPlayerId => Pool.LocalPlayerId;
    public string? LocalPlayerName => Pool.GetLocalPlayerName();
    public int LocalPlayerScore => Pool.GetLocalPlayerScore();
    public int LocalPlayerPing => Pool.GetLocalPlayerPing();

    public string? GetName(ushort playerId) => Pool.GetName(playerId);
    public int GetScore(ushort playerId) => Pool.GetScore(playerId);
    public int GetPing(ushort playerId) => Pool.GetPing(playerId);
    public bool IsConnected(ushort playerId) => Pool.IsConnected(playerId);
    public bool IsNpc(ushort playerId) => Pool.IsNpc(playerId);
    public int GetConnectedCount(bool includeNpcPlayers = true) => Pool.GetCount(includeNpcPlayers);

    public bool TryGetName(ushort playerId, out string? name)
    {
        if (!IsConnected(playerId))
        {
            name = null;
            return false;
        }

        name = GetName(playerId);
        return !string.IsNullOrWhiteSpace(name);
    }

    public bool TryGetScore(ushort playerId, out int score)
    {
        if (!IsConnected(playerId))
        {
            score = 0;
            return false;
        }

        score = GetScore(playerId);
        return true;
    }

    public bool TryGetPing(ushort playerId, out int ping)
    {
        if (!IsConnected(playerId))
        {
            ping = 0;
            return false;
        }

        ping = GetPing(playerId);
        return true;
    }

    public bool TryGetPlayer(ushort playerId, out SFPlayerSnapshot player)
    {
        bool isConnected = IsConnected(playerId);
        if (!isConnected)
        {
            player = default;
            return false;
        }

        player = new(
            Id: playerId,
            Name: GetName(playerId),
            Score: GetScore(playerId),
            Ping: GetPing(playerId),
            IsConnected: true,
            IsNpc: IsNpc(playerId),
            IsLocal: playerId == LocalPlayerId
        );
        return true;
    }

    public SFPlayer Get(ushort playerId)
    {
        return playerId == LocalPlayerId ? Local : new SFRemotePlayer(playerId);
    }

    public SFRemotePlayer GetRemote(ushort playerId)
    {
        return new SFRemotePlayer(playerId);
    }

    public bool TryGetRemote(ushort playerId, out SFRemotePlayer player)
    {
        if (playerId == LocalPlayerId || !IsConnected(playerId))
        {
            player = null!;
            return false;
        }

        player = new SFRemotePlayer(playerId);
        return true;
    }

    public IEnumerable<SFPlayerSnapshot> EnumeratePlayers(bool includeNpcPlayers = true)
    {
        RequireThread();
        List<SFPlayerSnapshot> snapshot = [];
        for (ushort playerId = 0; playerId < SampOffsets.CPlayerPool.MaxPlayers; playerId++)
        {
            if (!TryGetPlayer(playerId, out SFPlayerSnapshot player))
            {
                continue;
            }

            if (!includeNpcPlayers && player.IsNpc)
            {
                continue;
            }

            snapshot.Add(player);
        }
        return snapshot;
    }

    public ushort? GetAimedPlayerId()
    {
        RequireThread();
        ushort aimedPlayer = CLocalPlayer.Instance.AimedPlayerId;
        return aimedPlayer != ushort.MaxValue && IsConnected(aimedPlayer) ? aimedPlayer : null;
    }

    public SFRemotePlayer? GetAimedPlayer()
    {
        ushort? playerId = GetAimedPlayerId();
        return playerId.HasValue ? new SFRemotePlayer(playerId.Value) : null;
    }
}
