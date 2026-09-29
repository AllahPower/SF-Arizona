using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet;

// SA-MP 0.3.7 R3-1 sync packet structures.
// Reference: samp.dll packet readers and SAMP.Lua synchronization.lua.
// All sync packets are prefixed with a EPacketId byte which is NOT included in these structs.

// ---- shared types ----

public readonly record struct SampKeys(
    bool PrimaryFire, bool HornCrouch, bool SecondaryFire, bool AccelZoomOut,
    bool EnterExitCar, bool DecelJump, bool CircleRight, bool Aim,
    bool CircleLeft, bool LandingGearLookback, bool WalkSlow,
    bool SpecialCtrlUp, bool SpecialCtrlDown, bool SpecialCtrlLeft, bool SpecialCtrlRight)
{
    public static SampKeys Parse(ushort raw)
    {
        return new SampKeys(
            PrimaryFire: (raw & (1 << 0)) != 0,
            HornCrouch: (raw & (1 << 1)) != 0,
            SecondaryFire: (raw & (1 << 2)) != 0,
            AccelZoomOut: (raw & (1 << 3)) != 0,
            EnterExitCar: (raw & (1 << 4)) != 0,
            DecelJump: (raw & (1 << 5)) != 0,
            CircleRight: (raw & (1 << 6)) != 0,
            Aim: (raw & (1 << 7)) != 0,
            CircleLeft: (raw & (1 << 8)) != 0,
            LandingGearLookback: (raw & (1 << 9)) != 0,
            WalkSlow: (raw & (1 << 10)) != 0,
            SpecialCtrlUp: (raw & (1 << 11)) != 0,
            SpecialCtrlDown: (raw & (1 << 12)) != 0,
            SpecialCtrlLeft: (raw & (1 << 13)) != 0,
            SpecialCtrlRight: (raw & (1 << 14)) != 0);
    }
}

public readonly record struct SampAnimation(ushort Id, byte FrameDelta, byte Flags)
{
    public bool LoopA => (Flags & 1) != 0;
    public bool LockX => (Flags & 2) != 0;
    public bool LockY => (Flags & 4) != 0;
    public bool LockF => (Flags & 8) != 0;
    public byte Time => (byte)(Flags >> 4);
    public uint RawValue => (uint)(Id | (FrameDelta << 16) | (Flags << 24));

    public static SampAnimation FromRaw(uint rawValue)
    {
        return new SampAnimation((ushort)rawValue, (byte)(rawValue >> 16), (byte)(rawValue >> 24));
    }
}

internal static class SampSyncCodec
{
    private const float QuaternionScale = 1.0f / 65535.0f;
    private const float VectorComponentScale = 2.0f / 65535.0f;

    public static ushort ReadOptionalUInt16(ref BitStreamReader r)
    {
        return r.ReadBitBool() ? r.ReadUInt16() : (ushort)0;
    }

    public static uint ReadOptionalUInt32(ref BitStreamReader r)
    {
        return r.ReadBitBool() ? r.ReadUInt32() : 0u;
    }

    public static void ReadPackedHealthArmor(byte raw, out byte health, out byte armor)
    {
        armor = ExpandNibble((byte)(raw & 0x0F));
        health = ExpandNibble((byte)(raw >> 4));
    }

    public static byte ExpandNibble(byte nibble)
    {
        if (nibble == 0x0F)
        {
            return 100;
        }

        if (nibble == 0)
        {
            return 0;
        }

        return (byte)(nibble * 7);
    }

    public static (float W, float X, float Y, float Z) ReadCompressedQuaternion(ref BitStreamReader r)
    {
        bool wPositive = r.ReadBitBool();
        bool xPositive = r.ReadBitBool();
        bool yPositive = r.ReadBitBool();
        bool zPositive = r.ReadBitBool();

        ushort xRaw = r.ReadUInt16();
        ushort yRaw = r.ReadUInt16();
        ushort zRaw = r.ReadUInt16();

        float x = xRaw * QuaternionScale;
        float y = yRaw * QuaternionScale;
        float z = zRaw * QuaternionScale;

        if (!xPositive)
        {
            x = -x;
        }

        if (!yPositive)
        {
            y = -y;
        }

        if (!zPositive)
        {
            z = -z;
        }

        float wSquared = 1.0f - (x * x) - (y * y) - (z * z);
        if (wSquared < 0.0f)
        {
            wSquared = 0.0f;
        }

        float w = MathF.Sqrt(wSquared);
        if (!wPositive)
        {
            w = -w;
        }

        return (w, x, y, z);
    }

