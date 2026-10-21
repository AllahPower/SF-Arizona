namespace SFSharp.Runtime;

/// <summary>
/// The game services behind <see cref="ISF"/>, created by <see cref="SFRuntime"/> once CNetGame is ready.
/// Every service receives its dependencies through its constructor.
/// </summary>
internal sealed class SFHost : ISF, ISFGta, ISFSamp
{
    public SFHost(SFRuntime runtime)
    {
        ChatImpl = new SFChat(runtime.Exceptions);
        DialogImpl = new SFDialog(runtime.MainThread, runtime.Exceptions);
        KeyboardImpl = new SFKeyboard(runtime.Exceptions);
        PlayersImpl = new SFPlayers();
        VehiclesImpl = new SFVehicles();
        PoolsImpl = new SFGamePools(PlayersImpl, VehiclesImpl);
        EntitiesImpl = new SFEntities(runtime.MainThread);
        SampEntitiesImpl = new SFSampEntities(PlayersImpl, VehiclesImpl, PoolsImpl);
        RpcImpl = new SFRpc(runtime.Filters, runtime.Dispatcher);
        PacketsImpl = new SFPackets(runtime.Filters, runtime.Dispatcher);
        RpcParsersImpl = new SFRpcParsers(RpcParserCatalog.CreateDefaultRegistry(), RpcImpl);
        PacketParsersImpl = new SFPacketParsers(PacketParserCatalog.CreateDefaultRegistry(), PacketsImpl);
        CameraImpl = new SFGtaCamera(runtime.MainThread);
        World = new SFGtaWorld(runtime.MainThread);
        SampCameraImpl = new SFSampCamera(PlayersImpl, RpcImpl);
        EventsImpl = new SFEvents(new SFEventFactory(RpcImpl, RpcParsersImpl, PacketParsersImpl));
        NetworkImpl = new SFNetwork(runtime.Hooks.IncomingRpcPacket);
        LoadingImpl = runtime.Loading;
        MainThread = runtime.MainThread;
        Clock = runtime.Clock;
    }

    public SFChat ChatImpl { get; }
    public SFDialog DialogImpl { get; }
    public SFKeyboard KeyboardImpl { get; }
    public SFPlayers PlayersImpl { get; }
    public SFVehicles VehiclesImpl { get; }
    public SFGamePools PoolsImpl { get; }
    public SFEntities EntitiesImpl { get; }
    public SFSampEntities SampEntitiesImpl { get; }
    public SFRpc RpcImpl { get; }
    public SFPackets PacketsImpl { get; }
    public SFPacketParsers PacketParsersImpl { get; }
    public SFRpcParsers RpcParsersImpl { get; }
    public SFGtaCamera CameraImpl { get; }
    public SFSampCamera SampCameraImpl { get; }
    public SFEvents EventsImpl { get; }
    public SFNetwork NetworkImpl { get; }
    public SFGameLoading LoadingImpl { get; }
    public SFModuleRuntime ModuleRuntime { get; } = new();
    public ISFMainThread MainThread { get; }
    public ISFGameClock Clock { get; }
    public ISFWorld World { get; }

    public string UserFilesDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files");

    ISFGta ISF.Gta => this;
    ISFSamp ISF.Samp => this;
    ISFChat ISFSamp.Chat => ChatImpl;
    ISFDialog ISFSamp.Dialog => DialogImpl;
    ISFKeyboard ISF.Keyboard => KeyboardImpl;
    ISFPlayers ISFSamp.Players => PlayersImpl;
    ISFVehicles ISFSamp.Vehicles => VehiclesImpl;
    ISFGamePools ISFSamp.Pools => PoolsImpl;
    ISFEntities ISFGta.Entities => EntitiesImpl;
    ISFSampEntities ISFSamp.Entities => SampEntitiesImpl;
    ISFSampCamera ISFSamp.Camera => SampCameraImpl;
    ISFModules ISF.Modules => SFPublicModules.Instance;
    ISFEvents ISFSamp.Events => EventsImpl;
    ISFRpc ISFSamp.Rpc => RpcImpl;
    ISFPackets ISFSamp.Packets => PacketsImpl;
    ISFPacketParsers ISFSamp.PacketParsers => PacketParsersImpl;
    ISFRpcParsers ISFSamp.RpcParsers => RpcParsersImpl;
    ISFCamera ISFGta.Camera => CameraImpl;
    ISFNetwork ISFSamp.Network => NetworkImpl;
    ISFGameLoading ISF.Loading => LoadingImpl;
}
