using System.Numerics;

namespace SFSharp.Runtime.Interop.RakNet.Sync;

// SpectatorData (RakNetPacketId 212).

public readonly record struct SpectatorSyncData(
    ushort LeftRightKeys,
    ushort UpDownKeys,
    ushort KeysRaw,
    Vector3 Position)
{
    public SampKeys Keys => SampKeys.Parse(KeysRaw);

    public static SpectatorSyncData Parse(ref SampBitStreamReader r)
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
