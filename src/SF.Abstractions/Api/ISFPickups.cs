namespace SFSharp;

/// <summary>Read-only pickup pool facade.</summary>
public interface ISFPickups
{
    int Count { get; }

    bool Exists(int pickupIndex);

    int GetIndex(int serverPickupId);

    IEnumerable<int> EnumerateIndices();
}
