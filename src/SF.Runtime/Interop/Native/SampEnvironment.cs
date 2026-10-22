using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Native;

/// <summary>
/// Identifies the loaded SA-MP client from its PE header and exact version marker.
/// </summary>
/// <remarks>The first detected module stays bound for the process lifetime. Native views and
/// function pointers must not change revision while hooks are installed.</remarks>
public static unsafe class SampEnvironment
{
    private const uint R1VersionStringRva = 0xD3B84;
    private const uint R3R5VersionStringRva = 0xE5B8C;

    private static DetectionSnapshot? _detected;
    private static readonly SFSampClientInfo UnknownClient = new(SFSampVersion.Unknown, SFSampCapabilities.None);

    public static SFSampClientInfo Client => Volatile.Read(ref _detected)?.Client ?? UnknownClient;

    public static SampVersionInfo? Current => Volatile.Read(ref _detected)?.Info;

    /// <summary>Gets the verified layout or refuses native access before any field is read.</summary>
    internal static SampClientProfile RequiredProfile => GetOrDetect()?.Profile is { IsRuntimeSupported: true } profile
        ? profile
        : throw new NotSupportedException("No verified native profile for the loaded SA-MP client.");

    /// <summary>
    /// Performs full environment detection. First successful result is cached.
    /// </summary>
    public static SampVersionInfo? Detect()
    {
        if (Current is { } cached)
            return cached;

        uint sampBase = Win32.GetModuleHandle("samp.dll");
        if (sampBase == 0)
            return null;

        PeFingerprint sampPe = ReadPeFingerprint(sampBase);
        SampVersion version = ClassifySampVersion(sampBase, sampPe);

        SampVersionInfo info = new(
            version,
            sampBase,
            sampPe.EntryPointRva,
            sampPe.SizeOfImage,
            sampPe.TimeDateStamp,
            sampPe.SizeOfCode);

        DetectionSnapshot candidate = DetectionSnapshot.Create(info);
        // Publish identity and capabilities together. Concurrent probes use the same winning snapshot.
        return (Interlocked.CompareExchange(ref _detected, candidate, null) ?? candidate).Info;
    }

    public static SampVersionInfo? GetOrDetect() => Current ?? Detect();

    public static bool IsSampR3 => GetOrDetect()?.Version == SampVersion.R3_037;
    internal static bool IsR5 => RequiredProfile.Version == SFSampVersion.R5_1;
    internal static bool IsR1 => RequiredProfile.Version == SFSampVersion.R1;


    private static SampVersion ClassifySampVersion(uint moduleBase, PeFingerprint pe)
    {
        return Identify(pe.EntryPointRva, pe.SizeOfImage,
            VerifyAsciiString(moduleBase + R3R5VersionStringRva, "0.3.7-R3"),
            VerifyAsciiString(moduleBase + R3R5VersionStringRva, "0.3.7-R5"),
            VerifyAsciiString(moduleBase + R1VersionStringRva, "0.3.7-R1"));
    }

    internal static SampVersion Identify(uint entryPoint, uint imageSize, bool r3Marker, bool r5Marker, bool r1Marker = false)
    {
        SampClientProfile? profile = SampClientProfile.FromEntryPoint(entryPoint);
        if (profile is null || imageSize < profile.NetGameRva + sizeof(uint) || entryPoint >= imageSize)
            return SampVersion.Unknown;
        return profile.Version switch
        {
            SFSampVersion.R1 when r1Marker && !r3Marker && !r5Marker => SampVersion.R1_037,
            SFSampVersion.R3_1 when r3Marker && !r5Marker && !r1Marker => SampVersion.R3_037,
            SFSampVersion.R5_1 when r5Marker && !r3Marker && !r1Marker => SampVersion.R5_037,
            _ => SampVersion.Unknown,
        };
    }

    private static bool VerifyAsciiString(uint address, string expected)
    {
        byte* ptr = (byte*)address;
        if (!NativeMemoryValidator.IsReadable((nint)ptr, (nuint)(expected.Length + 1)))
            return false;

        for (int i = 0; i < expected.Length; i++)
        {
            if (ptr[i] != (byte)expected[i])
                return false;
        }

        return ptr[expected.Length] == 0;
    }

    private static PeFingerprint ReadPeFingerprint(uint moduleBase)
    {
        byte* basePtr = (byte*)moduleBase;
        if (!NativeMemoryValidator.IsReadable((nint)basePtr, 0x40))
            return default;

        if (MemoryMarshal.Read<ushort>(new ReadOnlySpan<byte>(basePtr, 2)) != 0x5A4D)
            return default;
        int peOffset = MemoryMarshal.Read<int>(new ReadOnlySpan<byte>(basePtr + 0x3C, 4));
        if (peOffset < 0x40 || peOffset > 0x100000)
            return default;
        byte* ntHeaders = basePtr + peOffset;
        if (!NativeMemoryValidator.IsReadable((nint)ntHeaders, 0x78))
            return default;

        if (MemoryMarshal.Read<uint>(new ReadOnlySpan<byte>(ntHeaders, 4)) != 0x4550
            || MemoryMarshal.Read<ushort>(new ReadOnlySpan<byte>(ntHeaders + 4, 2)) != 0x14C
            || MemoryMarshal.Read<ushort>(new ReadOnlySpan<byte>(ntHeaders + 0x18, 2)) != 0x10B)
            return default;

        uint timeDateStamp = *(uint*)(ntHeaders + 0x08);
        byte* optionalHeader = ntHeaders + 0x18;
        uint sizeOfCode = *(uint*)(optionalHeader + 0x04);
        uint entryPointRva = *(uint*)(optionalHeader + 0x10);
        uint sizeOfImage = *(uint*)(optionalHeader + 0x38);

        return new PeFingerprint(entryPointRva, sizeOfImage, timeDateStamp, sizeOfCode);
    }

    private readonly record struct PeFingerprint(
        uint EntryPointRva,
        uint SizeOfImage,
        uint TimeDateStamp,
        uint SizeOfCode);

    /// <summary>One immutable publication unit for native bindings and plugin-visible capabilities.</summary>
    internal sealed record DetectionSnapshot(SampVersionInfo Info, SFSampClientInfo Client)
    {
        internal static DetectionSnapshot Create(SampVersionInfo info)
        {
            SampClientProfile? profile = info.Profile;
            SFSampCapabilities capabilities = profile is { IsRuntimeSupported: true }
                ? SFSampCapabilities.Chat | SFSampCapabilities.Dialogs | SFSampCapabilities.Pools
                    | SFSampCapabilities.Network | SFSampCapabilities.IncomingEmulation | SFSampCapabilities.CompressedStrings
                : SFSampCapabilities.None;
            return new(info, new(profile?.Version ?? SFSampVersion.Unknown, capabilities));
        }
    }
}

public enum SampVersion
{
    Unknown,
    R3_037,
    R5_037,
    R1_037,
}

public sealed record SampVersionInfo(
    SampVersion Version,
    uint ModuleBase,
    uint EntryPointRva,
    uint SizeOfImage,
    uint TimeDateStamp,
    uint SizeOfCode)
{
    internal SampClientProfile? Profile => SampClientProfile.FromEntryPoint(EntryPointRva) is { } profile
        && profile.DetectedVersion == Version
        && EntryPointRva < SizeOfImage
        && SizeOfImage >= profile.NetGameRva + sizeof(uint)
            ? profile
            : null;
    public bool IsSupported => Profile?.IsRuntimeSupported == true;
}
