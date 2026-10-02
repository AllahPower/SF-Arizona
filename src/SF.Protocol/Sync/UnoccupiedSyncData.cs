using System.Numerics;

namespace SFSharp.Protocol.Sync;

// UnoccupiedData (RakNetPacketId 209).

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
    public static UnoccupiedSyncData Parse(ref SampBitStreamReader r)
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
