using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Native;

/// <summary>Groups the verified client identity and the CNetGame layout used during bootstrap.</summary>
/// <remarks>RVA values are relative to samp.dll; field offsets are relative to a CNetGame instance.
/// Recognition does not prove in-game behavior. See docs/SAMP_CLIENTS.md for verification status.</remarks>
internal sealed record SampClientProfile(
    SFSampVersion Version, uint EntryPointRva, uint NetGameRva,
    int RakClientOffset, int PoolsOffset, int StateOffset, uint GetStateRva,
    bool IsRuntimeSupported)
{
    // SAMP-API multiver 6d4db99ab41f19d1a6a7c6cd48f5878bd1e14b62,
    // CNetGame.h/.cpp for each revision. Entry points: SAMP-API.lua init.lua.
    internal static readonly SampClientProfile R1 = new(SFSampVersion.R1,
        0x31DF13, 0x21A0F8, 0x3C9, 0x3CD, 0x3BD, 0x2E20, true);
    internal static readonly SampClientProfile R3 = new(SFSampVersion.R3_1,
        0xCC4D0, 0x26E8DC, 0x2C, 0x3DE, 0x3CD, 0x2E10, true);
    internal static readonly SampClientProfile R5 = new(SFSampVersion.R5_1,
        0xCBC90, 0x26EB94, 0, 0x3DE, 0x3CD, 0x2E30, true);

    internal static SampClientProfile? FromEntryPoint(uint rva) => rva switch
    {
        0x31DF13 => R1,
        0xCC4D0 => R3,
        0xCBC90 => R5,
        _ => null,
    };

    internal SampVersion DetectedVersion => Version switch
    {
        SFSampVersion.R1 => SampVersion.R1_037,
        SFSampVersion.R3_1 => SampVersion.R3_037,
        SFSampVersion.R5_1 => SampVersion.R5_037,
        _ => SampVersion.Unknown,
    };

    internal int NetGameReadinessSize => Math.Max(RakClientOffset, PoolsOffset) + IntPtr.Size;

    /// <summary>Checks the actual RakClient and pool pointers, not the first word of CNetGame.</summary>
    /// <remarks>The caller owns the readable native span. This check does not extend its lifetime
    /// or make concurrent native destruction safe; call it on the game thread.</remarks>
    internal bool IsNetGameReady(ReadOnlySpan<byte> memory) =>
        memory.Length >= NetGameReadinessSize
        && MemoryMarshal.Read<nint>(memory[RakClientOffset..]) != 0
        && MemoryMarshal.Read<nint>(memory[PoolsOffset..]) != 0;

    internal SFSampGameState NormalizeState(int raw) => (Version, raw) switch
    {
        (SFSampVersion.R1, 9) => SFSampGameState.WaitingToConnect,
        (SFSampVersion.R1, 13) => SFSampGameState.Connecting,
        (SFSampVersion.R1, 14) => SFSampGameState.Connected,
        (SFSampVersion.R1, 15) => SFSampGameState.WaitingToJoin,
        (SFSampVersion.R1, 18) => SFSampGameState.Restarting,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 1) => SFSampGameState.WaitingToConnect,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 2) => SFSampGameState.Connecting,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 5) => SFSampGameState.Connected,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 6) => SFSampGameState.WaitingToJoin,
        (SFSampVersion.R3_1 or SFSampVersion.R5_1, 11) => SFSampGameState.Restarting,
        _ => SFSampGameState.Unknown,
    };
}
