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
}
