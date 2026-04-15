namespace SFSharp;

/// <summary>Read-only actor pool facade.</summary>
public interface ISFActors
{
    int LargestId { get; }

    bool Exists(ushort actorId);

    IEnumerable<ushort> EnumerateIds();
}