    public static Vector3 ReadCompressedVector(ref BitStreamReader r)
    {
        float magnitude = r.ReadFloat();
        if (magnitude <= 0.00001f)
        {
            return Vector3.Zero;
        }

        float x = (r.ReadUInt16() * VectorComponentScale) - 1.0f;
        float y = (r.ReadUInt16() * VectorComponentScale) - 1.0f;
        float z = (r.ReadUInt16() * VectorComponentScale) - 1.0f;

        return new Vector3(x * magnitude, y * magnitude, z * magnitude);
    }
}

// ---- OnfootData (EPacketId 207) ----

public readonly record struct OnfootSyncData(
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    Vector3 Position,
    float QuatW,
    float QuatX,
    float QuatY,
    float QuatZ,
    byte Health,
    byte Armor,
    byte PackedHealthArmor,
    byte WeaponByteRaw,
    byte WeaponId,
    byte SpecialAction,
    Vector3 MoveSpeed,
    bool HasSurfingData,
    Vector3 SurfingOffsets,
    ushort SurfingVehicleId,
    bool HasAnimation,
    SampAnimation Animation)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);
    public Quaternion Rotation => new Quaternion(QuatX, QuatY, QuatZ, QuatW);
    public byte WeaponExtraBits => (byte)(WeaponByteRaw >> 6);
    public bool IsSurfing => HasSurfingData && SurfingVehicleId != ushort.MaxValue;

    public static OnfootSyncData Parse(ref BitStreamReader r)
    {
        ushort lr = SampSyncCodec.ReadOptionalUInt16(ref r);
        ushort ud = SampSyncCodec.ReadOptionalUInt16(ref r);
        ushort keys = r.ReadUInt16();
        Vector3 pos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        (float qw, float qx, float qy, float qz) = SampSyncCodec.ReadCompressedQuaternion(ref r);

        byte packedHealthArmor = r.ReadUInt8();
        SampSyncCodec.ReadPackedHealthArmor(packedHealthArmor, out byte health, out byte armor);

        byte weaponByte = r.ReadUInt8();
        byte weaponId = (byte)(weaponByte & 0x3F);
        byte specialAction = r.ReadUInt8();
        Vector3 speed = SampSyncCodec.ReadCompressedVector(ref r);

        bool hasSurfing = r.ReadBitBool();
        ushort surfingVehicleId = ushort.MaxValue;
        Vector3 surfingOffsets = Vector3.Zero;
        if (hasSurfing)
        {
            surfingVehicleId = r.ReadUInt16();
            surfingOffsets = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        }

        bool hasAnimation = r.ReadBitBool();
        SampAnimation animation = default;
        if (hasAnimation)
        {
            animation = SampAnimation.FromRaw(r.ReadUInt32());
        }

        return new OnfootSyncData(
            lr,
            ud,
            keys,
            pos,
            qw,
            qx,
            qy,
            qz,
            health,
            armor,
            packedHealthArmor,
            weaponByte,
            weaponId,
            specialAction,
            speed,
            hasSurfing,
            surfingOffsets,
            surfingVehicleId,
            hasAnimation,
            animation);
    }

    public override string ToString()
    {
        return $"pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1}) hp={Health} arm={Armor} wep={WeaponId} spd=({MoveSpeed.X:F2},{MoveSpeed.Y:F2},{MoveSpeed.Z:F2})";
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct OutgoingOnfootSyncData(
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    Vector3 Position,
    float QuatX,
    float QuatY,
    float QuatZ,
    float QuatW,
    byte Health,
    byte Armor,
    byte WeaponByteRaw,
    byte SpecialAction,
    Vector3 MoveSpeed,
    Vector3 SurfingOffsets,
    ushort SurfingVehicleId,
    ushort AnimationId,
    ushort AnimationFlags)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);
    public Quaternion Rotation => new Quaternion(QuatX, QuatY, QuatZ, QuatW);
    public byte WeaponId => (byte)(WeaponByteRaw & 0x3F);
    public byte SpecialKey => (byte)(WeaponByteRaw >> 6);

    public static OutgoingOnfootSyncData Parse(ref BitStreamReader r)
    {
        return new OutgoingOnfootSyncData(
            r.ReadUInt16(),
            r.ReadUInt16(),
            r.ReadUInt16(),
            new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat()),
            r.ReadFloat(),
            r.ReadFloat(),
            r.ReadFloat(),
            r.ReadFloat(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat()),
            new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat()),
            r.ReadUInt16(),
            r.ReadUInt16(),
            r.ReadUInt16());
    }

    public override string ToString()
    {
        return $"pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1}) hp={Health} arm={Armor} wep={WeaponId} spd=({MoveSpeed.X:F2},{MoveSpeed.Y:F2},{MoveSpeed.Z:F2})";
    }
}

