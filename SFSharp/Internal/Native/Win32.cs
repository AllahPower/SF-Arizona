using System.Runtime.InteropServices;

namespace SFSharp;

public static unsafe partial class Win32
{
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    internal static partial uint GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string? lpModuleName);

    [LibraryImport("kernel32.dll")]
    internal static partial uint VirtualAlloc(uint lpAddress, uint dwSize, MEM flAllocationType, PAGE flProtect);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool VirtualProtect(uint lpAddress, uint dwSize, PAGE flNewProtect, out PAGE lpflOldProtect);

    [LibraryImport("kernel32.dll")]
    internal static partial void VirtualFree(uint lpAddress, uint dwSize, MEM dwFreeType);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool FlushInstructionCache(nint hProcess, uint lpBaseAddress, uint dwSize);
}
