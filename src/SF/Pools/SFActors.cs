namespace SFSharp;

public sealed unsafe class SFActors
{
    public int LargestId => CActorPool.Instance.LargestId;

    public bool Exists(ushort actorId) => CActorPool.Instance.DoesExist(actorId);

    public SFActor Get(ushort actorId)
    {
        return new(actorId, CActorPool.Instance.Get(actorId));
    }

    public bool TryGet(ushort actorId, out SFActor actor)
    {
        if (!CActorPool.Instance.TryGet(actorId, out CActor* nativeActor))
        {
            actor = null!;
            return false;
        }

        actor = new SFActor(actorId, nativeActor);
        return true;
    }

    public ushort Find(SFPed ped)
    {
        return ped.Native == null ? ushort.MaxValue : CActorPool.Instance.Find(ped.Native);
    }

    public bool Delete(ushort actorId)
    {
        return CActorPool.Instance.Delete(actorId);
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        foreach (ushort actorId in CActorPool.Instance.GetExistingIds())
        {
            yield return actorId;
        }
    }

    public IEnumerable<SFActor> Enumerate()
    {
        foreach (ushort actorId in EnumerateIds())
        {
            if (TryGet(actorId, out SFActor actor))
            {
                yield return actor;
            }
        }
    }
}
