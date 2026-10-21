namespace SFSharp.Runtime.Interop.Native;

/// <summary>Verified CNetGame layout. Recognition is separate from complete runtime support.</summary>
internal sealed record SampClientProfile(
    SFSampVersion Version, uint EntryPointRva, uint NetGameRva,
    int RakClientOffset, int PoolsOffset, int StateOffset, uint GetStateRva,
    bool IsRuntimeSupported)
{
    // SAMP-API multiver 6d4db99ab41f19d1a6a7c6cd48f5878bd1e14b62,
    // CNetGame.h/.cpp for each revision. Entry points: SAMP-API.lua init.lua.
    internal static readonly SampClientProfile R3 = new(SFSampVersion.R3_1,
        0xCC4D0, 0x26E8DC, 0x2C, 0x3DE, 0x3CD, 0x2E10, true);
    internal static readonly SampClientProfile R5 = new(SFSampVersion.R5_1,
        0xCBC90, 0x26EB94, 0, 0x3DE, 0x3CD, 0x2E30, true);

    internal static SampClientProfile? FromEntryPoint(uint rva) => rva switch
    {
        0xCC4D0 => R3,
        0xCBC90 => R5,
        _ => null,
    };

    internal SFSampGameState NormalizeState(int raw) => (Version, raw) switch
    {
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 1) => SFSampGameState.WaitingToConnect,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 2) => SFSampGameState.Connecting,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 5) => SFSampGameState.Connected,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 6) => SFSampGameState.WaitingToJoin,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 11) => SFSampGameState.Restarting,
        _ => SFSampGameState.Unknown,
    };
}
