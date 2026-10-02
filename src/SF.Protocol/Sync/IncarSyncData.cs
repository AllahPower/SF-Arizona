using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Protocol.Sync;

// IncarData (RakNetPacketId 200).

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

    public static IncarSyncData Parse(ref SampBitStreamReader r)
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

    public static OutgoingIncarSyncData Parse(ref SampBitStreamReader r)
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
