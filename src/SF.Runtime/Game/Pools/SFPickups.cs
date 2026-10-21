using System.Numerics;

namespace SFSharp.Runtime.Game.Pools;

public sealed class SFPickups(ISFMainThread mainThread) : ISFPickups
{
    private ref readonly CPickupPool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP pickups require the game thread.");
            return ref CPickupPool.Instance;
        }
    }

    public int Count => Pool.Count;

    public bool Exists(int pickupIndex)
    {
        return pickupIndex >= 0 &&
               pickupIndex < SampOffsets.CPickupPool.MaxPickups &&
               Pool.GetServerId(pickupIndex) != 0;
    }

    public SFPickup Get(int pickupIndex)
    {
        return new(pickupIndex);
    }

    public bool TryGet(int pickupIndex, out SFPickup pickup)
    {
        if (!Exists(pickupIndex))
        {
            pickup = default;
            return false;
        }

        pickup = new SFPickup(pickupIndex);
        return true;
    }

    public int GetIndex(int serverPickupId)
    {
        return Pool.GetIndex(serverPickupId);
    }

    public void Create(in Pickup pickup, ushort pickupId)
    {
        Pool.Create(pickup, pickupId);
    }

    public void CreateWeapon(int modelId, Vector3 position, int ammo, ushort excludedOwnerId)
    {
        Pool.CreateWeapon(modelId, position, ammo, excludedOwnerId);
    }

    public void Delete(int pickupIndex)
    {
        Pool.Delete(pickupIndex);
    }

    public void DeleteWeapon(ushort excludedOwnerId)
    {
        Pool.DeleteWeapon(excludedOwnerId);
    }

    public void SendNotification(int pickupIndex)
    {
        Pool.SendNotification(pickupIndex);
    }

    public void Process()
    {
        Pool.Process();
    }

    public IEnumerable<int> EnumerateIndices()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP pickups require the game thread.");
        List<int> indices = [];
        for (int pickupIndex = 0; pickupIndex < SampOffsets.CPickupPool.MaxPickups; pickupIndex++)
        {
            if (Exists(pickupIndex))
            {
                indices.Add(pickupIndex);
            }
        }
        return indices;
    }

    public IEnumerable<SFPickup> Enumerate()
    {
        foreach (int pickupIndex in EnumerateIndices())
        {
            yield return new SFPickup(pickupIndex);
        }
    }
}
