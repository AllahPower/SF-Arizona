using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampControllerState
{
    public short LeftStickX;
    public short LeftStickY;
    public short Buttons;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampAnimation
{
    public int Value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampOnfootData
{
    public SampControllerState ControllerState;
    public Vector3 Position;
    public float QuaternionX;
    public float QuaternionY;
    public float QuaternionZ;
    public float QuaternionW;
    public byte Health;
    public byte Armour;
    public byte CurrentWeapon;
    public byte SpecialAction;
    public Vector3 Speed;
    public Vector3 SurfingOffset;
    public ushort SurfingVehicleId;
    public SampAnimation Animation;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampIncarData
{
    public ushort VehicleId;
    public SampControllerState ControllerState;
    public float QuaternionX;
    public float QuaternionY;
    public float QuaternionZ;
    public float QuaternionW;
    public Vector3 Position;
    public Vector3 Speed;
    public float VehicleHealth;
    public byte DriverHealth;
    public byte DriverArmour;
    public byte CurrentWeapon;
    public byte SirenEnabled;
    public byte LandingGear;
    public ushort TrailerId;
    public float TrainSpeedOrHydraX;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampAimData
{
    public byte CameraMode;
    public Vector3 AimDirection;
    public Vector3 AimPosition;
    public float AimZ;
    public byte CameraExtZoomAndWeaponState;
    public sbyte AspectRatio;

    public byte CameraExtZoom => (byte)(CameraExtZoomAndWeaponState & 0x3F);
    public byte WeaponState => (byte)(CameraExtZoomAndWeaponState >> 6);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampTrailerData
{
    public ushort TrailerId;
    public Vector3 Position;
    public float QuaternionX;
    public float QuaternionY;
    public float QuaternionZ;
    public float QuaternionW;
    public Vector3 Speed;
    public Vector3 TurnSpeed;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SampPassengerData
{
    public ushort VehicleId;
    public byte SeatId;
    public byte CurrentWeapon;
    public byte Health;
    public byte Armour;
    public SampControllerState ControllerState;
    public Vector3 Position;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerCameraTarget
{
    public ushort ObjectId;
    public ushort VehicleId;
    public ushort PlayerId;
    public ushort ActorId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CPlayerHeadState
{
    public Vector3 Direction;
    public uint LastUpdate;
    public uint LastLook;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerSpawnInfo
{
    public byte Team;
    public int Skin;
    public byte Unknown0;
    public Vector3 Position;
    public float Rotation;
    public int Weapon1;
    public int Weapon2;
    public int Weapon3;
    public int Ammo1;
    public int Ammo2;
    public int Ammo3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerWeaponsData
{
    public const int WeaponSlotCount = 13;

    public ushort AimedPlayer;
    public ushort AimedActor;
    public byte CurrentWeapon;
    public unsafe fixed byte LastWeapon[WeaponSlotCount];
    public unsafe fixed int LastWeaponAmmo[WeaponSlotCount];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerSurfingState
{
    public ushort EntityId;
    public uint LastUpdate;
    public nint EntityPointer;
    public int IsStuckRaw;
    public int IsActiveRaw;
    public Vector3 Position;
    public int Unknown0;
    public int Mode;

    public bool IsStuck => IsStuckRaw != 0;
    public bool IsActive => IsActiveRaw != 0;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerClassSelectionState
{
    public int EnableAfterDeathRaw;
    public int Selected;
    public int WaitingForSpawnReplyRaw;
    public int IsActiveRaw;

    public bool EnableAfterDeath => EnableAfterDeathRaw != 0;
    public bool WaitingForSpawnReply => WaitingForSpawnReplyRaw != 0;
    public bool IsActive => IsActiveRaw != 0;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerSpectatingState
{
    public byte Mode;
    public byte Type;
    public int ObjectId;
    public int ProcessedRaw;

    public bool Processed => ProcessedRaw != 0;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CLocalPlayerDamageState
{
    public ushort VehicleUpdatingId;
    public int Bumper;
    public int Door;
    public byte Light;
    public byte Wheel;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CRemotePlayerIncarTargetRotation
{
    public float Real;
    public Vector3 Imaginary;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CRemotePlayerMarkerPosition
{
    public int X;
    public int Y;
    public int Z;

    public Vector3 ToVector3() => new(X, Y, Z);
}
