namespace SFSharp.Abstractions.Samp;

/// <summary>Client revisions, independent of the host/package version.</summary>
public enum SFSampVersion
{
    Unknown,
    R1,
    R3_1,
    R5_1,
}

/// <summary>Implemented native services, not merely recognized client revisions.</summary>
[Flags]
public enum SFSampCapabilities
{
    None = 0,
    Chat = 1,
    Dialogs = 2,
    Pools = 4,
    Network = 8,
    IncomingEmulation = 16,
    CompressedStrings = 32,
}

/// <summary>Immutable client metadata, safe to read from any thread.</summary>
public sealed record SFSampClientInfo(SFSampVersion Version, SFSampCapabilities Capabilities);
