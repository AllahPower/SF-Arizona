namespace SFSharp;

public sealed unsafe class SFVehicles : ISFVehicles
{
    public int Count => CVehiclePool.Instance.Count;

    public bool Exists(ushort vehicleId) => CVehiclePool.Instance.DoesExist(vehicleId);

    public SFVehicle Get(ushort vehicleId)
    {
        return new SFVehicle(vehicleId, CVehiclePool.Instance.Get(vehicleId));
    }

    public bool TryGet(ushort vehicleId, out SFVehicle vehicle)
    {
        if (!CVehiclePool.Instance.TryGet(vehicleId, out CVehicle* nativeVehicle))
        {
            vehicle = null!;
            return false;
        }

        vehicle = new SFVehicle(vehicleId, nativeVehicle);
        return true;
    }

    public ushort GetNearestId()
    {
        return CVehiclePool.Instance.GetNearest();
    }

    public SFVehicle? GetNearest()
    {
        ushort vehicleId = GetNearestId();
        return TryGet(vehicleId, out SFVehicle vehicle) ? vehicle : null;
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        foreach (ushort vehicleId in CVehiclePool.Instance.GetExistingIds())
        {
            yield return vehicleId;
        }
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
        CVehiclePool.Instance.ChangeInterior(vehicleId, interiorId);
    }

    public void SetParams(ushort vehicleId, bool isObjective, bool isLocked)
    {
        CVehiclePool.Instance.SetParams(vehicleId, isObjective, isLocked);
    }
}
