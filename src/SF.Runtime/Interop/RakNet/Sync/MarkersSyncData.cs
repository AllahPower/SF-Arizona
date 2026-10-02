using System.Numerics;

namespace SFSharp.Runtime.Interop.RakNet.Sync;

// MarkersData (RakNetPacketId 208).

public readonly record struct PlayerMarker(ushort PlayerId, bool Active, Vector3 Position)
{
    public override string ToString()
    {
        return $"pid={PlayerId} active={Active} pos=({Position.X:F1},{Position.Y:F1},{Position.Z:F1})";
    }
}

public readonly record struct MarkersSyncData(int PlayerCount, PlayerMarker[] Markers)
{
    public static MarkersSyncData Parse(ref SampBitStreamReader r)
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
