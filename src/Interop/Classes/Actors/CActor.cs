using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x56, Pack = 1)]
public unsafe struct CActor
{
    [FieldOffset(0x00)]
    private CEntity _entity;

    [FieldOffset(0x48)]
    private nint _gamePed;

    [FieldOffset(0x4C)]
    private int _marker;

    [FieldOffset(0x50)]
    private int _arrow;

    [FieldOffset(0x54)]
    private byte _needsToCreateMarker;

    [FieldOffset(0x55)]
    private byte _invulnerable;

    private static readonly delegate* unmanaged[Thiscall]<CActor*, void> _destroy = (delegate* unmanaged[Thiscall]<CActor*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActor.Destroy);
    private static readonly delegate* unmanaged[Thiscall]<CActor*, byte*, byte*, float, int, int, int, int, int, void> _performAnimation = (delegate* unmanaged[Thiscall]<CActor*, byte*, byte*, float, int, int, int, int, int, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActor.PerformAnimation);
    private static readonly delegate* unmanaged[Thiscall]<CActor*, float, void> _setRotation = (delegate* unmanaged[Thiscall]<CActor*, float, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActor.SetRotation);
    private static readonly delegate* unmanaged[Thiscall]<CActor*, float> _getHealth = (delegate* unmanaged[Thiscall]<CActor*, float>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActor.GetHealth);
    private static readonly delegate* unmanaged[Thiscall]<CActor*, float, void> _setHealth = (delegate* unmanaged[Thiscall]<CActor*, float, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActor.SetHealth);
    private static readonly delegate* unmanaged[Thiscall]<CActor*, byte, void> _setInvulnerable = (delegate* unmanaged[Thiscall]<CActor*, byte, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActor.SetInvulnerable);

    public CEntity Entity => _entity;
    public nint GamePed => _gamePed;
    public int Marker => _marker;
    public int Arrow => _arrow;
    public bool NeedsToCreateMarker => _needsToCreateMarker != 0;
    public bool IsInvulnerable => _invulnerable != 0;

    public CEntity* GetEntityPointer()
    {
        return (CEntity*)Unsafe.AsPointer(ref this);
    }

    public float GetHealth()
    {
        return _getHealth((CActor*)Unsafe.AsPointer(ref this));
    }

    public void SetHealth(float health)
    {
        _setHealth((CActor*)Unsafe.AsPointer(ref this), health);
    }

    public void SetRotation(float angle)
    {
        _setRotation((CActor*)Unsafe.AsPointer(ref this), angle);
    }

    public void SetInvulnerable(bool enable)
    {
        _setInvulnerable((CActor*)Unsafe.AsPointer(ref this), enable ? (byte)1 : (byte)0);
    }

    public void Destroy()
    {
        _destroy((CActor*)Unsafe.AsPointer(ref this));
    }

    public void PerformAnimation(string animationName, string ifpName, float frameDelta, bool lockA, bool lockX, bool lockY, bool lockF, int time)
    {
        using AnsiString animationAnsi = AnsiString.Encode(animationName);
        using AnsiString ifpAnsi = AnsiString.Encode(ifpName);
        _performAnimation(
            (CActor*)Unsafe.AsPointer(ref this),
            animationAnsi,
            ifpAnsi,
            frameDelta,
            lockA ? 1 : 0,
            lockX ? 1 : 0,
            lockY ? 1 : 0,
            lockF ? 1 : 0,
            time);
    }
}
