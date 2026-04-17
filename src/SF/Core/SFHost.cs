namespace SFSharp;

/// <summary>
/// Single source of truth for every runtime SF service singleton. Static <see cref="SF"/> and the
/// plugin-facing <see cref="ISF"/> both delegate here so service identity is guaranteed.
/// </summary>
internal sealed class SFHost : ISF
{
    public static readonly SFHost Shared = new();

    private SFHost() { }

    public SFChat ChatImpl { get; } = new();
    public SFDialog DialogImpl { get; } = new();
    public SFKeyboard KeyboardImpl { get; } = new();
    public SFPlayers PlayersImpl { get; } = new();
    public SFVehicles VehiclesImpl { get; } = new();
    public SFGamePools PoolsImpl { get; } = new();
    public SFRpc RpcImpl { get; } = new();
    public SFPackets PacketsImpl { get; } = new();
    public SFArizonaPackets ArizonaImpl { get; } = new();
    public SFArizonaChat ArizonaChatImpl { get; } = new();
    public SFPacketParsers PacketParsersImpl { get; } = new();
    public SFRpcParsers RpcParsersImpl { get; } = new();
    public SFCamera CameraImpl { get; } = new();
    public SFEvents EventsImpl { get; } = new();
    public SFNetwork NetworkImpl { get; } = new();
    public SFModuleRuntime ModuleRuntime { get; } = new();

    public string UserFilesDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files");

    ISFChat ISF.Chat => ChatImpl;
    ISFDialog ISF.Dialog => DialogImpl;
    ISFKeyboard ISF.Keyboard => KeyboardImpl;
    ISFPlayers ISF.Players => PlayersImpl;
    ISFVehicles ISF.Vehicles => VehiclesImpl;
    ISFGamePools ISF.Pools => PoolsImpl;
    ISFModules ISF.Modules => SFPublicModules.Instance;
    ISFEvents ISF.Events => EventsImpl;
    ISFRpc ISF.Rpc => RpcImpl;
    ISFPackets ISF.Packets => PacketsImpl;
    ISFArizonaPackets ISF.Arizona => ArizonaImpl;
    ISFArizonaChat ISF.ArizonaChat => ArizonaChatImpl;
    ISFPacketParsers ISF.PacketParsers => PacketParsersImpl;
    ISFRpcParsers ISF.RpcParsers => RpcParsersImpl;
    ISFCamera ISF.Camera => CameraImpl;
    ISFNetwork ISF.Network => NetworkImpl;
}
