namespace SFSharp.Abstractions.Game;

public enum SFSampEntityType : byte
{
    Player,
    Vehicle,
    Object,
    Actor,
}

public enum SFSampEntityChange : byte
{
    /// <summary>Added to its SA-MP pool; for players this is the connection, without a ped yet.</summary>
    Created,

    /// <summary>About to be removed from its SA-MP pool; <see cref="SFSampEntityEvent.Entity"/> is still alive.</summary>
    Deleted,

    /// <summary>A remote player's ped was spawned into the world.</summary>
    StreamedIn,

    /// <summary>A remote player's ped is about to be removed from the world.</summary>
    StreamedOut,
}

/// <summary>Change of a server-owned entity in a SA-MP pool, raised synchronously on the main thread.</summary>
/// <param name="Entity">Game entity, when one exists at the time of the event.</param>
public readonly record struct SFSampEntityEvent(
    SFSampEntityType Type,
    ushort SampId,
    SFSampEntityChange Change,
    SFEntityRef? Entity);
