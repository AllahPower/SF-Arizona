using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Samp.Events;

internal sealed class SampSpectatingEvents : ISFSampSpectatingEvents
{
    private readonly Lazy<SampObservationHooks> _hooks;
    private readonly SampActionEvent<SFSampSpectatingEvent> _events;

    public SampSpectatingEvents(MainThreadDispatcher mainThread)
    {
        _hooks = new(() => SampObservationHooks.Create(Install));
        _events = new(nameof(OnChanged), mainThread, _ => _hooks.Value);
    }
    public IDisposable OnChanged(Action<SFSampSpectatingEvent> handler) => _events.Subscribe(handler);

    private void Install(SampObservationHooks hooks)
    {
        SFSampVersion version = SampEnvironment.RequiredProfile.Version;
        foreach (SampObservationPoint point in new[] { SampObservationPoint.SpectatingEnable, SampObservationPoint.SpectatingPlayer, SampObservationPoint.SpectatingVehicle })
            hooks.Add(point, point == SampObservationPoint.SpectatingEnable ? SampObservationAbi.Int : SampObservationAbi.Id,
                new SampObservation<SFSampSpectatingEvent>(args => SampObservationMemory.Spectating(args.Instance, version),
                    (args, before) =>
                    {
                        SFSampSpectatingEvent after = SampObservationMemory.Spectating(args.Instance, version);
                        if (before != after) _events.Publish(after);
                    }, () => _events.HasSubscribers));
    }
}
