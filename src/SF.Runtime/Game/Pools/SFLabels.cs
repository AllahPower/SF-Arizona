using System.Numerics;

namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFLabels(ISFMainThread mainThread)
    : ISFLabels
{
    private ref readonly CLabelPool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP labels require the game thread.");
            return ref CLabelPool.Instance;
        }
    }

    public bool Exists(ushort labelId) => Pool.DoesExist(labelId);

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

    public bool TryGetSnapshot(ushort labelId, out SFLabelSnapshot snapshot)
    {
        snapshot = default;
        return TryGet(labelId, out SFLabel label) && label.TryGetSnapshot(out snapshot);
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        return Pool.GetExistingIds().ToArray();
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
        Pool.Create(labelId, text, color, position, drawDistance, behindWalls, attachedToPlayer, attachedToVehicle);
    }

    public bool Delete(ushort labelId)
    {
        return Pool.Delete(labelId);
    }

    public void Draw()
    {
        Pool.Draw();
    }
}
