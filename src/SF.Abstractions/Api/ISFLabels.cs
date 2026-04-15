namespace SFSharp;

/// <summary>Read-only 3D label pool facade backed by copied label snapshots.</summary>
public interface ISFLabels
{
    bool Exists(ushort labelId);

    bool TryGetSnapshot(ushort labelId, out SFLabelSnapshot snapshot);

    IEnumerable<ushort> EnumerateIds();
}
