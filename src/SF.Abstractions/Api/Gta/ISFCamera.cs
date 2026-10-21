using System.Numerics;

namespace SFSharp.Abstractions.Gta;

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
    ushort Mode { get; }
    float ShakeForce { get; }
    bool IsWideScreen { get; }
    float FadeAlpha { get; }
    byte FadeState { get; }
    bool IsFading { get; }

    Vector3 GetLookAtPoint(float distance = 100f);

    float GetDistanceTo(Vector3 point);
    /// <summary>Current GTA camera projection, in screen pixels. False for unavailable camera or near/far clipping; off-screen points may succeed.</summary>
    bool TryWorldToScreen(Vector3 point, out Vector2 screen, out float cameraDepth);
}
