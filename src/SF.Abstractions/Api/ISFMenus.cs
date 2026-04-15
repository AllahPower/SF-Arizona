namespace SFSharp;

/// <summary>Read-only menu pool facade.</summary>
public interface ISFMenus
{
    ushort CurrentMenu { get; }
    bool Cancelled { get; }

    bool Exists(byte menuId);

    IEnumerable<byte> EnumerateIds();
}