// ---- IncarData (EPacketId 200) ----

public readonly record struct IncarSyncData(
    ushort VehicleId,
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    float QuatW,
    float QuatX,
    float QuatY,
    float QuatZ,
    Vector3 Position,
    Vector3 MoveSpeed,
    ushort VehicleHealthRaw,
    float VehicleHealth,
    byte DriverHealth,
    byte DriverArmor,
    byte PackedHealthArmor,
    byte WeaponByteRaw,
    byte WeaponId,
    bool SirenEnabled,
    bool LandingGearState,
    bool HasTrainSpeed,
    float TrainSpeed,
    bool HasTrailerId,
    ushort TrailerId)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);
    public Quaternion Rotation => new Quaternion(QuatX, QuatY, QuatZ, QuatW);
    public byte WeaponExtraBits => (byte)(WeaponByteRaw >> 6);

    public static IncarSyncData Parse(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        ushort lr = r.ReadUInt16();
        ushort ud = r.ReadUInt16();
        ushort keys = r.ReadUInt16();
        (float qw, float qx, float qy, float qz) = SampSyncCodec.ReadCompressedQuaternion(ref r);
        Vector3 pos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 speed = SampSyncCodec.ReadCompressedVector(ref r);

        ushort vehicleHealthRaw = r.ReadUInt16();
        float vehicleHealth = vehicleHealthRaw;

        byte packedHealthArmor = r.ReadUInt8();
        SampSyncCodec.ReadPackedHealthArmor(packedHealthArmor, out byte driverHealth, out byte driverArmor);

        byte weaponByte = r.ReadUInt8();
        byte weaponId = (byte)(weaponByte & 0x3F);

        bool sirenEnabled = r.ReadBitBool();
        bool landingGearState = r.ReadBitBool();

        bool hasTrainSpeed = r.ReadBitBool();
        float trainSpeed = hasTrainSpeed ? r.ReadFloat() : 0.0f;

        bool hasTrailerId = r.ReadBitBool();
        ushort trailerId = hasTrailerId ? r.ReadUInt16() : (ushort)0;

        return new IncarSyncData(
            vehicleId,
            lr,
            ud,
            keys,
            qw,
            qx,
            qy,
            qz,
            pos,
            speed,
            vehicleHealthRaw,
            vehicleHealth,
            driverHealth,
            driverArmor,
            packedHealthArmor,
            weaponByte,
            weaponId,
            sirenEnabled,
            landingGearState,
            hasTrainSpeed,
            trainSpeed,
            hasTrailerId,
            trailerId);
    }

    public override string ToString()
    {
        return $"veh={VehicleId} pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1}) vhp={VehicleHealth:F0} hp={DriverHealth} arm={DriverArmor} wep={WeaponId}";
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct OutgoingIncarSyncData(
    ushort VehicleId,
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    float QuatX,
    float QuatY,
    float QuatZ,
    float QuatW,
    Vector3 Position,
    Vector3 MoveSpeed,
    float VehicleHealth,
    byte DriverHealth,
    byte Armor,
    byte WeaponByteRaw,
    byte Siren,
    byte LandingGearState,
    ushort TrailerId,
    float TrainSpeed)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);
    public Quaternion Rotation => new Quaternion(QuatX, QuatY, QuatZ, QuatW);
    public byte WeaponId => (byte)(WeaponByteRaw & 0x3F);
    public byte SpecialKey => (byte)(WeaponByteRaw >> 6);

    public static OutgoingIncarSyncData Parse(ref BitStreamReader r)
    {
        return new OutgoingIncarSyncData(
            r.ReadUInt16(),
            r.ReadUInt16(),
            r.ReadUInt16(),
            r.ReadUInt16(),
            r.ReadFloat(),
            r.ReadFloat(),
            r.ReadFloat(),
            r.ReadFloat(),
            new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat()),
            new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat()),
            r.ReadFloat(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt16(),
            r.ReadFloat());
    }

    public override string ToString()
    {
        return $"veh={VehicleId} pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1}) vhp={VehicleHealth:F0} hp={DriverHealth} arm={Armor} wep={WeaponId}";
    }
}

