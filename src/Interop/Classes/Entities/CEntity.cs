using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe ApplyTurnSpeedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, void>;
using unsafe DoesExistDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, int>;
using unsafe GetCollisionFlagDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, int>;
using unsafe GetDistanceToCameraDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, float>;
using unsafe GetDistanceToLocalPlayerDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, float>;
using unsafe GetDistanceToLocalPlayerNoHeightDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, float>;
using unsafe GetDistanceToPointDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, System.Numerics.Vector3, float>;
using unsafe GetEulerInvertedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, float*, float*, float*, void>;
using unsafe GetMatrixDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, SFSharp.Runtime.Interop.Classes.Entities.CMatrix*, void>;
using unsafe GetModelIndexDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, int>;
using unsafe GetSpeedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, System.Numerics.Vector3*, void>;
using unsafe GetTurnSpeedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, System.Numerics.Vector3*, void>;
using unsafe SetCollisionFlagDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, int, void>;
using unsafe SetCollisionProcessedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, int, void>;
using unsafe SetMatrixDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, SFSharp.Runtime.Interop.Classes.Entities.CMatrix, void>;
using unsafe SetModelIndexDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, int, void>;
using unsafe SetSpeedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, System.Numerics.Vector3, void>;
using unsafe SetTurnSpeedDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, System.Numerics.Vector3, void>;
using unsafe TeleportDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, System.Numerics.Vector3, void>;
using unsafe UpdateRwFrameDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Entities.CEntity*, void>;

namespace SFSharp.Runtime.Interop.Classes.Entities;

[StructLayout(LayoutKind.Explicit, Size = 0x48, Pack = 1)]
public unsafe struct CEntity
{
    [FieldOffset(0x40)]
    private readonly nint _gameEntity;

    [FieldOffset(0x44)]
    private readonly int _handle;

    private static readonly GetMatrixDelegate _getMatrix = (GetMatrixDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetMatrix);
    private static readonly SetMatrixDelegate _setMatrix = (SetMatrixDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.SetMatrix);
    private static readonly UpdateRwFrameDelegate _updateRwFrame = (UpdateRwFrameDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.UpdateRwFrame);
    private static readonly GetSpeedDelegate _getSpeed = (GetSpeedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetSpeed);
    private static readonly SetSpeedDelegate _setSpeed = (SetSpeedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.SetSpeed);
    private static readonly GetTurnSpeedDelegate _getTurnSpeed = (GetTurnSpeedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetTurnSpeed);
    private static readonly SetTurnSpeedDelegate _setTurnSpeed = (SetTurnSpeedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.SetTurnSpeed);
    private static readonly ApplyTurnSpeedDelegate _applyTurnSpeed = (ApplyTurnSpeedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.ApplyTurnSpeed);
    private static readonly SetModelIndexDelegate _setModelIndex = (SetModelIndexDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.SetModelIndex);
    private static readonly GetModelIndexDelegate _getModelIndex = (GetModelIndexDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetModelIndex);
    private static readonly TeleportDelegate _teleport = (TeleportDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.Teleport);
    private static readonly GetDistanceToLocalPlayerDelegate _getDistanceToLocalPlayer = (GetDistanceToLocalPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetDistanceToLocalPlayer);
    private static readonly GetDistanceToCameraDelegate _getDistanceToCamera = (GetDistanceToCameraDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetDistanceToCamera);
    private static readonly GetDistanceToLocalPlayerNoHeightDelegate _getDistanceToLocalPlayerNoHeight = (GetDistanceToLocalPlayerNoHeightDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetDistanceToLocalPlayerNoHeight);
    private static readonly GetDistanceToPointDelegate _getDistanceToPoint = (GetDistanceToPointDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetDistanceToPoint);
    private static readonly DoesExistDelegate _doesExist = (DoesExistDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.DoesExist);
    private static readonly GetCollisionFlagDelegate _getCollisionFlag = (GetCollisionFlagDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetCollisionFlag);
    private static readonly SetCollisionFlagDelegate _setCollisionFlag = (SetCollisionFlagDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.SetCollisionFlag);
    private static readonly SetCollisionProcessedDelegate _setCollisionProcessed = (SetCollisionProcessedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.SetCollisionProcessed);
    private static readonly GetEulerInvertedDelegate _getEulerInverted = (GetEulerInvertedDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CEntity.GetEulerInverted);

