namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Observes remote client processing, not authoritative server or GTA physics outcomes.</summary>
public interface ISFSampRemotePlayerEvents
{
    /// <summary>Copies native sync input before processing and available stored data after it returns. No native pointers escape.</summary>
    IDisposable OnSyncProcessed(SFSampSyncKind kind, Action<SFSampRemoteSyncEvent> handler);
    /// <summary>Reports observed state transitions at Process, sync and action boundaries; Initial has no previous state.</summary>
    IDisposable OnStateChanged(Action<SFSampRemoteStateEvent> handler);
    /// <summary>Reports native entry, exit and Kill routine completions. These are not successful-animation confirmations.</summary>
    IDisposable OnActionCompleted(Action<SFSampRemoteActionEvent> handler);
}
