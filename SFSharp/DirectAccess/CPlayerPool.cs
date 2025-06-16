using SFSharp;
using System.Runtime.InteropServices;

using unsafe GetLocalPlayerDelegate = delegate* unmanaged[Thiscall]<CPlayerPool*, CLocalPlayer*>;

[StructLayout(LayoutKind.Explicit, Size = 16126, Pack = 1)]
public unsafe struct CPlayerPool
{
    private static readonly CPlayerPool* _instance = CNetGame.Instance.GetPlayerPool();
    public static ref CPlayerPool Instance => ref *_instance;

    private static readonly GetLocalPlayerDelegate _getLocalPlayer = (GetLocalPlayerDelegate)Win32.GetSampAddress(0x1A40);
    public CLocalPlayer* GetLocalPlayer()
    {
        return _getLocalPlayer(_instance);
    }
}
