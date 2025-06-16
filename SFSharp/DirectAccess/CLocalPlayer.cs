using SFSharp;
using System.Runtime.InteropServices;

using unsafe ChatDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, byte*, void>;

[StructLayout(LayoutKind.Explicit, Size = 812, Pack = 1)]
public unsafe struct CLocalPlayer
{
    private static readonly CLocalPlayer* _instance = CPlayerPool.Instance.GetLocalPlayer();
    public static ref CLocalPlayer Instance => ref *_instance;

    [FieldOffset(393)]
    public WeaponsData WeaponsData;

    private static readonly ChatDelegate _chat = (ChatDelegate)Win32.GetSampAddress(0x5A10);
    public void Chat(string text)
    {
        using var textAnsi = AnsiString.Encode(text);
        _chat(_instance, textAnsi);
    }
}

public unsafe struct WeaponsData
{
    public ushort AimedPlayer;
    public ushort AimedActor;
    public byte CurrentWeapon;
    public fixed byte LastWeapon[13];
    public fixed int LastWeaponAmmo[13];
}