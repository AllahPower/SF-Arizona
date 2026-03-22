namespace SFSharp;

internal delegate TPayload ArizonaReaderParser<TPayload>(ref BitStreamReader reader);

public static class PacketParserCatalog
{
    public static PacketParserRegistry CreateDefaultRegistry()
    {
        PacketParserRegistry registry = new();
        RegisterSync(registry);
        RegisterArizona220(registry);
        RegisterArizona221(registry);
        return registry;
    }

    private static void RegisterSync(PacketParserRegistry registry)
    {
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionRequestAcceptedPacket>(EPacketId.ConnectionRequestAccepted, ParseIncomingConnectionRequestAccepted, exactBitLength: 104));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionAttemptFailedPacket>(EPacketId.ConnectionAttemptFailed, ParseIncomingConnectionAttemptFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingNoFreeIncomingConnectionsPacket>(EPacketId.NoFreeIncomingConnections, ParseIncomingNoFreeIncomingConnections, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingDisconnectionNotificationPacket>(EPacketId.DisconnectionNotification, ParseIncomingDisconnectionNotification, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingAuthenticationPacket>(EPacketId.Authentication, ParseIncomingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAuthenticationPacket>(EPacketId.Authentication, ParseOutgoingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingRconCommandPacket>(EPacketId.RconCommand, ParseOutgoingRconCommand, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingRconResponsePacket>(EPacketId.RconResponse, ParseIncomingRconResponse, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionLostPacket>(EPacketId.ConnectionLost, ParseIncomingConnectionLost, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionBannedPacket>(EPacketId.ConnectionBanned, ParseIncomingConnectionBanned, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingInvalidPasswordPacket>(EPacketId.InvalidPassword, ParseIncomingInvalidPassword, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionFailedPacket>(EPacketId.ConnectionFailed, ParseIncomingConnectionFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingOnfootPacket>(EPacketId.OnfootData, ParseIncomingOnfoot, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingOnfootPacket>(EPacketId.OnfootData, ParseOutgoingOnfoot, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingIncarPacket>(EPacketId.IncarData, ParseIncomingIncar, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingIncarPacket>(EPacketId.IncarData, ParseOutgoingIncar, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingAimPacket>(EPacketId.AimData, ParseIncomingAim, minimumBitLength: 24, exactBitLength: 272));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAimPacket>(EPacketId.AimData, ParseOutgoingAim, minimumBitLength: 8, exactBitLength: 256));
        registry.Register(new DelegateIncomingPacketParser<IncomingBulletPacket>(EPacketId.BulletData, ParseIncomingBullet, minimumBitLength: 24, exactBitLength: 344));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingBulletPacket>(EPacketId.BulletData, ParseOutgoingBullet, minimumBitLength: 8, exactBitLength: 328));
        registry.Register(new DelegateIncomingPacketParser<IncomingPassengerPacket>(EPacketId.PassengerData, ParseIncomingPassenger, minimumBitLength: 24, exactBitLength: 216));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingPassengerPacket>(EPacketId.PassengerData, ParseOutgoingPassenger, minimumBitLength: 8, exactBitLength: 200));
        registry.Register(new DelegateIncomingPacketParser<IncomingUnoccupiedPacket>(EPacketId.UnoccupiedData, ParseIncomingUnoccupied, minimumBitLength: 24, exactBitLength: 560));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingUnoccupiedPacket>(EPacketId.UnoccupiedData, ParseOutgoingUnoccupied, minimumBitLength: 8, exactBitLength: 544));
        registry.Register(new DelegateIncomingPacketParser<IncomingTrailerPacket>(EPacketId.TrailerData, ParseIncomingTrailer, minimumBitLength: 24, exactBitLength: 456));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingTrailerPacket>(EPacketId.TrailerData, ParseOutgoingTrailer, minimumBitLength: 8, exactBitLength: 440));
        registry.Register(new DelegateIncomingPacketParser<IncomingSpectatorPacket>(EPacketId.SpectatorData, ParseIncomingSpectator, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingSpectatorPacket>(EPacketId.SpectatorData, ParseOutgoingSpectator, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingWeaponsPacket>(EPacketId.WeaponsData, ParseIncomingWeapons, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingWeaponsPacket>(EPacketId.WeaponsData, ParseOutgoingWeapons, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingStatsPacket>(EPacketId.StatsData, ParseIncomingStats, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingStatsPacket>(EPacketId.StatsData, ParseOutgoingStats, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingMarkersPacket>(EPacketId.MarkersData, ParseIncomingMarkers, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingMarkersPacket>(EPacketId.MarkersData, ParseOutgoingMarkers, minimumBitLength: 8));
    }

    private static void RegisterArizona220(PacketParserRegistry registry)
    {
        Register220Incoming(registry, EArizonaPacketId.SetLocalDriver, ArizonaPacket.ParseSetLocalDriver);
        Register220Incoming(registry, EArizonaPacketId.TurnLightUpdate, ArizonaPacket.ParseTurnLightUpdate);
        Register220Incoming(registry, EArizonaPacketId.SetSatiety, ArizonaPacket.ParseSetSatiety);
        Register220Incoming(registry, EArizonaPacketId.SetHudMode, ArizonaPacket.ParseSetHudMode);
        Register220Incoming(registry, EArizonaPacketId.SetRadarMode, ArizonaPacket.ParseSetRadarMode);
        Register220Incoming(registry, EArizonaPacketId.LoadJs, ArizonaPacket.ParseLoadJs);
        Register220Incoming(registry, EArizonaPacketId.PlayMediaOnBillboard, ArizonaPacket.ParsePlayMediaOnBillboard);
        Register220Incoming(registry, EArizonaPacketId.LoadHtml, ArizonaPacket.ParseLoadHtml);
        Register220Incoming(registry, EArizonaPacketId.InjectCode, ArizonaPacket.ParseInjectCode);
        Register220Incoming(registry, EArizonaPacketId.ToggleCursor, ArizonaPacket.ParseToggleCursor);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerUnknownState, ArizonaPacket.ParseSetPlayerUnknownState);
        Register220Incoming(registry, EArizonaPacketId.UiColorScale, ArizonaPacket.ParseUiColorScale);
        Register220Incoming(registry, EArizonaPacketId.SetChatGroup, ArizonaPacket.ParseSetChatGroup);
        Register220Incoming(registry, EArizonaPacketId.SetLocalInVehicle, ArizonaPacket.ParseSetLocalInVehicle);
        Register220Incoming(registry, EArizonaPacketId.SetNicknameMode, ArizonaPacket.ParseSetNicknameMode);
        Register220Incoming(registry, EArizonaPacketId.SwitchChatMode, ArizonaPacket.ParseSwitchChatMode);
        Register220Incoming(registry, EArizonaPacketId.SetVisibleDistance3DMarker, ArizonaPacket.ParseSetVisibleDistance3DMarker);
        Register220Incoming(registry, EArizonaPacketId.ShowPositionInDiscord, ArizonaPacket.ParseShowPositionInDiscord);
        Register220Incoming(registry, EArizonaPacketId.Unknown86, ArizonaPacket.ParseUnknown86);
        Register220Incoming(registry, EArizonaPacketId.AutoDrinkBeer, ArizonaPacket.ParseAutoDrinkBeer);
        Register220Incoming(registry, EArizonaPacketId.SetDayNightColors, ArizonaPacket.ParseSetDayNightColors);
        Register220Incoming(registry, EArizonaPacketId.ToggleCompass, ArizonaPacket.ParseToggleCompass);
        Register220Incoming(registry, EArizonaPacketId.SetAnimationProperty, ArizonaPacket.ParseSetAnimationProperty);
        Register220Incoming(registry, EArizonaPacketId.ToggleMapColors, ArizonaPacket.ParseToggleMapColors);
        Register220Incoming(registry, EArizonaPacketId.ToggleUnknown102, ArizonaPacket.ParseToggleUnknown102);
        Register220Incoming(registry, EArizonaPacketId.ChangeServer, ArizonaPacket.ParseChangeServer);
        Register220Incoming(registry, EArizonaPacketId.ShowLoadScreenVc, ArizonaPacket.ParseShowLoadScreenVc);
        Register220Incoming(registry, EArizonaPacketId.ToggleUnknown105, ArizonaPacket.ParseToggleUnknown105);
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<ArzSwitchChatState>>(EPacketId.ArizonaCef, (int)EArizonaPacketId.SwitchChatState, args => ParseIncoming220(args, EArizonaPacketId.SwitchChatState, ArizonaPacket.ParseSwitchChatState), name: $"Arizona220:{EArizonaPacketId.SwitchChatState}", minimumPayloadBitLength: 33));
        Register220Incoming(registry, EArizonaPacketId.UiConfig, ArizonaPacket.ParseUiConfig);
        Register220Incoming(registry, EArizonaPacketId.SetSpectatorPatches, ArizonaPacket.ParseSetSpectatorPatches);
        Register220Incoming(registry, EArizonaPacketId.ToggleUnknown114, ArizonaPacket.ParseToggleUnknown114);
        Register220Incoming(registry, EArizonaPacketId.SetViceCityFlag, ArizonaPacket.ParseSetViceCityFlag);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerNametagFlags, ArizonaPacket.ParseSetPlayerNametagFlags);
        Register220Incoming(registry, EArizonaPacketId.SetMapIcon, ArizonaPacket.ParseSetMapIcon);
        Register220Incoming(registry, EArizonaPacketId.UiScalar, ArizonaPacket.ParseUiScalar);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleColorSmoke, ArizonaPacket.ParseSetVehicleColorSmoke);
        Register220Incoming(registry, EArizonaPacketId.VehicleColor, ArizonaPacket.ParseVehicleColor);
        Register220Incoming(registry, EArizonaPacketId.SetSkyboxImages, ArizonaPacket.ParseSetSkyboxImages);
        Register220Incoming(registry, EArizonaPacketId.Create3DWaypoint, ArizonaPacket.ParseCreate3DWaypoint);
        Register220Incoming(registry, EArizonaPacketId.SetHudStyle, ArizonaPacket.ParseSetHudStyle);
        Register220Incoming(registry, EArizonaPacketId.TestDrive, ArizonaPacket.ParseTestDrive);
        Register220Incoming(registry, EArizonaPacketId.ToggleRenderTarget, ArizonaPacket.ParseToggleRenderTarget);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureFlag1, ArizonaPacket.ParseVehicleFeatureFlag1);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureFlag0, ArizonaPacket.ParseVehicleFeatureFlag0);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureFlag2, ArizonaPacket.ParseVehicleFeatureFlag2);
        Register220Incoming(registry, EArizonaPacketId.VehicleFeatureReset, ArizonaPacket.ParseVehicleFeatureReset);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleNumberPlate, ArizonaPacket.ParseSetVehicleNumberPlate);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerAttachedObject, ArizonaPacket.ParseSetPlayerAttachedObject);
        Register220Incoming(registry, EArizonaPacketId.LoadBinary, ArizonaPacket.ParseLoadBinary);
        Register220Incoming(registry, EArizonaPacketId.TogglePortal, ArizonaPacket.ParseTogglePortal);
        Register220Incoming(registry, EArizonaPacketId.CreatePortal, ArizonaPacket.ParseCreatePortal);
        Register220Incoming(registry, EArizonaPacketId.DestroyPortal, ArizonaPacket.ParseDestroyPortal);
        Register220Incoming(registry, EArizonaPacketId.ToggleUnknown163, ArizonaPacket.ParseToggleUnknown163);
        Register220Incoming(registry, EArizonaPacketId.ToggleUnknown164, ArizonaPacket.ParseToggleUnknown164);
        Register220Incoming(registry, EArizonaPacketId.SetCurrentTask, ArizonaPacket.ParseSetCurrentTask);
        Register220Incoming(registry, EArizonaPacketId.ToggleDrawInterface, ArizonaPacket.ParseToggleDrawInterface);
        Register220Incoming(registry, EArizonaPacketId.SetInterior, ArizonaPacket.ParseSetInterior);
        Register220Incoming(registry, EArizonaPacketId.UiToggle, ArizonaPacket.ParseUiToggle);
        Register220Incoming(registry, EArizonaPacketId.VehicleHeadlightsState, ArizonaPacket.ParseVehicleHeadlightsState);
        Register220Incoming(registry, EArizonaPacketId.SetVirtualWorld, ArizonaPacket.ParseSetVirtualWorld);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleDriftMode, ArizonaPacket.ParseSetVehicleDriftMode);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleLights, ArizonaPacket.ParseSetVehicleLights);
        Register220Incoming(registry, EArizonaPacketId.SetPlayerSkin, ArizonaPacket.ParseSetPlayerSkin);
        Register220Incoming(registry, EArizonaPacketId.SetVehicleStrobelights, ArizonaPacket.ParseSetVehicleStrobelights);
        Register220Incoming(registry, EArizonaPacketId.SetGpsRoute, ArizonaPacket.ParseSetGpsRoute);
        Register220Incoming(registry, EArizonaPacketId.SetFirstPersonCamera, ArizonaPacket.ParseSetFirstPersonCamera);

        Register220Incoming(registry, EArizonaPacketId.Unknown11, ArizonaPacket.ParseCustomUnknown11, "Unknown11");
        Register220Incoming(registry, EArizonaPacketId.Unknown13, ArizonaPacket.ParseCustomUnknown13, "Unknown13");
        Register220Incoming(registry, EArizonaPacketId.Close, ArizonaPacket.ParseCustomClose, "Close");
        Register220Incoming(registry, EArizonaPacketId.Move, ArizonaPacket.ParseCustomMove, "Move");
        Register220Incoming(registry, EArizonaPacketId.ToggleScreen, ArizonaPacket.ParseCustomToggleScreen, "ToggleScreen");
        Register220Incoming(registry, EArizonaPacketId.ModuleReadRequest, ArizonaPacket.ParseCustomModuleReadRequest, "ModuleReadRequest");
        Register220Incoming(registry, EArizonaPacketId.ToggleShow, ArizonaPacket.ParseCustomToggleShow, "ToggleShow");
        Register220Incoming(registry, EArizonaPacketId.GetBrowserControlState, ArizonaPacket.ParseCustomGetBrowserControlState, "GetBrowserControlState");
        Register220Incoming(registry, EArizonaPacketId.Resize, ArizonaPacket.ParseCustomResize, "Resize");
        Register220Incoming(registry, EArizonaPacketId.AddObject, ArizonaPacket.ParseCustomAddObject, "AddObject");
        Register220Incoming(registry, EArizonaPacketId.RemoveObject, ArizonaPacket.ParseCustomRemoveObject, "RemoveObject");
        Register220Incoming(registry, EArizonaPacketId.BlipIcon, ArizonaPacket.ParseCustomBlipIconRaw, "BlipIcon");
        Register220Incoming(registry, EArizonaPacketId.MarkerIconBatch, ArizonaPacket.ParseCustomMarkerIconBatchRaw, "MarkerIconBatch");

        Register220Outgoing(registry, EArizonaPacketId.SendKey, ArizonaPacket.ParseSendKey);
        Register220Outgoing(registry, EArizonaPacketId.SendSwitchChatState, ArizonaPacket.ParseSendSwitchChatState);
        Register220Outgoing(registry, EArizonaPacketId.SendTurnLights, ArizonaPacket.ParseSendTurnLights);
        Register220Outgoing(registry, EArizonaPacketId.SendOpenInterface, ArizonaPacket.ParseSendOpenInterface);
        Register220Outgoing(registry, EArizonaPacketId.Send, ArizonaPacket.ParseSendText);
        Register220Outgoing(registry, EArizonaPacketId.SendResolution, ArizonaPacket.ParseSendResolution);
        Register220Outgoing(registry, EArizonaPacketId.SendToggleDrawInterface, ArizonaPacket.ParseSendToggleDrawInterface);
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

        Register220Outgoing(registry, EArizonaPacketId.ModuleReadRequest, ArizonaPacket.ParseCustomModuleReadRequest, "ModuleReadRequest");
        Register220Outgoing(registry, EArizonaPacketId.BrowserClick, ArizonaPacket.ParseCustomBrowserClick, "BrowserClick");
        Register220Outgoing(registry, EArizonaPacketId.SetBrowserControlState, ArizonaPacket.ParseCustomSetBrowserControlState, "SetBrowserControlState");
    }

    private static void RegisterArizona221(PacketParserRegistry registry)
    {
        Register221Incoming(registry, EArizonaPacketIdEx.BotStreamIn, ArizonaPacket.ParseBotStreamIn);
        Register221Incoming(registry, EArizonaPacketIdEx.BotStreamOut, ArizonaPacket.ParseBotStreamOut);
        Register221Incoming(registry, EArizonaPacketIdEx.BotOnfootSync, ArizonaPacket.ParseBotOnfootSync);
        Register221Incoming(registry, EArizonaPacketIdEx.SetBotInvulnerable, ArizonaPacket.ParseSetBotInvulnerable);
        Register221Incoming(registry, EArizonaPacketIdEx.SetBotName, ArizonaPacket.ParseSetBotName);
        Register221Incoming(registry, EArizonaPacketIdEx.SetBotWeapon, ArizonaPacket.ParseSetBotWeapon);
        Register221Incoming(registry, EArizonaPacketIdEx.SetBotPos, ArizonaPacket.ParseSetBotPos);
        Register221Incoming(registry, EArizonaPacketIdEx.MoveBotToPos, ArizonaPacket.ParseMoveBotToPos);
        Register221Incoming(registry, EArizonaPacketIdEx.ApplyBotAnimation, ArizonaPacket.ParseApplyBotAnimation);
        Register221Incoming(registry, EArizonaPacketIdEx.BotAttackPlayer, ArizonaPacket.ParseBotAttackPlayer);
        Register221Incoming(registry, EArizonaPacketIdEx.BotEnterVehicle, ArizonaPacket.ParseBotEnterVehicle);
        Register221Incoming(registry, EArizonaPacketIdEx.BotPassengerSync, ArizonaPacket.ParseBotPassengerSync);
        Register221Incoming(registry, EArizonaPacketIdEx.BotExitVehicle, ArizonaPacket.ParseBotExitVehicle);
        Register221Incoming(registry, EArizonaPacketIdEx.BotChatBubble, ArizonaPacket.ParseBotChatBubble);
        Register221Incoming(registry, EArizonaPacketIdEx.SetBotAttachedObject, ArizonaPacket.ParseSetBotAttachedObject);
        Register221Incoming(registry, EArizonaPacketIdEx.RemoveBotAttachedObject, ArizonaPacket.ParseRemoveBotAttachedObject);
        Register221Incoming(registry, EArizonaPacketIdEx.ShootBotAtBot, ArizonaPacket.ParseShootBotAtBot);
        Register221Incoming(registry, EArizonaPacketIdEx.DestroyBot, ArizonaPacket.ParseDestroyBot);
        Register221Incoming(registry, EArizonaPacketIdEx.SetBotAttachedSimpleObject, ArizonaPacket.ParseSetBotAttachedSimpleObject);

        Register221Outgoing(registry, EArizonaPacketIdEx.SendBotOnfootSync, ArizonaPacket.ParseSendBotOnfootSync);
        Register221Outgoing(registry, EArizonaPacketIdEx.SendBotDamage, ArizonaPacket.ParseSendBotDamage);
    }

    private static IncomingConnectionAttemptFailedPacket ParseIncomingConnectionAttemptFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionAttemptFailedPacket();
    }

    private static IncomingNoFreeIncomingConnectionsPacket ParseIncomingNoFreeIncomingConnections(IncomingPacketArgs args)
    {
        return new IncomingNoFreeIncomingConnectionsPacket();
    }

    private static IncomingDisconnectionNotificationPacket ParseIncomingDisconnectionNotification(IncomingPacketArgs args)
    {
        return new IncomingDisconnectionNotificationPacket();
    }
    private static IncomingConnectionRequestAcceptedPacket ParseIncomingConnectionRequestAccepted(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        int ip = reader.ReadInt32();
        ushort port = reader.ReadUInt16();
        ushort playerId = reader.ReadUInt16();
        int challenge = reader.ReadInt32();
        return new IncomingConnectionRequestAcceptedPacket(ip, port, playerId, challenge);
    }

    private static IncomingAuthenticationPacket ParseIncomingAuthentication(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new IncomingAuthenticationPacket(reader.ReadStringUInt8Length());
    }

    private static OutgoingAuthenticationPacket ParseOutgoingAuthentication(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingAuthenticationPacket(reader.ReadStringUInt8Length());
    }

    private static OutgoingRconCommandPacket ParseOutgoingRconCommand(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingRconCommandPacket(reader.ReadStringUInt32Length());
    }

    private static IncomingRconResponsePacket ParseIncomingRconResponse(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new IncomingRconResponsePacket(reader.ReadStringUInt32Length());
    }

    private static IncomingConnectionLostPacket ParseIncomingConnectionLost(IncomingPacketArgs args)
    {
        return new IncomingConnectionLostPacket();
    }

    private static IncomingConnectionBannedPacket ParseIncomingConnectionBanned(IncomingPacketArgs args)
    {
        return new IncomingConnectionBannedPacket();
    }

    private static IncomingInvalidPasswordPacket ParseIncomingInvalidPassword(IncomingPacketArgs args)
    {
        return new IncomingInvalidPasswordPacket();
    }

    private static IncomingConnectionFailedPacket ParseIncomingConnectionFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionFailedPacket();
    }
    private static IncomingOnfootPacket ParseIncomingOnfoot(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingOnfootPacket(playerId, OnfootSyncData.Parse(ref reader));
    }

    private static OutgoingOnfootPacket ParseOutgoingOnfoot(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingOnfootPacket(OutgoingOnfootSyncData.Parse(ref reader));
    }

    private static IncomingIncarPacket ParseIncomingIncar(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingIncarPacket(playerId, IncarSyncData.Parse(ref reader));
    }

    private static OutgoingIncarPacket ParseOutgoingIncar(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingIncarPacket(OutgoingIncarSyncData.Parse(ref reader));
    }

    private static IncomingAimPacket ParseIncomingAim(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingAimPacket(playerId, AimSyncData.Parse(ref reader));
    }

    private static OutgoingAimPacket ParseOutgoingAim(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingAimPacket(AimSyncData.Parse(ref reader));
    }

    private static IncomingBulletPacket ParseIncomingBullet(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingBulletPacket(playerId, BulletSyncData.Parse(ref reader));
    }

    private static OutgoingBulletPacket ParseOutgoingBullet(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingBulletPacket(BulletSyncData.Parse(ref reader));
    }

    private static IncomingPassengerPacket ParseIncomingPassenger(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingPassengerPacket(playerId, PassengerSyncData.Parse(ref reader));
    }

    private static OutgoingPassengerPacket ParseOutgoingPassenger(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingPassengerPacket(PassengerSyncData.Parse(ref reader));
    }

    private static IncomingUnoccupiedPacket ParseIncomingUnoccupied(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingUnoccupiedPacket(playerId, UnoccupiedSyncData.Parse(ref reader));
    }

    private static OutgoingUnoccupiedPacket ParseOutgoingUnoccupied(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingUnoccupiedPacket(UnoccupiedSyncData.Parse(ref reader));
    }

    private static IncomingTrailerPacket ParseIncomingTrailer(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingTrailerPacket(playerId, TrailerSyncData.Parse(ref reader));
    }

    private static OutgoingTrailerPacket ParseOutgoingTrailer(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingTrailerPacket(TrailerSyncData.Parse(ref reader));
    }

    private static IncomingSpectatorPacket ParseIncomingSpectator(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingSpectatorPacket(playerId, SpectatorSyncData.Parse(ref reader));
    }

    private static OutgoingSpectatorPacket ParseOutgoingSpectator(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingSpectatorPacket(SpectatorSyncData.Parse(ref reader));
    }

    private static IncomingWeaponsPacket ParseIncomingWeapons(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingWeaponsPacket(playerId, WeaponsSyncData.Parse(ref reader));
    }

    private static OutgoingWeaponsPacket ParseOutgoingWeapons(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingWeaponsPacket(WeaponsSyncData.Parse(ref reader));
    }

    private static IncomingStatsPacket ParseIncomingStats(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingStatsPacket(playerId, StatsSyncData.Parse(ref reader));
    }

    private static OutgoingStatsPacket ParseOutgoingStats(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingStatsPacket(StatsSyncData.Parse(ref reader));
    }

    private static IncomingMarkersPacket ParseIncomingMarkers(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        byte markerSource = reader.ReadUInt8();
        return new IncomingMarkersPacket(markerSource, MarkersSyncData.Parse(ref reader));
    }

    private static OutgoingMarkersPacket ParseOutgoingMarkers(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingMarkersPacket(MarkersSyncData.Parse(ref reader));
    }

    private static void Register220Incoming<TPayload>(PacketParserRegistry registry, EArizonaPacketId subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(EPacketId.ArizonaCef, (int)subId, args => ParseIncoming220(args, subId, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register220Outgoing<TPayload>(PacketParserRegistry registry, EArizonaPacketId subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingArizonaPacket<TPayload>>(EPacketId.ArizonaCef, (int)subId, args => ParseOutgoing220(args, subId, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register221Incoming<TPayload>(PacketParserRegistry registry, EArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(EPacketId.ArizonaCefEx, (int)subId, args => ParseIncoming221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static void Register221Outgoing<TPayload>(PacketParserRegistry registry, EArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingArizonaPacket<TPayload>>(EPacketId.ArizonaCefEx, (int)subId, args => ParseOutgoing221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncoming220<TPayload>(IncomingArizonaPacketArgs args, EArizonaPacketId subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(EPacketId.ArizonaCef, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static OutgoingArizonaPacket<TPayload> ParseOutgoing220<TPayload>(OutgoingArizonaPacketArgs args, EArizonaPacketId subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingArizonaPacket<TPayload>(EPacketId.ArizonaCef, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncoming221<TPayload>(IncomingArizonaPacketArgs args, EArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(EPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static OutgoingArizonaPacket<TPayload> ParseOutgoing221<TPayload>(OutgoingArizonaPacketArgs args, EArizonaPacketIdEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingArizonaPacket<TPayload>(EPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }
}
