namespace SFSharp.Runtime;

/// <summary>
/// The game services behind <see cref="ISF"/>, created by <see cref="SFRuntime"/> once CNetGame is ready.
/// Every service receives its dependencies through its constructor.
/// </summary>
internal sealed class SFHost : ISF
{
    public SFHost(SFRuntime runtime)
    {
        ChatImpl = new SFChat(runtime.Exceptions, runtime.MainThread);
        DialogImpl = new SFDialog(runtime.MainThread, runtime.Exceptions);
        KeyboardImpl = new SFKeyboard(runtime.Exceptions);
        PlayersImpl = new SFPlayers(runtime.MainThread);
        VehiclesImpl = new SFVehicles(runtime.MainThread);
        PoolsImpl = new SFGamePools(PlayersImpl, VehiclesImpl, runtime.MainThread);
        EntitiesImpl = new SFEntities(runtime.MainThread);
        SampEntitiesImpl = new SFSampEntities(PlayersImpl, VehiclesImpl, PoolsImpl, runtime.MainThread);
        RpcImpl = new SFRpc(runtime.Filters, runtime.Dispatcher);
        PacketsImpl = new SFPackets(runtime.Filters, runtime.Dispatcher);
        RpcParsersImpl = new SFRpcParsers(RpcParserCatalog.CreateDefaultRegistry(), RpcImpl);
        PacketParsersImpl = new SFPacketParsers(PacketParserCatalog.CreateDefaultRegistry(), PacketsImpl);
        CameraImpl = new SFGtaCamera(runtime.MainThread);
        World = new SFGtaWorld(runtime.MainThread);
        SampCameraImpl = new SFSampCamera(PlayersImpl, RpcImpl, runtime.MainThread);
        EventsImpl = new SFEvents(new SFEventFactory(RpcImpl, RpcParsersImpl, PacketParsersImpl));
        NetworkImpl = new SFNetwork(runtime.Hooks.IncomingRpcPacket);
        LoadingImpl = runtime.Loading;
        MainThread = runtime.MainThread;
        Clock = runtime.Clock;
        Gta = new SFGtaServices(EntitiesImpl, CameraImpl, Clock, World);
        Samp = new SFSampServices(ChatImpl, DialogImpl, PlayersImpl, VehiclesImpl, PoolsImpl, SampEntitiesImpl,
            SampCameraImpl, EventsImpl, RpcImpl, PacketsImpl, PacketParsersImpl, RpcParsersImpl, NetworkImpl,
            new SFSampActions(runtime.MainThread, Clock, SampEntitiesImpl));
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

    public ISFGta Gta { get; }
    public ISFSamp Samp { get; }
    ISFKeyboard ISF.Keyboard => KeyboardImpl;
    ISFModules ISF.Modules => SFPublicModules.Instance;
    ISFGameLoading ISF.Loading => LoadingImpl;
}
