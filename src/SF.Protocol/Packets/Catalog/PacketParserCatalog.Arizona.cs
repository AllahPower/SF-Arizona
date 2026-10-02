namespace SFSharp.Protocol.Packets.Catalog;

public static partial class PacketParserCatalog
{
    private static void RegisterArizona220(PacketParserRegistry registry)
    {
        #region outgoing (client -> server)
        Register220Outgoing(registry, ArizonaPacket220Id.SendKey, ArizonaPacket.ParseSendKey, "SendKey");
        Register220Outgoing(registry, ArizonaPacket220Id.SendSwitchChatState, ArizonaPacket.ParseSendSwitchChatState);
        Register220Outgoing(registry, ArizonaPacket220Id.SendTurnLights, ArizonaPacket.ParseSendTurnLights, "SendTurnLights");
        Register220Outgoing(registry, ArizonaPacket220Id.InjectCodeResponse, ArizonaPacket.ParseInjectCodeResponse, "InjectCodeResponse");
        Register220Outgoing(registry, ArizonaPacket220Id.Send, ArizonaPacket.ParseSendText, "Send");
        Register220Outgoing(registry, ArizonaPacket220Id.ClientViewport, ArizonaPacket.ParseStatePair, "ClientViewport");
        Register220Outgoing(registry, ArizonaPacket220Id.ModuleReadResponse, ArizonaPacket.ParseModuleReadResponse, "ModuleReadResponse");
        Register220Outgoing(registry, ArizonaPacket220Id.BrowserControlStateReply, ArizonaPacket.ParseBrowserControlStateReply, "BrowserControlStateReply");
        Register220Outgoing(registry, ArizonaPacket220Id.SendHWID, ArizonaPacket.ParseSendHWID);
        Register220Outgoing(registry, ArizonaPacket220Id.SendVehicleSpeedLimiterState, ArizonaPacket.ParseSendVehicleSpeedLimiterState);
        Register220Outgoing(registry, ArizonaPacket220Id.SendSwitchChatMode, ArizonaPacket.ParseSendSwitchChatMode);
        Register220Outgoing(registry, ArizonaPacket220Id.SendSrcursorPosition, ArizonaPacket.ParseSendSrcursorPosition, "SendSrcursorPosition");
        Register220Outgoing(registry, ArizonaPacket220Id.SendInCarNanPosition, ArizonaPacket.ParseInCarNanCheckReport, "SendInCarNanPosition");
        Register220Outgoing(registry, ArizonaPacket220Id.SendInCarNanQuaternion, ArizonaPacket.ParseInCarNanCheckReport, "SendInCarNanQuaternion");
        Register220Outgoing(registry, ArizonaPacket220Id.SendInCarNanTrainSpeed, ArizonaPacket.ParseInCarNanCheckReport, "SendInCarNanTrainSpeed");
        Register220Outgoing(registry, ArizonaPacket220Id.SendKeyboardLayoutCapsState, ArizonaPacket.ParseSendKeyboardLayoutCapsState);
        Register220Outgoing(registry, ArizonaPacket220Id.SendFloatValue, ArizonaPacket.ParseSendFloatValue);
        Register220Outgoing(registry, ArizonaPacket220Id.SendToggleActionState, ArizonaPacket.ParseSendToggleActionState);
        Register220Outgoing(registry, ArizonaPacket220Id.SendTargetPosition, ArizonaPacket.ParseSendTargetPosition);
        Register220Outgoing(registry, ArizonaPacket220Id.SendSimpleTuningProgress, ArizonaPacket.ParseSendSimpleTuningProgress);
        Register220Outgoing(registry, ArizonaPacket220Id.SendCommandLine, ArizonaPacket.ParseSendCommandLine);
        Register220Outgoing(registry, ArizonaPacket220Id.SendDroneHeading, ArizonaPacket.ParseSendDroneHeading);
        Register220Outgoing(registry, ArizonaPacket220Id.SendNavigationArrowSelection, ArizonaPacket.ParseSendNavigationArrowSelection);
        Register220Outgoing(registry, ArizonaPacket220Id.SendPortalToggle, ArizonaPacket.ParseSendPortalToggle);
        Register220Outgoing(registry, ArizonaPacket220Id.SendPortalPlacementPreview, ArizonaPacket.ParseSendPortalPlacementPreview);
        Register220Outgoing(registry, ArizonaPacket220Id.SendWeaponScroll, ArizonaPacket.ParseSendWeaponScroll);
        Register220Outgoing(registry, ArizonaPacket220Id.SendDamageResponseWeapon, ArizonaPacket.ParseSendDamageResponseWeapon, "SendDamageResponseWeapon");
        Register220Outgoing(registry, ArizonaPacket220Id.ChatMessageRelay, ArizonaPacket.ParseLinkedChatSend, "LinkedChatSend");

        #endregion

        #region incoming (server -> client)
        Register220Incoming(registry, ArizonaPacket220Id.SetLocalDriver, ArizonaPacket.ParseSetLocalDriver, "SetLocalDriver");
        Register220Incoming(registry, ArizonaPacket220Id.TurnLightUpdate, ArizonaPacket.ParseTurnLightUpdate, "TurnLightUpdate");
        Register220Incoming(registry, ArizonaPacket220Id.SetSatiety, ArizonaPacket.ParseSetSatiety);
        Register220Incoming(registry, ArizonaPacket220Id.SetHudMode, ArizonaPacket.ParseSetHudMode);
        Register220Incoming(registry, ArizonaPacket220Id.SetRadarMode, ArizonaPacket.ParseSetRadarMode);
        Register220Incoming(registry, ArizonaPacket220Id.PlayMediaOnBillboard, ArizonaPacket.ParsePlayMediaOnBillboard);
        Register220Incoming(registry, ArizonaPacket220Id.Close, ArizonaPacket.ParseClose, "Close");
        Register220Incoming(registry, ArizonaPacket220Id.Move, ArizonaPacket.ParseMove, "Move");
        Register220Incoming(registry, ArizonaPacket220Id.ChangeUrl, ArizonaPacket.ParseChangeUrl, "ChangeUrl");
        Register220Incoming(registry, ArizonaPacket220Id.InjectCode, ArizonaPacket.ParseInjectCode, "InjectCode");
        Register220Incoming(registry, ArizonaPacket220Id.SendMessage, ArizonaPacket.ParseSendMessage, "SendMessage");
        Register220Incoming(registry, ArizonaPacket220Id.ToggleScreen, ArizonaPacket.ParseToggleScreen, "ToggleScreen");
        Register220Incoming(registry, ArizonaPacket220Id.RequestClientViewport, ArizonaPacket.ParseRequestClientViewport, "RequestClientViewport");
        Register220Incoming(registry, ArizonaPacket220Id.ToggleShow, ArizonaPacket.ParseToggleShow, "ToggleShow");
        Register220Incoming(registry, ArizonaPacket220Id.BrowserClick, ArizonaPacket.ParseBrowserClick, "BrowserClick");
        Register220Incoming(registry, ArizonaPacket220Id.GetBrowserControlState, ArizonaPacket.ParseGetBrowserControlState, "GetBrowserControlState");
        Register220Incoming(registry, ArizonaPacket220Id.SetBrowserControlState, ArizonaPacket.ParseSetBrowserControlState, "SetBrowserControlState");
        Register220Incoming(registry, ArizonaPacket220Id.Resize, ArizonaPacket.ParseResize, "Resize");
        Register220Incoming(registry, ArizonaPacket220Id.AddObject, ArizonaPacket.ParseAddObject, "AddObject");
        Register220Incoming(registry, ArizonaPacket220Id.RemoveObject, ArizonaPacket.ParseRemoveObject, "RemoveObject");
        Register220Incoming(registry, ArizonaPacket220Id.SetBulletTracersGroupPreset, ArizonaPacket.ParseSetBulletTracersGroupPreset);
        Register220Incoming(registry, ArizonaPacket220Id.SetBulletTracersIndexedPreset, ArizonaPacket.ParseSetBulletTracersIndexedPreset);
        Register220Incoming(registry, ArizonaPacket220Id.SetChatGroup, ArizonaPacket.ParseSetChatGroup);
        Register220Incoming(registry, ArizonaPacket220Id.HideDynamicRoom, ArizonaPacket.ParseHideDynamicRoom);
        Register220Incoming(registry, ArizonaPacket220Id.SetLocalInVehicle, ArizonaPacket.ParseSetLocalInVehicle);
        Register220Incoming(registry, ArizonaPacket220Id.SetNicknameMode, ArizonaPacket.ParseSetNicknameMode);
        Register220Incoming(registry, ArizonaPacket220Id.SetChatFlag, ArizonaPacket.ParseSetChatFlag);
        Register220Incoming(registry, ArizonaPacket220Id.SwitchChatMode, ArizonaPacket.ParseSwitchChatMode);
        Register220Incoming(registry, ArizonaPacket220Id.SetAntiAfkEnabled, ArizonaPacket.ParseSetAntiAfkEnabled);
        Register220Incoming(registry, ArizonaPacket220Id.SrcursorSyncMode, ArizonaPacket.ParseSrcursorSyncMode, "SrcursorSyncMode");
        Register220Incoming(registry, ArizonaPacket220Id.TranslateObservedTextDrawPosition, ArizonaPacket.ParseTranslateObservedTextDrawPosition, "TranslateObservedTextDrawPosition");
        Register220Incoming(registry, ArizonaPacket220Id.Waypoint3DSetPosition, ArizonaPacket.ParseWaypoint3DSetPosition);
        Register220Incoming(registry, ArizonaPacket220Id.ShowPositionInDiscord, ArizonaPacket.ParseShowPositionInDiscord);
        Register220Incoming(registry, ArizonaPacket220Id.ChatCommandHelperEnabled, ArizonaPacket.ParseChatCommandHelperEnabled);
        Register220Incoming(registry, ArizonaPacket220Id.SetRadarFixEnabled, ArizonaPacket.ParseSetRadarFixEnabled);
        Register220Incoming(registry, ArizonaPacket220Id.DiscordSetStateText, ArizonaPacket.ParseDiscordSetStateText);
        Register220Incoming(registry, ArizonaPacket220Id.DiscordClearStateText, ArizonaPacket.ParseDiscordClearStateText);
        Register220Incoming(registry, ArizonaPacket220Id.SetRadarVisibility, ArizonaPacket.ParseSetRadarVisibility);
        Register220Incoming(registry, ArizonaPacket220Id.SetCompassMode, ArizonaPacket.ParseSetCompassMode);
        Register220Incoming(registry, ArizonaPacket220Id.SetCompassCoords, ArizonaPacket.ParseSetCompassCoords);
        Register220Incoming(registry, ArizonaPacket220Id.ShowStunIcon, ArizonaPacket.ParseShowStunIcon);
        Register220Incoming(registry, ArizonaPacket220Id.HideStunIcon, ArizonaPacket.ParseHideStunIcon);
        Register220Incoming(registry, ArizonaPacket220Id.AutoDrinkBeer, ArizonaPacket.ParseAutoDrinkBeer);
        Register220Incoming(registry, ArizonaPacket220Id.SetDayNightColors, ArizonaPacket.ParseSetDayNightColors);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleCompass, ArizonaPacket.ParseToggleCompass);
        Register220Incoming(registry, ArizonaPacket220Id.SetAnimationProperty, ArizonaPacket.ParseSetAnimationProperty);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleCgps, ArizonaPacket.ParseToggleCgps);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleMapColors, ArizonaPacket.ParseToggleMapColors);
        Register220Incoming(registry, ArizonaPacket220Id.SetRenderRoutineEnabled, ArizonaPacket.ParseSetRenderRoutineEnabled);
        Register220Incoming(registry, ArizonaPacket220Id.ChangeServer, ArizonaPacket.ParseChangeServer);
        Register220Incoming(registry, ArizonaPacket220Id.ShowLoadScreenVc, ArizonaPacket.ParseShowLoadScreenVc);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleFlightForwardAssist, ArizonaPacket.ParseSetVehicleFlightForwardAssist);
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingSubPacket<ArzSetChatIconState>>(RakNetPacketId.ArizonaCef, (int)ArizonaPacket220Id.SetChatIconState, args => ArizonaPacketTransportParsing.ParseIncoming220(args, ArizonaPacket220Id.SetChatIconState, ArizonaPacket220Id.SetChatIconState.ToString(), ArizonaPacket.ParseSetChatIconState), name: $"Arizona220:{ArizonaPacket220Id.SetChatIconState}", minimumPayloadBitLength: 33));
        Register220Incoming(registry, ArizonaPacket220Id.SetGreenZone, ArizonaPacket.ParseUiConfig, "SetGreenZone");
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleModelSpeedLimit, ArizonaPacket.ParseSetVehicleModelSpeedLimit);
        Register220Incoming(registry, ArizonaPacket220Id.SetSpectatorPatches, ArizonaPacket.ParseSetSpectatorPatches);
        Register220Incoming(registry, ArizonaPacket220Id.SetActionStateToggleEnabled, ArizonaPacket.ParseSetActionStateToggleEnabled);
        Register220Incoming(registry, ArizonaPacket220Id.SetViceCityFlag, ArizonaPacket.ParseSetViceCityFlag);
        Register220Incoming(registry, ArizonaPacket220Id.SetTuningConfig, ArizonaPacket.ParseSetTuningConfig);
        Register220Incoming(registry, ArizonaPacket220Id.SetPlayerNametagFlags, ArizonaPacket.ParseSetPlayerNametagFlags);
        Register220Incoming(registry, ArizonaPacket220Id.LoadSharedTexture, ArizonaPacket.ParseLoadSharedTexture);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleSharedTxdFlag, ArizonaPacket.ParseToggleSharedTxdFlag);
        Register220Incoming(registry, ArizonaPacket220Id.StreamFixMode, ArizonaPacket.ParseStreamFixMode);
        Register220Incoming(registry, ArizonaPacket220Id.SetMapIcon, ArizonaPacket.ParseSetMapIcon);
        Register220Incoming(registry, ArizonaPacket220Id.DeleteCustomMarker, ArizonaPacket.ParseDeleteCustomMarker);
        Register220Incoming(registry, ArizonaPacket220Id.ClearCustomMarkers, ArizonaPacket.ParseClearCustomMarkers);
        Register220Incoming(registry, ArizonaPacket220Id.TestDrive, ArizonaPacket.ParseTestDrive);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleLightsColor, ArizonaPacket.ParseSetVehicleLightsColor);
        Register220Incoming(registry, ArizonaPacket220Id.UiScalar, ArizonaPacket.ParseUiScalar);
        Register220Incoming(registry, ArizonaPacket220Id.SetDriveOnWater, ArizonaPacket.ParseSetDriveOnWater);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleFlight, ArizonaPacket.ParseSetVehicleFlight);
        Register220Incoming(registry, ArizonaPacket220Id.AttachVehicleToVehicleData, ArizonaPacket.ParseAttachVehicleToVehicleData);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleColorSmoke, ArizonaPacket.ParseSetVehicleColorSmoke);
        Register220Incoming(registry, ArizonaPacket220Id.Create3DWaypoint, ArizonaPacket.ParseCreate3DWaypoint);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleNeonColor, ArizonaPacket.ParseSetVehicleNeonColor);
        Register220Incoming(registry, ArizonaPacket220Id.SetSkyboxImages, ArizonaPacket.ParseSetSkyboxImages);
        Register220Incoming(registry, ArizonaPacket220Id.SetHudStyle, ArizonaPacket.ParseSetHudStyle);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleRenderTarget, ArizonaPacket.ParseToggleRenderTarget);
        Register220Incoming(registry, ArizonaPacket220Id.VehicleFeatureFlag1, ArizonaPacket.ParseVehicleFeatureFlag1);
        Register220Incoming(registry, ArizonaPacket220Id.VehicleFeatureFlag0, ArizonaPacket.ParseVehicleFeatureFlag0);
        Register220Incoming(registry, ArizonaPacket220Id.VehicleFeatureFlag2, ArizonaPacket.ParseVehicleFeatureFlag2);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleNumberPlate, ArizonaPacket.ParseSetVehicleNumberPlate);
        Register220Incoming(registry, ArizonaPacket220Id.SetPlayerAttachedObject, ArizonaPacket.ParseSetPlayerAttachedObject);
        Register220Incoming(registry, ArizonaPacket220Id.VehicleFeatureReset, ArizonaPacket.ParseVehicleFeatureReset);
        Register220Incoming(registry, ArizonaPacket220Id.SetWeaponUpgrade, ArizonaPacket.ParseSetWeaponUpgrade);
        Register220Incoming(registry, ArizonaPacket220Id.SetPlayerAnimGroups, ArizonaPacket.ParseSetPlayerAnimGroups);
        Register220Incoming(registry, ArizonaPacket220Id.LoadBinary, ArizonaPacket.ParseLoadBinary);
        Register220Incoming(registry, ArizonaPacket220Id.SetSelectorHookEnabled, ArizonaPacket.ParseSetSelectorHookEnabled);
        Register220Incoming(registry, ArizonaPacket220Id.SetSelectorSlotBlocked, ArizonaPacket.ParseSetSelectorSlotBlocked);
        Register220Incoming(registry, ArizonaPacket220Id.TogglePortal, ArizonaPacket.ParseTogglePortal);
        Register220Incoming(registry, ArizonaPacket220Id.CreatePortal, ArizonaPacket.ParseCreatePortal);
        Register220Incoming(registry, ArizonaPacket220Id.DestroyPortal, ArizonaPacket.ParseDestroyPortal);
        Register220Incoming(registry, ArizonaPacket220Id.SetSingleAnimGroup, ArizonaPacket.ParseSetSingleAnimGroup);
        Register220Incoming(registry, ArizonaPacket220Id.SetCurrentTask, ArizonaPacket.ParseSetCurrentTask);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleDrawInterface, ArizonaPacket.ParseToggleDrawInterface);
        Register220Incoming(registry, ArizonaPacket220Id.SetInterior, ArizonaPacket.ParseSetInterior);
        Register220Incoming(registry, ArizonaPacket220Id.UiToggle, ArizonaPacket.ParseUiToggle);
        Register220Incoming(registry, ArizonaPacket220Id.SetWaterLevel, ArizonaPacket.ParseSetWaterLevel);
        Register220Incoming(registry, ArizonaPacket220Id.WallHackToggle, ArizonaPacket.ParseWallHackToggle);
        Register220Incoming(registry, ArizonaPacket220Id.VehicleHeadlightsState, ArizonaPacket.ParseVehicleHeadlightsState);
        Register220Incoming(registry, ArizonaPacket220Id.SetVirtualWorld, ArizonaPacket.ParseSetVirtualWorld);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleDriftMode, ArizonaPacket.ParseSetVehicleDriftMode);
        Register220Incoming(registry, ArizonaPacket220Id.RadarFixPlayerStyle, ArizonaPacket.ParseRadarFixPlayerStyle);
        Register220Incoming(registry, ArizonaPacket220Id.SimpleAttachmentsSetMaterial, ArizonaPacket.ParseSimpleAttachmentsSetMaterial);
        Register220Incoming(registry, ArizonaPacket220Id.SetLines, ArizonaPacket.ParseSetLines);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleLights, ArizonaPacket.ParseSetVehicleLights);
        Register220Incoming(registry, ArizonaPacket220Id.UpdateWeaponSlots, ArizonaPacket.ParseUpdateWeaponSlots);
        Register220Incoming(registry, ArizonaPacket220Id.NavigationArrowTargets, ArizonaPacket.ParseNavigationArrowTargets);
        Register220Incoming(registry, ArizonaPacket220Id.UpdateQueuePosition, ArizonaPacket.ParseUpdateQueuePosition);
        Register220Incoming(registry, ArizonaPacket220Id.Unknown200, ArizonaPacket.ParseUnknown200);
        Register220Incoming(registry, ArizonaPacket220Id.GoogleAnalyticsMessage, ArizonaPacket.ParseGoogleAnalyticsMessage);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleStrobelights, ArizonaPacket.ParseSetVehicleStrobelights);
        Register220Incoming(registry, ArizonaPacket220Id.ChatMessageRelay, ArizonaPacket.ParseChatMessageRelay);
        Register220Incoming(registry, ArizonaPacket220Id.AttachVehicleToVehicleToggle, ArizonaPacket.ParseAttachVehicleToVehicleToggle);
        Register220Incoming(registry, ArizonaPacket220Id.SetGpsRoute, ArizonaPacket.ParseSetGpsRoute);
        Register220Incoming(registry, ArizonaPacket220Id.VehicleDamageDoorPanelRules, ArizonaPacket.ParseVehicleDamageDoorPanelRules);
        Register220Incoming(registry, ArizonaPacket220Id.SetFirstPersonCamera, ArizonaPacket.ParseSetFirstPersonCamera);
        Register220Incoming(registry, ArizonaPacket220Id.SetExtendAnimGroups, ArizonaPacket.ParseSetExtendAnimGroups);
        Register220Incoming(registry, ArizonaPacket220Id.DirtySampObjectsMakeObjectDirty, ArizonaPacket.ParseDirtySampObjectsMakeObjectDirty);
        Register220Incoming(registry, ArizonaPacket220Id.ResetFirstPersonState, ArizonaPacket.ParseResetFirstPersonState, "ResetFirstPersonState");
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleBrakeCalipersModel, ArizonaPacket.ParseSetVehicleBrakeCalipersModel);
        Register220Incoming(registry, ArizonaPacket220Id.ToggleHeadMove, ArizonaPacket.ParseToggleHeadMove);
        Register220Incoming(registry, ArizonaPacket220Id.SetVehicleBrakeCalipers, ArizonaPacket.ParseSetVehicleBrakeCalipers);

        #endregion

        #region multiplexed / aliased IDs (payload shape depends on module/runtime path)
        Register220Incoming(registry, ArizonaPacket220Id.LoadJs, ArizonaPacket.ParseLoadJs, "LoadJs");
        Register220Incoming(registry, ArizonaPacket220Id.SimpleCreate, ArizonaPacket.ParseSimpleCreate, "SimpleCreate");
        Register220Incoming(registry, ArizonaPacket220Id.CreateScaled, ArizonaPacket.ParseCreateScaled, "CreateScaled");
        Register220Incoming(registry, ArizonaPacket220Id.ObjectCreate, ArizonaPacket.ParseObjectCreate, "ObjectCreate");
        Register220Incoming(registry, ArizonaPacket220Id.InsideObjectCreate, ArizonaPacket.ParseInsideObjectCreate, "InsideObjectCreate");
        Register220Incoming(registry, ArizonaPacket220Id.ModuleReadRequest, ArizonaPacket.ParseModuleReadRequest, "ModuleReadRequest");
        Register220Incoming(registry, ArizonaPacket220Id.Waypoint3DSetRadius, ArizonaPacket.ParseSetVisibleDistance3DMarker, "Waypoint3DSetRadius");
        Register220Incoming(registry, ArizonaPacket220Id.SetVisibleDistance3DMarker, ArizonaPacket.ParseSetVisibleDistance3DMarker, "SetVisibleDistance3DMarker");
        Register220Incoming(registry, ArizonaPacket220Id.UiConfig, ArizonaPacket.ParseUiConfig, "UiConfig");
        Register220Incoming(registry, ArizonaPacket220Id.ScaleRadarMapIcons, ArizonaPacket.ParseScaleRadarMapIconsRaw, "ScaleRadarMapIcons");
        Register220Incoming(registry, ArizonaPacket220Id.GangZonePoly, ArizonaPacket.ParseGangZonePolyRaw, "GangZonePoly");
        #endregion
    }

    private static void RegisterArizona221(PacketParserRegistry registry)
    {
        #region outgoing (client -> server)
        Register221Outgoing(registry, ArizonaPacket221Id.BotSendOnfootSync, ArizonaPacket.ParseSendBotOnfootSync);
        Register221Outgoing(registry, ArizonaPacket221Id.BotSendDamage, ArizonaPacket.ParseSendBotDamage);

        #endregion

        #region incoming (server -> client)
        Register221Incoming(registry, ArizonaPacket221Id.BotWorldPedAdd, ArizonaPacket.ParseBotStreamIn);
        Register221Incoming(registry, ArizonaPacket221Id.BotWorldPedRemove, ArizonaPacket.ParseBotStreamOut);
        Register221Incoming(registry, ArizonaPacket221Id.BotOnfootPedSync, ArizonaPacket.ParseBotOnfootSync);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedColor, ArizonaPacket.ParseSetBotColor);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedFightStyle, ArizonaPacket.ParseSetBotFightStyle);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedInvulnerable, ArizonaPacket.ParseSetBotInvulnerable);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedName, ArizonaPacket.ParseSetBotName);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedSkin, ArizonaPacket.ParseSetBotSkin);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedWeapon, ArizonaPacket.ParseSetBotWeapon);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedPos, ArizonaPacket.ParseSetBotPos);
        Register221Incoming(registry, ArizonaPacket221Id.BotMovePedToPos, ArizonaPacket.ParseMoveBotToPos);
        Register221Incoming(registry, ArizonaPacket221Id.BotShootPedAtPos, ArizonaPacket.ParseShootBotAtPos);
        Register221Incoming(registry, ArizonaPacket221Id.BotApplyPedAnimation, ArizonaPacket.ParseApplyBotAnimation);
        Register221Incoming(registry, ArizonaPacket221Id.BotClearPedAction, ArizonaPacket.ParseClearBotAction);
        Register221Incoming(registry, ArizonaPacket221Id.BotShootPedAtPlayer, ArizonaPacket.ParseShootBotAtPlayer);
        Register221Incoming(registry, ArizonaPacket221Id.BotAttackPlayer, ArizonaPacket.ParseBotAttackPlayer);
        Register221Incoming(registry, ArizonaPacket221Id.BotEnterToVehicle, ArizonaPacket.ParseBotEnterVehicle);
        Register221Incoming(registry, ArizonaPacket221Id.BotPassengerPedSync, ArizonaPacket.ParseBotPassengerSync);
        Register221Incoming(registry, ArizonaPacket221Id.BotDrivePedSync, ArizonaPacket.ParseBotDriveSync);
        Register221Incoming(registry, ArizonaPacket221Id.BotRemoveFromVehicle, ArizonaPacket.ParseBotExitVehicle);
        Register221Incoming(registry, ArizonaPacket221Id.BotChatBubble, ArizonaPacket.ParseBotChatBubble);
        Register221Incoming(registry, ArizonaPacket221Id.BotAttachObject, ArizonaPacket.ParseSetBotAttachedObject);
        Register221Incoming(registry, ArizonaPacket221Id.BotDetachObject, ArizonaPacket.ParseRemoveBotAttachedObject);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetPedAngle, ArizonaPacket.ParseSetBotAngle);
        Register221Incoming(registry, ArizonaPacket221Id.BotStopAllAction, ArizonaPacket.ParseStopBotAction);
        Register221Incoming(registry, ArizonaPacket221Id.BotShootPedAtPed, ArizonaPacket.ParseShootBotAtBot);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetAnimationGroup, ArizonaPacket.ParseSetBotAnimationGroup);
        Register221Incoming(registry, ArizonaPacket221Id.BotAttackPed, ArizonaPacket.ParseBotAttackPed);
        Register221Incoming(registry, ArizonaPacket221Id.BotToggleCollision, ArizonaPacket.ParseTogglePedCollision);
        Register221Incoming(registry, ArizonaPacket221Id.BotAttachSimpleObject, ArizonaPacket.ParseSetBotAttachedSimpleObject);
        Register221Incoming(registry, ArizonaPacket221Id.BotDetachSimpleObject, ArizonaPacket.ParseRemoveBotAttachedSimpleObject);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetHealth, ArizonaPacket.ParseSetBotHealth);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetArmour, ArizonaPacket.ParseSetBotArmour);
        Register221Incoming(registry, ArizonaPacket221Id.BotSetOnfootSyncRate, ArizonaPacket.ParseSetBotOnfootSyncRate);

        #endregion
    }

}



