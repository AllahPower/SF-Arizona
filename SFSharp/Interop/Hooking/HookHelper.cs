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

    public static bool IsClassReady(string moduleName, uint offset)
    {
        var moduleHandle = Win32.GetModuleHandle(moduleName);
        if (moduleHandle == 0) return false;
        var classPtr = (uint**)(moduleHandle + offset);
        if (*classPtr == null) return false;
        if (**classPtr == 0) return false;
        return true;
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
        for (uint i = 5; i < stolenByteCount; i++)
        {
            *(byte*)(targetAddress + i) = 0x90; // NOP
        }
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