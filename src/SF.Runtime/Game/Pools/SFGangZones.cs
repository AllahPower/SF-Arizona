namespace SFSharp.Runtime.Game.Pools;

public sealed class SFGangZones(ISFMainThread mainThread) : ISFGangZones
{
    private ref readonly CGangZonePool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP gang zones require the game thread.");
            return ref CGangZonePool.Instance;
        }
    }

    public bool Exists(ushort zoneId) => Pool.DoesExist(zoneId);

    public SFGangZone Get(ushort zoneId)
    {
        return new(zoneId);
    }

    public bool TryGet(ushort zoneId, out SFGangZone zone)
    {
        if (!Exists(zoneId))
        {
            zone = default;
            return false;
        }

        zone = new SFGangZone(zoneId);
        return true;
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        return Pool.GetExistingIds().ToArray();
    }

    public IEnumerable<SFGangZone> Enumerate()
    {
        foreach (ushort zoneId in EnumerateIds())
        {
            yield return new SFGangZone(zoneId);
        }
    }

    public void Create(ushort zoneId, float left, float top, float right, float bottom, uint color)
    {
        Pool.Create(zoneId, left, top, right, bottom, color);
    }

    public void StartFlashing(ushort zoneId, uint color)
    {
        Pool.StartFlashing(zoneId, color);
    }

    public void StopFlashing(ushort zoneId)
    {
        Pool.StopFlashing(zoneId);
    }

    public void Delete(ushort zoneId)
    {
        Pool.Delete(zoneId);
    }

    public void Draw()
    {
        Pool.Draw();
    }
}
