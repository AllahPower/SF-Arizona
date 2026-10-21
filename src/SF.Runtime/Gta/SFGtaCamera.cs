using System.Numerics;

namespace SFSharp.Runtime.Gta;

public sealed class SFGtaCamera(ISFMainThread mainThread) : ISFCamera
{
    private void RequireThread()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("GTA camera queries require the game thread.");
    }

    private GtaCameraView View
    {
        get
        {
            RequireThread();
            if (!CCamera.IsAvailable) throw new InvalidOperationException("The GTA camera is unavailable.");
            return new(CCamera.Pointer);
        }
    }

    public bool IsAvailable { get { RequireThread(); return CCamera.IsAvailable; } }
    public Vector3 Position => View.Position;
    public Vector3 Front => View.Front;
    public Vector3 Up => View.Up;
    public Vector3 Right => Vector3.Cross(Front, Up);
    public float FieldOfView => View.FieldOfView;
    public ushort Mode => View.Mode;
    public float ShakeForce => View.ShakeForce;
    public bool IsWideScreen => View.IsWideScreen;
    public float FadeAlpha => View.FadeAlpha;
    public byte FadeState => View.FadeState;
    public bool IsFading => FadeState == 1;
    public Vector3 GetLookAtPoint(float distance = 100f) => Position + Front * distance;
    public float GetDistanceTo(Vector3 point) => Vector3.Distance(Position, point);
    public bool TryWorldToScreen(Vector3 point, out Vector2 screen, out float cameraDepth)
    {
        RequireThread();
        SFGtaWorld.Validate(point);
        screen = default;
        cameraDepth = default;
        return GtaWorldQueries.IsWorldReady && IsAvailable && Front.LengthSquared() > 0 && GtaWorldQueries.Project(point, out screen, out cameraDepth);
    }
}
