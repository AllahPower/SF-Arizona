namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFActors(ISFMainThread mainThread) : ISFActors
{
    private ref readonly CActorPool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP actors require the game thread.");
            return ref CActorPool.Instance;
        }
    }

    public int LargestId => Pool.LargestId;

    public bool Exists(ushort actorId) => Pool.DoesExist(actorId);

    public SFActor Get(ushort actorId)
    {
        return new(actorId, Pool.Get(actorId));
    }

    public bool TryGet(ushort actorId, out SFActor actor)
    {
        if (!Pool.TryGet(actorId, out CActor* nativeActor))
        {
            actor = null!;
            return false;
        }

        actor = new SFActor(actorId, nativeActor);
        return true;
    }

    public ushort Find(SFPed ped)
    {
        ref readonly CActorPool pool = ref Pool;
        nint gamePed = ped.GamePedPointer;
        return gamePed == 0 ? ushort.MaxValue : pool.Find(gamePed);
    }

    public bool Delete(ushort actorId)
    {
        return Pool.Delete(actorId);
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        return Pool.GetExistingIds().ToArray();
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
