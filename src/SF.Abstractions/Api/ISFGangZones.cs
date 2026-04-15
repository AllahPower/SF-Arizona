namespace SFSharp;

/// <summary>Read-only gang zone pool facade.</summary>
public interface ISFGangZones
{
    bool Exists(ushort zoneId);

    IEnumerable<ushort> EnumerateIds();
}
