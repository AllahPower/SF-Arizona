using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public static partial class PacketParserCatalog
{
    private static void RegisterArizona220(PacketParserRegistry registry)
    {
        // Outgoing (client -> server)
        Register220Outgoing(registry, EArizona.SendKey, ArizonaPacket.ParseSendKey);
        Register220Outgoing(registry, EArizona.SendSwitchChatState, ArizonaPacket.ParseSendSwitchChatState);
        Register220Outgoing(registry, EArizona.SendTurnLights, ArizonaPacket.ParseSendTurnLights);
        Register220Outgoing(registry, EArizona.InjectCodeResponse, ArizonaPacket.ParseInjectCodeResponse, "InjectCodeResponse");
        Register220Outgoing(registry, EArizona.Send, ArizonaPacket.ParseSendText);
        Register220Outgoing(registry, EArizona.ClientViewport, ArizonaPacket.ParseStatePair, "ClientViewport");
        Register220Outgoing(registry, EArizona.ModuleReadResponse, ArizonaPacket.ParseModuleReadResponse, "ModuleReadResponse");
        Register220Outgoing(registry, EArizona.BrowserControlStateReply, ArizonaPacket.ParseBrowserControlStateReply, "BrowserControlStateReply");
        Register220Outgoing(registry, EArizona.SendHash, ArizonaPacket.ParseSendHash);
        Register220Outgoing(registry, EArizona.SendSwitchChatMode, ArizonaPacket.ParseSendSwitchChatMode);
        Register220Outgoing(registry, EArizona.SendFloatValue, ArizonaPacket.ParseSendFloatValue);
        Register220Outgoing(registry, EArizona.SendToggleActionState, ArizonaPacket.ParseSendToggleActionState);
        Register220Outgoing(registry, EArizona.SendTargetPosition, ArizonaPacket.ParseSendTargetPosition);
        Register220Outgoing(registry, EArizona.SendClientJoin, ArizonaPacket.ParseSendClientJoin);
        Register220Outgoing(registry, EArizona.SendDroneHeading, ArizonaPacket.ParseSendDroneHeading);
        Register220Outgoing(registry, EArizona.SendPortalToggle, ArizonaPacket.ParseSendPortalToggle);
        Register220Outgoing(registry, EArizona.SendWeaponScroll, ArizonaPacket.ParseSendWeaponScroll);
        Register220Outgoing(registry, EArizona.SendDamageResponseWeapon, ArizonaPacket.ParseSendDamageResponseWeapon);

        // Incoming (server -> client)
        Register220Incoming(registry, EArizona.SetLocalDriver, ArizonaPacket.ParseSetLocalDriver);
        Register220Incoming(registry, EArizona.TurnLightUpdate, ArizonaPacket.ParseTurnLightUpdate);
        Register220Incoming(registry, EArizona.SetSatiety, ArizonaPacket.ParseSetSatiety);
        Register220Incoming(registry, EArizona.SetHudMode, ArizonaPacket.ParseSetHudMode);
        Register220Incoming(registry, EArizona.SetRadarMode, ArizonaPacket.ParseSetRadarMode);
        Register220Incoming(registry, EArizona.LoadJs, ArizonaPacket.ParseLoadJs);
        Register220Incoming(registry, EArizona.SimpleCreate, ArizonaPacket.ParseSimpleCreate, "SimpleCreate");
        Register220Incoming(registry, EArizona.CreateScaled, ArizonaPacket.ParseCreateScaled, "CreateScaled");
        Register220Incoming(registry, EArizona.PlayMediaOnBillboard, ArizonaPacket.ParsePlayMediaOnBillboard);
        Register220Incoming(registry, EArizona.ObjectCreate, ArizonaPacket.ParseObjectCreate, "ObjectCreate");
        Register220Incoming(registry, EArizona.InsideObjectCreate, ArizonaPacket.ParseInsideObjectCreate, "InsideObjectCreate");
        Register220Incoming(registry, EArizona.Close, ArizonaPacket.ParseClose, "Close");
        Register220Incoming(registry, EArizona.Move, ArizonaPacket.ParseMove, "Move");
        Register220Incoming(registry, EArizona.ChangeUrl, ArizonaPacket.ParseChangeUrl, "ChangeUrl");
        Register220Incoming(registry, EArizona.InjectCode, ArizonaPacket.ParseInjectCode, "InjectCode");
        Register220Incoming(registry, EArizona.SendMessage, ArizonaPacket.ParseSendMessage, "SendMessage");
        Register220Incoming(registry, EArizona.ToggleScreen, ArizonaPacket.ParseToggleScreen, "ToggleScreen");
        Register220Incoming(registry, EArizona.RequestClientViewport, ArizonaPacket.ParseRequestClientViewport, "RequestClientViewport");
        Register220Incoming(registry, EArizona.ModuleReadRequest, ArizonaPacket.ParseModuleReadRequest, "ModuleReadRequest");
        Register220Incoming(registry, EArizona.ToggleShow, ArizonaPacket.ParseToggleShow, "ToggleShow");
        Register220Incoming(registry, EArizona.BrowserClick, ArizonaPacket.ParseBrowserClick, "BrowserClick");
        Register220Incoming(registry, EArizona.GetBrowserControlState, ArizonaPacket.ParseGetBrowserControlState, "GetBrowserControlState");
        Register220Incoming(registry, EArizona.SetBrowserControlState, ArizonaPacket.ParseSetBrowserControlState, "SetBrowserControlState");
        Register220Incoming(registry, EArizona.Resize, ArizonaPacket.ParseResize, "Resize");
        Register220Incoming(registry, EArizona.AddObject, ArizonaPacket.ParseAddObject, "AddObject");
        Register220Incoming(registry, EArizona.RemoveObject, ArizonaPacket.ParseRemoveObject, "RemoveObject");
        Register220Incoming(registry, EArizona.UiColorScale, ArizonaPacket.ParseUiColorScale);
        Register220Incoming(registry, EArizona.SetChatGroup, ArizonaPacket.ParseSetChatGroup);
        Register220Incoming(registry, EArizona.HideDynamicRoom, ArizonaPacket.ParseHideDynamicRoom);
        Register220Incoming(registry, EArizona.SetLocalInVehicle, ArizonaPacket.ParseSetLocalInVehicle);
        Register220Incoming(registry, EArizona.SetNicknameMode, ArizonaPacket.ParseSetNicknameMode);
        Register220Incoming(registry, EArizona.SetChatFlag, ArizonaPacket.ParseSetChatFlag);
        Register220Incoming(registry, EArizona.SwitchChatMode, ArizonaPacket.ParseSwitchChatMode);
        Register220Incoming(registry, EArizona.SetVisibleDistance3DMarker, ArizonaPacket.ParseSetVisibleDistance3DMarker);
        Register220Incoming(registry, EArizona.ShowPositionInDiscord, ArizonaPacket.ParseShowPositionInDiscord);
        Register220Incoming(registry, EArizona.SetRadarVisibility, ArizonaPacket.ParseSetRadarVisibility);
        Register220Incoming(registry, EArizona.SetCompassMode, ArizonaPacket.ParseSetCompassMode);
        Register220Incoming(registry, EArizona.SetCompassCoords, ArizonaPacket.ParseSetCompassCoords);
        Register220Incoming(registry, EArizona.ShowStunIcon, ArizonaPacket.ParseShowStunIcon);
        Register220Incoming(registry, EArizona.HideStunIcon, ArizonaPacket.ParseHideStunIcon);
        Register220Incoming(registry, EArizona.AutoDrinkBeer, ArizonaPacket.ParseAutoDrinkBeer);
        Register220Incoming(registry, EArizona.SetDayNightColors, ArizonaPacket.ParseSetDayNightColors);
        Register220Incoming(registry, EArizona.ToggleCompass, ArizonaPacket.ParseToggleCompass);
        Register220Incoming(registry, EArizona.SetAnimationProperty, ArizonaPacket.ParseSetAnimationProperty);
        Register220Incoming(registry, EArizona.ToggleCgps, ArizonaPacket.ParseToggleCgps);
        Register220Incoming(registry, EArizona.ToggleMapColors, ArizonaPacket.ParseToggleMapColors);
        Register220Incoming(registry, EArizona.ChangeServer, ArizonaPacket.ParseChangeServer);
        Register220Incoming(registry, EArizona.ShowLoadScreenVc, ArizonaPacket.ParseShowLoadScreenVc);
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingSubPacket<ArzSetChatIconState>>(EPacketId.ArizonaCef, (int)EArizona.SetChatIconState, args => ParseIncoming220(args, EArizona.SetChatIconState, EArizona.SetChatIconState.ToString(), ArizonaPacket.ParseSetChatIconState), name: $"Arizona220:{EArizona.SetChatIconState}", minimumPayloadBitLength: 33));
        Register220Incoming(registry, EArizona.SetGreenZone, ArizonaPacket.ParseUiConfig);
        Register220Incoming(registry, EArizona.SetVehicleModelSpeedLimit, ArizonaPacket.ParseSetVehicleModelSpeedLimit);
        Register220Incoming(registry, EArizona.SetSpectatorPatches, ArizonaPacket.ParseSetSpectatorPatches);
        Register220Incoming(registry, EArizona.SetViceCityFlag, ArizonaPacket.ParseSetViceCityFlag);
        Register220Incoming(registry, EArizona.SetTuningConfig, ArizonaPacket.ParseSetTuningConfig);
        Register220Incoming(registry, EArizona.SetPlayerNametagFlags, ArizonaPacket.ParseSetPlayerNametagFlags);
        Register220Incoming(registry, EArizona.LoadSharedTexture, ArizonaPacket.ParseLoadSharedTexture);
        Register220Incoming(registry, EArizona.ToggleSharedTxdFlag, ArizonaPacket.ParseToggleSharedTxdFlag);
        Register220Incoming(registry, EArizona.SetMapIcon, ArizonaPacket.ParseSetMapIcon);
        Register220Incoming(registry, EArizona.DeleteCustomMarker, ArizonaPacket.ParseDeleteCustomMarker);
        Register220Incoming(registry, EArizona.ClearCustomMarkers, ArizonaPacket.ParseClearCustomMarkers);
        Register220Incoming(registry, EArizona.TestDrive, ArizonaPacket.ParseTestDrive);
        Register220Incoming(registry, EArizona.SetVehicleLightsColor, ArizonaPacket.ParseSetVehicleLightsColor);
        Register220Incoming(registry, EArizona.UiScalar, ArizonaPacket.ParseUiScalar);
        Register220Incoming(registry, EArizona.SetDriveOnWater, ArizonaPacket.ParseSetDriveOnWater);
        Register220Incoming(registry, EArizona.SetVehicleFlight, ArizonaPacket.ParseSetVehicleFlight);
        Register220Incoming(registry, EArizona.SetVehicleColorSmoke, ArizonaPacket.ParseSetVehicleColorSmoke);
        Register220Incoming(registry, EArizona.Create3DWaypoint, ArizonaPacket.ParseCreate3DWaypoint);
        Register220Incoming(registry, EArizona.SetVehicleNeonColor, ArizonaPacket.ParseSetVehicleNeonColor);
        Register220Incoming(registry, EArizona.SetSkyboxImages, ArizonaPacket.ParseSetSkyboxImages);
        Register220Incoming(registry, EArizona.SetHudStyle, ArizonaPacket.ParseSetHudStyle);
        Register220Incoming(registry, EArizona.ToggleRenderTarget, ArizonaPacket.ParseToggleRenderTarget);
        Register220Incoming(registry, EArizona.VehicleFeatureFlag1, ArizonaPacket.ParseVehicleFeatureFlag1);
        Register220Incoming(registry, EArizona.VehicleFeatureFlag0, ArizonaPacket.ParseVehicleFeatureFlag0);
        Register220Incoming(registry, EArizona.VehicleFeatureFlag2, ArizonaPacket.ParseVehicleFeatureFlag2);
        Register220Incoming(registry, EArizona.SetVehicleNumberPlate, ArizonaPacket.ParseSetVehicleNumberPlate);
        Register220Incoming(registry, EArizona.SetPlayerAttachedObject, ArizonaPacket.ParseSetPlayerAttachedObject);
        Register220Incoming(registry, EArizona.VehicleFeatureReset, ArizonaPacket.ParseVehicleFeatureReset);
        Register220Incoming(registry, EArizona.SetWeaponUpgrade, ArizonaPacket.ParseSetWeaponUpgrade);
        Register220Incoming(registry, EArizona.SetPlayerAnimGroups, ArizonaPacket.ParseSetPlayerAnimGroups);
        Register220Incoming(registry, EArizona.LoadBinary, ArizonaPacket.ParseLoadBinary);
        Register220Incoming(registry, EArizona.TogglePortal, ArizonaPacket.ParseTogglePortal);
        Register220Incoming(registry, EArizona.CreatePortal, ArizonaPacket.ParseCreatePortal);
        Register220Incoming(registry, EArizona.DestroyPortal, ArizonaPacket.ParseDestroyPortal);
        Register220Incoming(registry, EArizona.SetSingleAnimGroup, ArizonaPacket.ParseSetSingleAnimGroup);
        Register220Incoming(registry, EArizona.SetCurrentTask, ArizonaPacket.ParseSetCurrentTask);
        Register220Incoming(registry, EArizona.ToggleDrawInterface, ArizonaPacket.ParseToggleDrawInterface);
        Register220Incoming(registry, EArizona.SetInterior, ArizonaPacket.ParseSetInterior);
        Register220Incoming(registry, EArizona.UiToggle, ArizonaPacket.ParseUiToggle);
        Register220Incoming(registry, EArizona.SetWaterLevel, ArizonaPacket.ParseSetWaterLevel);
        Register220Incoming(registry, EArizona.VehicleHeadlightsState, ArizonaPacket.ParseVehicleHeadlightsState);
        Register220Incoming(registry, EArizona.SetVirtualWorld, ArizonaPacket.ParseSetVirtualWorld);
        Register220Incoming(registry, EArizona.SetVehicleDriftMode, ArizonaPacket.ParseSetVehicleDriftMode);
        Register220Incoming(registry, EArizona.SetLines, ArizonaPacket.ParseSetLines);
        Register220Incoming(registry, EArizona.SetVehicleLights, ArizonaPacket.ParseSetVehicleLights);
        Register220Incoming(registry, EArizona.UpdateWeaponSlots, ArizonaPacket.ParseUpdateWeaponSlots);
        Register220Incoming(registry, EArizona.Unknown200, ArizonaPacket.ParseUnknown200);
        Register220Incoming(registry, EArizona.SetVehicleStrobelights, ArizonaPacket.ParseSetVehicleStrobelights);
        Register220Incoming(registry, EArizona.ChatMessageRelay, ArizonaPacket.ParseChatMessageRelay);
        Register220Incoming(registry, EArizona.SetGpsRoute, ArizonaPacket.ParseSetGpsRoute);
        Register220Incoming(registry, EArizona.SetFirstPersonCamera, ArizonaPacket.ParseSetFirstPersonCamera);
        Register220Incoming(registry, EArizona.SetExtendAnimGroups, ArizonaPacket.ParseSetExtendAnimGroups);
        Register220Incoming(registry, EArizona.ResetFirstPersonState, ArizonaPacket.ParseResetFirstPersonState);
        Register220Incoming(registry, EArizona.ToggleHeadMove, ArizonaPacket.ParseToggleHeadMove);
        Register220Incoming(registry, EArizona.SetVehicleBrakeCalipers, ArizonaPacket.ParseSetVehicleBrakeCalipers);
        Register220Incoming(registry, EArizona.BlipIcon, ArizonaPacket.ParseBlipIconRaw, "BlipIcon");
        Register220Incoming(registry, EArizona.MarkerIconBatch, ArizonaPacket.ParseMarkerIconBatchRaw, "MarkerIconBatch");
    }

    private static void RegisterArizona221(PacketParserRegistry registry)
    {
        // Outgoing (client -> server)
        Register221Outgoing(registry, EArizonaEx.BotSendOnfootSync, ArizonaPacket.ParseSendBotOnfootSync);
        Register221Outgoing(registry, EArizonaEx.BotSendDamage, ArizonaPacket.ParseSendBotDamage);

        // Incoming (server -> client)
        Register221Incoming(registry, EArizonaEx.BotWorldPedAdd, ArizonaPacket.ParseBotStreamIn);
        Register221Incoming(registry, EArizonaEx.BotWorldPedRemove, ArizonaPacket.ParseBotStreamOut);
        Register221Incoming(registry, EArizonaEx.BotOnfootPedSync, ArizonaPacket.ParseBotOnfootSync);
        Register221Incoming(registry, EArizonaEx.BotSetPedColor, ArizonaPacket.ParseSetBotColor);
        Register221Incoming(registry, EArizonaEx.BotSetPedFightStyle, ArizonaPacket.ParseSetBotFightStyle);
        Register221Incoming(registry, EArizonaEx.BotSetPedInvulnerable, ArizonaPacket.ParseSetBotInvulnerable);
        Register221Incoming(registry, EArizonaEx.BotSetPedName, ArizonaPacket.ParseSetBotName);
        Register221Incoming(registry, EArizonaEx.BotSetPedSkin, ArizonaPacket.ParseSetBotSkin);
        Register221Incoming(registry, EArizonaEx.BotSetPedWeapon, ArizonaPacket.ParseSetBotWeapon);
        Register221Incoming(registry, EArizonaEx.BotSetPedPos, ArizonaPacket.ParseSetBotPos);
        Register221Incoming(registry, EArizonaEx.BotMovePedToPos, ArizonaPacket.ParseMoveBotToPos);
        Register221Incoming(registry, EArizonaEx.BotShootPedAtPos, ArizonaPacket.ParseShootBotAtPos);
        Register221Incoming(registry, EArizonaEx.BotApplyPedAnimation, ArizonaPacket.ParseApplyBotAnimation);
        Register221Incoming(registry, EArizonaEx.BotClearPedAction, ArizonaPacket.ParseClearBotAction);
        Register221Incoming(registry, EArizonaEx.BotShootPedAtPlayer, ArizonaPacket.ParseShootBotAtPlayer);
        Register221Incoming(registry, EArizonaEx.BotAttackPlayer, ArizonaPacket.ParseBotAttackPlayer);
        Register221Incoming(registry, EArizonaEx.BotEnterToVehicle, ArizonaPacket.ParseBotEnterVehicle);
        Register221Incoming(registry, EArizonaEx.BotPassengerPedSync, ArizonaPacket.ParseBotPassengerSync);
        Register221Incoming(registry, EArizonaEx.BotDrivePedSync, ArizonaPacket.ParseBotDriveSync);
        Register221Incoming(registry, EArizonaEx.BotRemoveFromVehicle, ArizonaPacket.ParseBotExitVehicle);
        Register221Incoming(registry, EArizonaEx.BotChatBubble, ArizonaPacket.ParseBotChatBubble);
        Register221Incoming(registry, EArizonaEx.BotAttachObject, ArizonaPacket.ParseSetBotAttachedObject);
        Register221Incoming(registry, EArizonaEx.BotDetachObject, ArizonaPacket.ParseRemoveBotAttachedObject);
        Register221Incoming(registry, EArizonaEx.BotSetPedAngle, ArizonaPacket.ParseSetBotAngle);
        Register221Incoming(registry, EArizonaEx.BotStopAllAction, ArizonaPacket.ParseStopBotAction);
        Register221Incoming(registry, EArizonaEx.BotShootPedAtPed, ArizonaPacket.ParseShootBotAtBot);
        Register221Incoming(registry, EArizonaEx.BotSetAnimationGroup, ArizonaPacket.ParseSetBotAnimationGroup);
        Register221Incoming(registry, EArizonaEx.BotAttackPed, ArizonaPacket.ParseBotAttackPed);
        Register221Incoming(registry, EArizonaEx.BotToggleCollision, ArizonaPacket.ParseTogglePedCollision);
        Register221Incoming(registry, EArizonaEx.BotAttachSimpleObject, ArizonaPacket.ParseSetBotAttachedSimpleObject);
        Register221Incoming(registry, EArizonaEx.BotDetachSimpleObject, ArizonaPacket.ParseRemoveBotAttachedSimpleObject);
        Register221Incoming(registry, EArizonaEx.BotSetHealth, ArizonaPacket.ParseSetBotHealth);
        Register221Incoming(registry, EArizonaEx.BotSetArmour, ArizonaPacket.ParseSetBotArmour);
        Register221Incoming(registry, EArizonaEx.BotSetOnfootSyncRate, ArizonaPacket.ParseSetBotOnfootSyncRate);
    }


}

