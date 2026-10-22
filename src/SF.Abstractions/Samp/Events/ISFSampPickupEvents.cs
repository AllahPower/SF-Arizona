namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Server-pickup lifecycle and collection notification routines, independent of packet parsing.</summary>
public interface ISFSampPickupEvents
{
    /// <summary>Creation is published after the call; deletion before removal while its data is readable.</summary>
    IDisposable OnChanged(Action<SFSampPickupEvent> handler);
    /// <summary>Contains the original lookup argument and resolved pool index. It does not guarantee an RPC or reward.</summary>
    IDisposable OnCollectionNotified(Action<SFSampPickupNotificationEvent> handler);
}
