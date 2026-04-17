namespace SFSharp.Abstractions.Game;

/// <summary>
/// Plugin-facing read-only player API. Exposes scoreboard and targeting helpers without surfacing
/// runtime-specific player wrapper objects.
/// </summary>
/// <remarks>
/// NOT thread-safe. Every accessor reads native SA-MP player-pool memory and must be called from
/// the main game thread.
/// </remarks>
public interface ISFPlayers
{
    ushort LocalPlayerId { get; }
    string? LocalPlayerName { get; }
    int LocalPlayerScore { get; }
    int LocalPlayerPing { get; }

    string? GetName(ushort playerId);
    int GetScore(ushort playerId);
    int GetPing(ushort playerId);
    bool IsConnected(ushort playerId);
    bool IsNpc(ushort playerId);
    int GetConnectedCount(bool includeNpcPlayers = true);

    bool TryGetName(ushort playerId, out string? name);
    bool TryGetScore(ushort playerId, out int score);
    bool TryGetPing(ushort playerId, out int ping);
    bool TryGetPlayer(ushort playerId, out SFPlayerSnapshot player);

    IEnumerable<SFPlayerSnapshot> EnumeratePlayers(bool includeNpcPlayers = true);
    ushort? GetAimedPlayerId();
    Task UpdateScoreboard();
}