// ---- AimData (EPacketId 203) ----

public readonly record struct AimSyncData(
    byte CamMode,
    Vector3 CamFront,
    Vector3 CamPos,
    float AimZ,
    byte CamExtZoom,
    byte WeaponState,
    byte AspectRatio)
{
    public static AimSyncData Parse(ref BitStreamReader r)
    {
        byte mode = r.ReadUInt8();
        Vector3 front = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 camPos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        float aimZ = r.ReadFloat();
        byte zoomByte = r.ReadUInt8();
        byte zoom = (byte)(zoomByte & 0x3F);
        byte weapState = (byte)(zoomByte >> 6);
        byte aspect = r.ReadUInt8();
        return new AimSyncData(mode, front, camPos, aimZ, zoom, weapState, aspect);
    }

    public override string ToString()
    {
        return $"mode={CamMode} front=({CamFront.X:F2},{CamFront.Y:F2},{CamFront.Z:F2}) aimZ={AimZ:F2} zoom={CamExtZoom} wState={WeaponState}";
    }
}

// ---- BulletData (EPacketId 206) ----

public readonly record struct BulletSyncData(
    byte TargetType,
    ushort TargetId,
    Vector3 Origin,
    Vector3 Target,
    Vector3 Center,
    byte WeaponId)
{
    public static BulletSyncData Parse(ref BitStreamReader r)
    {
        byte type = r.ReadUInt8();
        ushort id = r.ReadUInt16();
        Vector3 origin = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 target = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 center = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        byte wep = r.ReadUInt8();
        return new BulletSyncData(type, id, origin, target, center, wep);
    }

    public override string ToString()
    {
        return $"tType={TargetType} tId={TargetId} wep={WeaponId} origin=({Origin.X:F1},{Origin.Y:F1},{Origin.Z:F1}) target=({Target.X:F1},{Target.Y:F1},{Target.Z:F1})";
    }
}

// ---- PassengerData (EPacketId 211) ----

public readonly record struct PassengerSyncData(
    ushort VehicleId,
    byte SeatId,
    bool DriveBy,
    bool Cuffed,
    byte WeaponId,
    byte SpecialKey,
    byte Health,
    byte Armor,
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    Vector3 Position)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);

    public static PassengerSyncData Parse(ref BitStreamReader r)
    {
        ushort vehId = r.ReadUInt16();
        byte seatByte = r.ReadUInt8();
        byte seatId = (byte)(seatByte & 0x3F);
        bool driveBy = (seatByte & 0x40) != 0;
        bool cuffed = (seatByte & 0x80) != 0;
        byte weaponByte = r.ReadUInt8();
        byte weaponId = (byte)(weaponByte & 0x3F);
        byte specialKey = (byte)(weaponByte >> 6);
        byte hp = r.ReadUInt8();
        byte arm = r.ReadUInt8();
        ushort lr = r.ReadUInt16();
        ushort ud = r.ReadUInt16();
        ushort keys = r.ReadUInt16();
        Vector3 pos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        return new PassengerSyncData(vehId, seatId, driveBy, cuffed, weaponId, specialKey, hp, arm, lr, ud, keys, pos);
    }

    public override string ToString()
    {
        return $"veh={VehicleId} seat={SeatId} hp={Health} arm={Armor} wep={WeaponId} driveBy={DriveBy}";
    }
}

// ---- UnoccupiedData (EPacketId 209) ----

public readonly record struct UnoccupiedSyncData(
    ushort VehicleId,
    byte SeatId,
    Vector3 Roll,
    Vector3 Direction,
    Vector3 Position,
    Vector3 MoveSpeed,
    Vector3 TurnSpeed,
    float VehicleHealth)
{
    public static UnoccupiedSyncData Parse(ref BitStreamReader r)
    {
        ushort vehId = r.ReadUInt16();
        byte seat = r.ReadUInt8();
        Vector3 roll = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 dir = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 pos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 moveSpd = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 turnSpd = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        float hp = r.ReadFloat();
        return new UnoccupiedSyncData(vehId, seat, roll, dir, pos, moveSpd, turnSpd, hp);
    }

    public override string ToString()
    {
        return $"veh={VehicleId} seat={SeatId} pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1}) vhp={VehicleHealth:F0}";
    }
}

