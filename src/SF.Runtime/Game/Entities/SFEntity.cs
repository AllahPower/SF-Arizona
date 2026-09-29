using System.Numerics;

namespace SFSharp.Runtime.Game.Entities;

public abstract unsafe class SFEntity : ISFEntity
{
    protected abstract CEntity* NativeEntity { get; }

    public nint NativePointer => (nint)NativeEntity;
    public nint GamePointer => NativeEntity is null ? 0 : NativeEntity->GameEntity;
    public int Handle => NativeEntity is null ? 0 : NativeEntity->Handle;
    public bool IsAvailable => NativeEntity is not null && NativeEntity->IsAvailable;
    public bool Exists => NativeEntity is not null && NativeEntity->DoesExist();
    public Vector3 Position => NativeEntity is null ? Vector3.Zero : NativeEntity->GetPosition();
    public Vector3 Speed => NativeEntity is null ? Vector3.Zero : NativeEntity->GetSpeed();
    public Vector3 TurnSpeed => NativeEntity is null ? Vector3.Zero : NativeEntity->GetTurnSpeed();
    public Vector3 EulerInverted => NativeEntity is null ? Vector3.Zero : NativeEntity->GetEulerInverted();
    public int ModelIndex => NativeEntity is null ? 0 : NativeEntity->GetModelIndex();
    public bool CollisionEnabled => NativeEntity is not null && NativeEntity->GetCollisionFlag();

    public void SetPosition(Vector3 position)
    {
        if (NativeEntity is not null)
        {
            NativeEntity->SetPosition(position);
        }
    }

    public void SetSpeed(Vector3 speed)
    {
        if (NativeEntity is not null)
        {
            NativeEntity->SetSpeed(speed);
        }
    }

    public void SetTurnSpeed(Vector3 turnSpeed)
    {
        if (NativeEntity is not null)
        {
            NativeEntity->SetTurnSpeed(turnSpeed);
        }
    }

    public void SetModelIndex(int modelId)
    {
        if (NativeEntity is not null)
        {
            NativeEntity->SetModelIndex(modelId);
        }
    }

    public void SetCollisionEnabled(bool enabled)
    {
        if (NativeEntity is not null)
        {
            NativeEntity->SetCollisionFlag(enabled);
        }
    }

    public float GetDistanceToLocalPlayer()
    {
        return NativeEntity is null ? 0f : NativeEntity->GetDistanceToLocalPlayer();
    }

    public float GetDistanceToPoint(Vector3 position)
    {
        return NativeEntity is null ? 0f : NativeEntity->GetDistanceToPoint(position);
    }
}
