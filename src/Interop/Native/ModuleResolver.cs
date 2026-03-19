namespace SFSharp;

public static unsafe class ModuleResolver
{
    public static nint GetProcAddress(string moduleName, uint offset)
    {
        return (nint)(Win32.GetModuleHandle(moduleName) + offset);
    }

    public static bool IsModuleLoaded(string moduleName)
    {
        return Win32.GetModuleHandle(moduleName) != 0;
    }

    public static bool IsClassReady(string moduleName, uint offset)
    {
        var moduleHandle = Win32.GetModuleHandle(moduleName);
        if (moduleHandle == 0)
        {
            return false;
        }

        var classPtr = (uint**)(moduleHandle + offset);
        if (*classPtr is null)
        {
            return false;
        }

        return **classPtr != 0;
    }

    /// <summary>
    /// Resolves a concrete function address from a C++ object's vtable.
    /// Dereferences: module+instanceOffset -> object -> +objectFieldOffset -> target -> vtable -> vtable[vtableIndex].
    /// </summary>
    public static nint ResolveVTableFunction(string moduleName, int instanceOffset, int objectFieldOffset, int vtableIndex)
    {
        nint moduleBase = (nint)Win32.GetModuleHandle(moduleName);
        nint instancePtr = *(nint*)(moduleBase + instanceOffset);
        nint objectPtr = *(nint*)(instancePtr + objectFieldOffset);
        nint vtable = *(nint*)objectPtr;
        nint funcAddr = *(nint*)(vtable + vtableIndex * 4);
        SFLog.Info($"ResolveVTableFunction module={moduleName} instance=0x{instanceOffset:X} field=0x{objectFieldOffset:X} vtableIdx={vtableIndex} -> 0x{funcAddr:X8}");
        return funcAddr;
    }
}
