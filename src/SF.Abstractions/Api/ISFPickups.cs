namespace SFSharp;

/// <summary>Read-only pickup pool facade.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP pickup pool memory - main-thread only.</remarks>
public interface ISFPickups
{
    int Count { get; }

    bool Exists(int pickupIndex);

    int GetIndex(int serverPickupId);

    IEnumerable<int> EnumerateIndices();
}
