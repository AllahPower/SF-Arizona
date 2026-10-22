using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Samp.Events;

internal sealed class SampPoolEvents : ISFSampPoolEvents
{
    private readonly Lazy<SampObservationHooks> _hooks;
    private readonly SampActionEvent<SFSampPoolEvent> _events;

    public SampPoolEvents(MainThreadDispatcher mainThread)
    {
        _hooks = new(() => SampObservationHooks.Create(Install));
        _events = new(nameof(OnChanged), mainThread, _ => _hooks.Value);
    }
    public IDisposable OnChanged(Action<SFSampPoolEvent> handler) => _events.Subscribe(handler);

    private void Install(SampObservationHooks hooks)
    {
        (SampObservationPoint point, SFSampPoolKind pool)[] entries = [
            (SampObservationPoint.ResetPools, SFSampPoolKind.All),
            (SampObservationPoint.ResetPlayers, SFSampPoolKind.Players),
            (SampObservationPoint.ResetVehicles, SFSampPoolKind.Vehicles),
            (SampObservationPoint.ResetObjects, SFSampPoolKind.Objects),
            (SampObservationPoint.ResetActors, SFSampPoolKind.Actors),
            (SampObservationPoint.ResetPickups, SFSampPoolKind.Pickups),
            (SampObservationPoint.ResetMenus, SFSampPoolKind.Menus),
            (SampObservationPoint.ResetTextDraws, SFSampPoolKind.TextDraws),
            (SampObservationPoint.ResetGangZones, SFSampPoolKind.GangZones),
            (SampObservationPoint.ResetLabels, SFSampPoolKind.Labels),
            (SampObservationPoint.InitializePools, SFSampPoolKind.All)];
        foreach ((SampObservationPoint point, SFSampPoolKind pool) in entries)
        {
            SFSampPoolOperation operation = point == SampObservationPoint.InitializePools ? SFSampPoolOperation.Initialize : SFSampPoolOperation.Reset;
            hooks.Add(point, SampObservationAbi.Void, new SampObservation<NoRetValue>(
                _ => { _events.Publish(new(pool, operation, SFSampEventPhase.Before)); return default; },
                (_, _) => _events.Publish(new(pool, operation, SFSampEventPhase.After)), () => _events.HasSubscribers));
        }
    }
}
