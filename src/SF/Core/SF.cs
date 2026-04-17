namespace SFSharp;

/// <summary>
/// Thin static proxy over <see cref="SFHost.Shared"/> for host-internal callers. Every property
/// returns the same singleton instance exposed through <see cref="ISF"/>, so there is a single
/// source of truth for service identity.
/// </summary>
public static class SF
{
    public static ISF Instance => SFHost.Shared;

    public static SFChat Chat => SFHost.Shared.ChatImpl;
    public static SFDialog Dialog => SFHost.Shared.DialogImpl;
    public static SFKeyboard Keyboard => SFHost.Shared.KeyboardImpl;
    public static SFPlayers Players => SFHost.Shared.PlayersImpl;
    public static SFVehicles Vehicles => SFHost.Shared.VehiclesImpl;
    public static SFGamePools Pools => SFHost.Shared.PoolsImpl;
    public static SFRpc Rpc => SFHost.Shared.RpcImpl;
    public static SFPackets Packets => SFHost.Shared.PacketsImpl;
    public static SFArizonaPackets Arizona => SFHost.Shared.ArizonaImpl;
    public static SFArizonaChat ArizonaChat => SFHost.Shared.ArizonaChatImpl;
    public static SFPacketParsers PacketParsers => SFHost.Shared.PacketParsersImpl;
    public static SFRpcParsers RpcParsers => SFHost.Shared.RpcParsersImpl;
    public static SFCamera Camera => SFHost.Shared.CameraImpl;
    public static SFEvents Events => SFHost.Shared.EventsImpl;
    public static SFNetwork Network => SFHost.Shared.NetworkImpl;

    public static SFModuleRuntime Modules => SFHost.Shared.ModuleRuntime;

    public static string UserFilesDirectory => SFHost.Shared.UserFilesDirectory;
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
