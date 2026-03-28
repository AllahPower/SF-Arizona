using System.Numerics;

namespace SFSharp;

public sealed unsafe class SFLabels
{
    public bool Exists(ushort labelId) => CLabelPool.Instance.DoesExist(labelId);

    public SFLabel Get(ushort labelId)
    {
        return new(labelId);
    }

    public bool TryGet(ushort labelId, out SFLabel label)
    {
        if (!Exists(labelId))
        {
            label = default;
            return false;
        }

        label = new SFLabel(labelId);
        return true;
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        foreach (ushort labelId in CLabelPool.Instance.GetExistingIds())
        {
            yield return labelId;
        }
    }

    public IEnumerable<SFLabel> Enumerate()
    {
        foreach (ushort labelId in EnumerateIds())
        {
            yield return new SFLabel(labelId);
        }
    }

    public void Create(ushort labelId, string text, uint color, Vector3 position, float drawDistance, bool behindWalls, ushort attachedToPlayer = ushort.MaxValue, ushort attachedToVehicle = ushort.MaxValue)
    {
        CLabelPool.Instance.Create(labelId, text, color, position, drawDistance, behindWalls, attachedToPlayer, attachedToVehicle);
    }

    public bool Delete(ushort labelId)
    {
        return CLabelPool.Instance.Delete(labelId);
    }

    public void Draw()
    {
        CLabelPool.Instance.Draw();
    }
}
