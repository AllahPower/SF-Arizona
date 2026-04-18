using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Network.RakNet.Rpc;

public static partial class RpcParserCatalog
{
    private static void RegisterOutgoing(RpcParserRegistry registry)
    {
        // Outgoing (client -> server)
        RegisterOutgoing(registry, ERpcId.ClickPlayer, SampRpc.ParseClickPlayer);
        RegisterOutgoing(registry, ERpcId.ClientJoin, SampRpc.ParseClientJoin);
        RegisterOutgoing(registry, ERpcId.EnterVehicle, SampRpc.ParseSendEnterVehicle, name: "SendEnterVehicle");
        RegisterOutgoing(registry, ERpcId.ServerCommand, SampRpc.ParseSendCommand);
        RegisterOutgoing(registry, ERpcId.Spawn, SampRpc.ParseSpawn);
        RegisterOutgoing(registry, ERpcId.Death, SampRpc.ParseDeathNotification);
        RegisterOutgoing(registry, ERpcId.DialogResponse, SampRpc.ParseDialogResponse);
        RegisterOutgoing(registry, ERpcId.ClickTextDraw, SampRpc.ParseSendClickTextDraw, name: "SendClickTextDraw");
        RegisterOutgoing(registry, ERpcId.ScmEvent, SampRpc.ParseScmEventOutgoing, name: "ScmEventOutgoing");
        RegisterOutgoing(registry, ERpcId.Chat, SampRpc.ParseSendChat, name: "SendChat");
        RegisterOutgoing(registry, ERpcId.ClientCheck, SampRpc.ParseClientCheckResponse, name: "ClientCheckResponse");
        RegisterOutgoing(registry, ERpcId.UpdateVehicleDamageStatus, SampRpc.ParseSendVehicleDamageStatus, name: "SendVehicleDamageStatus");
        RegisterOutgoing(registry, ERpcId.GiveTakeDamage, SampRpc.ParseGiveTakeDamage);
        RegisterOutgoing(registry, ERpcId.EditAttachedObject, SampRpc.ParseEditAttachedObjectOutgoing, name: "EditAttachedObjectOutgoing");
        RegisterOutgoing(registry, ERpcId.EditObject, SampRpc.ParseEditObjectOutgoing, name: "EditObjectOutgoing");
        RegisterOutgoing(registry, ERpcId.ExitVehicle, SampRpc.ParseSendExitVehicle, name: "SendExitVehicle");
        RegisterOutgoing(registry, ERpcId.SetInteriorId, SampRpc.ParseSetInteriorId);
        RegisterOutgoing(registry, ERpcId.MapMarker, SampRpc.ParseMapMarker);
        RegisterOutgoing(registry, ERpcId.RequestClass, SampRpc.ParseSendRequestClass, name: "SendRequestClass");
        RegisterOutgoing(registry, ERpcId.RequestSpawn, SampRpc.ParseSendRequestSpawn, name: "SendRequestSpawn");
        RegisterOutgoing(registry, ERpcId.PickedUpPickup, SampRpc.ParsePickedUpPickup);
        RegisterOutgoing(registry, ERpcId.MenuSelect, SampRpc.ParseMenuSelect);
        RegisterOutgoing(registry, ERpcId.MenuQuit, SampRpc.ParseMenuQuit);
        RegisterOutgoing(registry, ERpcId.VehicleDestroyed, SampRpc.ParseVehicleDestroyed);
        RegisterOutgoing(registry, ERpcId.NpcJoin, SampRpc.ParseNpcJoin);
        RegisterOutgoing(registry, ERpcId.CameraTargetUpdate, SampRpc.ParseCameraTargetUpdate);
        RegisterOutgoing(registry, ERpcId.GiveActorDamage, SampRpc.ParseGiveActorDamage);
        RegisterOutgoing(registry, ERpcId.UpdateScoresAndPings, SampRpc.ParseUpdateScoresAndPingsOutgoing, name: "UpdateScoresAndPingsOutgoing");
        RegisterOutgoing(registry, ERpcId.SelectObject, SampRpc.ParseSelectObjectOutgoing, name: "SelectObjectOutgoing");
        RegisterOutgoing(registry, ERpcId.ScriptCash, SampRpc.ParseScriptCash);
        RegisterOutgoing(registry, ERpcId.SrvNetStats, SampRpc.ParseSrvNetStatsRequest, name: "SrvNetStatsRequest");
        RegisterOutgoing(registry, ERpcId.WeaponPickupDestroy, SampRpc.ParseWeaponPickupDestroy);
    }
}
