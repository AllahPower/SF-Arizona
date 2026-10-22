using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Samp.Events;

internal sealed class SampPickupEvents : ISFSampPickupEvents
{
    private readonly Lazy<SampObservationHooks> _hooks;
    private readonly SampActionEvent<SFSampPickupEvent> _changes;
    private readonly SampActionEvent<SFSampPickupNotificationEvent> _notifications;

    public SampPickupEvents(MainThreadDispatcher mainThread)
    {
        _hooks = new(() => SampObservationHooks.Create(Install));
        _changes = new(nameof(OnChanged), mainThread, _ => _hooks.Value);
        _notifications = new(nameof(OnCollectionNotified), mainThread, _ => _hooks.Value);
    }
    public IDisposable OnChanged(Action<SFSampPickupEvent> handler) => _changes.Subscribe(handler);
    public IDisposable OnCollectionNotified(Action<SFSampPickupNotificationEvent> handler) => _notifications.Subscribe(handler);

    private void Install(SampObservationHooks hooks)
    {
        hooks.Add(SampObservationPoint.PickupCreate, SampObservationAbi.Pickup, new SampObservation<bool>(
            args => (uint)args.Value < 4096 && SampObservationMemory.Read<int>(args.Instance, 0) < 4096,
            (args, attempted) =>
            {
                if (attempted && SampObservationMemory.Pickup(args.Instance, args.Value, SFSampPickupChange.Created) is { } value) _changes.Publish(value);
            }, () => _changes.HasSubscribers));
        hooks.Add(SampObservationPoint.PickupDelete, SampObservationAbi.Int, new SampObservation<NoRetValue>(args =>
        {
            if (SampObservationMemory.Read<int>(args.Instance, 0) > 0
                && SampObservationMemory.Pickup(args.Instance, args.Value, SFSampPickupChange.Deleting) is { } value) _changes.Publish(value);
            return default;
        }, (_, _) => { }, () => _changes.HasSubscribers));
        hooks.Add(SampObservationPoint.PickupNotify, SampObservationAbi.Int, new SampObservation<SFSampPickupNotificationEvent>(args =>
        {
            int index = SampObservationMemory.PickupIndex(args.Instance, args.Value);
            bool occupied = SampObservationMemory.Pickup(args.Instance, index, SFSampPickupChange.Created).HasValue;
            return new(args.Value, index, occupied ? index : null);
        }, (_, notification) => _notifications.Publish(notification), () => _notifications.HasSubscribers));
    }
}
