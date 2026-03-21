using SFSharp;
using System;
using System.Runtime.InteropServices;

using unsafe ChatDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, byte*, void>;
using unsafe GetColorAsArgbDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, uint>;
using unsafe GetColorAsRgbaDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, uint>;
using unsafe GetPedDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, CPed*>;
using unsafe GetSpecialActionDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, byte>;
using unsafe RequestSpawnDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, void>;
using unsafe SetColorDelegate = delegate* unmanaged[Thiscall]<CLocalPlayer*, uint, void>;

[StructLayout(LayoutKind.Explicit, Size = 812, Pack = 1)]
public unsafe ref struct CLocalPlayer
{
    private static CLocalPlayer* CurrentInstance => CPlayerPool.Instance.GetLocalPlayer();
    public static ref readonly CLocalPlayer Instance => ref *RequireInstance();

    [FieldOffset(389)]
    public WeaponsData WeaponsData;

    public ushort AimedPlayerId => WeaponsData.AimedPlayer;
    public ushort AimedActorId => WeaponsData.AimedActor;
    public byte CurrentWeapon => WeaponsData.CurrentWeapon;

    private static readonly GetPedDelegate _getPed = (GetPedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.GetPed);
    private static readonly GetSpecialActionDelegate _getSpecialAction = (GetSpecialActionDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.GetSpecialAction);
    private static readonly SetColorDelegate _setColor = (SetColorDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.SetColor);
    private static readonly GetColorAsRgbaDelegate _getColorAsRgba = (GetColorAsRgbaDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.GetColorAsRgba);
    private static readonly GetColorAsArgbDelegate _getColorAsArgb = (GetColorAsArgbDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.GetColorAsArgb);
    private static readonly RequestSpawnDelegate _requestSpawn = (RequestSpawnDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.RequestSpawn);
    private static readonly ChatDelegate _chat = (ChatDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLocalPlayer.Chat);

    public CPed* GetPed()
    {
        return _getPed(RequireInstance());
    }

    public byte GetSpecialAction()
    {
        return _getSpecialAction(RequireInstance());
    }

    public void SetColor(uint colorArgb)
    {
        _setColor(RequireInstance(), colorArgb);
    }

    public uint GetColorAsRgba()
    {
        return _getColorAsRgba(RequireInstance());
    }

    public uint GetColorAsArgb()
    {
        return _getColorAsArgb(RequireInstance());
    }

    public void RequestSpawn()
    {
        _requestSpawn(RequireInstance());
    }

    public int GetWeaponAmmo(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, WeaponsData.WeaponSlotCount);
        fixed (int* ammo = WeaponsData.LastWeaponAmmo)
        {
            return ammo[slot];
        }
    }

    public byte GetWeaponId(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, WeaponsData.WeaponSlotCount);
        fixed (byte* weapons = WeaponsData.LastWeapon)
        {
            return weapons[slot];
        }
    }

    public void Chat(string text)
    {
        using var textAnsi = AnsiString.Encode(text);
        _chat(RequireInstance(), textAnsi);
    }

    private static CLocalPlayer* RequireInstance()
    {
        CLocalPlayer* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CLocalPlayer instance is not available.");
        }

        return instance;
    }
}

public unsafe struct CPed;

public unsafe struct WeaponsData
{
    public const int WeaponSlotCount = 13;

    public ushort AimedPlayer;
    public ushort AimedActor;
    public byte CurrentWeapon;
    public fixed byte LastWeapon[WeaponSlotCount];
    public fixed int LastWeaponAmmo[WeaponSlotCount];
}
