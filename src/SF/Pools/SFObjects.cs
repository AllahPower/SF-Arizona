using System.Numerics;

namespace SFSharp;

public sealed unsafe class SFObjects : ISFObjects
{
    public int Count => CObjectPool.Instance.GetCount();
    public int LargestId => CObjectPool.Instance.LargestId;

    public bool Exists(ushort objectId) => CObjectPool.Instance.IsAllocated(objectId);

    public SFObject Get(ushort objectId)
    {
        return new(objectId, CObjectPool.Instance.Get(objectId));
    }

    public bool TryGet(ushort objectId, out SFObject obj)
    {
        if (!CObjectPool.Instance.TryGet(objectId, out CObject* nativeObject))
        {
            obj = null!;
            return false;
        }

        obj = new SFObject(objectId, nativeObject);
        return true;
    }

    public bool Create(ushort objectId, int modelId, Vector3 position, Vector3 rotation, float drawDistance)
    {
        return CObjectPool.Instance.Create(objectId, modelId, position, rotation, drawDistance);
    }

    public bool Delete(ushort objectId)
    {
        return CObjectPool.Instance.Delete(objectId);
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        foreach (ushort objectId in CObjectPool.Instance.GetAllocatedIds())
        {
            yield return objectId;
        }
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
