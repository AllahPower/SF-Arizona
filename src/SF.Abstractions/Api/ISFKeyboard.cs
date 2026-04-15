namespace SFSharp;

/// <summary>
/// Plugin-facing contract for keyboard state queries. The host owns the polling loop and exposes
/// the latest sampled state through this interface.
/// </summary>
public interface ISFKeyboard
{
    /// <summary>Returns <see langword="true"/> while the virtual key is currently held down.</summary>
    bool IsKeyDown(byte virtualKeyCode);

    /// <summary>Returns <see langword="true"/> only on the transition from up to down.</summary>
    bool IsKeyPressed(byte virtualKeyCode);
}
