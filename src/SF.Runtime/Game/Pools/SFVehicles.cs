namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFVehicles(ISFMainThread mainThread) : ISFVehicles
{
    private ref readonly CVehiclePool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP vehicle queries require the game thread.");
            return ref CVehiclePool.Instance;
        }
    }

    public int Count => Pool.Count;

    public bool Exists(ushort vehicleId) => Pool.DoesExist(vehicleId);

    public SFVehicle Get(ushort vehicleId)
    {
        return new SFVehicle(vehicleId, Pool.Get(vehicleId));
    }

    public bool TryGet(ushort vehicleId, out SFVehicle vehicle)
    {
        if (!Pool.TryGet(vehicleId, out CVehicle* nativeVehicle))
        {
            vehicle = null!;
            return false;
        }

        vehicle = new SFVehicle(vehicleId, nativeVehicle);
        return true;
    }

    public ushort GetNearestId()
    {
        return Pool.GetNearest();
    }

    public SFVehicle? GetNearest()
    {
        ushort vehicleId = GetNearestId();
        return TryGet(vehicleId, out SFVehicle vehicle) ? vehicle : null;
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        return Pool.GetExistingIds().ToArray();
    }

    public IEnumerable<SFVehicle> Enumerate()
    {
        foreach (ushort vehicleId in EnumerateIds())
        {
            if (TryGet(vehicleId, out SFVehicle vehicle))
            {
                yield return vehicle;
            }
        }
    }

    public bool TryGetSnapshot(ushort vehicleId, out SFVehicleSnapshot snapshot)
    {
        if (!TryGet(vehicleId, out SFVehicle vehicle) || !vehicle.IsAvailable)
        {
            snapshot = default;
            return false;
        }

        snapshot = new(
            Id: vehicleId,
            Exists: true,
            Handle: vehicle.Handle,
            IsInvulnerable: vehicle.IsInvulnerable,
            IsLightsOn: vehicle.IsLightsOn,
            IsLocked: vehicle.IsLocked,
            EngineState: vehicle.EngineState,
            PrimaryColor: vehicle.PrimaryColor,
            SecondaryColor: vehicle.SecondaryColor);
        return true;
    }

    public void ChangeInterior(ushort vehicleId, int interiorId)
    {
        Pool.ChangeInterior(vehicleId, interiorId);
    }

    public void SetParams(ushort vehicleId, bool isObjective, bool isLocked)
    {
        Pool.SetParams(vehicleId, isObjective, isLocked);
    }
}
