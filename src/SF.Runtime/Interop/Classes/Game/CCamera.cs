using System.Numerics;

namespace SFSharp.Runtime.Interop.Classes;

public static unsafe class CCamera
{
    private static readonly nint _base = GtaOffsets.CCamera.TheCamera;

    public static bool IsAvailable => NativeMemoryValidator.IsReadable(_base, (nuint)GtaOffsets.CCamera.Size);
    public static nint Pointer => _base;

    private static nint ActiveCam => _base + GtaOffsets.CCamera.ActiveCam;

    public static Vector3 Position => *(Vector3*)(ActiveCam + GtaOffsets.CCam.Source);
    public static Vector3 Front => *(Vector3*)(ActiveCam + GtaOffsets.CCam.Front);
    public static Vector3 Up => *(Vector3*)(ActiveCam + GtaOffsets.CCam.Up);
    public static Vector3 Right => Vector3.Cross(Front, Up);
    public static float FieldOfView => *(float*)(ActiveCam + GtaOffsets.CCam.Fov);
    public static byte Mode => *(byte*)(ActiveCam + GtaOffsets.CCam.Mode);

    public static byte FadeAlpha => *(byte*)(_base + GtaOffsets.CCamera.FadeAlpha);
    public static byte FadeState => *(byte*)(_base + GtaOffsets.CCamera.FadeState);
    public static bool IsWideScreen => *(byte*)(_base + GtaOffsets.CCamera.WideScreenOn) != 0;
    public static float ShakeForce => *(float*)(_base + GtaOffsets.CCamera.ShakeForce);

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
