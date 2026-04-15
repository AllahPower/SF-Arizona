namespace SFSharp;

/// <summary>
/// Root plugin-facing facade. The concrete implementation is owned by the host and exposed to
/// every module through <see cref="IModuleContext.SF"/>. Host modules can still use the
/// <c>SF</c> static class for in-proc access, the static class forwards to the same singleton.
/// </summary>
public interface ISF
{
    /// <summary>Chat helpers, see <see cref="ISFChat"/>.</summary>
    ISFChat Chat { get; }

    /// <summary>Dialog helpers, see <see cref="ISFDialog"/>.</summary>
    ISFDialog Dialog { get; }

    /// <summary>Keyboard state helpers, see <see cref="ISFKeyboard"/>.</summary>
    ISFKeyboard Keyboard { get; }

    /// <summary>Read-only player helpers, see <see cref="ISFPlayers"/>.</summary>
    ISFPlayers Players { get; }

    /// <summary>Typed parsed event subscriptions and streams, see <see cref="ISFEvents"/>.</summary>
    ISFEvents Events { get; }
}
