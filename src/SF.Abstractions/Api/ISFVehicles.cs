namespace SFSharp;

/// <summary>Read-only vehicle pool facade backed by copied vehicle snapshots.</summary>
/// <remarks>NOT thread-safe. Members read native SA-MP pool memory - main-thread only.</remarks>
public interface ISFVehicles
{
    int Count { get; }

    bool Exists(ushort vehicleId);

    ushort GetNearestId();

    IEnumerable<ushort> EnumerateIds();

    bool TryGetSnapshot(ushort vehicleId, out SFVehicleSnapshot snapshot);
}
