namespace SFSharp;

/// <summary>Read-only object pool facade.</summary>
public interface ISFObjects
{
    int Count { get; }
    int LargestId { get; }

    bool Exists(ushort objectId);

    IEnumerable<ushort> EnumerateIds();
}
