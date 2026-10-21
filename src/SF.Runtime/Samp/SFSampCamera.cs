using System.Numerics;

namespace SFSharp.Runtime.Samp;

public sealed class SFSampCamera(SFPlayers players, SFRpc rpc, ISFMainThread mainThread)
    : ISFSampCamera
{
    private ref readonly CLocalPlayer Local
    {
        get
        {
            RequireThread();
            return ref CLocalPlayer.Instance;
        }
    }

    private void RequireThread()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP camera queries require the game thread.");
    }

    public byte AimCameraMode => Local.AimData.CameraMode;
    public Vector3 AimDirection => Local.AimData.AimDirection;
    public Vector3 AimPosition => Local.AimData.AimPosition;
    public float AimZ => Local.AimData.AimZ;
    public byte AimZoom => Local.AimData.CameraExtZoom;
    public sbyte AspectRatio => Local.AimData.AspectRatio;

    public SFLocalPlayerCameraTarget Target { get { RequireThread(); return players.Local.CameraTarget; } }

    public IAsyncEnumerable<SetPlayerCameraPosRpc> StreamSetPosition(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.SetPlayerCameraPos, SampRpc.ParseSetPlayerCameraPos, token);
    }

    public IAsyncEnumerable<SetPlayerCameraLookAtRpc> StreamSetLookAt(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.SetPlayerCameraLookAt, SampRpc.ParseSetPlayerCameraLookAt, token);
    }

    public IAsyncEnumerable<InterpolateCameraRpc> StreamInterpolate(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.InterpolateCamera, SampRpc.ParseInterpolateCamera, token);
    }

    public IAsyncEnumerable<AttachCameraToObjectRpc> StreamAttachToObject(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.AttachCameraToObject, SampRpc.ParseAttachCameraToObject, token);
    }

    public IAsyncEnumerable<SetCameraBehindPlayerRpc> StreamResetBehindPlayer(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.SetCameraBehindPlayer, SampRpc.ParseSetCameraBehindPlayer, token);
    }

    public IAsyncEnumerable<ToggleCameraTargetRpc> StreamToggleTarget(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.ToggleCameraTarget, SampRpc.ParseToggleCameraTarget, token);
    }

    public IAsyncEnumerable<SetPlayerObjectNoCameraColRpc> StreamDisableObjectCollision(CancellationToken token = default)
    {
        return rpc.Stream(SampRpcId.SetPlayerObjectNoCameraCol, SampRpc.ParseSetPlayerObjectNoCameraCol, token);
    }

    public IDisposable OnSetPosition(Action<SetPlayerCameraPosRpc> handler, CancellationToken token = default)
    {
        return rpc.Bind(SampRpcId.SetPlayerCameraPos, SampRpc.ParseSetPlayerCameraPos,
            (rpc, _) => handler(rpc), token, "Camera.SetPos");
    }

    public IDisposable OnSetLookAt(Action<SetPlayerCameraLookAtRpc> handler, CancellationToken token = default)
    {
        return rpc.Bind(SampRpcId.SetPlayerCameraLookAt, SampRpc.ParseSetPlayerCameraLookAt,
            (rpc, _) => handler(rpc), token, "Camera.SetLookAt");
    }

    public IDisposable OnInterpolate(Action<InterpolateCameraRpc> handler, CancellationToken token = default)
    {
        return rpc.Bind(SampRpcId.InterpolateCamera, SampRpc.ParseInterpolateCamera,
            (rpc, _) => handler(rpc), token, "Camera.Interpolate");
    }

    public IDisposable OnAttachToObject(Action<AttachCameraToObjectRpc> handler, CancellationToken token = default)
    {
        return rpc.Bind(SampRpcId.AttachCameraToObject, SampRpc.ParseAttachCameraToObject,
            (rpc, _) => handler(rpc), token, "Camera.AttachToObject");
    }

    public IDisposable OnResetBehindPlayer(Action<SetCameraBehindPlayerRpc> handler, CancellationToken token = default)
    {
        return rpc.Bind(SampRpcId.SetCameraBehindPlayer, SampRpc.ParseSetCameraBehindPlayer,
            (rpc, _) => handler(rpc), token, "Camera.ResetBehind");
    }

    public IDisposable OnToggleTarget(Action<ToggleCameraTargetRpc> handler, CancellationToken token = default)
    {
        return rpc.Bind(SampRpcId.ToggleCameraTarget, SampRpc.ParseToggleCameraTarget,
            (rpc, _) => handler(rpc), token, "Camera.ToggleTarget");
    }

    public IAsyncEnumerable<CameraTargetUpdateRpc> StreamOutgoingTargetUpdate(CancellationToken token = default)
    {
        return rpc.StreamOutgoing(SampRpcId.CameraTargetUpdate, SampRpc.ParseCameraTargetUpdate, token);
    }
}
