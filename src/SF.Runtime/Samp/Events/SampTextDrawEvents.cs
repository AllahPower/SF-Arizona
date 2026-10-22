using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Samp.Events;

internal sealed class SampTextDrawEvents : ISFSampTextDrawEvents
{
    private readonly Lazy<SampObservationHooks> _hooks;
    private readonly SampActionEvent<SFSampTextDrawSelectionEvent> _selection;
    private readonly SampActionEvent<ushort> _hover;
    private readonly SampActionEvent<ushort> _notification;

    public SampTextDrawEvents(MainThreadDispatcher mainThread)
    {
        _hooks = new(() => SampObservationHooks.Create(Install));
        _selection = new(nameof(OnSelectionChanged), mainThread, _ => _hooks.Value);
        _hover = new(nameof(OnHoveredChanged), mainThread, _ => _hooks.Value);
        _notification = new(nameof(OnSelectionNotified), mainThread, _ => _hooks.Value);
    }
    public IDisposable OnSelectionChanged(Action<SFSampTextDrawSelectionEvent> handler) => _selection.Subscribe(handler);
    public IDisposable OnHoveredChanged(Action<ushort> handler) => _hover.Subscribe(handler);
    public IDisposable OnSelectionNotified(Action<ushort> handler) => _notification.Subscribe(handler);

    private void Install(SampObservationHooks hooks)
    {
        foreach (SampObservationPoint point in new[] { SampObservationPoint.SelectionEnable, SampObservationPoint.SelectionDisable, SampObservationPoint.SelectionProcess })
            hooks.Add(point, point == SampObservationPoint.SelectionEnable ? SampObservationAbi.Int : SampObservationAbi.Void,
                new SampObservation<SFSampTextDrawSelectionEvent>(args => SampObservationMemory.Selection(args.Instance), (args, before) =>
                {
                    SFSampTextDrawSelectionEvent after = SampObservationMemory.Selection(args.Instance);
                    if (before.Enabled != after.Enabled || before.HoverColor != after.HoverColor) _selection.Publish(after);
                    if (before.HoveredId != after.HoveredId) _hover.Publish(after.HoveredId);
                }, () => _selection.HasSubscribers || _hover.HasSubscribers));
        // Capture the selected ID before RPC filters or nested UI calls can change it.
        hooks.Add(SampObservationPoint.SelectionNotify, SampObservationAbi.Void,
            new SampObservation<ushort>(args => SampObservationMemory.Selection(args.Instance).HoveredId,
                (_, id) => _notification.Publish(id), () => _notification.HasSubscribers));
    }
}