// ---- TrailerData (EPacketId 210) ----

public readonly record struct TrailerSyncData(
    ushort TrailerId,
    Vector3 Position,
    float QuatW,
    float QuatX,
    float QuatY,
    float QuatZ,
    Vector3 MoveSpeed,
    Vector3 TurnSpeed)
{
    public static TrailerSyncData Parse(ref BitStreamReader r)
    {
        ushort id = r.ReadUInt16();
        Vector3 pos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        float qw = r.ReadFloat();
        float qx = r.ReadFloat();
        float qy = r.ReadFloat();
        float qz = r.ReadFloat();
        Vector3 moveSpd = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        Vector3 turnSpd = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        return new TrailerSyncData(id, pos, qw, qx, qy, qz, moveSpd, turnSpd);
    }

    public override string ToString()
    {
        return $"trailer={TrailerId} pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1})";
    }
}

// ---- SpectatorData (EPacketId 212) ----

public readonly record struct SpectatorSyncData(
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    Vector3 Position)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);

    public static SpectatorSyncData Parse(ref BitStreamReader r)
    {
        ushort lr = r.ReadUInt16();
        ushort ud = r.ReadUInt16();
        ushort keys = r.ReadUInt16();
        Vector3 pos = new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
        return new SpectatorSyncData(lr, ud, keys, pos);
    }

    public override string ToString()
    {
        return $"pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1})";
    }
}

// ---- WeaponsData (EPacketId 204) ----

public readonly record struct WeaponSlot(byte Id, byte Unknown1, ushort Ammo)
{
    public static WeaponSlot Parse(ref BitStreamReader r)
    {
        byte id = r.ReadUInt8();
        byte unk = r.ReadUInt8();
        ushort ammo = r.ReadUInt16();
        return new WeaponSlot(id, unk, ammo);
    }
}

public readonly record struct WeaponsSyncData(
    ushort TargetId,
    ushort TargetActorId,
    WeaponSlot[] Slots)
{
    public static WeaponsSyncData Parse(ref BitStreamReader r)
    {
        ushort target = r.ReadUInt16();
        ushort actor = r.ReadUInt16();
        List<WeaponSlot> slots = new List<WeaponSlot>();
        while (r.RemainingBits >= 32)
        {
            slots.Add(WeaponSlot.Parse(ref r));
        }

        return new WeaponsSyncData(target, actor, slots.ToArray());
    }

    public override string ToString()
    {
        return $"target={TargetId} actor={TargetActorId} slots={Slots.Length}";
    }
}

// ---- StatsData (EPacketId 205) ----

public readonly record struct StatsSyncData(int Money, int DrunkLevel)
{
    public static StatsSyncData Parse(ref BitStreamReader r)
    {
        int money = r.ReadInt32();
        int drunk = r.ReadInt32();
        return new StatsSyncData(money, drunk);
    }

    public override string ToString()
    {
        return $"money={Money} drunk={DrunkLevel}";
    }
}

// ---- MarkersData (EPacketId 208) ----

public readonly record struct PlayerMarker(ushort PlayerId, bool Active, Vector3 Position)
{
    public override string ToString()
    {
        return $"pid={PlayerId} active={Active} pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1})";
    }
}

public readonly record struct MarkersSyncData(int PlayerCount, PlayerMarker[] Markers)
{
    public static MarkersSyncData Parse(ref BitStreamReader r)
    {
        int count = r.ReadInt32();
        List<PlayerMarker> markers = new List<PlayerMarker>();
        for (int i = 0; i < count && r.RemainingBits >= 17; i++)
        {
            ushort pid = r.ReadUInt16();
            bool active = r.ReadBitBool();
            Vector3 pos = default;
            if (active)
            {
                pos = new Vector3(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
            }

            markers.Add(new PlayerMarker(pid, active, pos));
        }

        return new MarkersSyncData(count, markers.ToArray());
    }

    public override string ToString()
    {
        return $"players={PlayerCount} markers={Markers.Length}";
    }
}
