using SFSharp;
using System.Runtime.InteropServices;

using unsafe ChatDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, byte*, void>;

[StructLayout(LayoutKind.Explicit, Size = 812, Pack = 1)]
public unsafe ref struct CLocalPlayer
{
    private static readonly CLocalPlayer* _instance = CPlayerPool.Instance.GetLocalPlayer();
    public static ref readonly CLocalPlayer Instance => ref *_instance;

    [FieldOffset(389)]
    public WeaponsData WeaponsData;

    private static readonly ChatDelegate _chat = (ChatDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.Chat);
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
