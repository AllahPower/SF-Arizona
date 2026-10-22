namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Native connection-handler completions and observed session state changes.</summary>
/// <remarks>Register and dispose subscriptions through the module context. Callbacks run on the game thread.</remarks>
public interface ISFSampSessionEvents
{
    /// <summary>Reports which connection handler returned, without equating transport acceptance with joining the game.</summary>
    IDisposable OnConnectionEvent(Action<SFSampConnectionEvent> handler);
    /// <summary>Observes state at game ticks. Transient states between ticks may be absent; Initial is an initial snapshot.</summary>
    IDisposable OnStateChanged(Action<SFSampSessionStateEvent> handler);
    /// <summary>Observes native Connect calls. Throws SFSampFeatureUnavailableException when its entry is unverified.</summary>
    IDisposable OnConnectAttempt(Action handler);
}
