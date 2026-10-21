namespace SFSharp.Abstractions.Samp;

/// <summary>Revision-independent SA-MP connection state.</summary>
public enum SFSampGameState
{
    Unknown,
    WaitingToConnect,
    Connecting,
    Connected,
    WaitingToJoin,
    Restarting,
}
