using System.Runtime.InteropServices;

using unsafe ChatDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, byte*, void>;
using unsafe GetColorAsArgbDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, uint>;
using unsafe GetColorAsRgbaDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, uint>;
using unsafe GetPedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, SFSharp.Runtime.Interop.Classes.Peds.CPed*>;
using unsafe GetSpecialActionDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, byte>;
using unsafe RequestSpawnDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, void>;
using unsafe SetColorDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.CLocalPlayer*, uint, void>;

namespace SFSharp.Runtime.Interop.Classes;

[StructLayout(LayoutKind.Explicit, Size = 812, Pack = 1)]
public unsafe ref struct CLocalPlayer
{
    [FieldOffset(0)]
    private readonly LocalPlayerR5 _r5;

    [FieldOffset(0)]
    private readonly LocalPlayerR1 _r1;

    private static CLocalPlayer* CurrentInstance => CPlayerPool.Instance.GetLocalPlayer();
    public static ref readonly CLocalPlayer Instance => ref *RequireInstance();

    [FieldOffset(0x00)]
    private readonly CPed* _ped;

    [FieldOffset(0x04)]
    private readonly SampIncarData _incarData;

    [FieldOffset(0x43)]
    private readonly SampAimData _aimData;

    [FieldOffset(0x62)]
    private readonly SampTrailerData _trailerData;

    [FieldOffset(0x98)]
    private readonly SampOnfootData _onfootData;

    [FieldOffset(0xDC)]
    private readonly SampPassengerData _passengerData;

    [FieldOffset(0xFC)]
    private readonly ushort _currentVehicleId;

    [FieldOffset(0xFE)]
    private readonly ushort _lastVehicleId;

    [FieldOffset(0x107)]
    private readonly byte _team;

    [FieldOffset(0x11F)]
    private readonly CLocalPlayerCameraTarget _cameraTarget;

    [FieldOffset(0x127)]
    private readonly CPlayerHeadState _head;

    [FieldOffset(0x14F)]
    private readonly CLocalPlayerSpawnInfo _spawnInfo;

    [FieldOffset(0x17D)]
    private readonly int _hasSpawnInfoRaw;

    [FieldOffset(389)]
    private readonly CLocalPlayerWeaponsData _weaponsData;

    public CLocalPlayerWeaponsData WeaponsData => SampEnvironment.IsR1 ? _r1.weaponsData : _weaponsData;

    [FieldOffset(0x1CB)]
    private readonly int _passengerDriveByRaw;

    [FieldOffset(0x1CF)]
    private readonly byte _currentInterior;

    [FieldOffset(0x1D0)]
    private readonly int _inRcModeRaw;

    [FieldOffset(0x1D4)]
    private unsafe fixed byte _name[256];

    [FieldOffset(0x2D4)]
    private readonly CLocalPlayerSurfingState _surfing;

    [FieldOffset(0x2FA)]
    private readonly CLocalPlayerClassSelectionState _classSelection;

    [FieldOffset(0x312)]
    private readonly CLocalPlayerSpectatingState _spectating;

    [FieldOffset(0x31C)]
    private readonly CLocalPlayerDamageState _damage;

    public ushort AimedPlayerId => WeaponsData.AimedPlayer;
    public ushort AimedActorId => WeaponsData.AimedActor;
    public byte CurrentWeapon => WeaponsData.CurrentWeapon;
    public ushort CurrentVehicleId => SampEnvironment.IsR1 ? _r1.currentVehicleId : SampEnvironment.IsR5 ? _r5.currentVehicleId : _currentVehicleId;
    public ushort LastVehicleId => SampEnvironment.IsR1 ? _r1.lastVehicleId : SampEnvironment.IsR5 ? _r5.lastVehicleId : _lastVehicleId;
    public byte Team => SampEnvironment.IsR1 ? _r1.team : SampEnvironment.IsR5 ? _r5.team : _team;
    public bool HasSpawnInfo => (SampEnvironment.IsR1 ? _r1.hasSpawnInfo : _hasSpawnInfoRaw) != 0;
    public bool IsPassengerDriveBy => (SampEnvironment.IsR1 ? _r1.passengerDriveBy : _passengerDriveByRaw) != 0;
    public byte CurrentInterior => SampEnvironment.IsR1 ? _r1.currentInterior : _currentInterior;
    public bool IsInRcMode => (SampEnvironment.IsR1 ? _r1.inRcMode : _inRcModeRaw) != 0;
    public SampIncarData IncarData => SampEnvironment.IsR1 ? _r1.incarData : SampEnvironment.IsR5 ? _r5.incarData : _incarData;
    public SampAimData AimData => SampEnvironment.IsR1 ? _r1.aimData : SampEnvironment.IsR5 ? _r5.aimData : _aimData;
    public SampTrailerData TrailerData => SampEnvironment.IsR1 ? _r1.trailerData : SampEnvironment.IsR5 ? _r5.trailerData : _trailerData;
    public SampOnfootData OnfootData => SampEnvironment.IsR1 ? _r1.onfootData : SampEnvironment.IsR5 ? _r5.onfootData : _onfootData;
    public SampPassengerData PassengerData => SampEnvironment.IsR1 ? _r1.passengerData : SampEnvironment.IsR5 ? _r5.passengerData : _passengerData;
    public CLocalPlayerCameraTarget CameraTarget => SampEnvironment.IsR1 ? _r1.cameraTarget : _cameraTarget;
    public CPlayerHeadState Head => SampEnvironment.IsR1 ? _r1.head : SampEnvironment.IsR5 ? _r5.head : _head;
    public CLocalPlayerSpawnInfo SpawnInfo => SampEnvironment.IsR1 ? _r1.spawnInfo : _spawnInfo;
    public CLocalPlayerSurfingState Surfing => _surfing;
    public CLocalPlayerClassSelectionState ClassSelection => SampEnvironment.IsR1 ? _r1.classSelection : SampEnvironment.IsR5 ? _r5.classSelection : _classSelection;
    public CLocalPlayerSpectatingState Spectating => SampEnvironment.IsR1 ? _r1.spectating : SampEnvironment.IsR5 ? _r5.spectating : _spectating;
    public CLocalPlayerDamageState Damage => SampEnvironment.IsR1 ? _r1.damage : SampEnvironment.IsR5 ? _r5.damage : _damage;

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

    public string? GetName()
    {
        fixed (byte* name = _name)
        {
            return AnsiString.Decode(name);
        }
    }

    public bool TryGetSurfingVehicle(out CVehicle* vehicle)
    {
        vehicle = null;
        if (!_surfing.IsActive || _surfing.EntityId >= SampOffsets.CVehiclePool.MaxVehicles)
        {
            return false;
        }

        vehicle = CVehiclePool.Instance.Get(_surfing.EntityId);
        return vehicle != null;
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
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, CLocalPlayerWeaponsData.WeaponSlotCount);
        CLocalPlayerWeaponsData weapons = WeaponsData;
        return weapons.LastWeaponAmmo[slot];
    }

    public byte GetWeaponId(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, CLocalPlayerWeaponsData.WeaponSlotCount);
        CLocalPlayerWeaponsData weapons = WeaponsData;
        return weapons.LastWeapon[slot];
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

public unsafe struct WeaponsData
{
    public const int WeaponSlotCount = 13;

    public ushort AimedPlayer;
    public ushort AimedActor;
    public byte CurrentWeapon;
    public fixed byte LastWeapon[WeaponSlotCount];
    public fixed int LastWeaponAmmo[WeaponSlotCount];
}
