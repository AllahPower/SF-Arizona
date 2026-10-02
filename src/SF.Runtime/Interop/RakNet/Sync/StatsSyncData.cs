namespace SFSharp.Runtime.Interop.RakNet.Sync;

// StatsData (RakNetPacketId 205).

public readonly record struct StatsSyncData(int Money, int DrunkLevel)
{
    public static StatsSyncData Parse(ref SampBitStreamReader r)
    {
        int money = r.ReadInt32();
        int drunk = r.ReadInt32();
        return new StatsSyncData(money, drunk);
    }

    public override string ToString()
    {
        return $"money={Money} drunk={DrunkLevel}";
    }
}
