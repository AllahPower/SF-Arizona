using System.Numerics;

namespace SFSharp.Abstractions.Ui;

/// <summary>Read-only camera facade.</summary>
/// <remarks>NOT thread-safe. Reads the GTA SA camera state struct - main-thread only.</remarks>
public interface ISFCamera
{
    bool IsAvailable { get; }

    Vector3 Position { get; }
    Vector3 Front { get; }
    Vector3 Up { get; }
    Vector3 Right { get; }
    float FieldOfView { get; }
    byte Mode { get; }
    float ShakeForce { get; }
    bool IsWideScreen { get; }
    byte FadeAlpha { get; }
    byte FadeState { get; }
    bool IsFading { get; }

    byte AimCameraMode { get; }
    Vector3 AimDirection { get; }
    Vector3 AimPosition { get; }
    float AimZ { get; }
    byte AimZoom { get; }
    sbyte AspectRatio { get; }

    SFLocalPlayerCameraTarget Target { get; }

    Vector3 GetLookAtPoint(float distance = 100f);

    float GetDistanceTo(Vector3 point);
}
