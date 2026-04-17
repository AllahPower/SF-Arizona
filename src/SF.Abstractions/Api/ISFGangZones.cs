namespace SFSharp;

/// <summary>Read-only gang zone pool facade.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP gang zone pool memory - main-thread only.</remarks>
public interface ISFGangZones
{
    bool Exists(ushort zoneId);

    IEnumerable<ushort> EnumerateIds();
}
