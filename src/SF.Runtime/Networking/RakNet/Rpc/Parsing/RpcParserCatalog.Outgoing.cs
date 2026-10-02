namespace SFSharp.Runtime.Networking.RakNet.Rpc.Parsing;

public static partial class RpcParserCatalog
{
    private static void RegisterOutgoing(RpcParserRegistry registry)
    {
        // Outgoing (client -> server)
        RegisterOutgoing(registry, SampRpcId.ClickPlayer, SampRpc.ParseClickPlayer);
        RegisterOutgoing(registry, SampRpcId.ClientJoin, SampRpc.ParseClientJoin);
        RegisterOutgoing(registry, SampRpcId.EnterVehicle, SampRpc.ParseSendEnterVehicle, name: "SendEnterVehicle");
        RegisterOutgoing(registry, SampRpcId.ServerCommand, SampRpc.ParseSendCommand);
        RegisterOutgoing(registry, SampRpcId.Spawn, SampRpc.ParseSpawn);
        RegisterOutgoing(registry, SampRpcId.Death, SampRpc.ParseDeathNotification);
        RegisterOutgoing(registry, SampRpcId.DialogResponse, SampRpc.ParseDialogResponse);
        RegisterOutgoing(registry, SampRpcId.ClickTextDraw, SampRpc.ParseSendClickTextDraw, name: "SendClickTextDraw");
        RegisterOutgoing(registry, SampRpcId.ScmEvent, SampRpc.ParseScmEventOutgoing, name: "ScmEventOutgoing");
        RegisterOutgoing(registry, SampRpcId.Chat, SampRpc.ParseSendChat, name: "SendChat");
        RegisterOutgoing(registry, SampRpcId.ClientCheck, SampRpc.ParseClientCheckResponse, name: "ClientCheckResponse");
        RegisterOutgoing(registry, SampRpcId.UpdateVehicleDamageStatus, SampRpc.ParseSendVehicleDamageStatus, name: "SendVehicleDamageStatus");
        RegisterOutgoing(registry, SampRpcId.GiveTakeDamage, SampRpc.ParseGiveTakeDamage);
        RegisterOutgoing(registry, SampRpcId.EditAttachedObject, SampRpc.ParseEditAttachedObjectOutgoing, name: "EditAttachedObjectOutgoing");
        RegisterOutgoing(registry, SampRpcId.EditObject, SampRpc.ParseEditObjectOutgoing, name: "EditObjectOutgoing");
        RegisterOutgoing(registry, SampRpcId.ExitVehicle, SampRpc.ParseSendExitVehicle, name: "SendExitVehicle");
        RegisterOutgoing(registry, SampRpcId.SetInteriorId, SampRpc.ParseSetInteriorId);
        RegisterOutgoing(registry, SampRpcId.MapMarker, SampRpc.ParseMapMarker);
        RegisterOutgoing(registry, SampRpcId.RequestClass, SampRpc.ParseSendRequestClass, name: "SendRequestClass");
        RegisterOutgoing(registry, SampRpcId.RequestSpawn, SampRpc.ParseSendRequestSpawn, name: "SendRequestSpawn");
        RegisterOutgoing(registry, SampRpcId.PickedUpPickup, SampRpc.ParsePickedUpPickup);
        RegisterOutgoing(registry, SampRpcId.MenuSelect, SampRpc.ParseMenuSelect);
        RegisterOutgoing(registry, SampRpcId.MenuQuit, SampRpc.ParseMenuQuit);
        RegisterOutgoing(registry, SampRpcId.VehicleDestroyed, SampRpc.ParseVehicleDestroyed);
        RegisterOutgoing(registry, SampRpcId.NpcJoin, SampRpc.ParseNpcJoin);
        RegisterOutgoing(registry, SampRpcId.CameraTargetUpdate, SampRpc.ParseCameraTargetUpdate);
        RegisterOutgoing(registry, SampRpcId.GiveActorDamage, SampRpc.ParseGiveActorDamage);
        RegisterOutgoing(registry, SampRpcId.UpdateScoresAndPings, SampRpc.ParseUpdateScoresAndPingsOutgoing, name: "UpdateScoresAndPingsOutgoing");
        RegisterOutgoing(registry, SampRpcId.SelectObject, SampRpc.ParseSelectObjectOutgoing, name: "SelectObjectOutgoing");
        RegisterOutgoing(registry, SampRpcId.ScriptCash, SampRpc.ParseScriptCash);
        RegisterOutgoing(registry, SampRpcId.SrvNetStats, SampRpc.ParseSrvNetStatsRequest, name: "SrvNetStatsRequest");
        RegisterOutgoing(registry, SampRpcId.WeaponPickupDestroy, SampRpc.ParseWeaponPickupDestroy);
    }
}
