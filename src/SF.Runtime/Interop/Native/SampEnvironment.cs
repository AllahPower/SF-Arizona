using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Native;

/// <summary>
/// Detects the SA-MP client version and companion modules at runtime
/// using PE header fingerprints and known in-memory markers.
/// </summary>
public static unsafe class SampEnvironment
{
    // samp.dll R3: null-terminated ASCII "0.3.7-R3" in .rdata.
    private const uint SampR3VersionStringRva = 0xE5B8C;

    private static SampVersionInfo? _cached;
    private static SFSampClientInfo _client = new(SFSampVersion.Unknown, SFSampCapabilities.None);

    public static SFSampClientInfo Client => Volatile.Read(ref _client);

    public static SampVersionInfo? Current => _cached;

    /// <summary>
    /// Performs full environment detection. First successful result is cached.
    /// </summary>
    public static SampVersionInfo? Detect()
    {
        if (_cached is not null)
            return _cached;

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

        SFSampCapabilities capabilities = info.IsSupported
            ? SFSampCapabilities.Chat | SFSampCapabilities.Dialogs | SFSampCapabilities.Pools
                | SFSampCapabilities.Network | SFSampCapabilities.IncomingEmulation | SFSampCapabilities.CompressedStrings
            : SFSampCapabilities.None;
        Volatile.Write(ref _client, new(info.Profile?.Version ?? SFSampVersion.Unknown, capabilities));
        Volatile.Write(ref _cached, info);
        return info;
    }

    public static SampVersionInfo? GetOrDetect() => _cached ?? Detect();

    public static bool IsSampR3 => GetOrDetect()?.Version == SampVersion.R3_037;
    internal static bool IsR5 => Current?.Version == SampVersion.R5_037;


    private static SampVersion ClassifySampVersion(uint moduleBase, PeFingerprint pe)
    {
        return Identify(pe.EntryPointRva, pe.SizeOfImage,
            VerifyAsciiString(moduleBase + SampR3VersionStringRva, "0.3.7-R3"),
            VerifyAsciiString(moduleBase + SampR3VersionStringRva, "0.3.7-R5"));
    }

    internal static SampVersion Identify(uint entryPoint, uint imageSize, bool r3Marker, bool r5Marker)
    {
        SampClientProfile? profile = SampClientProfile.FromEntryPoint(entryPoint);
        if (profile is null || imageSize < profile.NetGameRva + sizeof(uint) || entryPoint >= imageSize)
            return SampVersion.Unknown;
        return profile.Version switch
        {
            SFSampVersion.R3_1 when r3Marker && !r5Marker => SampVersion.R3_037,
            SFSampVersion.R5_1 when r5Marker && !r3Marker => SampVersion.R5_037,
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

    private static bool VerifyAsciiPrefix(uint address, string prefix)
    {
        byte* ptr = (byte*)address;
        if (!NativeMemoryValidator.IsReadable((nint)ptr, (nuint)prefix.Length))
            return false;

        for (int i = 0; i < prefix.Length; i++)
        {
            if (ptr[i] != (byte)prefix[i])
                return false;
        }

        return true;
    }

    private static string? TryReadAsciiString(uint address, int maxLength)
    {
        byte* ptr = (byte*)address;
        if (!NativeMemoryValidator.IsReadable((nint)ptr, 1))
            return null;

        int len = 0;
        while (len < maxLength && NativeMemoryValidator.IsReadable((nint)(ptr + len), 1) && ptr[len] != 0)
            len++;

        if (len == 0)
            return null;

        return new string((sbyte*)ptr, 0, len);
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
}

public enum SampVersion
{
    Unknown,
    R3_037,
    R3_037_Patched,
    R5_037,
}

public sealed record SampVersionInfo(
    SampVersion Version,
    uint ModuleBase,
    uint EntryPointRva,
    uint SizeOfImage,
    uint TimeDateStamp,
    uint SizeOfCode)
{
    internal SampClientProfile? Profile => Version == SampVersion.Unknown ? null : SampClientProfile.FromEntryPoint(EntryPointRva);
    public bool IsSupported => Profile?.IsRuntimeSupported == true;
}
