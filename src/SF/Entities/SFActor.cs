namespace SFSharp.Runtime.Game;

public sealed unsafe class SFActor : SFEntity
{
    private readonly CActor* _native;

    internal SFActor(ushort id, CActor* native)
    {
        Id = id;
        _native = native;
    }

    public ushort Id { get; }
    public new bool Exists => _native != null && CActorPool.Instance.DoesExist(Id);
    public nint GamePedPointer => _native is null ? 0 : _native->GamePed;
    public int Marker => _native is null ? 0 : _native->Marker;
    public int Arrow => _native is null ? 0 : _native->Arrow;
    public bool NeedsToCreateMarker => _native is not null && _native->NeedsToCreateMarker;
    public bool IsInvulnerable => _native is not null && _native->IsInvulnerable;
    public float Health => _native is null ? 0f : _native->GetHealth();

    protected override CEntity* NativeEntity => _native is null ? null : _native->GetEntityPointer();

    public void SetHealth(float health)
    {
        if (_native is not null)
        {
            _native->SetHealth(health);
        }
    }

    public void SetRotation(float angle)
    {
        if (_native is not null)
        {
            _native->SetRotation(angle);
        }
    }

    public void SetInvulnerable(bool enable)
    {
        if (_native is not null)
        {
            _native->SetInvulnerable(enable);
        }
    }

    public void Destroy()
    {
        if (_native is not null)
        {
            _native->Destroy();
        }
    }

    public void PerformAnimation(string animationName, string ifpName, float frameDelta, bool lockA, bool lockX, bool lockY, bool lockF, int time)
    {
        if (_native is not null)
        {
            _native->PerformAnimation(animationName, ifpName, frameDelta, lockA, lockX, lockY, lockF, time);
        }
    }
}
