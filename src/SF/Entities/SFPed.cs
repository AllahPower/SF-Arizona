using System.Numerics;

namespace SFSharp.Runtime.Game.Entities;

public readonly record struct SFPedSnapshot(
    Vector3 Position,
    float Health,
    float Armour,
    byte State,
    float Rotation,
    byte CurrentWeapon,
    ushort CurrentWeaponAmmo,
    bool HasJetpack,
    float AimZ);

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
    public bool ExistsInGame => TryValidateNativePed(_native, out _);
    public new Vector3 Position => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.Position : Vector3.Zero;
    public float Health => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.Health : 0f;
    public float Armour => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.Armour : 0f;
    public byte State => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.State : byte.MaxValue;
    public float Rotation => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.Rotation : 0f;
    public bool IsPassenger => TryGetValidatedNative(out CPed* native) && native->IsPassenger();
    public int VehicleSeatIndex => _native is null ? -1 : _native->GetVehicleSeatIndex();
    public byte CurrentWeapon => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.CurrentWeapon : byte.MaxValue;
    public ushort CurrentWeaponAmmo => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.CurrentWeaponAmmo : (ushort)0;
    public bool HasJetpack => TryGetSnapshot(out SFPedSnapshot snapshot) && snapshot.HasJetpack;
    public float AimZ => TryGetSnapshot(out SFPedSnapshot snapshot) ? snapshot.AimZ : 0f;
    public bool IsUsingCellphone => TryGetValidatedNative(out CPed* native) && native->IsUsingCellphone;
    public bool HasAccessory => TryGetValidatedNative(out CPed* native) && native->HasAccessory();
    public Vector3 GetBonePosition(SFPedBone bone)
    {
        return TryGetBonePosition(bone, out Vector3 position) ? position : Vector3.Zero;
    }

    public bool TryGetBonePosition(SFPedBone bone, out Vector3 position)
    {
        position = default;
        return TryGetBonePosition((int)bone, out position);
    }

    public bool TryGetBonePosition(int boneId, out Vector3 position)
    {
        position = default;
        if (!TryGetValidatedNative(out CPed* native))
        {
            return false;
        }

        position = native->GetBonePosition(boneId);
        return true;
    }

    public SFVehicle? Vehicle
    {
        get
        {
            if (!TryGetValidatedNative(out CPed* native))
            {
                return null;
            }

            CVehicle* vehicle = native->GetVehicle();
            return vehicle is null || !NativeMemoryValidator.IsReadable((nint)vehicle, (nuint)sizeof(CVehicle)) || !vehicle->DoesExist()
                ? null
                : new SFVehicle(vehicle);
        }
    }

    protected override CEntity* NativeEntity => (CEntity*)_native;

    internal static bool TryCreate(CPed* native, SFPlayer? player, out SFPed ped)
    {
        if (!TryValidateNativePed(native, out _))
        {
            ped = null!;
            return false;
        }

        ped = new SFPed(native, player);
        return true;
    }

    public bool TryGetSnapshot(out SFPedSnapshot snapshot)
    {
        snapshot = default;
        if (!TryGetValidatedNative(out CPed* native))
        {
            return false;
        }

        CEntity* entity = (CEntity*)native;
        snapshot = new SFPedSnapshot(
            Position: entity->GetPosition(),
            Health: native->GetHealth(),
            Armour: native->GetArmour(),
            State: native->GetState(),
            Rotation: native->GetRotation(),
            CurrentWeapon: native->GetCurrentWeapon(),
            CurrentWeaponAmmo: native->GetCurrentWeaponAmmo(),
            HasJetpack: native->HasJetpack(),
            AimZ: native->GetAimZ());
        return true;
    }

    private bool TryGetValidatedNative(out CPed* native)
    {
        native = _native;
        return TryValidateNativePed(native, out _);
    }

    private static bool TryValidateNativePed(CPed* native, out CEntity* entity)
    {
        entity = (CEntity*)native;
        if (native is null || !NativeMemoryValidator.IsReadable((nint)native, (nuint)sizeof(CPed)))
        {
            return false;
        }

        nint gameEntity = entity->GameEntity;
        if (gameEntity == 0 || !NativeMemoryValidator.IsReadable(gameEntity, (nuint)sizeof(nint) * 4))
        {
            return false;
        }

        return entity->DoesExist();
    }

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
