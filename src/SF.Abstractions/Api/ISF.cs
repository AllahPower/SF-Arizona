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
}
