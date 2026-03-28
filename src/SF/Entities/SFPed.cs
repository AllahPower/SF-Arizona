namespace SFSharp;

public sealed unsafe class SFPed : SFEntity
{
    private readonly SFPlayer? _player;
    private readonly CPed* _native;

    internal SFPed(CPed* native)
        : this(native, null)
    {
    }

    internal SFPed(CPed* native, SFPlayer? player)
    {
        _player = player;
        _native = native;
    }

    public SFPlayer? Player => _player;
    public CPed* Native => _native;
    public nint GamePedPointer => _native is null ? 0 : _native->GamePed;
    public byte PlayerNumber => _native is null ? byte.MaxValue : _native->PlayerNumber;
    public float Health => _native is null ? 0f : _native->GetHealth();
    public float Armour => _native is null ? 0f : _native->GetArmour();
    public byte State => _native is null ? byte.MaxValue : _native->GetState();
    public float Rotation => _native is null ? 0f : _native->GetRotation();
    public bool IsPassenger => _native is not null && _native->IsPassenger();
    public int VehicleSeatIndex => _native is null ? -1 : _native->GetVehicleSeatIndex();
    public byte CurrentWeapon => _native is null ? byte.MaxValue : _native->GetCurrentWeapon();
    public ushort CurrentWeaponAmmo => _native is null ? (ushort)0 : _native->GetCurrentWeaponAmmo();
    public bool HasJetpack => _native is not null && _native->HasJetpack();
    public float AimZ => _native is null ? 0f : _native->GetAimZ();
    public bool IsUsingCellphone => _native is not null && _native->IsUsingCellphone;
    public bool HasAccessory => _native is not null && _native->HasAccessory();
    public SFVehicle? Vehicle => _native is null || _native->GetVehicle() is null ? null : new SFVehicle(_native->GetVehicle());

    protected override CEntity* NativeEntity => (CEntity*)_native;

    public void SetHealth(float value)
    {
        if (_native is not null)
        {
            _native->SetHealth(value);
        }
    }

    public void SetArmour(float value)
    {
        if (_native is not null)
        {
            _native->SetArmour(value);
        }
    }

    public void SetState(byte value)
    {
        if (_native is not null)
        {
            _native->SetState(value);
        }
    }

    public void SetRotation(float value)
    {
        if (_native is not null)
        {
            _native->SetRotation(value);
        }
    }

    public void SetAimZ(float value)
    {
        if (_native is not null)
        {
            _native->SetAimZ(value);
        }
    }

    public void ClearWeapons()
    {
        if (_native is not null)
        {
            _native->ClearWeapons();
        }
    }

    public void EnableJetpack()
    {
        if (_native is not null)
        {
            _native->EnableJetpack();
        }
    }

    public void DisableJetpack()
    {
        if (_native is not null)
        {
            _native->DisableJetpack();
        }
    }

    public bool TryGetAccessory(int slot, out SFPedAccessory accessory)
    {
        if (_native is null || !_native->GetAccessoryState(slot))
        {
            accessory = default;
            return false;
        }

        CPedAccessory nativeAccessory = _native->GetAccessoryInfo(slot);
        accessory = new(
            Slot: slot,
            ModelId: nativeAccessory.ModelId,
            BoneId: nativeAccessory.BoneId,
            Offset: nativeAccessory.Offset,
            Rotation: nativeAccessory.Rotation,
            Scale: nativeAccessory.Scale,
            MaterialColor1: nativeAccessory.MaterialColor1,
            MaterialColor2: nativeAccessory.MaterialColor2,
            ObjectPointer: _native->GetAccessoryObjectPointer(slot));
        return true;
    }

    public IEnumerable<SFPedAccessory> EnumerateAccessories()
    {
        for (int slot = 0; slot < 10; slot++)
        {
            if (TryGetAccessory(slot, out SFPedAccessory accessory))
            {
                yield return accessory;
            }
        }
    }
}
