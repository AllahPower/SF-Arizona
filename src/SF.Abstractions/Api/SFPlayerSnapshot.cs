namespace SFSharp;

/// <summary>
/// Immutable read-only player snapshot suitable for external modules. Captured from the current
/// scoreboard/runtime state and detached from the host's internal player wrappers.
/// </summary>
public readonly record struct SFPlayerSnapshot(
    ushort Id,
    string? Name,
    int Score,
    int Ping,
    bool IsConnected,
    bool IsNpc,
    bool IsLocal
);
