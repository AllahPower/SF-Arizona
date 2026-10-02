using System.Numerics;

namespace SFSharp.Runtime.Networking.RakNet.Rpc;

public static partial class SampRpc
{
    public static ClickPlayerRpc ParseClickPlayer(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static ClientJoinRpc ParseClientJoin(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        int ver = r.ReadInt32();
        byte mod = r.ReadUInt8();
        string nick = r.ReadStringUInt8Length();
        int cr1 = r.ReadInt32();
        string authKey = r.ReadStringUInt8Length();
        string clientVer = r.ReadStringUInt8Length();
        int cr2 = r.ReadInt32();
        return new(ver, mod, nick, cr1, authKey, clientVer, cr2);
    }

    public static SendEnterVehicleRpc ParseSendEnterVehicle(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadBool8());
    }

    public static SendCommandRpc ParseSendCommand(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadStringUInt32Length());
    }

    public static SpawnRpc ParseSpawn(OutgoingRpcArgs args)
    {
        return new();
    }

    public static DeathNotificationRpc ParseDeathNotification(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadUInt16());
    }

    public static DialogResponseRpc ParseDialogResponse(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8(), r.ReadUInt16(), r.ReadStringUInt8Length());
    }

    public static SendClickTextDrawRpc ParseSendClickTextDraw(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ScmEventOutgoingRpc ParseScmEventOutgoing(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
    }

    public static SendChatRpc ParseSendChat(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadStringUInt8Length());
    }

    public static ClientCheckResponseRpc ParseClientCheckResponse(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadInt32(), r.ReadUInt8());
    }

    public static SendVehicleDamageStatusRpc ParseSendVehicleDamageStatus(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadInt32(), r.ReadUInt8(), r.ReadUInt8());
    }

    public static GiveTakeDamageRpc ParseGiveTakeDamage(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        bool take = r.ReadBitBool();
        return new(take, r.ReadUInt16(), r.ReadFloat(), r.ReadInt32(), r.ReadInt32());
    }

    public static EditAttachedObjectOutgoingRpc ParseEditAttachedObjectOutgoing(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        int response = r.ReadInt32();
        int index = r.ReadInt32();
        int model = r.ReadInt32();
        int bone = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        Vector3 rot = ReadVector3(ref r);
        Vector3 scale = ReadVector3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(response, index, model, bone, pos, rot, scale, c1, c2);
    }

    public static EditObjectOutgoingRpc ParseEditObjectOutgoing(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        bool playerObj = r.ReadBitBool();
        ushort objId = r.ReadUInt16();
        int response = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        Vector3 rot = ReadVector3(ref r);
        return new(playerObj, objId, response, pos, rot);
    }

    public static SendExitVehicleRpc ParseSendExitVehicle(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetInteriorIdRpc ParseSetInteriorId(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static MapMarkerRpc ParseMapMarker(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SendRequestClassRpc ParseSendRequestClass(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SendRequestSpawnRpc ParseSendRequestSpawn(OutgoingRpcArgs args)
    {
        return new();
    }

    public static PickedUpPickupRpc ParsePickedUpPickup(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static MenuSelectRpc ParseMenuSelect(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static MenuQuitRpc ParseMenuQuit(OutgoingRpcArgs args)
    {
        return new();
    }

    public static VehicleDestroyedRpc ParseVehicleDestroyed(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static NpcJoinRpc ParseNpcJoin(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadUInt8(), r.ReadStringUInt8Length(), r.ReadInt32());
    }

    public static CameraTargetUpdateRpc ParseCameraTargetUpdate(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
    }

    public static GiveActorDamageRpc ParseGiveActorDamage(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool(), r.ReadUInt16(), r.ReadFloat(), r.ReadInt32(), r.ReadInt32());
    }

    public static SelectObjectOutgoingRpc ParseSelectObjectOutgoing(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadUInt16(), r.ReadInt32(), ReadVector3(ref r));
    }

    public static UpdateScoresAndPingsOutgoingRpc ParseUpdateScoresAndPingsOutgoing(OutgoingRpcArgs args)
    {
        return new();
    }

    public static ScriptCashRpc ParseScriptCash(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32());
    }

    public static SrvNetStatsRequestRpc ParseSrvNetStatsRequest(OutgoingRpcArgs args)
    {
        return new();
    }

    public static WeaponPickupDestroyRpc ParseWeaponPickupDestroy(OutgoingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

}
