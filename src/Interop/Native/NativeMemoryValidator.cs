using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

internal static partial class NativeMemoryValidator
{
    private const uint MemCommit = 0x1000;
    private const uint PageNoAccess = 0x01;
    private const uint PageGuard = 0x100;
    private const uint ReadableProtectionMask =
        0x02 |
        0x04 |
        0x08 |
        0x20 |
        0x40 |
        0x80;

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryBasicInformation
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public nuint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [LibraryImport("kernel32.dll", SetLastError = false)]
    private static partial nuint VirtualQuery(nint address, out MemoryBasicInformation buffer, nuint length);

    public static bool IsReadable(nint address, nuint minBytes = 1)
    {
        if (address == 0 || minBytes == 0)
        {
            return false;
        }

        nuint result = VirtualQuery(address, out MemoryBasicInformation mbi, (nuint)Marshal.SizeOf<MemoryBasicInformation>());
        if (result == 0 || mbi.State != MemCommit)
        {
            return false;
        }

        uint protect = mbi.Protect & 0xFFu;
        if ((mbi.Protect & PageGuard) != 0 || protect == PageNoAccess || (protect & ReadableProtectionMask) == 0)
        {
            return false;
        }

        nuint start = (nuint)address;
        nuint regionStart = (nuint)mbi.BaseAddress;
        nuint regionEnd = regionStart + mbi.RegionSize;
        nuint end = start + minBytes;
        return start >= regionStart && end <= regionEnd && end >= start;
    }
}
