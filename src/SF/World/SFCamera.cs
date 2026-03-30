using System.Numerics;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public sealed class SFCamera
{
    public bool IsAvailable => CCamera.IsAvailable;

    public Vector3 Position => CCamera.IsAvailable ? CCamera.Position : Vector3.Zero;
    public Vector3 Front => CCamera.IsAvailable ? CCamera.Front : Vector3.UnitY;
    public Vector3 Up => CCamera.IsAvailable ? CCamera.Up : Vector3.UnitZ;
    public Vector3 Right => CCamera.IsAvailable ? CCamera.Right : Vector3.UnitX;
    public float FieldOfView => CCamera.IsAvailable ? CCamera.FieldOfView : 70f;
    public byte Mode => CCamera.IsAvailable ? CCamera.Mode : (byte)0;
    public float ShakeForce => CCamera.IsAvailable ? CCamera.ShakeForce : 0f;
    public bool IsWideScreen => CCamera.IsAvailable && CCamera.IsWideScreen;
    public byte FadeAlpha => CCamera.IsAvailable ? CCamera.FadeAlpha : (byte)0;
    public byte FadeState => CCamera.IsAvailable ? CCamera.FadeState : (byte)0;
    public bool IsFading => FadeState != 0;
    public CMatrix Matrix => CCamera.IsAvailable ? CCamera.GetMatrix() : default;

    public Vector3 GetLookAtPoint(float distance = 100f)
    {
        return CCamera.IsAvailable ? CCamera.GetLookAtPoint(distance) : Vector3.Zero;
    }

    public float GetDistanceTo(Vector3 point)
    {
        return CCamera.IsAvailable ? CCamera.GetDistanceTo(point) : 0f;
    }

    public byte AimCameraMode => CLocalPlayer.Instance.AimData.CameraMode;
    public Vector3 AimDirection => CLocalPlayer.Instance.AimData.AimDirection;
    public Vector3 AimPosition => CLocalPlayer.Instance.AimData.AimPosition;
    public float AimZ => CLocalPlayer.Instance.AimData.AimZ;
    public byte AimZoom => CLocalPlayer.Instance.AimData.CameraExtZoom;
    public sbyte AspectRatio => CLocalPlayer.Instance.AimData.AspectRatio;

    public SFLocalPlayerCameraTarget Target => SF.Players.Local.CameraTarget;

    public IAsyncEnumerable<SetPlayerCameraPosRpc> StreamSetPosition(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.SetPlayerCameraPos, SampRpc.ParseSetPlayerCameraPos, token);
    }

    public IAsyncEnumerable<SetPlayerCameraLookAtRpc> StreamSetLookAt(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.SetPlayerCameraLookAt, SampRpc.ParseSetPlayerCameraLookAt, token);
    }

    public IAsyncEnumerable<InterpolateCameraRpc> StreamInterpolate(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.InterpolateCamera, SampRpc.ParseInterpolateCamera, token);
    }

    public IAsyncEnumerable<AttachCameraToObjectRpc> StreamAttachToObject(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.AttachCameraToObject, SampRpc.ParseAttachCameraToObject, token);
    }

    public IAsyncEnumerable<SetCameraBehindPlayerRpc> StreamResetBehindPlayer(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.SetCameraBehindPlayer, SampRpc.ParseSetCameraBehindPlayer, token);
    }

    public IAsyncEnumerable<ToggleCameraTargetRpc> StreamToggleTarget(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.ToggleCameraTarget, SampRpc.ParseToggleCameraTarget, token);
    }

    public IAsyncEnumerable<SetPlayerObjectNoCameraColRpc> StreamDisableObjectCollision(CancellationToken token = default)
    {
        return SF.Rpc.Stream(ERpcId.SetPlayerObjectNoCameraCol, SampRpc.ParseSetPlayerObjectNoCameraCol, token);
    }

    public IDisposable OnSetPosition(Action<SetPlayerCameraPosRpc> handler, CancellationToken token = default)
    {
        return SF.Rpc.Bind(ERpcId.SetPlayerCameraPos, SampRpc.ParseSetPlayerCameraPos,
            (rpc, _) => handler(rpc), token, "Camera.SetPos");
    }

    public IDisposable OnSetLookAt(Action<SetPlayerCameraLookAtRpc> handler, CancellationToken token = default)
    {
        return SF.Rpc.Bind(ERpcId.SetPlayerCameraLookAt, SampRpc.ParseSetPlayerCameraLookAt,
            (rpc, _) => handler(rpc), token, "Camera.SetLookAt");
    }

    public IDisposable OnInterpolate(Action<InterpolateCameraRpc> handler, CancellationToken token = default)
    {
        return SF.Rpc.Bind(ERpcId.InterpolateCamera, SampRpc.ParseInterpolateCamera,
            (rpc, _) => handler(rpc), token, "Camera.Interpolate");
    }

    public IDisposable OnAttachToObject(Action<AttachCameraToObjectRpc> handler, CancellationToken token = default)
    {
        return SF.Rpc.Bind(ERpcId.AttachCameraToObject, SampRpc.ParseAttachCameraToObject,
            (rpc, _) => handler(rpc), token, "Camera.AttachToObject");
    }

    public IDisposable OnResetBehindPlayer(Action<SetCameraBehindPlayerRpc> handler, CancellationToken token = default)
    {
        return SF.Rpc.Bind(ERpcId.SetCameraBehindPlayer, SampRpc.ParseSetCameraBehindPlayer,
            (rpc, _) => handler(rpc), token, "Camera.ResetBehind");
    }

    public IDisposable OnToggleTarget(Action<ToggleCameraTargetRpc> handler, CancellationToken token = default)
    {
        return SF.Rpc.Bind(ERpcId.ToggleCameraTarget, SampRpc.ParseToggleCameraTarget,
            (rpc, _) => handler(rpc), token, "Camera.ToggleTarget");
    }

    public IAsyncEnumerable<CameraTargetUpdateRpc> StreamOutgoingTargetUpdate(CancellationToken token = default)
    {
        return SF.Rpc.StreamOutgoing(ERpcId.CameraTargetUpdate, SampRpc.ParseCameraTargetUpdate, token);
    }
}
