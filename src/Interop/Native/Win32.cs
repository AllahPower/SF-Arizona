using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp.Runtime.Interop.Native;

public static unsafe partial class Win32
{
    private const uint GMEM_MOVEABLE = 0x0002;
    private const uint CF_UNICODETEXT = 13;

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

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetKeyboardState(ref byte keyStates);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenClipboard(nint hWndNewOwner);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseClipboard();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EmptyClipboard();

    [LibraryImport("user32.dll")]
    private static partial nint SetClipboardData(uint uFormat, nint hMem);

    [LibraryImport("kernel32.dll")]
    private static partial nint GlobalAlloc(uint uFlags, nuint dwBytes);

    [LibraryImport("kernel32.dll")]
    private static partial nint GlobalLock(nint hMem);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalUnlock(nint hMem);

    [LibraryImport("kernel32.dll")]
    private static partial nint GlobalFree(nint hMem);

    public static bool TrySetClipboardText(string text)
    {
        if (!OpenClipboard(0))
        {
            return false;
        }

        try
        {
            if (!EmptyClipboard())
            {
                return false;
            }

            var bytes = Encoding.Unicode.GetBytes(text + "\0");
            var globalMemory = GlobalAlloc(GMEM_MOVEABLE, (nuint)bytes.Length);
            if (globalMemory == 0)
            {
                return false;
            }

            var memoryPointer = GlobalLock(globalMemory);
            if (memoryPointer == 0)
            {
                GlobalFree(globalMemory);
                return false;
            }

            try
            {
                Marshal.Copy(bytes, 0, memoryPointer, bytes.Length);
            }
            finally
            {
                GlobalUnlock(globalMemory);
            }

            if (SetClipboardData(CF_UNICODETEXT, globalMemory) == 0)
            {
                GlobalFree(globalMemory);
                return false;
            }

            return true;
        }
        finally
        {
            CloseClipboard();
        }
    }
}
