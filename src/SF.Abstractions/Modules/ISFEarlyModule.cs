namespace SFSharp.Abstractions.Modules;

/// <summary>
/// Plugin entry point that runs while GTA is still loading, before SA-MP's CNetGame and <see cref="ISF"/> exist.
/// </summary>
/// <remarks>
/// Every non-abstract type implementing this interface in a plugin assembly is created once through its public
/// parameterless constructor when the plugin loads, on the first runtime tick for plugins found at start-up.
/// It does not need <see cref="SFModuleAttribute"/> and is independent of <see cref="ISFModule"/> instances.
/// It lives until the plugin unloads; implement <see cref="IDisposable"/> for cleanup. A plugin loaded later
/// (<c>/sfs plugin-load</c>) gets the call too, with <see cref="ISFGameLoading.Stage"/> already advanced.
/// </remarks>
public interface ISFEarlyModule
{
    /// <summary>
    /// Called once on the game thread. Subscribe to load stages through <see cref="ISFEarlyContext.Loading"/>;
    /// an exception here fails the plugin load.
    /// </summary>
    void OnGameLoading(ISFEarlyContext context);
}
