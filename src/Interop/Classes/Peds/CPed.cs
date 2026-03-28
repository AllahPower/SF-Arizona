using SFSharp;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe AddAccessoryDelegate = delegate* unmanaged[Thiscall]<CPed*, int, CPedAccessory*, void>;
using unsafe ClearWeaponsDelegate = delegate* unmanaged[Thiscall]<CPed*, void>;
using unsafe DeleteAccessoryDelegate = delegate* unmanaged[Thiscall]<CPed*, int, void>;
using unsafe DeleteAllAccessoriesDelegate = delegate* unmanaged[Thiscall]<CPed*, void>;
using unsafe DisableJetpackDelegate = delegate* unmanaged[Thiscall]<CPed*, void>;
using unsafe EnableJetpackDelegate = delegate* unmanaged[Thiscall]<CPed*, void>;
using unsafe GetAccessoryDelegate = delegate* unmanaged[Thiscall]<CPed*, int, nint>;
using unsafe GetAccessoryStateDelegate = delegate* unmanaged[Thiscall]<CPed*, int, int>;
using unsafe GetAimZDelegate = delegate* unmanaged[Thiscall]<CPed*, float>;
using unsafe GetArmourDelegate = delegate* unmanaged[Thiscall]<CPed*, float>;
using unsafe GetCurrentWeaponAmmoDelegate = delegate* unmanaged[Thiscall]<CPed*, ushort>;
using unsafe GetCurrentWeaponDelegate = delegate* unmanaged[Thiscall]<CPed*, byte>;
using unsafe GetHealthDelegate = delegate* unmanaged[Thiscall]<CPed*, float>;
using unsafe GetRotationDelegate = delegate* unmanaged[Thiscall]<CPed*, float>;
using unsafe GetStateDelegate = delegate* unmanaged[Thiscall]<CPed*, byte>;
using unsafe GetVehicleDelegate = delegate* unmanaged[Thiscall]<CPed*, CVehicle*>;
using unsafe GetVehicleSeatIndexDelegate = delegate* unmanaged[Thiscall]<CPed*, int>;
using unsafe HasAccessoryDelegate = delegate* unmanaged[Thiscall]<CPed*, int>;
using unsafe HasJetpackDelegate = delegate* unmanaged[Thiscall]<CPed*, int>;
using unsafe IsPassengerDelegate = delegate* unmanaged[Thiscall]<CPed*, int>;
using unsafe SetAimZDelegate = delegate* unmanaged[Thiscall]<CPed*, float, void>;
using unsafe SetArmourDelegate = delegate* unmanaged[Thiscall]<CPed*, float, void>;
using unsafe SetHealthDelegate = delegate* unmanaged[Thiscall]<CPed*, float, void>;
using unsafe SetRotationDelegate = delegate* unmanaged[Thiscall]<CPed*, float, void>;
using unsafe SetStateDelegate = delegate* unmanaged[Thiscall]<CPed*, byte, void>;

[StructLayout(LayoutKind.Explicit, Size = 0x2B4, Pack = 1)]
public unsafe struct CPed
{
    [FieldOffset(0x00)]
    private CEntity _entity;

    [FieldOffset(0x48)]
    private readonly int _usingCellphone;

    [FieldOffset(0x4C)]
    private Int32Array10 _accessoryStates;

    [FieldOffset(0x74)]
    private CPedAccessoryArray10 _accessories;

    [FieldOffset(0x27C)]
    private NIntArray10 _accessoryObjects;

    [FieldOffset(0x2A4)]
    private readonly nint _gamePed;

    [FieldOffset(0x2B0)]
    private readonly byte _playerNumber;

