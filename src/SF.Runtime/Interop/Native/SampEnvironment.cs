namespace SFSharp.Runtime.Interop.Native;

/// <summary>
/// Detects the SA-MP client version and companion modules at runtime
/// using PE header fingerprints and known in-memory markers.
/// </summary>
public static unsafe class SampEnvironment
{
    // samp.dll R3: DllEntryPoint RVA confirmed in IDA.
    private const uint SampR3EntryPoint = 0xCC4D0;

    // samp.dll R3: null-terminated ASCII "0.3.7-R3" in .rdata.
    private const uint SampR3VersionStringRva = 0xE5B8C;

    // sampfuncs.asi 5.5.0 rel.22: DllEntryPoint RVA confirmed in IDA.
    private const uint SampfuncsExpectedEntryPoint = 0x9ACBC;

    // sampfuncs.asi 5.5.0: null-terminated ASCII "SAMPFUNCS v5.5.0 rel.22 (SA-MP 0.3.7 R3-1)" in .rdata.
    private const uint SampfuncsVersionStringRva = 0xD1A84;
    private const string SampfuncsExpectedVersionPrefix = "SAMPFUNCS v5.5.0";

    private static SampVersionInfo? _cached;

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

        uint sfBase = Win32.GetModuleHandle("sampfuncs.asi");
        SampfuncsInfo sfInfo = DetectSampfuncs(sfBase);

        SampVersionInfo info = new(
            version,
            sampBase,
            sampPe.EntryPointRva,
            sampPe.SizeOfImage,
            sampPe.TimeDateStamp,
            sampPe.SizeOfCode,
            sfInfo);

        _cached = info;
        return info;
    }

    public static SampVersionInfo? GetOrDetect() => _cached ?? Detect();

    public static bool IsSampR3 => GetOrDetect()?.Version == SampVersion.R3_037;

    public static bool IsSampfuncsLoaded => GetOrDetect()?.Sampfuncs.IsLoaded ?? false;

    private static SampVersion ClassifySampVersion(uint moduleBase, PeFingerprint pe)
    {
        if (pe.EntryPointRva == SampR3EntryPoint && VerifyAsciiString(moduleBase + SampR3VersionStringRva, "0.3.7-R3"))
            return SampVersion.R3_037;

        if (VerifyAsciiString(moduleBase + SampR3VersionStringRva, "0.3.7-R3"))
            return SampVersion.R3_037_Patched;

        return SampVersion.Unknown;
    }

    private static SampfuncsInfo DetectSampfuncs(uint moduleBase)
    {
        if (moduleBase == 0)
            return SampfuncsInfo.NotLoaded;

        PeFingerprint pe = ReadPeFingerprint(moduleBase);

        SampfuncsVersion sfVersion;
        if (pe.EntryPointRva == SampfuncsExpectedEntryPoint
            && VerifyAsciiPrefix(moduleBase + SampfuncsVersionStringRva, SampfuncsExpectedVersionPrefix))
        {
            sfVersion = SampfuncsVersion.V5_5_0;
        }
        else if (VerifyAsciiPrefix(moduleBase + SampfuncsVersionStringRva, "SAMPFUNCS v"))
        {
            sfVersion = SampfuncsVersion.OtherVersion;
        }
        else
        {
            sfVersion = SampfuncsVersion.Unknown;
        }

        string? versionString = TryReadAsciiString(moduleBase + SampfuncsVersionStringRva, 64);

        return new SampfuncsInfo(
            true,
            sfVersion,
            moduleBase,
            pe.EntryPointRva,
            pe.SizeOfImage,
            pe.TimeDateStamp,
            versionString);
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

        int peOffset = *(int*)(basePtr + 0x3C);
        byte* ntHeaders = basePtr + peOffset;
        if (!NativeMemoryValidator.IsReadable((nint)ntHeaders, 0x78))
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
    R3_037_Patched
}

public enum SampfuncsVersion
{
    Unknown,
    V5_5_0,
    OtherVersion
}

public readonly record struct SampfuncsInfo(
    bool IsLoaded,
    SampfuncsVersion Version,
    uint ModuleBase,
    uint EntryPointRva,
    uint SizeOfImage,
    uint TimeDateStamp,
    string? VersionString)
{
    public static SampfuncsInfo NotLoaded => default;
    public bool IsSupported => Version == SampfuncsVersion.V5_5_0;
}

public sealed record SampVersionInfo(
    SampVersion Version,
    uint ModuleBase,
    uint EntryPointRva,
    uint SizeOfImage,
    uint TimeDateStamp,
    uint SizeOfCode,
    SampfuncsInfo Sampfuncs)
{
    public bool IsSupported => Version is SampVersion.R3_037 or SampVersion.R3_037_Patched;
}
