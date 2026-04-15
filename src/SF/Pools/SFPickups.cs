using System.Numerics;

namespace SFSharp;

public sealed class SFPickups : ISFPickups
{
    public int Count => CPickupPool.Instance.Count;

    public bool Exists(int pickupIndex)
    {
        return pickupIndex >= 0 &&
               pickupIndex < SampOffsets.CPickupPool.MaxPickups &&
               CPickupPool.Instance.GetServerId(pickupIndex) != 0;
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
        return CPickupPool.Instance.GetIndex(serverPickupId);
    }

    public void Create(in Pickup pickup, ushort pickupId)
    {
        CPickupPool.Instance.Create(pickup, pickupId);
    }

    public void CreateWeapon(int modelId, Vector3 position, int ammo, ushort excludedOwnerId)
    {
        CPickupPool.Instance.CreateWeapon(modelId, position, ammo, excludedOwnerId);
    }

    public void Delete(int pickupIndex)
    {
        CPickupPool.Instance.Delete(pickupIndex);
    }

    public void DeleteWeapon(ushort excludedOwnerId)
    {
        CPickupPool.Instance.DeleteWeapon(excludedOwnerId);
    }

    public void SendNotification(int pickupIndex)
    {
        CPickupPool.Instance.SendNotification(pickupIndex);
    }

    public void Process()
    {
        CPickupPool.Instance.Process();
    }

    public IEnumerable<int> EnumerateIndices()
    {
        for (int pickupIndex = 0; pickupIndex < SampOffsets.CPickupPool.MaxPickups; pickupIndex++)
        {
            if (Exists(pickupIndex))
            {
                yield return pickupIndex;
            }
        }
    }

    public IEnumerable<SFPickup> Enumerate()
    {
        foreach (int pickupIndex in EnumerateIndices())
        {
            yield return new SFPickup(pickupIndex);
        }
    }
}
