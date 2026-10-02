using System.Numerics;

namespace SFSharp.Runtime.Networking.RakNet.Rpc;

public static partial class SampRpc
{
    public static ChatMessageRpc ParseChatMessage(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        string prefix = r.ReadStringUInt8Length();
        uint prefixColor = r.ReadUInt32();
        string text = r.ReadStringUInt32Length();
        return new(prefix, prefixColor, text);
    }

    public static EnterVehicleRpc ParseEnterVehicleIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16(), r.ReadBool8());
    }

    public static ExitVehicleRpc ParseExitVehicleIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16());
    }

    public static ClickTextDrawIncomingRpc ParseClickTextDrawIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool(), r.ReadInt32());
    }

    public static ScmEventIncomingRpc ParseScmEventIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
    }

    public static ClientCheckIncomingRpc ParseClientCheckIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadInt32(), r.ReadUInt16(), r.ReadUInt16());
    }

    public static UpdateVehicleDamageStatusRpc ParseUpdateVehicleDamageStatusIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadInt32(), r.ReadUInt8(), r.ReadUInt8());
    }

    public static UpdateScoresAndPingsRpc ParseUpdateScoresAndPings(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        Dictionary<ushort, ScorePingRpc> players = [];
        while (r.RemainingBits >= 80)
        {
            ushort playerId = r.ReadUInt16();
            players[playerId] = new ScorePingRpc(r.ReadInt32(), r.ReadInt32());
        }

        return new UpdateScoresAndPingsRpc
        {
            Players = players
        };
    }

    public static EditAttachedObjectIncomingRpc ParseEditAttachedObjectIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static EditObjectIncomingRpc ParseEditObjectIncoming(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool(), r.ReadUInt16());
    }

    public static RequestClassResponseRpc ParseRequestClassResponse(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        bool canSpawn = r.ReadBool8();
        byte team = r.ReadUInt8();
        int skin = r.ReadInt32();
        byte unused = r.ReadUInt8();
        Vector3 pos = ReadVector3(ref r);
        float rot = r.ReadFloat();
        int w1 = r.ReadInt32(); int w2 = r.ReadInt32(); int w3 = r.ReadInt32();
        int a1 = r.ReadInt32(); int a2 = r.ReadInt32(); int a3 = r.ReadInt32();
        return new(canSpawn, team, skin, unused, pos, rot, w1, w2, w3, a1, a2, a3);
    }

    public static RequestSpawnResponseRpc ParseRequestSpawnResponse(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBool8());
    }

    public static DestroyWeaponPickupRpc ParseDestroyWeaponPickup(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerAttachedObjectRpc ParseSetPlayerAttachedObject(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        PlayerAttachedObjectInfoRpc attachedObject = new()
        {
            ModelId = 0,
            Bone = 0,
            Offset = Vector3.Zero,
            Rotation = Vector3.Zero,
            Scale = Vector3.Zero,
            Color1 = 0,
            Color2 = 0
        };

        ushort playerId = r.ReadUInt16();
        int index = r.ReadInt32();
        bool create = r.ReadBitBool();
        if (r.RemainingBits >= (32 * 11))
        {
            attachedObject = new PlayerAttachedObjectInfoRpc
            {
                ModelId = r.ReadInt32(),
                Bone = r.ReadInt32(),
                Offset = ReadVector3(ref r),
                Rotation = ReadVector3(ref r),
                Scale = ReadVector3(ref r),
                Color1 = r.ReadInt32(),
                Color2 = r.ReadInt32()
            };
        }

        return new SetPlayerAttachedObjectRpc
        {
            PlayerId = playerId,
            Index = index,
            Create = create,
            Object = attachedObject
        };
    }

    public static SelectObjectRpc ParseSelectObject(IncomingRpcArgs args)
    {
        return new();
    }

    public static ServerNetStatsResponseRpc ParseServerNetStatsResponse(IncomingRpcArgs args)
    {
        return new();
    }

}
