namespace SFSharp.Runtime.Interop.RakNet.Sync;

// WeaponsData (RakNetPacketId 204).

public readonly record struct WeaponSlot(byte Id, byte Unknown1, ushort Ammo)
{
    public static WeaponSlot Parse(ref SampBitStreamReader r)
    {
        byte id = r.ReadUInt8();
        byte unk = r.ReadUInt8();
        ushort ammo = r.ReadUInt16();
        return new WeaponSlot(id, unk, ammo);
    }
}

public readonly record struct WeaponsSyncData(
    ushort TargetId,
    ushort TargetActorId,
    WeaponSlot[] Slots)
{
    public static WeaponsSyncData Parse(ref SampBitStreamReader r)
    {
        ushort target = r.ReadUInt16();
        ushort actor = r.ReadUInt16();
        List<WeaponSlot> slots = new List<WeaponSlot>();
        while (r.RemainingBits >= 32)
        {
            slots.Add(WeaponSlot.Parse(ref r));
        }

        return new WeaponsSyncData(target, actor, slots.ToArray());
    }

    public override string ToString()
    {
        return $"target={TargetId} actor={TargetActorId} slots={Slots.Length}";
    }
}