    public nint GameEntity => _gameEntity;
    public int Handle => _handle;
    public bool IsAvailable => _gameEntity != 0;

    public CMatrix GetMatrix()
    {
        CMatrix matrix = default;
        CMatrix* matrixPtr = &matrix;
        _getMatrix((CEntity*)Unsafe.AsPointer(ref this), matrixPtr);
        return matrix;
    }

    public void SetMatrix(CMatrix matrix)
    {
        _setMatrix((CEntity*)Unsafe.AsPointer(ref this), matrix);
    }

    public Vector3 GetPosition()
    {
        return GetMatrix().Position;
    }

    public void SetPosition(Vector3 position)
    {
        Teleport(position);
    }

    public Vector3 GetSpeed()
    {
        Vector3 speed = default;
        _getSpeed((CEntity*)Unsafe.AsPointer(ref this), &speed);
        return speed;
    }

    public void SetSpeed(Vector3 speed)
    {
        _setSpeed((CEntity*)Unsafe.AsPointer(ref this), speed);
    }

    public Vector3 GetTurnSpeed()
    {
        Vector3 turnSpeed = default;
        _getTurnSpeed((CEntity*)Unsafe.AsPointer(ref this), &turnSpeed);
        return turnSpeed;
    }

    public void SetTurnSpeed(Vector3 turnSpeed)
    {
        _setTurnSpeed((CEntity*)Unsafe.AsPointer(ref this), turnSpeed);
    }

    public void ApplyTurnSpeed()
    {
        _applyTurnSpeed((CEntity*)Unsafe.AsPointer(ref this));
    }

    public int GetModelIndex()
    {
        return _getModelIndex((CEntity*)Unsafe.AsPointer(ref this));
    }

    public void SetModelIndex(int modelId)
    {
        _setModelIndex((CEntity*)Unsafe.AsPointer(ref this), modelId);
    }

    public void Teleport(Vector3 position)
    {
        _teleport((CEntity*)Unsafe.AsPointer(ref this), position);
    }

    public float GetDistanceToLocalPlayer()
    {
        return _getDistanceToLocalPlayer((CEntity*)Unsafe.AsPointer(ref this));
    }

    public float GetDistanceToCamera()
    {
        return _getDistanceToCamera((CEntity*)Unsafe.AsPointer(ref this));
    }

    public float GetDistanceToLocalPlayerNoHeight()
    {
        return _getDistanceToLocalPlayerNoHeight((CEntity*)Unsafe.AsPointer(ref this));
    }

    public float GetDistanceToPoint(Vector3 position)
    {
        return _getDistanceToPoint((CEntity*)Unsafe.AsPointer(ref this), position);
    }

    public Vector3 GetEulerInverted()
    {
        float x = 0;
        float y = 0;
        float z = 0;
        _getEulerInverted((CEntity*)Unsafe.AsPointer(ref this), &x, &y, &z);
        return new Vector3(x, y, z);
    }

    public bool DoesExist()
    {
        return _doesExist((CEntity*)Unsafe.AsPointer(ref this)) != 0;
    }

    public bool GetCollisionFlag()
    {
        return _getCollisionFlag((CEntity*)Unsafe.AsPointer(ref this)) != 0;
    }

    public void SetCollisionFlag(bool enable)
    {
        _setCollisionFlag((CEntity*)Unsafe.AsPointer(ref this), enable ? 1 : 0);
    }

    public void SetCollisionProcessed(bool processed)
    {
        _setCollisionProcessed((CEntity*)Unsafe.AsPointer(ref this), processed ? 1 : 0);
    }

    public void UpdateRwFrame()
    {
        _updateRwFrame((CEntity*)Unsafe.AsPointer(ref this));
    }
}
