namespace SFSharp.Abstractions.Game;

/// <summary>Read-only actor pool facade.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP actor pool memory - main-thread only.</remarks>
public interface ISFActors
{
    int LargestId { get; }

    bool Exists(ushort actorId);

    IEnumerable<ushort> EnumerateIds();
}
