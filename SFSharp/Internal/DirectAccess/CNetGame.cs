using SFSharp;
using System.Runtime.InteropServices;

using unsafe GetPlayerPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CPlayerPool*>;

[StructLayout(LayoutKind.Explicit, Size = 1006, Pack = 1)]
public unsafe struct CNetGame
{
    private static readonly CNetGame* _instance = *(CNetGame**)HookHelper.GetFunctionPtr("samp.dll", 0x26EB94);
    public static ref CNetGame Instance => ref *_instance;

    private static readonly GetPlayerPoolDelegate _getPlayerPool = (GetPlayerPoolDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x1170);
    public CPlayerPool* GetPlayerPool()
    {
        return _getPlayerPool(_instance);
    }
}
