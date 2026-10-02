using System.Numerics;

namespace SFSharp.Runtime.Interop.RakNet.Sync;

// BulletData (RakNetPacketId 206).

public readonly record struct BulletSyncData(
    byte TargetType,
    ushort TargetId,
    Vector3 Origin,
    Vector3 Target,
    Vector3 Center,
    byte WeaponId)
{
    public static BulletSyncData Parse(ref SampBitStreamReader r)
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
