using System.Numerics;

namespace SFSharp.Protocol.Sync;

// PassengerData (RakNetPacketId 211).

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

    public static PassengerSyncData Parse(ref SampBitStreamReader r)
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
