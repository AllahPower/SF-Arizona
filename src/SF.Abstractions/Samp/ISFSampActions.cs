namespace SFSharp.Abstractions.Samp;

/// <summary>Observes native SA-MP local-player actions, independently of packet parsing.</summary>
/// <remarks>
/// Subscribe on the game thread and register the returned subscription with the module context.
/// Callbacks run after the native routine returns and cannot cancel or change its result.
/// Native calls from another thread are delivered on the next game-thread tick.
/// These events describe client routines, not server acceptance or guaranteed RPC delivery.
/// Exceptions in a subscriber are logged without interrupting the game or other subscribers.
/// </remarks>
public interface ISFSampActions
{
    /// <summary>The spawn-request routine returned. It may do nothing outside class selection.</summary>
    IDisposable OnSpawnRequested(Action handler);

    /// <summary>A spawn attempt completed. The argument is true only when native Spawn returned success.</summary>
    IDisposable OnSpawnCompleted(Action<bool> handler);

    /// <summary>The local death-notification routine returned. No killer or cause is inferred.</summary>
    IDisposable OnDeathNotified(Action handler);

    /// <summary>A class-request routine returned with the supplied class index, before any server reply.</summary>
    IDisposable OnClassRequested(Action<int> handler);

    /// <summary>ChangeInterior returned with the supplied interior ID. Repeated IDs are not suppressed.</summary>
    IDisposable OnInteriorChanged(Action<byte> handler);

    /// <summary>The entry-notification routine returned. This is not confirmation that the ped occupies a seat.</summary>
    IDisposable OnVehicleEntryNotified(Action<SFSampVehicleEntryEvent> handler);

    /// <summary>The exit-notification routine returned with a SA-MP vehicle ID. It may send no RPC for RC or missing vehicles.</summary>
    IDisposable OnVehicleExitNotified(Action<int> handler);

    /// <summary>ShutdownForRestart completed its pool and world reset. Not a general disconnect event.</summary>
    IDisposable OnRestarted(Action handler);
}
