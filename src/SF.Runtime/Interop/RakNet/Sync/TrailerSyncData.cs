using System.Numerics;

namespace SFSharp.Runtime.Interop.RakNet.Sync;

// TrailerData (RakNetPacketId 210).

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
    public static TrailerSyncData Parse(ref SampBitStreamReader r)
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
