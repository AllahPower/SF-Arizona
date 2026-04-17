namespace SFSharp.Abstractions.Game;

/// <summary>Read-only textdraw pool facade backed by copied textdraw snapshots.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP textdraw pool memory - main-thread only.</remarks>
public interface ISFTextDraws
{
    bool Exists(ushort textDrawId);

    bool TryGetSnapshot(ushort textDrawId, out SFTextDrawSnapshot snapshot);

    IEnumerable<ushort> EnumerateIds();
}
