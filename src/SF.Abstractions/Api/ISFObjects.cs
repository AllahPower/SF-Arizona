namespace SFSharp;

/// <summary>Read-only object pool facade.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP object pool memory - main-thread only.</remarks>
public interface ISFObjects
{
    int Count { get; }
    int LargestId { get; }

    bool Exists(ushort objectId);

    IEnumerable<ushort> EnumerateIds();
}
