using System.Numerics;

namespace SFSharp.Runtime.Game;

public readonly struct SFPickup(int index)
{
    public int Index => index;
    public bool Exists => CPickupPool.Instance.DoesExist(index);
    public int Handle => CPickupPool.Instance.GetHandle(index);
    public int ServerId => CPickupPool.Instance.GetServerId(index);
    public uint Timer => CPickupPool.Instance.GetTimer(index);
    public int Model => CPickupPool.Instance.GetPickup(index).Model;
    public int Type => CPickupPool.Instance.GetPickup(index).Type;
    public Vector3 Position => CPickupPool.Instance.GetPickup(index).Position;
    public WeaponPickup WeaponPickup => CPickupPool.Instance.GetWeaponPickup(index);
    public bool IsWeaponPickup => WeaponPickup.Exists != 0;
}
