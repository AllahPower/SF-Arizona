namespace SFSharp.Runtime;

/// <summary>
/// Thin static proxy over <see cref="SFBootstrap.Runtime.Host"/> for host-internal callers. Every property
/// returns the same singleton instance exposed through <see cref="ISF"/>, so there is a single
/// source of truth for service identity.
/// </summary>
public static class SF
{
    public static ISF Instance => SFBootstrap.Runtime.Host;

    public static SFChat Chat => SFBootstrap.Runtime.Host.ChatImpl;
    public static SFDialog Dialog => SFBootstrap.Runtime.Host.DialogImpl;
    public static SFKeyboard Keyboard => SFBootstrap.Runtime.Host.KeyboardImpl;
    public static SFPlayers Players => SFBootstrap.Runtime.Host.PlayersImpl;
    public static SFVehicles Vehicles => SFBootstrap.Runtime.Host.VehiclesImpl;
    public static SFGamePools Pools => SFBootstrap.Runtime.Host.PoolsImpl;
    public static SFEntities Entities => SFBootstrap.Runtime.Host.EntitiesImpl;
    public static SFRpc Rpc => SFBootstrap.Runtime.Host.RpcImpl;
    public static SFPackets Packets => SFBootstrap.Runtime.Host.PacketsImpl;
    public static SFArizonaPackets Arizona => SFBootstrap.Runtime.Host.ArizonaImpl;
    public static SFPacketParsers PacketParsers => SFBootstrap.Runtime.Host.PacketParsersImpl;
    public static SFRpcParsers RpcParsers => SFBootstrap.Runtime.Host.RpcParsersImpl;
    public static SFCamera Camera => SFBootstrap.Runtime.Host.CameraImpl;
    public static SFEvents Events => SFBootstrap.Runtime.Host.EventsImpl;
    public static SFNetwork Network => SFBootstrap.Runtime.Host.NetworkImpl;

    public static SFModuleRuntime Modules => SFBootstrap.Runtime.Host.ModuleRuntime;

    public static string UserFilesDirectory => SFBootstrap.Runtime.Host.UserFilesDirectory;
}

/// <summary>
/// Host-side module runtime settings, such as the pluggable <see cref="IModuleStorageProvider"/>.
/// Access through <see cref="SF.Modules"/>. Not to be confused with <see cref="ISFModules"/>, which
/// is the read-only public catalog of registered modules.
/// </summary>
/// <remarks>
/// <see cref="Storage"/> getter/setter is thread-safe but should be configured once during host
/// startup. Concurrent mutation after modules have started is not supported.
/// </remarks>
public sealed class SFModuleRuntime
{
    private IModuleStorageProvider _storage = new DefaultModuleStorageProvider();

    public IModuleStorageProvider Storage
    {
        get => _storage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _storage = value;
        }
    }

    public DefaultModuleStorageProvider? DefaultStorage => _storage as DefaultModuleStorageProvider;
}
