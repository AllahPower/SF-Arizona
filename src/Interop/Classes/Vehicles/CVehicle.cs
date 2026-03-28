using SFSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe DoesExistDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int>;
using unsafe EnableEngineDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int, void>;
using unsafe EnableSirenDelegate = delegate* unmanaged[Thiscall]<CVehicle*, byte, void>;
using unsafe GetHealthDelegate = delegate* unmanaged[Thiscall]<CVehicle*, float>;
using unsafe GetTrailerDelegate = delegate* unmanaged[Thiscall]<CVehicle*, CVehicle*>;
using unsafe HasDriverDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int>;
using unsafe IsOccupiedDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int>;
using unsafe SetColorDelegate = delegate* unmanaged[Thiscall]<CVehicle*, byte, byte, void>;
using unsafe SetHealthDelegate = delegate* unmanaged[Thiscall]<CVehicle*, float, void>;
using unsafe SetInvulnerableDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int, void>;
using unsafe SetLicensePlateTextDelegate = delegate* unmanaged[Thiscall]<CVehicle*, byte*, void>;
using unsafe SetLockedDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int, void>;
using unsafe SetRotationDelegate = delegate* unmanaged[Thiscall]<CVehicle*, float, void>;
using unsafe SirenEnabledDelegate = delegate* unmanaged[Thiscall]<CVehicle*, int>;
using unsafe UpdateColorDelegate = delegate* unmanaged[Thiscall]<CVehicle*, void>;

[StructLayout(LayoutKind.Explicit, Size = 0x80, Pack = 1)]
public unsafe struct CVehicle
{
    [FieldOffset(0x00)]
    private CEntity _entity;

    [FieldOffset(0x48)]
    private readonly CVehicle* _trailer;

    [FieldOffset(0x4C)]
    private readonly nint _gameVehicle;

    [FieldOffset(0x58)]
    private readonly int _isInvulnerable;

    [FieldOffset(0x5C)]
    private readonly int _isLightsOn;

    [FieldOffset(0x60)]
    private readonly int _isLocked;

    [FieldOffset(0x78)]
    private readonly int _engineState;

    [FieldOffset(0x7C)]
    private readonly byte _primaryColor;

    [FieldOffset(0x7D)]
    private readonly byte _secondaryColor;

    private static readonly HasDriverDelegate _hasDriver = (HasDriverDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.HasDriver);
    private static readonly IsOccupiedDelegate _isOccupied = (IsOccupiedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.IsOccupied);
    private static readonly SetInvulnerableDelegate _setInvulnerable = (SetInvulnerableDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SetInvulnerable);
    private static readonly SetLockedDelegate _setLocked = (SetLockedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SetLocked);
    private static readonly GetHealthDelegate _getHealth = (GetHealthDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.GetHealth);
    private static readonly SetHealthDelegate _setHealth = (SetHealthDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SetHealth);
    private static readonly SetColorDelegate _setColor = (SetColorDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SetColor);
    private static readonly UpdateColorDelegate _updateColor = (UpdateColorDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.UpdateColor);
    private static readonly EnableSirenDelegate _enableSiren = (EnableSirenDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.EnableSiren);
    private static readonly SirenEnabledDelegate _sirenEnabled = (SirenEnabledDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SirenEnabled);
    private static readonly GetTrailerDelegate _getTrailer = (GetTrailerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.GetTrailer);
    private static readonly DoesExistDelegate _doesExist = (DoesExistDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.DoesExist);
    private static readonly SetLicensePlateTextDelegate _setLicensePlateText = (SetLicensePlateTextDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SetLicensePlateText);
    private static readonly SetRotationDelegate _setRotation = (SetRotationDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.SetRotation);
    private static readonly EnableEngineDelegate _enableEngine = (EnableEngineDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehicle.EnableEngine);

    public CEntity Entity => _entity;
    public CVehicle* Trailer => _trailer;
    public nint GameVehicle => _gameVehicle;
    public bool IsInvulnerable => _isInvulnerable != 0;
    public bool IsLightsOn => _isLightsOn != 0;
    public bool IsLocked => _isLocked != 0;
    public bool EngineState => _engineState != 0;
    public byte PrimaryColor => _primaryColor;
    public byte SecondaryColor => _secondaryColor;
    public bool IsAvailable => _gameVehicle != 0;

    public bool HasDriver() => _hasDriver((CVehicle*)Unsafe.AsPointer(ref this)) != 0;
    public bool IsOccupied() => _isOccupied((CVehicle*)Unsafe.AsPointer(ref this)) != 0;
    public void SetInvulnerable(bool value) => _setInvulnerable((CVehicle*)Unsafe.AsPointer(ref this), value ? 1 : 0);
    public void SetLocked(bool value) => _setLocked((CVehicle*)Unsafe.AsPointer(ref this), value ? 1 : 0);
    public float GetHealth() => _getHealth((CVehicle*)Unsafe.AsPointer(ref this));
    public void SetHealth(float value) => _setHealth((CVehicle*)Unsafe.AsPointer(ref this), value);
    public void SetColor(byte primaryColor, byte secondaryColor) => _setColor((CVehicle*)Unsafe.AsPointer(ref this), primaryColor, secondaryColor);
    public void UpdateColor() => _updateColor((CVehicle*)Unsafe.AsPointer(ref this));
    public void EnableSiren(bool enable) => _enableSiren((CVehicle*)Unsafe.AsPointer(ref this), enable ? (byte)1 : (byte)0);
    public bool SirenEnabled() => _sirenEnabled((CVehicle*)Unsafe.AsPointer(ref this)) != 0;
    public CVehicle* GetTrailer() => _getTrailer((CVehicle*)Unsafe.AsPointer(ref this));
    public bool DoesExist() => _doesExist((CVehicle*)Unsafe.AsPointer(ref this)) != 0;

    public void SetLicensePlateText(string text)
    {
        using var textAnsi = AnsiString.Encode(text);
        _setLicensePlateText((CVehicle*)Unsafe.AsPointer(ref this), textAnsi);
    }

    public void SetRotation(float value) => _setRotation((CVehicle*)Unsafe.AsPointer(ref this), value);
    public void EnableEngine(bool enable) => _enableEngine((CVehicle*)Unsafe.AsPointer(ref this), enable ? 1 : 0);
}
