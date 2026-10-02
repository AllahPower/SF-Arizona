using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Protocol.Sync;

// OnfootData (RakNetPacketId 207).

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

    public static OnfootSyncData Parse(ref SampBitStreamReader r)
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

    public static OutgoingOnfootSyncData Parse(ref SampBitStreamReader r)
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
