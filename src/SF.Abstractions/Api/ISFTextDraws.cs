namespace SFSharp;

/// <summary>Read-only textdraw pool facade backed by copied textdraw snapshots.</summary>
public interface ISFTextDraws
{
    bool Exists(ushort textDrawId);

    bool TryGetSnapshot(ushort textDrawId, out SFTextDrawSnapshot snapshot);

    IEnumerable<ushort> EnumerateIds();
}
