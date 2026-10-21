using System.Numerics;

namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFObjects(ISFMainThread mainThread) : ISFObjects
{
    private ref readonly CObjectPool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP objects require the game thread.");
            return ref CObjectPool.Instance;
        }
    }

    public int Count => Pool.GetCount();
    public int LargestId => Pool.LargestId;

    public bool Exists(ushort objectId) => Pool.IsAllocated(objectId);

    public SFObject Get(ushort objectId)
    {
        return new(objectId, Pool.Get(objectId));
    }

    public bool TryGet(ushort objectId, out SFObject obj)
    {
        if (!Pool.TryGet(objectId, out CObject* nativeObject))
        {
            obj = null!;
            return false;
        }

        obj = new SFObject(objectId, nativeObject);
        return true;
    }

    public bool Create(ushort objectId, int modelId, Vector3 position, Vector3 rotation, float drawDistance)
    {
        return Pool.Create(objectId, modelId, position, rotation, drawDistance);
    }

    public bool Delete(ushort objectId)
    {
        return Pool.Delete(objectId);
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        return Pool.GetAllocatedIds().ToArray();
    }

    public IEnumerable<SFObject> Enumerate()
    {
        foreach (ushort objectId in EnumerateIds())
        {
            if (TryGet(objectId, out SFObject obj))
            {
                yield return obj;
            }
        }
    }
}
