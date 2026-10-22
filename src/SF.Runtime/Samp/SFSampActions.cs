using SFSharp.Abstractions.Samp.Events;
using SFSharp.Runtime.Samp.Events;

namespace SFSharp.Runtime.Samp;

/// <summary>Lazy native action observers; constructing the service does not resolve samp.dll.</summary>
internal sealed class SFSampActions(MainThreadDispatcher mainThread, ISFGameClock clock, ISFSampEntities entities) : ISFSampActions
{
    public ISFSampSessionEvents Session { get; } = new SampSessionEvents(mainThread, clock);
    public ISFSampRemotePlayerEvents RemotePlayers { get; } = new SampRemotePlayerEvents(mainThread, entities);
    public ISFSampPoolEvents Pools { get; } = new SampPoolEvents(mainThread);
    public ISFSampTextDrawEvents TextDraws { get; } = new SampTextDrawEvents(mainThread);
    public ISFSampPickupEvents Pickups { get; } = new SampPickupEvents(mainThread);
    public ISFSampSpectatingEvents Spectating { get; } = new SampSpectatingEvents(mainThread);
    private readonly SampActionEvent<NoRetValue> _spawnRequested = new(nameof(OnSpawnRequested), mainThread,
        publish => new SampVoidActionHook(Address(SampOffsets.CLocalPlayer.RequestSpawn), () => publish(default)));
    private readonly SampActionEvent<bool> _spawnCompleted = new(nameof(OnSpawnCompleted), mainThread,
        publish => new SampSpawnActionHook(Address(SampOffsets.CLocalPlayer.Spawn), publish));
    private readonly SampActionEvent<NoRetValue> _deathNotified = new(nameof(OnDeathNotified), mainThread,
        publish => new SampVoidActionHook(Address(SampOffsets.CLocalPlayer.WastedNotification), () => publish(default)));
    private readonly SampActionEvent<int> _classRequested = new(nameof(OnClassRequested), mainThread,
        publish => new SampIntActionHook(Address(SampOffsets.CLocalPlayer.RequestClass), publish));
    private readonly SampActionEvent<byte> _interiorChanged = new(nameof(OnInteriorChanged), mainThread,
        publish => new SampInteriorActionHook(Address(SampOffsets.CLocalPlayer.ChangeInterior), publish));
    private readonly SampActionEvent<SFSampVehicleEntryEvent> _vehicleEntry = new(nameof(OnVehicleEntryNotified), mainThread,
        publish => new SampVehicleEntryActionHook(Address(SampOffsets.CLocalPlayer.EnterVehicle), publish));
    private readonly SampActionEvent<int> _vehicleExit = new(nameof(OnVehicleExitNotified), mainThread,
        publish => new SampIntActionHook(Address(SampOffsets.CLocalPlayer.ExitVehicle), publish));
    private readonly SampActionEvent<NoRetValue> _restarted = new(nameof(OnRestarted), mainThread,
        publish => new SampVoidActionHook(Address(SampOffsets.CNetGame.ShutdownForRestart), () => publish(default)));

    public IDisposable OnSpawnRequested(Action handler) => Subscribe(_spawnRequested, handler);
    public IDisposable OnSpawnCompleted(Action<bool> handler) => _spawnCompleted.Subscribe(handler);
    public IDisposable OnDeathNotified(Action handler) => Subscribe(_deathNotified, handler);
    public IDisposable OnClassRequested(Action<int> handler) => _classRequested.Subscribe(handler);
    public IDisposable OnInteriorChanged(Action<byte> handler) => _interiorChanged.Subscribe(handler);
    public IDisposable OnVehicleEntryNotified(Action<SFSampVehicleEntryEvent> handler) => _vehicleEntry.Subscribe(handler);
    public IDisposable OnVehicleExitNotified(Action<int> handler) => _vehicleExit.Subscribe(handler);
    public IDisposable OnRestarted(Action handler) => Subscribe(_restarted, handler);

    private static IDisposable Subscribe(SampActionEvent<NoRetValue> source, Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return source.Subscribe(_ => handler());
    }

    private static nint Address(int rva) => ModuleResolver.GetProcAddress("samp.dll", rva);
}
