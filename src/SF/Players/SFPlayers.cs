namespace SFSharp;

public readonly record struct SFPlayerSnapshot(
    ushort Id,
    string? Name,
    int Score,
    int Ping,
    bool IsConnected,
    bool IsNpc,
    bool IsLocal
);

public partial class SFPlayers
{
    public SFLocalPlayer Local { get; } = new();

    public ushort LocalPlayerId => CPlayerPool.Instance.LocalPlayerId;
    public string? LocalPlayerName => CPlayerPool.Instance.GetLocalPlayerName();
    public int LocalPlayerScore => CPlayerPool.Instance.GetLocalPlayerScore();
    public int LocalPlayerPing => CPlayerPool.Instance.GetLocalPlayerPing();

    public string? GetName(ushort playerId) => CPlayerPool.Instance.GetName(playerId);
    public int GetScore(ushort playerId) => CPlayerPool.Instance.GetScore(playerId);
    public int GetPing(ushort playerId) => CPlayerPool.Instance.GetPing(playerId);
    public bool IsConnected(ushort playerId) => CPlayerPool.Instance.IsConnected(playerId);
    public bool IsNpc(ushort playerId) => CPlayerPool.Instance.IsNpc(playerId);
    public int GetConnectedCount(bool includeNpcPlayers = true) => CPlayerPool.Instance.GetCount(includeNpcPlayers);

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

            yield return player;
        }
    }

    public ushort? GetAimedPlayerId()
    {
        ushort aimedPlayer = CLocalPlayer.Instance.AimedPlayerId;
        return aimedPlayer != ushort.MaxValue && IsConnected(aimedPlayer) ? aimedPlayer : null;
    }

    public SFRemotePlayer? GetAimedPlayer()
    {
        ushort? playerId = GetAimedPlayerId();
        return playerId.HasValue ? new SFRemotePlayer(playerId.Value) : null;
    }
}
