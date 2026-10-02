using System.Numerics;

namespace SFSharp.Protocol.Sync;

// AimData (RakNetPacketId 203).

public readonly record struct AimSyncData(
    byte CamMode,
    Vector3 CamFront,
    Vector3 CamPos,
    float AimZ,
    byte CamExtZoom,
    byte WeaponState,
    byte AspectRatio)
{
    public static AimSyncData Parse(ref SampBitStreamReader r)
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
