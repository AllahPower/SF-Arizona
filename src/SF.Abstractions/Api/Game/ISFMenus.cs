namespace SFSharp;

/// <summary>Read-only menu pool facade.</summary>
/// <remarks>NOT thread-safe. Reads native SA-MP menu pool memory - main-thread only.</remarks>
public interface ISFMenus
{
    ushort CurrentMenu { get; }
    bool Cancelled { get; }

    bool Exists(byte menuId);

    IEnumerable<byte> EnumerateIds();
}
