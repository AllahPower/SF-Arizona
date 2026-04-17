namespace SFSharp.Abstractions.Game;

/// <summary>Read-only 3D label pool facade backed by copied label snapshots.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP 3D text label pool memory - main-thread only.</remarks>
public interface ISFLabels
{
    bool Exists(ushort labelId);

    bool TryGetSnapshot(ushort labelId, out SFLabelSnapshot snapshot);

    IEnumerable<ushort> EnumerateIds();
}
