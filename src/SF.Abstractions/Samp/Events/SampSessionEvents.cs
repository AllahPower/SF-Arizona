namespace SFSharp.Abstractions.Samp.Events;

public enum SFSampConnectionEventKind : byte
{
    PublicKeyMismatch, Banned, RequestAccepted, ServerFull, Disconnected,
    InvalidPassword, AttemptFailed, ConnectionLost, JoinHandshake,
}

/// <summary>The named native packet handler returned. JoinHandshake is not full game initialization.</summary>
public readonly record struct SFSampConnectionEvent(SFSampConnectionEventKind Kind, SFSampGameState State);
public readonly record struct SFSampSessionStateEvent(SFSampGameState? Previous, SFSampGameState Current);

public enum SFSampPoolKind : byte
{
    All, Players, Vehicles, Objects, Actors, Pickups, Menus, TextDraws, GangZones, Labels,
}

public enum SFSampPoolOperation : byte { Reset, Initialize }
public enum SFSampEventPhase : byte { Before, After }
public readonly record struct SFSampPoolEvent(SFSampPoolKind Pool, SFSampPoolOperation Operation, SFSampEventPhase Phase);
