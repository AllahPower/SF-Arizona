namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Observes selection mode, hover changes and native click/cancel notifications.</summary>
public interface ISFSampTextDrawEvents
{
    IDisposable OnSelectionChanged(Action<SFSampTextDrawSelectionEvent> handler);
    /// <summary>Hovered ID 65535 means no target. Only changes are published.</summary>
    IDisposable OnHoveredChanged(Action<ushort> handler);
    /// <summary>ID 65535 means cancellation. This reports a notification routine, not server acceptance.</summary>
    IDisposable OnSelectionNotified(Action<ushort> handler);
}
