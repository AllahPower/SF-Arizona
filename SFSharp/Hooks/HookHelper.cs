using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SFSharp;

public static unsafe class HookHelper
{
    public static uint GetFunctionPtr(string moduleName, uint offset)
    {
        return Win32.GetModuleHandle(moduleName) + offset;
    }

    public static uint InstallJumpHook(uint targetAddress, uint stolenByteCount, uint injectedFunctionPtr)
    {
        var trampolinePtr = Win32.VirtualAlloc(0, stolenByteCount + 5, MEM.COMMIT | MEM.RESERVE, PAGE.READWRITE);
        NativeMemory.Copy((void*)targetAddress, (void*)trampolinePtr, stolenByteCount);
        *(byte*)(trampolinePtr + stolenByteCount) = 0xE9;
        *(uint*)(trampolinePtr + stolenByteCount + 1) = (targetAddress + stolenByteCount) - (trampolinePtr + stolenByteCount + 5);
        Win32.VirtualProtect(trampolinePtr, stolenByteCount + 5, PAGE.EXECUTE_READ, out _);

        Win32.VirtualProtect(targetAddress, stolenByteCount, PAGE.READWRITE, out var oldProtect);
        *(byte*)targetAddress = 0xE9;
        *(uint*)(targetAddress + 1) = injectedFunctionPtr - (targetAddress + 5);
        Win32.VirtualProtect(targetAddress, stolenByteCount, oldProtect, out _);

        return trampolinePtr;
    }

    public static void RemoveJumpHook(uint targetAddress, uint stolenByteCount, uint trampolinePtr)
    {
        Win32.VirtualProtect(targetAddress, stolenByteCount, PAGE.READWRITE, out var oldProtect);
        NativeMemory.Copy((void*)trampolinePtr, (void*)targetAddress, stolenByteCount);
        Win32.VirtualProtect(targetAddress, stolenByteCount, oldProtect, out _);

        Win32.VirtualFree(trampolinePtr, stolenByteCount + 5, MEM.RELEASE);
    }

    public static uint InstallCallHook(uint targetAddress, uint stolenByteCount, uint injectedFunctionPtr)
    {
        var bufferPtr = (uint)NativeMemory.AllocZeroed(stolenByteCount);
        NativeMemory.Copy((void*)targetAddress, (void*)bufferPtr, stolenByteCount);

        Win32.VirtualProtect(targetAddress, stolenByteCount, PAGE.READWRITE, out var oldProtect);
        *(byte*)targetAddress = 0xE8;
        *(uint*)(targetAddress + 1) = injectedFunctionPtr - (targetAddress + 5);
        for (uint i = 5; i < stolenByteCount; i++)
        {
            *(byte*)(targetAddress + i) = 0x90;
        }
        Win32.VirtualProtect(targetAddress, stolenByteCount, oldProtect, out _);

        return bufferPtr;
    }

    public static void RemoveCallHook(uint targetAddress, uint stolenByteCount, uint originalByteBuffer)
    {
        Win32.VirtualProtect(targetAddress, stolenByteCount, PAGE.READWRITE, out var oldProtect);
        NativeMemory.Copy((void*)originalByteBuffer, (void*)targetAddress, stolenByteCount);
        Win32.VirtualProtect(targetAddress, stolenByteCount, oldProtect, out _);
    }
}

public static unsafe partial class Win32
{
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    internal static partial uint GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

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

internal enum MEM : uint
{
    COMMIT = 0x00001000,
    RESERVE = 0x00002000,
    RESET = 0x00080000,
    RESET_UNDO = 0x01000000,

    LARGE_PAGES = 0x20000000,
    PHYSICAL = 0x00400000,
    TOP_DOWN = 0x00100000,
    WRITE_WATCH = 0x00200000,

    DECOMMIT = 0x00004000,
    RELEASE = 0x00008000,

    COALESCE_PLACEHOLDERS = 0x00000001,
    PRESERVE_PLACEHOLDER = 0x00000002
}

internal enum PAGE : uint
{
    EXECUTE = 0x10,
    EXECUTE_READ = 0x20,
    EXECUTE_READWRITE = 0x40,
    EXECUTE_WRITECOPY = 0x80,
    NOACCESS = 0x01,
    READONLY = 0x02,
    READWRITE = 0x04,
    WRITECOPY = 0x08,
    GUARD = 0x100,
    NOCACHE = 0x200,
    WRITECOMBINE = 0x400,
}
