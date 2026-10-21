using System.Numerics;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA SA 1.0 US camera layout, verified against EXE MD5 84a781f30fac7eba030af2497da86930.</summary>
internal readonly unsafe struct GtaCameraView(nint address)
{
    public bool HasActiveCamera => *(byte*)(address + GtaOffsets.CCamera.ActiveCamIndex) < 3;
    private nint Active => HasActiveCamera
        ? address + GtaOffsets.CCamera.Cams + *(byte*)(address + GtaOffsets.CCamera.ActiveCamIndex) * GtaOffsets.CCam.Size
        : throw new InvalidOperationException("Invalid GTA active camera index.");

    public Vector3 Position => *(Vector3*)(Active + GtaOffsets.CCam.Source);
    public Vector3 Front => *(Vector3*)(Active + GtaOffsets.CCam.Front);
    public Vector3 Up => *(Vector3*)(Active + GtaOffsets.CCam.Up);
    public float FieldOfView => *(float*)(Active + GtaOffsets.CCam.Fov);
    public ushort Mode => *(ushort*)(Active + GtaOffsets.CCam.Mode);
    public float FadeAlpha => *(float*)(address + GtaOffsets.CCamera.FadeAlpha);
    public byte FadeState => FadeAlpha <= 0 ? (byte)0 : FadeAlpha >= 255 ? (byte)2 : (byte)1;
    public bool IsWideScreen => *(byte*)(address + GtaOffsets.CCamera.WideScreenOn) != 0;
    public float ShakeForce => *(float*)(address + GtaOffsets.CCamera.ShakeForce);
}
