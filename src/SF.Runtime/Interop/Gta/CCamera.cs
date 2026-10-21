using System.Numerics;

namespace SFSharp.Runtime.Interop.Gta;

public static unsafe class CCamera
{
    private static nint Base => ModuleResolver.GetGameAddress(GtaOffsets.CCamera.TheCameraRva);
    private static GtaCameraView View => new(Base);

    public static bool IsAvailable => NativeMemoryValidator.IsReadable(Base, (nuint)GtaOffsets.CCamera.Size) && View.HasActiveCamera;
    public static nint Pointer => Base;

    public static Vector3 Position => View.Position;
    public static Vector3 Front => View.Front;
    public static Vector3 Up => View.Up;
    public static Vector3 Right => Vector3.Cross(Front, Up);
    public static float FieldOfView => View.FieldOfView;
    public static ushort Mode => View.Mode;

    public static float FadeAlpha => View.FadeAlpha;
    public static byte FadeState => View.FadeState;
    public static bool IsWideScreen => View.IsWideScreen;
    public static float ShakeForce => View.ShakeForce;

    public static Vector3 GetLookAtPoint(float distance = 100f)
    {
        return Position + Front * distance;
    }

    public static float GetDistanceTo(Vector3 point)
    {
        return Vector3.Distance(Position, point);
    }

    public static CMatrix GetMatrix()
    {
        Vector3 front = Front;
        Vector3 up = Up;
        Vector3 right = Vector3.Cross(front, up);

        return new CMatrix
        {
            Right = right,
            Up = up,
            At = front,
            Position = Position
        };
    }
}
