using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public static partial class PacketParserCatalog
{
    private static void RegisterArizona220(PacketParserRegistry registry)
    {
        Register220Incoming(registry, EArizonaPacketId.SetLocalDriver, ArizonaPacket.ParseSetLocalDriver);
        Register220Incoming(registry, EArizonaPacketId.TurnLightUpdate, ArizonaPacket.ParseTurnLightUpdate);
        Register220Incoming(registry, EArizonaPacketId.SetSatiety, ArizonaPacket.ParseSetSatiety);
        Register220Incoming(registry, EArizonaPacketId.SetHudMode, ArizonaPacket.ParseSetHudMode);
        Register220Incoming(registry, EArizonaPacketId.SetRadarMode, ArizonaPacket.ParseSetRadarMode);
        Register220Incoming(registry, EArizonaPacketId.LoadJs, ArizonaPacket.ParseLoadJs);
        Register220Incoming(registry, EArizonaPacketId.SimpleCreate, ArizonaPacket.ParseSimpleCreate, "SimpleCreate");
        Register220Incoming(registry, EArizonaPacketId.CreateScaled, ArizonaPacket.ParseCreateScaled, "CreateScaled");
        Register220Incoming(registry, EArizonaPacketId.PlayMediaOnBillboard, ArizonaPacket.ParsePlayMediaOnBillboard);
        Register220Incoming(registry, EArizonaPacketId.ObjectCreate, ArizonaPacket.ParseObjectCreate, "ObjectCreate");
        Register220Incoming(registry, EArizonaPacketId.InsideObjectCreate, ArizonaPacket.ParseInsideObjectCreate, "InsideObjectCreate");
        Register220Incoming(registry, EArizonaPacketId.Close, ArizonaPacket.ParseClose, "Close");
        Register220Incoming(registry, EArizonaPacketId.Move, ArizonaPacket.ParseMove, "Move");
        Register220Incoming(registry, EArizonaPacketId.ChangeUrl, ArizonaPacket.ParseChangeUrl, "ChangeUrl");
        Register220Incoming(registry, EArizonaPacketId.InjectCode, ArizonaPacket.ParseInjectCode, "InjectCode");
        Register220Incoming(registry, EArizonaPacketId.SendMessage, ArizonaPacket.ParseSendMessage, "SendMessage");
        Register220Incoming(registry, EArizonaPacketId.ToggleScreen, ArizonaPacket.ParseToggleScreen, "ToggleScreen");
        Register220Incoming(registry, EArizonaPacketId.RequestClientViewport, ArizonaPacket.ParseRequestClientViewport, "RequestClientViewport");
        Register220Incoming(registry, EArizonaPacketId.ModuleReadRequest, ArizonaPacket.ParseModuleReadRequest, "ModuleReadRequest");
        Register220Incoming(registry, EArizonaPacketId.ToggleShow, ArizonaPacket.ParseToggleShow, "ToggleShow");
        Register220Incoming(registry, EArizonaPacketId.BrowserClick, ArizonaPacket.ParseBrowserClick, "BrowserClick");
        Register220Incoming(registry, EArizonaPacketId.GetBrowserControlState, ArizonaPacket.ParseGetBrowserControlState, "GetBrowserControlState");
        Register220Incoming(registry, EArizonaPacketId.SetBrowserControlState, ArizonaPacket.ParseSetBrowserControlState, "SetBrowserControlState");
        Register220Incoming(registry, EArizonaPacketId.Resize, ArizonaPacket.ParseResize, "Resize");
        Register220Incoming(registry, EArizonaPacketId.AddObject, ArizonaPacket.ParseAddObject, "AddObject");
        Register220Incoming(registry, EArizonaPacketId.RemoveObject, ArizonaPacket.ParseRemoveObject, "RemoveObject");
        Register220Incoming(registry, EArizonaPacketId.UiColorScale, ArizonaPacket.ParseUiColorScale);
        Register220Incoming(registry, EArizonaPacketId.SetChatGroup, ArizonaPacket.ParseSetChatGroup);
        Register220Incoming(registry, EArizonaPacketId.SetLocalInVehicle, ArizonaPacket.ParseSetLocalInVehicle);
        Register220Incoming(registry, EArizonaPacketId.SetNicknameMode, ArizonaPacket.ParseSetNicknameMode);
        Register220Incoming(registry, EArizonaPacketId.SwitchChatMode, ArizonaPacket.ParseSwitchChatMode);
        Register220Incoming(registry, EArizonaPacketId.SetVisibleDistance3DMarker, ArizonaPacket.ParseSetVisibleDistance3DMarker);
        Register220Incoming(registry, EArizonaPacketId.ShowPositionInDiscord, ArizonaPacket.ParseShowPositionInDiscord);
        Register220Incoming(registry, EArizonaPacketId.SetRadarVisibility, ArizonaPacket.ParseSetRadarVisibility);
        Register220Incoming(registry, EArizonaPacketId.SetCompassMode, ArizonaPacket.ParseSetCompassMode);
        Register220Incoming(registry, EArizonaPacketId.SetCompassCoords, ArizonaPacket.ParseSetCompassCoords);
        Register220Incoming(registry, EArizonaPacketId.AutoDrinkBeer, ArizonaPacket.ParseAutoDrinkBeer);
        Register220Incoming(registry, EArizonaPacketId.SetDayNightColors, ArizonaPacket.ParseSetDayNightColors);
        Register220Incoming(registry, EArizonaPacketId.ToggleCompass, ArizonaPacket.ParseToggleCompass);
        Register220Incoming(registry, EArizonaPacketId.SetAnimationProperty, ArizonaPacket.ParseSetAnimationProperty);
        Register220Incoming(registry, EArizonaPacketId.ToggleMapColors, ArizonaPacket.ParseToggleMapColors);
        Register220Incoming(registry, EArizonaPacketId.ChangeServer, ArizonaPacket.ParseChangeServer);
        Register220Incoming(registry, EArizonaPacketId.ShowLoadScreenVc, ArizonaPacket.ParseShowLoadScreenVc);
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<ArzSwitchChatState>>(EPacketId.ArizonaCef, (int)EArizonaPacketId.SwitchChatState, args => ParseIncoming220(args, EArizonaPacketId.SwitchChatState, EArizonaPacketId.SwitchChatState.ToString(), ArizonaPacket.ParseSwitchChatState), name: $"Arizona220:{EArizonaPacketId.SwitchChatState}", minimumPayloadBitLength: 33));
        Register220Incoming(registry, EArizonaPacketId.SetGreenZone, ArizonaPacket.ParseUiConfig);
        Register220Incoming(registry, EArizonaPacketId.SetSpectatorPatches, ArizonaPacket.ParseSetSpectatorPatches);
        Register220Incoming(registry, EArizonaPacketId.SetViceCityFlag, ArizonaPacket.ParseSetViceCityFlag);
        Register220Incoming(registry, EArizonaPacketId.SetTuningConfig, ArizonaPacket.ParseSetTuningConfig);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerNametagFlags, ArizonaPacket.ParseSetPlayerNametagFlags);
        Register220Incoming(registry, EArizonaPacketId.LoadSharedTexture, ArizonaPacket.ParseLoadSharedTexture);
        Register220Incoming(registry, EArizonaPacketId.ToggleSharedTxdFlag, ArizonaPacket.ParseToggleSharedTxdFlag);
        Register220Incoming(registry, EArizonaPacketId.SetMapIcon, ArizonaPacket.ParseSetMapIcon);
        Register220Incoming(registry, EArizonaPacketId.DeleteCustomMarker, ArizonaPacket.ParseDeleteCustomMarker);
        Register220Incoming(registry, EArizonaPacketId.ClearCustomMarkers, ArizonaPacket.ParseClearCustomMarkers);
        Register220Incoming(registry, EArizonaPacketId.TestDrive, ArizonaPacket.ParseTestDrive);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleLightsColor, ArizonaPacket.ParseSetVehicleLightsColor);
        Register220Incoming(registry, EArizonaPacketId.UiScalar, ArizonaPacket.ParseUiScalar);
        Register220Incoming(registry, EArizonaPacketId.SetDriveOnWater, ArizonaPacket.ParseSetDriveOnWater);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleFlight, ArizonaPacket.ParseSetVehicleFlight);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleColorSmoke, ArizonaPacket.ParseSetVehicleColorSmoke);
        Register220Incoming(registry, EArizonaPacketId.Create3DWaypoint, ArizonaPacket.ParseCreate3DWaypoint);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleNeonColor, ArizonaPacket.ParseSetVehicleNeonColor);
        Register220Incoming(registry, EArizonaPacketId.SetSkyboxImages, ArizonaPacket.ParseSetSkyboxImages);
        Register220Incoming(registry, EArizonaPacketId.SetHudStyle, ArizonaPacket.ParseSetHudStyle);
        Register220Incoming(registry, EArizonaPacketId.ToggleRenderTarget, ArizonaPacket.ParseToggleRenderTarget);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureFlag1, ArizonaPacket.ParseVehicleFeatureFlag1);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureFlag0, ArizonaPacket.ParseVehicleFeatureFlag0);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureFlag2, ArizonaPacket.ParseVehicleFeatureFlag2);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleNumberPlate, ArizonaPacket.ParseSetVehicleNumberPlate);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerAttachedObject, ArizonaPacket.ParseSetPlayerAttachedObject);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureReset, ArizonaPacket.ParseVehicleFeatureReset);
        Register220Incoming(registry, EArizonaPacketId.SetWeaponUpgrade, ArizonaPacket.ParseSetWeaponUpgrade);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerAnimGroups, ArizonaPacket.ParseSetPlayerAnimGroups);
        Register220Incoming(registry, EArizonaPacketId.LoadBinary, ArizonaPacket.ParseLoadBinary);
        Register220Incoming(registry, EArizonaPacketId.TogglePortal, ArizonaPacket.ParseTogglePortal);
        Register220Incoming(registry, EArizonaPacketId.CreatePortal, ArizonaPacket.ParseCreatePortal);
        Register220Incoming(registry, EArizonaPacketId.DestroyPortal, ArizonaPacket.ParseDestroyPortal);
        Register220Incoming(registry, EArizonaPacketId.SetSingleAnimGroup, ArizonaPacket.ParseSetSingleAnimGroup);
        Register220Incoming(registry, EArizonaPacketId.SetCurrentTask, ArizonaPacket.ParseSetCurrentTask);
        Register220Incoming(registry, EArizonaPacketId.ToggleDrawInterface, ArizonaPacket.ParseToggleDrawInterface);
        Register220Incoming(registry, EArizonaPacketId.SetInterior, ArizonaPacket.ParseSetInterior);
        Register220Incoming(registry, EArizonaPacketId.UiToggle, ArizonaPacket.ParseUiToggle);
        Register220Incoming(registry, EArizonaPacketId.SetWaterLevel, ArizonaPacket.ParseSetWaterLevel);
        Register220Incoming(registry, EArizonaPacketId.VehicleHeadlightsState, ArizonaPacket.ParseVehicleHeadlightsState);
        Register220Incoming(registry, EArizonaPacketId.SetVirtualWorld, ArizonaPacket.ParseSetVirtualWorld);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleDriftMode, ArizonaPacket.ParseSetVehicleDriftMode);
        Register220Incoming(registry, EArizonaPacketId.SetLines, ArizonaPacket.ParseSetLines);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleLights, ArizonaPacket.ParseSetVehicleLights);
        Register220Incoming(registry, EArizonaPacketId.UpdateWeaponSlots, ArizonaPacket.ParseUpdateWeaponSlots);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerSkin, ArizonaPacket.ParseSetPlayerSkin);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleStrobelights, ArizonaPacket.ParseSetVehicleStrobelights);
        Register220Incoming(registry, EArizonaPacketId.SetGpsRoute, ArizonaPacket.ParseSetGpsRoute);
        Register220Incoming(registry, EArizonaPacketId.SetFirstPersonCamera, ArizonaPacket.ParseSetFirstPersonCamera);
        Register220Incoming(registry, EArizonaPacketId.SetExtendAnimGroups, ArizonaPacket.ParseSetExtendAnimGroups);
        Register220Incoming(registry, EArizonaPacketId.ResetFirstPersonState, ArizonaPacket.ParseResetFirstPersonState);
        Register220Incoming(registry, EArizonaPacketId.ToggleHeadMove, ArizonaPacket.ParseToggleHeadMove);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleBrakeCalipers, ArizonaPacket.ParseSetVehicleBrakeCalipers);
        Register220Incoming(registry, EArizonaPacketId.BlipIcon, ArizonaPacket.ParseBlipIconRaw, "BlipIcon");
        Register220Incoming(registry, EArizonaPacketId.MarkerIconBatch, ArizonaPacket.ParseMarkerIconBatchRaw, "MarkerIconBatch");

        Register220Outgoing(registry, EArizonaPacketId.SendKey, ArizonaPacket.ParseSendKey);
        Register220Outgoing(registry, EArizonaPacketId.SendSwitchChatState, ArizonaPacket.ParseSendSwitchChatState);
        Register220Outgoing(registry, EArizonaPacketId.SendTurnLights, ArizonaPacket.ParseSendTurnLights);
        Register220Outgoing(registry, EArizonaPacketId.InjectCodeResponse, ArizonaPacket.ParseInjectCodeResponse, "InjectCodeResponse");
        Register220Outgoing(registry, EArizonaPacketId.Send, ArizonaPacket.ParseSendText);
        Register220Outgoing(registry, EArizonaPacketId.ClientViewport, ArizonaPacket.ParseStatePair, "ClientViewport");
        Register220Outgoing(registry, EArizonaPacketId.ModuleReadResponse, ArizonaPacket.ParseModuleReadResponse, "ModuleReadResponse");
        Register220Outgoing(registry, EArizonaPacketId.BrowserControlStateReply, ArizonaPacket.ParseBrowserControlStateReply, "BrowserControlStateReply");
        Register220Outgoing(registry, EArizonaPacketId.SendHash, ArizonaPacket.ParseSendHash);
        Register220Outgoing(registry, EArizonaPacketId.SendSwitchChatMode, ArizonaPacket.ParseSendSwitchChatMode);
        Register220Outgoing(registry, EArizonaPacketId.SendFloatValue, ArizonaPacket.ParseSendFloatValue);
        Register220Outgoing(registry, EArizonaPacketId.SendToggleActionState, ArizonaPacket.ParseSendToggleActionState);
        Register220Outgoing(registry, EArizonaPacketId.SendTargetPosition, ArizonaPacket.ParseSendTargetPosition);
        Register220Outgoing(registry, EArizonaPacketId.SendClientJoin, ArizonaPacket.ParseSendClientJoin);
        Register220Outgoing(registry, EArizonaPacketId.SendDroneHeading, ArizonaPacket.ParseSendDroneHeading);
        Register220Outgoing(registry, EArizonaPacketId.SendPortalToggle, ArizonaPacket.ParseSendPortalToggle);
        Register220Outgoing(registry, EArizonaPacketId.SendWeaponScroll, ArizonaPacket.ParseSendWeaponScroll);
        Register220Outgoing(registry, EArizonaPacketId.SendDamageResponseWeapon, ArizonaPacket.ParseSendDamageResponseWeapon);

    }

    private static void RegisterArizona221(PacketParserRegistry registry)
    {
        Register221Incoming(registry, EArizonaPacketIdEx.BotWorldPedAdd, ArizonaPacket.ParseBotStreamIn);
        Register221Incoming(registry, EArizonaPacketIdEx.BotWorldPedRemove, ArizonaPacket.ParseBotStreamOut);
        Register221Incoming(registry, EArizonaPacketIdEx.BotOnfootPedSync, ArizonaPacket.ParseBotOnfootSync);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedColor, ArizonaPacket.ParseSetBotColor);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedFightStyle, ArizonaPacket.ParseSetBotFightStyle);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedInvulnerable, ArizonaPacket.ParseSetBotInvulnerable);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedName, ArizonaPacket.ParseSetBotName);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedSkin, ArizonaPacket.ParseSetBotSkin);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedWeapon, ArizonaPacket.ParseSetBotWeapon);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedPos, ArizonaPacket.ParseSetBotPos);
        Register221Incoming(registry, EArizonaPacketIdEx.BotMovePedToPos, ArizonaPacket.ParseMoveBotToPos);
        Register221Incoming(registry, EArizonaPacketIdEx.BotShootPedAtPos, ArizonaPacket.ParseShootBotAtPos);
        Register221Incoming(registry, EArizonaPacketIdEx.BotApplyPedAnimation, ArizonaPacket.ParseApplyBotAnimation);
        Register221Incoming(registry, EArizonaPacketIdEx.BotClearPedAction, ArizonaPacket.ParseClearBotAction);
        Register221Incoming(registry, EArizonaPacketIdEx.BotShootPedAtPlayer, ArizonaPacket.ParseShootBotAtPlayer);
        Register221Incoming(registry, EArizonaPacketIdEx.BotAttackPlayer, ArizonaPacket.ParseBotAttackPlayer);
        Register221Incoming(registry, EArizonaPacketIdEx.BotEnterToVehicle, ArizonaPacket.ParseBotEnterVehicle);
        Register221Incoming(registry, EArizonaPacketIdEx.BotPassengerPedSync, ArizonaPacket.ParseBotPassengerSync);
        Register221Incoming(registry, EArizonaPacketIdEx.BotDrivePedSync, ArizonaPacket.ParseBotDriveSync);
        Register221Incoming(registry, EArizonaPacketIdEx.BotRemoveFromVehicle, ArizonaPacket.ParseBotExitVehicle);
        Register221Incoming(registry, EArizonaPacketIdEx.BotChatBubble, ArizonaPacket.ParseBotChatBubble);
        Register221Incoming(registry, EArizonaPacketIdEx.BotAttachObject, ArizonaPacket.ParseSetBotAttachedObject);
        Register221Incoming(registry, EArizonaPacketIdEx.BotDetachObject, ArizonaPacket.ParseRemoveBotAttachedObject);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetPedAngle, ArizonaPacket.ParseSetBotAngle);
        Register221Incoming(registry, EArizonaPacketIdEx.BotStopAllAction, ArizonaPacket.ParseStopBotAction);
        Register221Incoming(registry, EArizonaPacketIdEx.BotShootPedAtPed, ArizonaPacket.ParseShootBotAtBot);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetAnimationGroup, ArizonaPacket.ParseSetBotAnimationGroup);
        Register221Incoming(registry, EArizonaPacketIdEx.BotToggleCollision, ArizonaPacket.ParseTogglePedCollision);
        Register221Incoming(registry, EArizonaPacketIdEx.BotAttachSimpleObject, ArizonaPacket.ParseSetBotAttachedSimpleObject);
        Register221Incoming(registry, EArizonaPacketIdEx.BotDetachSimpleObject, ArizonaPacket.ParseRemoveBotAttachedSimpleObject);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetHealth, ArizonaPacket.ParseSetBotHealth);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetArmour, ArizonaPacket.ParseSetBotArmour);
        Register221Incoming(registry, EArizonaPacketIdEx.BotSetSettings, ArizonaPacket.ParseSetBotSettings);

        Register221Outgoing(registry, EArizonaPacketIdEx.BotSendOnfootSync, ArizonaPacket.ParseSendBotOnfootSync);
        Register221Outgoing(registry, EArizonaPacketIdEx.BotSendDamage, ArizonaPacket.ParseSendBotDamage);
    }


}