    private static readonly GetHealthDelegate _getHealth = (GetHealthDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetHealth);
    private static readonly SetHealthDelegate _setHealth = (SetHealthDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.SetHealth);
    private static readonly GetArmourDelegate _getArmour = (GetArmourDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetArmour);
    private static readonly SetArmourDelegate _setArmour = (SetArmourDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.SetArmour);
    private static readonly GetStateDelegate _getState = (GetStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetState);
    private static readonly SetStateDelegate _setState = (SetStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.SetState);
    private static readonly GetRotationDelegate _getRotation = (GetRotationDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetRotation);
    private static readonly SetRotationDelegate _setRotation = (SetRotationDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.SetRotation);
    private static readonly IsPassengerDelegate _isPassenger = (IsPassengerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.IsPassenger);
    private static readonly GetVehicleDelegate _getVehicle = (GetVehicleDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetVehicle);
    private static readonly GetCurrentWeaponDelegate _getCurrentWeapon = (GetCurrentWeaponDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetCurrentWeapon);
    private static readonly GetCurrentWeaponAmmoDelegate _getCurrentWeaponAmmo = (GetCurrentWeaponAmmoDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetCurrentWeaponAmmo);
    private static readonly GetVehicleSeatIndexDelegate _getVehicleSeatIndex = (GetVehicleSeatIndexDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetVehicleSeatIndex);
    private static readonly ClearWeaponsDelegate _clearWeapons = (ClearWeaponsDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.ClearWeapons);
    private static readonly EnableJetpackDelegate _enableJetpack = (EnableJetpackDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.EnableJetpack);
    private static readonly DisableJetpackDelegate _disableJetpack = (DisableJetpackDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.DisableJetpack);
    private static readonly HasJetpackDelegate _hasJetpack = (HasJetpackDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.HasJetpack);
    private static readonly GetAimZDelegate _getAimZ = (GetAimZDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetAimZ);
    private static readonly SetAimZDelegate _setAimZ = (SetAimZDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.SetAimZ);
    private static readonly HasAccessoryDelegate _hasAccessory = (HasAccessoryDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.HasAccessory);
    private static readonly DeleteAccessoryDelegate _deleteAccessory = (DeleteAccessoryDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.DeleteAccessory);
    private static readonly GetAccessoryStateDelegate _getAccessoryState = (GetAccessoryStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetAccessoryState);
    private static readonly GetAccessoryDelegate _getAccessory = (GetAccessoryDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.GetAccessory);
    private static readonly DeleteAllAccessoriesDelegate _deleteAllAccessories = (DeleteAllAccessoriesDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.DeleteAllAccessories);
    private static readonly AddAccessoryDelegate _addAccessory = (AddAccessoryDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPed.AddAccessory);

    public CEntity Entity => _entity;
    public nint GamePed => _gamePed;
    public byte PlayerNumber => _playerNumber;
    public bool IsAvailable => _gamePed != 0;
    public bool IsUsingCellphone => _usingCellphone != 0;

    public float GetHealth() => _getHealth((CPed*)Unsafe.AsPointer(ref this));
    public void SetHealth(float value) => _setHealth((CPed*)Unsafe.AsPointer(ref this), value);
    public float GetArmour() => _getArmour((CPed*)Unsafe.AsPointer(ref this));
    public void SetArmour(float value) => _setArmour((CPed*)Unsafe.AsPointer(ref this), value);
    public byte GetState() => _getState((CPed*)Unsafe.AsPointer(ref this));
    public void SetState(byte value) => _setState((CPed*)Unsafe.AsPointer(ref this), value);
    public float GetRotation() => _getRotation((CPed*)Unsafe.AsPointer(ref this));
    public void SetRotation(float value) => _setRotation((CPed*)Unsafe.AsPointer(ref this), value);
    public bool IsPassenger() => _isPassenger((CPed*)Unsafe.AsPointer(ref this)) != 0;
    public CVehicle* GetVehicle() => _getVehicle((CPed*)Unsafe.AsPointer(ref this));
    public byte GetCurrentWeapon() => _getCurrentWeapon((CPed*)Unsafe.AsPointer(ref this));
    public ushort GetCurrentWeaponAmmo() => _getCurrentWeaponAmmo((CPed*)Unsafe.AsPointer(ref this));
    public int GetVehicleSeatIndex() => _getVehicleSeatIndex((CPed*)Unsafe.AsPointer(ref this));
    public void ClearWeapons() => _clearWeapons((CPed*)Unsafe.AsPointer(ref this));
    public void EnableJetpack() => _enableJetpack((CPed*)Unsafe.AsPointer(ref this));
    public void DisableJetpack() => _disableJetpack((CPed*)Unsafe.AsPointer(ref this));
    public bool HasJetpack() => _hasJetpack((CPed*)Unsafe.AsPointer(ref this)) != 0;
    public float GetAimZ() => _getAimZ((CPed*)Unsafe.AsPointer(ref this));
    public void SetAimZ(float value) => _setAimZ((CPed*)Unsafe.AsPointer(ref this), value);
    public bool HasAccessory() => _hasAccessory((CPed*)Unsafe.AsPointer(ref this)) != 0;
    public bool GetAccessoryState(int slot) => _getAccessoryState((CPed*)Unsafe.AsPointer(ref this), slot) != 0;
    public void DeleteAccessory(int slot) => _deleteAccessory((CPed*)Unsafe.AsPointer(ref this), slot);
    public void DeleteAllAccessories() => _deleteAllAccessories((CPed*)Unsafe.AsPointer(ref this));

    public CPedAccessory GetAccessoryInfo(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, 10);
        return _accessories[slot];
    }

    public nint GetAccessoryObjectPointer(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, 10);
        return _accessoryObjects[slot];
    }

    public void AddAccessory(int slot, in CPedAccessory accessory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, 10);
        CPedAccessory copy = accessory;
        CPedAccessory* accessoryPtr = &copy;
        _addAccessory((CPed*)Unsafe.AsPointer(ref this), slot, accessoryPtr);
    }
}
