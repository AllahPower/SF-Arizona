using System.Numerics;

namespace SFSharp.Runtime.Game;

public abstract class SFPlayer
{
    public abstract ushort Id { get; }
    public abstract bool IsLocal { get; }
    public abstract bool IsConnected { get; }

    public virtual string? Name => IsLocal
        ? CPlayerPool.Instance.GetLocalPlayerName()
        : CPlayerPool.Instance.GetName(Id);

    public virtual int Score => IsLocal
        ? CPlayerPool.Instance.GetLocalPlayerScore()
        : CPlayerPool.Instance.GetScore(Id);

    public virtual int Ping => IsLocal
        ? CPlayerPool.Instance.GetLocalPlayerPing()
        : CPlayerPool.Instance.GetPing(Id);

    public virtual bool IsNpc => !IsLocal && CPlayerPool.Instance.IsNpc(Id);

    public abstract SFPed? Ped { get; }
    public virtual Vector3? Position => Ped?.Position;
    public virtual Vector3? Speed => Ped?.Speed;
    public virtual float? Rotation => Ped?.Rotation;
    public virtual float? Health => Ped?.Health;
    public virtual float? Armour => Ped?.Armour;
    public virtual SFVehicle? Vehicle => Ped?.Vehicle;
    public virtual bool IsInVehicle => Vehicle is not null;
}
