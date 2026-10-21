using System.Numerics;

namespace SFSharp.Abstractions.Samp;

/// <summary>SA-MP local aim/target state, not the GTA rendering camera. Game thread only.</summary>
public interface ISFSampCamera
{
    byte AimCameraMode { get; }
    Vector3 AimDirection { get; }
    Vector3 AimPosition { get; }
    float AimZ { get; }
    byte AimZoom { get; }
    sbyte AspectRatio { get; }
    SFLocalPlayerCameraTarget Target { get; }
}
