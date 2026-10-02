namespace SFSharp.Runtime;

/// <summary>
/// The game services behind <see cref="ISF"/>, created by <see cref="SFRuntime"/> once CNetGame is ready.
/// Every service receives its dependencies through its constructor.
/// </summary>
internal sealed class SFHost : ISF
{
    public SFHost(SFRuntime runtime)
    {
        ChatImpl = new SFChat(runtime.Exceptions);
        DialogImpl = new SFDialog(runtime.MainThread, runtime.Exceptions);
        KeyboardImpl = new SFKeyboard(runtime.Exceptions);
        PlayersImpl = new SFPlayers();
        VehiclesImpl = new SFVehicles();
        PoolsImpl = new SFGamePools(PlayersImpl, VehiclesImpl);
        EntitiesImpl = new SFEntities(PlayersImpl, VehiclesImpl, PoolsImpl);
        RpcImpl = new SFRpc(runtime.Filters, runtime.Dispatcher);
        PacketsImpl = new SFPackets(runtime.Filters, runtime.Dispatcher);
        ArizonaImpl = new SFArizonaPackets(PacketsImpl, runtime.Dispatcher);
        RpcParsersImpl = new SFRpcParsers(RpcParserCatalog.CreateDefaultRegistry(), RpcImpl);
        PacketParsersImpl = new SFPacketParsers(PacketParserCatalog.CreateDefaultRegistry(), PacketsImpl, ArizonaImpl);
        CameraImpl = new SFCamera(PlayersImpl, RpcImpl);
        EventsImpl = new SFEvents(new SFEventFactory(RpcImpl, RpcParsersImpl, PacketParsersImpl));
        NetworkImpl = new SFNetwork(runtime.Hooks.IncomingRpcPacket);
    }

    public SFChat ChatImpl { get; }
    public SFDialog DialogImpl { get; }
    public SFKeyboard KeyboardImpl { get; }
    public SFPlayers PlayersImpl { get; }
    public SFVehicles VehiclesImpl { get; }
    public SFGamePools PoolsImpl { get; }
    public SFEntities EntitiesImpl { get; }
    public SFRpc RpcImpl { get; }
    public SFPackets PacketsImpl { get; }
    public SFArizonaPackets ArizonaImpl { get; }
    public SFPacketParsers PacketParsersImpl { get; }
    public SFRpcParsers RpcParsersImpl { get; }
    public SFCamera CameraImpl { get; }
    public SFEvents EventsImpl { get; }
    public SFNetwork NetworkImpl { get; }
    public SFModuleRuntime ModuleRuntime { get; } = new();

    public string UserFilesDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files");

    ISFChat ISF.Chat => ChatImpl;
    ISFDialog ISF.Dialog => DialogImpl;
    ISFKeyboard ISF.Keyboard => KeyboardImpl;
    ISFPlayers ISF.Players => PlayersImpl;
    ISFVehicles ISF.Vehicles => VehiclesImpl;
    ISFGamePools ISF.Pools => PoolsImpl;
    ISFEntities ISF.Entities => EntitiesImpl;
    ISFModules ISF.Modules => SFPublicModules.Instance;
    ISFEvents ISF.Events => EventsImpl;
    ISFRpc ISF.Rpc => RpcImpl;
    ISFPackets ISF.Packets => PacketsImpl;
    ISFArizonaPackets ISF.Arizona => ArizonaImpl;
    ISFPacketParsers ISF.PacketParsers => PacketParsersImpl;
    ISFRpcParsers ISF.RpcParsers => RpcParsersImpl;
    ISFCamera ISF.Camera => CameraImpl;
    ISFNetwork ISF.Network => NetworkImpl;
}
