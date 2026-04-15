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

    public static nint FindPattern(string moduleName, ReadOnlySpan<byte?> pattern)
    {
        if (pattern.IsEmpty)
        {
            return 0;
        }

        nint moduleBase = (nint)Win32.GetModuleHandle(moduleName);
        if (moduleBase == 0)
        {
            return 0;
        }

        uint moduleSize = GetModuleSize((byte*)moduleBase);
        if (moduleSize < pattern.Length)
        {
            return 0;
        }

        byte* start = (byte*)moduleBase;
        nuint lastOffset = moduleSize - (uint)pattern.Length;
        for (nuint offset = 0; offset <= lastOffset; offset++)
        {
            byte* cursor = start + offset;
            bool matched = true;
            for (int i = 0; i < pattern.Length; i++)
            {
                byte? expected = pattern[i];
                if (expected.HasValue && cursor[i] != expected.Value)
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return (nint)cursor;
            }
        }

        return 0;
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
        SFLog.Debug($"ResolveVTableFunction module={moduleName} instance=0x{instanceOffset:X} field=0x{objectFieldOffset:X} vtableIdx={vtableIndex} -> 0x{funcAddr:X8}");
        return funcAddr;
    }

    private static uint GetModuleSize(byte* moduleBase)
    {
        int peHeaderOffset = *(int*)(moduleBase + 0x3C);
        byte* ntHeaders = moduleBase + peHeaderOffset;
        const int OptionalHeaderOffset = 0x18;
        const int SizeOfImageOffset = 0x38;
        return *(uint*)(ntHeaders + OptionalHeaderOffset + SizeOfImageOffset);
    }
}
